using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	internal abstract class ZCalculatorButton : ZButton.Bare
	{
		public ZCalculatorButton()
		{
			Font = new Font(OFont.DefaultFontName, 9.0f);
			ControlDpiScalingHelper.SetWidth(this, FixedWidth, true);
			ControlDpiScalingHelper.SetHeight(this, FixedHeight, true);
			TabStop = false;
			Text = "";

			this.SetStyle(ControlStyles.FixedHeight | ControlStyles.FixedWidth, true);
			this.SetStyle(ControlStyles.Selectable, false);
		}

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override Font Font
		{
			get { return base.Font;  }
			set { base.Font = value; }
		}

		[DpiState(DpiState.Unscaled)]
		protected virtual int FixedWidth
		{
			get { return 34; }
		}

		[DpiState(DpiState.Unscaled)]
		protected virtual int FixedHeight
		{
			get { return 28; }
		}

		#region OnResize()

		protected override void OnResize(EventArgs e)
		{
			SuspendLayout();
			try
			{
				if (Width  != ControlDpiScalingHelper.ScaleToCurrentDpiX(FixedWidth))
				{
					ControlDpiScalingHelper.SetWidth(this, FixedWidth, true);
				}

				if (Height != ControlDpiScalingHelper.ScaleToCurrentDpiY(FixedHeight))
				{
					ControlDpiScalingHelper.SetHeight(this, FixedHeight, true);
				}
			}
			finally
			{
				ResumeLayout();
			}

			base.OnResize(e);
		}

		#endregion

		#region .NET Properties Hidden from the Designer

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Size Size
	{
		get { return base.Size;  }
		set { base.Size = value; }
	}

	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new bool TabStop
	{
		get { return base.TabStop;  }
		set { base.TabStop = value; }
	}

	#endregion
	}
}