using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_UPD_HRJobApplicationDocument_ContentVersion))]
	class TG_UPD_HRJobApplicationDocument_ContentVersionTest : DbCreateScriptTest
	{
		public void TestContentVersion()
		{
			var hrJobApplicationDocumentPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(
$@"INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName) values ('51325B1F-2182-4B02-974C-1EE177D3B1C7', 'fullname');
INSERT INTO dbo.HRJobApplicant(HA_PK, HA_PER, HA_EmailAddress, HA_SystemCreateTimeUtc, HA_SystemLastEditTimeUtc, HA_SystemCreateUser, HA_SystemLastEditUser) values ('87FF0A0C-0459-4CFD-B2C6-2AEAB2C2D4D2', '51325B1F-2182-4B02-974C-1EE177D3B1C7', 'email123@gamil.com', GETUTCDATE(), GETUTCDATE(), 'V20', 'V20');
INSERT INTO dbo.HRJobApplication(HP_PK, HP_HA, HP_ApplicationNumber, HP_SystemCreateTimeUtc, HP_SystemLastEditTimeUtc, HP_SystemCreateUser, HP_SystemLastEditUser) values ('6DB1DBA8-E095-4587-9D2D-9095A7246719', '87FF0A0C-0459-4CFD-B2C6-2AEAB2C2D4D2', '123', GETUTCDATE(), GETUTCDATE(), 'V20', 'V20');
INSERT INTO dbo.HRJobApplicationDocument(HPD_PK, HPD_HP, HPD_Type, HPD_SystemCreateTimeUtc, HPD_SystemLastEditTimeUtc, HPD_SystemCreateUser, HPD_SystemLastEditUser)
VALUES ('{hrJobApplicationDocumentPK}', '6DB1DBA8-E095-4587-9D2D-9095A7246719', 'RES', GETUTCDATE(), GETUTCDATE(), 'V20', 'V20');");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT HPD_ContentVersion FROM dbo.HRJobApplicationDocument WHERE HPD_PK='{hrJobApplicationDocumentPK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.HRJobApplicationDocument SET HPD_Type='OTH', HPD_SystemLastEditTimeUtc = GETUTCDATE(), HPD_SystemLastEditUser = '~BP' WHERE HPD_PK='{hrJobApplicationDocumentPK}'");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT HPD_ContentVersion FROM dbo.HRJobApplicationDocument WHERE HPD_PK='{hrJobApplicationDocumentPK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.HRJobApplicationDocument SET HPD_Content='test', HPD_SystemLastEditTimeUtc = GETUTCDATE(), HPD_SystemLastEditUser = '~BP' WHERE HPD_PK='{hrJobApplicationDocumentPK}'");
			AssertEquals((short)1, TestConnection.ExecuteScalar<short>($"SELECT HPD_ContentVersion FROM dbo.HRJobApplicationDocument WHERE HPD_PK='{hrJobApplicationDocumentPK}'"));
		}
	}
}

