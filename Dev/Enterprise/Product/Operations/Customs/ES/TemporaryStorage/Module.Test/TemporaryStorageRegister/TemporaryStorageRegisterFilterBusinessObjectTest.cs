using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.TemporaryStorage.Module;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;
using CusTempStorageRegLineItem = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItem;
using CusTempStorageRegLineItemPivot = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.ES.Module.Testing
{
	[TestedType(typeof(TemporaryStorageRegisterFilterBusinessObject))]
	sealed class TemporaryStorageRegisterFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFiltersExist()
		{
			var filter = new TemporaryStorageRegisterFilterBusinessObject();
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.CustomerReference]);

			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineOwnerReferenceNumber]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineLimitDate]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineCustodianEori]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineOwnerEORI]);

			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LinePackageMarks]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineLocationOfGoods]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineCustomsStatus]);

			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineOwnerReferenceNumber]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineLimitDate]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineCustodianEori]);

			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineItemTSDItemNumber]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineItemTariffCode]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineItemDescription]);

			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionExitDate]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionEntryDate]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionDeclarationDate]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionReferenceType]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionReference]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionInternalReference]);
			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionInternalReferenceType]);

			AssertNotNull(filter[TemporaryStorageRegisterFilterBusinessObject.Schema.Premises]);
		}

		public void TestLineCustodianEoriFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_CustodianIdentifier = "DE12345";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_CustodianIdentifier = "DE98765";

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineCustodianEori];
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

		public void TestLineOwnerEORIFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_GoodsOwnerIdentifier = "EORI1";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_GoodsOwnerIdentifier = "EORI2";

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineOwnerEORI];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "EORI2";
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

		public void TestLineTSDItemNumberFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var lineItem1 = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
			var lineItemPivot1 = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot1.SRV_SRL_Line = line1.PK;
			lineItemPivot1.SRV_SRI_Item = lineItem1.PK;
			lineItemPivot1.RegLineItem.SRI_GoodsItemNumber = 11;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var lineItem2 = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
			var lineItemPivot2 = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot2.SRV_SRL_Line = line2.PK;
			lineItemPivot2.SRV_SRI_Item = lineItem2.PK;
			lineItemPivot2.RegLineItem.SRI_GoodsItemNumber = 12;

			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line3 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			line3.SRL_LineNumber = 1;
			var lineItem3 = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
			var lineItemPivot3 = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot3.SRV_SRL_Line = line3.PK;
			lineItemPivot3.SRV_SRI_Item = lineItem3.PK;
			lineItemPivot3.RegLineItem.SRI_GoodsItemNumber = 13;

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineItemTSDItemNumber];
			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filter.Property = "12";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 has no matching line", false, header1.MatchesFilter(filterQuery));
				AssertEquals("Header2 has matching line", true, header2.MatchesFilter(filterQuery));
				AssertEquals("Header3 has no matching line", false, header3.MatchesFilter(filterQuery));
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(lineQuery));
				AssertEquals("Line3 does not match", false, line3.MatchesFilter(lineQuery));
			});
		}

		public void TestLineTariffCodeFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var lineItem1 = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
			var lineItemPivot1 = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot1.SRV_SRL_Line = line1.PK;
			lineItemPivot1.SRV_SRI_Item = lineItem1.PK;
			lineItemPivot1.RegLineItem.SRI_Tariff = "0801222";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var lineItem2 = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
			var lineItemPivot2 = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot2.SRV_SRL_Line = line2.PK;
			lineItemPivot2.SRV_SRI_Item = lineItem2.PK;
			lineItemPivot2.RegLineItem.SRI_Tariff = "0801111";

			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line3 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			line3.SRL_LineNumber = 1;
			var lineItem3 = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
			var lineItemPivot3 = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot3.SRV_SRL_Line = line3.PK;
			lineItemPivot3.SRV_SRI_Item = lineItem3.PK;
			lineItemPivot3.RegLineItem.SRI_Tariff = "0801333";

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineItemTariffCode];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "0801111";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 has no matching line", false, header1.MatchesFilter(filterQuery));
				AssertEquals("Header2 has matching line", true, header2.MatchesFilter(filterQuery));
				AssertEquals("Header3 has no matching line", false, header3.MatchesFilter(filterQuery));
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(lineQuery));
				AssertEquals("Line3 does not match", false, line3.MatchesFilter(lineQuery));
			});
		}

		public void TestLineDescriptionFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var lineItem1 = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
			var lineItemPivot1 = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot1.SRV_SRL_Line = line1.PK;
			lineItemPivot1.SRV_SRI_Item = lineItem1.PK;
			lineItemPivot1.RegLineItem.SRI_GoodsDescription = "CCCC";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var lineItem2 = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
			var lineItemPivot2 = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot2.SRV_SRL_Line = line2.PK;
			lineItemPivot2.SRV_SRI_Item = lineItem2.PK;
			lineItemPivot2.RegLineItem.SRI_GoodsDescription = "BBBB";

			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line3 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			line3.SRL_LineNumber = 1;
			var lineItem3 = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
			var lineItemPivot3 = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot3.SRV_SRL_Line = line3.PK;
			lineItemPivot3.SRV_SRI_Item = lineItem3.PK;
			lineItemPivot3.RegLineItem.SRI_GoodsDescription = "AAAA";

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineItemDescription];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "BBBB";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 has no matching line", false, header1.MatchesFilter(filterQuery));
				AssertEquals("Header2 has matching line", true, header2.MatchesFilter(filterQuery));
				AssertEquals("Header3 has no matching line", false, header3.MatchesFilter(filterQuery));
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(lineQuery));
				AssertEquals("Line3 does not match", false, line3.MatchesFilter(lineQuery));
			});
		}

		public void TestCustomerReferenceFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header1.SRH_InternalReference = "AXTH09283";
			header1.SRH_AppCode = "ADT";
			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header2.SRH_InternalReference = "BXTZ00023";
			header2.SRH_AppCode = "ADT";
			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header3.SRH_InternalReference = "AXTZ00346";
			header3.SRH_AppCode = "ADT";
			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.CustomerReference];
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

		public void TestLineOwnerReferenceNumberFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_OwnerReference = "TEST1";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_OwnerReference = "REF1";

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineOwnerReferenceNumber];
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
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_LimitDate = new ZDate(2022, 11, 28);

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_LimitDate = new ZDate(2022, 11, 20);

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineLimitDate];
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

		public void TestLinePackageMarksFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_PackageMarks = "MARK1";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_PackageMarks = "MARK2";

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LinePackageMarks];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "MARK2";
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

		public void TestLineLocationOfGoodsFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_LocationOfGoods = "LOCATION1";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_LocationOfGoods = "LOCATION2";

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineLocationOfGoods];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "LOCATION2";
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

		public void TestLineCustomsStatusFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			line1.SRL_CustomsStatus = "ST1";

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			line2.SRL_CustomsStatus = "ST2";

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineCustomsStatus];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "ST2";
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
		public void TestLineTransactionExitDateFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var transaction11 = line1.CusTempStorageRegLineTransactions.AddNew();
			transaction11.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 11, 28);
			transaction11.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Adjustment;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var transaction12 = line2.CusTempStorageRegLineTransactions.AddNew();
			transaction12.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 11, 20);
			transaction12.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;

			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line3 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			line3.SRL_LineNumber = 1;
			var transaction13 = line3.CusTempStorageRegLineTransactions.AddNew();
			transaction13.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 11, 20);
			transaction13.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;

			var header4 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line4 = (CusTempStorageRegLine)header4.CusTempStorageRegLines.AddNew();
			line4.SRL_LineNumber = 1;
			var transaction14 = line4.CusTempStorageRegLineTransactions.AddNew();
			transaction14.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 11, 20);
			transaction14.SRT_TransactionType = TransactionTypes.AdjustmentNote;

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionExitDate];
			filter.Property1 = new ZDate(2022, 11, 19);
			filter.Property2 = new ZDate(2022, 11, 27);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 line outside of date range", false, header1.MatchesFilter(query));
				AssertEquals("Header2 line date in range and is TRN", true, header2.MatchesFilter(query));
				AssertEquals("Header3 line date in range but is not TRN / ADJ", false, header3.MatchesFilter(query));
				AssertEquals("Header2 line date in range and is ADJ", true, header4.MatchesFilter(query));
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 date in range and is TRN", true, line2.MatchesFilter(lineQuery));
				AssertEquals("Line3 date in range but is not TRN / ADJ", false, line3.MatchesFilter(lineQuery));
				AssertEquals("Line2 date in range and is ADJ", true, line4.MatchesFilter(lineQuery));
			});
		}

		[TestDate(2022, 11, 28)]
		public void TestLineTransactionEntryDateFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var transaction11 = line1.CusTempStorageRegLineTransactions.AddNew();
			transaction11.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 11, 28);
			transaction11.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var transaction12 = line2.CusTempStorageRegLineTransactions.AddNew();
			transaction12.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 11, 20);
			transaction12.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;

			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line3 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			line3.SRL_LineNumber = 1;
			var transaction13 = line3.CusTempStorageRegLineTransactions.AddNew();
			transaction13.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 11, 20);
			transaction13.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionEntryDate];
			filter.Property1 = new ZDate(2022, 11, 19);
			filter.Property2 = new ZDate(2022, 11, 27);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 line outside of date range", false, header1.MatchesFilter(query));
				AssertEquals("Header2 line date in range and is OBL", true, header2.MatchesFilter(query));
				AssertEquals("Header3 line date in range but is not OBL", false, header3.MatchesFilter(query));
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 date in range and is OBL", true, line2.MatchesFilter(lineQuery));
				AssertEquals("Line3 date in range but is not OBL", false, line3.MatchesFilter(lineQuery));
			});
		}

		[TestDate(2022, 11, 28)]
		public void TestLineTransactionDeclarationDateFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var transaction11 = line1.CusTempStorageRegLineTransactions.AddNew();
			transaction11.SRT_TransactionDate = new ZDateTimeOffset(2022, 11, 28);
			transaction11.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction11.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var transaction12 = line2.CusTempStorageRegLineTransactions.AddNew();
			transaction12.SRT_TransactionDate = new ZDateTimeOffset(2022, 11, 20);
			transaction12.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction12.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			var header3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line3 = (CusTempStorageRegLine)header3.CusTempStorageRegLines.AddNew();
			line3.SRL_LineNumber = 1;
			var transaction13 = line3.CusTempStorageRegLineTransactions.AddNew();
			transaction13.SRT_TransactionDate = new ZDateTimeOffset(2022, 11, 20);
			transaction13.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction13.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionDeclarationDate];
			filter.Property1 = new ZDate(2022, 11, 19);
			filter.Property2 = new ZDate(2022, 11, 27);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 line outside of date range", false, header1.MatchesFilter(query));
				AssertEquals("Header2 line date in range and not is DEL", true, header2.MatchesFilter(query));
				AssertEquals("Header3 line date in range but is DEL", false, header3.MatchesFilter(query));
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 date in range and not is DEL", true, line2.MatchesFilter(lineQuery));
				AssertEquals("Line3 date in range but is DEL", false, line3.MatchesFilter(lineQuery));
			});
		}

		public void TestLineTransactionReferenceTypeFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var transaction11 = line1.CusTempStorageRegLineTransactions.AddNew();
			transaction11.SRT_ReferenceType = "AA";
			transaction11.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction11.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var transaction12 = line2.CusTempStorageRegLineTransactions.AddNew();
			transaction12.SRT_ReferenceType = "BB";
			transaction12.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction12.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionReferenceType];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "AA";
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 is correct ", true, header1.MatchesFilter(query));
				AssertEquals("Header2 is not correct", false, header2.MatchesFilter(query));
				AssertEquals("Line1 is correct", true, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 is not correct", false, line2.MatchesFilter(lineQuery));
			});
		}

		public void TestLineTransactionReferenceFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var transaction11 = line1.CusTempStorageRegLineTransactions.AddNew();
			transaction11.SRT_Reference = "REF1";
			transaction11.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction11.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var transaction12 = line2.CusTempStorageRegLineTransactions.AddNew();
			transaction12.SRT_Reference = "REF2";
			transaction12.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction12.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionReference];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "REF1";
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 is correct ", true, header1.MatchesFilter(query));
				AssertEquals("Header2 is not correct", false, header2.MatchesFilter(query));
				AssertEquals("Line1 is correct", true, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 is not correct", false, line2.MatchesFilter(lineQuery));
			});
		}

		public void TestLineTransactionInternalReferenceFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var transaction11 = line1.CusTempStorageRegLineTransactions.AddNew();
			transaction11.SRT_InternalReferenceNumber = "REF1";
			transaction11.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction11.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var transaction12 = line2.CusTempStorageRegLineTransactions.AddNew();
			transaction12.SRT_InternalReferenceNumber = "REF2";
			transaction12.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction12.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionInternalReference];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "REF1";
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 is correct ", true, header1.MatchesFilter(query));
				AssertEquals("Header2 is not correct", false, header2.MatchesFilter(query));
				AssertEquals("Line1 is correct", true, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 is not correct", false, line2.MatchesFilter(lineQuery));
			});
		}

		public void TestLineTransactionInternalReferenceTypeFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line1 = (CusTempStorageRegLine)header1.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var transaction11 = line1.CusTempStorageRegLineTransactions.AddNew();
			transaction11.SRT_InternalReferenceType = "OTH";
			transaction11.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction11.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			var header2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line2 = (CusTempStorageRegLine)header2.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 1;
			var transaction12 = line2.CusTempStorageRegLineTransactions.AddNew();
			transaction12.SRT_InternalReferenceType = "TRA";
			transaction12.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction12.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.FilterConstants.LineTransactionInternalReferenceType];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "OTH";
			filter.IsActive = true;

			var query = filterObject.Filter;
			var lineQuery = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals("Header1 is correct ", true, header1.MatchesFilter(query));
				AssertEquals("Header2 is not correct", false, header2.MatchesFilter(query));
				AssertEquals("Line1 is correct", true, line1.MatchesFilter(lineQuery));
				AssertEquals("Line2 is not correct", false, line2.MatchesFilter(lineQuery));
			});
		}

		public void TestPremisesFilter()
		{
			var orgHeader = SetUpOrgHeader();
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "TEST1";
			header1.SRH_AppCode = "CO1";
			var premises1 = Factory.New<CusTempStorageRegPremises>();
			premises1.SRP_Code = "Text1";
			premises1.SRP_Type = "ADT";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgHeader.MainAddress.PK;
			header1.SRH_SRP_Premises = premises1.PK;

			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "TEST2";
			header2.SRH_AppCode = "CO2";
			var premises2 = Factory.New<CusTempStorageRegPremises>();
			premises2.SRP_Code = "Text2";
			premises2.SRP_Type = "ADT";
			premises2.SRP_Description = "DESC";
			premises2.SRP_OA_PremisesAddress = orgHeader.MainAddress.PK;
			header2.SRH_SRP_Premises = premises2.PK;

			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "TEST3";
			header3.SRH_AppCode = "CO3";
			var premises3 = Factory.New<CusTempStorageRegPremises>();
			premises3.SRP_Code = "Text3";
			premises3.SRP_Type = "LAM";
			premises3.SRP_Description = "DESC";
			premises3.SRP_OA_PremisesAddress = orgHeader.MainAddress.PK;
			header3.SRH_SRP_Premises = premises3.PK;

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

			Factory.Save();

			var filterObject = new TemporaryStorageRegisterFilterBusinessObject();
			var filter = (PremisesModuleFilter)filterObject[TemporaryStorageRegisterFilterBusinessObject.Schema.Premises];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.PremisesCode = "Text1";
			filter.IsActive = true;

			var query = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Filter by Code: Header1 is correct", true, header1.MatchesFilter(query));
				AssertEquals("Filter by Code: Header2 is not correct", false, header2.MatchesFilter(query));
				AssertEquals("Filter by Code: Header3 is not correct", false, header3.MatchesFilter(query));
				AssertEquals("Filter by Code: Header4 is not correct", false, header4.MatchesFilter(query));

				filter.PremisesCode = ZString.Empty;
				filter.PremisesType = "ADT";
				query = filterObject.Filter;
				AssertEquals("Filter by Type: Header1 is correct", true, header1.MatchesFilter(query));
				AssertEquals("Filter by Type: Header2 is correct", true, header2.MatchesFilter(query));
				AssertEquals("Filter by Type: Header3 is not correct", false, header3.MatchesFilter(query));
				AssertEquals("Filter by Type: Header4 is not correct", false, header4.MatchesFilter(query));

				filter.PremisesType = ZString.Empty;
				filter.PremisesLocation = "ES009999";
				query = filterObject.Filter;
				AssertEquals("Filter by Location: Header1 is not correct", false, header1.MatchesFilter(query));
				AssertEquals("Filter by Location: Header2 is not correct", false, header2.MatchesFilter(query));
				AssertEquals("Filter by Location: Header3 is not correct", false, header3.MatchesFilter(query));
				AssertEquals("Filter by Location: Header4 is correct", true, header4.MatchesFilter(query));
			});
		}

		OrgHeader SetUpOrgHeader()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			return orgHeader;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TemporaryStorageRegisterFilterBusinessObject();
	}
}
