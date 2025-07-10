var lastFocusFieldID = "__LASTFOCUSID";
var beforeLastFocusFieldID = "__BEFORELASTFOCUSID";
var updateFocusFieldFlagID = "__UPDATELASTFOCUSID";
var focusChanged = function() {focusHandler(this.id)};

document.addEvent('domready', addPageRequestManagerEventHandlers);

function focusHandler(id) 
{
    if ($(updateFocusFieldFlagID))
    {
        if ($(updateFocusFieldFlagID).get('value')=='')
        {
            if (id && typeof(id) != "undefined" && id!="")
            {
                try
                {
                    $(beforeLastFocusFieldID).set('value', $(lastFocusFieldID).get('value'));
                }
                catch(ex){}
                try
                {
                    $(lastFocusFieldID).set('value', id);
                }
                catch(ex){}
            }
        }
    }
}

function addPageRequestManagerEventHandlers()
{
    if (typeof(Sys) != "undefined")
    {
        Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(postbackRequestHandler);
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoadedHandler);
        pageLoadedHandler(this, null);  
    }
}

function focusControl(targetControl) 
{
    try
    {
        $('Testing').set('value', $(targetControl).get('id'));
        targetControl.focus();
        targetControl.select();
    }
    catch(ex){}
}

function postbackRequestHandler(sender, args)
{
    if ($(updateFocusFieldFlagID))
    {
        $(updateFocusFieldFlagID).set('value', 'No');
    }
}

function pageLoadedHandler(sender, args) 
{
    var elementCount=0;
    var inputElements = $$('input', 'a', 'select');
    for(var i = 0; i < inputElements.length; i++)
    {
        if ($(inputElements[i]).get('type')!='hidden')
        {
            if ($(inputElements[i]).get('id')!="undefined" && $(inputElements[i]).get('id')!="")
            {
                $(inputElements[i]).removeEvent('focus', focusChanged);
                $(inputElements[i]).addEvent('focus', focusChanged);
                elementCount++;
            }
        }
        
    }
    setTimeout("setSavedFocus()", 100);
    SetFormSubmitEvent();  
}

function canHaveFocus(id)
{
    if ($(id))
    {
        if ($(id).get('tag')=='input')
        {
            if ($(id).get('type')=='text' || $(id).get('type')=='checkbox')
            {
                if ($(id).get('readonly')=='' || $(id).get('readonly')=='undefined')
                {
                    return true;
                }
            }
        }
        else
        {
            if ($(id).get('tag')=='a' || $(id).get('tag')=='select')
            {
                return true;
            }
        }
    }
    return false;
}

function setSavedFocus()
{
    var newFocused = null;
    if ($(lastFocusFieldID) && canHaveFocus($(lastFocusFieldID).get('value')))
    {
        newFocused = $($(lastFocusFieldID).get('value'));
    }
    if (!newFocused)
    {
        if ($(beforeLastFocusFieldID) && canHaveFocus($(beforeLastFocusFieldID).get('value')))
        {
            newFocused = $($(beforeLastFocusFieldID).get('value'));
        }
    }
    try
    {
        $(updateFocusFieldFlagID).set('value', '');
    }
    catch(ex){}          
    if (!newFocused) 
    {
        try
        {
            $(beforeLastFocusFieldID).set('value', '');
        }
        catch(ex){}
        try
        {
            $(lastFocusFieldID).set('value', '');
        }
        catch(ex){}    
    }
    else
    {
        focusControl(newFocused);
    }   
}

