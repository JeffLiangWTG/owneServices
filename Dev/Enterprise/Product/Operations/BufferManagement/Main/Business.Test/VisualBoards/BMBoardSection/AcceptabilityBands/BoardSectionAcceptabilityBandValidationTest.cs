using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	class BoardSectionAcceptabilityBandValidationTest : BMSTestCaseWithFactory
	{
		public void TestDisplayName_ShouldNotBeMandatory()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 0, 0, 0, 0, 0, 0, "Bat Dand");

			var section = BMSTestHelper.CreateBoardSection(bucket);
			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();

			sectionBand.AcceptabilityBandPK = band.PK;
			AssertEquals("Bat Dand", sectionBand.DisplayName);

			sectionBand.DisplayName = ZString.Empty;
			AssertNoErrors(sectionBand.DisplayNameInfo);
		}

		public void TestDisplaySequence()
		{
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBucket(BMSTestHelper.CreateSystem(Factory)));
			var sectionBand1 = section.SectionConfiguration.AcceptabilityBands.AddNew();
			var sectionBand2 = section.SectionConfiguration.AcceptabilityBands.AddNew();

			sectionBand2.DisplaySequence = 1;
			AssertHasError(sectionBand2.DisplaySequenceInfo, "The Sequence has been duplicated and must be unique.");

			sectionBand1.DisplaySequence = 2;
			sectionBand2.Validation.ValidateAll();

			AssertNoErrors(sectionBand2.DisplaySequenceInfo);
		}

		public void TestAcceptabilityBandPK()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var band1 = BMSTestHelper.CreateAcceptabilityBand(bucket, 0, 0, 0, 0, 0, 0, "Bat Dand");
			var band2 = BMSTestHelper.CreateAcceptabilityBand(bucket, 0, 0, 0, 0, 0, 0, "Bis Dand");

			var section = BMSTestHelper.CreateBoardSection(bucket);
			var sectionBand1 = section.SectionConfiguration.AcceptabilityBands.AddNew();

			sectionBand1.Validation.ValidateAll();
			AssertHasError(sectionBand1.AcceptabilityBandPKInfo, "Please enter an Acceptability Band.");

			sectionBand1.AcceptabilityBandPK = ZGuid.NewZGuid();
			AssertHasError(sectionBand1.AcceptabilityBandPKInfo, "Enter a valid Acceptability Band.");

			sectionBand1.AcceptabilityBandPK = band1.PK;
			AssertNoErrors(sectionBand1.AcceptabilityBandPKInfo);

			var sectionBand2 = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand2.AcceptabilityBandPK = band1.PK;

			sectionBand1.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.YesOptionCode;
			sectionBand1.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand2.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand2.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			AssertNoErrors(sectionBand2.AcceptabilityBandPKInfo);

			sectionBand1.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand1.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.YesOptionCode;
			sectionBand2.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand2.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			AssertNoErrors(sectionBand2.AcceptabilityBandPKInfo);

			sectionBand1.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand1.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand2.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.NoOptionCode;
			sectionBand2.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			AssertNoErrors(sectionBand2.AcceptabilityBandPKInfo);

			sectionBand1.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand1.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand2.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand2.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.NoOptionCode;
			AssertNoErrors(sectionBand2.AcceptabilityBandPKInfo);

			sectionBand1.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand1.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand2.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			sectionBand2.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			AssertHasError(sectionBand2.AcceptabilityBandPKInfo, "The acceptability band has been duplicated. Same acceptability bands can be put on the same board if they have different values for the Filter By Release Group or Filter By Board Section fields only.");

			sectionBand2.AcceptabilityBandPK = band2.PK;
			AssertNoErrors(sectionBand2.AcceptabilityBandPKInfo);

			sectionBand2.AcceptabilityBandPK = band1.PK;
			AssertHasError(sectionBand2.AcceptabilityBandPKInfo, "The acceptability band has been duplicated. Same acceptability bands can be put on the same board if they have different values for the Filter By Release Group or Filter By Board Section fields only.");

			sectionBand1.Delete();
			sectionBand2.Validation.ValidateAll();
			AssertNoErrors(sectionBand2.AcceptabilityBandPKInfo);
		}

		public void TestBoundaryValues()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 0, 0, 0, 0, 0, 0, "Shazaam");
			var sectionBand = new BoardSectionAcceptabilityBand(section);
			sectionBand.AcceptabilityBandPK = band.PK;

			band.Validation.ValidateAll();

			AssertNoErrors(sectionBand.CautionMinOverrideInfo);
			AssertNoErrors(sectionBand.GoodMinOverrideInfo);
			AssertNoErrors(sectionBand.ExcellentMinOverrideInfo);
			AssertNoErrors(sectionBand.ExcellentMaxOverrideInfo);
			AssertNoErrors(sectionBand.GoodMaxOverrideInfo);
			AssertNoErrors(sectionBand.CautionMaxOverrideInfo);

			sectionBand.AreBoundaryValuesOverridden = true;
			sectionBand.CautionMinOverride = 10;
			sectionBand.GoodMinOverride = 9;
			sectionBand.ExcellentMinOverride = 8;
			sectionBand.ExcellentMaxOverride = 7;
			sectionBand.GoodMaxOverride = 6;
			sectionBand.CautionMaxOverride = 5;

			AssertHasError(sectionBand.CautionMinOverrideInfo, "The Minimum Caution Value must be less than or equal to the Minimum Good Value.");
			AssertHasError(sectionBand.GoodMinOverrideInfo, "The Minimum Good Value must be less than or equal to the Minimum Excellent Value.");
			AssertHasError(sectionBand.ExcellentMinOverrideInfo, "The Minimum Excellent Value must be less than or equal to the Maximum Excellent Value.");
			AssertHasError(sectionBand.ExcellentMaxOverrideInfo, "The Maximum Excellent Value must be greater than or equal to the Minimum Excellent Value.");
			AssertHasError(sectionBand.GoodMaxOverrideInfo, "The Maximum Good Value must be greater than or equal to the Maximum Excellent Value.");
			AssertHasError(sectionBand.CautionMaxOverrideInfo, "The Maximum Caution Value must be greater than or equal to the Maximum Good Value.");

			sectionBand.CautionMinOverride = -5;
			sectionBand.GoodMinOverride = -4;
			sectionBand.ExcellentMinOverride = -3;
			sectionBand.ExcellentMaxOverride = -2;
			sectionBand.GoodMaxOverride = -1;
			sectionBand.CautionMaxOverride = 0;

			AssertNoErrors(sectionBand.CautionMinOverrideInfo);
			AssertNoErrors(sectionBand.GoodMinOverrideInfo);
			AssertNoErrors(sectionBand.ExcellentMinOverrideInfo);
			AssertNoErrors(sectionBand.ExcellentMaxOverrideInfo);
			AssertNoErrors(sectionBand.GoodMaxOverrideInfo);
			AssertNoErrors(sectionBand.CautionMaxOverrideInfo);
		}

		public void TestBoundaryEffectiveValues_ShouldHaveMatchingValidationErrorsAsUnderlyingProperties()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 0, 0, 0, 0, 0, 0, "Shazaam");
			var sectionBand = new BoardSectionAcceptabilityBand(section);
			sectionBand.AcceptabilityBandPK = band.PK;

			band.Validation.ValidateAll();

			AssertNoErrors(sectionBand.CautionMinEffectiveValueInfo);
			AssertNoErrors(sectionBand.GoodMinEffectiveValueInfo);
			AssertNoErrors(sectionBand.ExcellentMinEffectiveValueInfo);
			AssertNoErrors(sectionBand.ExcellentMaxEffectiveValueInfo);
			AssertNoErrors(sectionBand.GoodMaxEffectiveValueInfo);
			AssertNoErrors(sectionBand.CautionMaxEffectiveValueInfo);

			sectionBand.AreBoundaryValuesOverridden = true;
			sectionBand.CautionMinOverride = 10;
			sectionBand.GoodMinOverride = 9;
			sectionBand.ExcellentMinOverride = 8;
			sectionBand.ExcellentMaxOverride = 7;
			sectionBand.GoodMaxOverride = 6;
			sectionBand.CautionMaxOverride = 5;

			AssertHasError(sectionBand.CautionMinEffectiveValueInfo, "The Minimum Caution Value must be less than or equal to the Minimum Good Value.");
			AssertHasError(sectionBand.GoodMinEffectiveValueInfo, "The Minimum Good Value must be less than or equal to the Minimum Excellent Value.");
			AssertHasError(sectionBand.ExcellentMinEffectiveValueInfo, "The Minimum Excellent Value must be less than or equal to the Maximum Excellent Value.");
			AssertHasError(sectionBand.ExcellentMaxEffectiveValueInfo, "The Maximum Excellent Value must be greater than or equal to the Minimum Excellent Value.");
			AssertHasError(sectionBand.GoodMaxEffectiveValueInfo, "The Maximum Good Value must be greater than or equal to the Maximum Excellent Value.");
			AssertHasError(sectionBand.CautionMaxEffectiveValueInfo, "The Maximum Caution Value must be greater than or equal to the Maximum Good Value.");
		}

		public void TestShowOn()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 0, 0, 0, 0, 0, 0, "Shazaam");
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			AssertEquals(AcceptabilityBandShowOnOptions.Codes.Tile, sectionBand.ShowOn);
			AssertNoErrors(sectionBand.ShowOnInfo);

			sectionBand.ShowOn = string.Empty;
			AssertHasError(sectionBand.ShowOnInfo, "Please enter an option for how the acceptability band should be displayed.");

			sectionBand.ShowOn = "Gloopy gloppy";
			AssertHasError(sectionBand.ShowOnInfo, "Please select a valid option for how the acceptability band should be displayed.");
		}

		public void TestZeroCellSection_FiltersBySection_ShouldHaveError()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.CellsPerSubsection = 1;

			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 0, 0, 0, 0, 0, 0, "Shazaam");
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			sectionBand.Validation.ValidateAll();
			AssertNoErrors(sectionBand.FiltersBySectionOverrideInfo);

			sectionBand.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.NoOptionCode;
			AssertNoErrors(sectionBand.FiltersBySectionOverrideInfo);

			sectionBand.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.YesOptionCode;
			AssertNoErrors(sectionBand.FiltersBySectionOverrideInfo);

			section.SectionConfiguration.CellsPerSubsection = 0;
			AssertHasError(sectionBand.FiltersBySectionOverrideInfo, "Acceptability bands cannot filter by board section when the section has 0 cells per subsection.");

			sectionBand.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.NoOptionCode;
			AssertNoErrors(sectionBand.FiltersBySectionOverrideInfo);

			sectionBand.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.YesOptionCode;
			AssertHasError(sectionBand.FiltersBySectionOverrideInfo, "Acceptability bands cannot filter by board section when the section has 0 cells per subsection.");
		}
	}
}
