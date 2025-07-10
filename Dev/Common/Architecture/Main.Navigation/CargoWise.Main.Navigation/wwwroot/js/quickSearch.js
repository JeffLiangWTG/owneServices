let focusShortcutHandler = undefined;
let selectedResultIndex = -1;
let resultItems = [];

export const registerFocusShortcut = (dotNet) => {
    focusShortcutHandler = (event) => {
        if (event.key === 'q' && event.ctrlKey) {
            event.preventDefault();
            dotNet.invokeMethodAsync('FocusAsync');
        }
    }
    document.addEventListener('keydown', focusShortcutHandler);
}

export const updateResults = (popoverId) => {
    const list = document.querySelector(`#${popoverId} .cwn-quick-search__result-items`);
    if (!list) {
        return;
    }
    resultItems = Array.from(list.querySelectorAll('.cwn-quick-search__result-item'));
    selectedResultIndex = -1;
}

export const registerPopoverNavigation = (dotNet, popoverId) => {
    document.addEventListener('keydown', (event) => {
        const searchPopover = document.getElementById(popoverId);
        if (!searchPopover) {
            return;
        }

        const isPopoverOpen = searchPopover.matches(':popover-open');
        if (event.key === 'Escape' && isPopoverOpen) {
            event.preventDefault();
            dotNet.invokeMethodAsync('HandleEscapeKeyAsync');
            const searchBox = document.querySelector(".cwn-search--input");
            searchBox.value = '';
            return;
        }
        if (resultItems.length === 0) {
            return;
        }
        if (event.key === 'ArrowDown') {
            event.preventDefault();
            selectedResultIndex = (selectedResultIndex + 1) % resultItems.length;
            resultItems[selectedResultIndex].focus();
        } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            selectedResultIndex = (selectedResultIndex - 1 + resultItems.length) % resultItems.length;
            resultItems[selectedResultIndex].focus();
        } else if (event.key === 'Enter') {
            if (document.activeElement && document.activeElement.classList.contains('cwn-quick-search__result-item')) {
                event.preventDefault();
                document.activeElement.click();
            }
        }
    });
}

export const registerFocusOutHandler = (dotNet, parentRef, handleFocusOutMethodName) => {
    if (!parentRef) {
        return;
    }
    parentRef.addEventListener('focusout', (event) => {
        const nextFocused = event.relatedTarget;
        if (!nextFocused || !parentRef.contains(nextFocused)) {
            dotNet.invokeMethodAsync(handleFocusOutMethodName);
        }
    });
};
