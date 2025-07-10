function ZErrorProvider_PositionControls(ValidationParentID)
{
	ValidationControls = document.getElementById(ValidationParentID);
	ZErrorProvider_RePositionControl();
	window.attachEvent("onresize", ZErrorProvider_RePositionControl);
}

var ValidationControls;

function ZErrorProvider_RePositionControl()
{
	if(ValidationControls != null)
	{
		for(var i = 0 ; i < ValidationControls.childNodes.length ; i++)
		{
			var	Control	= ValidationControls.childNodes[i];
			if(Control != null && Control.getAttribute("ControlToValidate")	!= '')
			{
				var targetID = Control.getAttribute("ControlToValidate");
				var ChildPath = new Array();
				var targetCtrl = document.getElementById(targetID);
				while (targetCtrl == null)
				{
					var index = targetID.lastIndexOf("_ctl");
					if (index == -1)
					{
					    break;
					}
					ChildPath.push(targetID.substr(index));
					targetID = targetID.slice(0, index);
					targetCtrl = document.getElementById(targetID);
				}
				if (targetCtrl != null)
				{
					while(ChildPath.length > 0)
					{
						var childIndex = ChildPath.pop().slice(5);
						var childCtrl = null;
						if (targetCtrl.tagName == "TABLE" && targetCtrl.rows.length > childIndex)
						{
							childCtrl = targetCtrl.rows[childIndex];
						}
						else if (targetCtrl.tagName == "TR" && targetCtrl.cells.length > childIndex)
						{
							childCtrl = targetCtrl.cells[childIndex];
						}
						else if (targetCtrl.childNodes.length > childIndex)
						{
							childCtrl = targetCtrl.childNodes[childIndex];
						}
						if (childCtrl != null)
						{
							targetCtrl = childCtrl;
						}
					}
					var incOffset = targetCtrl.tagName != "TABLE" && targetCtrl.tagName != "TR";
					ZErrorProvider_PositionControl(Control, targetCtrl, incOffset);
				}
			}
		}
	}
}

function ZErrorProvider_PositionControl(ErrorControl, Control, IncludeOffset)
{
	if(ErrorControl != null && Control != null)
	{
		var coords = ZErrorProvider_GetAbsolutePos(Control);
		if (IncludeOffset == true)
		{
			ErrorControl.style.left = coords.x + Control.offsetWidth;
		}
		else
		{
			ErrorControl.style.left = coords.x;
		}
		ErrorControl.style.top = coords.y;
		ErrorControl.style.display = "block";
	}
}

function ZErrorProvider_GetAbsolutePos(el)
{
	var SL = 0, ST = 0;
	var is_div = /^div$/i.test(el.tagName);
	if (is_div && el.scrollLeft) SL = el.scrollLeft;
	if (is_div && el.scrollTop) ST = el.scrollTop;
	var r = { x: el.offsetLeft - SL, y: el.offsetTop - ST };
	if (el.offsetParent)
	{					
		if (el.offsetParent.style.position == 'absolute') return r;
		var tmp = ZErrorProvider_GetAbsolutePos(el.offsetParent);
		r.x += tmp.x;
		r.y += tmp.y;
	}	
	return r;	
}

if (typeof(Sys) != "undefined"){
    Sys.Application.notifyScriptLoaded();
}