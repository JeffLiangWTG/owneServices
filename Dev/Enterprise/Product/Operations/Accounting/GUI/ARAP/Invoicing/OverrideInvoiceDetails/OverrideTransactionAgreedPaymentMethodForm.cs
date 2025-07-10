using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideTransactionAgreedPaymentMethodForm : OverrideInvoiceDetailsForm
	{
		public OverrideTransactionAgreedPaymentMethodForm(OverrideTransactionAgreedPaymentMethodHelper bo)
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

			var overTransHelper = BizO as OverrideTransactionAgreedPaymentMethodHelper;
			if (overTransHelper != null && overTransHelper.CanBizOBeSaved)
			{
				var trans = overTransHelper.WrappedObjects.FirstOrDefault() as AccTransactionHeader;
				var ledger = trans?.AH_Ledger;
				if (ledger.HasValue && (trans is InvoicingBase || trans is Business.ARAP.Journal.Journal))
				{
					if (ledger.Value == LedgerTypes.AccountsPayable)
					{
						bool canEditAgreedPaymentMethod = Env.Security.PayablesModifyAgreedPaymentMethodForPosted.IsAllowed;
						bool canEditDueDate = Env.Security.PayablesModifyDueDateForPosted.IsAllowed;
						if (canEditAgreedPaymentMethod && !canEditDueDate)
						{
							resString = Res.GetData("E4CD7859-29E4-4768-943F-4BF0EA935F52",
								"To override Due Date, the following security right is required: Manage > Payables > Payables Transactions > Modify Due Date");
						}
						else if (!canEditAgreedPaymentMethod && canEditDueDate)
						{
							resString = Res.GetData("265D198C-76F5-4DC4-96DD-2827F9F11647",
								"To override Agreed Payment Method, the following security right is required: Manage > Payables > Payables Transactions > Modify Agreed Payment Method");
						}
					}
					else if (ledger.Value == LedgerTypes.AccountsReceivable)
					{
						bool canEditAgreedPaymentMethod = Env.Security.ReceivablesModifyAgreedPaymentMethodForPosted.IsAllowed;
						bool canEditDueDate = Env.Security.ReceivablesModifyDueDateForPosted.IsAllowed;
						if (canEditAgreedPaymentMethod && !canEditDueDate)
						{
							resString = Res.GetData("DA623001-B34B-471E-B447-1C3064A45790",
								"To override Due Date, the following security right is required: Manage > Receivables > Receivables Transactions > Modify Due Date");
						}
						else if (!canEditAgreedPaymentMethod && canEditDueDate)
						{
							resString = Res.GetData("001F92A3-BF87-41C7-BAC1-CC17105DB59D",
								"To override Agreed Payment Method, the following security right is required: Manage > Receivables > Receivables Transactions > Modify Agreed Payment Method");
						}
					}
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
