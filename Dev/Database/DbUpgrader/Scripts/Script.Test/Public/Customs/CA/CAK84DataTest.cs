using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(CAK84Data))]
	class CAK84DataTest : DbCreateScriptTest
	{
		public void TestCAK84DataAccountNoIsEmpty()
		{
			var dn1tatemnetPK = TestDataCreator.CreateCusStatementHeader(companyPK, "TST00001", "B", 0, "10207", "");
			TestDataCreator.CreateCusStatementLine(dn1tatemnetPK, "EntryNum1", "AI");
			var dn2tatemnetPK = TestDataCreator.CreateCusStatementHeader(companyPK, "TST00002", "B", 0, "10207", "00001");
			TestDataCreator.CreateCusStatementLine(dn2tatemnetPK, "EntryNum2", "AI");
			var dn3tatemnetPK = TestDataCreator.CreateCusStatementHeader(companyPK, "TST00003", "R", 0, "10207", "");
			TestDataCreator.CreateCusStatementLine(dn3tatemnetPK, "EntryNum3", "AI");
			TestDataCreator.CreateStmData(companyPK, "0x31003000320030003700");

			var sql = @"SELECT * FROM dbo.CAK84Data(@CompanyPK, '', '', '', '', '', NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["B2_PK"], (string)reader["B2_StatementNumber"]);
					}

					AssertEquals(1, result.Count);
					AssertEquals("TST00001", result[dn1tatemnetPK]);
				}
			}
		}

		public void TestCAK84DataCADCharges()
		{
			#region SetUp Data
			var deptPK = TestDataCreator.CreateDepartment("TD1");
			var taxRate = TestDataCreator.CreateAccTaxRate("NOTREPORT", "Not Reportable", "NOT", "CA");
			var disbersementCharge = TestDataCreator.CreateAccCharges(companyPK, "CUSDSB", "Customs Disbursement Charges-GST", "DSB");
			var importerPK = TestDataCreator.CreateOrganisation("ACETESPHL", "ACE TEST IMPORTER 1", "USPHL");

			var lvsDeclaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "LVS", 1, importerPK: importerPK);
			var cusEntryHeader = TestDataCreator.CreateCusEntryHeader(true, "CAD", "CLO", "CLR", "10207000013722", 20, 0, "", lvsDeclaration, DateTime.Now, DateTime.Today.AddDays(-2), Guid.Empty, "", DateTime.Now, DateTime.Now, DateTime.Now, 1);
			var cusEntryLine = TestDataCreator.CreateCusEntryLine(cusEntryHeader, 1);
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "DTY", 10, 1);

			var statementPK = TestDataCreator.CreateCusStatementHeader(companyPK, "TST00001", "B", 0, "10207", "", importerPK);
			TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum1", "AI", 100, "B0000001");

			var lvxDeclaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000002", "LVX", 2, importerPK: importerPK);
			var jobHeader1 = TestDataCreator.CreateJobHeader(branchPK, companyPK, lvxDeclaration1, deptPK, "JE", "B0000002", "00000002", "INV");
			var accHeader1 = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, deptPK, jobHeader1, "AR", "INV", "B0000002", 20, 0, 20, 0, 0);
			var line1 = TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeader1, accHeader1, taxRate, deptPK, 1, "Customs Disbursement Charges", "REV", 20, 0, 20, 0, 0);
			TestConnection.Command($@"UPDATE dbo.AccTransactionLines SET AL_AC = '{disbersementCharge}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{line1}';").ExecuteNonQuery();
			var jobComInvoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(lvxDeclaration1, false, 2);
			var jobComInvoiceLine1 = TestDataCreator.CreateJobComInvoiceLine(jobComInvoiceHeader1, 2);

			TestDataCreator.CreateCusUnderBondDec(cusEntryLine, jobComInvoiceLine1, 1);
			#endregion

			var sql = @"SELECT * FROM dbo.CAK84Data(@CompanyPK, '', '', '', '', '', NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					int billedAmount = 0, count = 0;
					double declarationTotal = 0d, dNCW1Discrepancy = 0d;
					while (reader.Read())
					{
						count++;
						billedAmount = Convert.ToInt32(reader["BilledAmount"]);
						declarationTotal = Convert.ToDouble(reader["DeclarationTotal"]);
						dNCW1Discrepancy = Convert.ToDouble(reader["DNCW1Discrepancy"]);
					}
					CombineAssertions(() =>
					{
						AssertEquals("Count", 1, count);
						AssertEquals("BilledAmount", 20, billedAmount);
						AssertEquals("DeclarationTotal", 10d, declarationTotal);
						AssertEquals("DNCW1Discrepancy", 90d, dNCW1Discrepancy);
					});
				}
			}
		}

		public void TestLVSARDisbersementAmount_Declaration()
		{
			#region SetUp Data
			var deptPK = TestDataCreator.CreateDepartment("TD1");
			var taxRate = TestDataCreator.CreateAccTaxRate("NOTREPORT", "Not Reportable", "NOT", "CA");
			var disbersementCharge = TestDataCreator.CreateAccCharges(companyPK, "CUSDSB", "Customs Disbursement Charges-GST", "DSB");
			var importerPK = TestDataCreator.CreateOrganisation("ACETESPHL", "ACE TEST IMPORTER 1", "USPHL");

			var lvsDeclaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "LVS", 1, importerPK: importerPK);
			var cusEntryHeader = TestDataCreator.CreateCusEntryHeader(true, "B3C", "CLO", "CLR", "10207000013722", 20, 0, "", lvsDeclaration, DateTime.Now, DateTime.Today.AddDays(-2), Guid.Empty, "", DateTime.Now, DateTime.Now, DateTime.Now, 1);
			var cusEntryLine = TestDataCreator.CreateCusEntryLine(cusEntryHeader, 1);
			TestDataCreator.CreateCusEntryLineFee(cusEntryLine, "DTY", 10, 1);

			var statementPK = TestDataCreator.CreateCusStatementHeader(companyPK, "TST00001", "B", 0, "10207", "", importerPK);
			TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum1", "AI", 100, "B0000001");

			var lvxDeclaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000002", "LVX", 2, importerPK: importerPK);
			var jobHeader1 = TestDataCreator.CreateJobHeader(branchPK, companyPK, lvxDeclaration1, deptPK, "JE", "B0000002", "00000002", "INV");
			var accHeader1 = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, deptPK, jobHeader1, "AR", "INV", "B0000002", 20, 0, 20, 0, 0);
			var line1 = TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeader1, accHeader1, taxRate, deptPK, 1, "Customs Disbursement Charges", "REV", 20, 0, 20, 0, 0);
			TestConnection.Command($@"UPDATE dbo.AccTransactionLines SET AL_AC = '{disbersementCharge}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{line1}';").ExecuteNonQuery();
			var jobComInvoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(lvxDeclaration1, false, 2);
			var jobComInvoiceLine1 = TestDataCreator.CreateJobComInvoiceLine(jobComInvoiceHeader1, 2);

			var lvxDeclaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000003", "LVX", 3, importerPK: importerPK);
			var jobHeader2 = TestDataCreator.CreateJobHeader(branchPK, companyPK, lvxDeclaration2, deptPK, "JE", "B0000003", "00000003", "INV");
			var accHeader2 = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, deptPK, jobHeader2, "AR", "INV", "B0000003", 20, 0, 20, 0, 0);
			var line2 = TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeader2, accHeader2, taxRate, deptPK, 1, "Customs Disbursement Charges", "REV", 20, 0, 20, 0, 0);
			TestConnection.Command($@"UPDATE dbo.AccTransactionLines SET AL_AC = '{disbersementCharge}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{line2}';").ExecuteNonQuery();
			var jobComInvoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(lvxDeclaration2, false, 3);
			var jobComInvoiceLine2 = TestDataCreator.CreateJobComInvoiceLine(jobComInvoiceHeader2, 3);

			TestDataCreator.CreateCusUnderBondDec(cusEntryLine, jobComInvoiceLine1, 1);
			TestDataCreator.CreateCusUnderBondDec(cusEntryLine, jobComInvoiceLine2, 1);
			#endregion

			var sql = @"SELECT * FROM dbo.CAK84Data(@CompanyPK, '', '', '', '', '', NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					int billedAmount = 0, count = 0;
					double declarationTotal = 0d, dNCW1Discrepancy = 0d;
					while (reader.Read())
					{
						count++;
						billedAmount = Convert.ToInt32(reader["BilledAmount"]);
						declarationTotal = Convert.ToDouble(reader["DeclarationTotal"]);
						dNCW1Discrepancy = Convert.ToDouble(reader["DNCW1Discrepancy"]);
					}
					CombineAssertions(() =>
					{
						AssertEquals("Count", 1, count);
						AssertEquals("BilledAmount", 40, billedAmount);
						AssertEquals("DeclarationTotal", 10d, declarationTotal);
						AssertEquals("DNCW1Discrepancy", 90d, dNCW1Discrepancy);
					});
				}
			}
		}

		public void TestARDisbersementAmount_ExistingFunctionality()
		{
			var deptPK = TestDataCreator.CreateDepartment("TD1");
			var taxRate = TestDataCreator.CreateAccTaxRate("NOTREPORT", "Not Reportable", "NOT", "CA");
			var disbersementCharge = TestDataCreator.CreateAccCharges(companyPK, "CUSDSB", "Customs Disbursement Charges-GST", "DSB");
			var importerPK = TestDataCreator.CreateOrganisation("ACETESPHL", "ACE TEST IMPORTER 1", "USPHL");

			var impDeclaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "IMP", 13, importerPK: importerPK);
			var jobHeader = TestDataCreator.CreateJobHeader(branchPK, companyPK, impDeclaration, deptPK, "JE", "B0000004", "00000003", "INV");
			var accHeader = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, deptPK, jobHeader, "AR", "INV", "B0000004", 20, 0, 20, 0, 0);
			var line1 = TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeader, accHeader, taxRate, deptPK, 1, "Customs Disbursement Charges", "REV", 10, 0, 10, 0, 0);
			var line2 = TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeader, accHeader, taxRate, deptPK, 1, "Customs Disbursement Charges", "REV", 10, 0, 10, 0, 0);
			TestConnection.Command($@"UPDATE dbo.AccTransactionLines SET AL_AC = '{disbersementCharge}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{line1}';").ExecuteNonQuery();
			TestConnection.Command($@"UPDATE dbo.AccTransactionLines SET AL_AC = '{disbersementCharge}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{line2}';").ExecuteNonQuery();

			var statementPK = TestDataCreator.CreateCusStatementHeader(companyPK, "TST00001", "B", 0, "10207", "", importerPK);
			TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum1", "AI", 100, "B0000001");

			var sql = @"SELECT * FROM dbo.CAK84Data(@CompanyPK, '', '', '', '', '', NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var result = new List<int>();
					while (reader.Read())
					{
						result.Add(Convert.ToInt32(reader["BilledAmount"]));
					}
					AssertEquals(1, result.Count);
					AssertEquals(20, result[0]);
				}
			}
		}

		public void TestARDNDiscrepancy_ImporterPaid()
		{
			#region SetUp Data
			var deptPK = TestDataCreator.CreateDepartment("TD1");
			var taxRate = TestDataCreator.CreateAccTaxRate("NOTREPORT", "Not Reportable", "NOT", "CA");
			var importerPK = TestDataCreator.CreateOrganisation("ACETESPHL", "ACE TEST IMPORTER 1", "USPHL");

			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "IMP", 1, importerPK: importerPK);
			var jobHeader1 = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration, deptPK, "JE", "B0000001", "00000001", "INV");
			var accHeader1 = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, deptPK, jobHeader1, "AR", "INV", "B0000002", 20, 0, 20, 0, 0);
			var dsbCharge = TestDataCreator.CreateAccCharges(companyPK, "CUSDSB", "Customs Disbursement Charges", "DSB");
			var line1 = TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeader1, accHeader1, taxRate, deptPK, 1, "Customs Disbursement Charges", "REV", 20, 0, 20, 0, 0);
			TestConnection.Command($@"UPDATE dbo.AccTransactionLines SET AL_AC = '{dsbCharge}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{line1}';").ExecuteNonQuery();

			var statementPK = TestDataCreator.CreateCusStatementHeader(companyPK, "1", "B", 0, "10207", "", importerPK);
			var statementLine = TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum1", "AI", 40, "B0000001");
			TestDataCreator.CreateCusStatementLineCharge(statementLine, "SIM", 20, "IMP");
			TestDataCreator.CreateCusStatementLineCharge(statementLine, "DTY", 20, "IMP");
			#endregion

			var sql = @"SELECT * FROM dbo.CAK84Data(@CompanyPK, '', '', '', '', '', NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var result = new List<int>();
					while (reader.Read())
					{
						result.Add(Convert.ToInt32(reader["ARDNDiscrepancy"]));
					}
					AssertEquals(1, result.Count);
					AssertEquals(0, result[0]);
				}
			}
		}

		public void TestARDNDiscrepancy_BrokerPaid()
		{
			#region SetUp Data
			var deptPK = TestDataCreator.CreateDepartment("TD1");
			var taxRate = TestDataCreator.CreateAccTaxRate("NOTREPORT", "Not Reportable", "NOT", "CA");
			var importerPK = TestDataCreator.CreateOrganisation("ACETESPHL", "ACE TEST IMPORTER 1", "USPHL");

			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "IMP", 1, importerPK: importerPK);
			var jobHeader1 = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration, deptPK, "JE", "B0000001", "00000001", "INV");
			var accHeader1 = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, deptPK, jobHeader1, "AR", "INV", "B0000002", 20, 0, 20, 0, 0);
			var dsbCharge = TestDataCreator.CreateAccCharges(companyPK, "CUSDSB", "Customs Disbursement Charges", "DSB");
			var line1 = TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeader1, accHeader1, taxRate, deptPK, 1, "Customs Disbursement Charges", "REV", 20, 0, 20, 0, 0);
			TestConnection.Command($@"UPDATE dbo.AccTransactionLines SET AL_AC = '{dsbCharge}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{line1}';").ExecuteNonQuery();

			var statementPK = TestDataCreator.CreateCusStatementHeader(companyPK, "1", "B", 0, "10207", "", importerPK);
			var statementLine = TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum1", "B3", 40, "B0000001");
			TestDataCreator.CreateCusStatementLineCharge(statementLine, "GST", 20, "BRK");
			TestDataCreator.CreateCusStatementLineCharge(statementLine, "DTY", 20, "BRK");
			#endregion

			var sql = @"SELECT * FROM dbo.CAK84Data(@CompanyPK, '', '', '', '', '', NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var result = new List<int>();
					while (reader.Read())
					{
						result.Add(Convert.ToInt32(reader["ARDNDiscrepancy"]));
					}
					AssertEquals(1, result.Count);
					AssertEquals(-20, result[0]);
				}
			}
		}

		public void TestARDNDiscrepancy_GSTDirect()
		{
			#region SetUp Data
			var deptPK = TestDataCreator.CreateDepartment("TD1");
			var taxRate = TestDataCreator.CreateAccTaxRate("NOTREPORT", "Not Reportable", "NOT", "CA");
			var importerPK = TestDataCreator.CreateOrganisation("ACETESPHL", "ACE TEST IMPORTER 1", "USPHL");

			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "IMP", 1, importerPK: importerPK);
			var jobHeader1 = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration, deptPK, "JE", "B0000001", "00000001", "INV");
			var accHeader1 = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, deptPK, jobHeader1, "AR", "INV", "B0000002", 20, 0, 20, 0, 0);
			var dsbCharge = TestDataCreator.CreateAccCharges(companyPK, "CUSDSB", "Customs Disbursement Charges", "DSB");
			var line1 = TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeader1, accHeader1, taxRate, deptPK, 1, "Customs Disbursement Charges", "REV", 20, 0, 20, 0, 0);
			TestConnection.Command($@"UPDATE dbo.AccTransactionLines SET AL_AC = '{dsbCharge}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{line1}';").ExecuteNonQuery();

			var statementPK = TestDataCreator.CreateCusStatementHeader(companyPK, "1", "B", 0, "10207", "", importerPK);
			var statementLine = TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum1", "B3", 40, "B0000001");
			TestDataCreator.CreateCusStatementLineCharge(statementLine, "GSD", 20, "IMP");
			TestDataCreator.CreateCusStatementLineCharge(statementLine, "DTY", 20, "BRK");
			#endregion

			var sql = @"SELECT * FROM dbo.CAK84Data(@CompanyPK, '', '', '', '', '', NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var result = new List<int>();
					while (reader.Read())
					{
						result.Add(Convert.ToInt32(reader["ARDNDiscrepancy"]));
					}
					AssertEquals(1, result.Count);
					AssertEquals(0, result[0]);
				}
			}
		}

		public void TestImporter()
		{
			#region Set Up Data

			var importerPK1 = TestDataCreator.CreateOrganisation("ACETESPH1", "ACE TEST IMPORTER 1", "USPHL");
			var importerPK2 = TestDataCreator.CreateOrganisation("ACETESPH2", "ACE TEST IMPORTER 2", "USPHL");

			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "IMP", 1, importerPK: importerPK1);

			var statementPK = TestDataCreator.CreateCusStatementHeader(companyPK, "", "B", 0, "10207", "", importerPK2);
			var statementLine = TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum1", "B3", 40, "B0000001");

			#endregion

			var sql = @"SELECT * FROM dbo.CAK84Data(@CompanyPK, '', '', '', '', '', NULL, NULL)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals(importerPK2, (Guid)reader["ImporterPK"]);
						AssertEquals("ACETESPH2", Convert.ToString(reader["ImporterCode"]));
						AssertEquals("ACE TEST IMPORTER 2", Convert.ToString(reader["ImporterFullName"]));
					}
					AssertEquals(1, count);
				}
			}
		}

		public void TestSearchByMonthYearPeriod()
		{
			#region Set Up Data

			var importerPK1 = TestDataCreator.CreateOrganisation("ACETESPH1", "ACE TEST IMPORTER 1", "USPHL");

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "IMP", 1, importerPK: importerPK1, dataModel: "CA",
				addInfo: "K84AccountingDate=2025-05-24 00:00:00.000", entryAuthorisationDate: new DateTime(2025, 04, 18));
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000002", "IMP", 2, importerPK: importerPK1, dataModel: "CA",
				valuationDate: new DateTime(2025, 05, 20));
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000003", "IMP", 3, importerPK: importerPK1, dataModel: "CA",
				entryAuthorisationDate: new DateTime(2025, 05, 01));

			var statementPK = TestDataCreator.CreateCusStatementHeader(companyPK, "", "B", 0, "10207", "", importerPK1);
			TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum1", "B3", 40, "B0000001");
			TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum2", "B3", 45, "B0000002");
			TestDataCreator.CreateCusStatementLine(statementPK, "EntryNum3", "B3", 50, "B0000003");

			#endregion

			var sql = @"SELECT B3_BrokerReference FROM dbo.CAK84Data(@CompanyPK, '', '', '', '', '', 05, 2025)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var dt = new DataTable();
					dt.Load(reader);
					AssertEquals(2, dt.Rows.Count);
					Assert(dt.AsEnumerable().Any(x => x.Field<string>("B3_BrokerReference") == "B0000001"));
					Assert(dt.AsEnumerable().Any(x => x.Field<string>("B3_BrokerReference") == "B0000002"));
				}
			}
		}

		Guid companyPK;
		Guid branchPK;

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("DCA", "CA", "CAD");
			branchPK = TestDataCreator.CreateBranch(companyPK, "BLO", "CABLO");
		}
	}
}
