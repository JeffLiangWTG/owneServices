using System;
using System.Collections;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccGeneralLedgerDataFilterBusinessObject))]
	class AccGeneralLedgerDataFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccGeneralLedgerDataFilterBusinessObject();
		}

		public void TestFilter()
		{
			var generalLedgerDatas = Factory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals("Expecting collection 2 record", 2, generalLedgerDatas.Length);
			AssertEquals("Expecting collection 1 CurrentCompany record", 1, generalLedgerDatas.Where(p => p.GLD_GC_Company == Env.CurrentCompany.PK).Count());

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			AssertEquals("Expecting collection has CurrentCompany record.", Env.CurrentCompany.PK, TestCollection[0].Company.PK);
		}

		public void TestGLAccountFilter()
		{
			var filter = (AccGLHeaderRangeFilter)FilterBO["GL Account"];
			AssertNotNull(filter);
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property1 = "A";
			filter.Property2 = "C";
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property1 = "A";
			filter.Property2 = "A";
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);
		}

		public void TestEmptyGLAccountFilter()
		{
			var filter = (AccGLHeaderRangeFilter)FilterBO["GL Account"];
			AssertNotNull(filter);
			filter.IsActive = true;

			filter.Property1 = "A";
			filter.Property2 = "";
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property1 = "";
			filter.Property2 = "C";
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
		}

		public void TestGLDTypeFilter()
		{
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			var filter = (ModuleTextFilter)FilterBO["Journal Entries Type"];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "CBV";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.Property = "PST";
			AssertNoErrors(filter.PropertyInfo);

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "REC";
			AssertNoErrors(filter.PropertyInfo);

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);
		}

		public void TestGLDJournalEntriesNumberFilter()
		{
			string entries = "100009";

			var filter = (ModuleNumberFilter)FilterBO["Journal Entries Number"];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = entries;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
			Assert($@"Expecting collection only contains entries Journal Entries Number equals '{entries}'", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_JournalEntriesNumber == entries));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "2";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "9";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = entries;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "0";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "20";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
			Assert($@"Expecting collection only contains entries Journal Entries Number equals '{entries}'", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_JournalEntriesNumber == entries));
		}

		[TestDate(2004, 4, 1)]
		public void TestOrganisationFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			var accTransactionLines = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionHeader.AH_OH = orgHeader.PK;
			accTransactionLines.AL_OH = orgHeader.PK;

			var glbCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			var testBizos = Factory.Load<AccGeneralLedgerData>(new ZQuery());

			testBizos[0].GLD_GC_Company = glbCompany.PK;
			testBizos[1].GLD_GC_Company = glbCompany.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO["Organisation"];
			AssertNotNull(filter);
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.Property = orgHeader.PK;
			AssertNoErrors(filter.PropertyInfo);
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			testBizos[0].GLD_AH_TransactionHeader = transactionHeader.PK;
			Factory.Save();

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record which TransactionHeader Organisation is hited", 1, TestCollection.Count);

			testBizos[0].GLD_AH_TransactionHeader = ZGuid.Empty;
			testBizos[0].GLD_AL_TransactionLine = accTransactionLines.PK;
			Factory.Save();

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record which TransactionLine Organisation is hited", 1, TestCollection.Count);

			var taxGLMovement = GetAccTaxGLMovementForTest(Factory, 110m, orgHeader);

			testBizos[0].GLD_ATM_TaxGLMovement = taxGLMovement.PK;
			testBizos[0].GLD_AH_TransactionHeader = ZGuid.Empty;
			testBizos[0].GLD_AL_TransactionLine = ZGuid.Empty;
			Factory.Save();

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record which TaxGLMovement Organisation is hited", 1, TestCollection.Count);
		}

		public void TestBranchFilter()
		{
			var filter = (ModuleGuidFilter)FilterBO["Branch"];
			AssertNotNull(filter);
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = GlbBranch1.PK;
			AssertNoErrors(filter.PropertyInfo);

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = GlbBranch2.PK;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.Validation.ValidateComparisonOperator();
			Assert(filter.ComparisonOperatorInfo.HasErrors());

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.Validation.ValidateComparisonOperator();
			Assert(filter.ComparisonOperatorInfo.HasWarnings());
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
		}

		public void TestViewJournalEntriesNotRelatedToLoginBranch()
		{
			Env.Security.AccountingJournalsViewJournalEntriesNotRelatedToLoginBranch.IsAllowed = false;
			var branchFilter = (ModuleGuidFilter)FilterBO.GetModuleFiltersCore_ForTestOnly()["Branch"];

			AssertEquals("Should be read only", true, branchFilter.ReadOnly);
			AssertEquals("Should be always visible", FilterVisibility.AlwaysVisible, branchFilter.Visibility);
			AssertEquals("Should have the default value as current login branch", GlbBranch.CurrentBranch.PK, branchFilter.Property);

			Env.Security.AccountingJournalsViewJournalEntriesNotRelatedToLoginBranch.IsAllowed = true;
			branchFilter = (ModuleGuidFilter)FilterBO.GetModuleFiltersCore_ForTestOnly()["Branch"];

			AssertEquals("Should not be read only", false, branchFilter.ReadOnly);
			AssertNotEquals("Should not be always visible", FilterVisibility.AlwaysVisible, branchFilter.Visibility);
			AssertNotEquals("Should not have the default value as current login branch", GlbBranch.CurrentBranch.PK, branchFilter.Property);
		}

		public void TestTaxBranchFilter()
		{
			var filter = (ModuleGuidFilter)FilterBO["TaxBranch"];
			AssertNotNull(filter);
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = GlbBranch1.PK;
			AssertNoErrors(filter.PropertyInfo);

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = GlbBranch2.PK;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.Validation.ValidateComparisonOperator();
			Assert(!filter.ComparisonOperatorInfo.HasErrors());
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.Validation.ValidateComparisonOperator();

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
		}

		public void TestDepartmentFilter()
		{
			var filter = (ModuleGuidFilter)FilterBO["Department"];
			AssertNotNull(filter);
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = GlbDepartment1.PK;
			AssertNoErrors(filter.PropertyInfo);

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = GlbDepartment2.PK;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.Validation.ValidateComparisonOperator();
			Assert(filter.ComparisonOperatorInfo.HasErrors());

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.Validation.ValidateComparisonOperator();
			Assert(filter.ComparisonOperatorInfo.HasWarnings());
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
		}

		public void TestAccountingPeriodFilter()
		{
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			var startDateOfFinancialYear = new ZDateTime("2023-01-01 00:00:00");
			var periodManager = TestObjectCreator.CreateTestPeriods(startDateOfFinancialYear);

			Factory.Save();

			Assert("Expecting collection to contain twelve items", periodManager.Periods.Count == 12);

			var filter = (ModuleTextRangeFilter)FilterBO["Accounting Period"];

			filter.Property1 = "202301";
			filter.Property2 = "202303";
			filter.IsActive = true;

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property2 = "202301";
			filter.IsActive = true;

			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);
		}

		public void TestPostDateFilter()
		{
			TestDateFilter("PostDate");
		}

		void TestDateFilter(string filterName)
		{
			var filter = (ModuleDateFilter)FilterBO[filterName];
			AssertNotNull(filter);
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property1 = new ZDateTime("2022-02-07 09:28:00");
			AssertNoErrors(filter.Property1Info);

			filter.Property2 = new ZDateTime("2024-02-07 09:28:00");
			AssertNoErrors(filter.Property2Info);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property1 = filter.Property2;
			AssertNoErrors(filter.Property1Info);
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			TestCollection.Load(FilterBO.Filter);

			if (filter.FilterColumn.IsNullable)
			{
				AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);
			}
			else
			{
				AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
			}

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
		}

		public void TestTransactionDateFilterHelper(ModuleDateFilter filter, ZDateTime startDate, ZDateTime endDate, ZInt expectedRecordsNum, AccTransactionHeader singleHeaderFrom = null, AccTransactionLines singleLineFrom = null)
		{
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = startDate;
			filter.Property2 = endDate;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals($"Expecting collection has {expectedRecordsNum} records", expectedRecordsNum, TestCollection.Count);

			if (singleHeaderFrom != null)
			{
				Assert("Expecting all collection records are linked to the transaction header from which it generated", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == singleHeaderFrom.PK));
			}

			if (singleLineFrom != null)
			{
				Assert("Expecting all collection records are linked to the transaction line from which it generated", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AL_TransactionLine == singleLineFrom.PK));
			}
		}

		public void TestTransactionDateFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var transaction1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			var transaction2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "2100", TestObjectCreator.AUD, 1M, -10M, -10M, -10M, -10M);
			transaction1.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction2.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction1.AH_InvoiceDate = new ZDateTime(2023, 7, 15, 22, 0, 0);
			transaction2.AH_InvoiceDate = new ZDateTime(2023, 8, 15, 22, 0, 0);
			transaction1.AH_PostDate = new ZDateTime(2023, 7, 10, 22, 0, 0);
			transaction2.AH_PostDate = new ZDateTime(2023, 7, 10, 22, 0, 0);
			Factory.Save();

			ProcessGeneralLedgerData(transaction1.Lines[0]);
			ProcessGeneralLedgerData(transaction2.Lines[0]);

			var transactionDateFilter = (ModuleDateFilter)FilterBO["Transaction Date"];

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 7, 10), new ZDateTime(2023, 7, 10), 0);

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 7, 15), new ZDateTime(2023, 8, 15), 12);

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 7, 20), new ZDateTime(2023, 8, 10), 0);

			TestTransactionDateFilterHelper(transactionDateFilter, ZDateTime.Empty, new ZDateTime(2023, 7, 15), 6, transaction1);

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 8, 15), ZDateTime.Empty, 6, transaction2);

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 8, 15), new ZDateTime(2023, 8, 15), 6, transaction2);
		}

		public void TestTransactionDateFilterForWIPAndAccrual()
		{
			SetUpForTestUsingTestObjectCreator();

			var lineForWIP = TestObjectCreator.CreateWIP();
			lineForWIP.AL_PostDate = new ZDateTime(2023, 8, 20, 22, 0, 0);

			var lineForAccrual = TestObjectCreator.CreateAccrual();
			lineForAccrual.AL_PostDate = new ZDateTime(2023, 8, 25, 22, 0, 0);

			Factory.Save();

			ProcessGeneralLedgerData(lineForWIP);
			ProcessGeneralLedgerData(lineForAccrual);

			var transactionDateFilter = (ModuleDateFilter)FilterBO["Transaction Date"];

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 8, 10), new ZDateTime(2023, 8, 10), 0);

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 8, 15), new ZDateTime(2023, 8, 30), 4);

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 8, 21), new ZDateTime(2023, 8, 24), 0);

			TestTransactionDateFilterHelper(transactionDateFilter, ZDateTime.Empty, new ZDateTime(2023, 8, 20), 2, null, lineForWIP);

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 8, 25), ZDateTime.Empty, 2, null, lineForAccrual);
		}

		[TestDate(2023, 8, 20)]
		public void TestTransactionDateFilterForTaxTransaction()
		{
			SetUpForTestUsingTestObjectCreator();

			var taxGLMovement = CreateAccTaxGLMovement(110m, new ZDateTime(2023, 8, 15));
			ProcessGeneralLedgerData(taxGLMovement);

			TestCollection.Load();

			var transactionDateFilter = (ModuleDateFilter)FilterBO["Transaction Date"];

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 8, 15), new ZDateTime(2023, 8, 15), 2);

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 8, 16), ZDateTime.Empty, 0);

			TestTransactionDateFilterHelper(transactionDateFilter, ZDateTime.Empty, new ZDateTime(2023, 8, 14), 0);

			TestTransactionDateFilterHelper(transactionDateFilter, new ZDateTime(2023, 8, 20), new ZDateTime(2023, 8, 20), 0);
		}

		void TestAmountFilter(ZString amountFilterName)
		{
			SetUpForTestUsingTestObjectCreator();

			var osAmount = 100M;
			var localAmount = 20M;
			var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.CNY, 5M, osAmount, 0M, localAmount, 0M);
			transaction.AH_OH = TestObjectCreator.ABIGAS.PK;

			Factory.Save();

			ProcessGeneralLedgerData(transaction.Lines[0]);

			var filter = (ModuleNumberRangeFilter)FilterBO[amountFilterName];

			var testAmount = amountFilterName == "OS Debit Amount" || amountFilterName == "OS Credit Amount" ? osAmount : localAmount;

			filter.Property1 = testAmount;
			filter.Property2 = testAmount;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection has 2 record", 2, TestCollection.Count);
			switch (amountFilterName)
			{
				case "OS Debit Amount":
					Assert("Expecting collection contains only Debit entries", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_OSDebitAmount > 0.0M));
					break;
				case "OS Credit Amount":
					Assert("Expecting collection contains only Credit entries", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_OSCreditAmount > 0.0M));
					break;
				case "Local Debit Amount":
					Assert("Expecting collection contains only Debit entries", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_LocalDebitAmount > 0.0M));
					break;
				case "Local Credit Amount":
					Assert("Expecting collection contains only Credit entries", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_LocalCreditAmount > 0.0M));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(amountFilterName), "No such a subAccountFilter name");
			}

			filter.Property1 = 0.0;
			filter.Property2 = 200.0;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection has 4 record", 4, TestCollection.Count);

			filter.Property1 = 5.0;
			filter.Property2 = 15.0;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection has 0 record", 0, TestCollection.Count);

			filter.Property1 = 25.0;
			filter.Property2 = 95.0;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection has 0 record", 0, TestCollection.Count);

			filter.Property1 = 110.0;
			filter.Property2 = 200.0;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection has 0 record", 0, TestCollection.Count);
		}

		public void TestOSDebitAmountFilter()
		{
			TestAmountFilter("OS Debit Amount");
		}

		public void TestOSCreditAmountFilter()
		{
			TestAmountFilter("OS Credit Amount");
		}

		public void TestLocalDebitAmountFilter()
		{
			TestAmountFilter("Local Debit Amount");
		}

		public void TestLocalCreditAmountFilter()
		{
			TestAmountFilter("Local Credit Amount");
		}

		public void TestLedgerFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var transaction1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var transaction2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "2100", TestObjectCreator.AUD, 1M, -10M, -0M, -10M, -0M);
			transaction1.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction2.AH_OH = TestObjectCreator.ABIGAS.PK;

			Factory.Save();

			ProcessGeneralLedgerData(transaction1.Lines[0]);
			ProcessGeneralLedgerData(transaction2.Lines[0]);

			var filter = (DependentListFilter)FilterBO["Ledger/Transaction Type"];

			filter.Property1 = LedgerTypes.AccountsReceivable;
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 4 records", 4, TestCollection.Count);
			Assert("Expecting collection only contains records from transaction1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == transaction1.PK));

			filter.Property1 = LedgerTypes.AccountsPayable;
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 4 records", 4, TestCollection.Count);
			Assert("Expecting collection only contains records from transaction2", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == transaction2.PK));

			filter.Property1 = LedgerTypes.General;
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);
		}

		public void TestLedgerFilterForJobCosting()
		{
			SetUpForTestUsingTestObjectCreator();

			var lineForWIP = TestObjectCreator.CreateWIP();
			var lineForAccrual = TestObjectCreator.CreateAccrual();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);

			Factory.Save();

			ProcessGeneralLedgerData(lineForWIP);
			ProcessGeneralLedgerData(lineForAccrual);
			ProcessGeneralLedgerData(jobRevenueJournal.Lines[0]);
			ProcessGeneralLedgerData(jobRevenueJournal.Lines[1]);

			var filter = (DependentListFilter)FilterBO["Ledger/Transaction Type"];

			filter.Property1 = LedgerTypes.JobCosting;
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 12(2 + 2 + 8) records", 12, TestCollection.Count);
		}

		public void TestTransactionTypeFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var transaction1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var transaction2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var transaction3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "2100", TestObjectCreator.AUD, 1M, -10M, -0M, -10M, -0M);

			transaction1.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction2.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction3.AH_OH = TestObjectCreator.ABIGAS.PK;

			Factory.Save();

			ProcessGeneralLedgerData(transaction1.Lines[0]);
			ProcessGeneralLedgerData(transaction2.Lines[0]);
			ProcessGeneralLedgerData(transaction3.Lines[0]);

			var filter = (DependentListFilter)FilterBO["Ledger/Transaction Type"];

			filter.Property1 = LedgerTypes.AccountsReceivable;
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 8(4+4+0) records", 8, TestCollection.Count);

			filter.Property1 = LedgerTypes.AccountsPayable;
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 4(0+0+4) records", 4, TestCollection.Count);

			filter.Property1 = ZString.Empty;
			filter.Property2 = TransactionTypes.Invoice;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 12(4+4+4) records", 12, TestCollection.Count);

			filter.Property1 = ZString.Empty;
			filter.Property2 = TransactionTypes.Payment;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 0(0+0+0) records", 0, TestCollection.Count);
		}

		public void TestTransactionTypeFilterForJobCosting()
		{
			SetUpForTestUsingTestObjectCreator();

			var lineForWIP = TestObjectCreator.CreateWIP();
			var lineForAccrual = TestObjectCreator.CreateAccrual();

			Factory.Save();

			ProcessGeneralLedgerData(lineForWIP);
			ProcessGeneralLedgerData(lineForAccrual);

			var filter = (DependentListFilter)FilterBO["Ledger/Transaction Type"];

			filter.Property1 = LedgerTypes.JobCosting;
			filter.Property2 = TransactionLineTypes.WIP;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 2(2 + 0) records", 2, TestCollection.Count);
			Assert("Expecting collection only contains records from WIP line", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AL_TransactionLine == lineForWIP.PK));

			filter.Property1 = ZString.Empty;
			filter.Property2 = TransactionLineTypes.WIP;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 2(2 + 0) records", 2, TestCollection.Count);
			Assert("Expecting collection only contains records from WIP line", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AL_TransactionLine == lineForWIP.PK));

			filter.Property1 = ZString.Empty;
			filter.Property2 = TransactionLineTypes.Accrual;
			filter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);

			AssertEquals("Expecting collection contains 2(0 + 2) records", 2, TestCollection.Count);
			Assert("Expecting collection only contains records from Accrual line", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AL_TransactionLine == lineForAccrual.PK));
		}

		void createPresentationCategory(GLPresentationJournalCategory category, ZString categoryCode, ZString categoryDescription)
		{
			category.Code = categoryCode;
			category.Description = (NoResString)categoryDescription;
			category.Bool = true; // active
			category.Bool2 = false; // elimination
			category.Bool3 = false; // closing
			category.Bool4 = false; // opening
		}

		public void TestPresentationCategoryFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var list = new GLPresentationJournalCategoryCollection();

			var category1 = list.AddNew();
			createPresentationCategory(category1, "ELM", "Category ELM");
			category1.Bool2 = true;

			var category2 = list.AddNew();
			createPresentationCategory(category2, "AAA", "Category AAA");

			var category3 = list.AddNew();
			createPresentationCategory(category3, "BBB", "Category BBB");

			using (AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				var glHeader = TestObjectCreator.CreateGLHeader("TestGLAcc");

				var journal1 = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
				journal1.AH_TransactionCategory = category1.Code;
				TestObjectCreator.CreateGLJournalLine(journal1, 10M, DebitCredit.DR, glHeader.PK);
				TestObjectCreator.CreateGLJournalLine(journal1, 10M, DebitCredit.CR, glHeader.PK);

				var journal2 = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
				journal2.AH_TransactionCategory = category2.Code;
				TestObjectCreator.CreateGLJournalLine(journal2, 10M, DebitCredit.DR, glHeader.PK);
				TestObjectCreator.CreateGLJournalLine(journal2, 10M, DebitCredit.CR, glHeader.PK);

				var journal3 = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
				journal3.AH_TransactionCategory = string.Empty;
				TestObjectCreator.CreateGLJournalLine(journal3, 10M, DebitCredit.DR, glHeader.PK);
				TestObjectCreator.CreateGLJournalLine(journal3, 10M, DebitCredit.CR, glHeader.PK);

				Factory.Save();

				ProcessGeneralLedgerData(journal1.Lines[0]);
				ProcessGeneralLedgerData(journal1.Lines[1]);
				ProcessGeneralLedgerData(journal2.Lines[0]);
				ProcessGeneralLedgerData(journal2.Lines[1]);
				ProcessGeneralLedgerData(journal3.Lines[0]);
				ProcessGeneralLedgerData(journal3.Lines[1]);

				var filter = (ModuleTextFilter)FilterBO["Presentation Category"];
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"",
					"exact",
					"starts with",
					"contains",
					"not equal",
					"not starting",
					"not contain",
					"is blank",
					"is not blank"
				}, filter.AllowedComparisonOperators);

				filter.Property = category1.Code;
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				TestCollection.Load(FilterBO.Filter);

				AssertEquals("Expecting collection contains 2(2 + 0) records", 2, TestCollection.Count);
				Assert("Expecting collection only contains records from journal", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == journal1.PK));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				TestCollection.Load(FilterBO.Filter);

				AssertEquals("Expecting collection contains 2 records", 2, TestCollection.Count);

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				TestCollection.Load(FilterBO.Filter);

				AssertEquals("Expecting collection contains 2(2 + 2) records", 4, TestCollection.Count);
				AssertEquals("Expecting collection contains 2 records from journal1", 2, TestCollection.Count(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == journal1.PK));
				AssertEquals("Expecting collection contains 2 records from journal2", 2, TestCollection.Count(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == journal2.PK));

				filter.Property = category3.Code;
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				TestCollection.Load(FilterBO.Filter);

				AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);
			}
		}

		public void TestPresentationCategoryFilterForLedgerNotGL()
		{
			SetUpForTestUsingTestObjectCreator();

			var list = new GLPresentationJournalCategoryCollection();

			var categorySTD = list.AddNew();
			createPresentationCategory(categorySTD, "STD", "coincidentally has the same name as the existing category STD");

			using (AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				var transaction1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
				transaction1.AH_OH = TestObjectCreator.ABIGAS.PK;

				Factory.Save();

				ProcessGeneralLedgerData(transaction1.Lines[0]);
				TestCollection.Load(FilterBO.Filter);

				var filter = (ModuleTextFilter)FilterBO["Presentation Category"];

				filter.Property = categorySTD.Code;
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				TestCollection.Load(FilterBO.Filter);

				AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);
			}
		}

		public void TestPresentationCategoryFilterInvalidCode()
		{
			SetUpForTestUsingTestObjectCreator();

			var list = new GLPresentationJournalCategoryCollection();

			var category = list.AddNew();
			createPresentationCategory(category, "ELM", "Category ELM");
			category.Bool2 = true;

			using (AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				var filter = (ModuleTextFilter)FilterBO["Presentation Category"];
				filter.Property = "X";
				filter.IsActive = true;

				var warningComparisons = new[]
				{
					SQLComparisonOperator.StartsWith,
					SQLComparisonOperator.Contains,
					SQLComparisonOperator.DoesNotStartWith,
					SQLComparisonOperator.NotContains
				};

				foreach (var comparison in warningComparisons)
				{
					filter.SqlComparisonOperator = comparison;
					filter.Validation.ValidateProperty();
					Assert(filter.PropertyInfo.HasWarning(ListValidation.InvalidCodeMessage.ToString()));
				}

				var errorComparisons = new[]
				{
					SQLComparisonOperator.Equal,
					SQLComparisonOperator.NotEqual,
				};

				foreach (var comparison in errorComparisons)
				{
					filter.SqlComparisonOperator = comparison;
					filter.Validation.ValidateProperty();
					Assert(filter.PropertyInfo.HasError("Enter a valid selection."));
				}
			}
		}

		public void TestSubAccountFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var orgHeader = TestObjectCreator.CreateOrgHeader("TSTORG1", false, false);
			var staffGroup = TestObjectCreator.CreateStaffGroup("TST");
			var staffGroup2 = TestObjectCreator.CreateStaffGroup("TST2");
			var glAccount1 = TestObjectCreator.CreateGLHeader();
			var glAccount2 = TestObjectCreator.CreateGLHeader();

			TestObjectCreator.CreateGLHeaderSubAccount(glAccount1, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glAccount1, AccGroupsSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(glAccount1, GlbStaffSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(glAccount1, GlbGroupSchema.Constants.Prefix, true);

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AA1", TestObjectCreator.AUD, 1M, 50M, 5M, 50M, 5M);
			arInvoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			AssertEquals("Precondition", 1, arInvoice1.Lines.Count);
			var arLine1 = arInvoice1.Lines[0];
			arLine1.AL_AG = glAccount1.PK;
			TestObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(arLine1.PK, Core.Constants.SubAccountType.Organization, orgHeader.PK);
			TestObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(arLine1.PK, Core.Constants.SubAccountType.StaffGroup, staffGroup.PK);

			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AA2", TestObjectCreator.AUD, 1M, 40M, 4M, 40M, 4M);
			arInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			AssertEquals("Precondition", 1, arInvoice2.Lines.Count);
			var arLine2 = arInvoice2.Lines[0];
			arLine2.AL_AG = glAccount1.PK;

			var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AA3", TestObjectCreator.AUD, 1M, 30M, 3M, 30M, 3M);
			arInvoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			AssertEquals("Precondition", 1, arInvoice3.Lines.Count);
			var arLine3 = arInvoice3.Lines[0];
			arLine3.AL_AG = glAccount2.PK;

			Factory.Save();

			ProcessGeneralLedgerData(arLine1);
			ProcessGeneralLedgerData(arLine2);
			ProcessGeneralLedgerData(arLine3);

			var subAccountFilter = (SubAccountFilter)FilterBO["Sub Account + Value"];

			subAccountFilter.SubAccountType = Core.Constants.SubAccountType.Organization;
			subAccountFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			Assert("Expecting all records are related to glAccount1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AG_GLAccount == glAccount1.PK));
			Assert("Expecting collection to contain invoice1", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice3.PK));

			subAccountFilter.SubAccountType = Core.Constants.SubAccountType.StaffGroup;
			subAccountFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2(1+1+0) records", 2, TestCollection.Count);
			Assert("Expecting all records are related to glAccount1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AG_GLAccount == glAccount1.PK));
			Assert("Expecting collection to contain invoice1", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice3.PK));

			subAccountFilter.SubAccountType = Core.Constants.SubAccountType.Organization;
			subAccountFilter.SubAccount = orgHeader.PK;
			subAccountFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 1(1+0+0) records", 1, TestCollection.Count);
			Assert("Expecting the record is related to glAccount1", TestCollection[0].GLD_AG_GLAccount == glAccount1.PK);
			Assert("Expecting collection to contain invoice1", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice1.PK));
			Assert("Expecting collection not to contain invoice2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice3.PK));

			subAccountFilter.SubAccountType = Core.Constants.SubAccountType.StaffGroup;
			subAccountFilter.SubAccount = staffGroup.PK;
			subAccountFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 1(1+0+0) records", 1, TestCollection.Count);
			Assert("Expecting the record is related to glAccount1", TestCollection[0].GLD_AG_GLAccount == glAccount1.PK);
			Assert("Expecting collection to contain invoice1", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice1.PK));
			Assert("Expecting collection not to contain invoice2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice3.PK));

			subAccountFilter.SubAccountType = Core.Constants.SubAccountType.StaffGroup;
			subAccountFilter.SubAccount = staffGroup2.PK;
			subAccountFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);

			subAccountFilter.IsActive = false;

			var subAccountWithoutValueFilter = (ModuleTextFilter)FilterBO["Sub Account + No Value"];

			subAccountWithoutValueFilter.Property = Core.Constants.SubAccountType.Organization;
			subAccountWithoutValueFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 1(0+1+0) records", 1, TestCollection.Count);
			Assert("Expecting the record is related to glAccount1", TestCollection[0].GLD_AG_GLAccount == glAccount1.PK);
			Assert("Expecting collection not to contain invoice1", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice3.PK));

			subAccountWithoutValueFilter.Property = Core.Constants.SubAccountType.StaffGroup;
			subAccountWithoutValueFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 1(0+1+0) records", 1, TestCollection.Count);
			Assert("Expecting the record is related to glAccount1", TestCollection[0].GLD_AG_GLAccount == glAccount1.PK);
			Assert("Expecting collection not to contain invoice1", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice3.PK));

			subAccountWithoutValueFilter.Property = Core.Constants.SubAccountType.StaffAndResources;
			subAccountWithoutValueFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 1(1+1+0) records", 2, TestCollection.Count);
			Assert("Expecting the record is related to glAccount1", TestCollection[0].GLD_AG_GLAccount == glAccount1.PK);
			Assert("Expecting collection to contain invoice1", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice3.PK));

			subAccountWithoutValueFilter.Property = Core.Constants.SubAccountType.SalesGroup;
			subAccountWithoutValueFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 1(1+1+0) records", 2, TestCollection.Count);
			Assert("Expecting the record is related to glAccount1", TestCollection[0].GLD_AG_GLAccount == glAccount1.PK);
			Assert("Expecting collection to contain invoice1", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != arInvoice3.PK));
		}

		public void TestSubAccountFilterWithValueForBothHeaderAndLine()
		{
			SetUpForTestUsingTestObjectCreator();

			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, false);
			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, false);
			var glAccount1 = TestObjectCreator.CreateGLHeader();

			var arJournal = TestObjectCreator.CreateJournal<ARJournal>(100m, ZDate.Today, TestObjectCreator.ABIGAS.PK);
			arJournal.AH_AG = glAccount1.PK;
			TestObjectCreator.CreateTransactionHeaderSubAccount(arJournal.PK, Core.Constants.SubAccountType.Organization, orgHeader1.PK);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AA", TestObjectCreator.AUD, 1M, 50M, 0M, 50M, 0M);
			arInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			var arInvoiceLine = arInvoice.Lines[0];
			arInvoiceLine.AL_AG = glAccount1.PK;
			TestObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(arInvoiceLine.PK, Core.Constants.SubAccountType.Organization, orgHeader1.PK);

			Factory.Save();

			ProcessGeneralLedgerData(arJournal);
			ProcessGeneralLedgerData(arInvoiceLine);

			var subAccountFilter = (SubAccountFilter)FilterBO["Sub Account + Value"];

			subAccountFilter.SubAccountType = Core.Constants.SubAccountType.Organization;
			subAccountFilter.SubAccount = orgHeader1.PK;
			subAccountFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2(arJournal:1 + arInvoice:1) record", 2, TestCollection.Count);

			subAccountFilter.SubAccountType = Core.Constants.SubAccountType.Organization;
			subAccountFilter.SubAccount = orgHeader2.PK;
			subAccountFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 0 record", 0, TestCollection.Count);
		}

		public void TestSubAccountFilterWithValueForDoubleLines()
		{
			SetUpForTestUsingTestObjectCreator();

			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, false);
			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, false);
			var glAccount1 = TestObjectCreator.CreateGLHeader();
			var glAccount2 = TestObjectCreator.CreateGLHeader();

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10M, DebitCredit.DR, glAccount1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10M, DebitCredit.CR, glAccount2.PK);

			var journalLine1 = journal.Lines[0];
			var journalLine2 = journal.Lines[1];

			TestObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(journalLine1.PK, Core.Constants.SubAccountType.Organization, orgHeader1.PK);
			TestObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(journalLine2.PK, Core.Constants.SubAccountType.Organization, orgHeader1.PK);

			Factory.Save();

			ProcessGeneralLedgerData(journalLine1);
			ProcessGeneralLedgerData(journalLine2);

			var subAccountFilter = (SubAccountFilter)FilterBO["Sub Account + Value"];

			subAccountFilter.SubAccountType = Core.Constants.SubAccountType.Organization;
			subAccountFilter.SubAccount = orgHeader1.PK;
			subAccountFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2 record", 2, TestCollection.Count);

			subAccountFilter.SubAccountType = Core.Constants.SubAccountType.Organization;
			subAccountFilter.SubAccount = orgHeader2.PK;
			subAccountFilter.IsActive = true;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 0 record", 0, TestCollection.Count);
		}

		public void TestTransactionNumberFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var transaction1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "A00001", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			var transaction2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "A00002", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			var transaction3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "B00001", TestObjectCreator.AUD, 1M, -10M, -10M, -10M, -10M);
			transaction1.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction2.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction3.AH_OH = TestObjectCreator.ABIGAS.PK;

			Factory.Save();

			ProcessGeneralLedgerData(transaction1.Lines[0]);
			ProcessGeneralLedgerData(transaction2.Lines[0]);
			ProcessGeneralLedgerData(transaction3.Lines[0]);

			var filter = (ModuleTextFilter)FilterBO["Transaction Number"];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "A00001";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 6 records", 6, TestCollection.Count);
			Assert("Expecting collection only contains entries from transaction1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == transaction1.PK));

			filter.Property = "XXX";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 12 (6+6+0) records", 12, TestCollection.Count);
			Assert("Expecting collection Expecting collection not to contain transaction3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != transaction3.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "1";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 12 (6+0+6) records", 12, TestCollection.Count);
			Assert("Expecting collection Expecting collection not to contain transaction2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != transaction2.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains all 18 records", 18, TestCollection.Count);
		}

		public void TestTransactionNumberFilterForTaxGLMovement()
		{
			SetUpForTestUsingTestObjectCreator();

			var taxGLMovement1 = CreateAccTaxGLMovement(110m, ZDateTime.Today);
			var taxGLMovement2 = CreateAccTaxGLMovement(110m, ZDateTime.Today);

			ProcessGeneralLedgerData(taxGLMovement1);
			ProcessGeneralLedgerData(taxGLMovement2);

			var filter = (ModuleTextFilter)FilterBO["Transaction Number"];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "00001000";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2(2+0) records", 2, TestCollection.Count);
			Assert("Expecting collection only contains entries from taxGLMovement1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_ATM_TaxGLMovement == taxGLMovement1.PK));

			filter.Property = "00000000";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "0000";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 4 (2+2) records", 4, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "1001";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2 (0+2) records", 2, TestCollection.Count);
			Assert("Expecting collection only contains entries from taxGLMovement2", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_ATM_TaxGLMovement == taxGLMovement2.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 4 (2+2) records", 4, TestCollection.Count);
		}

		public void TestCurrencyFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var transaction1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "A00001", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			var transaction2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "A00002", TestObjectCreator.USD, 1M, 10M, 10M, 10M, 10M);

			transaction1.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction2.AH_OH = TestObjectCreator.ABIGAS.PK;

			Factory.Save();

			ProcessGeneralLedgerData(transaction1.Lines[0]);
			ProcessGeneralLedgerData(transaction2.Lines[0]);

			TestCollection.Load();
			AssertEquals("Expecting collection contains all 12(6+6) records", 12, TestCollection.Count);

			var filter = (ModuleNkFilter)FilterBO["Currency"];
			filter.IsActive = true;
			filter.Property = "AUD";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 6 records", 6, TestCollection.Count);
			Assert("Expecting collection only contains entries from transaction1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == transaction1.PK));
		}

		public void TestChargeCodeFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			var jobRevenueJournal1 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			var jobRevenueJournal2 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC2, job, 100M);

			Factory.Save();

			ProcessGeneralLedgerData(jobRevenueJournal1.Lines[0]);
			ProcessGeneralLedgerData(jobRevenueJournal1.Lines[1]);
			ProcessGeneralLedgerData(jobRevenueJournal2.Lines[0]);
			ProcessGeneralLedgerData(jobRevenueJournal2.Lines[1]);

			var filter = (ModuleGuidFilter)FilterBO["Charge Code"];
			filter.IsActive = true;
			filter.Property = TestObjectCreator.CC1.PK;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 8 records", 8, TestCollection.Count);
			Assert("Expecting collection only contains entries from jobRevenueJournal1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == jobRevenueJournal1.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains all 16(8+8) records", 16, TestCollection.Count);
		}

		public void TestJobNumberFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S0010", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);

			var journal1 = TestObjectCreator.CreateJournal<ARJournal>(100m, ZDate.Today, TestObjectCreator.ABIGAS.PK);
			var journal2 = TestObjectCreator.CreateJournal<ARJournal>(200m, ZDate.Today, TestObjectCreator.ABIGAS.PK);
			journal1.AH_JH = job1.PK;
			journal2.AH_JH = job2.PK;

			var taxGLMovement1 = CreateAccTaxGLMovement(110m, ZDateTime.Today, job1);
			var taxGLMovement2 = CreateAccTaxGLMovement(220m, ZDateTime.Today, job2);

			var lineForWIP1 = TestObjectCreator.CreateWIP(job1);
			var lineForAccrual1 = TestObjectCreator.CreateAccrual(job1);
			var lineForWIP2 = TestObjectCreator.CreateWIP(job2);
			var lineForAccrual2 = TestObjectCreator.CreateAccrual(job2);

			Factory.Save();

			ProcessGeneralLedgerData(journal1);
			ProcessGeneralLedgerData(journal2);
			ProcessGeneralLedgerData(taxGLMovement1);
			ProcessGeneralLedgerData(taxGLMovement2);
			ProcessGeneralLedgerData(lineForWIP1);
			ProcessGeneralLedgerData(lineForAccrual1);
			ProcessGeneralLedgerData(lineForWIP2);
			ProcessGeneralLedgerData(lineForAccrual2);

			var filter = (ModuleTextRangeFilter)FilterBO["Job Number"];
			filter.IsActive = true;

			filter.Property1 = ZString.Empty;
			filter.Property2 = "S0001";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 8 (journal:2 + taxGLMovement:2 + WIP:2 + Accural:2) records", 8, TestCollection.Count);
			Assert("Expecting collection only contains entries related to job1", TestCollection.All(x => AssertGeneralLedgerDataProperties((AccGeneralLedgerData)x, journal1, taxGLMovement1, lineForWIP1, lineForAccrual1)));

			filter.Property1 = "S0010";
			filter.Property2 = ZString.Empty;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 8 (journal:2 + taxGLMovement:2 + WIP:2 + Accural:2) records", 8, TestCollection.Count);
			Assert("Expecting collection only contains entries related to job2", TestCollection.All(x => AssertGeneralLedgerDataProperties((AccGeneralLedgerData)x, journal2, taxGLMovement2, lineForWIP2, lineForAccrual2)));

			filter.Property1 = "S0001";
			filter.Property2 = "S0010";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 16 records", 16, TestCollection.Count);

			filter.Property1 = "S0002";
			filter.Property2 = "S0009";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);
		}

		bool AssertGeneralLedgerDataProperties(AccGeneralLedgerData data, TransactionHeader header, AccTaxGLMovement taxGLMovement, TransactionLine lineForWIP, TransactionLine lineForAccrual)
		{
			if (!data.GLD_AH_TransactionHeader.IsEmpty)
			{
				return data.GLD_AH_TransactionHeader == header.PK;
			}

			if (!data.GLD_ATM_TaxGLMovement.IsEmpty)
			{
				return data.GLD_ATM_TaxGLMovement == taxGLMovement.PK;
			}

			if (!data.GLD_AL_TransactionLine.IsEmpty && data.TransactionLine.AL_LineType == TransactionLineTypes.WIP)
			{
				return data.GLD_AL_TransactionLine == lineForWIP.PK;
			}

			if (!data.GLD_AL_TransactionLine.IsEmpty && data.TransactionLine.AL_LineType == TransactionLineTypes.Accrual)
			{
				return data.GLD_AL_TransactionLine == lineForAccrual.PK;
			}

			return false;
		}

		public void TestHeaderDescriptionFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var transaction1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "000001", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			var transaction2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "000002", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			var transaction3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000003", TestObjectCreator.AUD, 1M, -10M, -10M, -10M, -10M);
			transaction1.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction2.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction3.AH_OH = TestObjectCreator.ABIGAS.PK;
			transaction1.AH_Desc = "AR Invoice 1";
			transaction2.AH_Desc = "AR Invoice 2";
			transaction3.AH_Desc = "AP Invoice 1";

			Factory.Save();

			ProcessGeneralLedgerData(transaction1.Lines[0]);
			ProcessGeneralLedgerData(transaction2.Lines[0]);
			ProcessGeneralLedgerData(transaction3.Lines[0]);

			var filter = (ModuleTextFilter)FilterBO["Header Description"];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "AR Invoice 1";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 6 records", 6, TestCollection.Count);
			Assert("Expecting collection only contains entries from transaction1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader == transaction1.PK));

			filter.Property = "XXX";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "AR";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 12 (6+6+0) records", 12, TestCollection.Count);
			Assert("Expecting collection Expecting collection not to contain transaction3", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != transaction3.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "Invoice 1";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 12 (6+0+6) records", 12, TestCollection.Count);
			Assert("Expecting collection Expecting collection not to contain transaction2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AH_TransactionHeader != transaction2.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains all 18 records", 18, TestCollection.Count);
		}

		public void TestHeaderDescriptionFilterForTaxGLMovement()
		{
			SetUpForTestUsingTestObjectCreator();

			var taxGLMovement1 = CreateAccTaxGLMovement(110m, ZDateTime.Today);

			ProcessGeneralLedgerData(taxGLMovement1);

			var filter = (ModuleTextFilter)FilterBO["Header Description"];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Test Invoice";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2 records", 2, TestCollection.Count);
			Assert("Expecting collection only contains entries from taxGLMovement1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_ATM_TaxGLMovement == taxGLMovement1.PK));

			filter.Property = "XXX";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "Test";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2 records", 2, TestCollection.Count);
			Assert("Expecting collection only contains entries from taxGLMovement1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_ATM_TaxGLMovement == taxGLMovement1.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "Invoice";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2 records", 2, TestCollection.Count);
			Assert("Expecting collection only contains entries from taxGLMovement1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_ATM_TaxGLMovement == taxGLMovement1.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2 records", 2, TestCollection.Count);
			Assert("Expecting collection only contains entries from taxGLMovement1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_ATM_TaxGLMovement == taxGLMovement1.PK));
		}

		public void TestLineDescriptionFilter()
		{
			SetUpForTestUsingTestObjectCreator();

			var lineForWIP1 = TestObjectCreator.CreateWIP();
			var lineForWIP2 = TestObjectCreator.CreateWIP();
			var lineForAccural1 = TestObjectCreator.CreateAccrual();
			lineForWIP1.AL_Desc = "WIP Line 1";
			lineForWIP2.AL_Desc = "WIP Line 2";
			lineForAccural1.AL_Desc = "Accural Line 1";

			Factory.Save();

			ProcessGeneralLedgerData(lineForWIP1);
			ProcessGeneralLedgerData(lineForWIP2);
			ProcessGeneralLedgerData(lineForAccural1);

			var filter = (ModuleTextFilter)FilterBO["Line Description"];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "WIP Line 1";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 2 records", 2, TestCollection.Count);
			Assert("Expecting collection only contains entries from lineForWIP1", TestCollection.All(x => ((AccGeneralLedgerData)x).GLD_AL_TransactionLine == lineForWIP1.PK));

			filter.Property = "XXX";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 0 records", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "WIP";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 4 (2+2+0) records", 4, TestCollection.Count);
			Assert("Expecting collection Expecting collection not to contain lineForAccural1", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AL_TransactionLine != lineForAccural1.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "1";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 4 (2+2+0) records", 4, TestCollection.Count);
			Assert("Expecting collection Expecting collection not to contain lineForWIP2", TestCollection.Any(x => ((AccGeneralLedgerData)x).GLD_AL_TransactionLine != lineForWIP2.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains all 6 records", 6, TestCollection.Count);
		}

		public void TestGLAccountTypeFilter()
		{
			TestCaseHelper.ClearTable(AccGeneralLedgerData.Schema.TableName);
			var bshHeaderPk = CreateGeneralLedgerDataAndGLHeader("BSH");
			var plHeaderPk = CreateGeneralLedgerDataAndGLHeader("P&L");
			var nteHeaderPk = CreateGeneralLedgerDataAndGLHeader("NTE");
			var filter = (ModuleTextFilter)FilterBO["Account Type"];
			filter.IsActive = true;

			filter.Property = "BSH";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 1 records", 1, TestCollection.Count);
			AssertEquals("Expecting collection has BSH record.", bshHeaderPk, TestCollection[0].GLD_AG_GLAccount);

			filter.Property = "P&L";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 1 records", 1, TestCollection.Count);
			AssertEquals("Expecting collection has P&L record.", plHeaderPk, TestCollection[0].GLD_AG_GLAccount);

			filter.Property = "NTE";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 1 records", 1, TestCollection.Count);
			AssertEquals("Expecting collection has NTE record.", nteHeaderPk, TestCollection[0].GLD_AG_GLAccount);

			filter.Property = "ALL";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contains 3(1+1+1) records", 3, TestCollection.Count);

			ZGuid CreateGeneralLedgerDataAndGLHeader(string accountType)
			{
				var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
				accGeneralLedgerData.GLD_Type = "PST";
				accGeneralLedgerData.GLD_GLAccountType = "ARC";
				accGeneralLedgerData.GLD_PostDate = ZDateTime.Today;
				accGeneralLedgerData.GLD_PostPeriod = 1;
				accGeneralLedgerData.GLD_GC_Company = GlbCompany.CurrentCompany.PK;
				accGeneralLedgerData.GLD_GB_Branch = GlbBranch.CurrentBranch.PK;
				accGeneralLedgerData.GLD_GE_Department = GlbDepartment.CurrentDepartment.PK;

				var accGlHeader = Factory.NewWithValidTestData<AccGLHeader>();
				accGlHeader.AG_AccountType = accountType;
				accGeneralLedgerData.GLD_AG_GLAccount = accGlHeader.PK;
				Factory.Save();
				return accGlHeader.PK;
			}
		}

		public void TestTransactionNumberFilterComparisonOperators()
		{
			var filter = (ModuleTextFilter)FilterBO["Transaction Number"];
			TestFilterDoesNotContainUnexpectedComparisonOperators(filter);
		}

		public void TestChargeCodeFilterComparisonOperators()
		{
			var filter = (ModuleGuidFilter)FilterBO["Charge Code"];
			TestFilterDoesNotContainUnexpectedComparisonOperators(filter);
		}

		public void TestHeaderDescriptionFilterComparisonOperators()
		{
			var filter = (ModuleTextFilter)FilterBO["Header Description"];
			TestFilterDoesNotContainUnexpectedComparisonOperators(filter);
		}

		public void TestLineDescriptionFilterComparisonOperators()
		{
			var filter = (ModuleTextFilter)FilterBO["Line Description"];
			TestFilterDoesNotContainUnexpectedComparisonOperators(filter);
		}

		public void TestJobNumberFilterDescription()
		{
			ModuleTextRangeFilter filter = (ModuleTextRangeFilter)FilterBO["Job Number"];
			AssertEquals("Localized Filter Name", filter.LocalizedDescription, "Job Number Range");
		}

		void TestFilterDoesNotContainUnexpectedComparisonOperators<T>(ModuleFilterWithSelectedFilters<T> filter)
			where T : IZType
		{
			var comparisonOperators = new ArrayList();
			comparisonOperators.Add(ModuleTextFilter.ComparisonConstants.IsBlank);
			comparisonOperators.Add(ModuleTextFilter.ComparisonConstants.NotEqual);
			comparisonOperators.Add(ModuleTextFilter.ComparisonConstants.NotContain);
			comparisonOperators.Add(ModuleTextFilter.ComparisonConstants.NotStartsWith);

			foreach (var comparisonOperator in comparisonOperators)
			{
				Assert(!filter.ComparisonOperator_List.ContainsCode(comparisonOperator));
			}
		}

		void SetUpForTestUsingTestObjectCreator()
		{
			TestCaseHelper.ClearTable(AccGeneralLedgerData.Schema.TableName);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			TestObjectCreator.CreateTestPeriods(ZDate.Today);
			var account1 = TestObjectCreator.CreateARControlAccount();
			var account2 = TestObjectCreator.CreateARSuspenseControlAccount();
			var account3 = TestObjectCreator.CreateAPSuspenseControlAccount();
			var account4 = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account3.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account4.PK.ToGuid());
		}

		void ProcessGeneralLedgerData(INeedRow gLDSource)
		{
			gLDSource.Row.SetAdded();
			AssertNotNull(GeneralLedgerDataProcessor);

			TestObjectCreator.MockNudgeGLDProcessData([gLDSource.Row]);
		}

		AccTaxGLMovement CreateAccTaxGLMovement(decimal osTaxAmount, ZDateTime invoiceDate, Job job = null)
		{
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			invoice.AH_TransactionType = TransactionTypes.InvoiceBatch;
			invoice.AH_InvoiceDate = invoiceDate;
			if (job != null)
			{
				invoice.AH_JH = job.PK;
			}
			var objForTest = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			var taxTransaction = Factory.New<AccTaxTransaction>();
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;
			taxTransaction.ATT_ETC = taxConfiguration.PK;
			taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
			taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			taxTransaction.ATT_Ledger = LedgerTypes.AccountsReceivable;
			taxTransaction.ATT_Basis = TaxBasisList.PostingOnMatching.Code;
			taxTransaction.ATT_RX_NKOSTaxCurrency = "AUD";
			taxTransaction.ATT_OSTaxBaseAmount = osTaxAmount;
			taxTransaction.ATT_LocalTaxBaseAmount = osTaxAmount;
			taxTransaction.ATT_OSTaxAmount = osTaxAmount;
			taxTransaction.ATT_LocalTaxAmount = osTaxAmount;
			taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.Perceptions.Code;
			taxTransaction.ATT_Rate = 0.1274m;

			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK));
			if (pivots.Length == 0)
			{
				var pivot = Factory.New<AccTaxRecordTransactionLinePivot>();
				pivot.ATP_ATT = taxTransaction.PK;
				pivot.FillWithValidTestData();
			}
			taxTransaction.ATT_PostDate = ZDate.Today;
			taxTransaction.ATT_TaxDate = ZDate.Today;
			taxTransaction.ATT_TaxSystemCode = "DNC";
			taxTransaction.ATT_AH = ((ITaxRecordParentBase)objForTest).PK;

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxTransaction.ATT_AT_TaxID = taxRate.PK;

			Factory.Save();

			var taxGLMovement = Factory.New<AccTaxGLMovement>();

			taxGLMovement.ATM_ATT_TaxTransaction = taxTransaction.PK;
			taxGLMovement.ATM_Period = 202301;
			taxGLMovement.ATM_Date = ZDate.Today;
			taxGLMovement.ATM_Amount = 100m;
			taxGLMovement.ATM_Type = TaxGLMovementTypeList.Realised.Code;

			var glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();

			taxGLMovement.ATM_AG_DebitAccount = glHeader1.PK;
			taxGLMovement.ATM_AG_CreditAccount = glHeader2.PK;

			Factory.Save();

			return taxGLMovement;
		}

		#region Override

		protected override void SetUp()
		{
			base.SetUp();

			var glbCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			glbCompany.GC_SystemCreateUser = "GLD";

			var accGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			accGLHeader.AG_SystemCreateUser = "GLD";
			accGLHeader.AG_AccountNum = "B";

			var testAccGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			testAccGLHeader1.AG_AccountNum = "A";

			var testAccGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			testAccGLHeader2.AG_AccountNum = "C";

			glbBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch1.GB_SystemCreateUser = "GLD";

			glbBranch2 = Factory.NewWithValidTestData<GlbBranch>();

			glbDepartment1 = Factory.NewWithValidTestData<GlbDepartment>();
			glbDepartment1.GE_SystemCreateUser = "GLD";

			glbDepartment2 = Factory.NewWithValidTestData<GlbDepartment>();

			var accChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			accChargeCode1.AC_SystemCreateUser = "GLD";
			accChargeCode1.AC_Code = "A";

			Factory.Save();

			fTestObjectCreator = new TestObjectCreator(Factory);

			TestConnection.ExecuteNonQuery(Sql);
			FilterBO = (AccGeneralLedgerDataFilterBusinessObject)GetNewFilterStripBusinessObject();
			testCollection = new AccGeneralLedgerDataCollection(Factory);

			fGeneralLedgerDataProcessor = new GeneralLedgerDataProcessor();
		}

		#endregion

		static AccTaxGLMovement GetAccTaxGLMovementForTest(BusinessObjectFactory factory, decimal osTaxAmount, OrgHeader orgHeader)
		{
			var testObjectCreator = new TestObjectCreator(factory);
			testObjectCreator.CreateTestPeriodsForEntireYear(2004);

			factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(ARInvoice));
			invoice.AH_TransactionType = TransactionTypes.InvoiceBatch;
			var objForTest = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			var taxTransaction = factory.New<AccTaxTransaction>();
			var taxConfiguration = factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;
			taxTransaction.ATT_ETC = taxConfiguration.PK;
			taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
			taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			taxTransaction.ATT_Ledger = LedgerTypes.AccountsReceivable;
			taxTransaction.ATT_Basis = TaxBasisList.PostingOnMatching.Code;
			taxTransaction.ATT_RX_NKOSTaxCurrency = "AUD";
			taxTransaction.ATT_OSTaxBaseAmount = osTaxAmount;
			taxTransaction.ATT_LocalTaxBaseAmount = osTaxAmount;
			taxTransaction.ATT_OSTaxAmount = osTaxAmount;
			taxTransaction.ATT_LocalTaxAmount = osTaxAmount;
			taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.Perceptions.Code;
			taxTransaction.ATT_Rate = 0.1274m;

			var pivots = factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK));
			if (pivots.Length == 0)
			{
				var pivot = factory.New<AccTaxRecordTransactionLinePivot>();
				pivot.ATP_ATT = taxTransaction.PK;
				pivot.FillWithValidTestData();
			}
			taxTransaction.ATT_PostDate = ZDate.Today;
			taxTransaction.ATT_TaxDate = ZDate.Today;
			taxTransaction.ATT_TaxSystemCode = "DNC";
			var transactionHeader = factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_OH = orgHeader.PK;
			taxTransaction.ATT_AH = transactionHeader.PK;

			var taxRate = factory.NewWithValidTestData<AccTaxRate>();
			taxTransaction.ATT_AT_TaxID = taxRate.PK;

			factory.Save();

			var taxGLMovement = factory.New<AccTaxGLMovement>();

			taxGLMovement.ATM_ATT_TaxTransaction = taxTransaction.PK;
			taxGLMovement.ATM_Period = 200401;
			taxGLMovement.ATM_Date = ZDate.Today;
			taxGLMovement.ATM_Amount = 100m;
			taxGLMovement.ATM_Type = TaxGLMovementTypeList.Realised.Code;

			var glHeader1 = factory.NewWithValidTestData<AccGLHeader>();
			var glHeader2 = factory.NewWithValidTestData<AccGLHeader>();

			taxGLMovement.ATM_AG_DebitAccount = glHeader1.PK;
			taxGLMovement.ATM_AG_CreditAccount = glHeader2.PK;

			factory.Save();

			return taxGLMovement;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator; }
		}
		TestObjectCreator fTestObjectCreator;

		IGeneralLedgerDataProcessor GeneralLedgerDataProcessor
		{
			get { return fGeneralLedgerDataProcessor; }
		}
		IGeneralLedgerDataProcessor fGeneralLedgerDataProcessor;

		GlbBranch GlbBranch1
		{
			get { return glbBranch1; }
		}
		GlbBranch glbBranch1;

		GlbBranch GlbBranch2
		{
			get { return glbBranch2; }
		}
		GlbBranch glbBranch2;

		GlbDepartment GlbDepartment1
		{
			get { return glbDepartment1; }
		}
		GlbDepartment glbDepartment1;

		GlbDepartment GlbDepartment2
		{
			get { return glbDepartment2; }
		}
		GlbDepartment glbDepartment2;

		AccGeneralLedgerDataCollection TestCollection
		{
			get { return testCollection; }
		}

		AccGeneralLedgerDataCollection testCollection;

		AccGeneralLedgerDataFilterBusinessObject FilterBO;

		const string Sql = @"
