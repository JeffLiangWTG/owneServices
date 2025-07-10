var ZTextPopup_TextBox;
var ZTextPopup_Popup;
var ZTextPopup_GuidContainer;
var ZTextPopup_Button;
var ZTextPopup_IsPublishedTextBox;

function ZTextPopup_SetValueAndHidePopup(RetVal)
{		
	if (ZTextPopup_TextBox != null && ZTextPopup_TextBox.get('value') != RetVal)
	{
		ZTextPopup_TextBox.set('value', RetVal);
		if (ZTextPopup_TextBox.onchange != null) ZTextPopup_TextBox.onchange();
	}
	ZTextPopup_HidePopup();
}

function ZTextPopup_SetValueAndHidePopup(RetVal1, RetVal2)
{
    if (ZTextPopup_TextBox != null && ZTextPopup_TextBox.get('value') != RetVal1) {
        if (ZTextPopup_IsPublishedTextBox != null && ZTextPopup_IsPublishedTextBox.get('value') != RetVal2) {
            ZTextPopup_IsPublishedTextBox.set('value', RetVal2);
        }
        ZTextPopup_TextBox.set('value', RetVal1);
        if (ZTextPopup_TextBox.onchange != null) ZTextPopup_TextBox.onchange();
    }
    ZTextPopup_HidePopup();
}

function ZTextPopup_SetValueAndGuidThenHidePopup(RetVal, RetGuid)
{
	if (ZTextPopup_GuidContainer != null && RetGuid != null)
	{
		ZTextPopup_GuidContainer.set('value', RetGuid);
	}
	ZTextPopup_SetValueAndHidePopup(RetVal);
}

function ZTextPopup_HidePopupAndTriggerPostBack()
{
    ZTextPopup_HidePopup();
    __doPostBack('eDocsAddNewLink$TextBox','');
}

function ZTextPopup_HidePopup()
{
    ZTextPopup_DetachBlurEvent();
    
	if (ZTextPopup_Popup != null)
	{
	    ZTextPopup_Popup.setStyle('display', 'none');
        try {
            var autoCompleteTextBox = $(ZTextPopup_TextBox.getAttribute('AutoCompleteTextBox'));
            if (autoCompleteTextBox != null) {
                autoCompleteTextBox.focus();
                autoCompleteTextBox.select();
            }
            else {
                ZTextPopup_TextBox.focus();
                ZTextPopup_TextBox.select();
            }
        }
        catch (ex) { }
    }
	
	ZTextPopup_TextBox = null;
	ZTextPopup_Popup = null;
	ZTextPopup_GuidContainer = null;
}

function ZTextPopup_SetIsPublishedTextBoxID(IsPublishedTextBoxID) {
    ZTextPopup_IsPublishedTextBox = $(IsPublishedTextBoxID);
}

function ZTextPopup_ShowPopup(TextBoxID, PopupID)
{
    ZTextPopup_ShowPopup(TextBoxID, null, PopupID);
}

function ZTextPopup_ShowPopup(TextBoxID, ButtonID, PopupID) {
	ZTextPopup_TextBox = $(TextBoxID);
	ZTextPopup_Button = $(ButtonID);
	ZTextPopup_Popup = $(PopupID);

	if (ZTextPopup_TextBox == null) return;
	if (ZTextPopup_Popup == null) return;

	ZTextPopup_Popup.setStyle('display', 'inline');

	var mainControlDimensions;

	if ((ZTextPopup_TextBox.getStyle('visibility') == 'hidden' || ZTextPopup_TextBox.getStyle('display') == 'none')
	    && ZTextPopup_Button != null) {
		var autoCompleteTextBox = $(ZTextPopup_TextBox.getAttribute('AutoCompleteTextBox'));
		if (autoCompleteTextBox != null) {
			mainControlDimensions = autoCompleteTextBox.getCoordinates();
		}
		else {
			mainControlDimensions = ZTextPopup_Button.getCoordinates();
		}
	}
	else {
		mainControlDimensions = ZTextPopup_TextBox.getCoordinates();
	}

	var popupLeft = mainControlDimensions.left;
	var popupAboveTop = mainControlDimensions.top;
	var popupBelowTop = mainControlDimensions.top + mainControlDimensions.height;
	var popupRight = mainControlDimensions.right;
	var popupBottom = mainControlDimensions.bottom;

	var popupHeight = parseInt(ZTextPopup_Popup.getStyle('height'));
	var popupWidth = parseInt(ZTextPopup_Popup.getStyle('width'));

	//DebugConsole_NewMessage('Popup: left: ' + popupLeft + ', width: ' + popupWidth);
	//DebugConsole_NewMessage('Screen: left: ' + posLeft() + ', right: ' + posRight());

	var windowTop = posTop();
	var windowBottom = posBottom();
	var windowHeight = windowBottom - windowTop;

	var windowLeft = posLeft();
	var windowRight = posRight();
	var windowWidth = windowRight - windowLeft;

	var isPopupCenteredInWindow = false;
	if (ZTextPopup_ShowAbove(popupBelowTop, popupAboveTop, popupHeight)) {
		ZTextPopup_Popup.setStyle('top', popupAboveTop - popupHeight);
	}
	else {
		if (popupBelowTop + popupHeight >= windowBottom) {
			// it doesn't fit on the bottom either so center on page
			isPopupCenteredInWindow = true;
			var proposedTopPosition = windowTop + ((windowHeight / 2) - (popupHeight / 2));
			ZTextPopup_Popup.setStyle('top', proposedTopPosition > windowTop ? proposedTopPosition : windowTop);
			var proposedLeftPosition = (windowWidth / 2) - (popupWidth / 2);
			ZTextPopup_Popup.setStyle('left', proposedLeftPosition > 0 ? proposedLeftPosition : 0);
		}
		else {
			ZTextPopup_Popup.setStyle('top', popupBelowTop);
		}
	}

	if (!isPopupCenteredInWindow) {
		if (popupLeft + popupWidth > windowRight) {
			popupLeft = windowRight - popupWidth;
		}

		if (popupLeft < windowLeft) {
			popupLeft = windowLeft;
		}

		ZTextPopup_Popup.setStyle('left', popupLeft);
	}

	ZTextPopup_AttachBlurEvent();
}

