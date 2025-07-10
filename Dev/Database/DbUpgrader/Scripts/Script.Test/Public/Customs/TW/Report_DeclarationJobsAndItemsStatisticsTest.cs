using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.TW
{
	[TestedType(typeof(Report_DeclarationJobsAndItemsStatistics))]
	class Report_DeclarationJobsAndItemsStatisticsTest : DbCreateScriptTest
	{
		Guid companyPK;
		Guid branchPK;
		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("TC1", "TW", "NTD");
			branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'TAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
		}

		public void TestDeclarationDate()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "SEA", "ABC", "ABC", 3, "C2", 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", 5);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01");

			var reportSql = @"select Staff, C1_20 from Report_DeclarationJobsAndItemsStatistics(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @UserType, @PrintNotCleared)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 12));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 13));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ABC", reader.GetString(0));
					AssertEquals("Count of declarations that has 1 ~ 20 invoice lines", 4, reader.GetInt32(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ABC", reader.GetString(0));
					AssertEquals("Count of declarations that has 1 ~ 20 invoice lines", 6, reader.GetInt32(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2018, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2018, 12, 31));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestShipmentType()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 1, "C1", 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "SEA", "ABC", "ABC", 2, "C1", 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 2, "C1", 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 3, "C1", 5);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01");

			var reportSql = @"select ImportTotal, ExportTotal, TransshipmentTotal, Total from Report_DeclarationJobsAndItemsStatistics(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @UserType, @PrintNotCleared)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ImportTotal", 2, reader.GetInt32(0));
					AssertEquals("ExportTotal", 3, reader.GetInt32(1));
					AssertEquals("TransshipmentTotal", 1, reader.GetInt32(2));
					AssertEquals("Total", 6, reader.GetInt32(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ImportTotal", 2, reader.GetInt32(0));
					AssertEquals("ExportTotal", 0, reader.GetInt32(1));
					AssertEquals("TransshipmentTotal", 1, reader.GetInt32(2));
					AssertEquals("Total", 3, reader.GetInt32(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ImportTotal", 0, reader.GetInt32(0));
					AssertEquals("ExportTotal", 3, reader.GetInt32(1));
					AssertEquals("TransshipmentTotal", 0, reader.GetInt32(2));
					AssertEquals("Total", 3, reader.GetInt32(3));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTransportMode()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 1, "C1", 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "SEA", "ABC", "ABC", 2, "C1", 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 2, "C1", 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "IMP", "AIR", "ABC", "ABC", 3, "C1", 5);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01");
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "04");
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "04");

			var reportSql = @"select ImportTotal, ExportTotal, TransshipmentTotal, Total from Report_DeclarationJobsAndItemsStatistics(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @UserType, @PrintNotCleared)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ImportTotal", 3, reader.GetInt32(0));
					AssertEquals("ExportTotal", 2, reader.GetInt32(1));
					AssertEquals("TransshipmentTotal", 3, reader.GetInt32(2));
					AssertEquals("Total", 8, reader.GetInt32(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ImportTotal", 2, reader.GetInt32(0));
					AssertEquals("ExportTotal", 1, reader.GetInt32(1));
					AssertEquals("TransshipmentTotal", 1, reader.GetInt32(2));
					AssertEquals("Total", 4, reader.GetInt32(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "AIR");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ImportTotal", 1, reader.GetInt32(0));
					AssertEquals("ExportTotal", 1, reader.GetInt32(1));
					AssertEquals("TransshipmentTotal", 2, reader.GetInt32(2));
					AssertEquals("Total", 4, reader.GetInt32(3));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestEntryStatus()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 1, "C2", 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "SEA", "ABC", "ABC", 2, "C3M", 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 2, "C3X", 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "IMP", "AIR", "ABC", "ABC", 3, "AA", 5);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "IMP", "AIR", "ABC", "ABC", 3, "", 6);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01");

			var reportSql = @"select ImportTotal, ExportTotal, TransshipmentTotal, Total from Report_DeclarationJobsAndItemsStatistics(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @UserType, @PrintNotCleared)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ImportTotal", 4, reader.GetInt32(0));
					AssertEquals("ExportTotal", 2, reader.GetInt32(1));
					AssertEquals("TransshipmentTotal", 1, reader.GetInt32(2));
					AssertEquals("Total", 7, reader.GetInt32(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ImportTotal", 2, reader.GetInt32(0));
					AssertEquals("ExportTotal", 2, reader.GetInt32(1));
					AssertEquals("TransshipmentTotal", 1, reader.GetInt32(2));
					AssertEquals("Total", 5, reader.GetInt32(3));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestCountBy()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "BBB", 1, "C1", 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 1, "C2", 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "SEA", "ABC", "BBB", 2, "C3M", 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 2, "C3X", 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "IMP", "AIR", "ABC", "BBB", 3, "AA", 5);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "IMP", "AIR", "ABC", "ABC", 3, "", 6);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "BBB", "01");

			var reportSql = @"select ImportTotal, ExportTotal, TransshipmentTotal, Total from Report_DeclarationJobsAndItemsStatistics(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @UserType, @PrintNotCleared)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "C");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ImportTotal", 4, reader.GetInt32(0));
					AssertEquals("ExportTotal", 2, reader.GetInt32(1));
					AssertEquals("TransshipmentTotal", 1, reader.GetInt32(2));
					AssertEquals("Total", 7, reader.GetInt32(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			reportSql = @"select Staff, ImportTotal, ExportTotal, TransshipmentTotal, Total from Report_DeclarationJobsAndItemsStatistics(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @UserType, @PrintNotCleared) Order By Staff";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@UserType", SqlDbType.VarChar, "L");
				command.AddParameter("@PrintNotCleared", SqlDbType.VarChar, "Y");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ABC", reader.GetString(0));
					AssertEquals("ImportTotal", 1, reader.GetInt32(1));
					AssertEquals("ExportTotal", 2, reader.GetInt32(2));
					AssertEquals("TransshipmentTotal", 0, reader.GetInt32(3));
					AssertEquals("Total", 3, reader.GetInt32(4));

					Assert("There should be another record", reader.Read());
					AssertEquals("BBB", reader.GetString(0));
					AssertEquals("ImportTotal", 3, reader.GetInt32(1));
					AssertEquals("ExportTotal", 0, reader.GetInt32(2));
					AssertEquals("TransshipmentTotal", 1, reader.GetInt32(3));
					AssertEquals("Total", 4, reader.GetInt32(4));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		void CreateCusInBondHeader(DateTime createDate, string createUser, string lastEditUser, string transportType)
		{
			var cusInBondHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInBondHeader (BH_PK, BH_ETA, BH_GB, BH_SystemCreateUser, BH_SystemLastEditUser, BH_ImportTransportMode, BH_ApplicationCode, BH_SystemCreateTimeUtc, BH_SystemLastEditTimeUtc)
VALUES (@cusInBondHeaderPK, @createDate, @branchPK, @createUser, @lastEditUser, @transportType, @applicationCode, GetUtcDate(), GetUtcDate())
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusInBondHeaderPK", SqlDbType.UniqueIdentifier, cusInBondHeaderPK);
				command.AddParameter("@createDate", SqlDbType.SmallDateTime, createDate);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@createUser", SqlDbType.VarChar, CusInBondHeaderSchema.BH_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@lastEditUser", SqlDbType.VarChar, CusInBondHeaderSchema.BH_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@transportType", SqlDbType.VarChar, CusInBondHeaderSchema.BH_ImportTransportMode.MaxLength, transportType);
				command.AddParameter("@applicationCode", SqlDbType.VarChar, CusInBondHeaderSchema.BH_ApplicationCode.MaxLength, "TW");
				command.ExecuteNonQuery();
			}
		}

		void CreateTestJobDeclaration(DateTime createDate, string messageType, string transportMode, string createUser, string lastEditUser, int invoiceLineCount, string entryStatus, int clusterKey)
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, messageType, transportMode, createUser, lastEditUser, clusterKey);
			var entryInstructionPK = CreateCusEntryInstruction(declarationPK, createDate, clusterKey);
			var invoiceHeaderPK = CreateInvoiceHeader(declarationPK, clusterKey);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryHeaderPK, messageType, entryStatus);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, clusterKey);

			for (int i = 0; i < invoiceLineCount; i++)
			{
				CreateInvoiceLine(invoiceHeaderPK, entryInstructionPK, entryLinePK, clusterKey);
			}
		}

		Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string messageType, string transportMode, string createUser, string lastEditUser, int clusterKey, string declarationReference = "")
		{
			var declarationPK = Guid.NewGuid();
			var declarationRef = string.IsNullOrEmpty(declarationReference) ? Guid.NewGuid().ToString("n") : declarationReference;
			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_TransportMode, JE_SystemCreateUser, JE_SystemLastEditUser, JE_ClusterKey, JE_DeclarationReference)
