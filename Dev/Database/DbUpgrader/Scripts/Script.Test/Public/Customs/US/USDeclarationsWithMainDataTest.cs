using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USDeclarationsWithMainData))]
	class USDeclarationsWithMainDataTest : DbCreateScriptTest
	{
		public void TestReconIssue()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US", addInfo: "OtherReconIndicator=NA");
			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "US", addInfo: "OtherReconIndicator=98");

			const string sql = @"SELECT JE_PK, ReconIssue FROM dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				var result = new List<Tuple<Guid, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JE_PK"];
						var reconIssue = (string)reader["ReconIssue"];
						result.Add(new Tuple<Guid, string>(pk, reconIssue));
					}
				}

				CombineAssertions(() =>
				{
					var result1 = result.Find(x => x.Item1 == declarationPK1);
					AssertEquals("NA", result1.Item2);
					var result2 = result.Find(x => x.Item1 == declarationPK2);
					AssertEquals("NA", result2.Item2);
					var result3 = result.Find(x => x.Item1 == declarationPK3);
					AssertEquals("98", result3.Item2);
				});
			}
		}

		public void TestNAFTAReconIndicator()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US", addInfo: "NAFTAReconIndicator=Y");
			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "US", addInfo: "NAFTAReconIndicator=N");

			const string sql = @"SELECT JE_PK, NAFTAReconIndicator FROM dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				var result = new List<Tuple<Guid, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JE_PK"];
						var naFTAReconIndicator = (string)reader["NAFTAReconIndicator"];
						result.Add(new Tuple<Guid, string>(pk, naFTAReconIndicator));
					}
				}

				CombineAssertions(() =>
				{
					var result1 = result.Find(x => x.Item1 == declarationPK1);
					AssertEquals("N", result1.Item2);
					var result2 = result.Find(x => x.Item1 == declarationPK2);
					AssertEquals("Y", result2.Item2);
					var result3 = result.Find(x => x.Item1 == declarationPK3);
					AssertEquals("N", result3.Item2);
				});
			}
		}

		public void TestReconFlagged()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US", addInfo: "NAFTAReconIndicator=Y*OtherReconIndicator=NA");
			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "US", addInfo: "NAFTAReconIndicator=N*OtherReconIndicator=NA");
			var declarationPK4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B004", "IMP", 4, dataModel: "US", addInfo: "NAFTAReconIndicator=N*OtherReconIndicator=98");
			var declarationPK5 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B005", "IMP", 5, dataModel: "US", addInfo: "NAFTAReconIndicator=Y*OtherReconIndicator=98");

			const string sql = @"SELECT JE_PK, ReconFlagged FROM dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				var result = new List<Tuple<Guid, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JE_PK"];
						var reconFlagged = (string)reader["ReconFlagged"];
						result.Add(new Tuple<Guid, string>(pk, reconFlagged));
					}
				}

				CombineAssertions(() =>
				{
					var result1 = result.Find(x => x.Item1 == declarationPK1);
					AssertEquals("N", result1.Item2);
					var result2 = result.Find(x => x.Item1 == declarationPK2);
					AssertEquals("Y", result2.Item2);
					var result3 = result.Find(x => x.Item1 == declarationPK3);
					AssertEquals("N", result3.Item2);
					var result4 = result.Find(x => x.Item1 == declarationPK4);
					AssertEquals("Y", result4.Item2);
					var result5 = result.Find(x => x.Item1 == declarationPK5);
					AssertEquals("Y", result5.Item2);
				});
			}
		}

		public void TestPortDescriptionUsingGlobalData()
		{
			var portSQL = @"INSERT INTO dbo.RefDbEntUS_USCForeignPort(UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES(NEWID(), '0000', 'TEST PORT 0000', '');";
			using (var command = Db.Connection.Command(portSQL))
			{
				command.ExecuteNonQuery();
			}
			TestDataCreator.CreateRefDatabaseRefDataGrouping("US", "US DataGrouping");
			TestDataCreator.CreateRefDbEntZZRefCusCodeType("PORT", "PORT", "US");
			var code9999PK = TestDataCreator.CreateRefDbEntZZRefCusCodeList("PORT", "9999", "TEST PORT 9999", "US");
			var code9998PK = TestDataCreator.CreateRefDbEntZZRefCusCodeList("PORT", "9998", "TEST PORT 9998", "US");
			var code9997PK = TestDataCreator.CreateRefDbEntZZRefCusCodeList("PORT", "9997", "TEST PORT 9997", "US");
			var code9996PK = TestDataCreator.CreateRefDbEntZZRefCusCodeList("PORT", "9996", "TEST PORT 9996", "US", startDate: "1900-01-01", endDate: "1900-01-02");
			var attributeNamePK = TestDataCreator.CreateRefCusCodeListAttributeName("PortValidType", "PORT", "US");
			TestDataCreator.CreateRefCusCodeListAttribute(code9998PK, "PortValidType", "");
			TestDataCreator.CreateRefCusCodeListAttribute(code9997PK, "PortValidType", "ASD");

			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var decPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US", addInfo: "SchDLoading=0000");
			var decPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US", addInfo: "SchDLoading=9999");
			var decPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "US", addInfo: "SchDLoading=9998");
			var decPK4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B004", "IMP", 4, dataModel: "US", addInfo: "SchDLoading=9997");
			var decPK5 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B005", "IMP", 5, dataModel: "US", addInfo: "SchDLoading=9996");

			var result = new Dictionary<Guid, string>();
			const string sql = @"select JE_PK, LadingPortDescription from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.IsDBNull(1) ? "" : reader.GetString(1));
					}
				}
			}
			AssertEquals("", result[decPK1]);
			AssertEquals("TEST PORT 9999", result[decPK2]);
			AssertEquals("TEST PORT 9998", result[decPK3]);
			AssertEquals("TEST PORT 9997", result[decPK4]);
			AssertEquals("TEST PORT 9996", result[decPK5]);
		}

		public void TestGetCorrectEntryNumber()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "FTZ", 1, dataModel: "US");
			var entryNum1 = TestDataCreator.CreateCusEntryNum(declarationPK1, "JobDeclaration", "ENSNUM", "ENS", "CUS", "US", newPK: new Guid("5AF93141-528C-48EB-B517-EFAC4A30C9B6"));
			var entryNum2 = TestDataCreator.CreateCusEntryNum(declarationPK1, "JobDeclaration", "FTZNUM", "FTZ", "CUS", "US", newPK: new Guid("28045B08-885E-402B-ACEB-A8431067E5C2"));

			const string sql = @"select JE_PK, CE_EntryNum from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", "FTZNUM", result[declarationPK1]);
					});
				}
			}
		}

		public void TestJE_ClusterKey()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");

			const string sql = @"select JE_PK, JE_ClusterKey from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, int>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetInt32(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 1, result[declarationPK1]);
						AssertEquals("declarationPK2", 2, result[declarationPK2]);
					});
				}
			}
		}

		public void TestENSActionAndCRLAction()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US", addInfo: "ENSAction=Incomplete*CRLAction=Complete");
			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");

			using (var cmd = Db.Connection.Command("SELECT JE_PK, JE_ENSAction, JE_CRLAction FROM dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)"))
			{
				cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = cmd.ExecuteReader())
				{
					var dt = new DataTable("USDeclarationsWithMainData");
					dt.Load(reader);
					AssertEquals(1, dt.Select($"JE_PK = '{declaration1PK}' AND JE_ENSAction = 'Incomplete' AND JE_CRLAction = 'Complete'").Length);
					AssertEquals(1, dt.Select($"JE_PK = '{declaration2PK}' AND JE_ENSAction = '' AND JE_CRLAction = ''").Length);
				}
			}
		}

		public void TestENSEntryPK()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, "FTZ", dataModel: "US");
			var dec1Entry2PK = TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, "ENS", dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "ENS", dataModel: "US");
			TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "FTZ", dataModel: "US");

			const string sql = @"select JE_PK, ENSEntryPK from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetGuid(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", dec1Entry2PK, result[declaration1PK]);
						AssertEquals("declarationPK2", dec2Entry1PK, result[declaration2PK]);
					});
				}
			}
		}

		public void TestCRLEntryPK()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, "CRL", "IsDeactivated=Y", dataModel: "US");
			var dec1Entry2PK = TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, "CRL", "IsDeactivated=N", dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "BCR", "IsDeactivated=N", dataModel: "US");
			TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "FTZ", "IsDeactivated=N", dataModel: "US");

			const string sql = @"select JE_PK, CRLEntryPK from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetGuid(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", dec1Entry2PK, result[declaration1PK]);
						AssertEquals("declarationPK2", dec2Entry1PK, result[declaration2PK]);
					});
				}
			}
		}

		public void TestFTZEntryPK()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, "FTZ", dataModel: "US");
			TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, "ENS", dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "ENS", dataModel: "US");
			var dec2Entry2PK = TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "FTZ", dataModel: "US");

			const string sql = @"select JE_PK, FTZEntryPK from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetGuid(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", dec1Entry1PK, result[declaration1PK]);
						AssertEquals("declarationPK2", dec2Entry2PK, result[declaration2PK]);
					});
				}
			}
		}

		public void TestReconJobNumbers()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, "ENS", dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "REC", 2, dataModel: "US");
			TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "RCI", primaryEntry: dec1Entry1PK, dataModel: "US");

			var declaration3PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "REC", 3, dataModel: "US");
			TestDataCreator.CreateCusEntryHeader(declaration3PK, 3, "RCI", primaryEntry: dec1Entry1PK, dataModel: "US");

			const string sql = @"select JE_PK, ReconJobNumbers from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					AssertEquals("declarationPK1", "B002, B003", result[declaration1PK]);
				}
			}
		}

		public void TestInvoiceDecFlag()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1, addInfo: "FirstSale=Y", dataModel: "US");
			var dec1InvoiceLine1PK = TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, dataModel: "US");
			TestDataCreator.CreateCusCodeData("CWO", "", "DD", dec1InvoiceLine1PK, "JI");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, addInfo: "FirstSale=N", dataModel: "US");

			const string sql = @"select JE_PK, CWO, FirstSale from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (string CWO, string FirstSale)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], (reader["CWO"] == DBNull.Value ? null : reader.GetString(1), reader.GetString(2)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", ("Y", "Y"), result[declaration1PK]);
						AssertEquals("declarationPK2", ((string)null, "N"), result[declaration2PK]);
					});
				}
			}
		}

		public void TestMilestones()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			TestDataCreator.CreateProcessTasks(1, declaration1PK, "JE", "MIL", new DateTime(2021, 10, 1, 11, 11, 00), "Desc1", false);
			TestDataCreator.CreateProcessTasks(2, declaration1PK, "JE", "MIL", new DateTime(2021, 10, 1, 11, 12, 00), "Desc2", true);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			TestDataCreator.CreateProcessTasks(1, declaration2PK, "JE", "MIL", new DateTime(2021, 10, 1, 11, 12, 00), "Desc3", true);
			TestDataCreator.CreateProcessTasks(2, declaration2PK, "JE", "MIL", new DateTime(2021, 10, 1, 11, 13, 00), "Desc4", false);

			const string sql = @"select JE_PK, UnpublishedActualStartDate1, ActualStartDate1, UnpublishedMilestoneDescription1, MilestoneDescription1 from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (DateTime UnpublishedActualStartDate1, DateTime ActualStartDate1, string UnpublishedMilestoneDescription1, string MilestoneDescription1)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], (reader.GetDateTime(1), reader.GetDateTime(2), reader.GetString(3), reader.GetString(4)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", (new DateTime(2021, 10, 1, 11, 11, 00), new DateTime(2021, 10, 1, 11, 12, 00), "Desc1", "Desc2"), result[declaration1PK]);
						AssertEquals("declarationPK2", (new DateTime(2021, 10, 1, 11, 13, 00), new DateTime(2021, 10, 1, 11, 12, 00), "Desc4", "Desc3"), result[declaration2PK]);
					});
				}
			}
		}

		public void TestLatestCusLiquidation()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			TestDataCreator.CreateCusLiquidation(declaration1PK, companyPK, 1, new DateTime(2021, 10, 1, 11, 11, 00), new DateTime(2021, 10, 1, 11, 11, 00));
			TestDataCreator.CreateCusLiquidation(declaration1PK, companyPK, 1, new DateTime(2021, 10, 1, 11, 12, 00), new DateTime(2021, 10, 1, 11, 12, 00));

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			TestDataCreator.CreateCusLiquidation(declaration2PK, companyPK, 2, new DateTime(2021, 10, 1, 11, 13, 00), new DateTime(2021, 10, 1, 11, 13, 00));
			TestDataCreator.CreateCusLiquidation(declaration2PK, companyPK, 2, new DateTime(2021, 10, 1, 11, 14, 00), new DateTime(2021, 10, 1, 11, 14, 00));

			const string sql = @"select JE_PK, LiquidationDate from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, DateTime>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetDateTime(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", new DateTime(2021, 10, 1, 11, 12, 00), result[declaration1PK]);
						AssertEquals("declarationPK2", new DateTime(2021, 10, 1, 11, 14, 00), result[declaration2PK]);
					});
				}
			}
		}

		public void TestMasterBills()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declaration1Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK1, 1, billNum: "B1", billType: "HB");
			var declaration1Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK1, 1, billNum: "B2", billType: "MB");
			TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, billNum: "B3", billType: "HB");
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration1Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration1Bill2);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var declaration2Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B4", billType: "MB");
			var declaration2Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B5", billType: "HB");
			var declaration2Bill3 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B6", billType: "SH");
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill2);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill3);

			const string sql = @"select JE_PK, MasterBills from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", "APLUB2", result[declarationPK1]);
						AssertEquals("declarationPK2", "APLUB4", result[declarationPK2]);
					});
				}
			}
		}

		public void TestHouseBills()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declaration1Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK1, 1, billNum: "B1", billType: "HB");
			var declaration1Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK1, 1, billNum: "B2", billType: "MB");
			TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, billNum: "B3", billType: "HB");
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration1Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration1Bill2);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var declaration2Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B4", billType: "MB");
			var declaration2Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B5", billType: "HB");
			var declaration2Bill3 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B6", billType: "SH");
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill2);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill3);

			const string sql = @"select JE_PK, HouseBills from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", "APLUB1, B3", result[declarationPK1]);
						AssertEquals("declarationPK2", "APLUB5", result[declarationPK2]);
					});
				}
			}
		}

		public void TestSubHouseBills()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declaration1Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK1, 1, billNum: "B1", billType: "HB");
			var declaration1Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK1, 1, billNum: "B2", billType: "MB");
			TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, billNum: "B3", billType: "HB");
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration1Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration1Bill2);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var declaration2Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B4", billType: "MB");
			var declaration2Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B5", billType: "HB");
			var declaration2Bill3 = TestDataCreator.CreateCusDecHouseBill(true, "UI_NKBillIssuerSCAC=APLU", declarationPK2, 2, billNum: "B6", billType: "SH");
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill2);
			TestDataCreator.CreateCusAddInfo("UDP", "", "CU", declaration2Bill3);

			const string sql = @"select JE_PK, SubHouseBills from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", string.Empty, result[declarationPK1]);
						AssertEquals("declarationPK2", "APLUB6", result[declarationPK2]);
					});
				}
			}
		}

		public void TestITNumbers()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declaration1Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1);
			var declaration1Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1);
			TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK1, 1, billNum: "B3", billType: "HB");
			TestDataCreator.CreateCusAddInfo("ITN", "ITNumber=111", "CU", declaration1Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "ITNumber=222", "CU", declaration1Bill2);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");

			const string sql = @"select JE_PK, ITNumbers from dbo.USDeclarationsWithMainData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", "111", result[declarationPK1]);
						AssertEquals("declarationPK2", string.Empty, result[declarationPK2]);
					});
				}
			}
		}

		public void TestEntryDateGenAddOnColumn()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "US");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IMP", 2, dataModel: "US");

			TestDataCreator.CreateCusEntryNum(declarationPK1, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK2, "JobDeclaration", "EntryNum2", "ENS", "CUS", "US");
			AssertDeclarationReferences("No declaration should be returned in result", companyPK, "2021-01-01", "2021-01-15", Array.Empty<string>());

			UpdateDeclarationAdditionalInfo(declarationPK1, "EntryDate=2020-01-10");
			AssertDeclarationReferences("No declaration should be returned in result", companyPK, "2021-01-01", "2021-01-15", Array.Empty<string>());

			UpdateDeclarationAdditionalInfo(declarationPK1, "EntryDate=2021-01-10");
			AssertDeclarationReferences("Only B0001 should be returned in result", companyPK, "2021-01-01", "2021-01-15", new string[] { "B0001" });

			UpdateDeclarationAdditionalInfo(declarationPK2, "EntryDate=2021-02-01");
			AssertDeclarationReferences("Only B0001 should be returned in result", companyPK, "2021-01-01", "2021-01-15", new string[] { "B0001" });
			AssertDeclarationReferences("Both B0001 and B0002 should be returned in result", companyPK, "", "", new string[] { "B0001", "B0002" });

			UpdateDeclarationAdditionalInfo(declarationPK2, "EntryDate=2021-01-14");
			AssertDeclarationReferences("Both B0001 and B0002 should be returned in result", companyPK, "", "", new string[] { "B0001", "B0002" });
		}

		void UpdateDeclarationAdditionalInfo(Guid declarationPK, string additionalInfo)
		{
			var updateSQL = @"
UPDATE dbo.JobDeclaration
SET
	JE_AddInfo = @additionalInfo,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK";
			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@additionalInfo", SqlDbType.VarChar, additionalInfo);
				command.ExecuteNonQuery();
			}
		}

		static void AssertDeclarationReferences(string message, Guid companyPK, string entryDateFrom, string entryDateTo, params string[] expectedDeclarationReferences)
		{
			var declarationQuery = $"SELECT JE_DeclarationReference FROM dbo.USDeclarationsWithMainData('{companyPK}', NULL, NULL, NULL, NULL, NULL, NULL, '{entryDateFrom}', '{entryDateTo}', NULL, NULL)";
			var actualDeclarationReferences = new List<string>();
			using (var command = Db.Connection.Command(declarationQuery))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						actualDeclarationReferences.Add(reader.GetString(0));
					}
				}
			}

			AssertContainsExactElementsInAnyOrder(message, expectedDeclarationReferences, actualDeclarationReferences);
		}

		public void TestStatementMonthlyNumber()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var declarationPK = Guid.NewGuid();

			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, Je_addinfo, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, 'BJOB1', 'ACE', 0, 'EntryFilerCode=SV9', 1)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");

			var monthlyStatemnetPK = Guid.NewGuid();
			var monthlySql = @"insert into dbo.CusStatementHeader (B2_PK, B2_GC, B2_B2_PeriodicStatement, B2_IsMonthlyStatement, B2_EntryFilerCode) values
	(@PK, @companyPK, null, 1, 'SV9')";
			using (var command = Db.Connection.Command(monthlySql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, monthlyStatemnetPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			var dailyStatemnetPK = Guid.NewGuid();
			var dailySql = @"insert into dbo.CusStatementHeader (B2_PK, B2_GC, B2_B2_PeriodicStatement, B2_IsMonthlyStatement, B2_EntryFilerCode) values
	(@PK, @companyPK, @parentPK, 1, 'SV9')";
			using (var command = Db.Connection.Command(dailySql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, dailyStatemnetPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, monthlyStatemnetPK);
				command.ExecuteNonQuery();
			}

			var statementLinePK = Guid.NewGuid();
			var statementLinePKSql = @"insert into dbo.CusStatementLine (B3_PK, B3_B2, B3_EntryNum, B3_EntryFilerCode) values(@PK, @dailyPK, 'EntryNum1', 'SV9')";
			using (var command = Db.Connection.Command(statementLinePKSql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, statementLinePK);
				command.AddParameter("@dailyPK", SqlDbType.UniqueIdentifier, dailyStatemnetPK);
				command.ExecuteNonQuery();
			}

			var reportSql = @"select * from USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null,null, null, null, null, null, null, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				var statementPK = string.Empty;

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						statementPK = reader["PMSStatementPK"].ToString();
					}
				}
				AssertEquals(monthlyStatemnetPK.ToString(), statementPK);
			}
		}

		public void TestAuditData()
		{
			AssertNoExceptionThrown(() =>
			{
				var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
				var reportSql = $@"SELECT JE_AuditDateUtc, JE_AuditReference, JE_GS_NKAuditUser FROM dbo.USDeclarationsWithMainData('{companyPK}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";

				using (var command = Db.Connection.Command(reportSql))
				{
					command.ExecuteNonQuery();
				}
			});
		}

		public void TestSystemCreateTimeUtc()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var utcNow = DateTime.UtcNow;

			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 1, createTime: utcNow, dataModel: "US");
			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 2, createTime: utcNow.AddMinutes(2), dataModel: "US");
			var declarationPk3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 3, createTime: utcNow.AddMinutes(-2), dataModel: "US");

			var reportSql = @"SELECT JE_PK FROM USDeclarationsWithMainData(@companyPK, null, null, @dateFrom, @dateTo, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<Guid>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.SmallDateTime, utcNow);
				command.AddParameter("@dateTo", SqlDbType.SmallDateTime, utcNow.AddMinutes(1));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((Guid)reader["JE_PK"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals(declarationPk1, retList[0]);
			}
		}

		public void TestExportDate()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var now = DateTime.Now;

			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "IMP", 1, dataModel: "US", addInfo: $"DateOfExport={now.AddMinutes(1):yyyy-MM-dd HH:mm:ss}");
			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "IMP", 2, dataModel: "US", addInfo: $"DateOfExport={now.AddMinutes(-3):yyyy-MM-dd HH:mm:ss}");
			var declarationPk3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "IMP", 3, dataModel: "US", addInfo: $"DateOfExport={now.AddMinutes(3):yyyy-MM-dd HH:mm:ss}");

			var reportSql = @"SELECT JE_PK FROM USDeclarationsWithMainData(@companyPK, @exportDateFrom, @exportDateTo, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<Guid>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@exportDateFrom", SqlDbType.SmallDateTime, now);
				command.AddParameter("@exportDateTo", SqlDbType.SmallDateTime, now.AddMinutes(2));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((Guid)reader["JE_PK"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals(declarationPk1, retList[0]);
			}
		}

		public void TestImportDate()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var now = DateTime.Now;

			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "IMP", "SEA", "Calypso", "0308", now.AddMinutes(1), 1, dataModel: "US");
			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "IMP", "SEA", "Calypso", "0308", now.AddMinutes(-3), 2, dataModel: "US");
			var declarationPk3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "IMP", "SEA", "Calypso", "0308", now.AddMinutes(3), 3, dataModel: "US");

			var reportSql = @"SELECT JE_PK FROM USDeclarationsWithMainData(@companyPK, null, null, null, null, @importDateFrom, @importDateTo, null, null, null, null)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<Guid>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importDateFrom", SqlDbType.SmallDateTime, now);
				command.AddParameter("@importDateTo", SqlDbType.SmallDateTime, now.AddMinutes(2));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((Guid)reader["JE_PK"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals(declarationPk1, retList[0]);
			}
		}

		public void TestEntryDate()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var now = DateTime.Now;

			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "IMP", 1, addInfo: $"EntryDate={now.AddMinutes(1):yyyy-MM-dd HH:mm:ss}", dataModel: "US");
			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "IMP", 2, addInfo: $"EntryDate={now.AddMinutes(-3):yyyy-MM-dd HH:mm:ss}", dataModel: "US");
			var declarationPk3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "IMP", 3, addInfo: $"EntryDate={now.AddMinutes(3):yyyy-MM-dd HH:mm:ss}", dataModel: "US");

			var reportSql = @"SELECT JE_PK FROM USDeclarationsWithMainData(@companyPK, null, null, null, null, null, null, @entryDateFrom, @entryDateTo, null, null)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<Guid>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryDateFrom", SqlDbType.SmallDateTime, now);
				command.AddParameter("@entryDateTo", SqlDbType.SmallDateTime, now.AddMinutes(2));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((Guid)reader["JE_PK"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals(declarationPk1, retList[0]);
			}
		}
	}
}
