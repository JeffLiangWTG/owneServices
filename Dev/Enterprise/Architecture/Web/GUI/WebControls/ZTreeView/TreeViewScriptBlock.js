
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

addLoadEvent(function() {
	InitialiseTreeView();
})

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
			ExpandImg.src = ImageDirectory()+"minus.gif";
		}
		ChildElement.style.display = 'block';
	}
	// Collapse the branch if it IS visible
	else
	{
		// Change the image (if there is an image)
		if (HasExpandImage)
		{
			ExpandImg.src = ImageDirectory()+"plus.gif";
		}
		ChildElement.style.display = 'none';
	}
}

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


function DisplayDescriptionAndQuantity(Id, DescTarget, Code, Description, QuantityTarget, Quantity)
{
	SelectItem(Id, Code);
	DisplayDescription(DescTarget, Description);
	DisplayQuantity(QuantityTarget, Quantity);
}

function InitialiseTreeView()
{
	InitialiseSelectedElement();
	InitialiseScrollPosition();
}

function ImageDirectory()
{
	var RuntimeDirectoryField = RuntimeDirectory();
	if(RuntimeDirectoryField != null && RuntimeDirectoryField.value != "")
	{
		return RuntimeDirectoryField.value;
	}
	else
	{
		return "//";
	}
}

function RuntimeDirectory()
{
	return GetField("RuntimeDirectory");
}

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
		var DescriptionControlField = GetField("DescriptionControl");
		if (DescriptionControlField != null && DescriptionControlField.value != "")
		{
			DisplayDescription(DescriptionControlField.value, DescriptionField.value);
		}
	}
	if(QuantityField != null)
	{
		var QuantityControlField = GetField("QuantityControl");
		if (QuantityControlField != null && QuantityControlField.value != "")
		{
			DisplayQuantity(QuantityControlField.value, QuantityField.value);
		}
	}
}

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

function GetField(FieldName)
{
	var Field = document.getElementById(FieldName);
	if(Field != null && Field.nodeName == "INPUT")
		return Field;
	else
		return null;	
}

function SelectItem(ItemId, Code)
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

	if(Code != null && Code != "")
	{
		var CodeField = document.getElementById("Code");
	
		if(CodeField != null && CodeField.nodeName == "INPUT")
		{
			CodeField.value = unescape(Code);
		}
	}
}

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

function DisplayDescription(Target, Arg)
{
	var DescriptionBox = document.getElementById(Target);
	
	if(DescriptionBox != null && DescriptionBox.nodeName == "DIV")
	{
		DescriptionBox.innerText = unescape(Arg);
	}
}

function DisplayQuantity(Target, Arg)
{
	var QuantityBox = document.getElementById(Target);
	
	if(QuantityBox != null && QuantityBox.nodeName == "DIV")
	{
		QuantityBox.innerText = Arg;
	}
}

function GetSelectedItemCode()
{
	var SelectedItemField = GetField('Code');
	if(SelectedItemField != null && SelectedItemField.value != '')
	{
		return SelectedItemField.value;
	}
	else
	{
		return '';
	}
}

function GetSelectedItemDescription()
{
	var SelectedItemField = GetField('Description');
	if(SelectedItemField != null && SelectedItemField.value != '')
	{
		return SelectedItemField.value;
	}
	else
	{
		return '';
	}
}