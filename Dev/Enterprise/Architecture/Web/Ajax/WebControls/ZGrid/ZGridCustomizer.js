function moveAll(lbFromID, lbToID, isSorted) {
    lbFrom = $(lbFromID);
    lbTo = $(lbToID);

    var tempArray = [];
    for (var i = 0; i < lbFrom.length; i++) {
        if (!isRequiredColumn(lbFrom.item(i))) {
            tempArray.push(lbFrom.item(i));
        }
    }

    if (isSorted) {
        for (var j = 0; j < lbTo.length; j++) {
            tempArray.push(lbTo.item(j));
        }

        tempArray.sort(function (a, b) {
            var aText = a.text.toLowerCase();
            var bText = b.text.toLowerCase();
            return aText < bText ? -1 : aText > bText ? 1 : 0;
        });
    }

    for (var k = 0; k < tempArray.length; k++) {
        lbTo.appendChild(tempArray[k]);
    }

    updateBoth(lbFrom, lbTo);
}

function moveSelected(lbFromID, lbToID, isSorted) {
    lbFrom = $(lbFromID);
    lbTo = $(lbToID);

    var selIndex = lbFrom.selectedIndex;
    if (selIndex < 0)
        return;
    var selected = lbFrom.options.item(selIndex);
    if (isRequiredColumn(selected)) {
        alert("This is required column and can not be removed from this list.");
        return;
    }

    var itemToInsert = lbFrom.options.item(selIndex);
    if (isSorted) {
        for (var i = 0; i < lbTo.length; i++) {
            var currentOption = lbTo.options[i];
            if (currentOption.text > itemToInsert.text) {
                lbTo.insertBefore(itemToInsert, currentOption);
                updateBoth(lbFrom, lbTo);
                return;
            }
        }
    }

    lbTo.appendChild(itemToInsert);
    updateBoth(lbFrom, lbTo);
}

function isRequiredColumn(item) {
    if (item.text.contains("[") && item.text.contains("]")) {
        return true;
    }
    return false;
}

function updateBoth(list1, list2) {
    selectNone(list1);
    selectNone(list2);

    updateSize(list1);
    updateSize(list2);
}

function updateSize(list) {
    list.size = getSize(list);
}

function selectNone(list) {
    list.selectedIndex = -1;
}

function getSize(list) {
    var len = list.childNodes.length;
    var nsLen = 0;
    for (i = 0; i < len; i++) {
        if (list.childNodes.item(i).nodeType == 1)
            nsLen++;
    }
    if (nsLen < 2)
        return 2;
    else
        return nsLen;
}

function moveSelectedUp(listBoxID) {
    var listBox = $(listBoxID);
    var selectedIndex = listBox.selectedIndex;
    if (selectedIndex > 0) {
        var listOpt = listBox.options[selectedIndex];
        var nextOpt = listBox.options[selectedIndex - 1];
        switchValues(listOpt, nextOpt);
        listBox.selectedIndex = selectedIndex - 1;
    }
}

function moveSelectedDn(listBoxID) {
    var listBox = $(listBoxID);
    var selectedIndex = listBox.selectedIndex;
    if (selectedIndex < listBox.childNodes.length) {
        var listOpt = listBox.options[selectedIndex];
        var nextOpt = listBox.options[selectedIndex + 1];
        switchValues(listOpt, nextOpt);
        listBox.selectedIndex = selectedIndex + 1;
    }
}

function switchValues(curOpt, anotherOpt) {
    var curTxt = curOpt.text;
    var curVal = curOpt.value;
    var anotherTxt = anotherOpt.text;
    var anotherVal = anotherOpt.value;

    curOpt.text = anotherTxt;
    curOpt.value = anotherVal;

    anotherOpt.text = curTxt;
    anotherOpt.value = curVal;
}

function getList(lbId) {
    lb = $(lbId);
    var optionList = lb.options;
    var result = '';
    var len = optionList.length;
    for (i = 0; i < len; i++) {
        if (result != '') {
            result += ',';
        }
        result += optionList.item(i).value;
    }
    return result;
}

function ShowControlPopup(buttonID, controlID) {
    ZTextPopup_Button = $(buttonID);
    ZTextPopup_Popup = $(controlID);

    if (ZTextPopup_Button == null) return;
    if (ZTextPopup_Popup == null) return;

    ZTextPopup_Popup.setStyle('display', 'inline');

    mainControlDimensions = ZTextPopup_Button.getCoordinates();

    var popupLeft = mainControlDimensions.left;
    var popupAboveTop = mainControlDimensions.top;
    var popupBelowTop = mainControlDimensions.top + mainControlDimensions.height;
    var popupRight = mainControlDimensions.right;
    var popupBottom = mainControlDimensions.bottom;

    var popupHeight = parseInt(ZTextPopup_Popup.getStyle('height'));
    var popupWidth = parseInt(ZTextPopup_Popup.getStyle('width'));

    if (ZTextPopup_ShowAbove(popupBelowTop, popupAboveTop, popupHeight)) {
        ZTextPopup_Popup.setStyle('top', popupAboveTop - popupHeight);
    }
    else {
        ZTextPopup_Popup.setStyle('top', popupBelowTop);
    }

    var windowLeft = posLeft()
    var windowRight = posRight();
    var windowWidth = windowRight - windowLeft;

    if (popupLeft + popupWidth > windowRight) {
        popupLeft = windowRight - popupWidth;
    }

    if (popupLeft < windowLeft) {
        popupLeft = windowLeft;
    }

    ZTextPopup_Popup.setStyle('left', popupLeft);
}

