using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using CargoWise.Common;

namespace CargoWise.Main.Navigation.DragDrop;
internal class ListBoxAdornerManager
{
	internal ListBoxAdornerManager(AdornerLayer layer, SolidColorBrush brush)
	{
		Argument.NotNull(layer, nameof(layer));

		adornerLayer = layer;
		adornerBrush = brush;
	}

	internal void Update(UIElement adornedElement, bool isAboveElement)
	{
		if (adorner != null && !shouldCreateNewAdorner)
		{
			if (adorner.AdornedElement == adornedElement && adorner.IsAboveElement == isAboveElement)
			{
				return;
			}
		}
		Clear();

		adorner = new ListBoxAdorner(adornedElement, adornerLayer, adornerBrush);
		adorner.IsAboveElement = isAboveElement;
		adorner.Update();
		shouldCreateNewAdorner = false;
	}

	internal void Clear()
	{
		if (adorner != null)
		{
			adorner.Remove();
			shouldCreateNewAdorner = true;
		}
	}

	internal ListBoxAdorner adorner;
	readonly AdornerLayer adornerLayer;
	readonly SolidColorBrush adornerBrush;
	bool shouldCreateNewAdorner;
}
