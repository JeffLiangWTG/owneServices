export const getFilterStripsPanelScrollTop = (element) => {
	while (element.getAttribute('data-name') != 'FilterStripsPanel' && element.parentElement != null) {
		element = element.parentElement;
	}

	return element.getAttribute('data-name') == 'FilterStripsPanel' ? Math.round(element.scrollTop) : 0;
};
