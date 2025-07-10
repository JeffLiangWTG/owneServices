function getParentLiofUl(ul) {
	let li = ul.closest('li');
	if (li == null && ul.hasAttribute('data-id')) {
		const dataId = ul.getAttribute('data-id');
		li = document.querySelector(`.cwn-navbar__toolstrip li[data-id="${dataId}"]`);
		if (li == null) {
			li = document.querySelector(`body > .cwn-submenu li[data-id="${dataId}"]`);
		}
	}
	return li;
}

function setUlPosition(ul, li) {
	const rect = li.getBoundingClientRect();
	ul.style.left = `${rect.left + window.scrollX - 300}px`;
	ul.style.top = `${rect.top + window.scrollY}px`;
	ul.style.maxHeight = `unset`;
	const height = window.innerHeight;
	const ulRect = ul.getBoundingClientRect();
	if (ulRect.bottom > height) {
		ul.style.maxHeight = `${height - ulRect.top - 20}px`;
	}
}

function getMaxZIndex() {
	let maxZIndex = 0;
	const appBar = document.querySelector('.cwn-appbar');
	if (appBar) {
		maxZIndex = parseInt(window.getComputedStyle(appBar).zIndex) + 1;
	}

	return maxZIndex;
}

let enablekeyboard = false;
export const InitScrollMenu = () => {
	const bts = document.querySelectorAll('.cwn-menu > .dropdown > button');
	for (let i = 0; i < bts.length; i++) {
		const btn = bts[i];
		btn.addEventListener('click', (e) => {
			e.preventDefault();
			e.stopPropagation();
			reset();
			btn.closest('li').classList.add('active');
			currentLi = null;
			enablekeyboard = true;
			const ul = btn.nextElementSibling;
			if (ul) {
				ul.style.display = 'block';
				ul.tabIndex = 0;
				ul.focus();
			}
		});
	}

	document.addEventListener('click', function () {
		reset(false);
		currentLi = null;
		enablekeyboard = false;
	});

	window.addEventListener('resize', function () {
		setTimeout(function () {
			const uis = document.querySelectorAll('body > ul.cwn-submenu');
			for (let i = 0; i < uis.length; i++) {
				const ul = uis[i];
				let li = getParentLiofUl(ul);
				setUlPosition(ul, li);
				const scrollTop = ul.getAttribute('data-scrollTop');
				if (scrollTop) {
					ul.scrollTop = scrollTop;
				}
			}
		}, 1000);
	});

	const uls = document.querySelectorAll('.cwn-navbar__toolstrip ul.cwn-submenu');
	for (let i = 0; i < uls.length; i++) {
		const ul = uls[i];
		ul.addEventListener('scroll', () => {
			ul.setAttribute('data-scrollTop', ul.scrollTop);
		});
	}

	const anchors = document.querySelectorAll('.cwn-menu .cwn-list-item a');
	let currentLi = null;
	for (let i = 0; i < anchors.length; i++) {
		const a = anchors[i];
		const li = a.parentElement;
		li.addEventListener('mouseover', (e) => {
			currentLi = li;
		});
	}
	const submenus = document.querySelectorAll('.cwn-submenu');
	submenus.forEach(m => {
		m.addEventListener('keydown', function (event) {
			if (event.key === "ArrowUp" || event.key === "ArrowDown" || event.key === "ArrowLeft" || event.key === "ArrowRight"
				|| event.key === "Enter") {
				event.preventDefault();
				event.stopPropagation();
				if (enablekeyboard == false) {
					return;
				}

				if (currentLi == null) {
					currentLi = document.querySelector('.cwn-menu > .dropdown.active > .cwn-submenu > li:first-child');
					liMouseOver(event, currentLi);
					return;
				}

				if (currentLi != null) {
					switch (event.key) {
						case "ArrowUp":
							let nextLiUp = currentLi.previousElementSibling;
							if (nextLiUp == null) {
								nextLiUp = currentLi.parentElement.lastElementChild;
							}
							while (nextLiUp.tagName != 'LI') {
								nextLiUp = nextLiUp.previousElementSibling;
								if (nextLiUp == null) {
									nextLiUp = currentLi.parentElement.lastElementChild;
								}
							}
							currentLi = nextLiUp;
							currentLi.scrollIntoView({
								behavior: 'auto',
								block: 'center'
							});
							liMouseOver(event, nextLiUp);
							break;
						case "ArrowLeft":
							if (currentLi.hasAttribute('data-id') == true) {
								const leftUl = getUlByDataId(currentLi.getAttribute('data-id'));
								let nextLiLeft = leftUl.querySelector('li');
								liMouseOver(event, nextLiLeft);
								currentLi = nextLiLeft;
							}
							break;
						case "ArrowRight":
							const currentUl = currentLi.closest('ul');
							if (currentUl.hasAttribute('data-id')) {
								const nextLiRight = getParentLiofUl(currentUl);
								liMouseOver(event, nextLiRight);
								currentLi = nextLiRight;
							}
							break;
						case "ArrowDown":
							let nextLiDown = currentLi.nextElementSibling;
							if (nextLiDown == null) {
								nextLiDown = currentLi.parentElement.firstElementChild;
							}
							while (nextLiDown.tagName != 'LI') {
								nextLiDown = nextLiDown.nextElementSibling;
								if (nextLiDown == null) {
									nextLiDown = currentLi.parentElement.firstElementChild;
								}
							}
							currentLi = nextLiDown;
							currentLi.scrollIntoView({
								behavior: 'auto',
								block: 'center'
							});
							liMouseOver(event, nextLiDown);
							break;
						case "Enter":
							let a = currentLi.querySelector('a');
							if (a) {
								a.click();
							}
							break;
					}
				}
			}
		});
	});
	


	const lis = document.querySelectorAll('.cwn-menu > .dropdown > .cwn-submenu .cwn-list-item');
	for (let i = 0; i < lis.length; i++) {
		const li = lis[i];
		li.addEventListener('mouseover', (e) => {
			liMouseOver(e, li);
		});

		const span = li.querySelector(':scope > span');
		if (span) {
			li.addEventListener('click', (e) => {
				liMouseOver(e, li);
			});
		}
	}

	function liMouseOver(e, li) {
		e.preventDefault();
		e.stopPropagation();

		let ulParent = li.closest('ul');
		let ulParents = [];
		while (ulParent && ulParent.hasAttribute('data-id')) {
			ulParents.push(ulParent);
			let tempLi = getParentLiofUl(ulParent);
			ulParent = tempLi.closest('ul');
		}
		ulParents.push(ulParent);
		reset(true, ulParents);

		

		li.classList.add('active');

		

		const ul = li.querySelector('.cwn-submenu');
		openSubMenu(ul, li);
	}
	function openSubMenu(ul, li) {
		if (ul != null) {
			ul.remove();
			document.body.appendChild(ul);
			ul.style.display = 'block';
			ul.style.position = 'absolute';
			ul.style.zIndex = getMaxZIndex();
			setUlPosition(ul, li);
		}
	}

	function reset(hidePopover = true, ulParents=[]) {
		if (hidePopover) {
			const popover = document.querySelector('.cwn-popover');
			if (popover) {
				popover.hidePopover();
			}
		}
		
 		const uls = document.querySelectorAll('body > ul.cwn-submenu');
		for (let i = 0; i < uls.length; i++) { //move uls from body to lis
			const ul = uls[i];
			if (ulParents.includes(ul)) {
				continue;
			}
			ul.style.display = 'none';
			let li = getParentLiofUl(ul);
			if (li) {
				ul.remove();
				li.appendChild(ul);
			}
		}

		const lis = document.querySelectorAll('.cwn-navbar__toolstrip li.active');
		for (let i = 0; i < lis.length; i++) {// clear active state
			const li = lis[i];
			if (li.classList.contains('active') == true) {
				li.classList.remove('active');
			}
		}

		const ulstoolstrip = document.querySelectorAll('.cwn-navbar__toolstrip .cwn-menu ul');
		for (let i = 0; i < ulstoolstrip.length; i++) {// clear ul
			const ul = ulstoolstrip[i];
			if (ulParents.includes(ul)) {
				continue;
			}
			ul.style.display = 'none';
		}

		const bodyLis = document.querySelectorAll('body > ul.cwn-submenu li.active');
		for (let i = 0; i < bodyLis.length; i++) {// clear active state
			const li = bodyLis[i];
			if (li.classList.contains('active') == true) {
				li.classList.remove('active');
			}
		}

		if (ulParents.length > 0) {
			const topLi = getParentLiofUl(ulParents[ulParents.length-1]);
			topLi.classList.add('active');
		}
	}

	function getUlByDataId(dataId) {
		let ul = document.querySelector(`.cwn-navbar__toolstrip ul[data-id="${dataId}"]`);
		if (ul == null) {
			ul = document.querySelector(`body > ul[data-id="${dataId}"]`);
		}
		return ul;
	}
}
