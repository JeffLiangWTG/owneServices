using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	[TestedType(typeof(AccGeneralLedgerData))]
	public class AccGeneralLedgerDataTest : EnterpriseBusinessObjectTestCase
	{
		protected AccGeneralLedgerData GetNewAccGeneralLedgerData()
		{
			var bizo = Factory.New<AccGeneralLedgerData>();
			return bizo;
		}

		public void TestLookups()
		{
			var bizo = GetNewAccGeneralLedgerData();
			AssertNotNull(bizo.Lookups);
			AssertEquals(typeof(AccGeneralLedgerDataLookups), bizo.Lookups.GetType());
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccGeneralLedgerData()
		{
			var bizo = Factory.NewWithValidTestData<AccGeneralLedgerData>();

			var localList = new List<string>
			{
				nameof(bizo.GLD_OSDebitAmount),
				nameof(bizo.GLD_OSCreditAmount),
				nameof(bizo.GLD_LocalDebitAmount),
				nameof(bizo.GLD_LocalCreditAmount),
			};

			var tester = new DecimalPlacesAttributeTester(bizo);
			tester.CheckLocalCurrency(localList, nameof(bizo.LocalDecimals));
		}

		public void TestExchangeRateDecimals()
		{
			var bizo = GetNewAccGeneralLedgerData();
			AssertEquals(6, bizo.ExchangeRateDecimalPlaces);
		}

		public void TestLedger()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = "WIP";
			accGeneralLedgerData.GLD_AL_TransactionLine = line.PK;

			AssertEquals("JC", accGeneralLedgerData.Ledger);

			line.AL_LineType = "ACR";
			AssertEquals("JC", accGeneralLedgerData.Ledger);

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = "AR";
			line.AL_AH = header.PK;
			line.AL_LineType = "CST";
			accGeneralLedgerData.GLD_AH_TransactionHeader = header.PK;

			AssertEquals("AR", accGeneralLedgerData.Ledger);
		}

		public void TestTransactionNum()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionNum = "123";

			AssertEquals(ZString.Empty, accGeneralLedgerData.TransactionNumber);

			accGeneralLedgerData.GLD_AH_TransactionHeader = header.PK;

			AssertEquals("123", accGeneralLedgerData.TransactionNumber);
		}

		public void TestDueDate()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_DueDate = ZDateTime.Today;

			AssertEquals(ZDateTime.Empty, accGeneralLedgerData.DueDate);

			accGeneralLedgerData.GLD_AH_TransactionHeader = header.PK;

			AssertEquals(ZDateTime.Today, accGeneralLedgerData.DueDate);
		}

		public void TestTransactionDate()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = "WIP";
			accGeneralLedgerData.GLD_AL_TransactionLine = line.PK;

			ZDateTime date1 = new ZDateTime(2023, 07, 30);
			ZDateTime date2 = new ZDateTime(2023, 07, 31);

			line.AL_PostDate = date1;
			AssertEquals(date1, accGeneralLedgerData.TransactionDate);

			line.AL_LineType = "ACR";
			AssertEquals(date1, accGeneralLedgerData.TransactionDate);

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceDate = date2;
			line.AL_AH = header.PK;
			line.AL_LineType = "CST";
			accGeneralLedgerData.GLD_AH_TransactionHeader = header.PK;
			AssertEquals(date2, accGeneralLedgerData.TransactionDate);
		}

		public void TestJobHeaderPK()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader1.JH_JobNum = "abcde";
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_JH = jobHeader1.PK;
			accGeneralLedgerData.GLD_AH_TransactionHeader = header.PK;

			AssertEquals(jobHeader1.PK, accGeneralLedgerData.JobHeaderPK);

			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader2.JH_JobNum = "12345";
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_JH = jobHeader2.PK;
			line.AL_LineType = "CST";
			accGeneralLedgerData.GLD_AL_TransactionLine = line.PK;

			AssertEquals(jobHeader1.PK, accGeneralLedgerData.JobHeaderPK);

			line.AL_LineType = "WIP";

			AssertEquals(jobHeader2.PK, accGeneralLedgerData.JobHeaderPK);

			line.AL_LineType = "ACR";

			AssertEquals(jobHeader2.PK, accGeneralLedgerData.JobHeaderPK);
		}

		public void TestChargeCodePK()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();

			AssertEquals(ZGuid.Empty, accGeneralLedgerData.ChargeCodePK);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AC = accChargeCode.PK;
			accGeneralLedgerData.GLD_AL_TransactionLine = line.PK;

			AssertEquals(accChargeCode.PK, accGeneralLedgerData.ChargeCodePK);
		}

		public void TestGLAccountDesc()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			AssertEquals(ZString.Empty, accGeneralLedgerData.GLAccountDesc);

			var accGlHeader = Factory.NewWithValidTestData<AccGLHeader>();
			accGlHeader.AG_Description = "ABC";

			accGeneralLedgerData.GLD_AG_GLAccount = accGlHeader.PK;

			AssertEquals("ABC", accGeneralLedgerData.GLAccountDesc);
		}

		[TestDate(2023, 5, 18)]
		public void TestSubAccount()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			var receipt = TestObjectCreator.CreateARReceipt(1m, 100m, ZDateTime.Today, ZDateTime.Today, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			receipt.AH_AG = TestObjectCreator.GLHeader1.PK;

			aRInvoice.Lines[0].AL_PostDate = ZDateTime.Now;
			aRInvoice.Lines[0].AL_ReverseDate = ZDateTime.Now;
			aRInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;

			TestObjectCreator.CreateTransactionLineSubAccount<AccTransactionLineSubAccount>(aRInvoice.Lines[0].PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.ABIGAS.PK);
			TestObjectCreator.CreateTransactionHeaderSubAccount(receipt.PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.AALSHI.PK);

			Factory.Save();

			((INeedRow)aRInvoice.Lines[0]).Row.SetAdded();
			((INeedRow)receipt).Row.SetAdded();

			TestObjectCreator.MockNudgeGLDProcessData([aRInvoice.Lines[0], receipt]);

			var accGeneralLedgerData1 = (AccGeneralLedgerData)Factory.Load(typeof(AccGeneralLedgerData), new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, aRInvoice.PK))[0];
			var accGeneralLedgerData2 = (AccGeneralLedgerData)Factory.Load(typeof(AccGeneralLedgerData), new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, receipt.PK))[0];

			AssertEquals(false, accGeneralLedgerData1.GLD_AH_TransactionHeader.IsEmpty);

			AssertEquals(false, accGeneralLedgerData1.GLD_AL_TransactionLine.IsEmpty);

			AssertEquals(true, accGeneralLedgerData2.GLD_AL_TransactionLine.IsEmpty);

			AssertEquals(false, accGeneralLedgerData2.GLD_AH_TransactionHeader.IsEmpty);

			AssertEquals("ORG: " + TestObjectCreator.ABIGAS.OH_Code, accGeneralLedgerData1.SubAccount);

			AssertEquals("ORG: " + TestObjectCreator.AALSHI.OH_Code, accGeneralLedgerData2.SubAccount);

			accGeneralLedgerData1.GLD_AG_GLAccount = ZGuid.Empty;

			accGeneralLedgerData2.GLD_AG_GLAccount = ZGuid.Empty;

			AssertEquals(ZString.Empty, accGeneralLedgerData1.SubAccount);

			AssertEquals(ZString.Empty, accGeneralLedgerData2.SubAccount);
		}

		public void TestHeaderDescription()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Desc = "Header Description";

			AssertEquals(ZString.Empty, accGeneralLedgerData.HeaderDescription);

			accGeneralLedgerData.GLD_AH_TransactionHeader = header.PK;

			AssertEquals("Header Description", accGeneralLedgerData.HeaderDescription);
		}

		public void TestLineDescription()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_Desc = "Line Description";

			AssertEquals(ZString.Empty, accGeneralLedgerData.LineDescription);

			accGeneralLedgerData.GLD_AL_TransactionLine = line.PK;

			AssertEquals("Line Description", accGeneralLedgerData.LineDescription);
		}

		public void TestPresentationJournalCategory()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionCategory = "STD";
			header.AH_Ledger = LedgerTypes.General;

			AssertEquals(ZString.Empty, accGeneralLedgerData.PresentationJournalCategory);

			accGeneralLedgerData.GLD_AH_TransactionHeader = header.PK;

			AssertEquals("STD", accGeneralLedgerData.PresentationJournalCategory);

			header.AH_Ledger = LedgerTypes.AccountsPayable;

			AssertEquals(ZString.Empty, accGeneralLedgerData.PresentationJournalCategory);
		}

		public void TestUnits()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			AssertEquals(ZString.Empty, accGeneralLedgerData.Units);

			var accGlHeader = Factory.NewWithValidTestData<AccGLHeader>();
			accGlHeader.AG_StatisticalUnits = "KWH";

			accGeneralLedgerData.GLD_AG_GLAccount = accGlHeader.PK;

			AssertEquals("KWH", accGeneralLedgerData.Units);
		}

		public void TestTransactionType()
		{
			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = "WIP";
			accGeneralLedgerData.GLD_AL_TransactionLine = line.PK;

			AssertEquals("WIP", accGeneralLedgerData.TransactionType);

			line.AL_LineType = "ACR";
			AssertEquals("ACR", accGeneralLedgerData.TransactionType);

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			line.AL_AH = header.PK;
			line.AL_LineType = "CST";
			accGeneralLedgerData.GLD_AH_TransactionHeader = header.PK;

			AssertEquals(header.AH_TransactionType, accGeneralLedgerData.TransactionType);
		}

		public void TestHumanReadableName()
		{
			var testBizo = Factory.NewWithValidTestData<AccGeneralLedgerDataForTest>();
			AssertEquals(testBizo.HumanReadableNameCoreForTest, testBizo.HumanReadableName);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;

		public void TestOrganizationPK()
		{
			var testBizo = Factory.NewWithValidTestData<AccGeneralLedgerData>();

			testBizo.GLD_AH_TransactionHeader = ZGuid.Empty;
			testBizo.GLD_AL_TransactionLine = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, testBizo.OrganizationPK);

			var transactionHeader1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader1.AH_OH = ZGuid.Empty;
			testBizo.GLD_AH_TransactionHeader = transactionHeader1.PK;

			AssertEquals(ZGuid.Empty, testBizo.OrganizationPK);

			var accTransactionLines = Factory.NewWithValidTestData<AccTransactionLines>();
			testBizo.GLD_AL_TransactionLine = accTransactionLines.PK;
			accTransactionLines.AL_OH = ZGuid.Empty;

			AssertEquals(ZGuid.Empty, testBizo.OrganizationPK);

			var orgHeader1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader1.AH_OH = orgHeader1.PK;

			AssertEquals(orgHeader1.PK, testBizo.OrganizationPK);

			testBizo = Factory.NewWithValidTestData<AccGeneralLedgerData>();

			var orgHeader2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			accTransactionLines.AL_OH = orgHeader2.PK;
			testBizo.GLD_AH_TransactionHeader = transactionHeader1.PK;
			testBizo.GLD_AL_TransactionLine = accTransactionLines.PK;

			AssertEquals(orgHeader1.PK, testBizo.OrganizationPK);

			testBizo = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			transactionHeader1.AH_OH = ZGuid.Empty;
			testBizo.GLD_AH_TransactionHeader = transactionHeader1.PK;
			testBizo.GLD_AL_TransactionLine = accTransactionLines.PK;

			AssertEquals(orgHeader2.PK, testBizo.OrganizationPK);

			testBizo = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			testBizo.GLD_AH_TransactionHeader = transactionHeader1.PK;
			testBizo.GLD_AL_TransactionLine = accTransactionLines.PK;
			transactionHeader1.AH_OH = orgHeader1.PK;
			var taxGLMovement = Factory.NewWithValidTestData<AccTaxGLMovement>();
			var transaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			var transactionHeader2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.ATT_AH = transactionHeader2.PK;
			transactionHeader2.AH_OH = ZGuid.Empty;
			taxGLMovement.ATM_ATT_TaxTransaction = transaction.PK;
			testBizo.GLD_ATM_TaxGLMovement = taxGLMovement.PK;

			AssertEquals(orgHeader2.PK, testBizo.OrganizationPK);

			testBizo = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			testBizo.GLD_AH_TransactionHeader = transactionHeader1.PK;
			testBizo.GLD_AL_TransactionLine = accTransactionLines.PK;
			testBizo.GLD_ATM_TaxGLMovement = taxGLMovement.PK;
			var orgHeader3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader2.AH_OH = orgHeader3.PK;
			AssertEquals(orgHeader3.PK, testBizo.OrganizationPK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizo = factory.NewWithValidTestData<AccGeneralLedgerData>();
			bizo.GLD_Type = "PST";
			bizo.GLD_GLAccountType = "ARC";
			bizo.GLD_PostDate = ZDateTime.Today;
			bizo.GLD_PostPeriod = 1;
			bizo.GLD_GC_Company = GlbCompany.CurrentCompany.PK;
			bizo.GLD_GB_Branch = GlbBranch.CurrentBranch.PK;
			bizo.GLD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			return bizo;
		}

		class AccGeneralLedgerDataForTest : AccGeneralLedgerData
		{
			public AccGeneralLedgerDataForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString HumanReadableNameCoreForTest => HumanReadableNameCore;
		}
	}
}
