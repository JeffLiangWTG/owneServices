export const enableSpellCheck = (elementReference, dotNetObjectReference) => {
	const editor = getEditor(elementReference);
	if (!editor) {
		return;
	}

	if (editor.classList.contains('textbox')) {
		editor.setAttribute('spellcheck', true);
	}
	else if (editor.spellCheckEnabled === undefined) {
		editor.spellCheckTimer = null;
		editor.debounce = debounce.bind(editor);
		editor.getText = getText.bind(editor);
		editor.clearSquiggles = clearSquiggles.bind(editor);
		editor.withSelectionRestore = withSelectionRestore.bind(editor);

		editor.originalInsertContent = editor.insertContent;
		editor.insertContent = newInsertContent.bind(editor, dotNetObjectReference);

		editor.addEventListener('keyup', handleKeyUp.bind(editor, dotNetObjectReference));
		editor.addEventListener('paste', handlePaste.bind(editor, dotNetObjectReference));

		editor.spellCheckEnabled = true;
		checkTextSpellingAsync.call(editor, dotNetObjectReference);
	}
	else if (!editor.spellCheckEnabled) {
		editor.spellCheckEnabled = true;
	}
}

export const disableSpellCheck = (elementReference) => {
	const editor = getEditor(elementReference);
	if (!editor) {
		return;
	}

	if (editor.classList.contains('textbox')) {
		editor.setAttribute('spellcheck', false);
	}
	else if (editor.spellCheckEnabled) {
		editor.spellCheckEnabled = false;
	}
}

function getEditor(elementReference) {
	if (!elementReference) {
		return;
	}

	if (elementReference.classList.contains('textbox')) {
		return elementReference;
	}
	else if (elementReference.classList.contains('richtextbox')) {
		return elementReference.querySelector('.richtextbox__editoranchor');
	}

	return null;
}

function getText() {
	let text = '';
	const paragraphs = this.querySelectorAll('p');
	for (const p of paragraphs) {
		text += p.innerText;
		if (!p.innerText.endsWith('\n')) {
			text += '\n';
		}
	}
	return text;
}

// refer to WTG.SpellCheck/SpellChecker/Ispell/SpellingSingleWordMatcher.cs
const validEngChars = 'a-zàáèéìíòóùúâêîôûäëïöüāēīōūăĕğĭŏŭçñ';
const invalidCharRegex = new RegExp(`[^${validEngChars}0-9]`, 'i');

function handleKeyUp(dotNetObjectReference, e) {
	if (!this.spellCheckEnabled || !isKeyValid(e)) {
		return;
	}

	// to reset editor html when it is empty
	// there is an issue with the editor that it does not remove the <p> tag when it is empty
	if (this.innerText === '') {
		this.innerHTML = '';
	}

	this.debounce(checkTextSpellingAsync.bind(this, dotNetObjectReference), 500);
}

function handlePaste(dotNetObjectReference) {
	if (!this.spellCheckEnabled) {
		return;
	}

	this.debounce(checkTextSpellingAsync.bind(this, dotNetObjectReference), 500);
}

async function checkTextSpellingAsync(dotNetObjectReference) {
	let text = this.getText();
	if (text.endsWith('\n') && !text.endsWith('\n\n')) {
		text = text.slice(0, -1);
	}

	// we do not want to underline the last word if it is NOT followed by an invalidChar
	let lastIndex = -1;
	for (let i = text.length - 1; i >= 0; i--) {
		if (invalidCharRegex.test(text[i])) {
			lastIndex = i;
			break;
		}
	}

	text = lastIndex >= 0 ? text.slice(0, lastIndex) : '';
	if (text === '') {
		this.clearSquiggles();
		return;
	}

	const ranges = await dotNetObjectReference.invokeMethodAsync('CheckTextSpelling', text);
	this.clearSquiggles();
	this.withSelectionRestore(() => ranges.map(this.getDOMRangeFor).forEach(paintSquiggle));
}

function newInsertContent(dotNetObjectReference, html) {
	this.originalInsertContent(html);
	handlePaste.call(this, dotNetObjectReference);
}

function isKeyValid(e) {
	const additionalKeys = ['Enter', 'Tab', 'Delete', 'Backspace'];
	if (e.key.length > 1 && !additionalKeys.includes(e.key)) {
		return false;
	}
	else if (e.ctrlKey && ['c', 'v'].includes(e.key.toLowerCase())) {
		return false;
	}
	return true;
}

function clearSquiggles() {
	const squiggles = this.querySelectorAll('span[data-squiggle="true"]');
	if (squiggles.length > 0) {
		this.withSelectionRestore(() => squiggles.forEach(removeSquiggle));
	}
}

function paintSquiggle(range) {
	const fragment = range.extractContents();
	const squiggle = document.createElement('span');
	squiggle.dataset.squiggle = true;
	squiggle.appendChild(fragment);
	range.insertNode(squiggle);
}

function removeSquiggle(squiggle) {
	const parent = squiggle.parentNode;
	while (squiggle.firstChild) {
		parent.insertBefore(squiggle.firstChild, squiggle);
	}
	parent.removeChild(squiggle);
}

function withSelectionRestore(action) {
	const selection = this.getSelection();
	try {
		action();
	} catch (error) {
		console.error(error);
	}
	this.setSelection(selection);
}

function debounce(func, time) {
	const delay = time || 100;
	if (this.spellCheckTimer) clearTimeout(this.spellCheckTimer);
	this.spellCheckTimer = setTimeout(func, delay);
}
