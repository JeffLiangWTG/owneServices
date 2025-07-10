var CalSMN = new Array
	("JAN",
		"FEB",
		"MAR",
		"APR",
		"MAY",
		"JUN",
		"JUL",
		"AUG",
		"SEP",
		"OCT",
		"NOV",
		"DEC");

function ZDateEdit_ShowCalendar() {
	if (ZTextPopup_Popup != null && ZTextPopup_Popup.contentWindow != null && ZTextPopup_Popup.contentWindow.SetSelectedDate != null) {
		if (ZTextPopup_TextBox != null) {
			ZTextPopup_Popup.contentWindow.SetSelectedDate(ZTextPopup_TextBox.value);
		}
	}
}

function ZDateEdit_SetTime(TimeTextBoxID, Time) {
	var TimeTextBox = $(TimeTextBoxID);
	if (TimeTextBox != null) {
		if ((Time != '' && TimeTextBox.value == '') || (Time == '' && TimeTextBox.value != '')) {
			TimeTextBox.value = Time;
		}
	}
}

function ZDateEdit_SetDefaultTimezone(TimezoneTextBoxID, DateTextBoxID) {
	var DateBox = $(DateTextBoxID)
	if (DateBox == null || DateBox.value == "") {
		return;
	}
	var TimezoneTextBox = $(TimezoneTextBoxID);
	if (TimezoneTextBox != null && TimezoneTextBox.value == '') {
		var offset = Math.round(0 - new Date().getTimezoneOffset() / 60);
		if (offset >= 0) {
			TimezoneTextBox.value = "GMT +" + (offset <= 9 ? '0' + offset : offset) + ":00";
		}
		else {
			TimezoneTextBox.value = "GMT -" + (offset >= -9 ? '0' + Math.abs(offset) : Math.abs(offset)) + ":00";
		}
	}
}

function ZDateEdit_CalendarHeightChanged(Height) {
	if (ZTextPopup_Popup != null) {
		ZTextPopup_Popup.style.height = Height;
	}
}

function NormalizeAndValidateDate(controlID, format) {
	var control = $(controlID);
	if (control == null || control.value == '') {
		return false;
	}
	if (NormalizeDate(control, format)) {
		return true;
	}
	var result = ValidateDate(control.value);
	if (!result) {
		alert('Please enter valid date!');
		setTimeout("$('" + controlID + "').focus()", 100);
	}
	return result;
}

function NormalizeDate(control, format) {
	var day, month, year;
	var monthNotDigital = false;
	var dateToNormalize = control.value;
	if (TryMatchDate("^\\d{1,2}$", dateToNormalize)) {
		day = dateToNormalize;
	}
	else if (TryMatchDate("^\\d{3,4}$", dateToNormalize)) {
		if (format == 'en-us' || format == 'ja') {
			day = dateToNormalize.substring(dateToNormalize.length - 2);
			month = dateToNormalize.substring(0, dateToNormalize.length - 2);
		}
		else {
			day = dateToNormalize.substring(0, dateToNormalize.length - 2);
			month = dateToNormalize.substring(dateToNormalize.length - 2);
		}
	}
	else if (TryMatchDate("^\\d{1,2}[-/.]\\d{2}$", dateToNormalize)) {
		if (format == 'en-us' || format == 'ja') {
			day = dateToNormalize.substring(dateToNormalize.length - 2);
			month = dateToNormalize.substring(0, dateToNormalize.length - 3);
		}
		else {
			day = dateToNormalize.substring(0, dateToNormalize.length - 3);
			month = dateToNormalize.substring(dateToNormalize.length - 2);
		}
	}
	else if (TryMatchDate("^\\d{5,6}$", dateToNormalize)) {
		if (format == 'en-us') {
			month = dateToNormalize.substring(0, dateToNormalize.length - 4);
			day = dateToNormalize.substring(dateToNormalize.length - 4, dateToNormalize.length - 2);
			year = dateToNormalize.substring(dateToNormalize.length - 2);
		}
		else if (format == 'ja') {
			year = dateToNormalize.substring(0, dateToNormalize.length - 4);
			month = dateToNormalize.substring(dateToNormalize.length - 4, dateToNormalize.length - 2);
			day = dateToNormalize.substring(dateToNormalize.length - 2);
		}
		else {
			day = dateToNormalize.substring(0, dateToNormalize.length - 4);
			month = dateToNormalize.substring(dateToNormalize.length - 4, dateToNormalize.length - 2);
			year = dateToNormalize.substring(dateToNormalize.length - 2);
		}
	}
	else if (TryMatchDate("^\\d{1,2}[-/.]\\d{2}[-/.]\\d{2}$", dateToNormalize)) {
		if (format == 'en-us') {
			month = dateToNormalize.substring(0, dateToNormalize.length - 6);
			day = dateToNormalize.substring(dateToNormalize.length - 5, dateToNormalize.length - 3);
			year = dateToNormalize.substring(dateToNormalize.length - 2);
		}
		else if (format == 'ja') {
			year = dateToNormalize.substring(0, dateToNormalize.length - 6);
			month = dateToNormalize.substring(dateToNormalize.length - 5, dateToNormalize.length - 3);
			day = dateToNormalize.substring(dateToNormalize.length - 2);
		}
		else {
			day = dateToNormalize.substring(0, dateToNormalize.length - 6);
			month = dateToNormalize.substring(dateToNormalize.length - 5, dateToNormalize.length - 3);
			year = dateToNormalize.substring(dateToNormalize.length - 2);
		}
	}
	else if (TryMatchDate("^\\d{1,2}\\D{3}$", dateToNormalize)) {
		monthNotDigital = true;
		day = dateToNormalize.substring(0, dateToNormalize.length - 3);
		month = dateToNormalize.substring(dateToNormalize.length - 3);
	}
	else if (TryMatchDate("^\\d{1,2}[-/.]\\D{3}$", dateToNormalize)) {
		monthNotDigital = true;
		day = dateToNormalize.substring(0, dateToNormalize.length - 4);
		month = dateToNormalize.substring(dateToNormalize.length - 3);
	}

	if (day != null) {
		var now = new Date();
		if (month == null) {
			month = now.getMonth() + 1; //(it returns 0 to 11)
		}

		return TryNormalizeDate(control, year != null ? GetFullYear(year) : now.getFullYear(), monthNotDigital ? GetMonth(month) : month, day);
	}

	return false;
}

