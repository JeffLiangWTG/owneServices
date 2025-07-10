function ExecuteServiceMethod(methodName, parameters) {
    try {
        Enterprise.ZArchitecture.Web.ServerServices.WebServiceShared.Execute(methodName, parameters, HandleServiceMethodResponse);
    }
    catch (ex) { alert(ex); }
}

function HandleServiceMethodResponse(response) {
    var responseObject;
    try {
        responseTokens = JSON.decode(response, true);
        if (responseTokens) {
            responseTokens.each(function (responseToken, index) {
                if (AllowExecution(responseToken)) {
                    if (responseToken.Action == 'Error') {
                        HandleShowErrorResponse(responseToken);
                    } else if (responseToken.Action == 'PostBack') {
                        HandlePostBackResponse(responseToken);
                    } else if (responseToken.Action == 'Update') {
                        HandleUpdateValueResponse(responseToken);
                    } else if (responseToken.Action == 'UpdateList') {
                        HandleUpdateListResponse(responseToken);
                    } else if (responseToken.Action == 'Focus') {
                        HandleSetFocusResponse(responseToken);
                    } else if (responseToken.Action == 'ReadOnly') {
                        HandleSetReadOnlyResponse(responseToken);
                    } else {
                        alert('Unknown WebService response type: ' + responseToken.Action);
                    }
                }
            });
        }
    }
    catch (ex) { alert (ex) }
}

function AllowExecution(responseToken) {
    var result = true;
    var firstCondition = true;
    var conditionToken;
    if (responseToken.Conditions) {
        for (var i = 0; i < responseToken.Conditions.length; i++) {
            conditionToken = responseToken.Conditions[i];
            if (conditionToken && conditionToken.JavaScript) {
                if (conditionToken.JavaScript.trim() != "") {
                    if (firstCondition) {
                        firstCondition = false;
                        result = false;
                    }
                    result = result || (eval(conditionToken.JavaScript) && AllowExecution(conditionToken));
                    if (result) {
                        return result;
                    }
                }
            }
        }
    }
    return result;
}

function HandleSetReadOnlyResponse(responseToken) {
    var control = $(responseToken.ControlID);
    if (control) {
		var readOnly = responseToken.Value === 'True';
		var enabledDisplayStyle = control.get('enabled-display');
		if (enabledDisplayStyle) {
			if (readOnly) {
				control.setStyles({ 'display': 'none' });
			}
			else {
				control.setStyles({ 'display': enabledDisplayStyle });
			}
		}
		if (readOnly) {
			control.setProperty('readonly', 'readonly');
			control.setProperty('disabled', 'disabled');
			control.addClass('aspNetDisabled');
			control.setStyles({ 'background-color': 'transparent', 'border': 'none' });
		}
		else {
			control.removeProperty('readonly');
			control.removeProperty('disabled');
			control.removeClass('aspNetDisabled');
			control.setStyles({ 'background-color': '', 'border': '' });
        }
    }
}

function HandleSetFocusResponse(responseToken) {
    setTimeout("$('" + responseToken.ControlID + "').focus()", 100);
}

function HandlePostBackResponse(responseToken) {
    setTimeout("__doPostBack('" + responseToken.ControlID + "','')", 0);
}

function HandleUpdateValueResponse(responseToken) {
    var control = $(responseToken.ControlID);
    if (control) {
        if (control.tagName == 'SPAN') {
            control.innerHTML = responseToken.Value;
        }
        else if (control.get('value') != responseToken.Value) {
            control.set('value', responseToken.Value);
            try {
                control.onchange();
            }
            catch (ex) { }
        }
    }
}

function HandleUpdateListResponse(responseToken) {
	var control = $(responseToken.ControlID);
	if (control) {
		if (control.tagName == 'SELECT') {
			var cachedValue = control.get('value');
			for (var i = control.options.length - 1; i >= 1; i--) {
				control.options[i].remove();
			}

			var values = JSON.decode(responseToken.Value);
			for (var i = 0; i < values.length; i++) {
				var option = document.createElement('OPTION');
				var value = values[i];
				option.value = value.Code;
				option.innerHTML = value.Description;
				control.options.add(option);
			}

			control.set('value', cachedValue);
		}
	}
}

function GetControlValue(control) {
    if (control) {
        if (control.tagName == 'SPAN') {
            return control.innerHTML || '';
        }

        return control.get('value');
    }
}

function GetControlNumericValue(control) {
    return parseFloat(GetControlValue(control) || 0);
}

function HandleShowErrorResponse(responseToken) {
    alert('Error: ' + responseToken.Value);
}
