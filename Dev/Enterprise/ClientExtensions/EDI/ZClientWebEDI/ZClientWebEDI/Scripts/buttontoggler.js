function toggleButtonAvailabilityBasedOnEnteredFields(button, fields) {
	var checkResult = document.getElementById('CheckResult');
	var checkPass = checkResult == null || checkResult.value == "pass";

	if (button.disabled) {
		if (fields.every(x => x.value.length > 0) && checkPass) {
			button.removeAttribute("disabled");
		}
	} else {
		if (fields.some(x => x.value.length === 0) || !checkPass) {
			button.setAttribute("disabled", true);
		}
	}
}

function setupToggleButtonAvailabilityEventListener(button, fields) {
	if (button != null && fields.every(x => x != null)) {
		button.setAttribute("disabled", true);
		fields.forEach(x => x.addEventListener("keyup", function () { toggleButtonAvailabilityBasedOnEnteredFields(button, fields) }));
	}
}
