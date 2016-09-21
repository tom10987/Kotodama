﻿
using UnityEngine;

//------------------------------------------------------------
// NOTICE:
// シングルタッチのみ対応
// エディター上で使用する場合、すべて左クリックの判定として振る舞う
//
// レイキャスト判定には、各種 Collider コンポーネントが必要な点に注意
//
// TIPS:
// 1. Camera.ScreenToViewportPoint
//  * スクリーン上の座標をビューポート上の座標に変換する
//  * タッチ判定などでタッチの座標を変換するときは、こちらを利用する
//
// 2. Camera.ViewportToScreenPoint
//  * ビューポート上の座標をスクリーン上の座標に変換する
//
//------------------------------------------------------------

static class TouchController {

  public static bool IsAndroid {
    get { return Application.platform == RuntimePlatform.Android; }
  }

  public static bool IsIPhone {
    get { return Application.platform == RuntimePlatform.IPhonePlayer; }
  }

  public static bool IsSmartDevice {
    get { return IsAndroid || IsIPhone; }
  }


  /** <summary> タッチされたスクリーン座標を返す（XY 平面：左下基準）
    * <para> スマートフォン等でなければ、代わりにマウスの座標を返す
    * </para> タッチされてなければ、(0, 0, 0) を返す </summary> */
  public static Vector3 GetScreenPosition() {
    return !IsSmartDevice ? Input.mousePosition :
      (Input.touchCount > 0) ? (Vector3)Input.touches[0].position : Vector3.zero;
  }

  /// <summary> 画面中央から見たスクリーン上のタッチ座標を返す（XY 平面） </summary>
  public static Vector3 GetScreenPositionFromCenter() {
    return GetScreenPosition() - ScreenExtension.center;
  }

  /// <summary> <see cref="GetScreenPositionFromCenter()"/> を XZ 平面に変換 </summary>
  public static Vector3 GetScreenPositionFromCenterXZ() {
    return SwitchYZ(GetScreenPositionFromCenter());
  }

  /// <summary> 画面中央から見たワールドの標準座標を返す（XY 平面） </summary>
  public static Vector3 GetScreenToWorldPosition() {
    var touch = GetScreenPositionFromCenter();

    // TIPS: タッチがスクリーン座標なので、ワールド座標に変換
    var distance = Vector3.zero;
    distance.x = (touch.x / ScreenExtension.center.x) * ScreenExtension.aspect.x;
    distance.y = (touch.y / ScreenExtension.center.y) * ScreenExtension.aspect.y;

    return distance;
  }

  /// <summary> <see cref="GetScreenToWorldPosition()"/> を XZ 平面に変換 </summary>
  public static Vector3 GetScreenToWorldPositionXZ() {
    return SwitchYZ(GetScreenToWorldPosition());
  }

  // TIPS: 基本的に z が 0 なので、一時的な変数を用意せずに入れ替えを行う
  static Vector3 SwitchYZ(Vector3 position) {
    position.z = position.y;
    position.y = 0f;
    return position;
  }


  /// <summary> スクリーンからの Raycast による、オブジェクトとの交差判定 </summary>
  public static bool IsRaycastHit(out RaycastHit hit) {
    var ray = Camera.main.ScreenPointToRay(GetScreenPosition());
    return Physics.Raycast(ray, out hit, Camera.main.farClipPlane);
  }

  /// <summary> 指定したタグと一致するオブジェクトと交差していれば true を返す </summary>
  public static bool IsRaycastHitWithTag(out RaycastHit hit, string tag) {
    var isHit = IsRaycastHit(out hit);
    return isHit && (hit.transform.tag == tag);
  }

  /// <summary> 指定したレイヤー上にあるオブジェクトとの交差判定を行う </summary>
  public static bool IsRaycastHitWithLayer(out RaycastHit hit, int layerMask) {
    var ray = Camera.main.ScreenPointToRay(GetScreenPosition());
    return Physics.Raycast(ray, out hit, Camera.main.farClipPlane, layerMask);
  }


  /// <summary> タッチされた瞬間 true を返す </summary>
  public static bool IsTouchBegan() {
    if (!IsSmartDevice) { return Input.GetMouseButtonDown(0); }
    if (Input.touchCount <= 0) { return false; }
    return Input.touches[0].phase == TouchPhase.Began;
  }

  /// <summary> タッチされ続けている間 true を返す </summary>
  public static bool IsTouchMoved() {
    if (!IsSmartDevice) { return Input.GetMouseButton(0); }
    if (Input.touchCount <= 0) { return false; }

    var touch = Input.touches[0];
    var isMoved = (touch.phase == TouchPhase.Moved);
    var isStationary = (touch.phase == TouchPhase.Stationary);

    return isMoved || isStationary;
  }

  /// <summary> タッチが離された瞬間 true を返す </summary>
  public static bool IsTouchEnded() {
    if (!IsSmartDevice) { return Input.GetMouseButtonUp(0); }
    if (Input.touchCount <= 0) { return false; }
    return Input.touches[0].phase == TouchPhase.Ended;
  }

  /// <summary> 端末の戻るボタンが押された時 true を返す </summary>
  public static bool IsPushedQuitKey() {
    return Input.GetKeyDown(KeyCode.Escape);
  }
}
