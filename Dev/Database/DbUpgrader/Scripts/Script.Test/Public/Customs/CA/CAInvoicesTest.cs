using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(CAInvoices))]
	class CAInvoicesTest : DbCreateScriptTest
	{
		public void TestInvoiceVFDAndVFCCs()
		{
			var declaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(declaration1, false, 1, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, addInfo: "CustomsValue=11*CVforCurrConv=12", dataModel: "CA");

			var declaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IMP", 2, dataModel: "CA");
			var invoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(declaration2, false, 2, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 2, addInfo: "CustomsValue=21*CVforCurrConv=22", dataModel: "CA");

			var declaration3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "IMP", 3, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceHeader(declaration3, false, 3, dataModel: "CA");

			var result = new Dictionary<string, (decimal InvoiceVFD, decimal InvoiceVFCC)>();
			Db.Connection.ExecuteReader($@"SELECT JE_DeclarationReference, InvoiceVFD, InvoiceVFCC FROM CAInvoices('{companyPK}') ORDER BY JE_DeclarationReference", reader =>
			{
				result.Add(reader.GetString(0), (reader.GetDecimal(1), reader.GetDecimal(2)));
			});

			CombineAssertions(() =>
			{
				AssertEquals("B0001", (11m, 12m), result["B0001"]);
				AssertEquals("B0002", (21m, 22m), result["B0002"]);
				AssertEquals("B0003", (0m, 0m), result["B0003"]);
			});
		}

		public void TestInvoiceChargeAmountsData()
		{
			var declaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(declaration1, false, 1, originState: "NB", dataModel: "CA");
			Db.Connection.ExecuteNonQuery($@"INSERT dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_RX_NKCurrency, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES
(NEWID(), 'JZ', '{invoiceHeader1}', 1, 'OFT', 120, 'CAD', 10, 0, 1, 0),
(NEWID(), 'JZ', '{invoiceHeader1}', 1, 'ONS', 130, 'CAD', 10, 0, 1, 0),
(NEWID(), 'JZ', '{invoiceHeader1}', 1, 'OFT', 120, 'CNY', 10, 0, 1, 0),
(NEWID(), 'JZ', '{invoiceHeader1}', 1, 'ONS', 130, 'CNY', 10, 0, 1, 0)
");

			var declaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IMP", 2, dataModel: "CA");
			var invoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(declaration2, false, 2, originState: "NB", dataModel: "CA");
			Db.Connection.ExecuteNonQuery($@"INSERT dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_RX_NKCurrency, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES
(NEWID(), 'JZ', '{invoiceHeader2}', 1, 'OFT', 220, 'CAD', 10, 0, 1, 0),
(NEWID(), 'JZ', '{invoiceHeader2}', 1, 'ONS', 230, 'CAD', 10, 0, 1, 0),
(NEWID(), 'JZ', '{invoiceHeader2}', 1, 'OFT', 220, 'CNY', 10, 0, 1, 0),
(NEWID(), 'JZ', '{invoiceHeader2}', 1, 'ONS', 230, 'CNY', 10, 0, 1, 0)
");

			var declaration3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "IMP", 3, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceHeader(declaration3, false, 3, originState: "NB", dataModel: "CA");

			var result = new Dictionary<string, (string InvoiceOFTAmounts, string InvoiceONSAmounts)>();
			Db.Connection.ExecuteReader($@"SELECT JE_DeclarationReference, InvoiceOFTAmounts, InvoiceONSAmounts FROM CAInvoices('{companyPK}') ORDER BY JE_DeclarationReference", reader =>
			{
				result.Add(reader.GetString(0), (reader.GetString(1), reader.GetString(2)));
			});

			CombineAssertions(() =>
			{
				AssertEquals("B0001", ("120.00, 120.00CNY", "130.00, 130.00CNY"), result["B0001"]);
				AssertEquals("B0002", ("220.00, 220.00CNY", "230.00, 230.00CNY"), result["B0002"]);
				AssertEquals("B0003", ("0", "0"), result["B0003"]);
			});
		}

		public void TestInvoiceDutiesAndTaxes()
		{
			var declaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(declaration1, false, 1, originState: "NB", dataModel: "CA");
			var invoiceLine1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, orignState: "MAN", dataModel: "CA", addInfo: "SIMAmount=100");

			var declaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IMP", 2, dataModel: "CA");
			var invoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(declaration2, false, 2, originState: "NB", dataModel: "CA");
			var invoiceLine2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 2, orignState: "MAN", dataModel: "CA", addInfo: "GSTAmount=200");

			var declaration3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "IMP", 3, dataModel: "CA");
			var invoiceHeader3 = TestDataCreator.CreateJobComInvoiceHeader(declaration3, false, 3, originState: "NB", dataModel: "CA");
			var invoiceLine3 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader3, 3, orignState: "MAN", dataModel: "CA", addInfo: "DTYAmount=300");

			var declaration4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0004", "IMP", 4, dataModel: "CA");
			var invoiceHeader4 = TestDataCreator.CreateJobComInvoiceHeader(declaration4, false, 4, originState: "NB", dataModel: "CA");
			var invoiceLine4 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader4, 4, orignState: "MAN", dataModel: "CA", addInfo: "EXSAmount=400");

			var result = new Dictionary<string, (decimal InvoiceDuty, decimal InvoiceExciseTax, decimal InvoiceSIMADuty, decimal InvoiceGST, decimal TotalInvoiceDutiesAndTaxes)>();
			Db.Connection.ExecuteReader($@"SELECT JE_DeclarationReference, InvoiceDuty, InvoiceExciseTax, InvoiceSIMADuty, InvoiceGST, TotalInvoiceDutiesAndTaxes FROM CAInvoices('{companyPK}') ORDER BY JE_DeclarationReference", reader =>
			{
				result.Add(reader.GetString(0), (reader.GetDecimal(1), reader.GetDecimal(2), reader.GetDecimal(3), reader.GetDecimal(4), reader.GetDecimal(5)));
			});

			CombineAssertions(() =>
			{
				AssertEquals("B0001", (0m, 0m, 100m, 0m, 100m), result["B0001"]);
				AssertEquals("B0002", (0m, 0m, 0m, 200m, 200m), result["B0002"]);
				AssertEquals("B0003", (300m, 0m, 0m, 0m, 300m), result["B0003"]);
				AssertEquals("B0004", (0m, 400m, 0m, 0m, 400m), result["B0004"]);
			});
		}

		public void TestConsigneeData()
		{
			var organisationPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var shipmentPK1 = TestDataCreator.CreateShipment("S001");
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, shipmentPK1, organisationPK, dataModel: "CA");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, organisationPK, string.Empty, organisationPK, dataModel: "CA");

			var reportSql = @"SELECT InvoiceConsigneeCode,InvoiceConsigneeFullName FROM CAInvoices(@companyPK)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("OrgCode1", reader.GetString(0));
					AssertEquals("Company Name 1", reader.GetString(1));
				}
			}
		}

		public void TestShipperCodeAndShipperName()
		{
			var organizationPK = TestDataCreator.CreateOrganisation("ORG", "Test Company");
			var addressPK = TestDataCreator.CreateAddress(organizationPK, "ADD1", "Supplier Address 1");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoicePK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "CA");
			var shipperDocAddressPK = TestDataCreator.CreateDocAddress(addressPK, "", invoicePK, "JZ", "SUG");
			var invoiceLinePK = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1, dataModel: "CA");

			var reportSql = @"SELECT InvoiceShipperCode,InvoiceShipperFullName FROM CAInvoices(@companyPK)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ORG", reader.GetString(0));
					AssertEquals("Test Company", reader.GetString(1));
				}
			}
		}

		public void TestExporterCodeAndExporterName()
		{
			var organizationPK = TestDataCreator.CreateOrganisation("ORG", "Test Company");
			var addressPK = TestDataCreator.CreateAddress(organizationPK, "ADD1", "Supplier Address 1");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoicePK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "CA");
			var exporterDocAddressPK = TestDataCreator.CreateDocAddress(addressPK, "", invoicePK, "JZ", "EXP");
			var invoiceLinePK = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1, dataModel: "CA");

			var reportSql = @"SELECT InvoiceExporterCode,InvoiceExporterFullName FROM CAInvoices(@companyPK)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("ORG", reader.GetString(0));
					AssertEquals("Test Company", reader.GetString(1));
				}
			}
		}

		public void TestCarrier()
		{
			TestDataCreator.CreateRefDatabaseRefDataGrouping("CA", "Canada");
			var carrier = TestDataCreator.CreateRefDbEntZZRefCarrier("A000", "A000 TEST CARRIER", "CA", "CARGOCARRIER");
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "LVS", 1, dataModel: "CA");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", addInfo: "CarrierCode=A000", dataModel: "CA");
			var invoiceLine = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, orignState: "MAN", dataModel: "CA");

			var reportSql = @"SELECT InvoiceCarrierCode,InvoiceCarrierName FROM CAInvoices(@companyPK)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("A000", reader.GetString(0));
					AssertEquals("A000 TEST CARRIER", reader.GetString(1));
				}
			}
		}

		public void TestInvoiceNumber()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "LVX", 1, dataModel: "CA");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", addInfo: "CarrierCode=A000", invoiceNumber: "LVSID", dataModel: "CA");
			var invoiceLine = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, orignState: "MAN", dataModel: "CA");

			var reportSql = @"SELECT InvoiceNumber FROM CAInvoices(@companyPK)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("LVSID", reader.GetString(0));
				}
			}
		}

		public void TestCADEntryLineFee()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "LVX", 1, dataModel: "CA");
			var invoiceHeaderWithOneLine = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", addInfo: "CarrierCode=A000", invoiceNumber: "LVSID", dataModel: "CA");
			var cusEntryHeader = TestDataCreator.CreateCusEntryHeader(declaration, 1, "CAD", dataModel: "CA");
			var cusEntryLine = TestDataCreator.CreateCusEntryLine(cusEntryHeader, 1, 6.6m);
			var invoiceLine = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderWithOneLine, 1, orignState: "MAN", entryLinePK: cusEntryLine, dataModel: "CA");
			TestDataCreator.CreateCusUnderBondDec(cusEntryLine, invoiceLine, 1);

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "CUD", 2.2f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "CUD", 2.3f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "CUD", 3.2f, 1, "CW1");

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "ADD", 1.5f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "CVD", 1.6f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "SUR", 1.8f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "ADD", 1.7f, 1, "CW1");

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "GST", 1.2f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "GST", 1.3f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "GST", 3.2f, 1, "CW1");

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "FET", 1.3f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "FET", 8.3f, 1, "CW1");

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "TOT", 5.5f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "TOT", 4.1f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "TOT", 15.5f, 1, "CW1");

			var invoiceHeaderWithMultiLine = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", addInfo: "CarrierCode=A001", invoiceNumber: "LVSID", dataModel: "CA");
			var cusEntryLine1 = TestDataCreator.CreateCusEntryLine(cusEntryHeader, 1, 6.6m);
			var invoiceLine1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderWithMultiLine, 1, orignState: "MAN", entryLinePK: cusEntryLine1, lineNo: 1, dataModel: "CA");
			var cusEntryLine2 = TestDataCreator.CreateCusEntryLine(cusEntryHeader, 1, 6.7m);
			var invoiceLine2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderWithMultiLine, 1, orignState: "MAN", entryLinePK: cusEntryLine2, lineNo: 2, dataModel: "CA");
			TestDataCreator.CreateCusUnderBondDec(cusEntryLine1, invoiceLine1, 1);
			TestDataCreator.CreateCusUnderBondDec(cusEntryLine2, invoiceLine2, 1);

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine1, "CUD", 6.1f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine2, "CUD", 6.2f, 1, "CUS");

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine1, "ADD", 5.1f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine2, "CVD", 5.2f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine1, "SUR", 5.3f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine2, "CVD", 5.4f, 1, "CUS");

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine1, "GST", 7.7f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine1, "GST", 7.8f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine2, "GST", 7.9f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine2, "GST", 8f, 1, "CUS");

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine1, "FET", 3.4f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine2, "FET", 3.5f, 1, "CUS");

			TestDataCreator.CreateCusEntryLineFee(cusEntryLine1, "TOT", 9.1f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine2, "TOT", 9.3f, 1, "CUS");

			var reportSql = @"SELECT CADTotalEnteredValue, CADTotalDuty, CADTotalSIMA, CADTotalEXS, CADTotalGST, CADTotalDutiesAndTaxes, JZ_PK FROM CAInvoices(@companyPK)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (invoiceHeaderWithOneLine.Equals(reader.GetGuid(6)))
						{
							AssertEquals(9.6m, reader.GetDecimal(0));
							AssertEquals(4.5m, reader.GetDecimal(1));
							AssertEquals(4.9m, reader.GetDecimal(2));
							AssertEquals(1.3m, reader.GetDecimal(3));
							AssertEquals(2.5m, reader.GetDecimal(4));
							AssertEquals(13.2m, reader.GetDecimal(5));
							continue;
						}
						if (invoiceHeaderWithMultiLine.Equals(reader.GetGuid(6)))
						{
							AssertEquals(18.4m, reader.GetDecimal(0));
							AssertEquals(12.3m, reader.GetDecimal(1));
							AssertEquals(21m, reader.GetDecimal(2));
							AssertEquals(6.9m, reader.GetDecimal(3));
							AssertEquals(31.4m, reader.GetDecimal(4));
							AssertEquals(71.6m, reader.GetDecimal(5));
						}
					}
				}
			}
		}

		public void TestPreCarm()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "LVX", 1, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", addInfo: "CarrierCode=A000", invoiceNumber: "LVSID", dataModel: "CA");
			var declaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "LVX", 2, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceHeader(declaration1, false, 2, originState: "NB", addInfo: "CarrierCode=A001", invoiceNumber: "LVSID", dataModel: "CA");
			AssertPreCarm(declaration, "N");

			TestDataCreator.CreateGenPivot(declaration1, declaration, "PRE");
			AssertPreCarm(declaration, "N");

			var entryPK = TestDataCreator.CreateCusEntryHeader(declaration, 1, "B3C", dataModel: "CA");
			TestDataCreator.CreateGenPivot(entryPK, declaration, "PRE");
			AssertPreCarm(declaration, "N");
			AssertPreCarm(declaration1, "N");

			TestDataCreator.CreateGenPivot(declaration, declaration1, "PRE");
			AssertPreCarm(declaration1, "N");

			var declaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "LVX", 3, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceHeader(declaration2, false, 3, originState: "NB", addInfo: "CarrierCode=A002", invoiceNumber: "LVSID", dataModel: "CA");
			TestDataCreator.CreateGenPivot(declaration2, declaration, "PRE");
			TestDataCreator.CreateCusEntryHeader(declaration2, 3, "B3C", entryStatus: "CLR", dataModel: "CA");

			AssertPreCarm(declaration, "Y");

			var declaration3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0004", "LVX", 4, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceHeader(declaration3, false, 4, originState: "NB", addInfo: "CarrierCode=A003", invoiceNumber: "LVSID", dataModel: "CA");
			TestDataCreator.CreateGenPivot(declaration3, declaration, "PRE");
			TestDataCreator.CreateCusEntryHeader(declaration3, 4, "B3C", entryStatus: "CNF", dataModel: "CA");
			AssertPreCarm(declaration, "Y");

			var declaration4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0005", "LVX", 5, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceHeader(declaration4, false, 5, originState: "NB", addInfo: "CarrierCode=A004", invoiceNumber: "LVSID", dataModel: "CA");
			TestDataCreator.CreateGenPivot(declaration4, declaration, "PRE");
			TestDataCreator.CreateCusEntryHeader(declaration4, 5, "B3C", entryStatus: "39", dataModel: "CA");
			AssertPreCarm(declaration, "Y");

			void AssertPreCarm(Guid declaration, string value)
			{
				var reportSql = $@"SELECT PreCarm FROM CAInvoices('{companyPK}') WHERE JE_PK = '{declaration}'";
				var isPreCarm = "";
				Db.Connection.ExecuteReader(reportSql, reader =>
				{
					isPreCarm = reader.GetString(0);
				});
				AssertEquals(isPreCarm, value);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestDataCreator.CreateTariffDataForTesting();
			companyPK = TestDataCreator.CreateCompany("CAN", "CA", "CAD");
			branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'BLO', 'CABLO', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
		}

		Guid companyPK;
		Guid branchPK;
	}
}
