using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(SupplierBookingExtraDelete))]
	class SupplierBookingExtraDeleteTest : DbCreateScriptTest
	{
		public void TestWith_SupplierBookingHeader()
		{
			InsertTestData();
			AssertEquals(2, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobDocAddress WHERE E2_ParentID IN (SELECT 'E56E74D4-1410-4922-9EA7-EB99FD321F3B' UNION ALL SELECT '86A13B4D-54DC-464D-A6E6-81253B05F08B')"));
			RunSP("E56E74D4-1410-4922-9EA7-EB99FD321F3B");
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobDocAddress WHERE E2_ParentID IN (SELECT 'E56E74D4-1410-4922-9EA7-EB99FD321F3B' UNION ALL SELECT '86A13B4D-54DC-464D-A6E6-81253B05F08B')"));
		}

		public void TestWith_SupplierBookingLine()
		{
			InsertTestData();
			AssertEquals(2, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobDocAddress WHERE E2_ParentID IN (SELECT 'E56E74D4-1410-4922-9EA7-EB99FD321F3B' UNION ALL SELECT '86A13B4D-54DC-464D-A6E6-81253B05F08B')"));
			RunSP("86A13B4D-54DC-464D-A6E6-81253B05F08B");
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobDocAddress WHERE E2_ParentID = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobDocAddress WHERE E2_ParentID = '86A13B4D-54DC-464D-A6E6-81253B05F08B'"));
		}

		void InsertTestData()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.SupplierBookingHeader (DH_PK, DH_OA_Consignor, DH_SystemCreateTimeUtc, DH_SystemLastEditTimeUtc) VALUES('E56E74D4-1410-4922-9EA7-EB99FD321F3B', '0EA85FB9-EC2E-4713-8A96-85B6290E97BB', GETDATE(), GETDATE())");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.SupplierBookingLine (DL_PK, DL_DH_BookingHeader, DL_SystemCreateTimeUtc, DL_SystemLastEditTimeUtc) VALUES('86A13B4D-54DC-464D-A6E6-81253B05F08B','E56E74D4-1410-4922-9EA7-EB99FD321F3B',GETDATE(),GETDATE())");

			TestConnection.ExecuteNonQuery("INSERT INTO dbo.JobDocAddress(E2_PK, E2_ParentID, E2_ParentTableCode) VALUES('A3C7B3E7-CE30-472D-8612-3C7EB4B28591', 'E56E74D4-1410-4922-9EA7-EB99FD321F3B', 'Z0')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.JobDocAddress(E2_PK, E2_ParentID, E2_ParentTableCode) VALUES('1BCA8DA4-94C6-47B2-8794-CD487C45A276', '86A13B4D-54DC-464D-A6E6-81253B05F08B', 'Z0')");
		}

		void RunSP(string pk)
		{
			using (var command = Db.Connection.Command("SupplierBookingExtraDelete"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@SupplierBookingPK", SqlDbType.UniqueIdentifier, new Guid(pk));
				command.ExecuteScalar();
			}
		}
	}
}
