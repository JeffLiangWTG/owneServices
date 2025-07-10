using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_UPD_DtbConsignmentAddress_NotesVersion))]
	class TG_UPD_DtbConsignmentAddress_NotesVersionTest : DbCreateScriptTest
	{
		public void TestNotesVersion()
		{
			var dtbConsignmentAddressPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(
$@"INSERT INTO dbo.DtbConsignment (LTC_PK, LTC_ConsignmentType, LTC_JobID, LTC_SystemCreateTimeUtc, LTC_SystemLastEditTimeUtc, LTC_SystemLastEditUser, LTC_Direction, LTC_SystemCreateUser, LTC_JobType)
VALUES ('B8D15389-03E4-4513-83B4-CA53526B1881', 'LTC', 'TN00000001', GETUTCDATE(), GETUTCDATE(), 'V20', 'LOC', 'V20', 'LTL');
INSERT INTO dbo.DtbConsignmentAddress (LTS_PK, LTS_LTC_Consignment, LTS_InstructionType, LTS_Status, LTS_Sequence, LTS_SystemCreateTimeUtc, LTS_SystemLastEditTimeUtc, LTS_SystemCreateUser, LTS_SystemLastEditUser)
VALUES ('{dtbConsignmentAddressPK}', 'B8D15389-03E4-4513-83B4-CA53526B1881', 'PIC', 'INC', 1, getutcdate(), getutcdate(), 'V20', 'V20');
");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT LTS_NotesVersion FROM dbo.DtbConsignmentAddress WHERE LTS_PK='{dtbConsignmentAddressPK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.DtbConsignmentAddress SET LTS_InstructionType='DLV' WHERE LTS_PK='{dtbConsignmentAddressPK}'");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT LTS_NotesVersion FROM dbo.DtbConsignmentAddress WHERE LTS_PK='{dtbConsignmentAddressPK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.DtbConsignmentAddress SET LTS_Notes='test' WHERE LTS_PK='{dtbConsignmentAddressPK}'");
			AssertEquals((short)1, TestConnection.ExecuteScalar<short>($"SELECT LTS_NotesVersion FROM dbo.DtbConsignmentAddress WHERE LTS_PK='{dtbConsignmentAddressPK}'"));
		}
	}
}

