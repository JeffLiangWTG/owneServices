
/* code uses functions from mootools library */

function OnSubmitButtonClick(e) {
	var target;
	if (!e) {
		e = window.event;
		target = (window.event) ? e.srcElement : e.target;
	}
	else {
		target = e.target;
	}
	if (target) {
		setTimeout(function () { DisableSubmitButton(target); target = null; }, 0);
	}
	return true;
}

addLoadEvent(SetFormSubmitEvent);

function DisableSubmitButton(target) {
	if (target) {
		target.set('disabled', true);
	}
}

function SetFormSubmitEvent() {

	if (!(window.DisabledButtons === undefined)) {
		if (DisabledButtons) {
			for (var i = 0; i < DisabledButtons.length; i++) {
				var button = $(DisabledButtons[i]);
				if (button) {
					button.addEvent('click', OnSubmitButtonClick);
				}
			}
		}
	}
}

function ZPage_ProcessKeyDown(keyCode, id, e) {
	var evt = new Event(e);
	if (evt.code == keyCode) {
		var elem = $(id);
		if (elem) {
			elem.focus();
			elem.click();
		}

		evt.stopPropagation();
		return false;
	}
}

function addLoadEvent(func) {
	var oldonload = window.onload;
	if (typeof window.onload != 'function') {
		window.onload = func;
	} else {
		window.onload = function () {
			if (oldonload) {
				oldonload();
			}
			func();
		}
	}
}

function getElement(Reference) {
	try {
		if ($(Reference)) {
			return $(Reference);
		}
		var controls = document.getElementsByName(Reference);
		if (controls) {
			if (controls.length == 1) {
				return $(controls[0]);
			}
		}
	}
	catch (e) { }
	return null;
}

function changeLanguage(Language) {
    setCookie("language", Language, 365);
    window.location.reload(true);
}

function setCookie(name, value, exdays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + exdays);
    var value = escape(value) + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString());
    document.cookie = name + "=" + value;
}

function setFocus(controlID) {
	var control = $(controlID);
	if (control) {
		if (control.select) {
			control.select();
		}
		if (control.focus) {
			control.focus();
		}
	}
}