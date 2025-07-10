const MinColumnWidth = 3;
const itemDragSensitivity = 4;

export const changeColumnWidth = (dotnet, args, column) => {
	const tree = column.parentElement.parentElement;
	const columnHeaders = tree.querySelectorAll('.treeviewadv__columnheader');
	const rows = tree.querySelectorAll('.treeviewadv__rowcontent');

	const offsetStart = column.offsetWidth - getPageX(args);
	const columnIndex = parseInt(column.getAttribute('columnindex'));
	const currentHeaderIndex = getHeaderIndexByColumnIndex(columnHeaders, columnIndex);

	const onMouseMove = (e) => {
		const oldWidth = column.offsetWidth;
		const newWidth = getNewWidth(columnHeaders[currentHeaderIndex], e, offsetStart);
		column.style.width = `${newWidth}px`;

		for (let i = 0; i < rows.length; i++) {
			const items = rows[i].querySelectorAll('.treeviewadv__item');

			// Get items which need to update width
			let cellItems = null;
			if (columnIndex == 0) {
				cellItems = rows[i].querySelectorAll('.treeviewadv__plusminus, div[columnindex="0"]');
			} else {
				cellItems = rows[i].querySelectorAll(`div[columnindex="${columnIndex}"]`);
			}

			// Update all items in the current column
			let tempWidth = columnIndex == 0 ? cellItems[0].offsetLeft : 0;
			let findCurrentItem = false;

			for (let j = 0; j < cellItems.length; j++) {
				if (findCurrentItem) {
					cellItems[j].style.width = '0px';
					continue;
				}

				if (j < cellItems.length - 1) {
					let itemMaxWidth = cellItems[j + 1].offsetLeft - cellItems[j].offsetLeft;
					tempWidth += itemMaxWidth;

					if (tempWidth < newWidth) {
						cellItems[j].style.width = `${itemMaxWidth}px`;
					} else {
						cellItems[j].style.width = `${itemMaxWidth - tempWidth + newWidth}px`;
						findCurrentItem = true;
					}
				} else {
					cellItems[j].style.width = `${newWidth - tempWidth}px`;
				}
			}

			// Change item left, start from the next column
			for (let item of items) {
				if (item.getAttribute('columnindex') > columnIndex) {
					item.style.left = `${item.offsetLeft + newWidth - oldWidth}px`;
				}
			}
		}

		// Update left for headers, start from the next column
		for (let i = currentHeaderIndex + 1; i < columnHeaders.length; i++) {
			columnHeaders[i].style.left = `${columnHeaders[i].offsetLeft + newWidth - oldWidth}px`;
		}
	};

	const onMouseUp = (e) => {
		e.stopPropagation();
		document.removeEventListener('mousemove', onMouseMove);
		document.removeEventListener('mouseup', onMouseUp);
		const newWidth = getNewWidth(columnHeaders[currentHeaderIndex], e, offsetStart);
		dotnet.invokeMethodAsync('SetColumnWidthAsync', columnIndex, newWidth);
	};

	document.addEventListener('mousemove', onMouseMove);
	document.addEventListener('mouseup', onMouseUp);
};

const getHeaderIndexByColumnIndex = (items, columnIndex) => {
	for (let i = 0; i < items.length; i++) {
		if (items[i].getAttribute('columnindex') == columnIndex) {
			return i;
		}
	}
	return -1;
};

const getNewWidth = (columnHeader, e, startOffset) => {
	const minWidth = parseInt(columnHeader.style.minWidth);
	const maxWidth = parseInt(columnHeader.style.maxWidth);
	let newWidth = startOffset + getPageX(e);

	if (minWidth >= 0) {
		newWidth = Math.max(newWidth, minWidth);
	}
	if (maxWidth > 0) {
		newWidth = Math.min(newWidth, maxWidth);
	}
	return Math.max(newWidth, MinColumnWidth);
};

const getPageX = (e) => {
	return Math.floor(e.pageX);
};

