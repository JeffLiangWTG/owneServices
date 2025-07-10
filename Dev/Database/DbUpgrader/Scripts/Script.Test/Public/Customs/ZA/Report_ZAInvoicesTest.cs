using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZA
{
	[TestedType(typeof(Report_ZAInvoices))]
	class Report_ZAInvoicesTest : DbCreateScriptTest
	{
		public void TestReport_ZAInvoices()
		{
			TestDataCreator.CreateTariffDataForTesting();

			var companyPK = TestDataCreator.CreateCompany("TC1", "ZA", "ZAR");

			var branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'ZAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
			var supplier1PK = TestDataCreator.CreateOrganisation("ABCCo", "ABBBBC", "AUSYD");
			var supplier2PK = TestDataCreator.CreateOrganisation("XYZCo", "XYYYYZ", "AUMEL");

			const int clusterKey1 = 10000001;
			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", clusterKey1, supplierPK: supplier1PK, dataModel: "ZA");
			var invoiceGroup1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, isGroupInvoice: true, clusterKey1, dataModel: "ZA");
			var invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, isGroupInvoice: false, clusterKey1, supplier2PK, dataModel: "ZA");
			var invoiceLine1PK = CreateJobComInvoiceLine(invoice1PK, "VAT", clusterKey1);
			var cusEntryLine1PK = AddCusEntryHeaderLineAndFees(declaration1PK, invoiceLine1PK, "ITF", clusterKey1, "", "");
			AddCusEntryLineFee(cusEntryLine1PK, "DTY", 100m, isLandedCostOnly: true, clusterKey1);
			AddCusEntryLineFee(cusEntryLine1PK, "DTY", 200m, isLandedCostOnly: false, clusterKey1);
			AddCusEntryLineFee(cusEntryLine1PK, "VAT", 300m, isLandedCostOnly: true, clusterKey1);
			AddCusEntryLineFee(cusEntryLine1PK, "VAT", 400m, isLandedCostOnly: false, clusterKey1);

			const int clusterKey2 = 10000002;
			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", clusterKey2, supplierPK: supplier1PK, dataModel: "ZA");
			var invoiceGroup2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, isGroupInvoice: true, clusterKey2, dataModel: "ZA");
			var invoice2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, isGroupInvoice: false, clusterKey2, dataModel: "ZA");
			var invoiceLine2PK = CreateJobComInvoiceLine(invoice2PK, "VEX", clusterKey2);
			var cusEntryLine2PK = AddCusEntryHeaderLineAndFees(declaration2PK, invoiceLine2PK, "BLT", clusterKey2, "", "", "existingaddinfo=blah*ExchangeRateDate=2016-02-29 00:00:00.000");
			AddCusEntryLineFee(cusEntryLine2PK, "1P1", 100m, isLandedCostOnly: true, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK, "DTY", 200m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK, "12B", 300m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK, "13B", 400m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK, "VEX", 500m, isLandedCostOnly: true, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK, "VEX", 600m, isLandedCostOnly: false, clusterKey2);

			var invoiceGroup2PK2 = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, true, 2, dataModel: "ZA");
			var invoice2PK2 = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, dataModel: "ZA");
			var invoiceLine2PK2 = CreateJobComInvoiceLine(invoice2PK2, "VEX", clusterKey2);
			var cusEntryLine2PK2 = AddCusEntryHeaderLineAndFees(declaration2PK, invoiceLine2PK2, "BLT", clusterKey2, "CCC", "CCCC", "existingaddinfo=blah*ExchangeRateDate=2016-05-29 00:00:00.000");
			AddCusEntryLineFee(cusEntryLine2PK2, "1P1", 100m, isLandedCostOnly: true, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK2, "DTY", 200m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK2, "12B", 300m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK2, "13B", 400m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK2, "VEX", 500m, isLandedCostOnly: true, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK2, "VEX", 600m, isLandedCostOnly: false, clusterKey2);

			var invoiceLine2PK3 = CreateJobComInvoiceLine(invoice2PK2, "VEX", clusterKey2);
			var cusEntryLine2PK3 = AddCusEntryHeaderLineAndFees(declaration2PK, invoiceLine2PK3, "BLT", clusterKey2, "AAA", "AAAA", "existingaddinfo=blah");
			AddCusEntryLineFee(cusEntryLine2PK3, "1P1", 100m, isLandedCostOnly: true, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK3, "DTY", 200m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK3, "12B", 300m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK3, "13B", 400m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK3, "VEX", 500m, isLandedCostOnly: true, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK3, "VEX", 600m, isLandedCostOnly: false, clusterKey2);

			var invoiceLine2PK4 = CreateJobComInvoiceLine(invoice2PK2, "VEX", clusterKey2);
			var cusEntryLine2PK4 = AddCusEntryHeaderLineAndFees(declaration2PK, invoiceLine2PK4, "BLT", clusterKey2, "BBB", "BBBB", "existingaddinfo=blah*ExchangeRateDate=2016-06-29 00:00:00.000");
			AddCusEntryLineFee(cusEntryLine2PK4, "1P1", 100m, isLandedCostOnly: true, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK4, "DTY", 200m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK4, "12B", 300m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK4, "13B", 400m, isLandedCostOnly: false, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK4, "VEX", 500m, isLandedCostOnly: true, clusterKey2);
			AddCusEntryLineFee(cusEntryLine2PK4, "VEX", 600m, isLandedCostOnly: false, clusterKey2);

			var reportSql = "SELECT JobNumber, TotalInvoiceDuty, TotalInvoiceVAT, ExchangeRateDate " +
				"FROM Report_ZAInvoices(@companyPK, @messageType, null, null, null, null, null, null, null) " +
				"ORDER BY ExchangeRateDate";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("B00001", reader.GetString(0));
					AssertEquals("TotalInvoiceDuty", 200m, reader.GetDecimal(1));
					AssertEquals("TotalInvoiceVAT", 700m, reader.GetDecimal(2));
					AssertEquals("ExchangeRateDate is MasterBillIssuedDate", new DateTime(2016, 3, 1), reader.GetDateTime(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, "EXP");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("B00002", reader.GetString(0));
					AssertEquals("TotalInvoiceDuty", 700m, reader.GetDecimal(1));
					AssertEquals("TotalInvoiceVAT", 1100m, reader.GetDecimal(2));
					AssertEquals("ExchangeRateDate is what recorded in database", new DateTime(2016, 2, 29), reader.GetDateTime(3));
					Assert("There should be another record", reader.Read());
					AssertEquals("B00002", reader.GetString(0));
					AssertEquals("TotalInvoiceDuty", 2100m, reader.GetDecimal(1));
					AssertEquals("TotalInvoiceVAT", 3300m, reader.GetDecimal(2));
					AssertEquals("ExchangeRateDate is what recorded in database", new DateTime(2016, 6, 29), reader.GetDateTime(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			reportSql = "SELECT JobNumber, TotalInvoiceDuty, TotalInvoiceVAT, ExchangeRateDate " +
				"FROM Report_ZAInvoices(@companyPK, null, null, null, null, null, @supplierPK, null, null) " +
				"ORDER BY ExchangeRateDate";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplier2PK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("B00001", reader.GetString(0));
					AssertEquals("TotalInvoiceDuty", 200m, reader.GetDecimal(1));
					AssertEquals("TotalInvoiceVAT", 700m, reader.GetDecimal(2));
					AssertEquals("ExchangeRateDate is MasterBillIssuedDate", new DateTime(2016, 3, 1), reader.GetDateTime(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplier1PK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("B00002", reader.GetString(0));
					AssertEquals("TotalInvoiceDuty", 700m, reader.GetDecimal(1));
					AssertEquals("TotalInvoiceVAT", 1100m, reader.GetDecimal(2));
					AssertEquals("ExchangeRateDate is what recorded in database", new DateTime(2016, 2, 29), reader.GetDateTime(3));
					Assert("There should be another record", reader.Read());
					AssertEquals("B00002", reader.GetString(0));
					AssertEquals("TotalInvoiceDuty", 2100m, reader.GetDecimal(1));
					AssertEquals("TotalInvoiceVAT", 3300m, reader.GetDecimal(2));
					AssertEquals("ExchangeRateDate is what recorded in database", new DateTime(2016, 6, 29), reader.GetDateTime(3));
					Assert("There should be no other records", !reader.Read());
				}
			}

			var additionalInfoHelper = new AdditionalInfoHelper(new[]
			{
				new AdditionalInfoConfig("UZ_VDN", "VDN", "VDN123")
			});
			UpdateInvoiceAdditionalInfo(invoice1PK, additionalInfoHelper.AdditionalInfoText);

			reportSql = $"SELECT {additionalInfoHelper.SelectList} FROM Report_ZAInvoices(@companyPK, null, null, null, null, null, @supplierPK, null, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplier2PK);

				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("There should be a record", reader.Read());
						foreach (var config in additionalInfoHelper.Configurations)
						{
							AssertEquals($"Additional info \"{config.ColumnName}\" from ModelView", config.Value, reader[config.ColumnName]);
						}
					});
				}
			}
		}

		Guid CreateJobComInvoiceLine(Guid jobComInvoiceHeaderPK, string taxType, int clusterKey)
		{
			var jobComInvoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ZZF_NKTaxType, JI_ClusterKey)
