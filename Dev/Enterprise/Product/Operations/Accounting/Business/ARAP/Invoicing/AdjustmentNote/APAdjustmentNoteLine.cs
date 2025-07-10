using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APAdjustmentNoteLine : InvoicingLineBase
	{
		public APAdjustmentNoteLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override AccTransactionHeader TransactionHeader => APAdjustmentNote ?? base.TransactionHeader;

		protected override AccTransactionLinesLookups GetNewLookups()
		{
			return new APAdjustmentNoteLineLookups(this);
		}

		APAdjustmentNote APAdjustmentNote => MasterTransactionHeader as APAdjustmentNote;

		protected override bool SupportsInputVatRecoverableCore
		{
			get { return true; }
		}

		protected override Security.SecurityCheckpoint OverrideInputVatRecoverableSecurityCheckPoint
		{
			get { return Env.Security.AllowAPAdjustmentNoteLineVATRecoverableOverride; }
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString LineType
		{
			get { return TransactionLineTypes.Cost; }
		}

		protected override GenericChargeCollectionBuilder GetGenericChargeCollectionBuilder()
		{
			return new APGenericChargeCollectionBuilder(this);
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			if (Factory.HasContext(BusinessContext.SavingIncompleteTransaction))
			{
				return new IncompleteInvoicingLineBaseValidation(this);
			}

			return new InvoicingLineBaseValidation(this);
		}
	}
}
