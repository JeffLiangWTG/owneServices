using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(AcceptabilityBandFilterBusinessObject))]
	class AcceptabilityBandFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Bool Filters

		public void TestFiltersByReleaseGroup()
		{
			var band1 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			var band2 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band1.BAB_FiltersByReleaseGroup = false;
			band2.BAB_FiltersByReleaseGroup = true;

			Factory.Save();

			var bizo = new AcceptabilityBandFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo["Filters by Release Group"];
			filter.IsActive = true;

			filter.Property = ReleaseGroupFilteringOptions.Codes.Enabled;
			var results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { band2 }, results);

			filter.Property = ReleaseGroupFilteringOptions.Codes.Disabled;
			results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { band1 }, results);

			filter.Property = ReleaseGroupFilteringOptions.Codes.All;
			results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { band1, band2 }, results);
		}

		public void TestFiltersBySection()
		{
			var band1 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			var band2 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band1.BAB_FiltersBySection = false;
			band2.BAB_FiltersBySection = true;

			Factory.Save();

			var bizo = new AcceptabilityBandFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo["Filters by Board Section"];
			filter.IsActive = true;

			filter.Property = SectionFilteringOptions.Codes.Enabled;
			var results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { band2 }, results);

			filter.Property = SectionFilteringOptions.Codes.Disabled;
			results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { band1 }, results);

			filter.Property = SectionFilteringOptions.Codes.All;
			results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { band1, band2 }, results);
		}

		#endregion

		#region Number Filters

		public void TestMininumValue()
		{
			var acceptabilityBand1 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			var acceptabilityBand2 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			acceptabilityBand1.BAB_CautionLowerBound = 5;
			acceptabilityBand1.BAB_CautionUpperBound = 10;
			acceptabilityBand2.BAB_CautionLowerBound = 1;
			acceptabilityBand2.BAB_CautionUpperBound = 5;

			Factory.Save();

			var bizo = new AcceptabilityBandFilterBusinessObject();
			var filter = (ModuleNumberRangeFilter)bizo["Minimum Value"];
			filter.IsActive = true;
			filter.Property1 = 4;
			filter.Property2 = 6;

			var results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(acceptabilityBand1, results);
		}

		public void TestMaximumValue()
		{
			var acceptabilityBand1 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			var acceptabilityBand2 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			acceptabilityBand1.BAB_CautionLowerBound = 5;
			acceptabilityBand1.BAB_CautionUpperBound = 10;
			acceptabilityBand2.BAB_CautionLowerBound = 1;
			acceptabilityBand2.BAB_CautionUpperBound = 5;

			Factory.Save();

			var bizo = new AcceptabilityBandFilterBusinessObject();
			var filter = (ModuleNumberRangeFilter)bizo["Maximum Value"];
			filter.IsActive = true;
			filter.Property1 = 4;
			filter.Property2 = 6;

			var results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(acceptabilityBand2, results);
		}

		#endregion

		#region Text Filters

		public void TestName()
		{
			var acceptabilityBand1 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			var acceptabilityBand2 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			acceptabilityBand1.BAB_Name = "The Chamber of Secrets";
			acceptabilityBand2.BAB_Name = "Here's Johnny!";

			Factory.Save();

			var bizo = new AcceptabilityBandFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo["Name"];
			filter.IsActive = true;
			filter.Property = "The Chamber of Secrets";

			var results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(acceptabilityBand1, results);
		}

		public void TestSQL()
		{
			var acceptabilityBand1 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			var acceptabilityBand2 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			acceptabilityBand1.BAB_SqlText = "SELECT GS_Code \"Value\", GS_PK \"Component\", GS_PK \"ReleaseGroup\" from dbo.GlbStaff";
			acceptabilityBand2.BAB_SqlText = "SELECT FH_CompletionStatement \"Value\", FH_PK \"Component\", FH_PK \"ReleaseGroup\" from dbo.ProcessHeader";

			Factory.Save();

			var bizo = new AcceptabilityBandFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo["SQL Text"];
			filter.IsActive = true;
			filter.Property = "SELECT GS_Code \"Value\", GS_PK \"Component\", GS_PK \"ReleaseGroup\" from dbo.GlbStaff";

			var results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(acceptabilityBand1, results);
		}

		#endregion

		#region Guid Filters

		public void TestComponentFilter()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var bucket = BMSTestHelper.CreateBucket(system);

			var acceptabilityBand1 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			var acceptabilityBand2 = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			acceptabilityBand1.BAB_FC_Component = buffer.PK;
			acceptabilityBand2.BAB_FC_Component = bucket.PK;

			Factory.Save();

			var bizo = new AcceptabilityBandFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo["Component"];
			filter.IsActive = true;
			filter.Property = buffer.PK;

			var results = Factory.Load<BMComponentAcceptabilityBand>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(acceptabilityBand1, results);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AcceptabilityBandFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion

		#region Index Search Filters

		public void TestIndexSearchFilters()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AcceptabilityBand))
			using (new GlowIndexQueryEngineMock(mock =>
			{
				_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IBMComponentAcceptabilityBand" });
			}))
			{
				var filtersByBoardSectionFilter = module.FilterBusinessObject["Filters by Board Section"] as IndexSearchModuleTextFilter;
				AssertNotNull(filtersByBoardSectionFilter);
				AssertEquals(FilterCategories.StatusAndFlags, filtersByBoardSectionFilter.Category);
				AssertEquals("Filters by Board Section", filtersByBoardSectionFilter.MultilingualDescription.ToString());

				var filtersByReleaseGroupFilter = module.FilterBusinessObject["Filters by Release Group"] as IndexSearchModuleTextFilter;
				AssertNotNull(filtersByReleaseGroupFilter);
				AssertEquals(FilterCategories.StatusAndFlags, filtersByReleaseGroupFilter.Category);
				AssertEquals("Filters by Release Group", filtersByReleaseGroupFilter.MultilingualDescription.ToString());

				var typeFilter = module.FilterBusinessObject["Type"] as IndexSearchModuleTextFilter;
				AssertNotNull(typeFilter);
				AssertEquals(FilterCategories.TextSearch, typeFilter.Category);
				AssertEquals("Type", typeFilter.MultilingualDescription.ToString());
			}
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = new SearchField("CWDefaultHiddenFiltersByBoardSection", "CWDefaultHiddenFiltersByBoardSection", typeof(bool), uiHidden: true);
			var field2 = new SearchField("CWDefaultHiddenFiltersByReleaseGroup", "CWDefaultHiddenFiltersByReleaseGroup", typeof(bool), uiHidden: true);
			var field3 = new SearchField("CWDefaultHiddenType", "CWDefaultHiddenType", typeof(string), uiHidden: true);
			var ret = new SearchFieldCollection("IBMComponentAcceptabilityBand", [field1, field2, field3]);
			return ret;
		}

		#endregion
	}
}
