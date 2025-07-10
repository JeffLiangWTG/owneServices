using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetFDAPCAsStringInline))]
	class csfn_GetFDAPCAsStringInlineTest : DbCreateScriptTest
	{
		public void Testcsfn_GetFDAPCAsStringInline()
		{
			var parentID = Guid.NewGuid();
			var cusAddInfoSql = @"
INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
VALUES
 (NEWID(), 'USA', 'FDAProductCode=79E--ZX', 'JI', @B7_ParentID, 1),
 (NEWID(), 'USA', 'FDAProductCode=60E--ZY', 'JI', @B7_ParentID, 1),
 (NEWID(), 'USA', 'FDAProductCode=77E--ZM', 'JI', @B7_ParentID, 1)";

			using (var command = Db.Connection.Command(cusAddInfoSql))
			{
				command.AddParameter("@B7_ParentID", SqlDbType.UniqueIdentifier, parentID);
				command.ExecuteNonQuery();
			}

			var value = "";
			Db.Connection.ExecuteReader($@"SELECT Value FROM csfn_GetFDAPCAsStringInline('{parentID}', 'USA', 'FDAProductCode')", reader =>
			{
				value = reader.GetString(0);
			});

			AssertEquals("Value", "79E--ZX,60E--ZY,77E--ZM", value);
		}
	}
}
