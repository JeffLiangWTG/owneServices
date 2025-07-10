//--------------------------------------------------
// Select all the checkboxes for the current collumn
//--------------------------------------------------
function SelectAllCheckBoxesForThisColumn(chkBox)
{
    xState = chkBox.checked;    

    elm = chkBox.form.elements;
    var commonIDPart = chkBox.id.substring(chkBox.id.indexOf("ZCheckBoxForSelectColumn-"));
    for (i = 0; i < elm.length; i++) {
        if (elm[i].type == "checkbox" && elm[i].id != chkBox.id && elm[i].checked != xState && elm[i].id.indexOf(commonIDPart) !== -1)
        {
            elm[i].click();
        }
    }
}
// -->

if (typeof(Sys) != "undefined"){
    Sys.Application.notifyScriptLoaded();
}