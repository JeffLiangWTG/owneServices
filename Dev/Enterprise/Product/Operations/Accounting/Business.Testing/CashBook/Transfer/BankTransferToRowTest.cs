using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	[TestedType(typeof(BankTransferToRow))]
	public class BankTransferToRowTest : BankTransferRowTest
	{
		public void TestToRowDefaultValues()
		{
			AssertEquals(TransferType.TransferTo, TestBankTransferToRow.TransferType);
			AssertEquals((ZByte)2, TestBankTransferToRow.AH_TransactionCount);
		}

		public void TestInvertSigns()
		{
			AssertEquals(false, TestBankTransferToRow.InvertSigns_ForTestOnly);
		}

		public void TestRateType()
		{
			AssertEquals(ExchangeRateType.Buy, TestBankTransferToRow.RateType);
		}

		public void TestIsSavedByFactoryWhenSameRowLoadAsDifferentBizOType()
		{
			var header = Factory.Load<AccTransactionHeader>(TestBankTransferToRow.PK);
			TestBankTransferToRow.BankTransferParent = null;
			PreviewInvoiceIsNotSavedByFactoryServiceProvider.Register(Factory);

			AssertEquals(false, header.IsSavedByFactory);
			AssertEquals(false, TestBankTransferToRow.IsSavedByFactory);
			AssertNoExceptionThrown("IsSavedByFactory should be consistant.", () => Factory.Save());
		}

		public new void TestAH_MatchStatus() => Assert("BankTransferToRow will be re-evaluated when save", true);

		public new void TestAH_MatchStatusReasonCode() => Assert("BankTransferToRow will be re-evaluated when save", true);

		protected override BusinessObject PrepareTransactionHeaderForTest()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			return bankTransfer.TransferRowTo;
		}

		protected override Type TypeOfValidation => typeof(BankTransferToRowValidation);

		BankTransferToRow TestBankTransferToRow => Header as BankTransferToRow;
	}
}
