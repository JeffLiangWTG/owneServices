using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocMasterTransferRecord))]
	sealed class DocMasterTransferRecordTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocMasterTransferRecord.New(TransferTo, Factory)
			};
		}

		public void TestDefaults()
		{
			AssertNotNull(MasterTransferRecord.TransferTo);
			AssertNotNull(MasterTransferRecord.TransferFrom);
			AssertNotNull(MasterTransferRecord.Charges);
			AssertEquals((ZByte)1, MasterTransferRecord.TransferFrom.TransactionCount);
			AssertEquals((ZByte)2, MasterTransferRecord.TransferTo.TransactionCount);
			AssertEquals((ZByte)3, MasterTransferRecord.Charges.TransactionCount);
		}

		public void TestCreateMasterWrapperFromAToParty()
		{
			MasterTransferRecord = DocMasterTransferRecord.New(TransferTo, Factory);
			AssertNotNull(MasterTransferRecord.TransferTo);
			AssertNotNull(MasterTransferRecord.TransferFrom);
			AssertEquals((ZByte)1, MasterTransferRecord.TransferFrom.TransactionCount);
			AssertEquals((ZByte)2, MasterTransferRecord.TransferTo.TransactionCount);
			AssertEquals((ZByte)3, MasterTransferRecord.Charges.TransactionCount);
		}

		public void TestCharges()
		{
			AssertNotNull(MasterTransferRecord.Charges);
		}

		public void TestReversedTransfers()
		{
			BankTransferFromRow reversedTransferFrom = Factory.New<BankTransferFromRow>();
			BankTransferToRow reversedTransferTo = Factory.New<BankTransferToRow>();
			DirectPayment reversedCharges = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 0m, 0m, 0m, 0m);

			SetTransferSettings(reversedTransferTo, (ZByte)4, "T0088895", 200M, GroupID);
			SetTransferSettings(reversedTransferFrom, (ZByte)5, "T0088895", -200M, GroupID);
			SetTransferSettings(reversedCharges, (ZByte)6, "T0088896", -2M, GroupID);
			reversedCharges.Lines[0].AL_LineAmount = reversedCharges.Lines[1].AL_LineAmount = -1m;
			reversedCharges.Lines[0].AL_OSAmount = reversedCharges.Lines[1].AL_OSAmount = -1m;
			Factory.Save();

			MasterTransferRecord = DocMasterTransferRecord.New(reversedTransferFrom, Factory);
			AssertEquals((ZByte)5, MasterTransferRecord.TransferFrom.TransactionCount);
			AssertEquals((ZByte)4, MasterTransferRecord.TransferTo.TransactionCount);
			AssertEquals((ZByte)6, MasterTransferRecord.Charges.TransactionCount);

			MasterTransferRecord = DocMasterTransferRecord.New(reversedTransferTo, Factory);
			AssertEquals((ZByte)5, MasterTransferRecord.TransferFrom.TransactionCount);
			AssertEquals((ZByte)4, MasterTransferRecord.TransferTo.TransactionCount);
			AssertEquals((ZByte)6, MasterTransferRecord.Charges.TransactionCount);

			MasterTransferRecord = DocMasterTransferRecord.New(TransferTo, Factory);
			AssertEquals((ZByte)1, MasterTransferRecord.TransferFrom.TransactionCount);
			AssertEquals((ZByte)2, MasterTransferRecord.TransferTo.TransactionCount);
			AssertEquals((ZByte)3, MasterTransferRecord.Charges.TransactionCount);
		}

		public void TestTransactionType()
		{
			AssertEquals(ZArchitecture.Core.TransactionTypes.Transfer, MasterTransferRecord.TransactionType);
		}

		public void TestShowOSTotal()
		{
			TransferTo.AH_RX_NKTransactionCurrency = "AUD";
			TransferFrom.AH_RX_NKTransactionCurrency = "USD";
			AssertEquals("Y", MasterTransferRecord.ShowOSTotal.ToString());

			TransferFrom.AH_RX_NKTransactionCurrency = "AUD";
			Factory.Save();
			MasterTransferRecord = DocMasterTransferRecord.New(TransferFrom, Factory);
			AssertEquals("N", MasterTransferRecord.ShowOSTotal.ToString());
		}

		#region TestBarcode

		public void TestBarcode()
		{
			AssertEquals("", MasterTransferRecord.Barcode);
		}

		#endregion

		DocMasterTransferRecord MasterTransferRecord;
		BankTransferFromRow TransferFrom;
		BankTransferToRow TransferTo;
		DirectPayment Charges;
		ZGuid GroupID;
		protected override void SetUp()
		{
			TransferFrom = Factory.New<BankTransferFromRow>();
			TransferTo = Factory.New<BankTransferToRow>();
			Charges = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 0m, 0m, 0m, 0m);

			GroupID = ZGuid.NewZGuid();
			SetTransferSettings(TransferFrom, (ZByte)1, "T0088893", -200M, GroupID);
			SetTransferSettings(TransferTo, (ZByte)2, "T0088893", 200M, GroupID);
			SetTransferSettings(Charges, (ZByte)3, "T0088894", 2M, GroupID);
			Charges.Lines[0].AL_LineAmount = Charges.Lines[1].AL_LineAmount = 1m;
			Charges.Lines[0].AL_OSAmount = Charges.Lines[1].AL_OSAmount = 1m;

			Factory.Save();

			MasterTransferRecord = DocMasterTransferRecord.New(TransferFrom, Factory);
			base.SetUp();
		}

		void SetTransferSettings(TransactionHeader transfer, ZByte count, ZString transactionNum, ZDecimal amount, ZGuid groupID)
		{
			transfer.AH_TransactionCount = count;
			transfer.AH_TransactionNum = transactionNum;
			transfer.AH_InvoiceAmount = amount;
			transfer.AH_OSTotal = amount;
			transfer.AH_TransactionBelongsToGroup = groupID;
		}
	}
}
