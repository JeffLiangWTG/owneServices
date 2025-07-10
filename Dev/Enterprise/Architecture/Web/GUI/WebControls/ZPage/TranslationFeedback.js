
var translationFeedbackMouseEnter;
var translationFeedbackMouseLeave;
var translationFeedbackClick;
var lastHighlighted;
var highlightColor = '#20B2AA';

function TranslationFeedbackOn() {
    if (!translationFeedbackMouseEnter) {
        $$('*').addEvents({
            mouseenter: translationFeedbackMouseEnter = function () {
                if (this.tagName.toUpperCase() === 'SELECT' ||
                    (this.tagName.toUpperCase() === 'INPUT' && (this.type.toUpperCase() === 'SUBMIT' || this.type.toUpperCase() === 'BUTTON')) ||
                    (this.children.length == 0 && this.innerText)) {
                    this.style.backgroundColor = highlightColor;
                    this.style.backgroundImage = 'none';
                    this.style.cursor = 'crosshair';
                    lastHighlighted = this;
                }
            },

            mouseleave: translationFeedbackMouseLeave = function () {
                TranslationFeedbackClearHighlight(this);
            },

            click: translationFeedbackClick = function () {
                if (NormalizeColor(this.style.backgroundColor) === highlightColor) {
                    var caption;
                    if (this.tagName.toUpperCase() === 'SELECT') {
                        caption = this.options[this.selectedIndex].innerText;
                    }
                    else if (this.tagName.toUpperCase() === 'INPUT' || this.tagName.toUpperCase() === 'SUBMIT') {
                        caption = this.value;
                    }
                    else {
                        caption = this.innerText;
                    }
                    TranslationFeedbackOff();
                    var url = 'edient:LicenceCode=' + TranslationFeedbackConfiguration.LicenceCode +
                                '&Command=WebTranslationFeedback' +
                                '&l=' + TranslationFeedbackConfiguration.Language +
                                '&c=' + encodeURIComponent(caption) +
                                '&u=' + encodeURIComponent(TranslationFeedbackConfiguration.UsageCallback);
                    window.location.href = url;
                    return false;
                }
            }
        });
    }
}

function TranslationFeedbackClearHighlight(element) {
    element.style.backgroundColor = '';
    element.style.backgroundImage = '';
    element.style.cursor = '';
}

function TranslationFeedbackOff() {
    if (lastHighlighted) {
        TranslationFeedbackClearHighlight(lastHighlighted);
    }
    $$('*').removeEvent('mouseenter', translationFeedbackMouseEnter);
    $$('*').removeEvent('mouseleave', translationFeedbackMouseLeave);
    $$('*').removeEvent('click', translationFeedbackClick);
    translationFeedbackMouseEnter = null;
    translationFeedbackMouseLeave = null;
}

function NormalizeColor(color) {
    if (!color) {
        color = '';
    }
    var parsed = /rgb\((\d+), (\d+), (\d+)\)/.exec(color);
    if (parsed) {
        var red = parseInt(parsed[1]);
        var green = parseInt(parsed[2]);
        var blue = parseInt(parsed[3]);
        var rgb = blue | (green << 8) | (red << 16);
        rgb = rgb.toString(16).toUpperCase();
        while (rgb.length < 6) {
            rgb = '0' + rgb;
        }
        return '#' + rgb;
    }
    else {
        return color.toUpperCase();
    }
}

$(document).addListener('keydown', function (evt) {
    if (evt.keyCode === 113) {
        TranslationFeedbackOn();
    }
});

$(document).addListener('keyup', function (evt) {
    if (evt.keyCode === 113) {
        TranslationFeedbackOff();
    }
});