export const reorderColumnHeader = (dotnet, args, column) => {
	const columnHeaders = column.parentElement;
	const tree = columnHeaders.parentElement;
	const columns = columnHeaders.getElementsByTagName('div');
	const headerLength = columns.length;
	const columnIndex = column.getAttribute('columnindex');
	let ghostImage;

	const onMouseMove = (e) => {
		if (e.buttons !== 1) {
			removeReorderHoverStyle();
			return;
		}
		if (Math.abs(e.pageX - args.pageX) > itemDragSensitivity) {
			if (!ghostImage) {
				ghostImage = column.cloneNode(true);
				ghostImage.setAttribute('class', 'treeviewadv__columnheader treeviewadv__columnheader-ghostimage');
				ghostImage.setAttribute(
					'style',
					`width:${column.offsetWidth}px;height:${column.offsetHeight}px;left:${column.offsetLeft}px;`
				);

				columnHeaders.appendChild(ghostImage);
				columnHeaders.classList.add('treeviewadv__columnheaders--no-hover');
			}
		}

		if (ghostImage) {
			const currentX = e.pageX - args.pageX + column.offsetLeft;
			const ghostImageMaxX = Math.max(columnHeaders.offsetWidth, tree.clientWidth);

			if (currentX + column.offsetWidth >= ghostImageMaxX) {
				ghostImage.style.width = `${Math.max(ghostImageMaxX - currentX, 0)}px`;
				ghostImage.querySelector('p').style.textOverflow = 'clip';

				if (ghostImageMaxX - currentX <= 0) {
					ghostImage.style.display = 'none';
				} else {
					ghostImage.style.display = '';
				}
			} else {
				ghostImage.style.width = `${column.offsetWidth}px`;
			}

			ghostImage.style.left = `${Math.min(currentX, ghostImageMaxX - ghostImage.offsetWidth)}px`;
			findTargetX(e);
		}
	};

	const onMouseUp = (e) => {
		if (ghostImage) {
			ghostImage.remove();
			columnHeaders.classList.remove('treeviewadv__columnheaders--no-hover');
			ghostImage = null;

			if (columnIndex != null && columnIndex != '') {
				dotnet.invokeMethodAsync('ReorderColumnAsync', e, parseInt(columnIndex), parseInt(findTargetX(e)));
			}
		}

		removeReorderHoverStyle();
		document.removeEventListener('mousemove', onMouseMove);
		document.removeEventListener('mouseup', onMouseUp);
	};

	const removeReorderHoverStyle = () => {
		for (let i = 0; i < headerLength; i++) {
			columns[i].classList.remove('treeviewadv__columnheader--border-left');
			columns[i].classList.remove('treeviewadv__columnheader--border-right');
		}
	};

	const findTargetX = (e) => {
		removeReorderHoverStyle();
		for (let i = 0; i < headerLength; i++) {
			const columnBound = columns[i].getBoundingClientRect();
			const columnMiddleX = columnBound.left + columnBound.width / 2;

			if (e.pageX < columnMiddleX) {
				columns[i].classList.add('treeviewadv__columnheader--border-left');
				return columns[i].offsetLeft;
			} else if (i == headerLength - 1) {
				columns[i].classList.add('treeviewadv__columnheader--border-right');
				return columns[i].offsetLeft + columns[i].offsetWidth;
			}
		}
	};

	document.addEventListener('mousemove', onMouseMove);
	document.addEventListener('mouseup', onMouseUp);
};

