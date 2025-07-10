export const pictureBoxEnableDraggablePositionAsync = (element) => {
	element.style['position'] = 'absolute';
	element.style['cursor'] = 'pointer';
	let prevLeft = null,
		prevTop = null;

	// this flag is true when the user is dragging the mouse
	let isDown = false;

	const moveMouse = (e) => {
		if (isDown) {
			element.style['cursor'] = 'grabbing';
			movePictureBox(e);
		}
	};
	const addEventListeners = (e) => {
		e.srcElement.addEventListener('mousemove', moveMouse);
		e.srcElement.addEventListener('mouseup', stopClickAndDrag);
		e.srcElement.addEventListener('mouseout', stopClickAndDrag);
	};
	const beginClickAndDrag = (e) => {
		isDown = true;
		addEventListeners(e);
	};
	const stopClickAndDrag = () => {
		isDown = false;
		prevLeft = null;
		prevTop = null;
		element.style['cursor'] = 'pointer';
	};
	const movePictureBox = (e) => {
		const left = e.clientX;
		const top = e.clientY;
		if (prevLeft !== null && prevTop !== null && (left !== prevLeft || top !== prevTop)) {
			pictureBoxMoveBy(left - prevLeft, top - prevTop, element);
		}
		prevLeft = left;
		prevTop = top;
	};

	element.addEventListener('mousedown', beginClickAndDrag);
};

export const pictureBoxEnableMovementByToolbarButtons = (element, toolbar, distance) => {
	let buttons = toolbar.querySelectorAll('.toolbar__button');
	let left = buttons[0];
	let up = buttons[1];
	let down = buttons[2];
	let right = buttons[3];

	left.addEventListener('click', function () {
		pictureBoxMoveBy(distance, 0, element);
	});
	up.addEventListener('click', function () {
		pictureBoxMoveBy(0, distance, element);
	});
	down.addEventListener('click', function () {
		pictureBoxMoveBy(0, -distance, element);
	});
	right.addEventListener('click', function () {
		pictureBoxMoveBy(-distance, 0, element);
	});
};

export const pictureBoxEnableZoomByDropEdit = (element, dropEdit) => {
	let input = dropEdit.querySelector('input');

	const processZoom = () => {
		let zoom = parseInt(input.value);
		if (!isNaN(zoom)) {
			if (zoom == 0) {
				pictureBoxFillToWidth(element);
			} else {
				pictureBoxScale(element, zoom);
				pictureBoxMoveBy(0, 0, element);
			}
		}
	};
	input.addEventListener('change', processZoom);
	input.addEventListener('select', processZoom);
};

export const pictureBoxFillToWidth = (element) => {
	if (element.style['width'] != '100%') {
		element.parentElement.style['width'] = '100%';
		element.parentElement.style['height'] = null;
		element.style['width'] = '100%';
		element.style['height'] = null;
		element.style['top'] = null;
		element.style['left'] = null;

		let img = element.querySelector('img');
		img.style['width'] = '100%';
		img.style['height'] = null;
	}
};

const pictureBoxMoveBy = (x, y, pictureBox) => {
	let fitToWidth = pictureBox.querySelector('img').style['width'] == '100%';
	let parentHeight = parseInt(pictureBox.parentElement.parentElement.style['height']);
	let pictureBoxHeight = parseInt(pictureBox.style['height']);

	if ((!isNaN(parentHeight) && !isNaN(pictureBoxHeight) && pictureBoxHeight > parentHeight) || fitToWidth) {
		let top = y + (parseInt(pictureBox.style['top']) || 0);

		//Minimum of zero to prevent gap between top of picture and container.
		pictureBox.style['top'] = Math.min(0, top) + 'px';

		//Prevent gap between bottom of picture and container.
		let bottomDiff =
			pictureBox.parentElement.parentElement.getBoundingClientRect().bottom -
			pictureBox.getBoundingClientRect().bottom;
		if (bottomDiff > 0) {
			pictureBox.style['top'] = Math.min(0, top + bottomDiff) + 'px';
		}
	} else {
		pictureBox.style['top'] = '0px';
	}

	if (!fitToWidth) {
		let parentWidth = parseInt(pictureBox.parentElement.parentElement.style['width']);
		let pictureBoxWidth = parseInt(pictureBox.style['width']);
		if (!isNaN(parentWidth) && !isNaN(pictureBoxWidth) && pictureBoxWidth > parentWidth) {
			let left = x + (parseInt(pictureBox.style['left']) || 0);

			//Minimum of zero to prevent gap between left of picture and container.
			pictureBox.style['left'] = Math.min(0, left) + 'px';

			//Prevent gap between right of picture and container.
			let rightDiff =
				pictureBox.parentElement.parentElement.getBoundingClientRect().right -
				pictureBox.getBoundingClientRect().right;
			if (rightDiff > 0) {
				pictureBox.style['left'] = Math.min(0, left + rightDiff) + 'px';
			}
		} else {
			pictureBox.style['left'] = '0px';
		}
	}
};

export const pictureBoxScale = (pictureBox, percent) => {
	let img = pictureBox.querySelector('img');
	let width = (img.naturalWidth / 100) * percent;
	let height = (img.naturalHeight / 100) * percent;

	pictureBox.style['width'] = width + 'px';
	pictureBox.style['height'] = height + 'px';
	img.style['width'] = null;
	img.style['height'] = null;
};
