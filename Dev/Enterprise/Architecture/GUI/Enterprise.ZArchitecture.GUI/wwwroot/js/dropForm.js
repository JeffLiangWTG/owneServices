export const scrollTo = (dropForm, itemHeight, index) => {
	if (!dropForm) {
		return;
	}

	const itemTop = itemHeight * index;
	let additionalTries = 0;
	const scrollToPosition = () => {
		const itemOffsetTop = itemTop - dropForm.scrollTop;
		const dropFormContentHeight = dropForm.offsetHeight - 2;
		if (itemOffsetTop >= 0 && itemOffsetTop <= dropFormContentHeight - itemHeight) {
			// item is already in viewport.
			return;
		}
		// simulation of CW1 scroll behaviour.
		if (itemTop > dropFormContentHeight - itemHeight && itemTop > dropForm.scrollTop) {
			dropForm.scrollTop = itemTop - dropFormContentHeight + itemHeight;
		} else if (itemTop < dropForm.scrollTop) {
			dropForm.scrollTop = itemTop;
		}
		// have seen dropFormContentHeight not accurate, retry up to 2 times just to be safe.
		if (additionalTries++ < 2) {
			setTimeout(scrollToPosition, 10);
		}
	};
	setTimeout(scrollToPosition, 10);
};