var FadeStep = 0.25;

function FadeIn(elementID) {
    if ($(elementID)) {
        if (GetOpacity(elementID) < 1) {
            $(elementID).fadeDirection = 1;
            setTimeout("FadeHelper('" + elementID + "')", 100);
        }
    }
}

function FadeOut(elementID) {
    if ($(elementID)) {
        if (GetOpacity(elementID) > 0) {
            $(elementID).fadeDirection = -1;
            setTimeout("FadeHelper('" + elementID + "')", 100);
        }
    }
}

function FadeHelper(elementID) {
    if ($(elementID)) {
        var fadeDirection = GetFadeDirection(elementID);
        var opacity = GetOpacity(elementID);
        if ((fadeDirection > 0 && opacity < 1) || (fadeDirection < 0 && opacity > 0)) {
            opacity += fadeDirection * FadeStep;
            if (fadeDirection > 0 && opacity > 1) {
                opacity = 1;
            }
            if (fadeDirection < 0 && opacity < 0) {
                opacity = 0;
            }
            SetOpacity(elementID, opacity);
            setTimeout("FadeHelper('" + elementID + "')", 25);
        }
    }
}

function GetFadeDirection(elementID) {
    if ($(elementID).fadeDirection) {
        return $(elementID).fadeDirection;
    }
    return 0;
}

function GetOpacity(elementID) {
    if ($(elementID).opacity) {
        return $(elementID).opacity;
    }
    return 0;
}

function SetOpacity(elementID, opacity) {
    if ($(elementID)) {
        if (opacity > 0 && $(elementID).style.display != "block") {
            $(elementID).style.display = "block";
        }
        $(elementID).opacity = opacity;
        $(elementID).style.opacity = opacity;
        $(elementID).style.MozOpacity = opacity;
        $(elementID).style.filter = 'alpha(opacity=' + opacity * 100 + ')'; // IE
        if (opacity == 0) {
            $(elementID).style.display = "none";
        }
    }
}

function FadeOutAllRegesteredMenuButtons(target) {
    if (target && target.id != "") {
        for (var i = 0; i < RegisteredMenuButtons.length; i++) {
            var hideCurrent = true;
            if (ControlPrefix(target.id) == ControlPrefix(RegisteredMenuButtons[i])) {
                FadeIn(RegisteredMenuButtons[i]);
                if (target.id == RegisteredMenuButtons[i]) {
                    FadeIn(RegisteredMenuContainers[i]);
                }
                else {
                    if (!IsParent(RegisteredMenuContainers[i], target.id)) {
                        FadeOut(RegisteredMenuContainers[i])
                    }
                }
                hideCurrent = false;
            }
            if (hideCurrent) {
                FadeOut(RegisteredMenuButtons[i])
                FadeOut(RegisteredMenuContainers[i])
            }
        }
    }
}

function IsParent(parentID, childID) {
    var parent = $(childID);
    if (parent) {
        while (parent && parent.nodeName.toUpperCase() != "BODY") {
            if (parent.id == parentID) {
                return true;
            }
            parent = parent.parentNode;
        }
    }
    return false;
}

function ControlPrefix(controlID) {
    if (controlID) {
        if (controlID.indexOf("_") > 0) {
            return controlID.substr(0, controlID.indexOf("_"));
        }
    }
    return controlID;
}

function AttachBodyMouseOverEvent() {
    document.onmouseover = function (e) {
        if (RegisteredMenuButtons.length > 0) {
            if (!e) {
                e = window.event;
            }
            var target = (window.event) ? e.srcElement : e.target;
            if (target) {
                while (target && target.id == "" && target.nodeName.toUpperCase() != "BODY") {
                    target = target.parentNode;
                }
            }

            FadeOutAllRegesteredMenuButtons(target);
            return false;
        }
    }
}

function RegisterMenuButton(menuButtonID, menuContainerID) {
    for (var i = 0; i < RegisteredMenuButtons.length; i++) {
        if (RegisteredMenuButtons[i] == menuButtonID) {
            return;
        }
    }
    var newID = RegisteredMenuButtons.length;
    RegisteredMenuButtons[newID] = menuButtonID;
    RegisteredMenuContainers[newID] = menuContainerID;
}

var RegisteredMenuButtons = new Array();
var RegisteredMenuContainers = new Array();

function SetCustomizeMenuVisibility(e, menuButtonID, menuContainerID) {
    RegisterMenuButton(menuButtonID, menuContainerID);
}

document.addEvent('domready', addPageRequestManagerEventHandlers);

addLoadEvent(function () {
    AttachBodyMouseOverEvent();
})