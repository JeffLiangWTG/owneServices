/*
** Treeview scripts to manage the treeview
**
**
*/

/*
** addLoadEvent(func) - Adds function to list of functions called when page is loaded
*/
function addLoadEvent(func) {
    var oldonload = window.onload;
    if (typeof window.onload != 'function') {
        window.onload = func;
    } else {
        window.onload = function() {
            oldonload();
            func();
        }
    }
}

/*
** Initialise TreeView on load
*/

addLoadEvent(function() {
	InitialiseTreeView();
})


/*
** Function to hide/display a node
*/
function Toggle(node)
{
    // Get the next tag (read the HTML source)
	var ChildElement = node.parentElement.parentElement.nextSibling;
	
	// find the next DIV
	while(ChildElement.nodeName != "TR") {
		ChildElement = ChildElement.nextSibling;
	}

	var HasExpandImage = false;
	
	if ( (node.parentElement.childNodes.length > 0) &&
		 (node.parentElement.childNodes[0].childNodes.length > 0) &&
		 (node.parentElement.childNodes[0].childNodes[0].nodeName == "IMG"))
	{
		HasExpandImage = true;
		var ExpandImg = node.parentElement.childNodes[0].childNodes[0];
	}
	
	// Unfold the branch if it isn't visible
	if (ChildElement.style.display == 'none')
	{
		// Change the image (if there is an image)
		if(HasExpandImage)
		{
			ExpandImg.src = "images/minus.gif";
		}
		ChildElement.style.display = 'block';
	}
	// Collapse the branch if it IS visible
	else
	{
		// Change the image (if there is an image)
		if (HasExpandImage)
		{
			ExpandImg.src = "images/plus.gif";
		}
		ChildElement.style.display = 'none';
	}
}

/*
** Function to post back and get branch for node that has not yet been loaded
*/
function LoadExpandPostBack(eventTarget, eventArgument)
{
	var theform;
	if (window.navigator.appName.toLowerCase().indexOf("microsoft") > -1) {
		theform = document.Form1;
	}
	else {
		theform = document.forms["Form1"];
	}
	theform.__EVENTTARGET.value = eventTarget.split("$").join(":");
	theform.__EVENTARGUMENT.value = eventArgument;
	theform.submit();
}


/*
** Function to display the description of a selected node
*/
function DisplayDescriptionAndQuantity(Id, DescTarget, Description, QuantityTarget, Quantity)
{
	SelectItem(Id);
	DisplayDescription(DescTarget, Description);
	DisplayQuantity(QuantityTarget, Quantity);
}

/*
** Function to Initialise the TreeView
*/
function InitialiseTreeView()
{
	InitialiseSelectedElement();
	InitialiseScrollPosition();
}

/*
** Function to initialise selection after postback
*/
function InitialiseSelectedElement()
{
	var SelectedField = GetField("SelectedItem");
	var DescriptionField = GetField("Description");
	var QuantityField = GetField("Quantity");
	if(SelectedField != null && SelectedField.value != "")
	{
		SetElementBackgroundColor(SelectedField.value, "Gray");
		SelectItem(SelectedField.value);
		var SelectedItem = document.getElementById(SelectedField.value);
	}
	if(DescriptionField != null)
	{
		DisplayDescription("TestTreeView.DescriptionBox", DescriptionField.value);
	}
	if(QuantityField != null)
	{
		DisplayQuantity("TestTreeView.Quantity", QuantityField.value);
	}
}

/*
** Function to return the scroll position to bring the selected element into view
*/
function InitialiseScrollPosition()
{	
	var Scroller = document.getElementById("ScrollDiv");

	if(Scroller != null)
	{
		var ExpandedPathField = document.getElementById("ExpandedPath");
		if(ExpandedPathField != null && ExpandedPathField.value != "")
		{
			Scroller.scrollTop = GetElementOffset(ExpandedPathField.value);
		}
		else
		{
			var SelectedField = document.getElementById("SelectedItem");
			if(SelectedField != null && SelectedField.value != "")
			{
				var SelectedItem = document.getElementById(SelectedField.value);
				if(SelectedItem != null)
				{
					Scroller.scrollTop = GetElementOffset(SelectedField.value);
				}
			}
		}
	}
}

/*
** Function to get the offset of an element from the top of the treeview.
** This works recursively getting the offset from all parent elements up the tree
*/
function GetElementOffset(ElementId)
{
	var Element = document.getElementById(ElementId);
	var offset=0;
	if(Element != null)
	{
		// offset = Element.offsetTop;
		while((Element.parentElement != null) && 
			(Element.parentElement != document.getElementById("ScrollDiv")))
		{
			if(Element.parentElement.nodeName != "TD")
			{
				offset += Element.parentElement.offsetTop;
			}
			Element = Element.parentElement;
		}
	}
	return offset;
}

/*
** Get the element with a given name or null if the element does not exist.
*/
function GetField(FieldName)
{
	var Field = document.getElementById(FieldName);
	if(Field != null && Field.nodeName == "INPUT")
		return Field;
	else
		return null;	
}

/*
** Selects an element with a given id
*/
function SelectItem(ItemId)
{
	if(ItemId != null && ItemId != "")
	{
		var SelectionField = document.getElementById("SelectedItem");
		var SelectedElement = document.getElementById(ItemId);
		
		if(SelectionField != null && SelectionField.nodeName == "INPUT")
		{
			if(SelectionField.value != "")
			{
				SetElementBackgroundColor(SelectionField.value, "white");
			}
			SelectionField.value = ItemId;
		}
		SetElementBackgroundColor(ItemId, "Gray");
	}
}

/*
** Sets the background color of an element to a particular color
** This method should be replaced with a css class
*/
function SetElementBackgroundColor(ElementID, Color)
{
	if(ElementID != null && ElementID != "")
	{
		var SelectedElement = document.getElementById(ElementID);
		if(SelectedElement != null)
		{
			SelectedElement.style.backgroundColor = Color;
		}
	}
}

/*
** Displays the description in a particular target div.
*/
function DisplayDescription(Target, Arg)
{
	var DescriptionBox = document.getElementById(Target);
	
	if(DescriptionBox != null && DescriptionBox.nodeName == "DIV")
	{
		DescriptionBox.innerText = unescape(Arg);
	}
}

/*
** Displays the quantity in particular div
*/
function DisplayQuantity(Target, Arg)
{
	var QuantityBox = document.getElementById(Target);
	
	if(QuantityBox != null && QuantityBox.nodeName == "DIV")
	{
		QuantityBox.innerText = Arg;
	}
}