Declare @Id int
Set @Id = 1
declare @bdate smalldatetime, @edate smalldatetime
set @bdate = '2010-04-07 09:28:00'
set @edate = '2023-02-07 09:28:00'
declare @companyPK UNIQUEIDENTIFIER
declare @companyPKNotLoad UNIQUEIDENTIFIER
declare @accountPK UNIQUEIDENTIFIER
declare @branchPK UNIQUEIDENTIFIER
declare @departmentPK UNIQUEIDENTIFIER

While @Id <= 2
Begin 
   SELECT @companyPK = (select TOP 1 GC_PK FROM dbo.GLBCompany WHERE GC_SystemCreateUser = 'GLD' ORDER BY NEWID());
   SELECT @companyPKNotLoad = (select TOP 1 GC_PK FROM dbo.GLBCompany WHERE GC_SystemCreateUser != 'GLD' ORDER BY NEWID());
   SELECT @accountPK = (select TOP 1 AG_PK FROM dbo.AccGLHeader WHERE AG_SystemCreateUser = 'GLD' ORDER BY NEWID());
   SELECT @branchPK = (select TOP 1 GB_PK FROM dbo.GLBBranch WHERE GB_SystemCreateUser = 'GLD' ORDER BY NEWID());
   SELECT @departmentPK = (select TOP 1 GE_PK FROM dbo.GLBDepartment WHERE GE_SystemCreateUser = 'GLD' ORDER BY NEWID());
   Insert Into dbo.AccGeneralLedgerData 
   (
   GLD_PK,
   GLD_GC_Company,
   GLD_PostDate,
   GLD_AG_GLAccount,
   GLD_OSDebitAmount,
   GLD_GB_Branch,
   GLD_GE_Department,
   GLD_SystemCreateTimeUtc,
   GLD_SystemLastEditTimeUtc,
   GLD_SystemCreateUser,
   GLD_SystemLastEditUser,
   GLD_PostPeriod,
   GLD_Type,
   GLD_GB_TaxBranch,
   GLD_GLAccountType,
   GLD_JournalEntriesNumber,
   GLD_JournalEntriesNumberRuleCode
	)
   Values (
			NEWID(),
			CASE WHEN @Id = 1 THEN @companyPK ELSE @companyPKNotLoad END,
			@edate,
			@accountPK,
			ceiling(rand() * 500000),
			@branchPK,
			@departmentPK,
			@edate,
			@edate,
			'~BP',
			'~BP',
			1,
			'PST',
			@branchPK,
			'ARC',
			CAST(@Id as VARCHAR(1)) + '00009',
			CAST(@Id as VARCHAR(1))
		)
	Set @Id = @Id + 1
End
";
	}
}
