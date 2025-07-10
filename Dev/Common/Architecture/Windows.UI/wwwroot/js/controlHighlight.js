const translationFeedbackTargetSelectors = [
	'.radiobutton', // good
	'.linklabel', // good
	'.listbox .listbox__item', // click no response, same to RDP
	'.statusbar', // click no response, disappear on click
	'.button', // good
	'.label', // good
	'.treeview .treeview__nodetext', // click no reponse
	'.treeviewadv .treeviewadv__columnheader', // click no response
	'.checkbox', // click no response
	'.balloon__caption, .balloon__description', // disappear on hover
	'.tabcontrol .tabcontrol__button', // click no response
	'.zdropedit .textbox', // good
	'.datagrid th:not(:first-of-type)', // no matching resource string found
	'.groupbox .groupbox__text', // click no response
	'.zmessagebox .textbox', // good
	'.zdropform .zdropform__item', // good for selectable item, click no response for unselectable items
	'.toolstrip .toolstrip-item', // click no response
].join(',');

let lastHighlightedElement = null;

export const initialize = (enableTranslationFeedbackHighlight) => {
	if (!enableTranslationFeedbackHighlight) {
		return;
	}
	document.addEventListener('keydown', handleKeyDown);
	document.addEventListener('keyup', handleKeyUp);
}

function handleKeyDown(e) {
	if (e.key === 'F2' && !e.repeat) {
		turnOnHighlight();
	}
}

function handleKeyUp(e) {
	if (e.key === 'F2') {
		turnOffHighlight();
	}
}

function turnOnHighlight() {
	document.querySelectorAll(translationFeedbackTargetSelectors).forEach(element => {
		element.addEventListener('mouseenter', handleMouseEnterOrMove);
		element.addEventListener('mousemove', handleMouseEnterOrMove);
		element.addEventListener('mouseleave', handleMouseLeave);
		element.addEventListener('click', handleClick);
	});
}

function turnOffHighlight() {
	if (lastHighlightedElement) {
		clearHighlight(lastHighlightedElement);
	}
	document.querySelectorAll(translationFeedbackTargetSelectors).forEach(element => {
		element.removeEventListener('mouseenter', handleMouseEnterOrMove);
		element.removeEventListener('mousemove', handleMouseEnterOrMove);
		element.removeEventListener('mouseleave', handleMouseLeave);
		element.removeEventListener('click', handleClick);
	});
}

function handleMouseEnterOrMove() {
	applyHighlight(this);
}

function handleMouseLeave() {
	clearHighlight(this);
}

function handleClick() {
	turnOffHighlight();
}

function clearHighlight(element) {
	element.classList?.remove('feedback-highlighted');
}

function applyHighlight(element) {
	element.classList.add('feedback-highlighted');
	lastHighlightedElement = element;
}
