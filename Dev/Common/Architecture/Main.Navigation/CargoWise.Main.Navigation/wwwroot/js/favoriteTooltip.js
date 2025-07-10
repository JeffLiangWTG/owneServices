export const setupTooltip = () => {
	const favorites = document.querySelector('.cwn-home__favorites');

	if (favorites) {
		favorites.addEventListener('mouseover', (event) => {
			if (event.target && event.target.matches('.cwn-favorites__item')) {
				const tooltip = event.target.querySelector('.cwn-favorites__item_tooltip');
				if (tooltip) {
					tooltip.style.position = 'fixed';
					const rect = event.target.getBoundingClientRect();
					const tooltipRect = tooltip.getBoundingClientRect();
					tooltip.style.top = `${rect.top - 30}px`;
					tooltip.style.left = `${Math.max(rect.width - tooltipRect.width + rect.x, 0)}px`;
				}
			}
		});
	}
}
