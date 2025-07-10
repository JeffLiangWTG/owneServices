using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(csfn_RNSMessage))]
	class csfn_RNSMessageTest : DbCreateScriptTest
	{
		public void TestContainers()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CAYVR");
			var departmentPK = TestDataCreator.CreateDepartment("TD1");

			var sql = @"INSERT INTO dbo.EDIMessage (EM_PK, EM_MessageType, EM_ReceiveTransmit, EM_ApplicationCode, EM_Status, EM_MessageSubType, EM_MessageText, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) 
VALUES (newid(), 'REL', 'RCV', 'CAI', 'RCV', 'REL', @messageText, @branchPK, @departmentPK, GETUTCDATE(), 'DAT', GETUTCDATE(), 'DAT')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@messageText", SqlDbType.VarChar, "EQD+CN+CONTAINER1'EQD+CN+CONTAINER2");
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.ExecuteNonQuery();
			}

			string reportSql = "SELECT Containers FROM csfn_RNSMessage('', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals("CONTAINER1, CONTAINER2", reader.GetString(0));
				}
			}
		}
	}
}
