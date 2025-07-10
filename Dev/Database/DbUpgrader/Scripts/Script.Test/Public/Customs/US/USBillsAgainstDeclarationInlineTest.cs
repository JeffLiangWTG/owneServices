using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USBillsAgainstDeclarationInline))]
	class USBillsAgainstDeclarationInlineTest : DbCreateScriptTest
	{
		public void TestUSBillsAgainstDeclarationInline()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1);
			var declaration1Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK1, 1, billNum: "B1", billType: "HB");
			var declaration1Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK1, 1, billNum: "B2", billType: "MB");
			TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, billNum: "B3", billType: "HB");
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration1Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration1Bill2);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2);
			var declaration2Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B4", billType: "MB");
			var declaration2Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B5", billType: "HB");
			var declaration2Bill3 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B6", billType: "HB");
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill2);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill3);

			CombineAssertions(() =>
			{
				AssertUSBillsAgainstDeclarationInline(1, "APLUB1, B3");
				AssertUSBillsAgainstDeclarationInline(2, "APLUB5, APLUB6");
			});
		}

		static void AssertUSBillsAgainstDeclarationInline(int declarationClusterKey, string expected)
		{
			const string sql = @"select * from USBillsAgainstDeclarationInline(@declarationClusterKey, 'HB')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationClusterKey", System.Data.SqlDbType.Int, declarationClusterKey);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(declarationClusterKey.ToString(), expected, reader.GetString(0));
				}
			}
		}
	}
}
