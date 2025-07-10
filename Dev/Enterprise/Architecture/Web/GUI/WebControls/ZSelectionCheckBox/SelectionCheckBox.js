//-------------------------------------------------------------
// Select all the checkboxes (Hotmail style)
//-------------------------------------------------------------
function SelectAllCheckboxes(chkBox)
{
    xState = chkBox.checked;    

    elm = chkBox.form.elements;
    for (i=0; i<elm.length; i++)
        if (elm[i].type == "checkbox" && elm[i].id != chkBox.id && elm[i].checked != xState && chkBox.id.substring(0, elm[i].id.substring(0, elm[i].id.indexOf('_')) == chkBox.id.substring(0, chkBox.id.indexOf('_'))))
    {
        elm[i].click();
    }
}

//-------------------------------------------------------------
// Highlight row when the checkboxes are selected
//-------------------------------------------------------------
function HighlightRow(chkBox, selectedClass, unselectedClass)
{
    xState = chkBox.checked;    
    if (xState)
    {
 	    chkBox.parentElement.parentElement.parentElement.className = selectedClass;
    }
    else 
    {
        chkBox.parentElement.parentElement.parentElement.className = unselectedClass; 
    }
}
// -->

if (typeof(Sys) != "undefined"){
    Sys.Application.notifyScriptLoaded();
}