VALUES(@jobComInvoiceLinePK, 'ZA', @jobComInvoiceHeaderPK, @taxType, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobComInvoiceLinePK", SqlDbType.UniqueIdentifier, jobComInvoiceLinePK);
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@taxType", SqlDbType.VarChar, taxType);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return jobComInvoiceLinePK;
		}

		Guid AddCusEntryHeaderLineAndFees(Guid declarationPK, Guid invoiceLinePK, string applicationCode, int clusterKey, string entryInstructionStyle, string entryInstructionDescription, string entryInstructionAddInfo = "existingaddinfo=blah")
		{
			var cusEntryInstructionPK = Guid.NewGuid();
			var cusEntryHeaderPK = Guid.NewGuid();
			var cusEntryLinePK = Guid.NewGuid();

			var sql = @"
UPDATE dbo.JobDeclaration
SET
	JE_ApplicationCode = @applicationCode,
	JE_AddInfo = 'MasterBillIssuedDate=2016-03-01 00:00:00.000',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK
INSERT INTO dbo.CusEntryInstruction (CEI_AddInfo, CEI_DataModel, CEI_PK, CEI_JE, CEI_Style, CEI_Description, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
	VALUES (@entryInstructionAddInfo, 'ZA', @cusEntryInstructionPK, @declarationPK, @entryInstructionStyle, @entryInstructionDescription, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP');
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_CEI_Instruction, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
	VALUES (@cusEntryHeaderPK, 'ZA', @declarationPK, 'IMP', @cusEntryInstructionPK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_LineNumber, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
	VALUES (@cusEntryLinePK, 'ZA', @cusEntryHeaderPK, 1, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryType, CE_Category, CE_EntryNum, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
	VALUES (NEWID(), @cusEntryHeaderPK, 'CusEntryHeader', 'OTH', 'CUS', 'JSA201603015000503', getutcdate(), '~BP', getutcdate(), '~BP');
UPDATE dbo.JobComInvoiceLine
SET
	JI_CL = @cusEntryLinePK,
	JI_CEI = @cusEntryInstructionPK,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK=@invoiceLinePK";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@applicationCode", SqlDbType.VarChar, applicationCode);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@cusEntryHeaderPK", SqlDbType.UniqueIdentifier, cusEntryHeaderPK);
				command.AddParameter("@cusEntryLinePK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@cusEntryInstructionPK", SqlDbType.UniqueIdentifier, cusEntryInstructionPK);
				command.AddParameter("@entryInstructionAddInfo", SqlDbType.VarChar, entryInstructionAddInfo);
				command.AddParameter("@entryInstructionStyle", SqlDbType.VarChar, entryInstructionStyle);
				command.AddParameter("@entryInstructionDescription", SqlDbType.VarChar, entryInstructionDescription);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			return cusEntryLinePK;
		}

		void AddCusEntryLineFee(Guid cusEntryLinePK, string chargeType, decimal chargeAmount, bool isLandedCostOnly, int clusterKey)
		{
			var sql = @"
			INSERT INTO dbo.CusEntryLineFee (CF_PK, CF_ChargeType, CF_ChargeAmount, CF_IsLandedCostOnly, CF_CL, CF_ClusterKey, CF_SystemCreateTimeUtc, CF_SystemCreateUser, CF_SystemLastEditTimeUtc, CF_SystemLastEditUser) VALUES (newid(), @chargeType, @chargeAmount, @isLandedCostOnly, @cusEntryLinePK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

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

		void UpdateInvoiceAdditionalInfo(Guid invoicePK, string additionalInfo)
		{
			var updateSQL = @"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_AddInfo = @additionalInfo,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = @invoicePK";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@invoicePK", SqlDbType.UniqueIdentifier, invoicePK);
				command.AddParameter("@additionalInfo", SqlDbType.VarChar, additionalInfo);
				command.ExecuteNonQuery();
			}
		}
	}
}

