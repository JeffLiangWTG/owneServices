using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using CargoWise.Common;

namespace CargoWise.Main.Navigation.DragDrop;
public class ListBoxAdorner : Adorner
{
	public ListBoxAdorner(UIElement adornedElement, AdornerLayer adornerLayer, SolidColorBrush adornerBrush)
		: base(adornedElement)
	{
		Argument.NotNull(adornerLayer, nameof(adornerLayer)); // Suggested By ReviewBot 
		this.adornerLayer = adornerLayer;
		this.adornerLayer.Add(this);
		this.adornerBrush = adornerBrush;
	}

	internal void Update()
	{
		adornerLayer.Update(this.AdornedElement);
		Visibility = System.Windows.Visibility.Visible;
	}

	public void Remove()
	{
		Visibility = System.Windows.Visibility.Collapsed;
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		var width = AdornedElement.DesiredSize.Width;
		var height = AdornedElement.DesiredSize.Height;
		var adornedElementRect = new Rect(AdornedElement.DesiredSize);

		var renderBrush = adornerBrush;
		var renderPen = new Pen(adornerBrush, 1.0);

		if (IsAboveElement)
		{
			drawingContext.DrawLine(renderPen, adornedElementRect.TopLeft, adornedElementRect.TopRight);
			drawingContext.DrawGeometry(renderBrush, renderPen, GetArrow(adornedElementRect.TopLeft, ArrowDirection.Left));
			drawingContext.DrawGeometry(renderBrush, renderPen, GetArrow(adornedElementRect.TopRight, ArrowDirection.Right));
		}
		else
		{
			drawingContext.DrawLine(renderPen, adornedElementRect.BottomLeft, adornedElementRect.BottomRight);
			drawingContext.DrawGeometry(renderBrush, renderPen, GetArrow(adornedElementRect.BottomLeft, ArrowDirection.Left));
			drawingContext.DrawGeometry(renderBrush, renderPen, GetArrow(adornedElementRect.BottomRight, ArrowDirection.Right));
		}
	}

	static PathGeometry GetArrow(Point point, ArrowDirection direction)
	{
		var offset = direction == ArrowDirection.Left ? 3 : -3;
		var lines = new[]
			{
				new LineSegment(new Point(point.X, point.Y - 3), true),
				new LineSegment(new Point(point.X, point.Y + 3), true)
			};
		var path = new PathFigure(new Point(point.X + offset, point.Y), lines, true);

		return new PathGeometry(new[] { path });
	}

	enum ArrowDirection
	{
		Left,
		Right
	}

	public bool IsAboveElement { get; set; }

	readonly AdornerLayer adornerLayer;
	readonly SolidColorBrush adornerBrush;
}
