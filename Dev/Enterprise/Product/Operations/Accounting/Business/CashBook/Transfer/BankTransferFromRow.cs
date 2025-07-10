using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public class BankTransferFromRow : BankTransferRow
	{
		public BankTransferFromRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("0550e301-2a11-47d3-8217-25256607c15f", "Bank Transfer From"); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TransferType = TransferType.TransferFrom;
			AH_TransactionCount = TransactionCountConstants.BankTransferFromRow;
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		public override ExchangeRateType RateType
		{
			get { return ExchangeRateType.Buy; }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new BankTransferFromRowValidation(this);
		}

		public override bool IsReversal
		{
			get
			{
				return AH_TransactionCount == TransactionCountConstants.BankTransferFromRowWhenReversing;
			}
		}

		public static BankTransferFromRow LoadBankTransferFromRow(BankTransferRow bankTransferRow)
		{
			var filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, bankTransferRow.AH_TransactionBelongsToGroup);

			if (bankTransferRow.IsReversal)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, TransactionCountConstants.BankTransferFromRowWhenReversing);
			}
			else
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, TransactionCountConstants.BankTransferFromRow);
			}
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, bankTransferRow.AH_GC);

			return bankTransferRow.Factory.LoadTop1<BankTransferFromRow>(filter);
		}
	}
}
