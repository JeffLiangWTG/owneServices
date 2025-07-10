using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Transaction.Base
{
	public abstract class TransactionControllerWithReadOnlyBehaviourControlledBySource : TransactionControllerWithLoginCompanyCheck
	{
		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			var invoice = bizObject as InvoicingBase;
			SecurityCheckpoint checkpoint = null;
			if (invoice != null)
			{
				if (invoice.IsImportedFromUniversalXML)
				{
					checkpoint = GetEditCheckPointForUniveralXMLImportedTransaction(invoice);
				}
				else
				{
					checkpoint = GetEditCheckPointForDirectEnteredTransaction(invoice);
				}
			}
			return checkpoint ?? base.GetCheckPointForEdit(bizObject);
		}

		protected override SecurityCheckpoint CheckPointForEdit => null;

		protected virtual SecurityCheckpoint GetEditCheckPointForDirectEnteredTransaction(InvoicingBase invoice) => null;

		protected virtual SecurityCheckpoint GetEditCheckPointForUniveralXMLImportedTransaction(InvoicingBase invoice) => null;

		protected virtual SecurityCheckpoint GetEditHeaderCheckPointForDirectEnteredTransaction(InvoicingBase invoice) => null;

		protected virtual SecurityCheckpoint GetEditHeaderCheckPointForUniveralXMLImportedTransaction(InvoicingBase invoice) => null;

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			var form = base.ShowLoadedForm(sourceEntity, action);
			if (form != null && action == FormAction.Edit)
			{
				var invoice = form.BusinessEntityForPersistingForm as InvoicingBase;
				if (invoice != null)
				{
					SecurityCheckpoint checkpoint = null;
					if (invoice.IsImportedFromUniversalXML)
					{
						checkpoint = GetEditHeaderCheckPointForUniveralXMLImportedTransaction(invoice);
					}
					else
					{
						checkpoint = GetEditHeaderCheckPointForDirectEnteredTransaction(invoice);
					}
					if (checkpoint != null && !checkpoint.IsAllowed)
					{
						invoice.AddWritableProperties(new[] { invoice.AH_RequisitionDateInfo.Name,
														invoice.AH_RequisitionStatusInfo.Name,
														invoice.IsSelfBillingInvoiceInfo.Name });
					}
				}
			}
			return form;
		}
	}
}
