
function ValidateUserInput(control, pattern, message)
{
	var regex = new RegExp(pattern);
	
	if (control.value.length > 0 && !regex.test(control.value) )
    {
        var controlID = control.id;
        alert(message);
        setTimeout(function () { setFocus(controlID); }, 100);

        return false;
    }
}
