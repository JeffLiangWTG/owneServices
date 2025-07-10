// Module-level variables with explicit initialization
let observer = null;
let mutationObserver = null;
let scrollInitiatedFromServer = false;
let scrollingUp = false;
let scrollingDown = false;
let lastScrollTop = 0;
let mouseDown = false;
let timeoutId = null;
let isInitializing = false;
let pendingUpdate = null;
let initializationDone = false;
const DEBOUNCE_DELAY = 500;

const queueUpdate = (dotNetReference, pageNo) => {
	if (!initializationDone || !observer) return;
	pendingUpdate = pageNo;

	debounce(async () => {
		if (pendingUpdate !== null && observer) {
			const updateValue = pendingUpdate;
			pendingUpdate = null;
			try {
				await dotNetReference.invokeMethodAsync('UpdateStartPageAfterScrollAsync', updateValue);
			} catch {
				pendingUpdate = updateValue;
			}
		}
	}, DEBOUNCE_DELAY);
};

const debounce = (func, delay) => {
	if (timeoutId) clearTimeout(timeoutId);
	timeoutId = setTimeout(() => {
		timeoutId = null;
		func();
	}, delay);
};

const updateScrollState = (preview) => {
	if (!initializationDone) return false;
	const newScrollTop = preview.scrollTop;
	const scrollDelta = Math.abs(newScrollTop - lastScrollTop);
	scrollingUp = newScrollTop < lastScrollTop;
	scrollingDown = newScrollTop > lastScrollTop;
	lastScrollTop = newScrollTop;
	return true;
};

export const initialiseMainIntersectionObserver = async (dotNetReference, preview) => {
	if (!preview || !preview.children) return;
	if (initializationDone) return;

	const initializeWithPages = (pages) => {
		if (isInitializing || initializationDone) return false;

		try {
			isInitializing = true;
			if (pages.length > 0) {
				setupEventHandlers();
				setupIntersectionObserver(pages);
				initializationDone = true;
				return true;
			}
		} finally {
			isInitializing = false;
		}
		return false;
	};

	const setupEventHandlers = () => {
		preview.onmousedown = () => mouseDown = true;
		document.onmouseup = () => mouseDown = false;
		lastScrollTop = preview.scrollTop;

		preview.onscroll = () => {
			if (mouseDown) scrollInitiatedFromServer = false;
			updateScrollState(preview);
		};

		preview.addEventListener('wheel', () => {
			scrollInitiatedFromServer = false;
		});
	};

	const setupIntersectionObserver = (pages) => {
		const callback = (entries) => {
			if (!observer || !initializationDone) return;
			if (scrollInitiatedFromServer) return;

			const sortedEntries = [...entries].sort((a, b) => {
				const pageNoA = Number(a.target.getAttribute("data-pageno"));
				const pageNoB = Number(b.target.getAttribute("data-pageno"));
				return scrollingUp ? pageNoB - pageNoA : pageNoA - pageNoB;
			});

			let targetPage = null;
			sortedEntries.forEach((entry) => {
				const pageNo = Number(entry.target.getAttribute('data-pageno'));

				if (scrollingDown) {
					if (!entry.isIntersecting || pageNo === pages.length) {
						targetPage = pageNo === pages.length ? pageNo : pageNo + 1;
					}
				} else if (scrollingUp && entry.isIntersecting) {
					targetPage = targetPage === null ? pageNo : Math.min(targetPage, pageNo);
				}
			});

			if (targetPage && targetPage <= pages.length) {
				queueUpdate(dotNetReference, targetPage);
			}
		};

		if (observer) {
			observer.takeRecords();
			observer.disconnect();
			observer = null;
		}

		observer = new IntersectionObserver(callback, {
			root: preview,
			rootMargin: '0px',
			threshold: 0,
		});

		pages.forEach(page => {
			observer.observe(page);
		});
	};

	let pages = preview.querySelectorAll(".document-preview__outer");
	if (initializeWithPages(pages)) return;

	if (mutationObserver) {
		mutationObserver.takeRecords();
		mutationObserver.disconnect();
		mutationObserver = null;
	}

	mutationObserver = new MutationObserver(() => {
		if (!mutationObserver || initializationDone) return;

		pages = preview.querySelectorAll(".document-preview__outer");
		if (pages.length > 0) {
			const success = initializeWithPages(pages);
			if (success) {
				mutationObserver.takeRecords();
				mutationObserver.disconnect();
				mutationObserver = null;
			}
		}
	});

	mutationObserver.observe(preview, {
		childList: true,
		subtree: true
	});
};


export const scrollMainPageIntoView = (preview, pageNo) => {
	if (!preview || !preview.children) return;
	const pages = preview.querySelectorAll(".document-preview__outer");
	if (pageNo < 1) return;

	pendingUpdate = null;
	if (timeoutId) {
		clearTimeout(timeoutId);
		timeoutId = null;
	}

	scrollInitiatedFromServer = true;
	const height = pages[0].getBoundingClientRect().height;
	const scrollTarget = height * (pageNo - 1);
	preview.scrollTo(preview.scrollLeft, scrollTarget);

};

