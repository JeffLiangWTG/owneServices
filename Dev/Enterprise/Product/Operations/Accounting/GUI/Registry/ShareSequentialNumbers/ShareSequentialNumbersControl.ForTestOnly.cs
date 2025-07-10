#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ShareSequentialNumbersControl
	{
		public CargoWise.Windows.UI.KGroupBox OptionGroupBox_ForTestOnly
		{
			get { return OptionGroupBox; }
			set { OptionGroupBox = value; }
		}

		public ZArchitecture.GUI.ZRadioButton NoShareSequentialNumbers_ForTestOnly
		{
			get { return NoShareSequentialNumbers; }
			set { NoShareSequentialNumbers = value; }
		}

		public ZArchitecture.GUI.ZRadioButton YesShareSequentialNumbers_ForTestOnly
		{
			get { return YesShareSequentialNumbers; }
			set { YesShareSequentialNumbers = value; }
		}
	}
}

#endif
