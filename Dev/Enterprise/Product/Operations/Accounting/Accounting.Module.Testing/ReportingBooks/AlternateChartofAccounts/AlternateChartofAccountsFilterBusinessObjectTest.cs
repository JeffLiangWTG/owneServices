using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AlternateChartofAccountsFilterBusinessObject))]
	class AlternateChartofAccountsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AlternateChartofAccountsFilterBusinessObject();
		}

		public void TestCodeFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Code"];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "TRR";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "MGT";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			Assert(!filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			Assert(!filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));

			AssertEquals(filter.FilterColumn.MaxLength, filter.MaxLength);
		}

		public void TestNameFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Name"];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "Turkey Local Reporting Chart";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "Management Reporting";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			Assert(!filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			Assert(!filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));

			AssertEquals(filter.FilterColumn.MaxLength, filter.MaxLength);
		}

		public void TestAccountLengthFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Account Length"];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "FIXED";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "VARIABLE";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "ALL";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			AssertEquals(8, filter.MaxLength);
		}

		public void TestGlobalStatusFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Global Status"];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "GLOBAL";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "NOT GLOBAL";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "ALL";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			AssertEquals(10, filter.MaxLength);
		}

		public void TestSetCurrentAlternateChartFormat()
		{
			var accAlternateCharts = Factory.Load<AccAlternateChart>(new ZQuery());
			AssertEquals("Expecting collection 2 record", 2, accAlternateCharts.Length);
			FilterBO.SetCurrentAlternateChartFormat(accAlternateCharts.Where(chart => chart.AAC_Code == "TRR").First());
			AssertEquals(1, FilterBO.AlternateChartFormats.Count);
			AssertEquals(TrrFormat.PK, FilterBO.AlternateChartFormats[0].PK);
		}

		public void TestBalanceSheetStyleFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Balance Sheet Style"];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "EAL";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "ELA";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "";
			TestCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			AssertEquals(filter.FilterColumn.MaxLength, filter.MaxLength);
		}

		#region Override

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			Trr = Creator.CreateAlternateChart("TRR", "Turkey Local Reporting Chart", false, true, BalanceSheetStyleCode.EAL);
			var mgt = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA, AccAlternateChartLookups.ReportOrderCode.BTP);

			TrrFormat = Creator.CreateAccAlternateChartFormat(Trr, 1, "9", "1", "-");
			Creator.CreateAccAlternateChartFormat(mgt, 1, "X", "2", ".");

			Factory.Save();
			FilterBO = (AlternateChartofAccountsFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		#endregion

		AccAlternateChartCollection TestCollection
		{
			get { return testCollection ?? (testCollection = new AccAlternateChartCollection(Factory)); }
		}

		AccAlternateChartCollection testCollection;

		AlternateChartofAccountsFilterBusinessObject FilterBO;

		AccAlternateChart Trr;

		AccAlternateChartFormat TrrFormat;

		TestObjectCreator Creator;
	}
}
