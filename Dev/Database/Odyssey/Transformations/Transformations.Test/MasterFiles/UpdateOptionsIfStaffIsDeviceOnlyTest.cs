using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.MasterFiles
{
	[TestedType(typeof(UpdateOptionsIfStaffIsDeviceOnly))]
	public class UpdateOptionsIfStaffIsDeviceOnlyTest : DataTransformationTestCase
	{
		readonly static Guid DbDeveloperGroupPK = new Guid("a99e7f0e-8379-4f50-8560-9b4bb804c0de");
		readonly static Guid DbReaderGroupPK = new Guid("6f0eb310-fc5c-4696-9594-f8ce156542c6");
		readonly static Guid BackupOperatorGroupPK = new Guid("208068b6-3383-44bf-8e0d-dbd827f9d675");

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateOptionsIfStaffIsDeviceOnly();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update some columns to false if staff is device only._1] ON [dbo].[GlbStaff] ([GS_IsDevice]) INCLUDE ([GS_PK], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser]) WHERE ([GS_IsDevice]=(1)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var staff1Pk = testDataCreator.CreateStaff("staff1", "SF1");
			var staff2Pk = testDataCreator.CreateStaff("staff2", "SF2");
			var staff3Pk = testDataCreator.CreateStaff("staff3", "SF3");
			var staff4Pk = testDataCreator.CreateStaff("staff4", "SF4");
			var staff5Pk = testDataCreator.CreateStaff("staff5", "SF5");

			testDataCreator.CreateGlbGroupLink(DbDeveloperGroupPK, staff1Pk);
			testDataCreator.CreateGlbGroupLink(DbReaderGroupPK, staff2Pk);

			testDataCreator.CreateGlbGroupLink(BackupOperatorGroupPK, staff3Pk);

			testDataCreator.CreateGlbGroupLink(DbDeveloperGroupPK, staff5Pk);
			testDataCreator.CreateGlbGroupLink(DbReaderGroupPK, staff5Pk);
			testDataCreator.CreateGlbGroupLink(BackupOperatorGroupPK, staff5Pk);

			TestConnection.ExecuteNonQuery($@"
ALTER TABLE dbo.GlbStaff DROP CONSTRAINT Constraint_GS_IsDevice;
UPDATE dbo.GlbStaff SET GS_IsDevice = 1, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = '~BP' WHERE GS_PK = '{staff1Pk}';
UPDATE dbo.GlbStaff SET GS_IsDevice = 1, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = '~BP' WHERE GS_PK = '{staff2Pk}';
UPDATE dbo.GlbStaff SET GS_IsDevice = 1, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = '~BP' WHERE GS_PK = '{staff3Pk}';
UPDATE dbo.GlbStaff SET GS_IsDevice = 1, GS_IsController = 1, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = '~BP' WHERE GS_PK = '{staff4Pk}';
UPDATE dbo.GlbStaff SET GS_IsController = 1, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = '~BP' WHERE GS_PK = '{staff5Pk}';
");
		}

		protected override void AssertPreConditions()
		{
			AssertEquals(3, TestConnection.ExecuteScalar($"SELECT count(*) FROM dbo.GlbGroupLink WHERE GK_GG IN ('{DbDeveloperGroupPK}', '{DbReaderGroupPK}', '{BackupOperatorGroupPK}') AND GK_GS IN (SELECT GS_PK FROM dbo.GlbStaff WHERE GS_IsDevice = 1);"));
			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT count(*) FROM dbo.GlbStaff WHERE GS_IsDevice = 1 AND GS_IsController = 1"));
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertEquals(0, TestConnection.ExecuteScalar($"SELECT count(*) FROM dbo.GlbGroupLink WHERE GK_GG IN ('{DbDeveloperGroupPK}', '{DbReaderGroupPK}', '{BackupOperatorGroupPK}') AND GK_GS IN (SELECT GS_PK FROM dbo.GlbStaff WHERE GS_IsDevice = 1);"));
				AssertEquals(0, TestConnection.ExecuteScalar($"SELECT count(*) FROM dbo.GlbStaff WHERE GS_IsDevice = 1 AND GS_IsController = 1"));
			});
		}
	}
}
