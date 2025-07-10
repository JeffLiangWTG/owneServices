
function ZExpandCollapseButton_ExpandCollapse(ContentControlID, ExpandCollapseButtonID, ExpandIconFilename, CollapseIconFilename, StateHiddenInputID)
{
    var ContentControl = document.getElementById(ContentControlID);
    var ExpandCollapseButton = document.getElementById(ExpandCollapseButtonID);

    if (ContentControl != null && ExpandCollapseButton != null)
	{
		if (ContentControl.style.display == 'none')
		{
		    ContentControl.style.display = '';
		    ExpandCollapseButton.src = CollapseIconFilename;
		    ZExpandCollapseButton_UpdateState(StateHiddenInputID, 'Y');
		}
		else
		{
		    ContentControl.style.display = 'none';
		    ExpandCollapseButton.src = ExpandIconFilename;
		    ZExpandCollapseButton_UpdateState(StateHiddenInputID, 'N');
		}
	}
}

function ZExpandCollapseButton_UpdateState(StateHiddenInputID, Value)
{
    var Input = document.getElementById(StateHiddenInputID);
    if (Input != null)
	{
	    Input.value = Value;
	}	
}

if (typeof(Sys) != "undefined"){
    Sys.Application.notifyScriptLoaded();
}