using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetBillMessageStatusAsStringInline))]
	class csfn_GetBillMessageStatusAsStringInlineTest : DbCreateScriptTest
	{
		public void Testcsfn_GetBillMessageStatusAsStringInline()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1);
			TestDataCreator.CreateCusDecHouseBill(true, string.Empty, declarationPK1, 1, "51/52");
			TestDataCreator.CreateCusDecHouseBill(true, string.Empty, declarationPK1, 1, "1G/1H/2P");

			Assertcsfn_GetBillMessageStatusAsStringInline(1, "51/52,1G/1H/2P");
		}

		static void Assertcsfn_GetBillMessageStatusAsStringInline(int declarationClusterKey, string expectedStatus)
		{
			const string sql = @"select * from csfn_GetBillMessageStatusAsStringInline(@declarationClusterKey)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationClusterKey", System.Data.SqlDbType.Int, declarationClusterKey);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(expectedStatus, reader.GetString(0));
				}
			}
		}
	}
}
