using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BoardSectionAcceptabilityBand))]
	class BoardSectionAcceptabilityBandTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var band1 = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 0, 0, 0, 0, 0, 0);
			var band2 = BMSTestHelper.CreateAcceptabilityBand_NullResult(bucket, 0, 0, 0, 0, 0, 0, "No Results...");

			var sectionBand1 = section.SectionConfiguration.AcceptabilityBands.AddNew();
			var sectionBand2 = section.SectionConfiguration.AcceptabilityBands.AddNew();

			sectionBand1.AcceptabilityBandPK = band1.PK;
			sectionBand2.AcceptabilityBandPK = band2.PK;

			AssertEquals("Number of Workflows", sectionBand1.DisplayName);
			AssertEquals((ZShort)1, sectionBand1.DisplaySequence);

			AssertEquals("No Results...", sectionBand2.DisplayName);
			AssertEquals((ZShort)2, sectionBand2.DisplaySequence);

			sectionBand2.DisplayName = "Booo";

			Factory.Save();

			var loadedSection = Factory.CreateNewFactory().Load<BMBoardSection>(section.PK);
			AssertEquals(2, loadedSection.SectionConfiguration.AcceptabilityBands.Count);

			sectionBand1 = loadedSection.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Single(b => b.AcceptabilityBandPK == band1.PK);
			sectionBand2 = loadedSection.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Single(b => b.AcceptabilityBandPK == band2.PK);

			AssertEquals("Number of Workflows", sectionBand1.DisplayName);
			AssertEquals((ZShort)1, sectionBand1.DisplaySequence);

			AssertEquals("Booo", sectionBand2.DisplayName);
			AssertEquals((ZShort)2, sectionBand2.DisplaySequence);

			sectionBand2.AcceptabilityBandPK = ZGuid.Invalid;
			AssertEquals("Booo", sectionBand2.DisplayName);

			sectionBand2.AcceptabilityBandPK = band2.PK;
			AssertEquals("No Results...", sectionBand2.DisplayName);
		}

		public void TestAvailableAcceptabilityBands_ShouldIncludeAll()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var band1 = BMSTestHelper.CreateAcceptabilityBand(bucket1, 0, 0, 0, 0, 0, 0, "I'm in the section's component");
			var band2 = BMSTestHelper.CreateAcceptabilityBand(bucket2, 0, 0, 0, 0, 0, 0, "I'm in some other component");
			var band3 = BMSTestHelper.CreateAcceptabilityBand(bucket2, 0, 0, 0, 0, 0, 0, "I don't have any component. Pity me.");
			band3.BAB_FC_Component = ZGuid.Empty;

			var section = BMSTestHelper.CreateBoardSection(bucket1);

			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			AssertContainsExactElementsInAnyOrder("All acceptability bands should be available regardless of component, and yet...", new[] { band1, band2, band3 }, sectionBand.Lookups.AvailableAcceptabilityBands);
		}

		public void TestDefaultValues()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system, "bucket1");
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 1, 2, 3, 4, 5, 6, "Lara");
			band.BAB_FiltersByReleaseGroup = true;
			band.BAB_FiltersBySection = true;

			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.AcceptabilityBandPK = band.PK;

			AssertEquals("Default", sectionBand.FiltersByReleaseGroupOverride);
			AssertEquals("Default", sectionBand.FiltersBySectionOverride);
			AssertEquals(1, sectionBand.CautionMinOverride);
			AssertEquals(2, sectionBand.GoodMinOverride);
			AssertEquals(3, sectionBand.ExcellentMinOverride);
			AssertEquals(4, sectionBand.ExcellentMaxOverride);
			AssertEquals(5, sectionBand.GoodMaxOverride);
			AssertEquals(6, sectionBand.CautionMaxOverride);
			AssertEquals("Tile", sectionBand.ShowOn);

			band = BMSTestHelper.CreateAcceptabilityBand(bucket, 7, 8, 9, 10, 11, 12, "Robyn");
			band.BAB_FiltersByReleaseGroup = false;
			band.BAB_FiltersBySection = false;

			sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.AcceptabilityBandPK = band.PK;

			AssertEquals("Default", sectionBand.FiltersByReleaseGroupOverride);
			AssertEquals("Default", sectionBand.FiltersBySectionOverride);
			AssertEquals(7, sectionBand.CautionMinOverride);
			AssertEquals(8, sectionBand.GoodMinOverride);
			AssertEquals(9, sectionBand.ExcellentMinOverride);
			AssertEquals(10, sectionBand.ExcellentMaxOverride);
			AssertEquals(11, sectionBand.GoodMaxOverride);
			AssertEquals(12, sectionBand.CautionMaxOverride);
		}

		public void TestChangeAcceptabilityBandPk_ShouldUpdateOverrideValues()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system, "bucket1");
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 1, 2, 3, 4, 5, 6, "Lara");
			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();

			AssertEquals(0, sectionBand.CautionMinOverride);
			AssertEquals(0, sectionBand.GoodMinOverride);
			AssertEquals(0, sectionBand.ExcellentMinOverride);
			AssertEquals(0, sectionBand.ExcellentMaxOverride);
			AssertEquals(0, sectionBand.GoodMaxOverride);
			AssertEquals(0, sectionBand.CautionMaxOverride);

			sectionBand.AcceptabilityBandPK = band.PK;

			AssertEquals(1, sectionBand.CautionMinOverride);
			AssertEquals(2, sectionBand.GoodMinOverride);
			AssertEquals(3, sectionBand.ExcellentMinOverride);
			AssertEquals(4, sectionBand.ExcellentMaxOverride);
			AssertEquals(5, sectionBand.GoodMaxOverride);
			AssertEquals(6, sectionBand.CautionMaxOverride);
		}

		public void TestDisableBoundaryOverrides_ShouldUpdateOverrideValues()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system, "bucket1");
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 1, 2, 3, 4, 5, 6, "Lara");
			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.AcceptabilityBandPK = band.PK;
			sectionBand.AreBoundaryValuesOverridden = true;

			AssertEquals(1, sectionBand.CautionMinOverride);
			AssertEquals(2, sectionBand.GoodMinOverride);
			AssertEquals(3, sectionBand.ExcellentMinOverride);
			AssertEquals(4, sectionBand.ExcellentMaxOverride);
			AssertEquals(5, sectionBand.GoodMaxOverride);
			AssertEquals(6, sectionBand.CautionMaxOverride);

			sectionBand.CautionMinOverride = 7;
			sectionBand.GoodMinOverride = 8;
			sectionBand.ExcellentMinOverride = 9;
			sectionBand.ExcellentMaxOverride = 10;
			sectionBand.GoodMaxOverride = 11;
			sectionBand.CautionMaxOverride = 12;

			sectionBand.AreBoundaryValuesOverridden = false;

			AssertEquals(1, sectionBand.CautionMinOverride);
			AssertEquals(2, sectionBand.GoodMinOverride);
			AssertEquals(3, sectionBand.ExcellentMinOverride);
			AssertEquals(4, sectionBand.ExcellentMaxOverride);
			AssertEquals(5, sectionBand.GoodMaxOverride);
			AssertEquals(6, sectionBand.CautionMaxOverride);
		}

		public void TestBoundaryValues_WhenOverridden_ForBinding()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 1, 2, 3, 4, 5, 6, "Values Must Change!");
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);

			AssertEquals(false, sectionBand.AreBoundaryValuesOverridden);
			AssertEquals(1, sectionBand.CautionMinEffectiveValue);
			AssertEquals(2, sectionBand.GoodMinEffectiveValue);
			AssertEquals(3, sectionBand.ExcellentMinEffectiveValue);
			AssertEquals(4, sectionBand.ExcellentMaxEffectiveValue);
			AssertEquals(5, sectionBand.GoodMaxEffectiveValue);
			AssertEquals(6, sectionBand.CautionMaxEffectiveValue);

			sectionBand.AreBoundaryValuesOverridden = true;
			AssertEquals(1, sectionBand.CautionMinEffectiveValue);
			AssertEquals(2, sectionBand.GoodMinEffectiveValue);
			AssertEquals(3, sectionBand.ExcellentMinEffectiveValue);
			AssertEquals(4, sectionBand.ExcellentMaxEffectiveValue);
			AssertEquals(5, sectionBand.GoodMaxEffectiveValue);
			AssertEquals(6, sectionBand.CautionMaxEffectiveValue);

			sectionBand.CautionMinEffectiveValue = 7;
			sectionBand.GoodMinEffectiveValue = 8;
			sectionBand.ExcellentMinEffectiveValue = 9;
			sectionBand.ExcellentMaxEffectiveValue = 10;
			sectionBand.GoodMaxEffectiveValue = 11;
			sectionBand.CautionMaxEffectiveValue = 12;

			AssertEquals(7, sectionBand.CautionMinEffectiveValue);
			AssertEquals(8, sectionBand.GoodMinEffectiveValue);
			AssertEquals(9, sectionBand.ExcellentMinEffectiveValue);
			AssertEquals(10, sectionBand.ExcellentMaxEffectiveValue);
			AssertEquals(11, sectionBand.GoodMaxEffectiveValue);
			AssertEquals(12, sectionBand.CautionMaxEffectiveValue);

			sectionBand.AreBoundaryValuesOverridden = false;
			AssertEquals(1, sectionBand.CautionMinEffectiveValue);
			AssertEquals(2, sectionBand.GoodMinEffectiveValue);
			AssertEquals(3, sectionBand.ExcellentMinEffectiveValue);
			AssertEquals(4, sectionBand.ExcellentMaxEffectiveValue);
			AssertEquals(5, sectionBand.GoodMaxEffectiveValue);
			AssertEquals(6, sectionBand.CautionMaxEffectiveValue);
		}

		public void TestUpdateAcceptabilityBandBoundaryValues_AfterAddedToBoardSection_WhenNotOverriding_ShouldUpdateBindingValues()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 1, 2, 3, 4, 5, 6, "Values Must Change!");
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);

			AssertEquals(false, sectionBand.AreBoundaryValuesOverridden);
			AssertEquals(1, sectionBand.CautionMinEffectiveValue);
			AssertEquals(2, sectionBand.GoodMinEffectiveValue);
			AssertEquals(3, sectionBand.ExcellentMinEffectiveValue);
			AssertEquals(4, sectionBand.ExcellentMaxEffectiveValue);
			AssertEquals(5, sectionBand.GoodMaxEffectiveValue);
			AssertEquals(6, sectionBand.CautionMaxEffectiveValue);

			band.BAB_CautionLowerBound = 7;
			band.BAB_GoodLowerBound = 8;
			band.BAB_ExcellentLowerBound = 9;
			band.BAB_ExcellentUpperBound = 10;
			band.BAB_GoodUpperBound = 11;
			band.BAB_CautionUpperBound = 12;

			CombineAssertions("When not overriding values, the board section band values (shown in config grid) should show the same ones as the underlying band. SAD!", () =>
			{
				AssertEquals("Values should still not be overridden.", false, sectionBand.AreBoundaryValuesOverridden);
				AssertEquals("Caution Min", 7, sectionBand.CautionMinEffectiveValue);
				AssertEquals("Good Min", 8, sectionBand.GoodMinEffectiveValue);
				AssertEquals("Excellent Min", 9, sectionBand.ExcellentMinEffectiveValue);
				AssertEquals("Excellent Max", 10, sectionBand.ExcellentMaxEffectiveValue);
				AssertEquals("Good Max", 11, sectionBand.GoodMaxEffectiveValue);
				AssertEquals("Good Min", 12, sectionBand.CautionMaxEffectiveValue);
			});
		}

		public void TestNewBoardSectionAcceptabilityBand_WhenBoundaryPropertiesAccessed_ShouldNotThrowExceptions()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var sectionBand = new BoardSectionAcceptabilityBand(config.BucketSection);

			AssertEquals("Accessing these properties before the band is selected should not throw exceptions. SAD!", 0, sectionBand.CautionMinEffectiveValue);
			AssertEquals("Accessing these properties before the band is selected should not throw exceptions. SAD!", 0, sectionBand.GoodMinEffectiveValue);
			AssertEquals("Accessing these properties before the band is selected should not throw exceptions. SAD!", 0, sectionBand.ExcellentMinEffectiveValue);
			AssertEquals("Accessing these properties before the band is selected should not throw exceptions. SAD!", 0, sectionBand.ExcellentMaxEffectiveValue);
			AssertEquals("Accessing these properties before the band is selected should not throw exceptions. SAD!", 0, sectionBand.GoodMaxEffectiveValue);
			AssertEquals("Accessing these properties before the band is selected should not throw exceptions. SAD!", 0, sectionBand.CautionMaxEffectiveValue);

			object boundaryValues;
			AssertNoExceptionThrown(() => boundaryValues = sectionBand.BoundaryValues);
		}

		public void TestBoundaryValues()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system, "bucket1");
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 1, 2, 3, 4, 5, 6, "Lara");
			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.AcceptabilityBandPK = band.PK;
			sectionBand.AreBoundaryValuesOverridden = false;
			sectionBand.CautionMinOverride = 7;
			sectionBand.GoodMinOverride = 8;
			sectionBand.ExcellentMinOverride = 9;
			sectionBand.ExcellentMaxOverride = 10;
			sectionBand.GoodMaxOverride = 11;
			sectionBand.CautionMaxOverride = 12;

			var expected = new AcceptabilityBandBoundaryValues(1, 2, 3, 4, 5, 6);
			AssertEquals("The overridden values should be ignored because the override is disabled, and yet...", expected, sectionBand.BoundaryValues);

			sectionBand.AreBoundaryValuesOverridden = true;
			AssertEquals("The overridden values should be reset because we changed the overridden flag, and yet...", expected, sectionBand.BoundaryValues);

			sectionBand.CautionMinOverride = 7;
			sectionBand.GoodMinOverride = 8;
			sectionBand.ExcellentMinOverride = 9;
			sectionBand.ExcellentMaxOverride = 10;
			sectionBand.GoodMaxOverride = 11;
			sectionBand.CautionMaxOverride = 12;

			expected = new AcceptabilityBandBoundaryValues(7, 8, 9, 10, 11, 12);
			AssertEquals("The values from the board section should be used because the override is enabled, and yet...", expected, sectionBand.BoundaryValues);
		}

		public void TestShowOn()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system, "bucket1");
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 1, 2, 3, 4, 5, 6, "Lara");
			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();

			AssertEquals("Should be set to 'Tile' by default, and yet...", "Tile", sectionBand.ShowOn);
			AssertEquals("Should be show as a tile by default, and yet...", true, sectionBand.ShouldShowAsTile);
			AssertEquals("Should not include in the heading by default, and yet...", false, sectionBand.ShouldShowInHeading);

			sectionBand.ShowOn = "Heading";

			AssertEquals("Should not show as a tile because it is set to show in the heading, and yet...", false, sectionBand.ShouldShowAsTile);
			AssertEquals("Should be included in the heading, and yet...", true, sectionBand.ShouldShowInHeading);

			sectionBand.ShowOn = "Both";

			AssertEquals("Should show as a tile because it is set to 'Both', and yet...", true, sectionBand.ShouldShowAsTile);
			AssertEquals("Should be included in the heading because it is set to 'Both', and yet...", true, sectionBand.ShouldShowInHeading);

			sectionBand.ShowOn = string.Empty; // this could maybe happen for pre-existing section bands?

			AssertEquals("Should be show as a tile when the value is invalid, and yet...", true, sectionBand.ShouldShowAsTile);
			AssertEquals("Should be show as a tile when the value is invalid, and yet...", false, sectionBand.ShouldShowInHeading);
		}

		public void TestMaximumItems_WhenNoAdditionalAggregatorColumnIsUsed_ShouldBeReadOnly()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Mine band", "SELECT null Value, null Component, null ReleaseGroup, null AdditionalAggregator FROM ATable");
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);
			AssertEquals(false, sectionBand.MaximumItemsInfo.ReadOnly);

			band.BAB_SqlText = "SELECT null Value, null Component, null ReleaseGroup";
			AssertEquals(true, sectionBand.MaximumItemsInfo.ReadOnly);

			band.BAB_SqlText = ZString.Empty;
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			AssertEquals(true, sectionBand.MaximumItemsInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBucket(BMSTestHelper.CreateSystem(Factory)));
			return new BoardSectionAcceptabilityBand(section) { AreBoundaryValuesOverridden = true };
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "AcceptabilityBandPK";
				yield return "DisplayName";
				yield return "DisplaySequence";
				yield return "DisplayUnits";
				yield return "AreBoundaryValuesOverridden";
				yield return "CautionMaxOverride";
				yield return "CautionMinOverride";
				yield return "ExcellentMaxOverride";
				yield return "ExcellentMinOverride";
				yield return "FiltersByReleaseGroupOverride";
				yield return "FiltersBySectionOverride";
				yield return "GoodMaxOverride";
				yield return "GoodMinOverride";
				yield return "MaximumItems";
				yield return "ShowOn";
			}
		}
	}
}
