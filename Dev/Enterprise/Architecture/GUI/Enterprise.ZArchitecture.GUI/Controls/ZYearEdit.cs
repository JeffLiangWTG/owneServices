using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(true)]
	public class ZYearEdit : ZCalcEdit
	{
		#region Property Overrides

		public ZYearEdit()
		{
			// it's critical to set this value in the core rather than merely overriding the
			// Decimals property, since it lazy-loads the Core which results in important
			// initialisation.
			base.Decimals = 0;
			base.AllowNegative = false;
			base.MaxLength = 4;
			base.ShowGroupSeparators = false;

			if (!DesignModeFinder.IsDesigning)
			{
				Text = ZDateTime.Now.Year.ToString();
			}
		}

		protected override System.Drawing.Size DefaultSize
		{
			get { return CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20); }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool IsCalculatorEnabled
		{
			get { return false; }
		}

		public new bool Multiline
		{
			get { return false; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int MaxLength
		{
			get { return base.MaxLength; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool ShowGroupSeparators
		{
			get { return base.ShowGroupSeparators; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int Decimals
		{
			get { return base.Decimals; }
		}

		public override string Text
		{
			get { return base.Text; }
			set
			{
				if (value == null || value == "0")
				{
					base.Text = string.Empty;
				}
				else if (value.Length == 1)
				{
					base.Text = "200" + value;
				}
				else if (value.Length == 2)
				{
					if (value[0] > '5')
					{
						base.Text = "19" + value;
					}
					else
					{
						base.Text = "20" + value;
					}
				}
				else
				{
					base.Text = value;
				}
			}
		}

		#endregion
	}
}
