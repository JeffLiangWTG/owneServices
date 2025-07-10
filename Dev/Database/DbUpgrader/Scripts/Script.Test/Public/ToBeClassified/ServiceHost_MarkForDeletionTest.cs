using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(ServiceHost_MarkForDeletion))]
	class ServiceHost_MarkForDeletionTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestSetsCurrentTimeForHost()
		{
			//Arrange
			var sql = $@"
EXEC
ServiceHost_MarkForDeletion
'activeServiceHost1'
";

			Db.Connection.ExecuteNonQuery(sql);

			//Act
			var markedTime = Db.Connection.ExecuteScalar<DateTime>("SELECT SH_DeleteTimeStampUtc FROM dbo.StmServiceHost WHERE SH_HostName = 'activeServiceHost1'");

			//Assert
			var timeDiff = Math.Round((DateTime.UtcNow - markedTime).TotalMinutes);
			NUnit.Framework.Assert.That((int)timeDiff, NUnit.Framework.Is.EqualTo(0).Within(2), "We expect the marked time difference to be close to 0");
		}

		public void TestDoesNotSetTimeForOtherHosts()
		{
			//Arrange
			var sql = $@"
EXEC
ServiceHost_MarkForDeletion
'activeServiceHost1'
";

			Db.Connection.ExecuteNonQuery(sql);
			var result = new List<string>();

			//Act
			Db.Connection.ExecuteReader("SELECT SH_HostName FROM dbo.StmServiceHost WHERE SH_DeleteTimeStampUtc is NULL",
				reader =>
			{
				result.Add(reader["SH_HostName"].ToString());
			});

			//Assert
			AssertContainsExactElementsInAnyOrder("Query should return the only activeServiceHosts 2 and 3",
				new[] { "activeServiceHost2", "activeServiceHost3" }, result);
		}

		public void TestDoesNotOverrideExistingDeleteTimestamp()
		{
			CombineAssertions(() =>
			{
				Test("hostWithDeleteTime1", new DateTime(2022, 1, 1, 1, 1, 0));
				Test("hostWithDeleteTime2", new DateTime(2020, 10, 28, 1, 1, 0));
			});

			void Test(string hostName, DateTime deleteTimeStamp)
			{
				// Arrange
				CreateActiveServiceHost(hostName, deleteTimeStamp);
				var sql = $@"
	EXEC
	ServiceHost_MarkForDeletion
	'{hostName}'
	";

				// Act
				Db.Connection.ExecuteNonQuery(sql);

				// Assert
				var result = Db.Connection.ExecuteScalar<DateTime>($"SELECT SH_DeleteTimeStampUtc FROM dbo.StmServiceHost WHERE SH_HostName = '{hostName}'");
				AssertEquals(deleteTimeStamp, result);
			}
		}

		public void TestNumberOfRowsAffectedByProcedure()
		{
			CombineAssertions(() =>
			{
				//Act
				Test("activeServiceHost1", 1, "success");
				Test("fakeServiceHost", 0, "failure");
			});

			void Test(string hostName, int returnValue, string testCase)
			{
				//Arrange
				var sql = $@"
EXEC
ServiceHost_MarkForDeletion
'{hostName}'
";

				//Assert
				var sqlReturn = Db.Connection.ExecuteNonQuery(sql);
				AssertEquals($"Procedure should return {returnValue} on {testCase}", sqlReturn, returnValue);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateActiveServiceHost("activeServiceHost1");
			CreateActiveServiceHost("activeServiceHost2");
			CreateActiveServiceHost("activeServiceHost3");
		}

		void CreateActiveServiceHost(string hostName, DateTime? deleteDateTime = null)
		{
			var result = Guid.NewGuid();
			var addDeleteTime = deleteDateTime != null;
			var sql = string.Format(
$@"INSERT INTO {StmServiceHostSchema.Constants.SqlSchemaName}.{StmServiceHostSchema.Constants.TableName}
(
	{StmServiceHostSchema.Constants.PK}
	,{StmServiceHostSchema.Constants.SH_HostName}
	,{StmServiceHostSchema.Constants.SH_ProxyPort}
	,{StmServiceHostSchema.Constants.SH_ProxyAutoDetect}
	,{StmServiceHostSchema.Constants.SH_SystemCreateTimeUtc}
	,{StmServiceHostSchema.Constants.SH_SystemCreateUser}
	,{StmServiceHostSchema.Constants.SH_SystemLastEditTimeUtc}
	,{StmServiceHostSchema.Constants.SH_SystemLastEditUser}
	{(addDeleteTime ? $",{StmServiceHostSchema.Constants.SH_DeleteTimeStampUtc}" : string.Empty)}
)
VALUES
(
	@SH_PK
	,@SH_HostName
	,@SH_ProxyPort
	,@SH_ProxyAutoDetect
	,GetUtcDate()
	,'~BP'
	,GetUtcDate()
	,'~BP'
	{(addDeleteTime ? ",@SH_DeleteTimeStampUtc" : string.Empty)}
)");

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@SH_PK", result, StmServiceHostSchema.PK);
				command.AddParameterBasedOnDbColumn("@SH_HostName", hostName, StmServiceHostSchema.SH_HostName);
				command.AddParameterBasedOnDbColumn("@SH_ProxyPort", false, StmServiceHostSchema.SH_ProxyPort);
				command.AddParameterBasedOnDbColumn("@SH_ProxyAutoDetect", true, StmServiceHostSchema.SH_ProxyAutoDetect);
				if (addDeleteTime)
				{
					command.AddParameterBasedOnDbColumn("@SH_DeleteTimeStampUtc", deleteDateTime, StmServiceHostSchema.SH_DeleteTimeStampUtc);
				}

				command.ExecuteNonQuery();
			}
		}
	}
}
