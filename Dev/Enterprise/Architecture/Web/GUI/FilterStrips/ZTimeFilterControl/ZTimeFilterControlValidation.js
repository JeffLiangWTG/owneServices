function ValidateUserTimeInput(control, pattern, message) {
	var regex = new RegExp(pattern);
	var value = control.value.trim();
	var isEmpty = value.replace(':', '').replace('0', '').length === 0;
	if (isEmpty) {
		value = ':';
	}
	else {
		value = value.replace(/^0+/, '');

		if (value.indexOf(':') < 0) {
			value += ':';
		}

		if (value[0] === ':') {
			value = '00' + value;
		}

		value += '00';

		for (var i = value.length - 1; i > 0; i--) {
			if (regex.test(value)) {
				break;
			}

			var last = value[i];
			if (last === '0') {
				value = value.substring(0, i);
			}
			else {
				break;
			}
		}
	}

	if (control.value !== value) {
		control.value = value;
	}

    if (!isEmpty && !regex.test(control.value)) {
        var controlID = control.id;
        alert(message);
        setTimeout(function () { setFocus(controlID); }, 100);

        return false;
    }
}
