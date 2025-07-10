function ZFilterPage_GridResizeOld(GridID, LowerControlID)
{
	var Grid = document.getElementById(GridID);
	var LowerControl = document.getElementById(LowerControlID);
	
	if ((Grid != null) && (LowerControl != null))
	{
		if (LowerControl.offsetParent  != null)
		{
			var LowerOffset = LowerControl.offsetParent.offsetTop;
			var Height = LowerOffset + LowerControl.offsetTop - Grid.offsetTop;
			if (Height > 0)
			{
				Grid.style.height = Height;
				Grid.style.width = LowerControl.offsetParent.offsetLeft - (Grid.offsetLeft * 2);
			}
		}
	}
}

function ZFilterPage_GridResize(GridID, LowerControlID)
{
	var grid = $(GridID);
	var lowerControl = $(LowerControlID);
	
	if ((grid != null) && (lowerControl != null))
	{
	    var parent = lowerControl.getParent();
	    
	    var lowerControlDimensions = lowerControl.getCoordinates();
	    var gridDimensions = grid.getCoordinates();
	    var parentDimensions = parent.getCoordinates();
	    
	    var newGridHeight = lowerControlDimensions.top - gridDimensions.top;
	    
	    if (newGridHeight > 0)
	    {
	        grid.setStyle('height', newGridHeight);
	    }
	}
}
