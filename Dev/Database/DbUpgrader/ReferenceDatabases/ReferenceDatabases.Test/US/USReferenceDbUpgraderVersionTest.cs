using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	abstract class USReferenceDbUpgraderVersionTest : ReferenceDbUpgraderVersionTest<USReferenceDbUpgraderForVersionTesting>
	{
		protected override void DoExtraAssertions()
		{
			base.DoExtraAssertions();
			AssertScheduleBData();

			int latestVersion = LatestVersionNumber;
			int actualLatestVersion = refDbUpgrader.GetActualLatestVersion(testConnection);
			Assert(string.Format("You have not updated USReferenceDbUpgrader.LatestVersion to {0}. It is currently {1}.", latestVersion, actualLatestVersion), latestVersion <= actualLatestVersion);
		}

		void AssertScheduleBData()
		{
			AssertEquals(9363, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCScheduleB"));
			AssertEquals("FROG'S LEGS, FRESH, CHILLED OR FROZEN", UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select UB_ShortDescription from dbo.USCScheduleB where UB_Code = '0208902500'"));
			AssertEquals("CGM", UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select UB_Unit1 from dbo.USCScheduleB where UB_Code = '2616100040'"));
			AssertEquals("CKG", UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select UB_Unit2 from dbo.USCScheduleB where UB_Code = '2815120000'"));
			AssertEquals("CRABMEAT, NESOI", UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select UB_ShortDescription from dbo.USCScheduleB where UB_Code = '0306932000'"));
		}

		public const string CreateUSCRuleSecondaryTariffScript = @"
CREATE TABLE [USCRuleSecondaryTariff]
	(
		[U3_PK] [uniqueidentifier] NOT NULL CONSTRAINT [DF_USCRuleSecondaryTariff_U3_PK]  DEFAULT (newid()),
		[U3_U1] [uniqueidentifier] NOT NULL,
		[U3_TariffFrom] [varchar](10) NOT NULL DEFAULT (''),
		[U3_TariffTo] [varchar](10) NOT NULL DEFAULT (''),
		[U3_DateFrom] [datetime] NOT NULL,
		[U3_DateTo] [datetime] NULL,
		[U3_Tariff2] [varchar](10) NOT NULL DEFAULT (''),
		[U3_Tariff3] [varchar](10) NOT NULL DEFAULT (''),

		CONSTRAINT [PK_USCRuleSecondaryTariff] PRIMARY KEY NONCLUSTERED
		(
			[U3_PK] ASC
		)
	)
";
	}
}
