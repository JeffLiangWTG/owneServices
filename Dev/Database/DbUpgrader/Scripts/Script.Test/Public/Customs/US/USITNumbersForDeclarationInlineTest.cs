using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USITNumbersForDeclarationInline))]
	class USITNumbersForDeclarationInlineTest : DbCreateScriptTest
	{
		public void TestUSITNumbersForDeclarationInline()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1);
			var declaration1Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1);
			var declaration1Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1);
			TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, billNum: "B3", billType: "HB");
			TestDataCreator.CreateCusAddInfo("ITN", "ITNumber=111", "CU", declaration1Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "ITNumber=222", "CU", declaration1Bill2);

			AssertUSITNumbersForDeclarationInline(1, "111");
		}

		static void AssertUSITNumbersForDeclarationInline(int declarationClusterKey, string expected)
		{
			const string sql = @"select * from USITNumbersForDeclarationInline(@declarationClusterKey)";
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
