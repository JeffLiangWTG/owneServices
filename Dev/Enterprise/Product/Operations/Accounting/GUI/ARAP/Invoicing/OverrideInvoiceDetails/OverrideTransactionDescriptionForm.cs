using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideTransactionDescriptionForm : OverrideInvoiceDetailsForm
	{
		public OverrideTransactionDescriptionForm(OverrideTransactionDescriptionHelper bo)
			: base(bo)
		{
		}

		public OverrideTransactionDescriptionForm(OverrideTransactionDescriptionHelper bo, ResourceStringData newCaption)
			: this(bo)
		{
			CaptionResourceString = newCaption;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