export const dragDropRows = (dotnet, element, topEdgeSensitivity, bottomEdgeSensitivity) => {
	const tree = element.parentElement;
	const getRows = () => element.querySelectorAll('.treeviewadv__rowcontent');

	const getNodePosition = (targetRow, offsetY) => {
		const positionFlags = targetRow.getAttribute('data-row-cursors').split(',', 3);
		const rowHeight = targetRow.getBoundingClientRect().height;
		offsetY = Math.min(Math.max(0, offsetY), rowHeight);

		if (positionFlags[1] == 'move' && offsetY >= 0 && offsetY < topEdgeSensitivity * rowHeight) {
			return NodePosition.BEFORE;
		} else if (
			positionFlags[2] == 'move' &&
			offsetY > rowHeight * (1 - bottomEdgeSensitivity) &&
			offsetY <= rowHeight
		) {
			return NodePosition.AFTER;
		} else if (positionFlags[0] == 'move') {
			return NodePosition.INSIDE;
		}

		return NodePosition.NONE;
	};

	let dropMark;
	const addDropMark = (targetRow, nodePosition) => {
		dropMark = document.createElement('div');
		const offsetLeft = targetRow.querySelector('.treeviewadv__item').offsetLeft;
		const treeScrollOffset = tree.scrollWidth - tree.clientWidth - tree.scrollLeft;

		dropMark.style.left = `${offsetLeft}px`;
		dropMark.style.width = `${Math.max(tree.clientWidth, targetRow.offsetWidth - treeScrollOffset) - offsetLeft}px`;
		dropMark.style.zIndex = 999;
		dropMark.classList.add('treeviewadv__dropmark');

		if (nodePosition === NodePosition.AFTER) {
			dropMark.style.top = `${targetRow.offsetHeight}px`;
		}
		targetRow.appendChild(dropMark);
	};

	const removeDropMark = () => {
		if (dropMark) {
			dropMark.remove();
			dropMark = null;
		}
	};

	const handleDrop = (e) => {
		if (!e.target.classList.contains('treeviewadv__item')) {
			return;
		}

		const nodePosition = getNodePosition(e.target.parentElement, e.offsetY);
		if (nodePosition !== NodePosition.NONE) {
			const targetRect = e.target.getBoundingClientRect();
			const treeRect = tree.getBoundingClientRect();

			if (nodePosition === NodePosition.BEFORE) {
				dotnet.invokeMethodAsync(
					'DragDropAsync',
					0,
					parseInt(targetRect.x - treeRect.x + tree.scrollLeft),
					parseInt(targetRect.y - treeRect.y + 1)
				);
			} else if (nodePosition === NodePosition.AFTER) {
				dotnet.invokeMethodAsync(
					'DragDropAsync',
					0,
					parseInt(targetRect.x - treeRect.x + tree.scrollLeft),
					parseInt(targetRect.y - treeRect.y + targetRect.height - 1)
				);
			} else if (nodePosition === NodePosition.INSIDE) {
				dotnet.invokeMethodAsync(
					'DragDropAsync',
					0,
					parseInt(targetRect.x - treeRect.x + tree.scrollLeft),
					parseInt(targetRect.y - treeRect.y + targetRect.height / 2)
				);
			}
		}
	};

	const removeDragClasses = () =>
		getRows().forEach((row) => {
			row.classList.remove('treeviewadv__rowcontent--ondrag');
			row.classList.remove('treeviewadv__rowcontent--ondragover');
		});

	let savedRow;
	const handleDragOver = (e) => {
		e.preventDefault();
		e.stopPropagation();
		if (e.dataTransfer.effectAllowed != 'move') {
			return;
		}

		if (e.target.classList.contains('treeviewadv__item')) {
			const targetRow = e.target.parentElement;
			const nodePosition = getNodePosition(targetRow, e.offsetY);
			targetRow.classList.add('treeviewadv__rowcontent--ondragover');

			if (nodePosition === NodePosition.BEFORE || nodePosition === NodePosition.AFTER) {
				if (targetRow !== savedRow) {
					removeDropMark();
				}
				if (!dropMark) {
					addDropMark(targetRow, nodePosition);
				}
			} else {
				removeDropMark();
			}
			savedRow = targetRow;

			if (nodePosition !== NodePosition.NONE) {
				e.dataTransfer.dropEffect = 'move';
				return;
			}
		}
		removeDropMark();
		e.dataTransfer.dropEffect = 'none';
	};

	const handleDragLeave = (e) => {
		const targetRow = e.target.parentElement;
		if (targetRow && targetRow.classList.contains('treeviewadv__rowcontent')) {
			targetRow.classList.remove('treeviewadv__rowcontent--ondragover');
		}
	};

	const handleDragEnter = (e) => {
		e.preventDefault();
	};

	const handleDragEnd = () => {
		removeDragClasses();
		removeDropMark();

		document.removeEventListener('dragover', handleDragOver);
		document.removeEventListener('dragenter', handleDragEnter);
		document.removeEventListener('dragleave', handleDragLeave);
		document.removeEventListener('dragend', handleDragEnd);
		document.removeEventListener('drop', handleDrop);
	};

	document.addEventListener('dragover', handleDragOver);
	document.addEventListener('dragenter', handleDragEnter);
	document.addEventListener('dragleave', handleDragLeave);
	document.addEventListener('dragend', handleDragEnd);
	document.addEventListener('drop', handleDrop);
};

const NodePosition = {
	BEFORE: 'before',
	AFTER: 'after',
	INSIDE: 'inside',
	NONE: 'none',
};
