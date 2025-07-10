//SuckerTree Horizontal Menu (Sept 14th, 06)
//By Dynamic Drive: http://www.dynamicdrive.com/style/

var menuids=["menu"]

function buildsubmenus_horizontal()
{
	for (var i=0; i<menuids.length; i++)
	{
		// Get the menu item
		var menucontrol = document.getElementById(menuids[i]);
		if (menucontrol != null)
		{
			// Get a list of all the submenus (uls) beneath this menu
			var ultags=menucontrol.getElementsByTagName("ul");
			
			for (var t=0; t<ultags.length; t++)
			{
				if (ultags[t].parentNode.parentNode.id==menuids[i])  //if this is a first level submenu
				{
					ultags[t].style.top = ultags[t].parentNode.parentNode.offsetHeight;
				}
				else //else if this is a sub level menu (ul)
				{
					// at this stage more than single menu levels are not supported
				}
				ultags[t].parentNode.onmouseover=function()
				{
					HoverOver(this);
				}
				ultags[t].parentNode.onmouseout=function()
				{
					HoverOut(this);
				}
			}
		}
	}
}

function HoverOver(node)
{
	node.getElementsByTagName("ul")[0].style.display = "block";
	ResizeIFrame(node);
}

function HoverOut(node)
{
	node.getElementsByTagName("ul")[0].style.display = "none";
	RemoveIFrame(node);
}

if (window.addEventListener)
window.addEventListener("load", buildsubmenus_horizontal, false)
else if (window.attachEvent)
window.attachEvent("onload", buildsubmenus_horizontal)

function ResizeIFrame(node)
{
	if (menuIFrame == null)
	{
		menuIFrame = document.createElement("iframe");
		menuIFrame.setAttribute("id","menuIFrame");
		menuIFrame.setAttribute("frameborder", "0");
		menuIFrame.setAttribute("scrolling", "no");
		menuIFrame.style.position = "absolute";
		menuIFrame.style.visibility = 'inherit';
		menuIFrame.style.width = node.offsetWidth;

		menuIFrame.style.zIndex = '-1';
	}
	if (node.style.display == '')
	{
		menuIFrame.style.top = node.offsetTop;
		menuIFrame.style.height = node.offsetHeight; 
		menuIFrame.style.left = node.offsetLeft;
		node.appendChild(menuIFrame);
	}
}

var menuIFrame;

function RemoveIFrame(node)
{
	if (menuIFrame != null && node.contains(menuIFrame))
	{
		node.removeChild(menuIFrame);
	}
}