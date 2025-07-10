using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[UseSnapshotProtection]
	public class TransactionHeaderNonTransactionalTest : TestCase
	{
		public void TestInvalidOrgHeader()
		{
			var factory = new BusinessObjectFactory();
			var invoice = factory.New<ARInvoice>();

			AssertNoExceptionThrown(() => invoice.OrganisationAddressWithContact.OrgPK = ZGuid.Invalid);
		}

		public void TestEmptyOrgHeader()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var invoice = factory.New<ARInvoice>();

			invoice.OrganisationAddressWithContact.OrgPK = testObjectCreator.ABIGAS.PK;
			AssertEquals(testObjectCreator.ABIGAS.PK, invoice.AH_OH);

			invoice.OrganisationAddressWithContact.OrgPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, invoice.AH_OH);
		}

		public void TestDontSkipFountainNumbersForARInvoice()
		{
			var factory1 = new BusinessObjectFactory();
			var testObjectCreator1 = new TestObjectCreator(factory1);
			var invoiceInstance1 = testObjectCreator1.CreateInvoiceWithLine(typeof(ARInvoiceToTestSecondInstance), "", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			var invoiceInstance2 = factory1.Load<ARInvoice>(invoiceInstance1.PK);
			factory1.Save();
			AssertEquals("AH_TransactionNum", "00001000", invoiceInstance1.AH_TransactionNum);

			invoiceInstance1 = testObjectCreator1.CreateInvoiceWithLine(typeof(ARInvoice), "", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			invoiceInstance2 = factory1.Load<ARInvoiceToTestSecondInstance>(invoiceInstance1.PK);
			var bizoInvalidForSaving = factory1.New<BizoWithOnSavingExceptionSavedAfterTrasnactionHeader>();
			bizoInvalidForSaving.SetValues(invoiceInstance1);
			bizoInvalidForSaving.FillWithValidTestData();

			AssertExceptionThrown<ZCannotSaveException>(() => factory1.Save());

			var factory2 = new BusinessObjectFactory();
			var testObjectCreator2 = new TestObjectCreator(factory2);
			var invoiceInstance1InNewFactory = testObjectCreator2.CreateInvoiceWithLine(typeof(ARInvoiceToTestSecondInstance), "", testObjectCreator2.AUD, 1, 10, 0, 10, 0);
			var invoiceInstance2InNewFactory = factory2.Load<ARInvoice>(invoiceInstance1InNewFactory.PK);
			factory2.Save();
			AssertEquals("AH_TransactionNum", "00001001", invoiceInstance1InNewFactory.AH_TransactionNum);

			invoiceInstance1InNewFactory = testObjectCreator2.CreateInvoiceWithLine(typeof(ARInvoiceToTestSecondInstance), "", testObjectCreator2.AUD, 1, 10, 0, 10, 0);
			invoiceInstance2InNewFactory = factory2.Load<ARInvoice>(invoiceInstance1InNewFactory.PK);
			factory2.Save();
			AssertEquals("AH_TransactionNum", "00001002", invoiceInstance1InNewFactory.AH_TransactionNum);

			bizoInvalidForSaving.Delete();
			factory1.Save();
			AssertEquals("AH_TransactionNum", "00001003", invoiceInstance1.AH_TransactionNum);
		}

		public void TestDontSkipFountainNumbersForARInvoice_AfterOnSavedFailure()
		{
			var factory1 = new BusinessObjectFactory();
			var testObjectCreator1 = new TestObjectCreator(factory1);
			var invoiceInstance1 = testObjectCreator1.CreateInvoiceWithLine(typeof(ARInvoice), "", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			var invoiceInstance2 = factory1.Load<ARInvoiceToTestSecondInstance>(invoiceInstance1.PK);
			var bizoInvalidForSaving1 = factory1.New<BizoWithOnSavedExceptionSavedBeforeTrasnactionHeader>();
			bizoInvalidForSaving1.FillWithValidTestData();
			var bizoInvalidForSaving2 = factory1.New<BizoWithOnSavingExceptionSavedAfterTrasnactionHeader>();
			bizoInvalidForSaving2.SetValues(invoiceInstance1);
			bizoInvalidForSaving2.FillWithValidTestData();

			AssertExceptionThrown<ZCannotSaveException>("Precondition: the first saving is broken.", () => factory1.Save());
			ErrorReporter.Clear();

			Assert("Postcondition: invoiceInstance1.IsInDatabase", !invoiceInstance1.IsInDatabase);
			AssertEquals("Postcondition: AH_TransactionNum", string.Empty, invoiceInstance1.AH_TransactionNum);
			Assert("Postcondition: LastSavingRollbackHadException", ((IBusinessObjectFactoryInternals)factory1).LastSavingRollbackHadException);

			bizoInvalidForSaving1.Delete();
			bizoInvalidForSaving2.Delete();
			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("The second saving must be prevented as number fountain related logic may not be cleared properly as OnSaved may not be called.", () => factory1.Save());
		}

		[SuspendCriticalValidation]
		public void TestTransactionHeaderLogsMasterType()
		{
			var factory1 = new BusinessObjectFactory();
			var arInvoice = factory1.NewWithValidTestData<ARInvoice>();
			arInvoice.Logs.AddNew(new EventValue(Events.DocumentDelivered));
			var accTransactionHeader = factory1.NewWithValidTestData<AccTransactionHeader>();
			accTransactionHeader.AH_Ledger = LedgerTypes.CashBook;
			accTransactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			accTransactionHeader.Logs.AddNew(new EventValue(Events.DocumentDelivered));
			factory1.Save();

			var bizObjType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AccTransactionHeaderSchema.Constants.Prefix);
			AssertEquals(bizObjType, typeof(TransactionHeader));

			var factory2 = new BusinessObjectFactory();

			var log1 = factory2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, arInvoice.PK))[0];
			AssertEquals(typeof(ARInvoice), log1.Master.GetType());

			var log2 = factory2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, accTransactionHeader.PK))[0];
			AssertExceptionThrown(typeof(ArgumentException), () => { var logCount = log2.Master.Count; });

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertContains("Transaction Type 'INV' is not a valid CB Transaction Type", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			ExceptionReporterTestListener.Instance.Clear();

			var factory3 = new BusinessObjectFactory();
			var arInvoiceInFactory3 = factory3.Load<AccTransactionHeader>(arInvoice.PK);
			var transactionHeaderInFactory3 = factory3.Load<AccTransactionHeader>(accTransactionHeader.PK);

			AssertEquals(typeof(AccTransactionHeader), arInvoiceInFactory3.Logs.GetAllLogs()[0].Master.GetType());
			AssertEquals(typeof(AccTransactionHeader), transactionHeaderInFactory3.Logs.GetAllLogs()[0].Master.GetType());
		}

		protected class ARInvoiceToTestSecondInstance : ARInvoice
		{
			public ARInvoiceToTestSecondInstance(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class BizoWithOnSavingExceptionSavedAfterTrasnactionHeader : TransactionMatchLink
		{
			public BizoWithOnSavingExceptionSavedAfterTrasnactionHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void OnSavingCore()
			{
				throw new ZCannotSaveException("OnSaving failed", "BizoWithOnSavingExceptionSavedAfterTrasnactionHeader");
			}
		}

		class BizoWithOnSavedExceptionSavedBeforeTrasnactionHeader : AccBankAccount
		{
			public BizoWithOnSavedExceptionSavedBeforeTrasnactionHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaved(bool saveSucceeded)
			{
				throw new InvalidOperationException();
			}
		}
	}
}
