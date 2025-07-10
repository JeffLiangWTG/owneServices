using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI;

[TestedType(typeof(UpdateReleaseBuildHlPackagePathToPointToNewArtifactRepoAlias))]
public class UpdateReleaseBuildHlPackagePathToPointToNewArtifactRepoAliasTest : DataTransformationTestCase
{
	protected override void AssertTransformationResults()
	{
		var allReleaseBuildEntries = releaseBuildsEntriesWithOldPrefix.Concat(releaseBuildsEntriesWithNewPrefix).ToList();
		var noReleaseBuildEntriesShouldBeDeleted = @$"SELECT COUNT(*)
FROM dbo.ReleaseBuild
WHERE HL_PK in ({string.Join(",", allReleaseBuildEntries.Select(x => $"'{x}'"))});";
		AssertEquals("None of the Release Build entries should be deleted.", allReleaseBuildEntries.Count, TestConnection.ExecuteScalar<int>(noReleaseBuildEntriesShouldBeDeleted));

		var allReleaseBuildEntriesShouldHaveNewPathPrefixSet = $@"SELECT COUNT(*)
FROM dbo.ReleaseBuild
WHERE HL_PK in ({string.Join(",", allReleaseBuildEntries.Select(x => $"'{x}'"))})
AND HL_PackagePath LIKE '\\cw1datfiles.wtg.zone\CW1Packages\%'";
		AssertEquals("All of the Release Build entries whose HL_PackagePath had the old prefix should have had it set to new prefix.", allReleaseBuildEntries.Count, TestConnection.ExecuteScalar<int>(allReleaseBuildEntriesShouldHaveNewPathPrefixSet));

		const string cw1Packages = @"\\cw1datfiles.wtg.zone\CW1Packages\";
		var updatedReleaseBuildsDataTable = DataUtils.GetDataTableFromQuery(
			TestConnection,
			$@"SELECT HL_PK, HL_PackagePath
FROM dbo.ReleaseBuild 
WHERE HL_PK IN ({string.Join(",", releaseBuildsEntriesWithOldPrefix.Select(x => $"'{x}'"))});");
		AssertEquals("Updated Release Build should retain pre update suffix.", cw1Packages + pathSuffixesOfPackagesWithOldPrefix[0], updatedReleaseBuildsDataTable.Select($"HL_PK='{releaseBuildsEntriesWithOldPrefix[0]}'")[0]["HL_PackagePath"].ToString());
		AssertEquals("Updated Release Build should retain pre update suffix.", cw1Packages + pathSuffixesOfPackagesWithOldPrefix[1], updatedReleaseBuildsDataTable.Select($"HL_PK='{releaseBuildsEntriesWithOldPrefix[1]}'")[0]["HL_PackagePath"].ToString());
		AssertEquals("Updated Release Build should retain pre update suffix.", cw1Packages + pathSuffixesOfPackagesWithOldPrefix[2], updatedReleaseBuildsDataTable.Select($"HL_PK='{releaseBuildsEntriesWithOldPrefix[2]}'")[0]["HL_PackagePath"].ToString());
	}

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new UpdateReleaseBuildHlPackagePathToPointToNewArtifactRepoAlias();
	}

	readonly List<string> pathSuffixesOfPackagesWithOldPrefix = new ()
	{
		"Package20240621_000000_24_6_21_01.edp",
		"Package20240622_000000_24_6_22_02.edp",
		"Package20240625_000000_24_6_25_05.edp",
	};

	readonly List<string> pathSuffixesOfPackagesWithNewPrefix = new ()
	{
		"Package20240623_000000_24_6_23_03.edp",
		"Package20240624_000000_24_6_24_04.edp",
		"Package20240626_000000_24_6_26_06.edp",
	};

	readonly List<Guid> releaseBuildsEntriesWithOldPrefix = new ()
	{
		Guid.NewGuid(),
		Guid.NewGuid(),
		Guid.NewGuid(),
	};

	readonly List<Guid> releaseBuildsEntriesWithNewPrefix = new ()
	{
		Guid.NewGuid(),
		Guid.NewGuid(),
		Guid.NewGuid(),
	};

	protected override void PrepareTestData()
	{
		var dataQuery = $@"
DECLARE @CW1Packages NVARCHAR(1000) = '\\cw1datfiles.wtg.zone\CW1Packages\';
DECLARE @datArtifactRepository NVARCHAR(1000)  = '\\datartifactrepository.wtg.zone\CW1Packages\';

IF NOT EXISTS (
	SELECT * 
	FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_NAME = N'ReleaseBuild'
)
BEGIN
CREATE TABLE dbo.ReleaseBuild
(
	[HL_PK] UNIQUEIDENTIFIER NOT NULL,
	[HL_Product] VARCHAR(3) NOT NULL DEFAULT 'ENT',
	[HL_ReleaseStatus] VARCHAR(3) NOT NULL DEFAULT '',
	[HL_ExeVersionDate] SMALLDATETIME NULL,
	[HL_MajorVersion] INT NOT NULL DEFAULT 0,
	[HL_MinorVersion] INT NOT NULL DEFAULT 0,
	[HL_Release] INT NOT NULL DEFAULT 0,
	[HL_Patch] INT NOT NULL DEFAULT 0,
	[HL_PackagePath] VARCHAR(256) NULL DEFAULT '',
	[HL_IsActive] BIT NOT NULL DEFAULT 1,
	[HL_Superceded] CHAR(1) NOT NULL DEFAULT 'Y',
	[HL_CompressedDLLs] VARBINARY(MAX) NULL,
	[HL_Comment] VARCHAR(256) NOT NULL DEFAULT '',
	[HL_IsTestPassed] BIT NOT NULL DEFAULT 0,
	[HL_TestDateUtc] SMALLDATETIME NULL,
	CONSTRAINT [PK_UX__HL_PK] PRIMARY KEY NONCLUSTERED ([HL_PK] ASC)
);
END

INSERT INTO dbo.ReleaseBuild (HL_PK , HL_PackagePath)
VALUES
	('{releaseBuildsEntriesWithOldPrefix[0]}', CONCAT(@datArtifactRepository, '{pathSuffixesOfPackagesWithOldPrefix[0]}')),
	('{releaseBuildsEntriesWithOldPrefix[1]}', CONCAT(@datArtifactRepository, '{pathSuffixesOfPackagesWithOldPrefix[1]}')),
	('{releaseBuildsEntriesWithNewPrefix[0]}', CONCAT(@CW1Packages, '{pathSuffixesOfPackagesWithNewPrefix[0]}')),
	('{releaseBuildsEntriesWithNewPrefix[1]}', CONCAT(@CW1Packages, '{pathSuffixesOfPackagesWithNewPrefix[1]}')),
	('{releaseBuildsEntriesWithOldPrefix[2]}', CONCAT(@datArtifactRepository, '{pathSuffixesOfPackagesWithOldPrefix[2]}')),
	('{releaseBuildsEntriesWithNewPrefix[2]}', CONCAT(@CW1Packages, '{pathSuffixesOfPackagesWithNewPrefix[2]}'))
;";

		using var cmd = Db.Connection.Command(dataQuery);
		cmd.ExecuteNonQuery();
	}
}
