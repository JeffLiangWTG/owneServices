using System;
using System.Drawing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class Rectangle : DocumentElement
	{
		public Rectangle(IDocumentCell cell, PointF location, SizeF size)
			: base(location, size)
		{
			this.cell = cell;
		}

		readonly IDocumentCell cell;

		public override ElementType ElementType
		{
			get { return ElementType.Rectangle; }
		}

		public Color SolidColor
		{
			get
			{
				return !cell.HasNotifications()
					? cell.Format.BackgroundColor
					: Color.Empty;
			}
		}

		public Tuple<Color, Color> GradientColors
		{
			get
			{
				if (cell.HasErrors())
				{
					return ColorSchema.ErrorBackgroundGradient;
				}

				if (cell.HasMessageErrors())
				{
					return ColorSchema.MessageErrorBackgroundGradient;
				}
				if (cell.HasWarnings())
				{
					return ColorSchema.WarningBackgroundGradient;
				}

				return new Tuple<Color, Color>(cell.Format.BackgroundColor, cell.Format.BackgroundColor);
			}
		}
	}
}