export const initialize = (elementReference, dotNetObjectReference, magicChar, selectedTags) => {
	const editor = elementReference.querySelector('.richtextbox__editoranchor');
	if (editor) {
		editor.updateTags = updateTags.bind(editor);
		editor.removeTags = removeTags.bind(editor);
		editor.colorTags = colorTags.bind(editor);
		editor.updateCurrentWord = updateCurrentWord.bind(editor, dotNetObjectReference);
		editor.removeCurrentWord = removeCurrentWord.bind(editor);
		editor.isRecordingCurrentWord = isRecordingCurrentWord.bind(editor);

		editor.magicChar = magicChar;
		editor.tags = [];
		editor.distinctTags = [];
		editor.selectedTags = selectedTags;
		editor.updateTags();

		editor.addEventListener('keydown', handleKeyDown.bind(editor));
		editor.addEventListener('mousedown', handleMouseDown.bind(editor));

		editor.autoCompleteInitialized = true;
	}
}

export const insertTagAndUpdateTags = (elementReference, tag, selectedTags) => {
	const editor = elementReference.querySelector('.richtextbox__editoranchor');
	if (editor && editor.autoCompleteInitialized) {
		// remove incomplete word before inserting the selected tag
		editor.removeCurrentWord();
		editor.insertContent(tag);
		editor.selectedTags = selectedTags;
		editor.updateTags();
	}
};

function colorTags() {
	const BLACK = '#000000';
	const BLUE = '#0000ff';
	const selection = this.getSelection();
	this.setSelection({ start: 0, end: this.innerText.length });
	this.setForeColor(BLACK);

	for (const tag of this.distinctTags) {
		let startIndex = this.innerText.indexOf(this.magicChar + tag);
		while (startIndex > -1) {
			const endIndex = startIndex + tag.length + 1;
			this.setSelection({ start: startIndex, end: endIndex });
			this.setForeColor(BLUE);
			startIndex = this.innerText.indexOf(this.magicChar + tag, endIndex);
		}
	}

	this.setSelection(selection);
}

function updateTags() {
	let finalTags = [];
	let text = this.innerText;
	const orderedTags = this.selectedTags.toSorted((a, b) => b.length - a.length);
	for (const tag of orderedTags) {
		const startIndex = text.indexOf(this.magicChar + tag);
		if (startIndex > -1) {
			const endIndex = startIndex + tag.length + 1;
			text = text.slice(0, startIndex) + text.slice(endIndex);
			finalTags.push(tag);
		}
	}
	this.tags = finalTags;
	this.distinctTags = [...new Set(finalTags)];
	this.colorTags();
}

function handleKeyDown(e) {
	switch (e.key) {
		case this.magicChar: {
			this.updateCurrentWord('');
			registerEditorScrollHandler.call(this);
			registerDropFormMouseDownHandler.call(this);
			break;
		}
		case 'ArrowUp':
		case 'ArrowDown':
		case 'PageUp':
		case 'PageDown': {
			if (this.isRecordingCurrentWord()) {
				e.preventDefault();
				e.stopPropagation();
			}
			break;
		}
		case 'Tab':
		case 'Enter': {
			if (this.isRecordingCurrentWord()) {
				e.preventDefault();
				e.stopPropagation();
				this.previousElementSibling.dataset.acceptsTab = 'true';
			}
			else {
				this.colorTags();
			}
			break;
		}
		case 'Home':
		case 'End':
		case 'Escape':
		case 'ArrowLeft':
		case 'ArrowRight': {
			if (this.isRecordingCurrentWord()) {
				this.updateCurrentWord(null);
			}
			break;
		}
		case 'Delete':
		case 'Backspace': {
			const isDelete = e.key === 'Delete';
			if (this.isRecordingCurrentWord()) {
				const value = (isDelete || this.currentWord === '') ? null : this.currentWord.slice(0, -1);
				this.updateCurrentWord(value);
			}
			else if (this.removeTags(isDelete)) {
				e.preventDefault();
				e.stopPropagation();
			}
			break;
		}
		case ' ': {
			if (this.isRecordingCurrentWord()) {
				this.updateCurrentWord(null);
			}
			else {
				this.colorTags();
			}
			break;
		}
		default: {
			if (this.isRecordingCurrentWord() && isKeyValidCharacter(e)) {
				const value = this.currentWord + e.key;
				this.updateCurrentWord(value);
			}
			else if (isKeyValidCharacter(e)) {
				this.colorTags();
			}
			break;
		}
	}
}

function handleMouseDown() {
	if (this.isRecordingCurrentWord()) {
		this.updateCurrentWord(null);
	}
}

