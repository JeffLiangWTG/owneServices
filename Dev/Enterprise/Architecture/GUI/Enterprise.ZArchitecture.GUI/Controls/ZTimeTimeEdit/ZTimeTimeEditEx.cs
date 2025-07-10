using System.ComponentModel;

using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Supports 3 digit hours (ie format hhh:mm)
	/// </summary>
	[ToolboxItem(true)]
	public class ZTimeTimeEditEx : ZTimeTimeEdit
	{
		#region Bare

		[ToolboxItem(false)]
		public new class Bare : ZTimeTimeEditEx
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(7)]
		public override int MaxLength
		{
			get { return maxLength; }
			set
			{
				maxLength = value;
				FixedWidth = -1; // This resets fixed width calculation.
				this.UpdateWidth();
			}
		}

		int maxLength = 7; // this is the default and thats ok.

		internal override ZTimeTimeEditCore GetNewCore()
		{
			return new ZTimeTimeEditExCore(this);
		}

		protected override string EmptyZeros
		{
			get { return new string('0', MaxLength - 4) + ":00"; }
		}

		protected internal override int HoursDigitCount => MaxLength - 4;
	}
}
