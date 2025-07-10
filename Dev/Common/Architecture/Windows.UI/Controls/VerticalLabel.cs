using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;

namespace CargoWise.Windows.UI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
	public enum VerticalLabelDrawMode
	{
		/// <summary>
		/// Text is drawn from bottom to top
		/// </summary>
		BottomUp = 1,
		/// <summary>
		/// Text is drawn from top to bottom
		/// </summary>
		TopBottom
	}

	/// <summary>
	/// A label that draws its text vertically.
	/// </summary>
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class VerticalLabel : System.Windows.Forms.Control
	{
		public VerticalLabel()
		{
			Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 100);
			TextDrawMode = VerticalLabelDrawMode.BottomUp;
		}

		[Category(DesignerConstants.Category)]
		[Description("Text that is displayed vertically on the label.")]
		public override string Text
		{
			get { return text; }
			set
			{
				text = value;
				Invalidate();
			}
		}
		string text;

		[Category("Properties"), Description("Whether the text will be drawn from Bottom or from Top.")]
		[DefaultValue(VerticalLabelDrawMode.BottomUp)]
		public VerticalLabelDrawMode TextDrawMode { get; set; }

		#region Implementation
#if !WINZOR
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint(e);

			float width = Size.Width;
			float height = Size.Height;
			e.Graphics.DrawRectangle(BackColorPen, 0, 0, width, height);
			e.Graphics.FillRectangle(BackColorBrush, 0, 0, width, height);

			if (TextDrawMode == VerticalLabelDrawMode.BottomUp)
			{
				e.Graphics.TranslateTransform(0, height);
				e.Graphics.RotateTransform(270);
				e.Graphics.DrawString(text, Font, ForeColorBrush, 0, 0);
			}
			else
			{
				e.Graphics.TranslateTransform(width, 0);
				e.Graphics.RotateTransform(90);
				e.Graphics.DrawString(text, Font, ForeColorBrush, 0, 0, StringFormat.GenericTypographic);
			}
		}

		Pen BackColorPen
		{
			get
			{
				if (backColorPen == null)
				{
					if (BackColor.IsSystemColor)
					{
						backColorPen = SystemPens.FromSystemColor(BackColor);
					}
					else
					{
						backColorPen = new Pen(BackColor);
					}
				}
				return backColorPen;
			}
		}
		Pen backColorPen;

		Brush BackColorBrush
		{
			get
			{
				if (backColorBrush == null)
				{
					if (BackColor.IsSystemColor)
					{
						backColorBrush = SystemBrushes.FromSystemColor(BackColor);
					}
					else
					{
						backColorBrush = new SolidBrush(BackColor);
					}
				}
				return backColorBrush;
			}
		}
		Brush backColorBrush;

		Brush ForeColorBrush
		{
			get
			{
				if (foreColorBrush == null)
				{
					if (ForeColor.IsSystemColor)
					{
						foreColorBrush = SystemBrushes.FromSystemColor(ForeColor);
					}
					else
					{
						foreColorBrush = new SolidBrush(ForeColor);
					}
				}
				return foreColorBrush;
			}
		}
		Brush foreColorBrush;
#endif
		#endregion
	}
}
