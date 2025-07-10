using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	[TestedType(typeof(BankTransferFromRow))]
	public class BankTransferFromRowTest : BankTransferRowTest
	{
		BankTransferFromRow TestBankTransferFromRow
		{
			get { return Header as BankTransferFromRow; }
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(BankTransferFromRowValidation); }
		}

		public void TestFromRowDefaultValues()
		{
			AssertEquals(TransferType.TransferFrom, TestBankTransferFromRow.TransferType);
			AssertEquals((ZByte)1, TestBankTransferFromRow.AH_TransactionCount);
		}

		public void TestInvertSigns()
		{
			AssertEquals(true, TestBankTransferFromRow.InvertSigns_ForTestOnly);
		}

		public void TestRateType()
		{
			AssertEquals(ExchangeRateType.Buy, TestBankTransferFromRow.RateType);
		}

		protected override BusinessObject PrepareTransactionHeaderForTest()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			return bankTransfer.TransferRowFrom;
		}
	}
}
