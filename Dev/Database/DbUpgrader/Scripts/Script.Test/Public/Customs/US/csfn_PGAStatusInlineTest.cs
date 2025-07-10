using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_PGAStatusInline))]
	class csfn_PGAStatusInlineTest : DbCreateScriptTest
	{
		public void Testcsfn_PGAStatusInline()
		{
			var parentID = Guid.NewGuid();
			var cusDispositionSql = @"
INSERT INTO dbo.CusDisposition (CDI_PK, CDI_ParentID, CDI_ParentTableCode, CDI_Type, CDI_StatusKey, CDI_Status, CDI_StatusDate, CDI_Sequence, CDI_Notes)
VALUES
 (NEWID(), @CDI_ParentID, 'JE', 'PES', 'APH', '07', '2023-07-05 12:01', 0, ''),
 (NEWID(), @CDI_ParentID, 'JE', 'PES', 'NHT', '02', '2023-07-05 13:14', 0, ''),
 (NEWID(), @CDI_ParentID, 'JE', 'PES', 'FDA', '01', '2023-07-05 14:45', 0, '')";

			using (var command = Db.Connection.Command(cusDispositionSql))
			{
				command.AddParameter("@CDI_ParentID", SqlDbType.UniqueIdentifier, parentID);
				command.ExecuteNonQuery();
			}

			var value = "";
			Db.Connection.ExecuteReader($@"SELECT Value FROM csfn_PGAStatusInline('{parentID}', 'desc')", reader =>
			{
				value = reader.GetString(0);
			});

			AssertEquals("Value", "APH - MAY PROCEED; NHT - HOLD INTACT; FDA - DATA UNDER PGA REVIEW", value);

			value = "";
			Db.Connection.ExecuteReader($@"SELECT Value FROM csfn_PGAStatusInline('{parentID}', 'dateTime')", reader =>
			{
				value = reader.GetString(0);
			});

			AssertEquals("Value", "APH - 05-Jul-23 12:01; NHT - 05-Jul-23 13:14; FDA - 05-Jul-23 14:45", value);
		}
	}
}
