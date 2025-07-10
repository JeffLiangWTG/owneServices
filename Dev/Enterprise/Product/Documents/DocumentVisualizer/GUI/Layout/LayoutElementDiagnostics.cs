using System;
using System.Drawing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	/// <summary>
	/// Class used in @diagnostics macro with the goal to provide help to diagnose UI issues
	/// </summary>
	public sealed class LayoutElementDiagnostics
	{
		public LayoutElementDiagnostics(ILayoutElementInternals element, Action<bool> visualCuesSwitch)
		{
			this.element = element ?? throw new ArgumentNullException(nameof(element));
			this.visualCuesSwitch = visualCuesSwitch;
		}

		readonly ILayoutElementInternals element;
		readonly Action<bool> visualCuesSwitch;

		public IFont Font => element.Font;
		public float Scale => element.Scale;
		public IFont DrawFont => element.DrawFont;
		public RectangleF PaintArea => element.PaintArea;
		public RectangleF LayoutArea => element.LayoutArea;

		[MacroInvokable]
		public object RunTextDrawTest(string text = null) => RunTextDrawTest(text, element.Cell.Format.WrapText);

		[MacroInvokable]
		public object RunTextDrawTest(string text, bool wrapText)
		{
			var content = text ?? element.Text;

			var stringFormat = WorksheetExtensions.GetStringFormat(element.Cell.Format.HAlignment, element.Cell.Format.VAlignment, wrapText);
			var unscaledArea = PaintAreaCalculator.CalculateUnscaledSize(PaintArea, Scale);

			using (var shrinkToFitCalculator = new ShrinkToFitCalculator())
			{
				var drawFontSize = shrinkToFitCalculator.ShrinkToFit(content, unscaledArea, Font, stringFormat);
				var requiredSpace = shrinkToFitCalculator.CalculateRequiredSpace(content, Font, unscaledArea, stringFormat);

				return new
				{
					Text = content,
					FontInTemplate = element.Font,
					SpaceRequredToDrawTextWithFontSizeFromTemplate = requiredSpace,
					SpaceAvailableToDrawText = unscaledArea,
					Scale = Scale,
					DPI = Util.GetDpi(),
					PaintArea = PaintArea,
					LayoutArea = LayoutArea,
					StringFormat = stringFormat,
					WrapText = wrapText,
					FontSizeAdjustedToFitTextInTheAvailableSpace = drawFontSize
				};
			}
		}

		[MacroInvokable]
		public void EnableVisualCues() => visualCuesSwitch?.Invoke(true);

		[MacroInvokable]
		public void DisableVisualCues() => visualCuesSwitch?.Invoke(false);
	}
}
