using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetCWOAsStringInline))]
	class csfn_GetCWOAsStringInlineTest : DbCreateScriptTest
	{
		public void Testcsfn_GetCWOAsStringInline()
		{
			var parentID = Guid.NewGuid();
			var cusCodeDataSql = @"
INSERT INTO dbo.CusCodeData (CY_PK, CY_Code, CY_Data, CY_ParentID, CY_ParentTableCode, CY_TYPE, CY_IsOverridden)
VALUES
 (NEWID(), '27J', '05', @CY_ParentID, 'JI', 'CWO', 0),
 (NEWID(), '27C', '50', @CY_ParentID, 'JI', 'CWO', 0),
 (NEWID(), '27M', '11', @CY_ParentID, 'JI', 'CWO', 0)";

			using (var command = Db.Connection.Command(cusCodeDataSql))
			{
				command.AddParameter("@CY_ParentID", SqlDbType.UniqueIdentifier, parentID);
				command.ExecuteNonQuery();
			}

			var cWOs = "";
			Db.Connection.ExecuteReader($@"SELECT CWOs FROM csfn_GetCWOAsStringInline('{parentID}')", reader =>
			{
				cWOs = reader.GetString(0);
			});

			AssertEquals("CWOs", "27J:05,27C:50,27M:11", cWOs);
		}
	}
}
