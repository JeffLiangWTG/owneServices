using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSDutiesAndTaxesUserControl : ZUserControl
	{
		public LVSDutiesAndTaxesUserControl()
		{
			InitializeComponent();
			CustomsValueOvrCheckBox.AllowOutsideOfParent();
			CurrConvOvrrideCheckBox.AllowOutsideOfParent();
		}
	}
}
