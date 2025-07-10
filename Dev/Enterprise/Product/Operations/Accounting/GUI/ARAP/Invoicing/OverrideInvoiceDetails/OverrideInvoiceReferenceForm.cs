using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideInvoiceReferenceForm : OverrideInvoiceDetailsForm
	{
		public OverrideInvoiceReferenceForm(OverrideInvoiceReferenceHelper bo)
			: base(bo)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void HideSecurityRightsMessage()
		{
			if (BizO == null)
			{
				return; //For designer it is possible that form does not have business object
			}

			UpperPanel.Visible = false;
			var resString = ResourceStringData.Empty;

			var overInvRefHelper = BizO as OverrideInvoiceReferenceHelper;
			if (overInvRefHelper != null && overInvRefHelper.CanBizOBeSaved)
			{
				bool canEditInvoiceRemittance = Env.Security.PayablesModifyInvoiceRemittanceReference.IsAllowed;
				bool canEditInvoiceDateNumOrSupplierCostRef = Env.Security.PayablesModifyInvoiceDateNumOrSupplierCostRef.IsAllowed;
				if (canEditInvoiceRemittance && !canEditInvoiceDateNumOrSupplierCostRef)
				{
					resString = Res.GetData("A807CB76-3030-46F6-9295-F36699E3483B",
						"To override Invoice Date, Invoice Number and Supplier Cost Reference, the following security right is required: Manage > Payables > Payables Transactions > Modify Invoice Date, Invoice Number and Supplier Cost Reference");
				}
				else if (!canEditInvoiceRemittance && canEditInvoiceDateNumOrSupplierCostRef)
				{
					resString = Res.GetData("A0AE46A9-7E9A-4AAB-ACC1-0E033B121DA0",
						"To override Invoice Remittance Reference, the following security right is required: Manage > Payables > Payables Transactions > Modify Invoice Remittance Reference");
				}
			}

			if (resString != ResourceStringData.Empty)
			{
				UpperPanel.Visible = true;
				UpperLabelRegistryMessage.CaptionResourceString = resString;
			}
		}
	}
}
