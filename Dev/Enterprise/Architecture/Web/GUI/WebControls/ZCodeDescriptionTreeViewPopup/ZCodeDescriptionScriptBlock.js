var ZCodeDescriptionTreeViewPopup_DescriptionBox;

function ZCodeDescriptionTreeViewPopup_ProcessSelection(Code, Description)
{
	if(Code != "")
	{
		ZTextPopup_SetValueAndHidePopup(Code);
		if(ZCodeDescriptionTreeViewPopup_DescriptionBox != null)
		{
			ZCodeDescriptionTreeViewPopup_DescriptionBox.value = Description;
		}
	}
	else
	{
		ZTextPopup_HidePopup();
	}
}

function ZCodeDescriptionTreeViewPopup_ShowPopup(DescriptionBoxID)
{
	ZCodeDescriptionTreeViewPopup_DescriptionBox = document.getElementById(DescriptionBoxID);
}

function StringTrim(str)
{
	if(str == null || str.length < 1)
	{
		return "";
	}
	else
	{
		var regex = /^\s*(\S+.*)\s*$/g;
		return str.replace(regex, "$1");
	}
}