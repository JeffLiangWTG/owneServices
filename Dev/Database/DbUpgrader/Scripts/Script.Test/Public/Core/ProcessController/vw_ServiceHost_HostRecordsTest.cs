using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.ProcessController;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Core.ProcessController
{
	[TestedType(typeof(vw_ServiceHost_HostRecords))]
	class vw_ServiceHost_HostRecordsTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestHasRequiredFields()
		{
			CombineAssertions(() =>
			{
				//Act
				Test("HostName");
				Test("HostStatus");
			});

			void Test(string fieldName)
			{
				//Assert
				Assert(DbObjectCreator.ViewColumnExists(Db.Connection, "vw_ServiceHost_HostRecords", fieldName));
			}
		}

		public void TestValuesForStatus()
		{
			//Arrange
			var valuesFromDb = new List<string>();
			var script = $@"
SELECT HostStatus from dbo.vw_ServiceHost_HostRecords WHERE HostName = 'activeServiceHost1'
UNION
SELECT HostStatus from dbo.vw_ServiceHost_HostRecords WHERE HostName = 'deletingServiceHost1'
UNION
SELECT HostStatus from dbo.vw_ServiceHost_HostRecords WHERE HostName = 'obsoleteServiceHost1'";

			//Act
			Db.Connection.ExecuteReader(script,
				reader =>
				{
					valuesFromDb.Add(reader["HostStatus"].ToString());
				});

			//Assert
			AssertContainsExactElementsInAnyOrder(new[] { "INS", "DEL", "OBS" }, valuesFromDb);
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateActiveServiceHost("activeServiceHost1");
			CreateActiveServiceHost("activeServiceHost2");
			CreateServiceHost("deletingServiceHost1", DateTime.UtcNow.AddMinutes(-1));
			CreateServiceHost("deletingServiceHost2", DateTime.UtcNow.AddMinutes(-59));
			CreateServiceHost("obsoleteServiceHost1", DateTime.UtcNow.AddHours(-2));
			CreateServiceHost("obsoleteServiceHost2", DateTime.UtcNow.AddDays(-2));
		}

		void CreateActiveServiceHost(string hostName)
		{
			var result = Guid.NewGuid();
			var sql = string.Format(
@"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})
VALUES (@SH_PK, @SH_HostName, @SH_ProxyPort, @SH_ProxyAutoDetect, GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				StmServiceHostSchema.Constants.TableName,
				StmServiceHostSchema.Constants.PK,
				StmServiceHostSchema.Constants.SH_HostName,
				StmServiceHostSchema.Constants.SH_ProxyPort,
				StmServiceHostSchema.Constants.SH_ProxyAutoDetect,
				StmServiceHostSchema.Constants.SH_SystemCreateTimeUtc,
				StmServiceHostSchema.Constants.SH_SystemCreateUser,
				StmServiceHostSchema.Constants.SH_SystemLastEditTimeUtc,
				StmServiceHostSchema.Constants.SH_SystemLastEditUser);

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@SH_PK", result, StmServiceHostSchema.PK);
				command.AddParameterBasedOnDbColumn("@SH_HostName", hostName, StmServiceHostSchema.SH_HostName);
				command.AddParameterBasedOnDbColumn("@SH_ProxyPort", false, StmServiceHostSchema.SH_ProxyPort);
				command.AddParameterBasedOnDbColumn("@SH_ProxyAutoDetect", true, StmServiceHostSchema.SH_ProxyAutoDetect);

				command.ExecuteNonQuery();
			}
		}

		void CreateServiceHost(string hostName, DateTime deleteTimeStamp)
		{
			var result = Guid.NewGuid();
			var sql = string.Format(
@"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})
VALUES (@SH_PK, @SH_HostName, @SH_ProxyPort, @SH_ProxyAutoDetect, @SH_DeleteTimeStampUtc, GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				StmServiceHostSchema.Constants.TableName,
				StmServiceHostSchema.Constants.PK,
				StmServiceHostSchema.Constants.SH_HostName,
				StmServiceHostSchema.Constants.SH_ProxyPort,
				StmServiceHostSchema.Constants.SH_ProxyAutoDetect,
				StmServiceHostSchema.Constants.SH_DeleteTimeStampUtc,
				StmServiceHostSchema.Constants.SH_SystemCreateTimeUtc,
				StmServiceHostSchema.Constants.SH_SystemCreateUser,
				StmServiceHostSchema.Constants.SH_SystemLastEditTimeUtc,
				StmServiceHostSchema.Constants.SH_SystemLastEditUser);

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@SH_PK", result, StmServiceHostSchema.PK);
				command.AddParameterBasedOnDbColumn("@SH_HostName", hostName, StmServiceHostSchema.SH_HostName);
				command.AddParameterBasedOnDbColumn("@SH_ProxyPort", false, StmServiceHostSchema.SH_ProxyPort);
				command.AddParameterBasedOnDbColumn("@SH_ProxyAutoDetect", true, StmServiceHostSchema.SH_ProxyAutoDetect);
				command.AddParameterBasedOnDbColumn("@SH_DeleteTimeStampUtc", deleteTimeStamp, StmServiceHostSchema.SH_DeleteTimeStampUtc);

				command.ExecuteNonQuery();
			}
		}
	}
}
