#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class BackDateInvoicesConfigurationControl
	{
		public ZArchitecture.GUI.ZCheckBox OverridePostDateCheckBox_ForTestOnly
		{
			get { return OverridePostDateCheckBox; }
			set { OverridePostDateCheckBox = value; }
		}

		public ZArchitecture.GUI.ZCheckBox DefaultPostDateFromInvoiceDateCheckBox_ForTestOnly
		{
			get { return DefaultPostDateFromInvoiceDateCheckBox; }
			set { DefaultPostDateFromInvoiceDateCheckBox = value; }
		}

		public ZArchitecture.ZGrid InvoiceDateConfigurationGrid_ForTestOnly
		{
			get { return InvoiceDateConfigurationGrid; }
			set { InvoiceDateConfigurationGrid = value; }
		}
	}
}

#endif
