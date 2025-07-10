using System;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.GUI;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DynamicContentPainter : TextPainter
	{
		public DynamicContentPainter(DynamicContent dynamicContent)
			: base(dynamicContent)
		{
			Argument.NotNull(dynamicContent, nameof(dynamicContent));

			this.dynamicContent = dynamicContent;
		}

		readonly DynamicContent dynamicContent;

		const float IndicatorWidth = 7f;

		float IndicatorActualWidth => Math.Min(IndicatorWidth * Zoom, PaintArea.Y);

		const float IconWidth = 10f;

		float IconActualWidth => Math.Min(IconWidth * Zoom, PaintArea.Y);

		public RectangleF ContentLayoutRectangle
		{
			get
			{
				var x = Util.ConvertToPixelsF(Dpi.X, dynamicContent.Location.X * Zoom);
				var width = Util.ConvertToPixelsF(Dpi.X, dynamicContent.Size.Width * Zoom);

				var y = Util.ConvertToPixelsF(Dpi.Y, dynamicContent.Location.Y * Zoom);
				var height = Util.ConvertToPixelsF(Dpi.Y, dynamicContent.Size.Height * Zoom);

				return new RectangleF(x, y, width, height);
			}
		}

		protected override void Paint(ICanvas canvas, bool isDiagnosticsEnabled)
		{
			InvalidatePaintArea();

			if (dynamicContent.IsEditing)
			{
				canvas.FillRectangle(EnterpriseFormLookStrategy.SelectedControlColor, ContentLayoutRectangle);
			}

			var stringFormat = WorksheetExtensions.GetStringFormat(dynamicContent.HAlignment, dynamicContent.VAlignment, dynamicContent.Wrap);

			var font = ShrinkToFit(canvas, dynamicContent.Content, dynamicContent.Font, stringFormat);

			DrawText(canvas,
				dynamicContent.Content,
				font,
				PaintArea,
				stringFormat,
				Zoom,
				dynamicContent.HAlignment == Alignment.Justify);

			if (!dynamicContent.IsEditing)
			{
				if (dynamicContent.HasOverriddenData)
				{
					DrawIndicator(canvas, ColorSchema.IsOverridden);
				}
				else if (dynamicContent.EditableData.Any())
				{
					DrawIndicator(canvas, ColorSchema.Editable);
				}

				if (dynamicContent.HasErrors())
				{
					DrawNotificationIcon(canvas, NotificationType.Error);
				}
				else if (dynamicContent.HasMessageErrors())
				{
					DrawNotificationIcon(canvas, NotificationType.MessageError);
				}
				else if (dynamicContent.HasWarnings())
				{
					DrawNotificationIcon(canvas, NotificationType.Warning);
				}
			}

			if (isDiagnosticsEnabled
				&& hasFontBeenShrunkToFit)
			{
				var pen = new Pen
				{
					Color = Color.Chocolate,
					Thickness = 1
				};
				canvas.DrawRectangle(pen, PaintArea);
			}
		}

		public IFont DrawFont => drawFont;
		public float Scale => Zoom;

		IFont drawFont;
		string recalcFontKey;
		bool hasFontBeenShrunkToFit;

		IFont ShrinkToFit(ICanvas canvas, string content, IFont font, StringFormat stringFormat)
		{
			if (drawFont != null && string.Compare(recalcFontKey, content, StringComparison.Ordinal) == 0)
			{
				return drawFont;
			}

			var unscaledArea = PaintAreaCalculator.CalculateUnscaledSize(PaintArea, Zoom);

			var fontSize = canvas.ShrinkToFit(content, unscaledArea, font, stringFormat);
			hasFontBeenShrunkToFit = font.Size - fontSize > 0.01f;

			recalcFontKey = content;
			drawFont = new Core.Font(font.Name, fontSize, font.Style, font.Color, font.Rotation);

			return drawFont;
		}

		void DrawIndicator(ICanvas canvas, Color color)
		{
			var rectangle = ContentLayoutRectangle;
			var indicatorWidth = IndicatorActualWidth;

			var topRight = new PointF(rectangle.X + rectangle.Width, rectangle.Y);
			var topLeft = new PointF(topRight.X - indicatorWidth, topRight.Y);
			var bottomRight = new PointF(topRight.X, rectangle.Y + indicatorWidth);

			var points = new[]
				{
					topLeft,
					topRight,
					bottomRight
				};

			canvas.FillPolygon(color, points);
		}

		void DrawNotificationIcon(ICanvas canvas, INotificationType notificationType)
		{
			var rectangle = ContentLayoutRectangle;
			var iconWidth = IconActualWidth;

			var topRight = new PointF(rectangle.X + rectangle.Width - IndicatorActualWidth, rectangle.Y);
			var topLeft = new PointF(topRight.X - iconWidth, topRight.Y);

			var icon = NotificationIconScheme.Instance.GetImage(notificationType);

			if (icon != null)
			{
				var size = new SizeF(iconWidth, iconWidth * icon.Size.Height / icon.Size.Width);
				var location = new RectangleF(topLeft, size);

				canvas.DrawImage(icon, location);
			}
		}
	}
}
