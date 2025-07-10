using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USEntriesOnHoldStats))]
	class USEntriesOnHoldStatsTest : DbCreateScriptTest
	{
		public void TestUSEntriesOnHoldStatsOnReport()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK1 = CreateJobDeclaration(importerPK, branchPK, companyPK, "Test001", 1, new DateTime(2022, 1, 1), "");
			var declarationPK2 = CreateJobDeclaration(importerPK, branchPK, companyPK, "Test002", 2, new DateTime(2022, 2, 1), "");
			var declarationPK3 = CreateJobDeclaration(importerPK, branchPK, companyPK, "Test003", 3, new DateTime(2022, 2, 2), "");
			var cusDecPk1 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1);
			var cusDecPk2 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK2, 2);
			var cusDecPk3 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK3, 3);
			var b7Pk1 = TestDataCreator.CreateCusAddInfo("UDP", "Code=0307*Source=SO", "CU", cusDecPk1);
			var b7Pk2 = TestDataCreator.CreateCusAddInfo("UDP", "Code=51*Source=SO", "CU", cusDecPk2);
			var b7Pk3 = TestDataCreator.CreateCusAddInfo("UDP", "Code=52*Source=SO", "CU", cusDecPk3);

			DateTime dateFrom = new DateTime(2021, 12, 1);
			DateTime dateTo = new DateTime(2022, 3, 1);

			var reportSql = @"select Month, Year, FileCount, IntensiveCount, StatusCount_51, StatusCount_52, StatusCount_53 from USEntriesOnHoldStats(@companyPK, @dateFrom, @dateTo, @importerPK) ORDER BY Year, Month";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<int, int, int, int, int, int, int>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.SmallDateTime, dateFrom);
				command.AddParameter("@dateTo", SqlDbType.SmallDateTime, dateTo);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var month = (int)reader["Month"];
						var year = (int)reader["Year"];
						var fileCount = (int)reader["FileCount"];
						var intensiveCount = (int)reader["IntensiveCount"];
						var statusCount_51 = (int)reader["StatusCount_51"];
						var statusCount_52 = (int)reader["StatusCount_52"];
						var statusCount_53 = (int)reader["StatusCount_53"];
						reportList.Add(new Tuple<int, int, int, int, int, int, int>(month, year, fileCount, intensiveCount, statusCount_51, statusCount_52, statusCount_53));
					}
				}
				AssertEquals(2, reportList.Count);
				var result1 = reportList.Find(x => x.Item1 == 1 && x.Item2 == 2022);
				AssertEquals(1, result1.Item3);
				AssertEquals(1, result1.Item4);
				AssertEquals(0, result1.Item5);
				AssertEquals(0, result1.Item6);
				AssertEquals(0, result1.Item7);
				var result2 = reportList.Find(x => x.Item1 == 2 && x.Item2 == 2022);
				AssertEquals(2, result2.Item3);
				AssertEquals(0, result2.Item4);
				AssertEquals(1, result2.Item5);
				AssertEquals(1, result2.Item6);
				AssertEquals(0, result2.Item7);
			}
		}

		[TestDate(2024, 03, 12, 08, 00, 00)]
		public void TestSystemCreateTimeUtc()
		{
			var reportSql = @"SELECT FileCount FROM USEntriesOnHoldStats(@companyPK, @dateFrom, @dateTo, null)";

			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var utcNow = DateTime.UtcNow;
			var dateFrom = utcNow;
			var dateTo = utcNow.AddMinutes(2);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 1, createTime: utcNow, dataModel: "US");
			var count = QueryCount();
			AssertEquals(1, count.Count);
			AssertEquals(1, count[0]);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 2, createTime: utcNow.AddMonths(1), dataModel: "US");
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 3, createTime: utcNow.AddMonths(-1), dataModel: "US");
			count = QueryCount();
			AssertEquals(1, count.Count);
			AssertEquals(1, count[0]);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test004", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 4, createTime: utcNow.AddMinutes(1), dataModel: "US");
			count = QueryCount();
			AssertEquals(1, count.Count);
			AssertEquals(2, count[0]);

			List<int> QueryCount()
			{
				var result = new List<int>();
				using (var command = Db.Connection.Command(reportSql))
				{
					command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
					command.AddParameter("@dateFrom", SqlDbType.SmallDateTime, dateFrom);
					command.AddParameter("@dateTo", SqlDbType.SmallDateTime, dateTo);
					
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add((int)reader["FileCount"]);
						}
					}
				}
				return result;
			}
		}

		Guid CreateJobDeclaration(Guid importerPK, Guid branchPK, Guid companyPK, string decReference, int clusterKey, DateTime createTime, string addInfo = "")
		{
			var declarationPK = Guid.NewGuid();
			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey, JE_AddInfo, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, @decReference, 'ACE', 0, @clusterKey, @addInfo, @createTime, '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@decReference", SqlDbType.VarChar, decReference);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@createTime", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}
	}
}
