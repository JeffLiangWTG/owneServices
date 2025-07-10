using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetBillHoldAndHoldRemoveDateInline))]
	class csfn_GetBillHoldAndHoldRemoveDateInlineTest : DbCreateScriptTest
	{
		public void Testcsfn_GetBillHoldAndHoldRemoveDateInline()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1);
			var declaration1Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, "51/52/53");
			var declaration1Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, "51/52/53");
			var declaration1Bill3 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, "51/52/53");
			var declaration1Bill4 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, "51/52/53");
			var declaration1Bill5 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, "51/52/53");
			TestDataCreator.CreateCusAddInfo("UDP", "Code=1A*DispositionDate=2018-08-29 13:45:00.000*Order=1*Source=SO", "CU", declaration1Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=1B*DispositionDate=2018-08-29 13:46:00.000*Order=1*Source=SO", "CU", declaration1Bill2);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=51*DispositionDate=2018-08-29 13:51:00.000*Order=1*Source=SO", "CU", declaration1Bill3);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=52*DispositionDate=2018-08-29 13:52:00.000*Order=1*Source=SO", "CU", declaration1Bill4);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=53*DispositionDate=2018-08-29 13:53:00.000*Order=1*Source=SO", "CU", declaration1Bill5);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2);
			var declaration2Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK2, 2, "1G/1H/2P");
			var declaration2Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK2, 2, "1G/1H/2P");
			var declaration2Bill3 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK2, 2, "1G/1H/2P");
			TestDataCreator.CreateCusAddInfo("UDP", "Code=54*DispositionDate=2018-08-29 13:54:00.000*Order=1*Source=SO", "CU", declaration2Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=55*DispositionDate=2018-08-29 13:55:00.000*Order=1*Source=SO", "CU", declaration2Bill2);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=56*DispositionDate=2018-08-29 13:56:00.000*Order=1*Source=SO", "CU", declaration2Bill3);

			CombineAssertions(() =>
			{
				Assertcsfn_GetBillHoldAndHoldRemoveDateInline(1, "29/08/2018 1:45:00 PM", "29/08/2018 1:46:00 PM", "29/08/2018 1:51:00 PM", "29/08/2018 1:52:00 PM", "29/08/2018 1:53:00 PM", "", "", "");
				Assertcsfn_GetBillHoldAndHoldRemoveDateInline(2, "", "", "", "", "", "29/08/2018 1:54:00 PM", "29/08/2018 1:55:00 PM", "29/08/2018 1:56:00 PM");
			});
		}

		static void Assertcsfn_GetBillHoldAndHoldRemoveDateInline(int declarationClusterKey, string expectedIntensiveExamRequiredDate, string expectedIntensiveExamCompleteDate, string expectedDisposition51Date, string expectedDisposition52Date, string expectedDisposition53Date, string expectedDisposition54Date, string expectedDisposition55Date, string expectedDisposition56Date)
		{
			const string sql = @"select * from csfn_GetBillHoldAndHoldRemoveDateInline(@declarationClusterKey)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationClusterKey", System.Data.SqlDbType.Int, declarationClusterKey);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertDate("IntensiveExamRequiredDate", expectedIntensiveExamRequiredDate);
					AssertDate("IntensiveExamCompleteDate", expectedIntensiveExamCompleteDate);
					AssertDate("Disposition51Date", expectedDisposition51Date);
					AssertDate("Disposition52Date", expectedDisposition52Date);
					AssertDate("Disposition53Date", expectedDisposition53Date);
					AssertDate("Disposition54Date", expectedDisposition54Date);
					AssertDate("Disposition55Date", expectedDisposition55Date);
					AssertDate("Disposition56Date", expectedDisposition56Date);

					void AssertDate(string fieldName, string dateStr)
					{
						if (string.IsNullOrEmpty(dateStr))
						{
							AssertEquals(declarationClusterKey + "." + fieldName, DBNull.Value, reader[fieldName]);
						}
						else
						{
							AssertEquals(declarationClusterKey + "." + fieldName, dateStr, reader[fieldName].ToString());
						}
					}
				}
			}
		}
	}
}
