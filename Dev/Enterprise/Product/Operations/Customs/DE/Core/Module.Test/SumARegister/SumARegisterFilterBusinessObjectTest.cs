using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageRegHeader;
using OwnerReferenceTypeList = Enterprise.Customs.DE.Business.OwnerReferenceTypeList;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SumARegisterFilterBusinessObject))]
	sealed class SumARegisterFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFiltersExist()
		{
			var filter = new SumARegisterFilterBusinessObject();
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.ATBNumber]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.ArrivalDate]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.PresentationDate]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.PreviousRefNumber]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.Status]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.CustomerReference]);

			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.LineOwnerReferenceType]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.LineOwnerReferenceNumber]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.LineLimitDate]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.LineCustodianEori]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.LineTraderEori]);
			AssertNotNull(filter[SumARegisterFilterBusinessObject.Schema.LineStatus]);
		}

		public void TestATBNumberFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header1.SRH_Reference = "Job1";
			header1.SRH_AppCode = "SUM";
			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header2.SRH_Reference = "Job2";
			header2.SRH_AppCode = "SUM";
			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header3.SRH_Reference = "Job3";
			header3.SRH_AppCode = "SUM";
			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[SumARegisterFilterBusinessObject.Schema.ATBNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "Job";

			var collection = new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode);
			collection.AdditionalFilter = filterObject.Filter;

			AssertEquals(3, collection.Count);
		}

		public void TestArrivalDateFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header1.SRH_ArrivalDate = ZDate.Today.AddDays(-1);
			header1.SRH_AppCode = "SUM";
			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header2.SRH_ArrivalDate = ZDate.Today;
			header2.SRH_AppCode = "SUM";
			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header3.SRH_ArrivalDate = ZDate.Today.AddDays(5);
			header3.SRH_AppCode = "SUM";
			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[SumARegisterFilterBusinessObject.Schema.ArrivalDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			filter.Property2 = ZDateTime.Today.AddDays(2);

			var collection = new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode);
			collection.AdditionalFilter = filterObject.Filter;

			AssertEquals(2, collection.Count);
		}

		public void TestPresentationDateFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header1.SRH_PresentationDate = ZDate.Today.AddDays(-1);
			header1.SRH_AppCode = "SUM";
			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header2.SRH_PresentationDate = ZDate.Today;
			header2.SRH_AppCode = "SUM";
			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header3.SRH_PresentationDate = ZDate.Today.AddDays(5);
			header3.SRH_AppCode = "SUM";
			Factory.Save();

			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[SumARegisterFilterBusinessObject.Schema.PresentationDate];
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			filter.Property2 = ZDateTime.Today.AddDays(2);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;

			var collection = new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode);
			collection.AdditionalFilter = filterObject.Filter;

			AssertEquals(2, collection.Count);
		}

		public void TestPreviousRefNumberFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header1.SRH_PreviousReference = "REF001";
			header1.SRH_AppCode = "SUM";
			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header2.SRH_PreviousReference = "REF002";
			header2.SRH_AppCode = "SUM";
			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header3.SRH_PreviousReference = "REF003";
			header3.SRH_AppCode = "SUM";
			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[SumARegisterFilterBusinessObject.Schema.PreviousRefNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "REF";

			var collection = new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode);
			collection.AdditionalFilter = filterObject.Filter;

			AssertEquals(3, collection.Count);
		}

		public void TestStatusFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header1.SRH_Status = "ST1";
			header1.SRH_AppCode = "SUM";
			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header2.SRH_Status = "ST2";
			header2.SRH_AppCode = "SUM";
			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header3.SRH_Status = "ST3";
			header3.SRH_AppCode = "SUM";
			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[SumARegisterFilterBusinessObject.Schema.Status];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "ST";

			var collection = new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode);
			collection.AdditionalFilter = filterObject.Filter;

			AssertEquals(3, collection.Count);
		}

		public void TestCustomerReferenceFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header1.SRH_InternalReference = "AXTH09283";
			header1.SRH_AppCode = "SUM";
			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header2.SRH_InternalReference = "BXTZ00023";
			header2.SRH_AppCode = "SUM";
			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header3.SRH_InternalReference = "AXTZ00346";
			header3.SRH_AppCode = "SUM";
			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[SumARegisterFilterBusinessObject.Schema.CustomerReference];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "AXT";
			var filterQuery = filter.Query;

			CombineAssertions(() =>
			{
				AssertEquals("header1", true, header1.MatchesFilter(filterQuery));
				AssertEquals("header2", false, header2.MatchesFilter(filterQuery));
				AssertEquals("header3", true, header3.MatchesFilter(filterQuery));
			});
		}

		public void TestLineOwnerReferenceTypeFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_OwnerReferenceType = OwnerReferenceTypeList.Codes.REG;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;

			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[SumARegisterFilterBusinessObject.Schema.LineOwnerReferenceType];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = OwnerReferenceTypeList.Codes.REG;

			var filterQuery = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 has matching line", true, header1.MatchesFilter(filterQuery));
				AssertEquals("Header2 has no matching line", false, header2.MatchesFilter(filterQuery));
				AssertEquals("Line1 does not match", true, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 matches", false, line2.MatchesFilter(lineQuery));
			});
		}

		public void TestLineOwnerReferenceNumberFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_OwnerReference = "TEST1";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_OwnerReference = "REF1";

			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[SumARegisterFilterBusinessObject.Schema.LineOwnerReferenceNumber];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "REF";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 has no matching line", false, header1.MatchesFilter(filterQuery));
				AssertEquals("Header2 has matching line", true, header2.MatchesFilter(filterQuery));
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(lineQuery));
			});
		}

		[TestDate(2022, 11, 28)]
		public void TestLineLimitDateFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_LimitDate = new ZDate(2022, 11, 28);

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_LimitDate = new ZDate(2022, 11, 20);

			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[SumARegisterFilterBusinessObject.Schema.LineLimitDate];
			filter.Property1 = new ZDateTime(2022, 11, 19);
			filter.Property2 = new ZDateTime(2022, 11, 27);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 line outside of date range", false, header1.MatchesFilter(query));
				AssertEquals("Header2 line date in range", true, header2.MatchesFilter(query));
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(lineQuery));
			});
		}

		public void TestLineCustodianEoriFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_CustodianIdentifier = "DE12345";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_CustodianIdentifier = "DE98765";

			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[SumARegisterFilterBusinessObject.Schema.LineCustodianEori];
			filter.Property = "DE123";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 has matching line", true, header1.MatchesFilter(query));
				AssertEquals("Header2 has no matching line", false, header2.MatchesFilter(query));
				AssertEquals("Line1 matches", true, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 does not match", false, line2.MatchesFilter(lineQuery));
			});
		}

		public void TestLineTraderEoriFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_GoodsOwnerIdentifier = "DE12345";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_GoodsOwnerIdentifier = "DE98765";

			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[SumARegisterFilterBusinessObject.Schema.LineTraderEori];
			filter.Property = "DE123";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 has matching line", true, header1.MatchesFilter(query));
				AssertEquals("Header2 has no matching line", false, header2.MatchesFilter(query));
				AssertEquals("Line1 matches", true, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 does not match", false, line2.MatchesFilter(lineQuery));
			});
		}

		public void TestLineStatusFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_CustomsStatus = Business.CustomsStatusList.Codes.DEL;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_CustomsStatus = Business.CustomsStatusList.Codes.FIN;

			Factory.Save();

			var filterObject = new SumARegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[SumARegisterFilterBusinessObject.Schema.LineStatus];
			filter.Property = Business.CustomsStatusList.Codes.FIN;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 line does not matche", false, header1.MatchesFilter(query));
				AssertEquals("Header2 line matches", true, header2.MatchesFilter(query));
				AssertEquals("Line does not match filter", false, line1.MatchesFilter(lineQuery));
				AssertEquals("Line matches filter", true, line2.MatchesFilter(lineQuery));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new SumARegisterFilterBusinessObject();
	}
}
