
function ZCollapsablePanel_ExpandCollapse(PanelID, ExpandCollapseImageID, ExpandIconFilename, CollapseIconFilename, HiddenTextBoxID)
{
	var Panel = document.getElementById(PanelID);
	var ExpandCollapseImage = document.getElementById(ExpandCollapseImageID);
	
	if (Panel != null && ExpandCollapseImage != null)
	{
		if (Panel.style.display == 'none')
		{
			Panel.style.display = 'inline';
			ExpandCollapseImage.src = CollapseIconFilename;
			ZCollapsablePanel_UpdateTextBox(HiddenTextBoxID, 'Y');
		}
		else
		{
			Panel.style.display = 'none';
			ExpandCollapseImage.src = ExpandIconFilename;
			ZCollapsablePanel_UpdateTextBox(HiddenTextBoxID, 'N');
		}
	}
}

function ZCollapsablePanel_UpdateTextBox(HiddenTextBoxID, Value)
{
	var TextBox = document.getElementById(HiddenTextBoxID);
	if (TextBox != null)
	{
		TextBox.value = Value;
	}	
}