function ZTextPopup_ShowAbove(popupBelowTop, popupAboveTop, popupHeight)
{
    try
    {
        var windowTop = posTop();
        var windowBottom = posBottom();
        if (popupBelowTop + popupHeight > windowBottom)
        {
            if (popupAboveTop - popupHeight >= windowTop)
            {
                return true;
            }
        }
        
    } catch(err) { }
    return false;
}

/**
 *   The commented out code creates an AJAX request. When the popup button is clicked
 *   the request starts. The target page (ie what will show up in the iFrame) is loaded
 *   in the background.
 *   onRequest: function that gets called as soon as the request starts 
 *              (set iFrame source to loading page)
 *   onComplete: function that gets called once the request completes
 *              (set iFrame source to target page)
 *
 *
 *   The following HTML makes a nice loading page, when coupled with an AJAXY loading image:
 *
 *  <html style="margin:0; padding:0; height:100%; border:none" >
 *      <head>
 *          <title>Loading</title>
 *          <link type="text/css" rel="stylesheet" href="BaseStyle.css" />
 *      </head>
 *      <body style="margin: 0; padding: 0; height: 100%">
 *          <table style="height: 100%; width: 100%; border: solid 1px gray;">
 *              <tr>
 *                  <td align="center">
 *                      <img src="Images/loading.gif" />
 *                      <br /><br />
 *                      Loading
 *                  </td>
 *              </tr>
 *          </table>
 *      </body>
 *  </html>
 **/

function ZTextPopup_ShowIFramePopup(TextBoxID, ButtonID, PopupID, IFrameSource)
{
    ZTextPopup_ShowIFramePopup(TextBoxID, ButtonID, PopupID, IFrameSource, false)
}

function ZTextPopup_ShowIFramePopup(TextBoxID, ButtonID, PopupID, IFrameSource, AlwaysReload)
{
    ZTextPopup_IFrame = $(PopupID);
    
    ZTextPopup_ShowPopup(TextBoxID, ButtonID, PopupID);
    
    if (ZTextPopup_IFrame != null)
    {
         var oldSrc = ZTextPopup_IFrame.getAttribute('src');
         if (IFrameSource != oldSrc || AlwaysReload)
            ZTextPopup_IFrame.setAttribute('src', IFrameSource);
    }
}

function ZTextPopup_ShowGuidPopup(TextBoxID, ButtonID, PopupID, GuidContainerID, IFrameSource)
{
	ZTextPopup_GuidContainer = $(GuidContainerID);
	ZTextPopup_ShowIFramePopup(TextBoxID, ButtonID, PopupID, IFrameSource);
}

function ZTextPopup_AttachBlurEvent() {
    document.addEvent('mousedown', ZTextPopup_HandleMouseKeyDown);
    document.addEvent('keydown', ZTextPopup_HandleMouseKeyDown);
}

function ZTextPopup_DetachBlurEvent() {
    document.removeEvent('mousedown', ZTextPopup_HandleMouseKeyDown);
    document.removeEvent('keydown', ZTextPopup_HandleMouseKeyDown);
}

var ZTextPopup_HandleMouseKeyDown = function() { ZTextPopup_HidePopup(); }