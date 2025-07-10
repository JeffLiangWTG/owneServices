using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class ExcludedFullyDigitalizedElectronicInvoiceDataControl : RegistryZUserControl
	{
		public ExcludedFullyDigitalizedElectronicInvoiceDataControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			BuyerAddressCheckEdit.ReadOnly = readOnly;
			BuyerPhoneNumberCheckEdit.ReadOnly = readOnly;
			BuyerBankAccountCheckEdit.ReadOnly = readOnly;
		}
	}
}
