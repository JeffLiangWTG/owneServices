using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class AdjustmentNote : InvoicingBase
	{
		public AdjustmentNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override ZString TransactionType
		{
			get
			{
				return Enterprise.ZArchitecture.Core.TransactionTypes.AdjustmentNote;
			}
		}

		public override bool IsCommissionable
		{
			get { return AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.AccountsReceivable; }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new AdjustmentNoteValidation(this);
		}

		protected bool AH_OSTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_OSExTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_OSTotalAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_OSWHTAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_OSExtraTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_LocalTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_LocalExTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_LocalWHTAmountInfo_ReadOnly
		{
			get { return true; }
		}

		protected override bool InvertSignsOfOriginalTransactionOnReversing
		{
			get { return true; }
		}

		protected override  bool AH_LocalExtraTaxAmount_ReadOnly
		{
			get { return true; }
		}

		#endregion
	}
}
