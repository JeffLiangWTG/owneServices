using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TempStorageRegisterLinesFilterBusinessObject))]
	class TempStorageRegisterLinesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFiltersExist()
		{
			var filter = new TempStorageRegisterLinesFilterBusinessObject();
			AssertNotNull("TSDNumber", filter[TempStorageRegisterLinesFilterBusinessObject.Schema.TSDNumber]);
			AssertNotNull("JobReference", filter[TempStorageRegisterLinesFilterBusinessObject.Schema.JobReference]);
			AssertNotNull("PreviousRefType", filter[TempStorageRegisterLinesFilterBusinessObject.Schema.PreviousRefType]);
			AssertNotNull("PreviousRefNumber", filter[TempStorageRegisterLinesFilterBusinessObject.Schema.PreviousRefNumber]);
			AssertNotNull("Status", filter[TempStorageRegisterLinesFilterBusinessObject.Schema.Status]);
			AssertNotNull("RemainingPackagesQuantity", filter[TempStorageRegisterLinesFilterBusinessObject.Schema.RemainingPackagesQuantity]);
			AssertNotNull("PackageType", filter[TempStorageRegisterLinesFilterBusinessObject.Schema.PackageType]);
		}

		public void TestTSDNumberFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "Job1";
			header1.SRH_AppCode = "TES";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "Job2";
			header2.SRH_AppCode = "TES";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "Job23";
			header3.SRH_AppCode = "TES";

			var storageRegline1_1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			storageRegline1_1.FillWithValidTestData();
			storageRegline1_1.SRL_LineNumber = 1;

			var storageRegline2_1 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			storageRegline2_1.FillWithValidTestData();
			storageRegline2_1.SRL_LineNumber = 1;

			var storageRegline3_1 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			storageRegline3_1.FillWithValidTestData();
			storageRegline3_1.SRL_LineNumber = 1;

			Factory.Save();

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterLinesFilterBusinessObject.Schema.TSDNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "Job2";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("line1_1, header's SRH_Reference doesn't match", false, storageRegline1_1.MatchesFilter(filterQuery));
				AssertEquals("line2_1, header's SRH_Reference matches", true, storageRegline2_1.MatchesFilter(filterQuery));
				AssertEquals("line3_1, header's SRH_Reference matches", true, storageRegline3_1.MatchesFilter(filterQuery));
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

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterLinesFilterBusinessObject.Schema.PackageType];
			filter.IsActive = true;
			filter.Property = "1A";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("header1-line1 should match because it has a package of type 1A", true, storageRegline1_1.MatchesFilter(filterQuery));
				AssertEquals("header2-line1 should not match because it has no package of type 1A.", false, storageRegline2_1.MatchesFilter(filterQuery));
				AssertEquals("header3-line1 should not match because it has no package of type 1A", false, storageRegline3_1.MatchesFilter(filterQuery));
				AssertEquals("header3-line2 should match because it has a package of type 1A", true, storageRegline3_2.MatchesFilter(filterQuery));
			});
		}

		public void TestRemainingPackageQuantityFilter()
		{
			var registerHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var registerLine1 = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine1.SRL_LineNumber = 1;
			registerLine1.SRL_PackagesRemaining = 70;
			var registerLine2 = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine2.SRL_LineNumber = 2;
			registerLine2.SRL_PackagesRemaining = 80;
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.RemainingPackagesQuantity];
			filter.IsActive = true;
			filter.Property1 = 60;
			filter.Property2 = 75;

			var filterQuery = filterObject.Filter;
			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("Line1 should match because it has packages remaining (70) in the range.", true, registerLine1.MatchesFilter(filterQuery));
				AssertEquals("Line2 should not match because it has no packages remaining (80) in the range.", false, registerLine2.MatchesFilter(filterQuery));
			});
		}

		public void TestJobReferenceFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "Job1";
			header1.SRH_InternalReference = "JobRef1";
			header1.SRH_AppCode = "TES";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "Job2";
			header2.SRH_InternalReference = "JobRef2";
			header2.SRH_AppCode = "TES";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "Job3";
			header3.SRH_InternalReference = "JobRef23";
			header3.SRH_AppCode = "TES";

			var storageRegline1_1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			storageRegline1_1.FillWithValidTestData();
			storageRegline1_1.SRL_LineNumber = 1;

			var storageRegline2_1 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			storageRegline2_1.FillWithValidTestData();
			storageRegline2_1.SRL_LineNumber = 1;

			var storageRegline3_1 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			storageRegline3_1.FillWithValidTestData();
			storageRegline3_1.SRL_LineNumber = 1;

			Factory.Save();

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterLinesFilterBusinessObject.Schema.JobReference];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "JobRef2";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("line1_1, header's SRH_InternalReference doesn't match", false, storageRegline1_1.MatchesFilter(filterQuery));
				AssertEquals("line2_1, header's SRH_InternalReference matches", true, storageRegline2_1.MatchesFilter(filterQuery));
				AssertEquals("line3_1, header's SRH_InternalReference matches", true, storageRegline3_1.MatchesFilter(filterQuery));
			});
		}

		public void TestPreviousRefNumberFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "Job1";
			header1.SRH_PreviousReference = "REF001";
			header1.SRH_AppCode = "TES";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "Job2";
			header2.SRH_PreviousReference = "REF002";
			header2.SRH_AppCode = "TES";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "Job3";
			header3.SRH_PreviousReference = "REF003";
			header3.SRH_AppCode = "TES";

			var storageRegline1_1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			storageRegline1_1.FillWithValidTestData();
			storageRegline1_1.SRL_LineNumber = 1;

			var storageRegline2_1 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			storageRegline2_1.FillWithValidTestData();
			storageRegline2_1.SRL_LineNumber = 1;

			var storageRegline3_1 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			storageRegline3_1.FillWithValidTestData();
			storageRegline3_1.SRL_LineNumber = 1;

			Factory.Save();

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterLinesFilterBusinessObject.Schema.PreviousRefNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "REF002";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("line1_1, header's SRH_PreviousReference doesn't match", false, storageRegline1_1.MatchesFilter(filterQuery));
				AssertEquals("line2_1, header's SRH_PreviousReference matches", true, storageRegline2_1.MatchesFilter(filterQuery));
				AssertEquals("line3_1, header's SRH_PreviousReference doesn't match", false, storageRegline3_1.MatchesFilter(filterQuery));
			});
		}

		public void TestPreviousRefTypeFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "Job1";
			header1.SRH_PreviousReferenceType = "820";
			header1.SRH_AppCode = "TES";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "Job2";
			header2.SRH_PreviousReferenceType = "821";
			header2.SRH_AppCode = "TES";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "Job3";
			header3.SRH_PreviousReferenceType = "822";
			header3.SRH_AppCode = "TES";

			var storageRegline1_1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			storageRegline1_1.FillWithValidTestData();
			storageRegline1_1.SRL_LineNumber = 1;

			var storageRegline2_1 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			storageRegline2_1.FillWithValidTestData();
			storageRegline2_1.SRL_LineNumber = 1;

			var storageRegline3_1 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			storageRegline3_1.FillWithValidTestData();
			storageRegline3_1.SRL_LineNumber = 1;

			Factory.Save();

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterLinesFilterBusinessObject.Schema.PreviousRefType];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "821";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("line1_1, header's SRH_PreviousReferenceType doesn't match", false, storageRegline1_1.MatchesFilter(filterQuery));
				AssertEquals("line2_1, header's SRH_PreviousReferenceType matches", true, storageRegline2_1.MatchesFilter(filterQuery));
				AssertEquals("line3_1, header's SRH_PreviousReferenceType doesn't match", false, storageRegline3_1.MatchesFilter(filterQuery));
			});
		}

		public void TestStatusFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "Job1";
			header1.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			header1.SRH_AppCode = "TES";
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "Job2";
			header2.SRH_Status = TempStorageDeclarationStatusList.Codes.Closed;
			header2.SRH_AppCode = "TES";
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "Job3";
			header3.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			header3.SRH_AppCode = "TES";

			var storageRegline1_1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			storageRegline1_1.FillWithValidTestData();
			storageRegline1_1.SRL_LineNumber = 1;

			var storageRegline2_1 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			storageRegline2_1.FillWithValidTestData();
			storageRegline2_1.SRL_LineNumber = 1;

			var storageRegline3_1 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			storageRegline3_1.FillWithValidTestData();
			storageRegline3_1.SRL_LineNumber = 1;

			Factory.Save();

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterLinesFilterBusinessObject.Schema.Status];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = TempStorageDeclarationStatusList.Codes.Closed;
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.StatusAndFlags, filter.Category);
				AssertEquals("line1_1, header's SRH_Status doesn't match", false, storageRegline1_1.MatchesFilter(filterQuery));
				AssertEquals("line2_1, header's SRH_Status matches", true, storageRegline2_1.MatchesFilter(filterQuery));
				AssertEquals("line3_1, header's SRH_Status doesn't match", false, storageRegline3_1.MatchesFilter(filterQuery));
			});
		}

		public void TestPremisesFilter()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "TEST1";
			header1.SRH_AppCode = "CO1";
			var premises1 = Factory.New<CusTempStorageRegPremises>();
			premises1.SRP_Code = "Text1";
			premises1.SRP_Type = "ADT";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgHeader.MainAddress.PK;
			header1.SRH_SRP_Premises = premises1.PK;
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;

			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "TEST2";
			header2.SRH_AppCode = "CO2";
			var premises2 = Factory.New<CusTempStorageRegPremises>();
			premises2.SRP_Code = "Text2";
			premises2.SRP_Type = "ADT";
			premises2.SRP_Description = "DESC";
			premises2.SRP_OA_PremisesAddress = orgHeader.MainAddress.PK;
			header2.SRH_SRP_Premises = premises2.PK;
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;

			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "TEST3";
			header3.SRH_AppCode = "CO3";
			var premises3 = Factory.New<CusTempStorageRegPremises>();
			premises3.SRP_Code = "Text3";
			premises3.SRP_Type = "LAM";
			premises3.SRP_Description = "DESC";
			premises3.SRP_OA_PremisesAddress = orgHeader.MainAddress.PK;
			header3.SRH_SRP_Premises = premises3.PK;
			var line3 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			line3.SRL_LineNumber = 1;

			var header4 = Factory.New<CusTempStorageRegHeader>();
			header4.SRH_Reference = "TEST4";
			header4.SRH_AppCode = "CO4";
			var premises4 = Factory.New<CusTempStorageRegPremises>();
			premises4.SRP_Code = "Text4";
			premises4.SRP_Type = "LAM";
			premises4.SRP_CustomsLocation = "ES009999";
			premises4.SRP_Description = "DESC";
			premises4.SRP_OA_PremisesAddress = orgHeader.MainAddress.PK;
			header4.SRH_SRP_Premises = premises4.PK;
			var line4 = (CusTempStorageRegLine)header4.CusTempStorageRegLines.AddNew();
			line4.SRL_LineNumber = 1;

			Factory.Save();

			var filterObject = new TempStorageRegisterLinesFilterBusinessObjectForPremisesFilterTest();
			var filter = (PremisesModuleFilter)filterObject[TempStorageRegisterLinesFilterBusinessObjectForPremisesFilterTest.Schema.Premises];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.PremisesCode = "Text1";
			filter.IsActive = true;

			var query = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Filter by Code: Line1 is correct", true, line1.MatchesFilter(query));
				AssertEquals("Filter by Code: Line2 is not correct", false, line2.MatchesFilter(query));
				AssertEquals("Filter by Code: Line3 is not correct", false, line3.MatchesFilter(query));
				AssertEquals("Filter by Code: Line4 is not correct", false, line4.MatchesFilter(query));

				filter.PremisesCode = ZString.Empty;
				filter.PremisesType = "ADT";
				query = filterObject.Filter;
				AssertEquals("Filter by Type: Line1 is correct", true, line1.MatchesFilter(query));
				AssertEquals("Filter by Type: Line2 is correct", true, line2.MatchesFilter(query));
				AssertEquals("Filter by Type: Line3 is not correct", false, line3.MatchesFilter(query));
				AssertEquals("Filter by Type: Line4 is not correct", false, line4.MatchesFilter(query));

				filter.PremisesType = ZString.Empty;
				filter.PremisesLocation = "ES009999";
				query = filterObject.Filter;
				AssertEquals("Filter by Location: Line1 is not correct", false, line1.MatchesFilter(query));
				AssertEquals("Filter by Location: Line2 is not correct", false, line2.MatchesFilter(query));
				AssertEquals("Filter by Location: Line3 is not correct", false, line3.MatchesFilter(query));
				AssertEquals("Filter by Location: Line4 is correct", true, line4.MatchesFilter(query));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TempStorageRegisterLinesFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterObject = (TempStorageRegisterLinesFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
		TempStorageRegisterLinesFilterBusinessObject filterObject;
	}

	class TempStorageRegisterLinesFilterBusinessObjectForPremisesFilterTest : TempStorageRegisterLinesFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filtersCollection = base.GetModuleFiltersCore();
			AddPremisesFilter(filtersCollection);
			return filtersCollection;
		}
	}
}
