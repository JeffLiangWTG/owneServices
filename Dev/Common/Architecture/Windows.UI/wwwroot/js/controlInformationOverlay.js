let isOverlayVisible;
let overlayContainer;
let overlayMask;
let overlayBorder;
let overlayInfoCard;
let dotNetRef;
let hostForm;
let lightedControl;
let mouseClientX;
let mouseClientY;

export const initialize = (dotNetObjectReference) => {
	dotNetRef = dotNetObjectReference;
	isOverlayVisible = false;
	overlayContainer = document.querySelector('.form--overlay--container');
	overlayMask = document.querySelector('.form--overlay--mask');
	overlayBorder = document.querySelector('.form--overlay--border');
	overlayInfoCard = document.querySelector('.form--overlay--infocard');
	hostForm = document.querySelector('.form');

	document.addEventListener('keydown', (e) => handleKeyDown(e));
	document.addEventListener('keyup', (e) => handleKeyUp(e));
	document.addEventListener('mousemove', (e) => handleMouseMove(e));
	window.addEventListener('focus', (e) => handleFocus(e));
	overlayMask.addEventListener('mousedown', (e) => handleMaskMouseDown(e));
}

export const highlightTargetControl = (winzorControlId) => {
	lightedControl = document.querySelector(`[data-winzor-control-id="${winzorControlId}"]`);
	updateOverlay();
	showOverlay();
}

const handleKeyDown = (e) => {
	if (!isOverlayVisible &&
		e.ctrlKey && e.shiftKey && e.key.toLowerCase() === 'f' &&
		mouseClientX && mouseClientY
	) {
		updateHighlightControl();
		updateOverlay();
		showOverlay();
	}
}

const handleKeyUp = (e) => {
	hideOverlay();
}

const handleMouseMove = (e) => {
	mouseClientX = e.clientX;
	mouseClientY = e.clientY;

	if (!isOverlayVisible || !document.hasFocus()) {
		return;
	}

	updateHighlightControl();
	updateOverlay();
}

const handleFocus = (e) => {
	hideOverlay();
}

const handleMaskMouseDown = (e) => {
	if (e.button === 0 && lightedControl) {
		dotNetRef.invokeMethodAsync('OnMouseDownAsync', lightedControl.getAttribute('data-winzor-control-id'));
	}
}

const updateHighlightControl = () => {
	const element = FindTheNearestElementInForm() || hostForm;
	const winzorControl = findWinzorControl(element);

	if (lightedControl === winzorControl) {
		return;
	}

	lightedControl = winzorControl;
}

const showOverlay = () => {
	isOverlayVisible = true;
	overlayContainer.style.display = 'block';
}

const hideOverlay = () => {
	isOverlayVisible = false;
	overlayContainer.style.display = 'none';
}

const updateOverlay = () => {
	if (!lightedControl) {
		return;
	}

	let rect = lightedControl.getBoundingClientRect();
	setControlStyle(overlayMask, rect);

	const parentElement = (lightedControl === hostForm || !lightedControl.parentElement) ?
		hostForm : findWinzorControl(lightedControl.parentElement);
	rect = parentElement.getBoundingClientRect();
	setControlStyle(overlayBorder, rect);

	overlayInfoCard.innerHTML = createInfoCardContent(lightedControl);
}

const FindTheNearestElementInForm = () => {
	const elements = document.elementsFromPoint(mouseClientX, mouseClientY);
	for (let i = 0; i < elements.length; i++) {
		const element = elements[i];
		if (element === overlayMask || element === overlayBorder ||
			element === overlayInfoCard || element === overlayContainer) {
			continue;
		}

		const style = window.getComputedStyle(element);
		if (style.display === 'none' || style.visibility === 'hidden') {
			continue;
		}

		const rect = element.getBoundingClientRect();
		if (rect.width === 0 || rect.height === 0) {
			continue;
		}

		return element;
	}

	return null;
};

const findWinzorControl = (element) => {
	while (!element.getAttribute('data-winzor-control-id') && element.parentElement) {
		element = element.parentElement;
	}

	return element;
}

const setControlStyle = (element, rect) => {
	element.style.top = `${rect.top}px`;
	element.style.left = `${rect.left}px`;
	element.style.width = `${rect.width}px`;
	element.style.height = `${rect.height}px`;
}

const createInfoCardContent = (element) => {
	const getLayoutAttribute = (element) => {
		const data = element.getAttribute('data-layout');
		if (!data) {
			return { margin: ['0', '0', '0', '0'], padding: ['0', '0', '0', '0'] };
		}
		// data string like margin: 0 0 0 0; padding: 0 0 0 0
		const layout = data.split(';');
		const margin = layout[0].split(': ')[1].split(' ');
		const padding = layout[1].split(': ')[1].split(' ');
		return { margin, padding };
	}
	const layout = getLayoutAttribute(element);
	const rect = element.getBoundingClientRect();
	let content = `<p>Name: ${element.getAttribute('data-name')}</p>`;
	content += `<p>Type: ${element.getAttribute('data-type')}</p>`;
	content += `<p>Location: ${rect.top}, ${rect.left}</p>`;
	content += `<p>Size: ${rect.width}, ${rect.height}</p>`;
	content += `<p>Padding (NESW): ${layout.padding} </p>`;
	content += `<p>Margin (NESW): ${layout.margin} </p>`;
	content += '<br>'
	content += '<p>Click for more details...</p>';
	content += '<br>'

	return content;
}
