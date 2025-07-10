using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components.Rendering;

namespace Enterprise.DocumentVisualizer.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017:Non DPI-aware code has been detected", Justification = "<Pending>")]
	public class EditableIndicator : Control
	{
		const int IndicatorSize = 5;
		public EditableIndicator(Control control)
		{
			Size = new Size(IndicatorSize, IndicatorSize);
			Location = new Point(control.Bounds.Location.X + control.Bounds.Width - IndicatorSize, control.Bounds.Location.Y - IndicatorSize);
			BackColor = control.BackColor;
		}

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenElement(0, (NoResString)"svg");
			builder.AddAttribute(1, (NoResString)"height", "100%");
			builder.AddAttribute(2, (NoResString)"width", "100%");
			builder.OpenElement(3, (NoResString)"polygon");
			builder.AddAttribute(4, (NoResString)"points", $"0 0, {IndicatorSize} 0, {IndicatorSize} {IndicatorSize}");
			builder.AddAttribute(5, (NoResString)"style", (NoResString)"fill:green;stroke-width:1");
			builder.CloseElement();
			builder.CloseElement();
		}
	}
}
