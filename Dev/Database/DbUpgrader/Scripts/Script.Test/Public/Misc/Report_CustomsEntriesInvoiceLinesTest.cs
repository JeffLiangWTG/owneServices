using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(Report_CustomsEntriesInvoiceLines))]
	class Report_CustomsEntriesInvoiceLinesTest : DbCreateScriptTest
	{
		public void TestSmoke()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var sql = @"SELECT * FROM Report_CustomsEntriesInvoiceLines(@companyPK, @from, @to) OPTION (RECOMPILE)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@from", SqlDbType.SmallDateTime, DateTime.Today.AddDays(-15));
				command.AddParameter("@to", SqlDbType.SmallDateTime, DateTime.Today);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						// we are not really interested in results
					}
				}
			}

			Assert("Just want to make sure that compilation does not fail when all columns are selected.", true);
		}

		public void TestReport_CustomsEntriesInvoiceLines_AUEXP()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var importPK = TestDataCreator.CreateOrganisation("IMP", "IMPORTER");

			var branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'AUSYD', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
			var declaration1PK = CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", importPK, 1);
			var invoiceGroup1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, true, 1);
			var invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1);
			var invoiceLine1PK = CreateJobComInvoiceLine(invoice1PK, 1, 100m, 1);
			var cusEntryLine1PK = AddCusEntryHeaderAndLine(declaration1PK, new Guid[] { invoiceLine1PK }, "ENT1", 1, "AU", 90m);

			var declaration2PK = CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", Guid.Empty, 2);
			AddCusEntryNum(declaration2PK, "JobDeclaration", "ENT2", "AU", "CAN");
			var invoiceGroup2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, true, 2);
			var invoice2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2);
			var invoiceLine2PK = CreateJobComInvoiceLine(invoice2PK, 2, 200m, 2);
			var invoiceLine3PK = CreateJobComInvoiceLine(invoice2PK, 3, 300m, 2);

			var declaration3PK = CreateJobDeclaration(branchPK, companyPK, "B00003", "EXP", importPK, 3);
			TestDataCreator.CreateJobComInvoiceHeader(declaration3PK, true, 3);
			var invoice3PK = TestDataCreator.CreateJobComInvoiceHeader(declaration3PK, false, 3);
			var invoiceLine4PK = CreateJobComInvoiceLine(invoice3PK, 4, 400m, 3);
			AddCusEntryHeaderAndLine(declaration3PK, new Guid[] { invoiceLine4PK }, "ENT3", 3, "AU", 0m, "CAN");

			var reportSql = @"SELECT JobComInvoiceLine_PK, CustomsValue, LinePrice, EntryNum FROM Report_CustomsEntriesInvoiceLines(@companyPK, null, null) ORDER BY InvoiceLine";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("The first line", reader.Read());
					AssertEquals(invoiceLine1PK, reader.GetGuid(0));
					AssertEquals("CustomsValue", 90m, reader.GetDecimal(1));
					AssertEquals("LinePrice", 100m, reader.GetDecimal(2));
					AssertEquals("EntryNum", "ENT1", reader.GetString(3));
					Assert("The second line", reader.Read());
					AssertEquals(invoiceLine2PK, reader.GetGuid(0));
					AssertEquals("CustomsValue", 0m, reader.GetDecimal(1));
					AssertEquals("LinePrice", 200m, reader.GetDecimal(2));
					AssertEquals("EntryNum", "ENT2", reader.GetString(3));
					Assert("The third line", reader.Read());
					AssertEquals(invoiceLine3PK, reader.GetGuid(0));
					AssertEquals("CustomsValue", 0m, reader.GetDecimal(1));
					AssertEquals("LinePrice", 300m, reader.GetDecimal(2));
					AssertEquals("EntryNum", "ENT2", reader.GetString(3));
					Assert("The fourth line", reader.Read());
					AssertEquals(invoiceLine4PK, reader.GetGuid(0));
					AssertEquals("CustomsValue", 0m, reader.GetDecimal(1));
					AssertEquals("LinePrice", 400m, reader.GetDecimal(2));
					AssertEquals("EntryNum", "ENT3", reader.GetString(3));
				}
			}
		}

		public void TestReport_CustomsEntriesInvoiceLines()
		{
			TestDataCreator.CreateTariffDataForTesting();

			var companyPK = TestDataCreator.CreateCompany("TC1", "ZA", "ZAR");
			var importPK = TestDataCreator.CreateOrganisation("IMP", "IMPORTER");

			var branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'ZAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
			var declaration1PK = CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", importPK, 1);
			var invoiceGroup1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, true, 1);
			var invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1);
			var invoiceLine1PK = CreateJobComInvoiceLine(invoice1PK, 1, 100m, 1);
			var cusEntryLine1PK = AddCusEntryHeaderAndLine(declaration1PK, new Guid[] { invoiceLine1PK }, "MRN1", 1);
			AddCusEntryLineFee(cusEntryLine1PK, "DTY", 100m, true, 1);
			AddCusEntryLineFee(cusEntryLine1PK, "DTY", 200m, false, 1);

			var declaration2PK = CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", importPK, 2);
			var invoiceGroup2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, true, 2);
			var invoice2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2);
			var invoiceLine2_1PK = CreateJobComInvoiceLine(invoice2PK, 2, 100m, 2);
			var invoiceLine2_2PK = CreateJobComInvoiceLine(invoice2PK, 3, 100m, 2);
			var cusEntryLine2PK = AddCusEntryHeaderAndLine(declaration2PK, new Guid[] { invoiceLine2_1PK, invoiceLine2_2PK }, "MRN2", 2);
			AddCusEntryLineFee(cusEntryLine2PK, "1P1", 100m, true, 2);
			AddCusEntryLineFee(cusEntryLine2PK, "DTY", 200m, false, 2);
			AddCusEntryLineFee(cusEntryLine2PK, "12B", 300m, false, 2);
			AddCusEntryLineFee(cusEntryLine2PK, "13B", 400m, false, 2);

			var declaration3PK = CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", importPK, 3);
			var invoiceGroup3PK = TestDataCreator.CreateJobComInvoiceHeader(declaration3PK, true, 3);
			var invoice3PK = TestDataCreator.CreateJobComInvoiceHeader(declaration3PK, false, 3);
			var invoiceLine3PK = CreateJobComInvoiceLine(invoice3PK, 4, 100m, 3);
			var cusEntryLine3PK = AddCusEntryHeaderAndLine(declaration3PK, new Guid[] { invoiceLine3PK }, "MRN3", 3);
			AddCusEntryLineFee(cusEntryLine3PK, "A00", 100m, false, 3);
			AddCusEntryLineFee(cusEntryLine3PK, "A20", 50m, false, 3);
			AddCusEntryLineFee(cusEntryLine3PK, "990", 25m, false, 3);

			var declaration4PK = CreateJobDeclaration(branchPK, companyPK, "B00004", "IMP", importPK, 4);
			var invoiceGroup4PK = TestDataCreator.CreateJobComInvoiceHeader(declaration4PK, true, 4);
			var invoice4PK = TestDataCreator.CreateJobComInvoiceHeader(declaration4PK, false, 4);
			var invoiceLine4PK = CreateJobComInvoiceLine(invoice4PK, 4, 100m, 4);
			var cusEntryLine4PK = AddCusEntryHeaderAndLine(declaration4PK, new Guid[] { invoiceLine4PK }, "MRN4", 4);
			AddCusEntryLineFee(cusEntryLine4PK, "A00", 100m, true, 4);
			AddCusEntryLineFee(cusEntryLine4PK, "A20", 50m, true, 4);
			AddCusEntryLineFee(cusEntryLine4PK, "990", 25m, true, 4);

			var declaration5PK = CreateJobDeclaration(branchPK, companyPK, "B00005", "IMP", importPK, 5);
			var invoiceGroup5PK = TestDataCreator.CreateJobComInvoiceHeader(declaration5PK, true, 5);
			var invoice5PK = TestDataCreator.CreateJobComInvoiceHeader(declaration5PK, false, 5);
			var invoiceLine5PK = CreateJobComInvoiceLine(invoice5PK, 4, 100m, 5);
			var cusEntryLine5PK = AddCusEntryHeaderAndLine(declaration5PK, new Guid[] { invoiceLine5PK }, "MRN5", 5);
			AddCusEntryLineFee(cusEntryLine5PK, "3P1", 100m, false, 5);
			AddCusEntryLineFee(cusEntryLine5PK, "12B", 200m, false, 5);
			AddCusEntryLineFee(cusEntryLine5PK, "1P1", 400m, false, 5);

			var reportSql = @"SELECT JobComInvoiceLine_PK, Duty, OtherDuties FROM Report_CustomsEntriesInvoiceLines(@companyPK, null, null) ORDER BY InvoiceLine, JobNumber";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertDutiesExist(reader, 1, invoiceLine1PK, 200m, 0m);
						AssertDutiesExist(reader, 2.1, invoiceLine2_1PK, 100m, 350m);
						AssertDutiesExist(reader, 2.2, invoiceLine2_2PK, 100m, 350m);
						AssertDutiesExist(reader, 3, invoiceLine3PK, 150m, 25m);
						AssertDutiesExist(reader, 4, invoiceLine4PK, 0m, 0m);
						AssertDutiesExist(reader, 5, invoiceLine5PK, 400m, 200m);

						AssertEquals("Should be no rows left to test", false, reader.Read());
					});
				}
			}
		}

		void AssertDutiesExist(IDataReader reader, double index, Guid invoiceLinePK, decimal dutiesValue, decimal otherDutiesValue)
		{
			Assert(string.Format("Line {0} should exist.", index), reader.Read());
			AssertEquals(invoiceLinePK, reader.GetGuid(0));
			AssertEquals(string.Format("{0}. Duty", index), dutiesValue, reader.GetDecimal(1));
			AssertEquals(string.Format("{0}. Other Duties", index), otherDutiesValue, reader.GetDecimal(2));
		}

		public void TestReport_CustomsEntriesInvoiceLinesWithConfirmedFees()
		{
			TestDataCreator.CreateTariffDataForTesting();

			var companyPK = TestDataCreator.CreateCompany("TC1", "ZA", "ZAR");
			var importPK = TestDataCreator.CreateOrganisation("IMP", "IMPORTER");

			var branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'ZAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", importPK, 1);
			var invoiceGroupPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, true, 1);
			var invoicePK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1);
			var invoiceLinePK = CreateJobComInvoiceLine(invoicePK, 4, 100m, 1);
			var cusEntryLinePK = AddCusEntryHeaderAndLine(declarationPK, new Guid[] { invoiceLinePK }, "MRN1", 1);
			AddCusEntryLineFee(cusEntryLinePK, "A00", 100m, false, 1);
			AddCusEntryLineFee(cusEntryLinePK, "A20", 50m, false, 1);
			AddCusEntryLineFee(cusEntryLinePK, "990", 25m, false, 1);
			AddCusEntryLineConfirmedFee(cusEntryLinePK, "A00", 99.99m, false, 1);
			AddCusEntryLineConfirmedFee(cusEntryLinePK, "A20", 49.99m, false, 1);
			AddCusEntryLineConfirmedFee(cusEntryLinePK, "990", 24.99m, false, 1);

			var reportSql = @"SELECT JobComInvoiceLine_PK, Duty, OtherDuties FROM Report_CustomsEntriesInvoiceLines(@companyPK, null, null) ORDER BY InvoiceLine, JobNumber";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertDutiesExist(reader, 1, invoiceLinePK, 149.98m, 24.99m);
					});
				}
			}
		}

		public void TestReport_CustomsEntriesInvoiceLinesWithMultipleMRNs()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "ZA", "ZAR");
			var importPK = TestDataCreator.CreateOrganisation("IMP", "IMPORTER");

			var branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'ZAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
			var declaration1PK = CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", importPK, 1);
			var invoiceGroup1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, true, 1);
			var invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1);
			var invoiceLine1PK = CreateJobComInvoiceLine(invoice1PK, 1, 100m, 1);
			var invoiceLine2PK = CreateJobComInvoiceLine(invoice1PK, 2, 100m, 2);
			var cusEntryLinePKs = AddCusEntryHeaderAndLineForEachInvoiceLine(declaration1PK, new Guid[] { invoiceLine1PK, invoiceLine2PK }, "MRN1", 1, customsValue: 1);

			var invoiceGroup2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, true, 2);
			var invoice2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 2);
			var invoiceLine3PK = CreateJobComInvoiceLine(invoice2PK, 3, 100m, 3);
			var invoiceLine4PK = CreateJobComInvoiceLine(invoice2PK, 4, 100m, 4);
			var cusEntryLine2PKs = AddCusEntryHeaderAndLineForEachInvoiceLine(declaration1PK, new Guid[] { invoiceLine3PK, invoiceLine4PK }, "MRN2", 3, customsValue: 3);

			var reportSql = @"SELECT JobComInvoiceLine_PK, EntryNum, CustomsValue FROM Report_CustomsEntriesInvoiceLines(@companyPK, null, null) ORDER BY InvoiceLine";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertCorrectMRNAndCustomsValue(reader,"1.1", invoiceLine1PK, 1m, "MRN1");
						AssertCorrectMRNAndCustomsValue(reader, "1.2", invoiceLine2PK, 2m, "MRN1");
						AssertCorrectMRNAndCustomsValue(reader, "2.1", invoiceLine3PK, 3m, "MRN2");
						AssertCorrectMRNAndCustomsValue(reader, "2.2", invoiceLine4PK, 4m, "MRN2");
						AssertEquals("Should be no rows left to test", false, reader.Read());
					});
				}
			}

			void AssertCorrectMRNAndCustomsValue(IDataReader reader, string index, Guid invoiceLinePK, decimal customsValue, string mrn)
			{
				Assert(string.Format("Line {0} should exist.", index), reader.Read());
				AssertEquals(invoiceLinePK, reader.GetGuid(0));
				AssertEquals(string.Format("{0}. Entry Num", index), mrn, reader.GetString(1));
				AssertEquals(string.Format("{0}. Customs Value", index), customsValue, reader.GetDecimal(2));
			}
		}

		Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string declarationReference, string messageType, Guid importerPK, int clusterKey)
		{
			var declarationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ClusterKey)