VALUES (@declarationPK, 'TW', @messageType, @branchPK, @companyPK, @transportMode, @createUser, @lastEditUser, @clusterKey, @declarationReference)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, JobDeclarationSchema.JE_MessageType.MaxLength, messageType);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobDeclarationSchema.JE_TransportMode.MaxLength, transportMode);
				command.AddParameter("@createUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@lastEditUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationRef);
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		Guid CreateCusEntryInstruction(Guid declarationPK, DateTime dateForDuty, int clusterKey)
		{
			var entryInstructionPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_DateForDuty, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
VALUES (@entryInstructionPK, 'TW', @declarationPK, @dateForDuty, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@dateForDuty", SqlDbType.SmallDateTime, CusEntryInstructionSchema.CEI_DateForDuty.MaxLength, dateForDuty);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryInstructionPK;
		}

		Guid CreateCusEntryHeader(Guid declarationPK, int clusterKey)
		{
			var entryHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@entryHeaderPK, 'TW', @declarationPK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		void CreateCusEntryNum(Guid entryHeaderPK, string messageType, string entryStatus)
		{
			var sql = @"
INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_Category, CE_EntryType, CE_EntryStatus, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
VALUES (NEWID(), @entryHeaderPK, 'CusEntryHeader', 'CUS', @messageType, @entryStatus, 'TW', getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, CusEntryNumSchema.CE_EntryStatus.MaxLength, entryStatus);
				command.ExecuteNonQuery();
			}
		}

		Guid CreateCusEntryLine(Guid entryHeaderPK, int clusterKey)
		{
			var entryLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@entryLinePK, 'TW', @entryHeaderPK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryLinePK;
		}

		Guid CreateInvoiceHeader(Guid declarationPK, int clusterKey)
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey)
VALUES (@invoiceLinePK, 'TW', @declarationPK, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return invoiceLinePK;
		}

		Guid CreateInvoiceLine(Guid invoiceHeaderPK, Guid entryInstructionPK, Guid entryLinePK, int clusterKey)
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_JZ, JI_CL, JI_CEI, JI_ClusterKey)
VALUES (@invoiceLinePK, 'TW', @invoiceHeaderPK, @entryLinePK, @entryInstructionPK, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return invoiceLinePK;
		}
	}
}

