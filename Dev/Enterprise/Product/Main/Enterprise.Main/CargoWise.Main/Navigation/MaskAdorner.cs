#if !WINZOR
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CargoWise.Main.Navigation;

public class MaskAdorner : Adorner
{
	readonly VisualCollection _visuals;
	readonly Grid _backgroundLayer;
	readonly UserControl _content;
	readonly AdornerLayer _adornerLayer;

	public MaskAdorner(UIElement parentElement, UserControl content, UIElement originalElement)
		: base(parentElement)
	{
		_adornerLayer = AdornerLayer.GetAdornerLayer(parentElement);
		_adornerLayer.Focusable = true;
		_adornerLayer.Focus();

		_backgroundLayer = new Grid
		{
			Background = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00)),
			IsHitTestVisible = true
		};

		_content = content;
		_content.HorizontalAlignment = HorizontalAlignment.Center;
		_content.VerticalAlignment = VerticalAlignment.Center;

		_visuals = new VisualCollection(this) { _backgroundLayer };
	}

	void Decorator_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			Close();
			e.Handled = true;
		}
	}
	public void Show()
	{
		if (_adornerLayer != null)
		{
			_adornerLayer.Focus();
			_adornerLayer.KeyDown += Decorator_KeyDown;
			_adornerLayer.Add(this);
			_backgroundLayer.Children.Add(_content);
		}
	}
	public void Close()
	{
		if (_adornerLayer != null)
		{
			_adornerLayer.KeyDown -= Decorator_KeyDown;
			_adornerLayer?.Remove(this);
			_backgroundLayer.Children.Remove(_content);
		}
	}

	protected override int VisualChildrenCount => _visuals.Count;
	protected override Visual GetVisualChild(int index) => _visuals[index];

	protected override Size ArrangeOverride(Size finalSize)
	{
		_backgroundLayer.Arrange(new Rect(finalSize));
		return base.ArrangeOverride(finalSize);
	}

	public static MaskAdorner Create(UIElement adornedElement, UserControl dialog, int dialogWidth)
	{
		var parent = FindVisualRoot(adornedElement);
		dialog.Width = dialogWidth;
		return new MaskAdorner(parent, dialog, adornedElement);
	}
	static UIElement FindVisualRoot(DependencyObject obj)
	{
		while (obj != null)
		{
			if (obj is NextHomeNavigation nextHome)
			{
				return nextHome;
			}
			obj = VisualTreeHelper.GetParent(obj) ?? LogicalTreeHelper.GetParent(obj);
		}
		return null;
	}
}
#endif
