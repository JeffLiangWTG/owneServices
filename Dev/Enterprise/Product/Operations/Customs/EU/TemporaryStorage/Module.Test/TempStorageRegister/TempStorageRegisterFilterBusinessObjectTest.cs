using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TempStorageRegisterFilterBusinessObject))]
	class TempStorageRegisterFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCustomsOfficeTextFilterFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "Job1";
			header1.SRH_CustomsOffice = "FR123456";
			header1.SRH_AppCode = "TES";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "Job2";
			header2.SRH_AppCode = "TES";
			header2.SRH_CustomsOffice = "FR654321";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "Job3";
			header3.SRH_AppCode = "TES";
			Factory.Save();

			var filter = (ModuleNkFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.CustomsOffice];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
			filter.Property = "FR123456";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("header1, SRH_CustomsOffice matches", true, header1.MatchesFilter(filterQuery));
				AssertEquals("header2, SRH_CustomsOffice doesn't match", false, header2.MatchesFilter(filterQuery));
				AssertEquals("header3, SRH_CustomsOffice doesn't match", false, header3.MatchesFilter(filterQuery));
			});
		}

		public void TestPackageTypeTextFilterFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "Job1";
			header1.SRH_AppCode = "TES";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "Job2";
			header2.SRH_AppCode = "TES";

			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "Job3";
			header3.SRH_AppCode = "TES";

			var header4 = Factory.New<CusTempStorageRegHeader>();
			header4.SRH_Reference = "Job4";
			header4.SRH_AppCode = "TES";

			var storageRegline1_1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			storageRegline1_1.FillWithValidTestData();
			storageRegline1_1.SRL_LineNumber = 1;
			storageRegline1_1.SRL_LocationOfGoods = "SRLlocationOfGoods";
			storageRegline1_1.SRL_PackagesRemaining = 2;
			storageRegline1_1.SRL_PackageType = "1A";

			var storageRegline2_1 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			storageRegline2_1.FillWithValidTestData();
			storageRegline2_1.SRL_LineNumber = 1;
			storageRegline2_1.SRL_LocationOfGoods = "SRLlocationOfGoods";
			storageRegline2_1.SRL_PackagesRemaining = 2;
			storageRegline2_1.SRL_PackageType = "1B";

			var storageRegline3_1 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			storageRegline3_1.FillWithValidTestData();
			storageRegline3_1.SRL_LineNumber = 1;
			storageRegline3_1.SRL_LocationOfGoods = "SRLlocationOfGoods";
			storageRegline3_1.SRL_PackagesRemaining = 2;
			storageRegline3_1.SRL_PackageType = "1C";

			var storageRegline3_2 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			storageRegline3_2.FillWithValidTestData();
			storageRegline3_2.SRL_LineNumber = 2;
			storageRegline3_2.SRL_LocationOfGoods = "SRLlocationOfGoods";
			storageRegline3_2.SRL_PackagesRemaining = 2;
			storageRegline3_2.SRL_PackageType = "1A";

			Factory.Save();

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.PackageType];
			filter.IsActive = true;
			filter.Property = "1A";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("header1 should match because it has a package of type 1A", true, header1.MatchesFilter(filterQuery));
				AssertEquals("header2 should not match because it has no package of type 1A.", false, header2.MatchesFilter(filterQuery));
				AssertEquals("header3 should match because it has a package of type 1A", true, header3.MatchesFilter(filterQuery));
				AssertEquals("header4 should not match because it has no package of type 1A", false, header4.MatchesFilter(filterQuery));
			});
		}

		public void TestRemainingPackageQuantityFilter()
		{
			var registerHeader1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();

			var registerHeader2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var registerLine1 = registerHeader2.CusTempStorageRegLines.AddNew();
			registerLine1.SRL_LineNumber = 1;
			registerLine1.SRL_PackagesRemaining = 70;
			var registerLine2 = registerHeader2.CusTempStorageRegLines.AddNew();
			registerLine2.SRL_LineNumber = 2;
			registerLine2.SRL_PackagesRemaining = 80;
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.RemainingPackagesQuantity];
			filter.IsActive = true;
			filter.Property1 = 140;
			filter.Property2 = 160;

			var filterQuery = filterObject.Filter;
			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("RegisterHeader1 should not match because it has not enough packages remaining (0).", false, registerHeader1.MatchesFilter(filterQuery));
				AssertEquals("RegisterHeader2 should match because it has enough packages remaining (150).", true, registerHeader2.MatchesFilter(filterQuery));
			});
		}

		public void TestDDTNumberFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "Job1";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "Job2";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "Job23";

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.DDTNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "Job2";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("header1, SRH_Reference doesn't match", false, header1.MatchesFilter(filterQuery));
				AssertEquals("header2, SRH_Reference matches", true, header2.MatchesFilter(filterQuery));
				AssertEquals("header3, SRH_Reference matches", true, header3.MatchesFilter(filterQuery));
			});
		}

		public void TestJobReferenceFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_InternalReference = "JobRef1";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_InternalReference = "JobRef2";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_InternalReference = "JobRef23";

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.JobReference];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "JobRef2";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("header1, SRH_InternalReference doesn't match", false, header1.MatchesFilter(filterQuery));
				AssertEquals("header2, SRH_InternalReference matches", true, header2.MatchesFilter(filterQuery));
				AssertEquals("header3, SRH_InternalReference matches", true, header3.MatchesFilter(filterQuery));
			});
		}

		public void TestArrivalDateFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_ArrivalDate = ZDate.Today.AddDays(-1);
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_ArrivalDate = ZDate.Today;
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_ArrivalDate = ZDate.Today.AddDays(5);

			var filter = (ModuleDateFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.ArrivalDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			filter.Property2 = ZDateTime.Today.AddDays(2);
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.Dates, filter.Category);
				AssertEquals("header1, SRH_ArrivalDate matches", true, header1.MatchesFilter(filterQuery));
				AssertEquals("header2, SRH_ArrivalDate matches", true, header2.MatchesFilter(filterQuery));
				AssertEquals("header3, SRH_ArrivalDate doesn't match", false, header3.MatchesFilter(filterQuery));
			});
		}

		public void TestPresentationDateFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_PresentationDate = ZDate.Today.AddDays(-1);
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_PresentationDate = ZDate.Today;
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_PresentationDate = ZDate.Today.AddDays(5);

			var filter = (ModuleDateFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.PresentationDate];
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			filter.Property2 = ZDateTime.Today.AddDays(2);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.Dates, filter.Category);
				AssertEquals("header1, SRH_PresentationDate matches", true, header1.MatchesFilter(filterQuery));
				AssertEquals("header2, SRH_PresentationDate matches", true, header2.MatchesFilter(filterQuery));
				AssertEquals("header3, SRH_PresentationDate doesn't match", false, header3.MatchesFilter(filterQuery));
			});
		}

		public void TestPreviousRefNumberFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_PreviousReference = "REF001";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_PreviousReference = "REF002";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_PreviousReference = "REF003";

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.PreviousRefNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "REF002";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("header1, SRH_PreviousReference doesn't match", false, header1.MatchesFilter(filterQuery));
				AssertEquals("header2, SRH_PreviousReference matches", true, header2.MatchesFilter(filterQuery));
				AssertEquals("header3, SRH_PreviousReference doesn't match", false, header3.MatchesFilter(filterQuery));
			});
		}

		public void TestPreviousRefTypeFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_PreviousReferenceType = "820";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_PreviousReferenceType = "821";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_PreviousReferenceType = "822";

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.PreviousRefType];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "822";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertType<CodeDescriptionPairList>("List type", filter.List);
				AssertEquals("Comparison operators", "exact", filter.ComparisonOperator_List.CodesAsString);

				AssertEquals("header1, SRH_PreviousReferenceType doesn't match", false, header1.MatchesFilter(filterQuery));
				AssertEquals("header2, SRH_PreviousReferenceType doesn't match", false, header2.MatchesFilter(filterQuery));
				AssertEquals("header3, SRH_PreviousReferenceType matches", true, header3.MatchesFilter(filterQuery));
			});
		}

		public void TestStatusFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Status = TempStorageDeclarationStatusList.Codes.Closed;
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.Status];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = TempStorageDeclarationStatusList.Codes.Open;
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.StatusAndFlags, filter.Category);
				AssertType<CodeDescriptionPairList>("List type", filter.List);
				AssertEquals("Comparison operators", "exact", filter.ComparisonOperator_List.CodesAsString);

				AssertEquals("header1, SRH_Status matches", true, header1.MatchesFilter(filterQuery));
				AssertEquals("header2, SRH_Status doesn't match", false, header2.MatchesFilter(filterQuery));
				AssertEquals("header3, SRH_Status matches", true, header3.MatchesFilter(filterQuery));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TempStorageRegisterFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterObject = (TempStorageRegisterFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
		TempStorageRegisterFilterBusinessObject filterObject;
	}
}
