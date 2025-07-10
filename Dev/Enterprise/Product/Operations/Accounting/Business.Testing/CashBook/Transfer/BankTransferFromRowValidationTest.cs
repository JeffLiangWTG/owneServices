using System;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public class BankTransferFromRowValidationTest : BankTransferRowValidationTest
	{
		protected override TransactionHeader CreateHeader()
		{
			BankTransfer transfer = new BankTransfer(Factory, null);
			transfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			transfer.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;
			transfer.BuyExchangeRate = 2M;
			transfer.SellAmount = 100m;

			return transfer.TransferRowFrom;
		}

		protected override Type HeaderType => typeof(BankTransferFromRow);
	}
}
