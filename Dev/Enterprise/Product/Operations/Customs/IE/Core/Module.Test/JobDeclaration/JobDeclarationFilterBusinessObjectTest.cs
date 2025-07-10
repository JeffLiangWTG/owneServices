using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	class JobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSupportRequestedProcedure()
		{
			Assert(new JobDeclarationFilterBusinessObject().SupportRequestedProcedure);
		}

		public void TestSupportsExitControl()
		{
			var filterBizObj = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			Assert(filterBizObj.SupportsExitControl);
		}

		public void TestLocalReferenceNumberFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_BGMReference = "IRE001";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_BGMReference = "DE002";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader3 = declaration3.ActiveEntryHeaders.AddNew();
			entryHeader3.CH_BGMReference = "IRE003";
			var entryHeader4 = declaration3.ActiveEntryHeaders.AddNew();
			entryHeader4.CH_BGMReference = "DE004";

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[JobDeclarationFilterBusinessObject.DeclarationFilterConstants.LocalReferenceNumber];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "IRE";
			var filterQuery = filterObj.Filter;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("IRE should match", true, declaration1.MatchesFilter(filterQuery));
				AssertEquals("DE should not match", false, declaration2.MatchesFilter(filterQuery));
				AssertEquals("contains IRE should match", true, declaration3.MatchesFilter(filterQuery));
			});
		}

		public void TestDeclarationTypeFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction1 = declaration1.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "B1";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "B2";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();

			var instruction3 = declaration3.CustomsEntryInstructions.AddNew();
			instruction3.CEI_Style = "B3";
			var instruction4 = declaration3.CustomsEntryInstructions.AddNew();
			instruction4.CEI_Style = "B2";

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[JobDeclarationFilterBusinessObject.DeclarationFilterConstants.DeclarationType];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "B2";
			var filterQuery = filterObj.Filter;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.ModesAndTypes, filter.Category);
				AssertEquals("MaxLength", 7, filter.MaxLength);
				AssertEquals("B1 should not match", false, declaration1.MatchesFilter(filterQuery));
				AssertEquals("B2 should match", true, declaration2.MatchesFilter(filterQuery));
				AssertEquals("contains B2 should match", true, declaration3.MatchesFilter(filterQuery));
			});
		}

		[TestDate(2021, 6, 15)]
		public void TestCustomsDocsReqdByDateFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction1 = declaration1.CustomsEntryInstructions.AddNew();

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction2 = declaration1.CustomsEntryInstructions.AddNew();
			var requestedDocument2 = instruction1.RequestedDocuments.AddNew();

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction3 = declaration3.CustomsEntryInstructions.AddNew();
			var requestedDocument3 = instruction3.RequestedDocuments.AddNew();

			requestedDocument2.CSI_DateOfExpiry = new ZDateTime(2021, 6, 12);
			requestedDocument3.CSI_DateOfExpiry = new ZDateTime(2021, 6, 16);

			Factory.Save();

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsDocsReqdByDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			filter.Property2 = ZDateTime.Today.AddDays(2);
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.Dates, filter.Category);
				AssertEquals("RequestedDocuments empty should not match", false, declaration1.MatchesFilter(filterQuery));
				AssertEquals("RequestedDocuments OUT of range should not match", false, declaration2.MatchesFilter(filterQuery));
				AssertEquals("RequestedDocuments IN range should match", true, declaration3.MatchesFilter(filterQuery));
			});
		}

		public void TestCustomsDocsReqdFlagFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction1 = declaration1.CustomsEntryInstructions.AddNew();

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction2 = declaration1.CustomsEntryInstructions.AddNew();
			var requestedDocument2 = instruction2.RequestedDocuments.AddNew();

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction3 = declaration3.CustomsEntryInstructions.AddNew();
			var requestedDocument3 = instruction3.RequestedDocuments.AddNew();

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction4 = declaration4.CustomsEntryInstructions.AddNew();
			var requestedDocument4 = instruction4.RequestedDocuments.AddNew();

			requestedDocument2.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument3.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			requestedDocument4.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;

			Factory.Save();

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterObj[JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsDocsReqd];
			filter.IsActive = true;
			filter.Property0 = false;
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.StatusAndFlags, filter.Category);
				AssertEquals("When unflagged RequestedDocuments empty should match", true, declaration1.MatchesFilter(filterQuery));
				AssertEquals("When unflagged RequestedDocuments with CSI_Status = 'CAN' should match", true, declaration2.MatchesFilter(filterQuery));
				AssertEquals("When unflagged RequestedDocuments with CSI_Status = 'RCV' should match", true, declaration3.MatchesFilter(filterQuery));
				AssertEquals("When unflagged RequestedDocuments with CSI_Status = 'OPE' should match", true, declaration4.MatchesFilter(filterQuery));
			});

			filter.Property0 = true;
			filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.StatusAndFlags, filter.Category);
				AssertEquals("When flagged RequestedDocuments empty should not match", false, declaration1.MatchesFilter(filterQuery));
				AssertEquals("When flagged RequestedDocuments with CSI_Status = 'CAN' not should match", false, declaration2.MatchesFilter(filterQuery));
				AssertEquals("When flagged RequestedDocuments with CSI_Status = 'RCV' not should match", false, declaration3.MatchesFilter(filterQuery));
				AssertEquals("When flagged RequestedDocuments with CSI_Status = 'OPE' should match", true, declaration4.MatchesFilter(filterQuery));
			});
		}

		public void TestCustomsRegistrationNumberFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var instruction1 = declaration1.CustomsEntryInstructions.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction1.PK;
			entryHeader1.CRN = "CRN-00001";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			entryHeader2.CH_CEI_Instruction = instruction2.PK;
			entryHeader2.CRN = "CRN-00002";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			var instruction3 = declaration3.CustomsEntryInstructions.AddNew();
			entryHeader2.CH_CEI_Instruction = instruction2.PK;

			Factory.Save();

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsRegistrationNumber];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "CRN-00001";
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("CRN in entry of declaration is CRN-00001, should match the filter", true, declaration1.MatchesFilter(filterQuery));
				AssertEquals("CRN in entry of declaration is not CRN-00001, should not match the filter", false, declaration2.MatchesFilter(filterQuery));
				AssertEquals("CRN is empty in entry of declaration, should not match the filter", false, declaration3.MatchesFilter(filterQuery));
			});
		}
		public void TestApplicationCodeFilters()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			if (filterBO != null)
			{
				ModuleTextFilter applicationCode = (ModuleTextFilter)filterBO[filterBO.ApplicationCodeFilterCaption];
				applicationCode.IsActive = true;
				applicationCode.Property = "CCC";
				applicationCode.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				BaseJobDeclaration testDec1 = Factory.New<BaseJobDeclaration>();
				testDec1.JE_ApplicationCode = "AAA";
				BaseJobDeclaration testDec2 = Factory.New<BaseJobDeclaration>();
				testDec2.JE_ApplicationCode = "BBB";
				Factory.Save();
				AssertType<DeclarationApplicationSearchFilterCodeList>(applicationCode.List);
				BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
				collection.Load(filterBO.Filter);
				AssertEquals("Not decs", 0, collection.Count);
				applicationCode.Property = "AAA";
				collection = new BaseJobDeclarationCollection(Factory);
				collection.Load(filterBO.Filter);
				AssertEquals("Decs", 1, collection.Count);
			}
		}

		public void TestLookups()
		{
			AssertType<JobDeclarationFilterLookups>("Lookups", new JobDeclarationFilterBusinessObject().Lookups);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobDeclarationFilterBusinessObject();
	}
}
