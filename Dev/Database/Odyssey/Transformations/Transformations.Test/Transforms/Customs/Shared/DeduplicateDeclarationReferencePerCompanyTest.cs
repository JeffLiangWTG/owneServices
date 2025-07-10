namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared;

using System.Threading;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

[TestedType(typeof(DeduplicateDeclarationReferencePerCompany))]
sealed class DeduplicateDeclarationReferencePerCompanyTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new DeduplicateDeclarationReferencePerCompany();

	public void TestOnlinePreUpgrade()
	{
		PrepareTestData();
		var transform = new DeduplicateDeclarationReferencePerCompany();
		transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
		AssertTransformationResults();
		transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
		AssertTransformationResults();
	}

	public void TestOffLinePostUpgrade()
	{
		PrepareTestData();
		var transform = new DeduplicateDeclarationReferencePerCompany();
		transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
		AssertTransformationResults();
		transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
		AssertTransformationResults();
	}

	protected override void PrepareTestData()
	{
		// Drop unique index
		DBTransformationTestHelper.DropIndexIfExists(JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.Constants.Indexes.NR_UX__JE_DeclarationReference_JE_GC);

		// Insert test companies and declarations with some duplicates, including some with 35-char references (JE_DeclarationReference max length)
		var sql = @"
				DECLARE
					@Company1Pk UNIQUEIDENTIFIER = NEWID(),
					@Company2Pk UNIQUEIDENTIFIER = NEWID(),
					@Company3Pk UNIQUEIDENTIFIER = NEWID(),
					@Branch1Pk  UNIQUEIDENTIFIER = NEWID(),
					@Branch2Pk  UNIQUEIDENTIFIER = NEWID(),
					@Branch3Pk  UNIQUEIDENTIFIER = NEWID();

				INSERT INTO GlbCompany (GC_PK, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser) VALUES
					(@Company1Pk, '~C1', 'AU company1', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(@Company2Pk, '~C2', 'AU company2', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(@Company3Pk, '~C3', 'AU company3', GETUTCDATE(), GETUTCDATE(), 'E', 'E');

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser) VALUES
					(@Branch1Pk, @Company1Pk, '~B1', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(@Branch2Pk, @Company2Pk, '~B2', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(@Branch3Pk, @Company3Pk, '~B3', GETUTCDATE(), GETUTCDATE(), 'E', 'E');

				INSERT INTO JobDeclaration(JE_PK, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_SystemCreateUser, JE_GB, JE_GC, JE_DeclarationReference) VALUES
					(newid(),  1, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch1Pk, @Company1Pk, 'Ref1'),
					(newid(),  2, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch1Pk, @Company1Pk, 'Ref1'),
					(newid(),  3, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch1Pk, @Company1Pk, 'Ref1'),
					(newid(),  4, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch1Pk, @Company1Pk, 'Ref2'),
					(newid(),  5, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch2Pk, @Company2Pk, 'Ref1'),
					(newid(),  6, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch2Pk, @Company2Pk, 'Ref2'),
					(newid(),  7, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch2Pk, @Company2Pk, 'Ref2'),
					(newid(),  8, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch2Pk, @Company2Pk, 'ReferenceABCDEFGHIJKLMNOPQRSTUVWXYZ'),
					(newid(),  9, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch2Pk, @Company2Pk, 'ReferenceABCDEFGHIJKLMNOPQRSTUVWXYZ'),
					(newid(), 10, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch2Pk, @Company2Pk, 'ReferenceABCDEFGHIJKLMNOPQRSTUVWXYZ'),
					(newid(), 11, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch3Pk, @Company3Pk, 'Ref1'),
					(newid(), 12, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch3Pk, @Company3Pk, 'Ref2'),
					(newid(), 13, 'XX', GETUTCDATE(), GETUTCDATE(), 'E', 'E', @Branch3Pk, @Company3Pk, 'ReferenceABCDEFGHIJKLMNOPQRSTUVWXYZ');
			";

		TestConnection.ExecuteNonQuery(sql);
	}

	protected override void AssertPreConditions()
	{
		// Assert there are duplicates
		AssertDuplicates(expected: true);
	}

	protected override void AssertTransformationResults()
	{
		CombineAssertions(() =>
		{
			// Assert NO duplicates
			AssertDuplicates(expected: false);

			// Assert declaration references after transform
			AssertDeclarationReference(1, "Ref1");
			AssertDeclarationReference(2, "Ref1:2");
			AssertDeclarationReference(3, "Ref1:3");
			AssertDeclarationReference(4, "Ref2");
			AssertDeclarationReference(5, "Ref1");
			AssertDeclarationReference(6, "Ref2");
			AssertDeclarationReference(7, "Ref2:7");
			AssertDeclarationReference(8, "ReferenceABCDEFGHIJKLMNOPQRSTUVWXYZ");
			AssertDeclarationReference(9, "ReferenceABCDEFGHIJKLMNOPQRSTUVWX:9");
			AssertDeclarationReference(10, "ReferenceABCDEFGHIJKLMNOPQRSTUVW:10");
			AssertDeclarationReference(11, "Ref1");
			AssertDeclarationReference(12, "Ref2");
			AssertDeclarationReference(13, "ReferenceABCDEFGHIJKLMNOPQRSTUVWXYZ");

			// Assert Logged Events
			AssertLoggedEventCount(5);
			AssertRefChangeLogged("Ref1", "Ref1:2");
			AssertRefChangeLogged("Ref1", "Ref1:3");
			AssertRefChangeLogged("Ref2", "Ref2:7");
			AssertRefChangeLogged("ReferenceABCDEFGHIJKLMNOPQRSTUVWXYZ", "ReferenceABCDEFGHIJKLMNOPQRSTUVWX:9");
			AssertRefChangeLogged("ReferenceABCDEFGHIJKLMNOPQRSTUVWXYZ", "ReferenceABCDEFGHIJKLMNOPQRSTUVW:10");
		});
	}

	void AssertDuplicates(bool expected)
	{
		var sql = "IF EXISTS (SELECT 1 FROM JobDeclaration GROUP BY JE_GC, JE_DeclarationReference HAVING count(*) > 1) SELECT CONVERT(BIT, 1) ELSE SELECT CONVERT(BIT, 0)";
		var areThereDuplicates = TestConnection.ExecuteScalar<bool>(sql);
		AssertEquals("Are there duplicated declarations by company and reference?", expected, areThereDuplicates);
	}

	void AssertDeclarationReference(int clusterKey, string expectedRef)
	{
		var sql = $"SELECT JE_DeclarationReference FROM JobDeclaration WHERE JE_ClusterKey = {clusterKey}";
		var actualRef = TestConnection.ExecuteScalar<string>(sql);
		AssertEquals($"Declaration [CK={clusterKey}] Reference.", expectedRef, actualRef);
	}

	void AssertLoggedEventCount(int expectedCount)
	{
		var sql = "SELECT count(*) FROM StmALog WHERE SL_Table = 'JobDeclaration' AND SL_Reference like 'Reference Number Changed: % > %'";
		var actualCount = TestConnection.ExecuteScalar<int>(sql);
		AssertEquals("Logged Ref Changed Event count.", expectedCount, actualCount);
	}

	void AssertRefChangeLogged(string refBefore, string refAfter)
	{
		var sql = $"IF EXISTS (SELECT 1 FROM StmALog WHERE SL_Table = 'JobDeclaration' AND SL_Reference = 'Reference Number Changed: [{refBefore}] > [{refAfter}]') SELECT CONVERT(BIT, 1) ELSE SELECT CONVERT(BIT, 0)";
		var refChangedLogged = TestConnection.ExecuteScalar<bool>(sql);
		AssertEquals($"Was Reference Number Changed: [{refBefore}] > [{refAfter}] logged", true, refChangedLogged);
	}
}
