using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARInvoiceLine : InvoiceLine, Integration.IARInvoiceLine
	{
		public ARInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void CopyFromCreditNoteLine(ARCreditNoteLine creditNoteLine)
		{
			base.CopyPersistentValuesFrom(creditNoteLine);
		}

		#region Overrides

		protected override GenericChargeCollectionBuilder GetGenericChargeCollectionBuilder()
		{
			return IsAmendingOriginal ? new ARAmendingChargeCollectionBuilder(this) : new ARGenericChargeCollectionBuilder(this);
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString LineType
		{
			get { return ZArchitecture.Core.TransactionLineTypes.Revenue; }
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			return new ARInvoiceLineValidation(this);
		}

		#endregion
		
		public override void LogLineDescriptionChanged(string jobNumber, string chargeCode)
		{
			this.LogLineDescriptionChangedOnInvoice(jobNumber, chargeCode);
		}

		protected override SecurityCheckpoint ModifyDefaultChargeCodeDescription => Env.Security.NewReceivablesInvoiceModifyDefaultChargeCodeDescription;
	}
}
