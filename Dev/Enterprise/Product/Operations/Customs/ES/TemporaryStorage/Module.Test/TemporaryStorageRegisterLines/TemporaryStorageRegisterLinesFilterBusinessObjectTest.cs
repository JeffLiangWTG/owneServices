using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;
using CusTempStorageRegLineItem = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItem;
using CusTempStorageRegLineItemPivot = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TemporaryStorageRegisterLinesFilterBusinessObject))]
	class TemporaryStorageRegisterLinesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFiltersExist() => CombineAssertions(() =>
		{
			var filter = new TemporaryStorageRegisterLinesFilterBusinessObject();
			AssertNotNull("LineOwnerReferenceNumber", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineOwnerReferenceNumber]);
			AssertNotNull("LineLimitDate", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineLimitDate]);
			AssertNotNull("LineOwnerEORI", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineOwnerEORI]);
			AssertNotNull("LinePackageMarks", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LinePackageMarks]);
			AssertNotNull("LineLocationOfGoods", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineLocationOfGoods]);
			AssertNotNull("LineCustomsStatus", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineCustomsStatus]);
			AssertNotNull("LineItemTSDItemNumber", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineItemTSDItemNumber]);
			AssertNotNull("LineItemTariffCode", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineItemTariffCode]);
			AssertNotNull("LineItemDescription", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineItemDescription]);
			AssertNotNull("LineTransactionEntryDate", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineTransactionEntryDate]);
			AssertNotNull("LineTransactionInternalReference", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineTransactionInternalReference]);
			AssertNotNull("LineTransactionInternalReferenceType", filter[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineTransactionInternalReferenceType]);
			AssertNotNull("Premises", filter[TempStorageRegisterLinesFilterBusinessObject.Schema.Premises]);
		});

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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineOwnerEORI];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "EORI2";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(filterQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(filterQuery));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineItemTSDItemNumber];
			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filter.Property = "12";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(filterQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(filterQuery));
				AssertEquals("Line3 does not match", false, line3.MatchesFilter(filterQuery));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineItemTariffCode];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "0801111";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(filterQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(filterQuery));
				AssertEquals("Line3 does not match", false, line3.MatchesFilter(filterQuery));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineItemDescription];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "BBBB";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(filterQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(filterQuery));
				AssertEquals("Line3 does not match", false, line3.MatchesFilter(filterQuery));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineOwnerReferenceNumber];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "REF";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(filterQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(filterQuery));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineLimitDate];
			filter.Property1 = new ZDateTime(2022, 11, 19);
			filter.Property2 = new ZDateTime(2022, 11, 27);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;

			var query = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(query));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(query));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LinePackageMarks];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "MARK2";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(filterQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(filterQuery));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineLocationOfGoods];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "LOCATION2";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(filterQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(filterQuery));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineCustomsStatus];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "ST2";
			filter.IsActive = true;

			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(filterQuery));
				AssertEquals("Line2 matches", true, line2.MatchesFilter(filterQuery));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineTransactionEntryDate];
			filter.Property1 = new ZDate(2022, 11, 19);
			filter.Property2 = new ZDate(2022, 11, 27);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;

			var query = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 does not match", false, line1.MatchesFilter(query));
				AssertEquals("Line2 date in range and is OBL", true, line2.MatchesFilter(query));
				AssertEquals("Line3 date in range but is not OBL", false, line3.MatchesFilter(query));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineTransactionInternalReference];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "REF1";
			filter.IsActive = true;

			var query = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 is correct", true, line1.MatchesFilter(query));
				AssertEquals("Line2 is not correct", false, line2.MatchesFilter(query));
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

			var filterObject = new TemporaryStorageRegisterLinesFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[TemporaryStorageRegisterLinesFilterBusinessObject.FilterConstants.LineTransactionInternalReferenceType];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "OTH";
			filter.IsActive = true;

			var query = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Line1 matches", true, line1.MatchesFilter(query));
				AssertEquals("Line2 doesn't match", false, line2.MatchesFilter(query));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TemporaryStorageRegisterLinesFilterBusinessObject();
	}
}
