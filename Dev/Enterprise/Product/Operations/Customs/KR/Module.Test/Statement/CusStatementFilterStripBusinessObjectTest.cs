using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(CusStatementFilterStripBusinessObject))]
	sealed class CusStatementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestStatementNumberFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var statementNumberFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.StatementNumber];
			statementNumberFilter.Property = "102";
			statementNumberFilter.IsActive = true;

			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1028142299", coll.Cast<CusStatementHeader>().First().B2_StatementNumber);
			AssertEquals("102-81-42299", coll.Cast<CusStatementHeader>().First().FormattedStatementNumber);
		}

		public void TestStatementProcessPortFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var statementProcessPortFilter = (ModuleNkFilter)filter[CusStatementFilterStripBusinessObject.Schema.CustomsOffice];
			statementProcessPortFilter.Property = "001";
			statementProcessPortFilter.IsActive = true;

			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("001", coll.Cast<CusStatementHeader>().First().B2_ProcessPort);
		}

		public void TestImporterCustomsIDFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var importerCustomsIDFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.ImporterCustomsID];
			importerCustomsIDFilter.Property = "7010101234567";
			importerCustomsIDFilter.IsActive = true;

			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("7010101234567", coll.Cast<CusStatementHeader>().First().B2_ImporterCustomsID);
		}

		public void TestStatementAmountFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var statementAmountFilter = (ModuleNumberRangeFilter)filter[CusStatementFilterStripBusinessObject.Schema.StatementAmount];
			statementAmountFilter.Property1 = 50m;
			statementAmountFilter.Property2 = 150m;
			statementAmountFilter.IsActive = true;

			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(100m, coll.Cast<CusStatementHeader>().First().B2_StatementAmount);
		}

		public void TestStatementDueDateFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var statementDueDateFilter = (ModuleDateFilter)filter[CusStatementFilterStripBusinessObject.Schema.DueDate];
			statementDueDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			statementDueDateFilter.Property2 = new ZDateTime(2021, 3, 30);
			statementDueDateFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(new ZDateTime(2021, 3, 1), coll.Cast<CusStatementHeader>().First().B2_DueDate);
		}

		public void TestStatementProcessDateFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var statementProcessDateFilter = (ModuleDateFilter)filter[CusStatementFilterStripBusinessObject.Schema.ProcessDate];
			statementProcessDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			statementProcessDateFilter.Property2 = new ZDateTime(2021, 6, 30);
			statementProcessDateFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(new ZDateTime(2021, 6, 1), coll.Cast<CusStatementHeader>().First().B2_ProcessDate);
		}

		public void TestStatementPaymentDateFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var statementPaymentDateFilter = (ModuleDateFilter)filter[CusStatementFilterStripBusinessObject.Schema.PaymentDate];
			statementPaymentDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			statementPaymentDateFilter.Property2 = new ZDateTime(2021, 6, 30);
			statementPaymentDateFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(new ZDateTime(2021, 6, 1), coll.Cast<CusStatementHeader>().First().B2_PaymentAuthorizationDate);
		}

		public void TestStatementStatusFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var statementStatusFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.PaymentStatus];
			statementStatusFilter.Property = "PYI";
			statementStatusFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("PYI", coll.Cast<CusStatementHeader>().First().B2_PaymentStatus);
		}

		public void TestStatementTypeFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var statementTypeFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.StatementType];
			statementTypeFilter.Property = "B";
			statementTypeFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("B", coll.Cast<CusStatementHeader>().First().B2_StatementType);
		}

		public void TestPaymentPartyFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var paymentPartyFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.PaymentParty];
			paymentPartyFilter.Property = "OWN";
			paymentPartyFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("OWN", coll.Cast<CusStatementHeader>().First().B2_PaymentParty);
		}

		public void TestImporterFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var payerFilter = (ModuleGuidFilter)filter[CusStatementFilterStripBusinessObject.Schema.Payer];
			payerFilter.Property = org1.PK;
			payerFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(org1.PK, coll.Cast<CusStatementHeader>().First().Importer.PK);
		}

		public void TestImporterNameFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var payerNameFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.PayerCompanyName];
			payerNameFilter.Property = "Readykorea";
			payerNameFilter.IsActive = true;

			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("Readykorea", coll.Cast<CusStatementHeader>().First().PayerCompanyName);
		}

		public void TestPreiodFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var preiodFilter = (PeriodFilter)filter[CusStatementFilterStripBusinessObject.Schema.StatementPeriod];
			preiodFilter.PeriodYear = 2021;
			preiodFilter.PeriodMonth = 1;
			preiodFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("01/2021", coll.Cast<CusStatementHeader>().First().PeriodFrom);
		}

		public void TestPaymentStatusFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var paymentStatusFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.PaymentStatus];
			paymentStatusFilter.Property = "PYI";
			paymentStatusFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("PYI", coll.Cast<CusStatementHeader>().First().B2_PaymentStatus);
		}

		public void TestEntryNumberFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var entryNumberFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.EntryNumber];
			entryNumberFilter.Property = "1234520000045M";
			entryNumberFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234520000045M", coll.Cast<CusStatementHeader>().First().FirstLine.B3_EntryNum);
		}

		public void TestBillTypeFilter()
		{
			SetupData();
			var filter = new CusStatementFilterStripBusinessObject();
			var billTypeFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.BillType];
			billTypeFilter.Property = "Z";
			billTypeFilter.IsActive = true;
			var coll = new CusStatementHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("Z", coll.Cast<CusStatementHeader>().First().B2_Status);
		}

		public void TestShowOnlyCurrentCompanyStatements()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var statement = Factory.New<CusStatementHeader>();
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_GC = company.PK;
			Factory.Save();

			var filterBizObj = new CusStatementFilterStripBusinessObject();
			AssertEquals(true, statement.MatchesFilter(filterBizObj.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizObj.Filter));
		}

		public void TestFilters()
		{
			var filter = new CusStatementFilterStripBusinessObject();
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.StatementNumber]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.CustomsOffice]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.ImporterCustomsID]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.StatementAmount]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.DueDate]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.ProcessDate]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.StatementType]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.PaymentParty]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.Payer]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.Company]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.PaymentDate]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.PayerCompanyName]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusStatementFilterStripBusinessObject();

		void SetupData()
		{
			org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "Readykorea";
			org1.OH_FullName = "Readykorea";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "CW1";
			org2.OH_FullName = "CW1";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "1028142299";
			statement.B2_ProcessPort = "001";
			statement.B2_ImporterCustomsID = "7010101234567";
			statement.B2_StatementAmount = 100m;
			statement.B2_DueDate = new ZDate(2021, 3, 1);
			statement.B2_ProcessDate = new ZDate(2021, 6, 1);
			statement.B2_PaymentAuthorizationDate = new ZDate(2021, 6, 1);
			statement.B2_PaymentStatus = "PYI";
			statement.B2_StatementType = "B";
			statement.B2_PaymentParty = "OWN";
			statement.B2_OH_Importer = org1.PK;
			statement.B2_PeriodStartDate = new ZDate(2021, 1, 1);
			statement.B2_PeriodEndDate = new ZDate(2021, 1, 31);
			statement.B2_Status = StatementHeaderStatusList.Codes.Z;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = "2222";
			statement2.B2_ProcessPort = "002";
			statement2.B2_ImporterCustomsID = "8010101234567";
			statement2.B2_StatementAmount = 200m;
			statement2.B2_DueDate = new ZDate(2021, 4, 1);
			statement2.B2_ProcessDate = new ZDate(2021, 7, 1);
			statement2.B2_PaymentAuthorizationDate = new ZDate(2021, 7, 1);
			statement2.B2_PaymentStatus = "PYC";
			statement2.B2_StatementType = "C";
			statement2.B2_PaymentParty = "BRK";
			statement2.B2_OH_Importer = org2.PK;
			statement2.B2_PeriodStartDate = new ZDate(2021, 2, 1);
			statement2.B2_PeriodEndDate = new ZDate(2021, 2, 28);
			statement2.B2_Status = StatementHeaderStatusList.Codes.A;
			var statementLine2 = statement.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "9876520000045M";

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementNumber = "3333";
			statement3.B2_ProcessPort = "003";
			statement3.B2_ImporterCustomsID = "9010101234567";
			statement3.B2_StatementAmount = 300m;
			statement3.B2_DueDate = new ZDate(2021, 5, 1);
			statement3.B2_ProcessDate = new ZDate(2021, 8, 1);
			statement3.B2_PaymentAuthorizationDate = new ZDate(2021, 8, 1);
			statement3.B2_PaymentStatus = "PYC";
			statement3.B2_StatementType = "C";
			statement3.B2_PaymentParty = "BRK";
			statement3.B2_OH_Importer = org2.PK;
			statement3.B2_PeriodStartDate = new ZDate(2021, 3, 1);
			statement3.B2_PeriodEndDate = new ZDate(2021, 3, 31);
			statement3.B2_Status = StatementHeaderStatusList.Codes.B;
			var statementLine3 = statement.StatementLines.AddNew();
			statementLine3.B3_EntryNum = "8520220000045M";

			Factory.Save();
		}

		OrgHeader org1;
	}
}
