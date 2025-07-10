using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.Statement.Testing
{
	[TestedType(typeof(StatementFilterStripBusinessObject))]
	class StatementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new StatementFilterStripBusinessObject();
		}

		public void TestAlwaysAppliedCompanyFilter()
		{
			var company = Factory.New<GlbCompany>();
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			var statement2 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_GC = company.PK;

			AssertEquals(FilterVisibility.AlwaysAppliedAndHidden, filterBizO[StatementFilterStripBusinessObject.Schema.AlwaysAppliedCompany].Visibility);
			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestAlwaysAppliedTypeFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Export;
			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_BranchDesignation = StatementEntryTypeList.Codes.DCG;

			AssertEquals(FilterVisibility.AlwaysAppliedAndHidden, filterBizO[StatementFilterStripBusinessObject.Schema.AlwaysAppliedType].Visibility);
			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(true, statement2.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement3.MatchesFilter(filterBizO.Filter));
		}

		public void TestStatusFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement1.B2_Status = StatementStatusList.Codes.Complete;
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_Status = StatementStatusList.Codes.Incomplete;

			var statusFilter = (ModuleTextFilter)filterBizO[StatementFilterStripBusinessObject.Schema.Status];
			statusFilter.Property = StatementStatusList.Codes.Complete;
			statusFilter.IsActive = true;

			AssertEquals(FilterCategories.StatusAndFlags, statusFilter.Category);
			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestRepresentativeIdFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement1.B2_ImporterCustomsID = "AAAAAA";
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_ImporterCustomsID = "BBBBBB";

			var representativeIdFilter = (ModuleNumberFilter)filterBizO[StatementFilterStripBusinessObject.Schema.RepresentativeId];
			representativeIdFilter.Property = "AAAAAA";
			representativeIdFilter.IsActive = true;

			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestRepresentativeOrganizationFilter()
		{
			var consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_IsConsignee = true;
			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_IsConsignee = true;

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement1.B2_OH_Importer = consignee1.PK;
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_OH_Importer = consignee2.PK;

			var representativeOrganizationFilter = (ModuleGuidFilter)filterBizO[StatementFilterStripBusinessObject.Schema.RepresentativeOrganization];
			representativeOrganizationFilter.Property = consignee1.PK;
			representativeOrganizationFilter.IsActive = true;

			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestDueDateFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement1.B2_DueDate = ZDate.Today.AddYears(-1);
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_DueDate = ZDate.Today.AddYears(1);

			var dueDateFilter = (ModuleDateFilter)filterBizO[StatementFilterStripBusinessObject.Schema.DueDate];
			dueDateFilter.PropertySearch = ModuleDateFilter.Past;
			dueDateFilter.IsActive = true;

			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestProfileFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement1.B2_EntryFilerCode = "AAAAAA";
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_EntryFilerCode = "BBBBBB";

			var profileFilter = (ModuleNumberFilter)filterBizO[StatementFilterStripBusinessObject.Schema.Profile];
			profileFilter.Property = "AAAAAA";
			profileFilter.IsActive = true;

			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestTypeFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Export;

			var typeFilter = (ModuleTextFilter)filterBizO[StatementFilterStripBusinessObject.Schema.Type];
			typeFilter.Property = StatementEntryTypeList.Codes.Import;
			typeFilter.IsActive = true;

			AssertEquals(FilterCategories.ModesAndTypes, typeFilter.Category);
			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestProcessDateFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement1.B2_ProcessDate = ZDate.Today.AddYears(-1);
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_ProcessDate = ZDate.Today.AddYears(1);

			var processDateFilter = (ModuleDateFilter)filterBizO[StatementFilterStripBusinessObject.Schema.ProcessDate];
			processDateFilter.PropertySearch = ModuleDateFilter.Past;
			processDateFilter.IsActive = true;

			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestStatementNumberFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement1.B2_StatementNumber = "AAAAAA";
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_StatementNumber = "BBBBBB";

			var liquidationNumberFilter = (ModuleNumberFilter)filterBizO[StatementFilterStripBusinessObject.Schema.StatementNumber];
			liquidationNumberFilter.Property = "AAAAAA";
			liquidationNumberFilter.IsActive = true;

			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestReferenceNumberFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement1.B2_StatementType = StatementPeriodicityList.Codes.Day;
			statement1.ChargesDetail.B3_BrokerReference = "002300";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_StatementType = StatementPeriodicityList.Codes.Day;
			statement2.ChargesDetail.B3_BrokerReference = "006700";

			Factory.Save();

			var referenceNumberFilter = (ModuleNumberFilter)filterBizO[StatementFilterStripBusinessObject.Schema.ReferenceNumber];
			referenceNumberFilter.Property = "002300";
			referenceNumberFilter.IsActive = true;

			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestEntryNumberFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement1.B2_StatementType = StatementPeriodicityList.Codes.Day;
			statement1.ChargesDetail.B3_EntryNum = "DCG004600";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement2.B2_StatementType = StatementPeriodicityList.Codes.Day;
			statement2.ChargesDetail.B3_EntryNum = "DCG008900";

			Factory.Save();

			var entryNumberFilter = (ModuleNumberFilter)filterBizO[StatementFilterStripBusinessObject.Schema.EntryNumber];
			entryNumberFilter.Property = "DCG004600";
			entryNumberFilter.IsActive = true;

			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizO = new StatementFilterStripBusinessObject();
		}

		StatementFilterStripBusinessObject filterBizO;
	}
}
