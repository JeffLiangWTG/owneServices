using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_UPD_IncidentRequest_DetailsVersion))]
	class TG_UPD_IncidentRequest_DetailsVersionTest : DbCreateScriptTest
	{
		public void TestDetailsVersion()
		{
			var incPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(
$@"INSERT INTO dbo.IncidentRequest (INC_PK, INC_IncidentNumber, INC_SystemCreateTimeUtc, INC_SystemLastEditTimeUtc, INC_SystemCreateUser, INC_SystemLastEditUser)
VALUES ('{incPK}', 'IR0001', GETUTCDATE(), GETUTCDATE(), 'V20', 'V20');");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT INC_DetailsVersion FROM dbo.IncidentRequest WHERE INC_PK='{incPK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.IncidentRequest SET INC_IncidentNumber='IR0002' WHERE INC_PK='{incPK}'");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT INC_DetailsVersion FROM dbo.IncidentRequest WHERE INC_PK='{incPK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.IncidentRequest SET INC_Details='test' WHERE INC_PK='{incPK}'");
			AssertEquals((short)1, TestConnection.ExecuteScalar<short>($"SELECT INC_DetailsVersion FROM dbo.IncidentRequest WHERE INC_PK='{incPK}'"));
		}
	}
}

