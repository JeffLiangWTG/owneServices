using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(CustomsSummaryFilterStripBusinessObject))]
sealed class CustomsSummaryFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CustomsSummaryFilterStripBusinessObject();

	public void TestAccountNumberFilterProperty()
	{
		var accountNumberFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.AccountNumber];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Account Number", accountNumberFilter.Description);
			AssertEquals("Category", FilterCategories.NumbersAndReferences, accountNumberFilter.Category);
		});
	}

	public void TestAccountNumberFilter()
	{
		var summary1 = Factory.New<CustomsSummaryHeader>();
		summary1.B2_AccountNo = "1234";
		var summary2 = Factory.New<CustomsSummaryHeader>();
		summary2.B2_AccountNo = "5678";
		Factory.Save();

		var summaryLine1 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine1.B3_B2 = summary1.PK;
		var summaryLine2 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine2.B3_B2 = summary2.PK;
		Factory.Save();

		var accountNumberFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.AccountNumber];
		accountNumberFilter.IsActive = true;
		accountNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

		CombineAssertions(() =>
		{
			accountNumberFilter.Property = "1234";
			AssertEquals("matches summary header 1 account No.", expected: true, summaryLine1.MatchesFilter(filter.Filter));
			AssertEquals("does not match summary header 2 account No.", expected: false, summaryLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestSummaryDateFilterProperty()
	{
		var summaryDateFilter = (ModuleDateFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.SummaryDate];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Summary Date", summaryDateFilter.Description);
			AssertEquals("Category", FilterCategories.Dates, summaryDateFilter.Category);
		});
	}

	public void TestSummaryDateFilter()
	{
		var summary1 = Factory.New<CustomsSummaryHeader>();
		summary1.B2_ProcessDate = ZDate.Today.AddDays(-1);
		var summary2 = Factory.New<CustomsSummaryHeader>();
		summary2.B2_ProcessDate = ZDate.Today.AddDays(5);
		Factory.Save();

		var summaryLine1 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine1.B3_B2 = summary1.PK;
		var summaryLine2 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine2.B3_B2 = summary2.PK;
		Factory.Save();

		var summaryDateFilter = (ModuleDateFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.SummaryDate];
		summaryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		summaryDateFilter.IsActive = true;
		summaryDateFilter.Property1 = ZDateTime.Today.AddDays(-2);
		summaryDateFilter.Property2 = ZDateTime.Today.AddDays(2);
		CombineAssertions(() =>
		{
			AssertEquals("matches summary header 1 summary date", expected: true, summaryLine1.MatchesFilter(filter.Filter));
			AssertEquals("does not match summary header 2 summary date", expected: false, summaryLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestSummaryNumberFilterProperty()
	{
		var summaryNumberFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.SummaryNumber];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Summary Number", summaryNumberFilter.Description);
			AssertEquals("Category", FilterCategories.NumbersAndReferences, summaryNumberFilter.Category);
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, summaryNumberFilter.Visibility);
		});
	}

	public void TestSummaryNumberFilter()
	{
		var summary1 = Factory.New<CustomsSummaryHeader>();
		summary1.B2_StatementNumber = "1234";
		var summary2 = Factory.New<CustomsSummaryHeader>();
		summary2.B2_StatementNumber = "5678";
		Factory.Save();

		var summaryLine1 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine1.B3_B2 = summary1.PK;
		var summaryLine2 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine2.B3_B2 = summary2.PK;
		Factory.Save();

		var summaryNumberFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.SummaryNumber];
		summaryNumberFilter.IsActive = true;
		summaryNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

		CombineAssertions(() =>
		{
			summaryNumberFilter.Property = "1234";
			AssertEquals("matches summary header 1 summary No.", expected: true, summaryLine1.MatchesFilter(filter.Filter));
			AssertEquals("does not match summary header 2 summary No.", expected: false, summaryLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestEvvDocumentReceiveFilterProperty()
	{
		var evvDocumentRecieveFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.EvvDocumentReceiveStatus];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "eVV Document Receive Status", evvDocumentRecieveFilter.Description);
			AssertEquals("Category", FilterCategories.StatusAndFlags, evvDocumentRecieveFilter.Category);
		});
	}

	public void TestEvvDocumentReceiveFilter()
	{
		var list = new BordereauReceivedStatusList().GetAllCodes();
		var summaryLineList = new List<CustomsSummaryLine>();
		foreach (var item in list)
		{
			var summaryLine = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
			summaryLine.B3_Status = item;
			summaryLineList.Add(summaryLine);
		}
		Factory.Save();

		var evvDocumentRecieveFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.EvvDocumentReceiveStatus];
		evvDocumentRecieveFilter.IsActive = true;

		CombineAssertions(() =>
		{
			evvDocumentRecieveFilter.Property = BordereauReceivedStatusList.Codes.Sent;
			AssertEquals("matches evv document recieve status", expected: true, summaryLineList[0].MatchesFilter(filter.Filter));

			foreach (var summaryLine in summaryLineList.Except(summaryLineList.FirstOrDefault(o => o.B3_Status == BordereauReceivedStatusList.Codes.Sent)))
			{
				AssertEquals("does not match evv document recieve status", expected: false, summaryLine.MatchesFilter(filter.Filter));
			}
		});
	}

	public void TestEntryNumberFilterProperty()
	{
		var entryNumberFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.EntryNumber];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Entry Number", entryNumberFilter.Description);
			AssertEquals("Category", FilterCategories.NumbersAndReferences, entryNumberFilter.Category);
		});
	}

	public void TestEntryNumberFilter()
	{
		var summaryLine1 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine1.B3_EntryNum = "123";
		var summaryLine2 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine2.B3_EntryNum = "456";
		Factory.Save();

		var entryNumberFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.EntryNumber];
		entryNumberFilter.IsActive = true;
		entryNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

		CombineAssertions(() =>
		{
			entryNumberFilter.Property = "123";
			AssertEquals("matches summary line 1 entry No.", expected: true, summaryLine1.MatchesFilter(filter.Filter));
			AssertEquals("does not match summary Line 2 entry No.", expected: false, summaryLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestTraderReferenceFilterProperty()
	{
		var traderReferenceFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.TraderReference];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Trader Reference", traderReferenceFilter.Description);
			AssertEquals("Category", FilterCategories.NumbersAndReferences, traderReferenceFilter.Category);
		});
	}

	public void TestTraderReferenceFilter()
	{
		var summaryLine1 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine1.B3_BrokerReference = "123";
		var summaryLine2 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		summaryLine2.B3_BrokerReference = "456";
		Factory.Save();

		var traderReferenceFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.TraderReference];
		traderReferenceFilter.IsActive = true;
		traderReferenceFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

		CombineAssertions(() =>
		{
			traderReferenceFilter.Property = "123";
			AssertEquals("matches summary line 1 Trader Reference", expected: true, summaryLine1.MatchesFilter(filter.Filter));
			AssertEquals("does not match summary Line 2 Trader Reference", expected: false, summaryLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestEvvDocumentTypeFilterProperty()
	{
		var evvDocumentTypeFilter = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.EvvDocumentType];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "eVV Document Type", evvDocumentTypeFilter.Description);
			AssertEquals("Category", FilterCategories.ModesAndTypes, evvDocumentTypeFilter.Category);
		});
	}

	public void TestEvvDocumentTypeFilter()
	{
		var list = new BordereauChargeTypeList().GetAllCodes();
		var summaryLineList = new List<CustomsSummaryLine>();
		foreach (var item in list)
		{
			var summaryLine = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
			summaryLine.LineCharge.B4_ChargeType = item;
			summaryLineList.Add(summaryLine);
		}
		Factory.Save();

		var evvDocumentType = (ModuleTextFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.EvvDocumentType];
		evvDocumentType.IsActive = true;

		CombineAssertions(() =>
		{
			evvDocumentType.Property = BordereauChargeTypeList.Codes.Duties;
			AssertEquals("matches evv document type", expected: true, summaryLineList[0].MatchesFilter(filter.Filter));

			foreach (var summaryLine in summaryLineList.Except(summaryLineList.FirstOrDefault(o => o.ChargeType == BordereauChargeTypeList.Codes.Duties)))
			{
				AssertEquals("does not match evv document type", expected: false, summaryLine.MatchesFilter(filter.Filter));
			}
		});
	}

	public void TestAmountFilterProperty()
	{
		var amountFilter = (ModuleNumberRangeFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.Amount];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Amount", amountFilter.Description);
			AssertEquals("Category", ModelViewConstants.AmountFilter, amountFilter.Category);
		});
	}

	public void TestAmountFilter()
	{
		var header = Factory.New<CustomsSummaryHeader>();
		var summaryLineCollection = new CustomsSummaryLineCollection(Factory);
		var summaryLine1 = summaryLineCollection.AddNew();
		summaryLine1.B3_B2 = header.PK;
		var lineCharge1 = Factory.New<CustomsSummaryLineCharge>();
		lineCharge1.B4_B3 = summaryLine1.PK;
		summaryLine1.LineCharge.B4_ChargeAmount = 2;

		var summaryLine2 = summaryLineCollection.AddNew();
		summaryLine2.B3_B2 = header.PK;
		var lineCharge2 = Factory.New<CustomsSummaryLineCharge>();
		lineCharge2.B4_B3 = summaryLine1.PK;
		summaryLine2.LineCharge.B4_ChargeAmount = 5;
		Factory.Save();

		var amountFilter = (ModuleNumberRangeFilter)filter[CustomsSummaryFilterStripBusinessObject.Schema.Amount];
		amountFilter.IsActive = true;
		amountFilter.Property1 = 1;
		amountFilter.Property2 = 3;
		summaryLineCollection.AdditionalFilter = filter.Filter;

		AssertEquals(1, summaryLineCollection.Count);
	}

	public void TestCreateDateFilterProperty()
	{
		var createDateFilter = (ModuleDateFilter)filter[FilterDescriptions.CreatedTime];
		CombineAssertions(() =>
		{
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, createDateFilter.Visibility);
			AssertEquals("Property Search", ModuleDateFilter.DateRangeSearchTexts.Today, createDateFilter.PropertySearch);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		filter = new CustomsSummaryFilterStripBusinessObject();
		filter.QueryObjectType = typeof(CustomsSummaryLine);
	}
	CustomsSummaryFilterStripBusinessObject filter;
}
