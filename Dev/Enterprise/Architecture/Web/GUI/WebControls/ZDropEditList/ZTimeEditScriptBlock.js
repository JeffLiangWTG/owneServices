var TedSMN = new Array
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

function NormalizeAndValidateTime(controlID) {
    var control = $(controlID);
    if (control == null || control.value == '') {
        return false;
    }
    try {
        var d = parseTimeString(control.value);
        control.value = padAZero(d.getHours()) + ':' + padAZero(d.getMinutes());
        return true;
    }
    catch (e) {
        alert('Please enter valid time!');
        setTimeout("$('" + controlID + "').focus()", 100);
        return false;
    }
}

function padAZero(s) {
    s = s.toString();
    if (s.length == 1) {
        return '0' + s;
    }
    else {
        return s;
    }
}

var timeParsePatterns = [
{// Now (example: now)
    re: /^now/i,
    handler: function() { return new Date(); }
},
{// p.m. (example: '9:55 pm','12:55 p.m.','9:55 p','11:5pm','9:5p')
    re: /(\d{1,2}):(\d{1,2})(?:p| p)/,
    handler: function(bits) {
        var h = parseInt(bits[1], 10);
        if (h < 12) { h += 12; }
        return GetDate(h, parseInt(bits[2], 10));
    }
},
{// p.m., (example: '9 pm','12 p.m.','9 p','11pm','9p')
    re: /(\d{1,4})(?:p| p)/,
    handler: function(bits) {
        var h, m;
        if (bits[1].length == 3) {
            h = parseInt(bits[1].substring(0, 1), 10);
            m = parseInt(bits[1].substring(1, 3), 10);
        }
        else if (bits[1].length == 4) {
            h = parseInt(bits[1].substring(0, 2), 10);
            m = parseInt(bits[1].substring(2, 4), 10);
        }
        else {
            h = parseInt(bits[1], 10);
            m = 0;
        }
        if (h < 12) { h += 12; }
        return GetDate(h, parseInt(m, 10));
    }
},
{// hh:mm (example: '9:55','19:55','19:5','9:55 a.m.','11:55a')
    re: /(\d{1,2}):(\d{1,2})/,
    handler: function(bits) {
        return GetDate(parseInt(bits[1], 10), parseInt(bits[2], 10));
    }
},
// hhmm (example: '9','9a','9am','19','1950','0955')
    {
    re: /(\d{1,4})/,
    handler: function(bits) {
        var h, m;
        if (bits[1].length == 3) {
            h = bits[1].substring(0, 1);
            m = parseInt(bits[1].substring(1, 3), 10);
        }
        else {
            h = bits[1].substring(0, 2);
            m = parseInt(bits[1].substring(2, 4), 10);
        }
        if (isNaN(m)) { m = 0; }
        return GetDate(parseInt(h, 10), parseInt(m, 10));
    }
},
];


function GetDate(h, m) {
    if (h > 23 || m > 59) {
        throw new Error();
    }
    else {
        var d = new Date();
        d.setHours(h);
        d.setMinutes(m);
        return d;
    }
}

function parseTimeString(s) {
    for (var i = 0; i < timeParsePatterns.length; i++) {
        var re = timeParsePatterns[i].re;
        var handler = timeParsePatterns[i].handler;
        var bits = re.exec(s.toLowerCase());
        if (bits) {
            return handler(bits);
        }
    }
    throw new Error();
}

function ZDropEditList_SelectItem_AdditionalHandler(RelatedControlID) {
    var relatedControl = document.getElementById(RelatedControlID);
    if (relatedControl != null && relatedControl.value == '') {
        var nowDate;
        var now = new Date();
        var day = now.getDate();
        if (day < 10) {
            nowDate = '0' + day;
        }
        else {
            nowDate = '' + day;
        }
        var month = now.getMonth();
        for (var i = 0; i < 12; i++) {
            if (i == month) {
                nowDate = nowDate + '-' + TedSMN[i] + '-';
                break;
            }
        }
        var year = now.getFullYear();
        year = year % 100;
        if (year < 10) {
            nowDate = nowDate + '0' + year;
        }
        else {
            nowDate = nowDate + year;
        }
        relatedControl.value = nowDate;
    }
}

function ZDateTimeEdit_SetDefaultTimezone(TimezoneTextBoxID) {
	var TimezoneTextBox = $(TimezoneTextBoxID);
	if (TimezoneTextBox != null) {
		if (TimezoneTextBox.value == '') {
			var offset = Math.round(0 - new Date().getTimezoneOffset() / 60);
			if (offset >= 0) {
				TimezoneTextBox.value = "GMT +" + (offset <= 9 ? '0' + offset : offset) + ":00";
			}
			else {
				TimezoneTextBox.value = "GMT -" + (offset >= -9 ? '0' + Math.abs(offset) : Math.abs(offset)) + ":00";
			}
		}
		else {
			var content = TimezoneTextBox.value.replace(" ", "")
			var re = new RegExp("^(GMT)([+-])([0-9]{2}):([0-9]{2})", "g")
			if (!re.test(content)) {
				alert('Please enter valid timezone!');
				setTimeout("$('" + TimezoneTextBoxID + "').focus()", 100);
			}
		}
	}
}

if (typeof(Sys) != "undefined"){
    Sys.Application.notifyScriptLoaded();
}