function updateCurrentWord(dotNetObjectReference, value) {
	this.currentWord = value;
	this.previousElementSibling.dataset.acceptsTab = this.isRecordingCurrentWord() ? 'false' : 'true';
	dotNetObjectReference.invokeMethodAsync('UpdateCurrentWordAsync', this.currentWord);
}

function isRecordingCurrentWord() {
	return !isNullOrUndefined(this.currentWord);
}

function removeCurrentWord() {
	if (isNullOrUndefined(this.currentWord) || this.currentWord === '') {
		this.currentWord = null;
		return;
	}
	const range = window.getSelection().getRangeAt(0);
	const removeStart = range.endOffset - this.currentWord.length;

	range.setStart(range.endContainer, removeStart);
	range.deleteContents();

	this.currentWord = null;
}

function removeTags(isDelete) {
	const selection = window.getSelection();
	const range = selection.getRangeAt(0);
	const text = range.commonAncestorContainer.textContent;

	// no need to explicitly remove tags when user deletes all text
	if (range.startOffset === 0 && range.endOffset === this.innerText.length) {
		return false;
	}

	let index = text.indexOf(this.magicChar);
	if (index === -1) {
		return false;
	}

	const removed = range.collapsed
		? removeTag(this.distinctTags, this.magicChar, range, isDelete)
		: removeTagsHighlighted(this.distinctTags, this.magicChar, range);

	this.updateTags();

	return removed;
}

function removeTag(distinctTags, magicChar, range, isDelete) {
	for (const tag of distinctTags) {
		const index = range.startContainer.textContent.indexOf(magicChar + tag);
		if (index === -1) {
			continue;
		}
		const offset = range.startOffset;
		const endIndex = index + tag.length + 1;
		if (index < offset && offset < endIndex || (offset === index && isDelete) || (offset === endIndex && !isDelete)) {
			range.setStart(range.startContainer, index);
			range.setEnd(range.startContainer, endIndex);
			range.deleteContents();
			return true;
		}
	}
	return false;
}

function removeTagsHighlighted(distinctTags, magicChar, range) {
	let removeStart = range.startOffset;
	let removeEnd = range.endOffset;

	// extend range in startContainer
	for (const tag of distinctTags) {
		const index = range.startContainer.textContent.indexOf(magicChar + tag);
		if (index > -1) {
			const endIndex = index + tag.length + 1;
			if (index <= range.startOffset && range.startOffset < endIndex) {
				removeStart = Math.min(removeStart, index);
			}
		}
	}
	// extend range in endContainer
	for (const tag of distinctTags) {
		const index = range.endContainer.textContent.indexOf(magicChar + tag);
		if (index > -1) {
			const endIndex = index + tag.length + 1;
			if (index < range.endOffset && range.endOffset <= endIndex) {
				removeEnd = Math.max(removeEnd, endIndex);
			}
		}
	}

	if (removeStart < range.startOffset || removeEnd > range.endOffset) {
		range.setStart(range.startContainer, removeStart);
		range.setEnd(range.endContainer, removeEnd);
		range.deleteContents();
		return true;
	}
	return false;
}

function registerEditorScrollHandler() {
	const handleScroll = () => {
		this.removeEventListener('scroll', handleScroll);
		this.updateCurrentWord(null);
	}
	this.removeEventListener('scroll', handleScroll);
	this.addEventListener('scroll', handleScroll);
}

function registerDropFormMouseDownHandler() {
	let intervalId, timeoutId;
	intervalId = setInterval(() => {
		const dropForm = document.querySelector('.zdropform');
		if (dropForm) {
			clearInterval(intervalId);
			clearTimeout(timeoutId);

			const handleDropFormMouseDown = (e) => {
				if (this.isRecordingCurrentWord()) {
					this.previousElementSibling.dataset.acceptsTab = 'true';
				}
				dropForm.removeEventListener('mousedown', handleDropFormMouseDown);
			}
			dropForm.removeEventListener('mousedown', handleDropFormMouseDown);
			dropForm.addEventListener('mousedown', handleDropFormMouseDown);
		}
	}, 10)
	// wait up to 1 second for .zdropform to show up
	timeoutId = setTimeout(() => {
		clearInterval(intervalId);
		clearTimeout(timeoutId);
	}, 1000);
}

function isNullOrUndefined(a) {
	return a === null || a === undefined;
}

function isKeyValidCharacter(e) {
	return e.key.length === 1 && !e.ctrlKey && !e.altKey && !e.metaKey && !e.repeat;
}
