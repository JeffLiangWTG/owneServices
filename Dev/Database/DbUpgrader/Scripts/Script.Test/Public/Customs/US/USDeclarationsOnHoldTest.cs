using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USDeclarationsOnHold))]
	class USDeclarationsOnHoldTest : DbCreateScriptTest
	{
		public void TestSystemCreateTimeUtc()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var utcNow = DateTime.UtcNow;
			var dateFrom = utcNow;
			var dateTo = utcNow.AddMinutes(1);

			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 1, createTime: utcNow, dataModel: "US");
			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 2, createTime: utcNow.AddMinutes(2), dataModel: "US");
			var declarationPk3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 3, createTime: utcNow.AddMinutes(-2), dataModel: "US");
			
			TestDataCreator.CreateCusAddInfo("UDP", $"Code=03*DispositionDate={DateTime.Now:yyyy-MM-dd HH:mm:ss tt}", "B0", declarationPk1);
			TestDataCreator.CreateCusAddInfo("UDP", $"Code=03*DispositionDate={DateTime.Now:yyyy-MM-dd HH:mm:ss tt}", "B0", declarationPk2);
			TestDataCreator.CreateCusAddInfo("UDP", $"Code=03*DispositionDate={DateTime.Now:yyyy-MM-dd HH:mm:ss tt}", "B0", declarationPk3);

			var reportSql = @"SELECT JE_DeclarationReference FROM USDeclarationsOnHold(@companyPK, @dateFrom, @dateTo, null)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<string>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.SmallDateTime, dateFrom);
				command.AddParameter("@dateTo", SqlDbType.SmallDateTime, dateTo);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((string)reader["JE_DeclarationReference"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals("Test001", retList[0]);
			}
		}
	}
}