function TryMatchDate(pattern, dateToMatch) {
	var re = new RegExp(pattern);
	return re.exec(dateToMatch) != null;
}

function TryNormalizeDate(control, year, month, day) {
	var yearInt = parseInt(year, 10);
	var monthInt = parseInt(month, 10);
	var dayInt = parseInt(day, 10);
	if (monthInt > 0 && monthInt <= 12 && dayInt <= 31) {
		control.value = ConstructDateString(yearInt, monthInt, dayInt);
		return true;
	}
	return false;
}

function ConstructDateString(yearInt, monthInt, dayInt) {
	var result = ((dayInt < 10) ? '0' : '') + dayInt;
	for (var i = 1; i <= 12; i++) {
		if (i == monthInt) {
			result = result + '-' + CalSMN[i - 1] + '-';
			break;
		}
	}
	yearInt = yearInt % 100;
	result = result + ((yearInt < 10) ? '0' : '') + yearInt;
	return result;
}

function GetFullYear(year) {
	var yearInt = parseInt(year, 10);
	return ((yearInt < 59) ? 2000 : 1900) + yearInt;
}

function GetMonth(month) {
	var monthUppercase = month.toUpperCase();
	var monthInt = -1;
	var i = 0;
	for (i = 1; i <= 12; i++) {
		if (CalSMN[i - 1] == monthUppercase) {
			monthInt = i;
			break;
		}
	}
	return monthInt;
}

function ValidateDate(dateToValidate) {
	var re = new RegExp("^\\d{1,2}\\-[a-zA-Z]{3}\\-\\d{2}$");
	var m = re.exec(dateToValidate);
	if (m != null) {
		var date_array = dateToValidate.split('-');

		var day = date_array[0];

		var month = date_array[1];
		var monthInt = GetMonth(month);

		var year = date_array[2];
		var yearInt = GetFullYear(year);

		source_date = new Date(yearInt, monthInt - 1, day);

		var fullYear = source_date.getFullYear();
		var fullYearStr = fullYear.toString(10);

		if (year == fullYearStr.substr(2) && (monthInt - 1) == source_date.getMonth() && day == source_date.getDate()) {
			return true;
		}
	}
	return false;
}

function ZDateTimeEditValidateHandler(TimezoneTextBoxID) {
	setTimeout(() => {
		var TimezoneTextBox = $(TimezoneTextBoxID);
		if (TimezoneTextBox != null) {
			var content = TimezoneTextBox.value.replace(" ", "")
			var re = new RegExp("^(GMT)([+-])([0-9]{2}):([0-9]{2})", "g")
			if (!re.test(content)) {
				alert('Please enter valid timezone!');
				setTimeout("$('" + TimezoneTextBoxID + "').focus()", 100);
			}
		}
	}, 500)
}

if (typeof (Sys) != "undefined") {
	Sys.Application.notifyScriptLoaded();
}
