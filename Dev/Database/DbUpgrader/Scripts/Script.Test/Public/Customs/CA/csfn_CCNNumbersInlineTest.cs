using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(csfn_CCNNumbersInline))]
	class csfn_CCNNumbersInlineTest : DbCreateScriptTest
	{
		public void Testcsfn_CCNNumbersInline()
		{
			var parentID = Guid.NewGuid();
			var cusAddInfoSql = @"
INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
VALUES
 (NEWID(), 'CAC', 'CCNInfoNumber=014-1235678', 'JE', @B7_ParentID, 1),
 (NEWID(), 'CAC', 'CCNInfoNumber=014-23456789', 'JE', @B7_ParentID, 1),
 (NEWID(), 'CAC', 'CCNInfoNumber=014-987654321', 'JE', @B7_ParentID, 1)";

			using (var command = Db.Connection.Command(cusAddInfoSql))
			{
				command.AddParameter("@B7_ParentID", SqlDbType.UniqueIdentifier, parentID);
				command.ExecuteNonQuery();
			}

			var cCNNumbers = "";
			Db.Connection.ExecuteReader($@"SELECT CCNNumbers FROM csfn_CCNNumbersInline('{parentID}')", reader =>
			{
				cCNNumbers = reader.GetString(0);
			});

			AssertEquals("CCNNumbers", "014-1235678, 014-23456789, 014-987654321", cCNNumbers);
		}
	}
}
