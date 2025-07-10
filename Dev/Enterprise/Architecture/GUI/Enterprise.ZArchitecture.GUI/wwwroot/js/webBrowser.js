export const updateAnchorAttributes = () => {
	Array.from(document.querySelectorAll('div.zwebbrowser *[name]')).forEach((anchor) => {
		// Blazor handles scrolling to element with an anchor by matching the ID to the segment from the uri
		// CargoWise is setting the name property so we need to copy it into the id and uri encode so it matches the route
		anchor.id = encodeURI(anchor.name);
	});
};

export const ResetScrollPosition = () => {
	const webBrowser = document.getElementsByClassName('zwebbrowser')[0];
	if (webBrowser) {
		webBrowser.scrollTo(0, 0);
	}
};
