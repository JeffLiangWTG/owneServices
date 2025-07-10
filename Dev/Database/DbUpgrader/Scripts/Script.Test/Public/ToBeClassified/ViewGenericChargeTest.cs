using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(ViewGenericCharge))]
	class ViewGenericChargeTest : DbCreateScriptTest
	{
		public void TestViewGenericChargeExcludesGlobalChargeCodes()
		{
			var testHelper = new TestDbHelper(TestConnection);
			Guid localChargeCodePK = testHelper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			Guid globalChargeCodePK = testHelper.InsertChargeCode(null, "CC1");
			Guid glAccountPK = testHelper.InsertGLAccount("1234.56.78", "My GL Account");
			var reader = (IDataReader)testHelper.RunSQL(null, "SELECT * FROM dbo.ViewGenericCharge ORDER BY VC_TableName, VC_Code", CommandType.Text, TestDbHelperBase.SQLExecutionTypes.ExecuteReader);
			var dataTable = new DataTable();
			dataTable.Load(reader);
			dataTable.DefaultView.RowFilter = string.Format("VC_PK = '{0}'", localChargeCodePK.ToString());
			AssertEquals("There should be a row for the local charge code", 1, dataTable.DefaultView.Count);
			dataTable.DefaultView.RowFilter = string.Format("VC_PK = '{0}'", glAccountPK.ToString());
			AssertEquals("There should be a row for the GL account", 1, dataTable.DefaultView.Count);
			dataTable.DefaultView.RowFilter = string.Format("VC_PK = '{0}'", globalChargeCodePK.ToString());
			AssertEquals("There should be no row for the global charge code", 0, dataTable.DefaultView.Count);
		}
	}
}

