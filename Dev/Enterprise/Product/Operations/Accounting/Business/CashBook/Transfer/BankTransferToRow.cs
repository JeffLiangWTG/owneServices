using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public class BankTransferToRow : BankTransferRow
	{
		public BankTransferToRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("c7a20c7d-1f49-453d-8a79-03926850886d", "Bank Transfer To"); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TransferType = TransferType.TransferTo;
			AH_TransactionCount = TransactionCountConstants.BankTransferToRow;
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		public override ExchangeRateType RateType
		{
			get { return ExchangeRateType.Buy; }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new BankTransferToRowValidation(this);
		}

		public override bool IsReversal
		{
			get
			{
				return AH_TransactionCount == TransactionCountConstants.BankTransferToRowWhenReversing;
			}
		}
	}
}
