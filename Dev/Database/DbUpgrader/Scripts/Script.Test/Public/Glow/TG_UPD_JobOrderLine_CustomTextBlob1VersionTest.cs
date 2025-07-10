using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_UPD_JobOrderLine_CustomTextBlob1Version))]
	class TG_UPD_JobOrderLine_CustomTextBlob1VersionTest : DbCreateScriptTest
	{
		public void TestCustomTextBlob1Version()
		{
			var orderHeaderPk = Guid.NewGuid();
			var orgAddressPk = Guid.NewGuid();
			var orderLinePk = Guid.NewGuid();
			var orgHeaderPk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(
$@"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES ('{orgHeaderPk}', 'TESTORG', 'Test Organisation');
INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES ('{orgAddressPk}', '{orgHeaderPk}', 'Address1')
INSERT INTO dbo.JobOrderHeader (JD_PK, JD_OrderNumber, JD_OrderNumberSplit, JD_OA_BuyerAddress) VALUES ('{orderHeaderPk}', 'X1', 0, '{orgAddressPk}');
INSERT INTO dbo.JobOrderLine (JO_PK, JO_JD) VALUES ('{orderLinePk}','{orderHeaderPk}');");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT JO_CustomTextBlob1Version FROM dbo.JobOrderLine WHERE JO_PK='{orderLinePk}'"));

			var editTime1 = new DateTime(2025, 6, 1);
			var editTime2 = new DateTime(2025, 6, 2);
			var editTime3 = new DateTime(2025, 6, 3);

			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobOrderLine
SET
	JO_Partno='100',
	JO_SystemLastEditTimeUtc = '{editTime1.ToString("yyyy-MM-dd HH:mm:ss.fff")}',
	JO_SystemLastEditUser = 'AAA'
WHERE
	JO_PK='{orderLinePk}'");
			AssertResult(orderLinePk, 0, editTime1, "AAA");

			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobOrderLine
SET
	JO_CustomTextBlob1 ='test',
	JO_SystemLastEditTimeUtc = '{editTime2.ToString("yyyy-MM-dd HH:mm:ss.fff")}',
	JO_SystemLastEditUser = 'AAA'
WHERE
	JO_PK='{orderLinePk}'");
			AssertResult(orderLinePk, 1, editTime2, "AAA");

			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobOrderLine
SET
	JO_CustomTextBlob1 ='test',
	JO_SystemLastEditTimeUtc = '{editTime2.ToString("yyyy-MM-dd HH:mm:ss.fff")}',
	JO_SystemLastEditUser = 'AAA'
WHERE
	JO_PK='{orderLinePk}'");
			AssertResult(orderLinePk, 1, editTime2, "AAA");

			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobOrderLine
SET
	JO_CustomTextBlob1 ='test1',
	JO_SystemLastEditTimeUtc = '{editTime3.ToString("yyyy-MM-dd HH:mm:ss.fff")}',
	JO_SystemLastEditUser = 'AAB'
WHERE
	JO_PK='{orderLinePk}'");
			AssertResult(orderLinePk, 2, editTime3, "AAB");
		}

		DataTable Execute(Guid orderLinePk)
		{
			using (var command = TestConnection.Command("SELECT JO_CustomTextBlob1Version, JO_SystemLastEditTimeUtc, JO_SystemLastEditUser FROM dbo.JobOrderLine WHERE JO_PK= @orderLinePk"))
			{
				command.AddParameter("@orderLinePk", SqlDbType.UniqueIdentifier, orderLinePk);
				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		void AssertResult(Guid orderLinePk, short expectedVersion, DateTime expectedLastEditTime, string expectedLastEditUser)
		{
			var results = Execute(orderLinePk).Select();
			AssertEquals(1, results.Length);
			var result = results[0];
			AssertEquals(expectedVersion, result[0]);
			AssertEquals(expectedLastEditTime, result[1]);
			AssertEquals(expectedLastEditUser, result[2]);
		}
	}
}

