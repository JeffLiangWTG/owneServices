
function ZDropEditList_ShowPopup(ControlID, RelatedControlIDParam)
{
	var Popup = $(ControlID);
	if(Popup != null && Popup.nodeName == "DIV")
	{
	    var TextBox = Popup.getParent().getFirst();
	    var mainControlDimensions = TextBox.getCoordinates();
	    var popupLeft = mainControlDimensions.left;
	    var popupTop = mainControlDimensions.top + mainControlDimensions.height;

	    Popup.setStyle('top', popupTop);
	    Popup.setStyle('left', popupLeft);
		Popup.setStyle('visibility', 'visible');
		Popup.getFirst().focus();
		CurrentPopup = Popup;
	}
	RelatedControlID = RelatedControlIDParam;
}

var CurrentPopup;
var RelatedControlID;

function ZDropEditList_HidePopup(PopupID)
{
	var Popup = $(PopupID);
	if(Popup != null)
	{
		CurrentPopup = Popup;
		ZDropEditList_ClosePopup();
	}
}

function ZDropEditList_ClosePopup()
{
	var Popup = CurrentPopup;

	if(Popup != null)
	{
		if(Popup != null && Popup.nodeName == "DIV")
		{
			Popup.setStyle('visibility', 'hidden');
			CurrentPopup = '';
		}
	}
}

function ZDropEditList_KeyPress(TextControlID)
{
	if (window.event.keyCode == 13)
    {
        ZDropEditList_SelectItem(TextControlID);
        return false;
    }
    else if (window.event.keyCode == 27)
    {
		ZDropEditList_ClosePopup();
		return false;
    }
	return true;
}

function ZDropEditList_SelectItem(TextControlID)
{
	var TextControl = $(TextControlID);
	if (TextControl != null && TextControl.nodeName == "INPUT")
	{
		if(document.activeElement.nodeName == "SELECT")
		{
			if(TextControl.value != document.activeElement.value)
			{
				TextControl.value = document.activeElement.value;
				if (TextControl.onchange != null) TextControl.onchange();
				if (typeof(ZDropEditList_SelectItem_AdditionalHandler) != "undefined") {
				    if (ZDropEditList_SelectItem_AdditionalHandler != null) {
				        ZDropEditList_SelectItem_AdditionalHandler(RelatedControlID);
				    }
				}
			}
		}
	}
	ZDropEditList_ClosePopup();
}

if (typeof(Sys) != "undefined"){
    Sys.Application.notifyScriptLoaded();
}
