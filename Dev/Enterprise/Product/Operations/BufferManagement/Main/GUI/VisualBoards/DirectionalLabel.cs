using System.Drawing;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class DirectionalLabel : FadeLabel
	{
		public DirectionalLabel(bool isVertical)
			: this(null, 0f, isVertical)
		{ }

		public DirectionalLabel(CellContent cell, float gradientAngle, bool isVertical)
			: base(cell, gradientAngle)
		{
			this.isVertical = isVertical;
			TextAlign = isVertical ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
		}

		readonly bool isVertical;
		public bool IsVertical => isVertical;

#if !WINZOR

		protected override void OnPaint(PaintEventArgs e)
		{
			if (isVertical)
			{
				var g = e.Graphics;

				using (var stringFormat = new StringFormat
				{
					Alignment = StringAlignment.Center,
					Trimming = StringTrimming.None,
					FormatFlags = StringFormatFlags.DirectionVertical,
				})
				using (var textBrush = new SolidBrush(this.ForeColor))
				using (var storedState = g.Transform)
				{
					g.RotateTransform(180f);
					g.TranslateTransform(-ClientRectangle.Width, -ClientRectangle.Height);

					TextRendererHelper.DrawText(g, this.Text, this.Font, ClientRectangle, textBrush, stringFormat, renderingEngine: TextRendererType.GDIPlus);
					g.Transform = storedState;
				}
			}
			else
			{
				base.OnPaint(e);
			}
		}

#endif
	}
}
