using CargoWise.EntityFramework;

using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class RequisitionValidation : TransactionHeaderValidation
	{
		public RequisitionValidation(APInvoice parent)
			: base(parent)
		{
		}

		protected APInvoice Invoice
		{
			get { return Parent as APInvoice; }
		}

		protected override void CheckAH_RequisitionDate()
		{
			base.CheckAH_RequisitionDate();
			MandatoryValidation.CheckEntered(Parent.AH_RequisitionDateInfo);
		}

		protected override void CheckAH_RequisitionStatus()
		{
			base.CheckAH_RequisitionStatus();
			MandatoryValidation.CheckEntered(Parent.AH_RequisitionStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AH_RequisitionStatusInfo, AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.Value);
		}
	}
}