export const scrollSinglePageIntoView = (preview, scrollToTop) => {
	if (!preview || !preview.children) return;
	const pages = preview.querySelectorAll(".document-preview__outer");
	if (pages.length !== 1) return;

	pendingUpdate = null;
	if (timeoutId) {
		clearTimeout(timeoutId);
		timeoutId = null;
	}

	scrollInitiatedFromServer = true;
	pages[0].scrollIntoView(scrollToTop);

};

export const scrollThumbPageIntoView = (preview, pageNo, pageYSep, realYSep) => {
	if (!preview || !preview.children) return;
	const pages = preview.querySelectorAll(".document-preview__outer");
	if (pageNo < 1) return;

	pendingUpdate = null;
	if (timeoutId) {
		clearTimeout(timeoutId);
		timeoutId = null;
	}

	let height = pages[0].getBoundingClientRect().height;
	if (pages[0].getAttribute('data-pageno') == 1) {
		height = height - pageYSep;
	}

	const offsetTop = height * (pageNo - 1) + pageYSep;
	const requiredScrollTop = offsetTop + height + realYSep - preview.offsetHeight;

	scrollInitiatedFromServer = true;

	if (offsetTop < preview.scrollTop) {
		preview.scrollTo(preview.scrollLeft, offsetTop - pageYSep);
	} else if (requiredScrollTop >= preview.scrollTop) {
		preview.scrollTo(preview.scrollLeft, requiredScrollTop);
	}

};

export const startDragScroll = (dotNetReference, preview, pageNo) => {
	if (!preview || !preview.children || !initializationDone) return;
	const pages = preview.querySelectorAll(".document-preview__outer");
	if (pageNo < 1) return;

	const targetPage = Array.from(pages).find(page =>
		Number(page.getAttribute('data-pageno')) === pageNo
	);

	if (!targetPage) return;

	const listener = {
		active: true,
		dragStarted: false,

		registerListeners() {
			if (!this.active) return;
			document.addEventListener('mouseup', this.onMouseUp);
			document.addEventListener('mousemove', this.onMouseMove);
			this.dragStarted = true;
		},

		removeListeners() {
			if (!this.active || !this.dragStarted) return;
			this.active = false;
			document.removeEventListener('mousemove', this.onMouseMove);
			document.removeEventListener('mouseup', this.onMouseUp);
			this.dragStarted = false;
		},
		onMouseMove: function (e) {
			if (!this.active || !this.dragStarted) return;

			if (isPrimaryMouseButtonPressed(e)) {
				preview.scrollBy(-e.movementX, -e.movementY);
			} else {
				this.removeListeners();
			}
		},
		onMouseUp: function (e) {
			if (!this.active || !this.dragStarted) return;
			this.removeListeners();

			// handle the case when cursor moves beyond the current page
			if (e.target !== targetPage) {
				dotNetReference.invokeMethodAsync('InvokeOnMouseUpAsync', e);
			}
		}
	};
	listener.onMouseMove = listener.onMouseMove.bind(listener);
	listener.onMouseUp = listener.onMouseUp.bind(listener);

	scrollInitiatedFromServer = false;
	listener.registerListeners();
};

export const registerMainKeyEventHandler = (preview) => {
	const step = 10;

	preview.onkeydown = (e) => {
		if (!initializationDone) return;

		e.preventDefault();
		e.stopPropagation();


		pendingUpdate = null;
		if (timeoutId) {
			clearTimeout(timeoutId);
			timeoutId = null;
		}

		scrollInitiatedFromServer = false;

		switch (e.key) {
			case 'ArrowDown':
				preview.scrollTop += e.ctrlKey ? preview.clientHeight : step;
				break;
			case 'ArrowUp':
				preview.scrollTop -= e.ctrlKey ? preview.clientHeight : step;
				break;
			case 'ArrowLeft':
				preview.scrollLeft -= e.ctrlKey ? preview.clientWidth : step;
				break;
			case 'ArrowRight':
				preview.scrollLeft += e.ctrlKey ? preview.clientWidth : step;
				break;
			case 'PageUp':
				if (e.ctrlKey) {
					preview.scrollTop = 0;
				} else {
					preview.scrollTop -= preview.clientHeight;
				}
				break;
			case 'PageDown':
				if (e.ctrlKey) {
					preview.scrollTop = preview.scrollHeight;
				} else {
					preview.scrollTop += preview.clientHeight;
				}
				break;
			case 'Home':
				preview.scrollTop = 0;
				break;
			case 'End':
				preview.scrollTop = preview.scrollHeight;
				break;
		}

	};
};

export const cleanupObserver = () => {
		initializationDone = false;

		if (observer) {
			observer.takeRecords();
			observer.disconnect();
			observer = null;
		}
		if (mutationObserver) {
			mutationObserver.takeRecords();
			mutationObserver.disconnect();
			mutationObserver = null;
		}

		if (timeoutId) {
			clearTimeout(timeoutId);
			timeoutId = null;
		}
		// Reset all state
		scrollingUp = false;
		scrollingDown = false;
		lastScrollTop = 0;
		mouseDown = false;
		pendingUpdate = null;
		isInitializing = false;
		scrollInitiatedFromServer = false;
};

function isPrimaryMouseButtonPressed(e) {
	// Learn more here: https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/buttons
	return e.buttons === 1;
}
