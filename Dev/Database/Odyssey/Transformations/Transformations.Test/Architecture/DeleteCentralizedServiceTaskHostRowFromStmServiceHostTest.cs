using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Architecture
{
	class DeleteCentralizedServiceTaskHostRowFromStmServiceHostTest : TestCase
	{
		[TestedType(typeof(DeleteCentralizedServiceTaskHostRowFromStmServiceHost))]
		class DeleteRowWhereHostNameIsCentralizedServiceTaskHost : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new DeleteCentralizedServiceTaskHostRowFromStmServiceHost();
			}

			protected override void PrepareTestData()
			{
				using (DataTransformationHelper.SuspendInsertAuditTriggerIfExists("StmServiceHost"))
				{
					TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceHost

INSERT INTO [dbo].[StmServiceHost]
	([SH_PK] ,[SH_IsActive] ,[SH_HostName] ,[SH_DeleteTimeStampUtc] ,[SH_ProxyHost] ,[SH_ProxyPort] ,[SH_ProxyUserName] ,[SH_ProxyPassword] ,[SH_ProxyAutoDetect])
VALUES
	(newid() ,1 ,'CentralizedServiceTaskHost' ,NULL ,'' ,0 ,'' ,'' ,1)
");
				}
			}

			protected override void AssertTransformationResults()
			{
				var sqlText = "SELECT SH_HostName FROM dbo.StmServiceHost;";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

				AssertEquals(0, dataTable.Rows.Count);
			}
		}

		[TestedType(typeof(DeleteCentralizedServiceTaskHostRowFromStmServiceHost))]
		class DoNothingIfCentralizedServiceTaskHostRowNotExist : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new DeleteCentralizedServiceTaskHostRowFromStmServiceHost();
			}

			protected override void PrepareTestData()
			{
				using (DataTransformationHelper.SuspendInsertAuditTriggerIfExists("StmServiceHost"))
				{
					TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceHost

INSERT INTO [dbo].[StmServiceHost]
	([SH_PK] ,[SH_IsActive] ,[SH_HostName] ,[SH_DeleteTimeStampUtc] ,[SH_ProxyHost] ,[SH_ProxyPort] ,[SH_ProxyUserName] ,[SH_ProxyPassword] ,[SH_ProxyAutoDetect])
VALUES
	(newid() ,1 ,'DummyHost' ,NULL ,'' ,0 ,'' ,'' ,1)
");
				}
			}

			protected override void AssertTransformationResults()
			{
				var sqlText = "SELECT SH_HostName FROM dbo.StmServiceHost;";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

				AssertEquals(1, dataTable.Rows.Count);
			}
		}
	}
}
