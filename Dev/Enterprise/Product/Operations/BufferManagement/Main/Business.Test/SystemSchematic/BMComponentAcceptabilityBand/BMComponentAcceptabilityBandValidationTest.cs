using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMComponentAcceptabilityBandValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBoundaryValues()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			band.Validation.ValidateAll();

			AssertNoErrors(band.BAB_CautionLowerBoundInfo);
			AssertNoErrors(band.BAB_GoodLowerBoundInfo);
			AssertNoErrors(band.BAB_ExcellentLowerBoundInfo);
			AssertNoErrors(band.BAB_ExcellentUpperBoundInfo);
			AssertNoErrors(band.BAB_GoodUpperBoundInfo);
			AssertNoErrors(band.BAB_CautionUpperBoundInfo);

			band.BAB_CautionLowerBound = 10;
			band.BAB_GoodLowerBound = 9;
			band.BAB_ExcellentLowerBound = 8;
			band.BAB_ExcellentUpperBound = 7;
			band.BAB_GoodUpperBound = 6;
			band.BAB_CautionUpperBound = 5;

			AssertHasError(band.BAB_CautionLowerBoundInfo, "The Minimum Caution Value must be less than or equal to the Minimum Good Value.");
			AssertHasError(band.BAB_GoodLowerBoundInfo, "The Minimum Good Value must be less than or equal to the Minimum Excellent Value.");
			AssertHasError(band.BAB_ExcellentLowerBoundInfo, "The Minimum Excellent Value must be less than or equal to the Maximum Excellent Value.");
			AssertHasError(band.BAB_ExcellentUpperBoundInfo, "The Maximum Excellent Value must be greater than or equal to the Minimum Excellent Value.");
			AssertHasError(band.BAB_GoodUpperBoundInfo, "The Maximum Good Value must be greater than or equal to the Maximum Excellent Value.");
			AssertHasError(band.BAB_CautionUpperBoundInfo, "The Maximum Caution Value must be greater than or equal to the Maximum Good Value.");

			band.BAB_CautionLowerBound = -5;
			band.BAB_GoodLowerBound = -4;
			band.BAB_ExcellentLowerBound = -3;
			band.BAB_ExcellentUpperBound = -2;
			band.BAB_GoodUpperBound = -1;
			band.BAB_CautionUpperBound = 0;

			AssertNoErrors(band.BAB_CautionLowerBoundInfo);
			AssertNoErrors(band.BAB_GoodLowerBoundInfo);
			AssertNoErrors(band.BAB_ExcellentLowerBoundInfo);
			AssertNoErrors(band.BAB_ExcellentUpperBoundInfo);
			AssertNoErrors(band.BAB_GoodUpperBoundInfo);
			AssertNoErrors(band.BAB_CautionUpperBoundInfo);
		}

		public void TestAggregationTypeValidation()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();

			band.BAB_Type = AcceptabilityBandTypes.Codes.Aggregate;

			band.BAB_SqlText = "SELECT YOUR DAD";
			AssertHasError(band.BAB_SqlTextInfo, "Invalid SQL Statement\r\nInvalid column name 'YOUR'.");

			band.BAB_SqlText = "SELECT GS_Code \"Value\", GS_PK \"Component\", GS_PK \"ReleaseGroup\" from dbo.GlbStaff";
			AssertHasError(band.BAB_SqlTextInfo, "Must select data from the 'Workflows' result set.");

			band.BAB_SqlText = "SELECT Count(*) Value, FH_FC_CurrentComponent Component, FH_GG_ReleaseGroup ReleaseGroup from dbo.ProcessHeader Group By FH_FC_CurrentComponent, FH_GG_ReleaseGroup";
			AssertHasError(band.BAB_SqlTextInfo, "Must select data from the 'Workflows' result set.");

			band.BAB_SqlText = "SELECT Count(*) Value, FH_FC_CurrentComponent Component, FH_GG_ReleaseGroup ReleaseGroup from Workflows Group By FH_FC_CurrentComponent, FH_GG_ReleaseGroup";
			AssertNoErrors(band.BAB_SqlTextInfo);
		}

		public void TestSqlValidation()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();

			band.BAB_SqlText = "SELECT TOP 1 PER CENT FROM (SELECT TOP 1 PER CENT FROM (SELECT TOP 1 PER CENT))";
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			AssertNoErrors(band.BAB_SqlTextInfo);

			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;
			band.Validation.ValidateAll();
			AssertHasError(band.BAB_SqlTextInfo, "Invalid SQL Statement\r\nIncorrect syntax near ')'.");

			band.BAB_SqlText = "SELECT GS_Code from dbo.GlbStaff";
			AssertHasError(band.BAB_SqlTextInfo, @"The SQL statement must return 3 columns: Value, Component, ReleaseGroup
The following columns are optional: AdditionalAggregator");

			band.BAB_SqlText = "SELECT GS_Code \"Value\", GS_PK \"Component\", GS_PK \"ReleaseGroup\" from dbo.GlbStaff";
			AssertNoErrors(band.BAB_SqlTextInfo);

			band.BAB_SqlText = "SELECT GS_Code Value, GS_PK Component, GS_Code AdditionalAggregator, GS_PK ReleaseGroup FROM dbo.GlbStaff";
			AssertHasError(band.BAB_SqlTextInfo, "The column at position 3 must have the name ReleaseGroup.");

			band.BAB_SqlText = "SELECT GS_Code Value, GS_PK Component, GS_PK ReleaseGroup, GS_Code AdditionalAggregator FROM GlbStaff";
			AssertNoErrors(band.BAB_SqlTextInfo);
		}

		#region Security

		public void TestSqlValidation_ModifyQuerySecurityCheckpointDisabled_ShouldNotAllowEditingQuery()
		{
			Env.Security.AcceptabilityBandModifyQuery.IsAllowed = false;
			var band = Factory.New<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;
			band.Validation.ValidateBAB_SqlText();
			AssertNoError(band.BAB_SqlTextInfo, "You do not have permission to modify Acceptability Band SQL."); // there will be a MandatoryValidation error, but that's a separate concern.

			band.BAB_SqlText = "SELECT Count(*) Value, FH_FC_CurrentComponent Component, FH_GG_ReleaseGroup ReleaseGroup from dbo.ProcessHeader Group By FH_FC_CurrentComponent, FH_GG_ReleaseGroup";
			AssertHasError(band.BAB_SqlTextInfo, "You do not have permission to modify Acceptability Band SQL.");

			Env.Security.AcceptabilityBandModifyQuery.IsAllowed = true;
			band.Validation.ValidateBAB_SqlText();
			AssertNoErrors(band.BAB_SqlTextInfo);

			Factory.Save();
			Env.Security.AcceptabilityBandModifyQuery.IsAllowed = false;
			var loadedBand = new BusinessObjectFactory().Load<BMComponentAcceptabilityBand>(band.PK);
			AssertNoErrors(loadedBand.BAB_SqlTextInfo);

			loadedBand.BAB_SqlText = "SELECT Count(*) AS Value, FH_FC_CurrentComponent Component, FH_GG_ReleaseGroup ReleaseGroup from dbo.ProcessHeader Group By FH_FC_CurrentComponent, FH_GG_ReleaseGroup";
			AssertHasError(loadedBand.BAB_SqlTextInfo, "You do not have permission to modify Acceptability Band SQL.");

			Env.Security.AcceptabilityBandModifyQuery.IsAllowed = true;
			loadedBand.Validation.ValidateBAB_SqlText();
			AssertNoErrors(loadedBand.BAB_SqlTextInfo);
		}

		#endregion

		public void TestComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.New<BMComponentAcceptabilityBand>();

			band.Validation.ValidateAll();
			AssertNoErrors(band.BAB_FC_ComponentInfo);

			AssertNoExceptionThrown("Should be able to save acceptability band with no component", () => Factory.Save());

			band.BAB_FC_Component = bucket.PK;
			AssertNoErrors(band.BAB_FC_ComponentInfo);
		}

		public void TestFilterByBoardSection_ForSqlBand_ShouldHaveError()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;
			band.BAB_FiltersBySection = true;
			AssertHasError(band.BAB_FiltersBySectionInfo, "Filtering by board section cannot be used for SQL Acceptability Bands. Please use AGR instead.");

			band.BAB_Type = AcceptabilityBandTypes.Codes.Aggregate;
			AssertNoErrors(band.BAB_FiltersBySectionInfo);

			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;
			band.BAB_FiltersBySection = false;
			AssertNoErrors(band.BAB_FiltersBySectionInfo);
		}

		public void TestGetCommandForColumnValidation_EnsureWriterConnectionUsed()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;

			using (var command = band.Validation.GetCommandForColumnValidation())
			{
				BMSTestHelper.AssertConnectionCannotBeUsedForWrites(command.DbConnection,
					"Writer connections should not be used when performing validation.");
			}
		}
	}
}
