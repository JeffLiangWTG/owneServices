using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class ComplianceDocumentFilterStripBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestJobNumberFilter()
		{
			var jobNumberFilter = (ModuleNumberFilter)TestFilterBizO["Job #"];
			jobNumberFilter.Property = "987";
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobNumberFilter.IsActive = true;

			var startWithCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			startWithCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, startWithCollection.Count);

			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			jobNumberFilter.Property = "54";

			var containsCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			containsCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, containsCollection.Count);

			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobNumberFilter.Property = "987654321";

			var equalCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			equalCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, equalCollection.Count);
		}

		public void TestTransactionNumberFilter()
		{
			var transactionNumberFilter = (ModuleNumberFilter)TestFilterBizO["Transaction #"];
			transactionNumberFilter.Property = "123";
			transactionNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			transactionNumberFilter.IsActive = true;

			var collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, collection.Count);

			transactionNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			transactionNumberFilter.Property = "45";

			var collection1 = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection1.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, collection1.Count);
		}

		public void TestInternalReferenceFilter()
		{
			var internalReferenceFilter = (ModuleNumberFilter)TestFilterBizO["Internal Reference #"];
			internalReferenceFilter.Property = "00001000";
			internalReferenceFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			internalReferenceFilter.IsActive = true;

			var collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, collection.Count);

			internalReferenceFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			internalReferenceFilter.Property = "45";

			collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals("There should be 0 compliance document in the collection", 0, collection.Count);
		}

		public void TestTransactionTypeFilter()
		{
			var transactionTypeFilter = (ModuleTextFilter)TestFilterBizO["Transaction Type"];
			transactionTypeFilter.Property = TransactionTypes.Invoice;
			transactionTypeFilter.IsActive = true;

			var invCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			invCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, invCollection.Count);

			transactionTypeFilter.Property = TransactionTypes.CreditNote;

			var crdCollection1 = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			crdCollection1.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, crdCollection1.Count);

			transactionTypeFilter.Property = "ALL";

			var allCollection2 = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			allCollection2.Load();

			AssertEquals("There should be 2 compliance document in the collection", 2, allCollection2.Count);
		}

		public void TestComplianceDocumentNumberFilter()
		{
			var documentNumberFilter = (ModuleNumberFilter)TestFilterBizO["Document #"];
			documentNumberFilter.Property = "123";
			documentNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			documentNumberFilter.IsActive = true;

			var startWithCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			startWithCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, startWithCollection.Count);

			documentNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			documentNumberFilter.Property = "45";

			var containsCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			containsCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, containsCollection.Count);

			documentNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			documentNumberFilter.Property = "123456789";

			var equalCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			equalCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, equalCollection.Count);
		}

		[TestDate(2018, 03, 22, 10, 10, 10)]
		public void TestDocumentDateFilter()
		{
			var documentDataFilter = (ModuleDateFilter)TestFilterBizO[AccountingConstants.DateFilterTypes.DocumentDate];
			documentDataFilter.Property1 = ZDateTime.Now;
			documentDataFilter.IsActive = true;

			var collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 2, collection.Count);
		}

		public void TestDocumentStatus()
		{
			var documentStatusFilter = (ModuleTextFilter)TestFilterBizO["Document Status"];
			documentStatusFilter.IsActive = true;
			documentStatusFilter.Property = Core.Constants.ComplianceDocumentStatus.Added;

			var collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, collection.Count);

			documentStatusFilter.Property = Core.Constants.ComplianceDocumentStatus.NumberSet;

			collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, collection.Count);
		}

		public void TestComplianceDocumentSubType()
		{
			var complianceSubTypeFilter = (ModuleTextFilter)TestFilterBizO["Compliance Sub Type"];
			complianceSubTypeFilter.IsActive = true;
			complianceSubTypeFilter.Property = "NTC";

			var collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, collection.Count);
		}

		public void TestAccountingPeriodFilter()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			ZDateTime startDateOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			PeriodManager periodManager = TestObjectCreator.CreateTestPeriods(startDateOfFinancialYear);

			Assert("Expecting collection to contain twelve items", periodManager.Periods.Count == 12);

			Factory.Save();

			var filter = (ModuleTextRangeFilter)TestFilterBizO["Accounting Period"];

			filter.Property1 = String.Empty;
			filter.Property2 = String.Format("{0}02", startDateOfFinancialYear.Year);
			filter.IsActive = true;

			var collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals("There should be 0 compliance document in the collection", 0, collection.Count);

			filter.Property1 = String.Format("{0}03", startDateOfFinancialYear.Year);
			filter.Property2 = String.Empty;

			var collection1 = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection1.Load();

			AssertEquals("There should be 0 compliance documents in the collection", 2, collection1.Count);

			filter.Property1 = String.Format("{0}03", startDateOfFinancialYear.Year);
			filter.Property2 = String.Format("{0}03", startDateOfFinancialYear.Year);

			var collection2 = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection2.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, collection2.Count);

			filter.Property1 = String.Format("{0}01", startDateOfFinancialYear.Year);
			filter.Property2 = String.Format("{0}012", startDateOfFinancialYear.Year);

			var collection3 = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			collection3.Load();

			AssertEquals("There should be 2 compliance documents in the collection", 2, collection3.Count);
		}

		#region Implementation

		protected ComplianceDocumentFilterStripBusinessObject TestFilterBizO
		{
			get
			{
				if (fTestFilterBizO == null)
				{
					fTestFilterBizO = (ComplianceDocumentFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				}
				return fTestFilterBizO;
			}
		}
		ComplianceDocumentFilterStripBusinessObject fTestFilterBizO;

		protected abstract AccComplianceDocumentHeader CreateNewComplianceDocumentHeader(BusinessObjectFactory factory);

		protected abstract ZString LedgerType { get; }

		protected override void SetUp()
		{
			base.SetUp();

			var frt = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var caf = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CAF"));

			var accInvMsg = Factory.NewWithValidTestData<AccInvMsg>();
			accInvMsg.A9_Code = "MSG";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TX1";
			taxRate.AT_PostingGroupId = 0;
			taxRate.AT_A9_DefaultVatClass = accInvMsg.PK;

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TX2";
			taxRate1.AT_PostingGroupId = 1;

			var job = TestObjectCreator.CreateJob("987654321", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 2);

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerType;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_TransactionNum = "123456789";
			transactionHeader.AH_JH = job.PK;

			var invoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine.AL_AH = transactionHeader.PK;
			invoiceLine.AL_AT = taxRate.PK;
			invoiceLine.AL_AC = frt.PK;
			invoiceLine.AL_JH = job.PK;

			var invoiceLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine1.AL_AH = transactionHeader.PK;
			invoiceLine1.AL_AT = taxRate.PK;
			invoiceLine1.AL_AC = caf.PK;
			invoiceLine1.AL_JH = job.PK;

			var invoiceLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine2.AL_AH = transactionHeader.PK;
			invoiceLine2.AL_AT = taxRate.PK;
			invoiceLine2.AL_AC = caf.PK;
			invoiceLine2.AL_JH = job.PK;

			var invoiceLine3 = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine3.AL_AH = transactionHeader.PK;
			invoiceLine3.AL_AT = taxRate1.PK;
			invoiceLine3.AL_AC = frt.PK;
			invoiceLine3.AL_JH = job.PK;

			var complianceDocumentHeader = CreateNewComplianceDocumentHeader(Factory);
			complianceDocumentHeader.ADH_DocumentNumber = "123456789";
			complianceDocumentHeader.ADH_DocumentDate = ZDateTime.Now;
			complianceDocumentHeader.ADH_ComplianceSubType = "NTC";
			complianceDocumentHeader.ADH_ReportingPeriod = ZDateTime.Now.Year * 100 + 3;
			complianceDocumentHeader.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceDocumentHeader.ADH_ApprovalNumber = "987654321";

			var complianceDocumentLine = Factory.New<AccComplianceDocumentLine>();
			complianceDocumentLine.ADL_ADH = complianceDocumentHeader.PK;
			complianceDocumentLine.ADL_Description = "Test";
			complianceDocumentLine.ADL_Sequence = 1;

			var complianceDocumentPivot = Factory.New<AccComplianceDocumentPivot>();
			complianceDocumentPivot.ADP_ADL = complianceDocumentLine.PK;
			complianceDocumentPivot.ADP_AL = invoiceLine1.PK;

			var complianceDocumentPivot1 = Factory.New<AccComplianceDocumentPivot>();
			complianceDocumentPivot1.ADP_ADL = complianceDocumentLine.PK;
			complianceDocumentPivot1.ADP_AL = invoiceLine2.PK;

			var complianceDocumentLine1 = Factory.New<AccComplianceDocumentLine>();
			complianceDocumentLine1.ADL_ADH = complianceDocumentHeader.PK;
			complianceDocumentLine1.ADL_Description = "Test";
			complianceDocumentLine1.ADL_Sequence = 2;

			var complianceDocumentPivot2 = Factory.New<AccComplianceDocumentPivot>();
			complianceDocumentPivot2.ADP_ADL = complianceDocumentLine1.PK;
			complianceDocumentPivot2.ADP_AL = invoiceLine.PK;

			var creditNote = Factory.NewWithValidTestData<AccTransactionHeader>();
			creditNote.AH_Ledger = LedgerType;
			creditNote.AH_TransactionType = TransactionTypes.CreditNote;

			var creditNoteLine = Factory.NewWithValidTestData<AccTransactionLines>();
			creditNoteLine.AL_AH = creditNote.PK;
			creditNoteLine.AL_AC = caf.PK;

			var complianceDocumentHeader1 = CreateNewComplianceDocumentHeader(Factory);
			complianceDocumentHeader1.ADH_Ledger = creditNote.AH_Ledger;
			complianceDocumentHeader1.ADH_ReportingPeriod = ZDateTime.Now.Year * 100 + 10;
			complianceDocumentHeader1.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceDocumentHeader1.ADH_TransactionType = TransactionTypes.CreditNote;
			complianceDocumentHeader1.ADH_VoidingReason = "987654321";

			var complianceDocumentLine2 = Factory.New<AccComplianceDocumentLine>();
			complianceDocumentLine2.ADL_ADH = complianceDocumentHeader1.PK;
			complianceDocumentLine2.ADL_Description = "Test";
			complianceDocumentLine2.ADL_Sequence = 1;

			var complianceDocumentPivot3 = Factory.New<AccComplianceDocumentPivot>();
			complianceDocumentPivot3.ADP_ADL = complianceDocumentLine2.PK;
			complianceDocumentPivot3.ADP_AL = creditNoteLine.PK;

			Factory.Save();
		}

		#endregion
	}
}
