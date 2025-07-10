using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class BatchQueueInvoicesForEInvoicingProviderTest : TestCaseWithFactory
	{
		public void TestGetTransactionsToBeQueued()
		{
			SetupTestBase();

			var romaniaAccountingFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Romania);
			var instanceProvider = romaniaAccountingFactory as IInstanceProvider<IBatchQueueInvoicesForEInvoicingProvider>;
			AssertNotNull(instanceProvider);

			var provider = instanceProvider.Get();
			AssertNotNull(provider);

			var txnToBeQueued = provider.GetTransactionsToBeQueued(Factory, companyPK, startDate, 5);
			var actualResult = txnToBeQueued.Select(x => x.PK);
			AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
		}

		void SetupTestBase()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			startDate = DateTime.Now.AddDays(-30);
			var anotherBranch = testObjectCreator.CreateBranchWithCompany(Constants.CountryCodes.Australia);

			var testAR1 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.AdjustmentNote, false, startDate.AddDays(3));
			companyPK = testAR1.AH_GC;
			var testAR2 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.AdjustmentNote, true, startDate.AddDays(6));
			var testAR3 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.AdjustmentNote, false, startDate.AddDays(-5));
			var testAR4 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.AdjustmentNote, false, startDate.AddDays(3), anotherBranch);
			var testAP5 = CreateTestInvoice(testObjectCreator, typeof(APInvoice), TransactionTypes.AdjustmentNote, true, startDate.AddDays(11));

			var testAR6 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.CreditNote, false, startDate.AddDays(2));
			var testAR7 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.CreditNote, true, startDate.AddDays(7));
			var testAR8 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.CreditNote, false, startDate.AddDays(-11));
			var testAR9 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.CreditNote, false, startDate.AddDays(13), anotherBranch);
			var testAP10 = CreateTestInvoice(testObjectCreator, typeof(APInvoice), TransactionTypes.CreditNote, false, startDate.AddDays(23));

			var testAR11 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.Invoice, false, startDate.AddDays(8));
			var testAR12 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.Invoice, true, startDate.AddDays(16));
			var testAR13 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.Invoice, false, startDate.AddDays(-6));
			var testAR14 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.Invoice, false, startDate.AddDays(4), anotherBranch);
			var testAP15 = CreateTestInvoice(testObjectCreator, typeof(APInvoice), TransactionTypes.Invoice, false, startDate.AddDays(9));

			var testAR16 = CreateTestInvoice(testObjectCreator, typeof(ARInvoice), TransactionTypes.Journal, false, startDate.AddDays(19));
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testAR16.AH_AG = glHeader.PK;

			Factory.Save();

			expectedResult = new ZGuid[3] { testAR1.PK, testAR6.PK, testAR11.PK };
		}

		InvoicingBase CreateTestInvoice(TestObjectCreator testObjectCreator, Type invoiceType, string transactionType,
			bool needCreatePivot, DateTime postDate, GlbBranch branch = null)
		{
			var testInvoice = testObjectCreator.CreateInvoice(invoiceType);

			testInvoice.AH_TransactionType = transactionType;
			testInvoice.AH_PostDate = postDate;

			if (branch != null)
			{
				testInvoice.AH_GB = branch.PK;
			}
			if (needCreatePivot)
			{
				CreatePivot(testInvoice);
			}

			return testInvoice;
		}

		void CreatePivot(InvoicingBase testInvoice)
		{
			var eInvoicingTransactionPivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			var company = Factory.LoadFromUniqueKey<GlbCompany>(GlbCompanySchema.PK, testInvoice.AH_GC);
			eInvoicingTransactionPivot.SetCompanyAndCountryCode(company);
			eInvoicingTransactionPivot.AIP_ParentTableCode = "AH";
			eInvoicingTransactionPivot.AIP_ParentID = testInvoice.PK;
		}

		ZGuid[] expectedResult;
		ZGuid companyPK;
		DateTime startDate;
	}
}
