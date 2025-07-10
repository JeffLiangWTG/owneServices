using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARCreditNoteLine : CreditNoteLine
	{
		public ARCreditNoteLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void CopyFromInvoiceLine(ARInvoiceLine invoiceLine)
		{
			base.CopyPersistentValuesFrom(invoiceLine);
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString LineType
		{
			get { return ZArchitecture.Core.TransactionLineTypes.Revenue; }
		}

		protected override GenericChargeCollectionBuilder GetGenericChargeCollectionBuilder()
		{
			return IsAmendingOriginal ? new ARAmendingChargeCollectionBuilder(this) : new ARGenericChargeCollectionBuilder(this);
		}

		public override void LogLineDescriptionChanged(string jobNumber, string chargeCode)
		{
			this.LogLineDescriptionChangedOnInvoice(jobNumber, chargeCode);
		}

		protected override SecurityCheckpoint ModifyDefaultChargeCodeDescription => Env.Security.NewReceivablesCreditNoteModifyDefaultChargeCodeDescription;

		bool IsReadOnlyForARCreditNote
		{
			get
			{
				if (InvoiceBase != null)
				{
					return InvoiceBase.IsPostingMiscARCreditNoteLinkedToARInvoiceForComplianceDocument || InvoiceBase.IsAmendingARCreditNoteForComplianceDocument;
				}

				return false;
			}
		}

		public override bool GenericCharge_ReadOnly => IsReadOnlyForARCreditNote || base.GenericCharge_ReadOnly;

		protected override bool AL_JH_ReadOnly => IsReadOnlyForARCreditNote || base.AL_JH_ReadOnly;

		protected override bool AL_AT_ReadOnly => IsReadOnlyForARCreditNote || base.AL_AT_ReadOnly;
	}
}
