using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc.Test
{
	[TestedType(typeof(csfn_GetJD_OrderNumberAndSplitInline))]
	class csfn_GetJD_OrderNumberAndSplitInlineTest : DbCreateScriptTest
	{
		public void TestGetJD_OrderNumberAndSplit()
		{
			var orderHeaderPk1 = Guid.NewGuid();
			var orderHeaderPk2 = Guid.NewGuid();
			var orderLinePk1 = Guid.NewGuid();
			var orderLinePk2 = Guid.NewGuid();
			var orgHeaderPk = Guid.NewGuid();
			var orgAddressPk = Guid.NewGuid();
			var sqlText = string.Format(@"
insert into dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES ('{4}', 'TESTORG', 'Test Organisation')
insert into dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES ('{5}', '{4}', 'Address1')
insert into dbo.JobOrderHeader (JD_PK, JD_OrderNumber, JD_OrderNumberSplit, JD_OA_BuyerAddress) values ('{0}', 'X1', 0, '{5}')
insert into dbo.JobOrderHeader (JD_PK, JD_OrderNumber, JD_OrderNumberSplit, JD_OA_BuyerAddress) values ('{1}', 'X2', 3, '{5}')
insert into dbo.JobOrderLine (JO_PK, JO_JD) values ('{2}','{0}')
insert into dbo.JobOrderLine (JO_PK, JO_JD) values ('{3}','{1}')",
			orderHeaderPk1.ToString(), orderHeaderPk2.ToString(), orderLinePk1.ToString(), orderLinePk2.ToString(), orgHeaderPk.ToString(), orgAddressPk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			sqlText = string.Format("SELECT NumberAndSplit FROM dbo.csfn_GetJD_OrderNumberAndSplitInline('{0}', 'xxx')", orderLinePk1.ToString());
			DataTable table = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("X1", table.Rows[0][0].ToString());

			sqlText = string.Format("SELECT NumberAndSplit FROM dbo.csfn_GetJD_OrderNumberAndSplitInline('{0}', '')", orderLinePk2.ToString());
			table = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("X2-3", table.Rows[0][0].ToString());

			sqlText = string.Format("SELECT NumberAndSplit FROM dbo.csfn_GetJD_OrderNumberAndSplitInline(null, 'zzz')", orderLinePk2.ToString());
			table = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("zzz", table.Rows[0][0].ToString());
		}
	}
}
