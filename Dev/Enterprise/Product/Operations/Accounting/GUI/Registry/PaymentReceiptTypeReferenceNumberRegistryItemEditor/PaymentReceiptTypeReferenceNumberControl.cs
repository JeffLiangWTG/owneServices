using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class PaymentReceiptTypeReferenceNumberControl : RegistryZUserControl
	{
		public PaymentReceiptTypeReferenceNumberControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PaymentReceiptTypeReferenceNumberGrid.ReadOnly = readOnly;
		}

		void PaymentReceiptTypeReferenceNumberGrid_RowDeleting(object sender, ZArchitecture.RowsDeletingEventArgs e)
		{
			e.Cancel = true;
		}
	}
}

