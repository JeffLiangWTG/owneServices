function FormatDate(dateControlID, timeControlID, dateFormatType) {
    if ($(dateControlID)) {
        var timeValue = "";
        if ($(timeControlID)) {
            timeValue = $(timeControlID).get('value');
        }
        ExecuteServiceMethod("FormatDate", JSON.encode({ DateControlID: dateControlID, DateValue: $(dateControlID).get('value'), DateFormatType: dateFormatType, TimeControlID: timeControlID, TimeValue: timeValue }));
    }
}

var DefaultAbbreviatedMonthNames = new Array(
    "JAN",
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

