using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion63_USCCountry_AddUcCodeWhenUserHaveNoColumnYet : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber
		{
			get { return 63; }
		}

		protected override void PrepareTestData(DbConnection conn)
		{
			base.PrepareTestData(conn);

			string script = @"
DECLARE @dfname NVARCHAR(50),
        @dropsql NVARCHAR(100)
SELECT @dfname=name FROM sys.objects WHERE object_id = (SELECT sys.columns.default_object_id FROM sys.objects INNER JOIN sys.columns ON sys.objects.object_id = sys.columns.object_id 
    WHERE upper(sys.columns.name) = 'UC_Code' AND upper(sys.objects.name) = 'USCCountry')
    IF LEN(@dfname)>0
    SET @dropsql='ALTER TABLE USCCountry DROP CONSTRAINT '+ CAST(@dfname AS NVARCHAR(50))
    IF LEN(@dfname)>0
    EXEC sp_executesql @dropsql";
			conn.ExecuteNonQuery(script);

			var dropColumn = @"
if exists(select null FROM sys.objects, sys.columns where sys.objects.object_id = sys.columns.object_id and sys.objects.type = 'u' and sys.objects.name = 'USCCountry' and sys.columns.name = 'UC_Code')
	begin
        IF EXISTS (SELECT null FROM sys.indexes WHERE name = 'UC_Code')
	    begin
		DROP INDEX USCCountry.UC_Code
	    end
		drop index usccountry.NR_IX__UC_Code
		ALTER TABLE USCCountry DROP COLUMN UC_Code
	end";
			conn.ExecuteNonQuery(dropColumn);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select Count(*) from sys.columns where name = 'uc_IsoCountryCode'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.columns where name = 'uc_Code'"));
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCCountry where uc_code != uc_IsoCOuntryCode"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where name = 'NR_IX__UC_Code'"));
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where name = 'NR_IX__UC_IsoCountryCode'"));
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where name = 'UC_Code'"));
		}
	}
}