VALUES (@declarationPK, '!!', @messageType, @branchPK, @companyPK, @importerPK, @declarationReference, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationReference);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				if (importerPK == Guid.Empty)
				{
					command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				}
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		Guid CreateJobComInvoiceLine(Guid jobComInvoiceHeaderPK, short lineNo, decimal linePrice, int clusterKey)
		{
			var jobComInvoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_ClusterKey)
VALUES(@jobComInvoiceLinePK, '!!', @jobComInvoiceHeaderPK, @lineNo, @linePrice, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobComInvoiceLinePK", SqlDbType.UniqueIdentifier, jobComInvoiceLinePK);
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@lineNo", SqlDbType.SmallInt, lineNo);
				command.AddParameter("@linePrice", SqlDbType.Decimal, linePrice);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return jobComInvoiceLinePK;
		}

		Guid AddCusEntryHeaderAndLine(Guid declarationPK, Guid[] invoiceLinePKs, string entryNum, int clusterKey, string countryCode = "ZA", decimal customsValue = 1m, string entryType = "MRN")
		{
			var cusEntryHeaderPK = Guid.NewGuid();
			var cusEntryLinePK = Guid.NewGuid();

			var sql = string.Format(@"INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
				VALUES (@cusEntryHeaderPK, '!!', @declarationPK, 'IMP', @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
			INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_LineNumber, CL_CustomsValue, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
				VALUES (@cusEntryLinePK, '!!', @cusEntryHeaderPK, 1, {2}, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
			INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_Category, CE_EntryType, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) 
				VALUES (NEWID(), @cusEntryHeaderPK, 'CusEntryHeader', @entryNum, 'CUS', '{3}', '{1}', getutcdate(), '~BP', getutcdate(), '~BP')
			UPDATE dbo.JobComInvoiceLine
			SET
				JI_CL = @cusEntryLinePK,
				JI_SystemLastEditTimeUtc = GETUTCDATE(),
				JI_SystemLastEditUser = '~BP'
			WHERE
				JI_PK IN ('{0}')", string.Join("','", invoiceLinePKs), countryCode, customsValue, entryType);

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@cusEntryHeaderPK", SqlDbType.UniqueIdentifier, cusEntryHeaderPK);
				command.AddParameter("@cusEntryLinePK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@entryNum", SqlDbType.VarChar, entryNum);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			return cusEntryLinePK;
		}

		Guid[] AddCusEntryHeaderAndLineForEachInvoiceLine(Guid declarationPK, Guid[] invoiceLinePKs, string entryNum, int clusterKey, string countryCode = "ZA", decimal customsValue = 1m, string entryType = "MRN")
		{
			var cusEntryHeaderPK = Guid.NewGuid();
			var cusEntryLinePKs = new List<Guid>();

			var sql = string.Format(@"INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
				VALUES (@cusEntryHeaderPK, '!!', @declarationPK, 'IMP', @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
			INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_Category, CE_EntryType, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) 
				VALUES (NEWID(), @cusEntryHeaderPK, 'CusEntryHeader', @entryNum, 'CUS', '{0}', '{1}', getutcdate(), '~BP', getutcdate(), '~BP')", entryType, countryCode);

			var lineNo = 1;
			foreach (var invoiceLinePK in invoiceLinePKs)
			{
				var cusEntryLinePK = Guid.NewGuid();
				cusEntryLinePKs.Add(cusEntryLinePK);
				sql += string.Format(@" INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_LineNumber, CL_CustomsValue, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
				VALUES ('{3}', '!!', @cusEntryHeaderPK, {4}, {2}, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')	
			UPDATE dbo.JobComInvoiceLine
			SET
				JI_CL = '{3}',
				JI_SystemLastEditTimeUtc = GETUTCDATE(),
				JI_SystemLastEditUser = '~BP'
			WHERE
				JI_PK = '{0}' ", invoiceLinePK, countryCode, customsValue++, cusEntryLinePK, lineNo++);
			}

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@cusEntryHeaderPK", SqlDbType.UniqueIdentifier, cusEntryHeaderPK);
				command.AddParameter("@entryNum", SqlDbType.VarChar, entryNum);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			return cusEntryLinePKs.ToArray();
		}

		void AddCusEntryNum(Guid parentPK, string parentTable, string entryNum, string countryCode, string entryType)
		{
			var sql = @"INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_Category, CE_EntryType, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) 
				VALUES (NEWID(), @parentPK, @parentTable, @entryNum, 'CUS', @entryType, @countryCode, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentTable", SqlDbType.VarChar, parentTable);
				command.AddParameter("@entryType", SqlDbType.VarChar, entryType);
				command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				command.AddParameter("@entryNum", SqlDbType.VarChar, entryNum);

				command.ExecuteNonQuery();
			}
		}

		void AddCusEntryLineFee(Guid cusEntryLinePK, string chargeType, decimal chargeAmount, bool isLandedCostOnly, int clusterKey)
		{
			var sql = @"
			INSERT INTO dbo.CusEntryLineFee (CF_PK, CF_ChargeType, CF_ChargeAmount, CF_IsLandedCostOnly, CF_CL, CF_ClusterKey, CF_SystemCreateTimeUtc, CF_SystemCreateUser, CF_SystemLastEditTimeUtc, CF_SystemLastEditUser)
				VALUES (NEWID(), @chargeType, @chargeAmount, @isLandedCostOnly, @cusEntryLinePK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@chargeType", SqlDbType.VarChar, chargeType);
				command.AddParameter("@chargeAmount", SqlDbType.Decimal, chargeAmount);
				command.AddParameter("@cusEntryLinePK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@isLandedCostOnly", SqlDbType.Bit, isLandedCostOnly);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
		}

		void AddCusEntryLineConfirmedFee(Guid cusEntryLinePK, string chargeType, decimal chargeAmount, bool isLandedCostOnly, int clusterKey)
		{
			var sql = @"
			INSERT INTO dbo.CusEntryLineFee (CF_PK, CF_ChargeType, CF_ChargeAmount, CF_IsLandedCostOnly, CF_CL, CF_ClusterKey, CF_SystemCreateTimeUtc, CF_SystemCreateUser, CF_SystemLastEditTimeUtc, CF_SystemLastEditUser, CF_Source)
				VALUES (NEWID(), @chargeType, @chargeAmount, @isLandedCostOnly, @cusEntryLinePK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP', 'CUS')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@chargeType", SqlDbType.VarChar, chargeType);
				command.AddParameter("@chargeAmount", SqlDbType.Decimal, chargeAmount);
				command.AddParameter("@cusEntryLinePK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@isLandedCostOnly", SqlDbType.Bit, isLandedCostOnly);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
		}
	}
}
