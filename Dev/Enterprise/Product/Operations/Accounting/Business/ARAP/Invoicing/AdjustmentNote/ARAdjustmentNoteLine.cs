using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARAdjustmentNoteLine : InvoicingLineBase
	{
		public ARAdjustmentNoteLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString LineType
		{
			get { return ZArchitecture.Core.TransactionLineTypes.Revenue; }
		}

		protected override GenericChargeCollectionBuilder GetGenericChargeCollectionBuilder()
		{
			return new ARGenericChargeCollectionBuilder(this);
		}

		protected override MasterFiles.Business.AccTransactionLinesValidation GetNewValidationCore()
		{
			return new InvoicingLineBaseValidation(this);
		}

		protected override SecurityCheckpoint ModifyDefaultChargeCodeDescription => Env.Security.NewReceivablesAdjustmentNoteModifyDefaultChargeCodeDescription;
		public override void LogLineDescriptionChanged(string jobNumber, string chargeCode)
		{
			this.LogLineDescriptionChangedOnInvoice(jobNumber, chargeCode);
		}
	}
}
