using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BufferManagement
{
	[TestedType(typeof(PopulateProcessHeaderJobCodeAndJobDescriptionFromJLW))]
	class PopulateProcessHeaderJobCodeAndJobDescriptionFromJLWTest : DataTransformationTestCase
	{
		Guid jobHeader1PK = Guid.NewGuid();
		Guid jobHeader2PK = Guid.NewGuid();

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateProcessHeaderJobCodeAndJobDescriptionFromJLW();

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.ProcessHeader
	(FH_PK, FH_FH_ParentHeader, FH_JobCode, FH_JobDescription, FH_WorkflowType, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_SystemCreateUser, FH_SystemLastEditUser, FH_Status, FH_IsActive, FH_ParentId)
VALUES
	-- job header with JobCode and JobDescription
	('{jobHeader1PK}', NULL, 'WI01234567', 'My little job', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, '{Guid.NewGuid()}'),
	('{Guid.NewGuid()}', '{jobHeader1PK}', '', '', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, NULL),
	('{Guid.NewGuid()}', '{jobHeader1PK}', '', '', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, null),
	-- job header without JobCode and JobDescription
	('{jobHeader2PK}', NULL, '', '', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, '{Guid.NewGuid()}'),
	('{Guid.NewGuid()}', '{jobHeader2PK}', '', '', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, NULL),
	('{Guid.NewGuid()}', '{jobHeader2PK}', '', '', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, NULL)
");
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertEquals(3, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.ProcessHeader WHERE (FH_PK = '{jobHeader1PK}' OR FH_FH_ParentHeader = '{jobHeader1PK}') AND FH_JobCode = 'WI01234567' AND FH_JobDescription = 'My little job'"));
				AssertEquals(3, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.ProcessHeader WHERE (FH_PK = '{jobHeader2PK}' OR FH_FH_ParentHeader = '{jobHeader2PK}') AND FH_JobCode = '' AND FH_JobDescription = ''"));
			});
		}
	}
}
