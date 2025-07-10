using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(CAInvoiceLines))]
	class CAInvoicesLineTest : DbCreateScriptTest
	{
		public void TestInvoiceLineVFTAndB3SubHeaderNumber()
		{
			var declaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(declaration1, false, 1, dataModel: "CA");
			var invoiceLine1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, addInfo: "ValueForTax=100.00*B3SubHeaderNumber=1", lineNo: 1, dataModel: "CA");
			var invoiceLine2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, addInfo: "ValueForTax=101.99", lineNo: 2, dataModel: "CA");
			var invoiceLine3 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, addInfo: "B3SubHeaderNumber=2", lineNo: 3, dataModel: "CA");
			var invoiceLine4 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, lineNo: 4, dataModel: "CA");

			var result = new Dictionary<string, (decimal vft, int b3SubHeaderNo)>();
			Db.Connection.ExecuteReader($@"SELECT InvoiceLineNumber, ValueForTax, B3SubHeaderNumber FROM CAInvoiceLines('{companyPK}', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY InvoiceLineNumber", reader =>
			{
				result.Add(reader.GetInt16(0).ToString(), (reader.GetDecimal(1), reader.GetInt32(2)));
			});

			CombineAssertions(() =>
			{
				AssertEquals("Invoice Line 1", (100m, 1), result["1"]);
				AssertEquals("Invoice Line 2", (101.99m, 0), result["2"]);
				AssertEquals("Invoice Line 3", (0m, 2), result["3"]);
				AssertEquals("Invoice Line 4", (0m, 0), result["4"]);
			});
		}

		public void TestInvoiceLineDutiesAndTaxes()
		{
			var declaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(declaration1, false, 1, dataModel: "CA");
			var invoiceLine1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, dataModel: "CA", addInfo: "SIMAmount=201*SIMExemptCode=11");

			var declaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IMP", 2, dataModel: "CA");
			var invoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(declaration2, false, 2, dataModel: "CA");
			var invoiceLine2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 2, dataModel: "CA", addInfo: "GSTAmount=401*GSTExemptCode=21*GSTRateDescription=42G 43G");

			var declaration3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "IMP", 3, dataModel: "CA");
			var invoiceHeader3 = TestDataCreator.CreateJobComInvoiceHeader(declaration3, false, 3, dataModel: "CA");
			var invoiceLine3 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader3, 3, dataModel: "CA", addInfo: "DTYAmount=601*DTYRateDescription=43D 44");

			var declaration4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0004", "IMP", 4, dataModel: "CA");
			var invoiceHeader4 = TestDataCreator.CreateJobComInvoiceHeader(declaration4, false, 4, dataModel: "CA");
			var invoiceLine4 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader4, 4, dataModel: "CA", addInfo: "EXSAmount=801*EXSExemptCode=41*EXSRateDescription=44E 45");

			var declaration5 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0005", "IMP", 5, dataModel: "CA");
			var invoiceHeader5 = TestDataCreator.CreateJobComInvoiceHeader(declaration5, false, 5, originState: "NB", dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeader5, 5, dataModel: "CA");

			var result = new Dictionary<string, (decimal LineDuty, string DutyRates, decimal LineExciseTax, string ExciseTaxRate, decimal LineSIMADuty, string SIMACode, decimal LineGST, string GSTExemptCode, string GSTRate, decimal TotalLineDutiesAndTaxes)>();
			Db.Connection.ExecuteReader($@"SELECT JE_DeclarationReference, LineDuty, DutyRates, LineExciseTax, ExciseTaxRate, LineSIMADuty, SIMACode, LineGST, GSTExemptCode, GSTRate, TotalLineDutiesAndTaxes FROM CAInvoiceLines('{companyPK}', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY JE_DeclarationReference", reader =>
			{
				result.Add(reader.GetString(0), (reader.GetDecimal(1), reader.GetString(2), reader.GetDecimal(3), reader.GetString(4), reader.GetDecimal(5), reader.GetString(6), reader.GetDecimal(7), reader.GetString(8), reader.GetString(9), reader.GetDecimal(10)));
			});

			CombineAssertions(() =>
			{
				AssertEquals("B0001", (0m, "", 0m, "", 201m, "11", 0m, "", "", 201m), result["B0001"]);
				AssertEquals("B0002", (0m, "", 0m, "", 0m, "", 401m, "21", "42G 43G", 401m), result["B0002"]);
				AssertEquals("B0003", (601m, "43D 44", 0m, "", 0m, "", 0m, "", "", 601m), result["B0003"]);
				AssertEquals("B0004", (0m, "", 801m, "44E 45", 0m, "", 0m, "", "", 801m), result["B0004"]);
				AssertEquals("B0005", (0m, "", 0m, "", 0m, "", 0m, "", "", 0m), result["B0005"]);
			});
		}

		public void TestInvoiceLineChargeAmountsData()
		{
			var declaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(declaration1, false, 1, dataModel: "CA");
			var invoiceLine1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, dataModel: "CA");
			Db.Connection.ExecuteNonQuery($@"INSERT dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_RX_NKCurrency, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES
(NEWID(), 'JI', '{invoiceLine1}', 1, 'OFT', 120, 'CAD', 10, 0, 1, 0),
(NEWID(), 'JI', '{invoiceLine1}', 1, 'ONS', 130, 'CAD', 10, 0, 1, 0),
(NEWID(), 'JI', '{invoiceLine1}', 1, 'OFT', 120, 'CNY', 10, 0, 1, 0),
(NEWID(), 'JI', '{invoiceLine1}', 1, 'ONS', 130, 'CNY', 10, 0, 1, 0)
");

			var declaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IMP", 2, dataModel: "CA");
			var invoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(declaration2, false, 2, dataModel: "CA");
			var invoiceLine2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 2, dataModel: "CA");
			Db.Connection.ExecuteNonQuery($@"INSERT dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_RX_NKCurrency, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES
(NEWID(), 'JI', '{invoiceLine2}', 1, 'OFT', 220, 'CAD', 10, 0, 1, 0),
(NEWID(), 'JI', '{invoiceLine2}', 1, 'ONS', 230, 'CAD', 10, 0, 1, 0),
(NEWID(), 'JI', '{invoiceLine2}', 1, 'OFT', 220, 'CNY', 10, 0, 1, 0),
(NEWID(), 'JI', '{invoiceLine2}', 1, 'ONS', 230, 'CNY', 10, 0, 1, 0)
");

			var declaration3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "IMP", 3, dataModel: "CA");
			var invoiceHeader3 = TestDataCreator.CreateJobComInvoiceHeader(declaration3, false, 3, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeader3, 3, dataModel: "CA");

			var result = new Dictionary<string, (string LineOFTAmounts, string LineONSAmounts)>();
			Db.Connection.ExecuteReader($@"SELECT JE_DeclarationReference, LineOFTAmounts, LineONSAmounts FROM CAInvoiceLines('{companyPK}', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY JE_DeclarationReference", reader =>
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

		public void TestInvoiceSIMADuty()
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

			var resultDic = new Dictionary<string, decimal>();
			Db.Connection.ExecuteReader($@"SELECT JE_DeclarationReference, InvoiceSIMADuty FROM CAInvoiceLines('{companyPK}', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY JE_DeclarationReference", reader =>
			{
				resultDic.Add(reader.GetString(0), reader.GetDecimal(1));
			});
			CombineAssertions(() =>
			{
				AssertEquals("TaxType SIM fail", 100M, resultDic["B0001"]);
				AssertEquals("TaxType GST fail", 0M, resultDic["B0002"]);
				AssertEquals("TaxType DTY fail", 0M, resultDic["B0003"]);
			});
		}

		public void TestOriginalStateFromInvoiceLine()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", dataModel: "CA");
			var invoiceLine = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, orignState: "MAN", dataModel: "CA");

			var declaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IMP", 2, dataModel: "CA");
			var invoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(declaration2, false, 2, dataModel: "CA");
			var invoiceLine2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 2, orignState: "NOV", dataModel: "CA");

			var reportSql = @"SELECT JE_DeclarationReference, LineOriginState FROM CAInvoiceLines(@companyPK, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY JE_DeclarationReference";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("B0001", reader.GetString(0));
					AssertEquals("Original State", "MAN", reader.GetString(1));

					Assert("There should be a second record", reader.Read());
					AssertEquals("B0002", reader.GetString(0));
					AssertEquals("Original State", "NOV", reader.GetString(1));
				}
			}
		}

		public void TestOriginalStateFallsbackToInvoiceHeader()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", dataModel: "CA");
			var invoiceLine = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, dataModel: "CA");

			var reportSql = @"SELECT JE_DeclarationReference, LineOriginState FROM CAInvoiceLines(@companyPK, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY JE_DeclarationReference";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals("B0001", reader.GetString(0));
					AssertEquals("Original State", "NB", reader.GetString(1));
				}
			}
		}

		public void TestPGAProgramFilter()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", dataModel: "CA");
			var addInfoPrefix = JobComInvoiceLineSchema.Constants.Prefix;

			var invoiceLine11 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 1, dataModel: "CA");
			var invoiceLine12 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 2, dataModel: "CA");
			var invoiceLine13 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 3, dataModel: "CA");
			var invoiceLine14 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 4, dataModel: "CA");
			var invoiceLine15 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 5, dataModel: "CA");
			var invoiceLine16 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 6, dataModel: "CA");
			var invoiceLine17 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 7, dataModel: "CA");
			var invoiceLine18 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 8, dataModel: "CA");
			var invoiceLine19 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 9, dataModel: "CA");
			var invoiceLine1A = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 10, dataModel: "CA");
			var invoiceLine1B = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 11, dataModel: "CA");
			var invoiceLine1C = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "HCInd=Y", lineNo: 12, dataModel: "CA");

			var addinfo11 = TestDataCreator.CreateCusAddInfo("CHC", "APIProgramInd=Y", addInfoPrefix, invoiceLine11);
			var addinfo12 = TestDataCreator.CreateCusAddInfo("CHC", "BBCProgramInd=Y", addInfoPrefix, invoiceLine12);
			var addinfo13 = TestDataCreator.CreateCusAddInfo("CHC", "CTOProgramInd=Y", addInfoPrefix, invoiceLine13);
			var addinfo14 = TestDataCreator.CreateCusAddInfo("CHC", "CPRProgramInd=Y", addInfoPrefix, invoiceLine14);
			var addinfo15 = TestDataCreator.CreateCusAddInfo("CHC", "DSEProgramInd=Y", addInfoPrefix, invoiceLine15);
			var addinfo16 = TestDataCreator.CreateCusAddInfo("CHC", "HDRProgramInd=Y", addInfoPrefix, invoiceLine16);
			var addinfo17 = TestDataCreator.CreateCusAddInfo("CHC", "OCSProgramInd=Y", addInfoPrefix, invoiceLine17);
			var addinfo18 = TestDataCreator.CreateCusAddInfo("CHC", "MDEProgramInd=Y", addInfoPrefix, invoiceLine18);
			var addinfo19 = TestDataCreator.CreateCusAddInfo("CHC", "NHPProgramInd=Y", addInfoPrefix, invoiceLine19);
			var addinfo1A = TestDataCreator.CreateCusAddInfo("CHC", "PESProgramInd=Y", addInfoPrefix, invoiceLine1A);
			var addinfo1B = TestDataCreator.CreateCusAddInfo("CHC", "REDProgramInd=Y", addInfoPrefix, invoiceLine1B);
			var addinfo1C = TestDataCreator.CreateCusAddInfo("CHC", "VETProgramInd=Y", addInfoPrefix, invoiceLine1C);

			var invoiceLine21 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "TCInd=Y", lineNo: 13, dataModel: "CA");
			var invoiceLine22 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "TCInd=Y", lineNo: 14, dataModel: "CA");

			var addinfo21 = TestDataCreator.CreateCusAddInfo("CTC", "TPRProgramInd=Y", addInfoPrefix, invoiceLine21);
			var addinfo22 = TestDataCreator.CreateCusAddInfo("CTC", "VPRProgramInd=Y", addInfoPrefix, invoiceLine22);

			var invoiceLine31 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "ECCCInd=Y", lineNo: 15, dataModel: "CA");
			var invoiceLine32 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "ECCCInd=Y", lineNo: 16, dataModel: "CA");
			var invoiceLine33 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "ECCCInd=Y", lineNo: 17, dataModel: "CA");
			var invoiceLine34 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "ECCCInd=Y", lineNo: 18, dataModel: "CA");

			var addinfo31 = TestDataCreator.CreateCusAddInfo("CEC", "WRMProgramInd=Y", addInfoPrefix, invoiceLine31);
			var addinfo32 = TestDataCreator.CreateCusAddInfo("CEC", "ODSProgramInd=Y", addInfoPrefix, invoiceLine32);
			var addinfo33 = TestDataCreator.CreateCusAddInfo("CEC", "WENProgramInd=Y", addInfoPrefix, invoiceLine33);
			var addinfo34 = TestDataCreator.CreateCusAddInfo("CEC", "VEEProgramInd=Y", addInfoPrefix, invoiceLine34);

			var invoiceLine41 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "NRCanInd=Y", lineNo: 19, dataModel: "CA");
			var invoiceLine42 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "NRCanInd=Y", lineNo: 20, dataModel: "CA");
			var invoiceLine43 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "NRCanInd=Y", lineNo: 21, dataModel: "CA");

			var addinfo41 = TestDataCreator.CreateCusAddInfo("CNR", "EEFProgramInd=Y", addInfoPrefix, invoiceLine41);
			var addinfo42 = TestDataCreator.CreateCusAddInfo("CNR", "EXPProgramInd=Y", addInfoPrefix, invoiceLine42);
			var addinfo43 = TestDataCreator.CreateCusAddInfo("CNR", "RDAProgramInd=Y", addInfoPrefix, invoiceLine43);

			var invoiceLine51 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "DFOInd=Y", lineNo: 22, dataModel: "CA");
			var invoiceLine52 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "DFOInd=Y", lineNo: 23, dataModel: "CA");
			var invoiceLine53 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "DFOInd=Y", lineNo: 24, dataModel: "CA");

			var addinfo51 = TestDataCreator.CreateCusAddInfo("CFO", "ABIProgramInd=Y", addInfoPrefix, invoiceLine51);
			var addinfo52 = TestDataCreator.CreateCusAddInfo("CFO", "AISProgramInd=Y", addInfoPrefix, invoiceLine52);
			var addinfo53 = TestDataCreator.CreateCusAddInfo("CFO", "TTPProgramInd=Y", addInfoPrefix, invoiceLine53);

			var invoiceLine60 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "CNSCInd=Y", lineNo: 25, dataModel: "CA");
			var addinfo60 = TestDataCreator.CreateCusAddInfo("CCN", "ALLProgramInd=Y", addInfoPrefix, invoiceLine60);

			var invoiceLine70 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "GACInd=Y", lineNo: 26, dataModel: "CA");
			var addinfo70 = TestDataCreator.CreateCusAddInfo("CGA", "ALLProgramInd=Y", addInfoPrefix, invoiceLine70);

			var invoiceLine80 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "CFIAInd=Y", lineNo: 27, dataModel: "CA");
			var addinfo80 = TestDataCreator.CreateCusAddInfo("CCF", "ALLProgramInd=Y", addInfoPrefix, invoiceLine80);

			var invoiceLine91 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, addInfo: "PHACInd=Y", lineNo: 28, dataModel: "CA");
			var addinfo91 = TestDataCreator.CreateCusAddInfo("CPH", "HAPProgramInd=Y", addInfoPrefix, invoiceLine91);

			var reportSql = @"SELECT InvoiceLineNumber FROM CAInvoiceLines(@companyPK, 'API,BBC,CTO,CPR,DSE,HDR,OCS,MDE,NHP,PES,RED,VET', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY InvoiceLineNumber";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					for (short i = 1; i < 13; i++)
					{
						Assert("There should be a record", reader.Read());
						AssertEquals(i, reader.GetInt16(0));
					}
				}
			}

			reportSql = @"SELECT InvoiceLineNumber FROM CAInvoiceLines(@companyPK, 'TPR,VPR', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY InvoiceLineNumber";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					for (short i = 13; i < 15; i++)
					{
						Assert("There should be a record", reader.Read());
						AssertEquals(i, reader.GetInt16(0));
					}
				}
			}

			reportSql = @"SELECT InvoiceLineNumber FROM CAInvoiceLines(@companyPK, 'WRM,ODS,WEN,VEE', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY InvoiceLineNumber";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					for (short i = 15; i < 19; i++)
					{
						Assert("There should be a record", reader.Read());
						AssertEquals(i, reader.GetInt16(0));
					}
				}
			}

			reportSql = @"SELECT InvoiceLineNumber FROM CAInvoiceLines(@companyPK, 'EEF,EXP,RDA', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY InvoiceLineNumber";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					for (short i = 19; i < 22; i++)
					{
						Assert("There should be a record", reader.Read());
						AssertEquals(i, reader.GetInt16(0));
					}
				}
			}

			reportSql = @"SELECT InvoiceLineNumber FROM CAInvoiceLines(@companyPK, 'ABI,AIS,TTP', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY InvoiceLineNumber";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					for (short i = 22; i < 25; i++)
					{
						Assert("There should be a record", reader.Read());
						AssertEquals(i, reader.GetInt16(0));
					}
				}
			}

			reportSql = @"SELECT InvoiceLineNumber FROM CAInvoiceLines(@companyPK, 'CNSC,GAC,CFIA,HAP', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY InvoiceLineNumber";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					for (short i = 25; i < 29; i++)
					{
						Assert("There should be a record", reader.Read());
						AssertEquals(i, reader.GetInt16(0));
					}
				}
			}
		}

		public void TestProductCodeFilter()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", dataModel: "CA");
			var invoiceLine01 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, lineNo: 1, partNo: "A01", dataModel: "CA");
			var invoiceLine02 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, lineNo: 2, partNo: "A02", dataModel: "CA");
			var invoiceLine03 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, lineNo: 3, partNo: "A03", dataModel: "CA");

			var reportSql = @"SELECT InvoiceLineNumber FROM CAInvoiceLines(@companyPK, '', 'A01', '', 'A03', '', '', '', '', '', '', '', '', '', '', '', '') ORDER BY InvoiceLineNumber";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals((short)1, reader.GetInt16(0));

					Assert("There should be a record", reader.Read());
					AssertEquals((short)3, reader.GetInt16(0));
				}
			}
		}

		public void TestClassificationFilter()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", dataModel: "CA");
			var invoiceLine01 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, lineNo: 1, classification: "3333333333", dataModel: "CA");
			var invoiceLine02 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, lineNo: 2, classification: "4444444444", dataModel: "CA");
			var invoiceLine03 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, lineNo: 3, classification: "5555555555", dataModel: "CA");

			var reportSql = @"SELECT InvoiceLineNumber FROM CAInvoiceLines(@companyPK, '', '', '', '', '', '', '', '3333333333', '', '5555555555', '', '', '', '', '', '') ORDER BY InvoiceLineNumber";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals((short)1, reader.GetInt16(0));

					Assert("There should be a record", reader.Read());
					AssertEquals((short)3, reader.GetInt16(0));
				}
			}
		}

		public void TestTariffCodeFilter()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, originState: "NB", dataModel: "CA");
			var invoiceLine01 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, lineNo: 1, addInfo: "99TariffCode=3333", dataModel: "CA");
			var invoiceLine02 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, lineNo: 2, addInfo: "99TariffCode=4444", dataModel: "CA");
			var invoiceLine03 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader, 1, lineNo: 3, addInfo: "99TariffCode=5555", dataModel: "CA");

			var reportSql = @"SELECT InvoiceLineNumber FROM CAInvoiceLines(@companyPK, '', '', '', '', '', '', '', '', '', '', '', '', '', '3333', '', '5555') ORDER BY InvoiceLineNumber";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals((short)1, reader.GetInt16(0));

					Assert("There should be a record", reader.Read());
					AssertEquals((short)3, reader.GetInt16(0));
				}
			}
		}

		public void TestCADEntryLineFee()
		{
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "1", "IMP", 1, dataModel: "CA");
			var headerPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "CA");
			var linePK = TestDataCreator.CreateJobComInvoiceLine(headerPK, 1, dataModel: "CA");
			var cusEntryHeaderPK = TestDataCreator.CreateCusEntryHeader(declarationPK, 1, "CAD", dataModel: "CA");
			var cusEntryLinePK = TestDataCreator.CreateCusEntryLine(cusEntryHeaderPK, 1, 1, 1.5m, "V");
			TestDataCreator.CreateCusUnderBondDec(cusEntryLinePK, linePK, 1);

			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "CUD", 1.1f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "CUD", 1.2f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "CUD", 1.3f, 1);

			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "FET", 2.1f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "FET", 2.2f, 1);

			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "GST", 3.1f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "GST", 3.2f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "GST", 3.3f, 1);

			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "ADD", 4.1f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "CVD", 4.2f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "SUR", 4.3f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK, "ADD", 4.4f, 1);

			var linePK2 = TestDataCreator.CreateJobComInvoiceLine(headerPK, 1, dataModel: "CA");
			var cusEntryLinePK2 = TestDataCreator.CreateCusEntryLine(cusEntryHeaderPK, 1, 2, 2.5m, "V");
			TestDataCreator.CreateCusUnderBondDec(cusEntryLinePK2, linePK2, 1);

			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK2, "CUD", 0.1f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK2, "FET", 0.2f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK2, "GST", 0.3f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK2, "ADD", 0.4f, 1, "CUS");

			var sql = $@"
				SELECT
					JI_PK, CADLineDuty, CADLineEXS, CADLineGST, CADLineSIMA, CADSeqNo
				FROM
					CAInvoiceLines('{companyPK}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
				WHERE
					JE_PK = '{declarationPK}'";

			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				var dt = new DataTable("CAInvoiceLines");
				dt.Load(reader);

				AssertEquals(2, dt.Rows.Count);

				var row = dt.Select($"JI_PK = '{linePK}'").Single();
				AssertFieldEquals(row, "CADLineDuty", 2.3m);
				AssertFieldEquals(row, "CADLineEXS", 2.1m);
				AssertFieldEquals(row, "CADLineGST", 6.3m);
				AssertFieldEquals(row, "CADLineSIMA", 12.6m);
				AssertFieldEquals(row, "CADSeqNo", (short)1);

				row = dt.Select($"JI_PK = '{linePK2}'").Single();
				AssertFieldEquals(row, "CADLineDuty", 0.1m);
				AssertFieldEquals(row, "CADLineEXS", 0.2m);
				AssertFieldEquals(row, "CADLineGST", 0.3m);
				AssertFieldEquals(row, "CADLineSIMA", 0.4m);
				AssertFieldEquals(row, "CADSeqNo", (short)2);
			}
		}

		public void TestPreCarm()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "1", "IMP", 1, dataModel: "CA");
			var headerPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceLine(headerPK1, 1, dataModel: "CA");
			AssertPreCarm(declarationPK1, "N");

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "2", "IMP", 2, dataModel: "CA");
			var headerPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2, dataModel: "CA");
			TestDataCreator.CreateJobComInvoiceLine(headerPK2, 2, dataModel: "CA");
			TestDataCreator.CreateGenPivot(declarationPK2, declarationPK1, "PRE");
			AssertPreCarm(declarationPK1, "N");
			TestDataCreator.CreateCusEntryHeader(declarationPK2, 2, "B3C", entryStatus: "CLR", dataModel: "CA");
			AssertPreCarm(declarationPK1, "Y");
		}

		void AssertFieldEquals<T>(DataRow row, string fieldName, T expectedValue)
		{
			var actualValue = row.Field<T>(fieldName);
			AssertEquals(fieldName, expectedValue, actualValue);
		}

		void AssertPreCarm(Guid declaration, string expectedValue)
		{
			var sql = $@"
				SELECT
					PreCarm
				FROM
					CAInvoiceLines('{companyPK}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
				WHERE
					JE_PK = '{declaration}'";

			var aaaa = Db.Connection.ExecuteScalar(sql);
			var actualValue = Db.Connection.ExecuteScalar<string>(sql);
			AssertEquals(expectedValue, actualValue);
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
