using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlReportFilterBusinessObject))]
	sealed class ExitControlReportFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestExitArrivalDateFilterProperties()
		{
			var discrepanciesFilter = (ModuleDateTimeOffsetFilter)filter[ExitControlReportFilterBusinessObject.IEFilterConstants.ExitArrivalDate];
			discrepanciesFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Exit/Arrival Date", discrepanciesFilter.Description);
				AssertEquals("Category", FilterCategories.Dates, discrepanciesFilter.Category);
			});
		}

		public void TestExitArrivalDateFilter()
		{
			var report1 = Factory.New<CusExitReport>();
			report1.CER_DateTime = new ZDateTimeOffset(2020, 01, 15);
			var report2 = Factory.New<CusExitReport>();
			report2.CER_DateTime = new ZDateTimeOffset(2020, 02, 01);
			var report3 = Factory.New<CusExitReport>();
			report3.CER_DateTime = new ZDateTimeOffset(2020, 02, 25);
			var report4 = Factory.New<CusExitReport>();

			var exitArrivalDateFilter = (ModuleDateTimeOffsetFilter)filter[ExitControlReportFilterBusinessObject.IEFilterConstants.ExitArrivalDate];
			exitArrivalDateFilter.IsActive = true;

			CombineAssertions(() =>
			{
				exitArrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				exitArrivalDateFilter.Property1 = new ZDate(2020, 02, 01);
				exitArrivalDateFilter.Property2 = new ZDate(2020, 02, 28);
				AssertMatch("Between 01/02/2020 - 28/02/2020", false, true, true, false);

				exitArrivalDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
				AssertMatch("Has Date", true, true, true, false);

				exitArrivalDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
				AssertMatch("No Date", false, false, false, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3, bool match4)
			{
				AssertEquals(message + "->declaration1", match1, report1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration2", match2, report2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration3", match3, report3.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration4", match4, report4.MatchesFilter(filter.Filter));
			}
		}

		public void TestDiscrepanciesFilterProperties()
		{
			var discrepanciesFilter = (ModuleFlagsFilter)filter[ExitControlReportFilterBusinessObject.IEFilterConstants.Discrepancies];

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Discrepancies", discrepanciesFilter.Description);
				AssertEquals("Category", FilterCategories.StatusAndFlags, discrepanciesFilter.Category);
			});
		}

		public void TestDiscrepanciesFilter()
		{
			var report1 = Factory.New<CusExitReport>();
			report1.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			var report2 = Factory.New<CusExitReport>();
			report2.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;

			var discrepanciesFilter = (ModuleFlagsFilter)filter[ExitControlReportFilterBusinessObject.IEFilterConstants.Discrepancies];
			discrepanciesFilter.IsActive = true;
			discrepanciesFilter.Property0 = true;

			AssertEquals(true, report1.MatchesFilter(filter.Filter));
			AssertEquals(false, report2.MatchesFilter(filter.Filter));
		}

		public void TestStatusFilter()
		{
			var report1 = Factory.New<CusExitReport>();
			report1.CER_Status = AESEntryStatusList.Codes.MrnAllocated;
			var report2 = Factory.New<CusExitReport>();
			report2.CER_Status = AESEntryStatusList.Codes.PendingControl;

			var statusFilter = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.Status];
			statusFilter.IsActive = true;
			statusFilter.Property = AESEntryStatusList.Codes.MrnAllocated;

			AssertEquals(true, report1.MatchesFilter(filter.Filter));
			AssertEquals(false, report2.MatchesFilter(filter.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ExitControlReportFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filter = new ExitControlReportFilterBusinessObject();
		}
		ExitControlReportFilterBusinessObject filter;
	}
}
