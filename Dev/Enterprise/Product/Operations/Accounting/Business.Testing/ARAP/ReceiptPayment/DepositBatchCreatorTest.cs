using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(DepositBatchCreator))]
	public class DepositBatchCreatorTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DepositBatchCreator(OriginalReceipt);
		}

		protected override void SetUp()
		{
			base.SetUp();
			OriginalReceipt = Factory.NewWithValidTestData<ARReceipt>();
			DepBatCreator = new DepositBatchCreator(OriginalReceipt);
		}

		Receipt OriginalReceipt;
		DepositBatchCreator DepBatCreator;

		#endregion

		#region TestCreateDepositBatch

		public void TestCreateDepositBatch()
		{
			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OriginalReceipt.AH_AB = bank.PK;
			OriginalReceipt.AH_OH = org.PK;
			OriginalReceipt.AH_InvoiceAmount = -40M;
			OriginalReceipt.AH_OutstandingAmount = -40M;
			OriginalReceipt.AH_OSTotal = -40M;
			OriginalReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			OriginalReceipt.SkipCreateDepositBatch = true; // we create deposit batch later
			Factory.Save();

			try
			{
				Db.Connection.BeginTransaction();
				DepBatCreator.CreateDepositBatch();

				AssertNotNull("Related Deposit Batch should be created", DepBatCreator.RelatedDepositBatch_ForTestOnly);

				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
				TransactionHeaderCollection headers = new TransactionHeaderCollection(Factory, filter);
				headers.Load();
				AssertEquals("There should be 1 deposit batch in the DB", 1, headers.Count);
				DepositBatch postedDepBat = (DepositBatch)headers[0];
				AssertEquals("DepositBatch local amount should be 40", 40M, postedDepBat.AH_InvoiceAmount);
				AssertEquals("DepositBatch overseas amount should be 40", 40M, postedDepBat.AH_OSTotal);
				AssertEquals("Currency should be current company currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, postedDepBat.AH_RX_NKTransactionCurrency);
				AssertEquals("Bank Account should be receipt's bank", bank.PK, postedDepBat.AH_AB);
				AssertEquals("Organisation should be receipt's organisation", org.PK, postedDepBat.AH_OH);

				AssertEquals("DepositBatch ReceiptBatchNo", ZString.Empty, postedDepBat.AH_ReceiptBatchNo);
				AssertEquals("DepositBatch TransactionNum", ZString.Empty, postedDepBat.AH_TransactionNum);
				AssertEquals("OriginalReceipt.AH_ReceiptBatchNo", ZString.Empty, OriginalReceipt.AH_ReceiptBatchNo);

				Factory.Save();
				ZString depositBatchNo = postedDepBat.AH_TransactionNum;
				AssertEquals("OriginalReceipt.AH_ReceiptBatchNo should be set correctly", depositBatchNo, OriginalReceipt.AH_ReceiptBatchNo);
				AssertEquals("DepositBatch ReceiptBatchNo", depositBatchNo, postedDepBat.AH_ReceiptBatchNo);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		#endregion

		#region TestCreateDepositBatchFromCompay

		[TestDate(2009, 09, 11)]
		public void TestCreateDepositBatchFromCompany()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OriginalReceipt.AH_AB = bank.PK;
			OriginalReceipt.AH_OH = org.PK;
			OriginalReceipt.AH_InvoiceAmount = -40M;
			OriginalReceipt.AH_OutstandingAmount = -40M;
			OriginalReceipt.AH_OSTotal = -40M;
			OriginalReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.eNettDirectCredit;
			OriginalReceipt.CompayReceiptBatchDate = new ZDateTime(2009, 01, 05);
			OriginalReceipt.SkipCreateDepositBatch = true; // we create deposit batch later
			Factory.Save();

			try
			{
				Db.Connection.BeginTransaction();
				DepBatCreator.CreateDepositBatch();

				AssertNotNull("Related Deposit Batch should be created", DepBatCreator.RelatedDepositBatch_ForTestOnly);

				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
				TransactionHeaderCollection headers = new TransactionHeaderCollection(Factory, filter);
				headers.Load();
				AssertEquals("There should be 1 deposit batch in the DB", 1, headers.Count);
				DepositBatch postedDepBat = (DepositBatch)headers[0];
				AssertEquals("DepositBatch local amount should be 40", 40M, postedDepBat.AH_InvoiceAmount);
				AssertEquals("DepositBatch overseas amount should be 40", 40M, postedDepBat.AH_OSTotal);
				AssertEquals("Currency should be current company currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, postedDepBat.AH_RX_NKTransactionCurrency);
				AssertEquals("Bank Account should be receipt's bank", bank.PK, postedDepBat.AH_AB);
				AssertEquals("Organisation should be receipt's organisation", org.PK, postedDepBat.AH_OH);

				AssertEquals("DepositBatch ReceiptBatchNo", ZString.Empty, postedDepBat.AH_ReceiptBatchNo);
				AssertEquals("DepositBatch TransactionNum", ZString.Empty, postedDepBat.AH_TransactionNum);
				AssertEquals("OriginalReceipt.AH_ReceiptBatchNo", ZString.Empty, OriginalReceipt.AH_ReceiptBatchNo);

				Factory.Save();
				ZString depositBatchNo = postedDepBat.AH_TransactionNum;
				AssertEquals("OriginalReceipt.AH_ReceiptBatchNo should be set correctly", depositBatchNo, OriginalReceipt.AH_ReceiptBatchNo);
				AssertEquals("DepositBatch ReceiptBatchNo", depositBatchNo, postedDepBat.AH_ReceiptBatchNo);

				AssertEquals("DepositBatch Description", "COMPAY-CC-000000 CREDIT-05012009", postedDepBat.AH_Desc);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		#endregion

		#region TestSetDepositBatchNumber

		public void TestRelatedDepositBatchContainsOnlyOriginalReceipt()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			OriginalReceipt.AH_AB = bank.PK;
			OriginalReceipt.AH_OH = org.PK;
			OriginalReceipt.AH_InvoiceAmount = -40M;
			OriginalReceipt.AH_OSTotal = -40M;
			OriginalReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;

			ARReceipt otherReceipt = Factory.NewWithValidTestData<ARReceipt>();
			otherReceipt.AH_AB = bank.PK;
			otherReceipt.AH_OH = org.PK;
			otherReceipt.AH_InvoiceAmount = -40M;
			otherReceipt.AH_OSTotal = -40M;
			otherReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;

			try
			{
				Db.Connection.BeginTransaction();
				DepBatCreator.CreateDepositBatch();

				AssertNotNull("Related Deposit Batch should be created", DepBatCreator.RelatedDepositBatch_ForTestOnly);
				AssertEquals("Deposit Batch should contain only 1 transaction", 1, DepBatCreator.RelatedDepositBatch_ForTestOnly.Transactions.Count);
				Assert("Deposit Batch should contain only OriginalReceipt", DepBatCreator.RelatedDepositBatch_ForTestOnly.Transactions.Contains(OriginalReceipt));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		#endregion

	}
}
