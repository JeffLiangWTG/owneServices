using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Test
{
	class TG_AccJobConfig_InsertUpdateIntegrationTest : TransactionedTestCase
	{
		public void TestTriggerForCFXInsert()
		{
			AssertExceptionThrownSafe("Missing required fields for a Job CFX Uplift configuration.", new Action<DbConnection>((connection) => InsertCFXJobConfig(connection, Guid.Empty, "", Guid.Empty, "")));
			AssertExceptionThrownSafe("Missing required fields for a Job CFX Uplift configuration.", new Action<DbConnection>((connection) => InsertCFXJobConfig(connection, CompanyPK, "", Guid.Empty, "", "AP")));

			AssertExceptionThrownSafe("Invalid Currency code for a Job CFX Uplift configuration.", new Action<DbConnection>((connection) => InsertCFXJobConfig(connection, CompanyPK, "", Guid.Empty, "***")));

			AssertExceptionThrownSafe("Invalid Parent Branch PK.", new Action<DbConnection>((connection) => InsertCFXJobConfig(connection, CompanyPK, "GB", Guid.NewGuid(), "")));
			AssertExceptionThrownSafe("Invalid Parent Organisation PK.", new Action<DbConnection>((connection) => InsertCFXJobConfig(connection, CompanyPK, "OH", Guid.NewGuid(), "")));
		}

		public void TestTriggerForCFXUpdate()
		{
			AssertExceptionThrownSafe("Missing required fields for a Job CFX Uplift configuration.", new Action<DbConnection>((connection) => UpdateCFXJobConfig(connection, Guid.Empty, "", Guid.Empty, "")));
			AssertExceptionThrownSafe("Missing required fields for a Job CFX Uplift configuration.", new Action<DbConnection>((connection) => UpdateCFXJobConfig(connection, CompanyPK, "", Guid.Empty, "", "AP")));

			AssertExceptionThrownSafe("Invalid Currency code for a Job CFX Uplift configuration.", new Action<DbConnection>((connection) => UpdateCFXJobConfig(connection, CompanyPK, "", Guid.Empty, "***")));

			AssertExceptionThrownSafe("Invalid Parent Branch PK.", new Action<DbConnection>((connection) => UpdateCFXJobConfig(connection, CompanyPK, "GB", Guid.NewGuid(), "")));
			AssertExceptionThrownSafe("Invalid Parent Organisation PK.", new Action<DbConnection>((connection) => UpdateCFXJobConfig(connection, CompanyPK, "OH", Guid.NewGuid(), "")));
		}

		public void TestTriggerForERTInsert()
		{
			AssertExceptionThrownSafe("Incorrect Exchage Rate Date Preference code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => InsertERTJobConfig(connection, CompanyPK, "", Guid.Empty, "***")));

			AssertExceptionThrownSafe("Invalid Parent Creditor Group PK.", new Action<DbConnection>((connection) => InsertERTJobConfig(connection, CompanyPK, "OG", Guid.NewGuid(), "TDR")));
			AssertExceptionThrownSafe("Invalid Parent Debtor Group PK.", new Action<DbConnection>((connection) => InsertERTJobConfig(connection, CompanyPK, "OJ", Guid.NewGuid(), "TDR")));
			AssertExceptionThrownSafe("Invalid Parent Organisation PK.", new Action<DbConnection>((connection) => InsertERTJobConfig(connection, CompanyPK, "OH", Guid.NewGuid(), "TDR")));
		}

		public void TestTriggerForERTUpdate()
		{
			AssertExceptionThrownSafe("Incorrect Exchage Rate Date Preference code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => UpdateERTJobConfig(connection, CompanyPK, "", Guid.Empty, "***")));

			AssertExceptionThrownSafe("Invalid Parent Creditor Group PK.", new Action<DbConnection>((connection) => UpdateERTJobConfig(connection, CompanyPK, "OG", Guid.NewGuid(),"TDR")));
			AssertExceptionThrownSafe("Invalid Parent Debtor Group PK.", new Action<DbConnection>((connection) => UpdateERTJobConfig(connection, CompanyPK, "OJ", Guid.NewGuid(), "TDR")));
			AssertExceptionThrownSafe("Invalid Parent Organisation PK.", new Action<DbConnection>((connection) => UpdateERTJobConfig(connection, CompanyPK, "OH", Guid.NewGuid(), "TDR")));
		}

		public void TestTriggerForTESInsert()
		{
			AssertExceptionThrownSafe("Incorrect XSLT Template Code for a XSLT Template Configuration.", new Action<DbConnection>((connection) => InsertTESJobConfig(connection, Guid.Empty, "", Guid.Empty, "***")));
			AssertExceptionThrownSafe("Invalid Parent Branch PK.", new Action<DbConnection>((connection) => InsertTESJobConfig(connection, CompanyPK, "GB", Guid.NewGuid(), "AIR", createTemplateFileRecord: true)));
			AssertExceptionThrownSafe("Invalid Parent Organisation PK.", new Action<DbConnection>((connection) => InsertTESJobConfig(connection, CompanyPK, "OH", Guid.NewGuid(), "LND", createTemplateFileRecord: true)));

			AssertExceptionThrownSafe("Incorrect XSLT Template Code for a XSLT Template Configuration.",
				new Action<DbConnection>(
					  (connection) =>
					  {
						  InsertTESJobConfig(connection, CompanyPK, "", Guid.Empty, "ART");
						  InsertTESJobConfig(connection, CompanyPK, "", Guid.Empty, "ART");
					  }));
		}

		public void TestTriggerForTESUpdate()
		{
			var testPK = Guid.Empty;
			AssertExceptionThrownSafe("Incorrect XSLT Template Code for a XSLT Template Configuration.",
				new Action<DbConnection>(
					  (connection) =>
					  {
						  testPK = InsertTESJobConfig(connection, CompanyPK, "", Guid.Empty, "TST", createTemplateFileRecord: true);
						  UpdateTESJobConfig(connection, Guid.Empty, "", Guid.Empty, "***", testPK);
					  }));

			AssertExceptionThrownSafe("Invalid Parent Branch PK.",
				new Action<DbConnection>(
					  (connection) =>
					  {
						  testPK = InsertTESJobConfig(connection, CompanyPK, "", Guid.Empty, "S01", createTemplateFileRecord: true);
						  UpdateTESJobConfig(connection, CompanyPK, "GB", Guid.NewGuid(), "S01", testPK);
					  }));

			AssertExceptionThrownSafe("Invalid Parent Organisation PK.",
				new Action<DbConnection>(
					  (connection) =>
					  {
						  testPK = InsertTESJobConfig(connection, CompanyPK, "", Guid.Empty, "S02", createTemplateFileRecord: true);
						  UpdateTESJobConfig(connection, CompanyPK, "OH", Guid.NewGuid(), "S02", testPK);
					  }));

			AssertExceptionThrownSafe("Incorrect XSLT Template Code for a XSLT Template Configuration.",
				new Action<DbConnection>(
					  (connection) =>
					  {
						  var testPK1 = InsertTESJobConfig(connection, CompanyPK, "", Guid.Empty, "ART", ledger: LedgerTypeCodes.AccountsReceivable);
						  var testPK2 = InsertTESJobConfig(connection, CompanyPK, "", Guid.Empty, "ART", ledger: LedgerTypeCodes.AccountsPayable);
						  UpdateTESJobConfig(connection, CompanyPK, "", Guid.Empty, "ART", testPK2, ledger: LedgerTypeCodes.AccountsReceivable);
					  }));

			AssertExceptionThrownSafe("Incorrect XSLT Template Code for a XSLT Template Configuration.",
				new Action<DbConnection>(
					  (connection) =>
					  {
						  TestDbHelper dbHelper = new TestDbHelper(connection);
						  var testCompanyPK = dbHelper.InsertCompany("TST", "Test Company",
							Enterprise.Core.Constants.CurrencyCodes.Turkey, Enterprise.Core.Constants.CountryCodes.Turkey, false, false);

						  var testPK1 = InsertTESJobConfig(connection, testCompanyPK, "", Guid.Empty, "ART");
						  var testPK2 = InsertTESJobConfig(connection, CompanyPK, "", Guid.Empty, "ART");
						  UpdateTESJobConfig(connection, testCompanyPK, "", Guid.Empty, "ART", testPK2);
					  }));
		}

		#region Place Of Supply

		public void TestTriggerForPOSInsert() => TestTriggerForPOS(InsertOrUpdate.Insert);

		public void TestTriggerForPOSUpdate() => TestTriggerForPOS(InsertOrUpdate.Update);

		void TestTriggerForPOS(InsertOrUpdate insertOrUpdate)
		{
			var defaultBranchPk = TestHelper.InsertBranch("B01", TestDbHelper.DefaultCompanyPK);
			var otherCompanyPk = TestHelper.InsertCompany("C01", "Some Company", "USD", "US", true, true);
			var otherCompanyBranchPk = TestHelper.InsertBranch("B02", otherCompanyPk);
			var placeOfSupplyGroupPk = TestHelper.InsertAccGroup("POS", code: "POSTEST");
			var globalChargeCodePk = TestHelper.InsertChargeCode(null, "GLBCODE");
			var companyChargeCodePk = TestHelper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "TSTCODE");
			var otherCompanyChargeCodePk = TestHelper.InsertChargeCode(otherCompanyPk, "OTHCODE");

			// Note that negative tests must come BEFORE positive tests. The DELETEs hold locks which prevent failures cases running.
			// DELETE is required due to unique index.
			AssertExceptionThrownSafe("Incorrect Job Type for Place Of Supply configuration.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "XYZ"));

			AssertExceptionThrownSafe("Incorrect Service Direction for Place Of Supply configuration.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, serviceDirection: "LIN"));
			AssertExceptionThrownSafe("Incorrect Service Direction for Place Of Supply configuration.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, serviceDirection: "LOC"));
			AssertExceptionThrownSafe("Incorrect Service Direction for Place Of Supply configuration.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, serviceDirection: "DST"));
			AssertExceptionThrownSafe("Incorrect Service Direction for Place Of Supply configuration.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, serviceDirection: "ORG"));

			AssertExceptionThrownSafe("Incorrect Tax Registration Type for Place Of Supply configuration.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, taxRegistrationType: "XYZ"));

			AssertExceptionThrownSafe("Invalid Branch Code for Place Of Supply configuration.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, branchCode: "B99"));

			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"ALL\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "ALL", placeOfSupplyRule: "XYZ"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"ALL\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "ALL", placeOfSupplyRule: "DLV"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"SHP\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "XYZ"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"FCN\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "FCN", placeOfSupplyRule: "XYZ"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"FCN\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "FCN", placeOfSupplyRule: "DLV"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"BRK\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "XYZ"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"BRK\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "CPL"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"QSH\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "XYZ"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"QSH\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "CSA"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"WKI\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "WKI", placeOfSupplyRule: "XYZ"));
			AssertExceptionThrownSafe("Incorrect Place Of Supply Rule Code for Job Type \"WKI\".", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, jobType: "WKI", placeOfSupplyRule: "CSA"));

			AssertExceptionThrownSafe("Invalid Parent Charge Code PK.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, parentPrefix: "AC", parentPK: Guid.NewGuid()));
			AssertExceptionThrownSafe("Invalid Parent Charge Code Group PK.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, parentPrefix: "GRO", parentPK: Guid.NewGuid()));
			AssertExceptionThrownSafe("Missing Parent Company PK.", (connection) => InsertOrUpdatePOSJobConfigAndCleanUp(connection, insertOrUpdate, companyPK: Guid.Empty));

			AssertNoExceptionThrown("JobType ALL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "ALL"));
			AssertNoExceptionThrown("JobType SHP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP"));
			AssertNoExceptionThrown("JobType FCN", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "FCN"));
			AssertNoExceptionThrown("JobType BRK", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK"));
			AssertNoExceptionThrown("JobType QSH", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH"));
			AssertNoExceptionThrown("JobType WKI", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "WKI"));

			AssertNoExceptionThrown("ServiceDirection ALL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, serviceDirection: "ALL"));
			AssertNoExceptionThrown("ServiceDirection IMP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, serviceDirection: "IMP"));
			AssertNoExceptionThrown("ServiceDirection EXP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, serviceDirection: "EXP"));
			AssertNoExceptionThrown("ServiceDirection DOM", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, serviceDirection: "DOM"));
			AssertNoExceptionThrown("ServiceDirection OTH", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, serviceDirection: "OTH"));

			AssertNoExceptionThrown("Transport Mode ALL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, transportMode: Constants.TransportModes.All));
			AssertNoExceptionThrown("Transport Mode AIR", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, transportMode: Constants.TransportModes.Air));
			AssertNoExceptionThrown("Transport Mode SEA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, transportMode: Constants.TransportModes.Sea));
			AssertNoExceptionThrown("Transport Mode FSA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, transportMode: Constants.TransportModes.SeaAir));
			AssertNoExceptionThrown("Transport Mode FAS", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, transportMode: Constants.TransportModes.AirSea));
			AssertNoExceptionThrown("Transport Mode ROA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, transportMode: Constants.TransportModes.Road));
			AssertNoExceptionThrown("Transport Mode RAI", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, transportMode: Constants.TransportModes.Rail));
			AssertNoExceptionThrown("Transport Mode COU", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, transportMode: Constants.TransportModes.Courier));

			AssertNoExceptionThrown("TaxRegistrationType ''", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, taxRegistrationType: ""));
			AssertNoExceptionThrown("TaxRegistrationType NON", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, taxRegistrationType: "NON"));
			AssertNoExceptionThrown("TaxRegistrationType FRO", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, taxRegistrationType: "FRO"));
			AssertNoExceptionThrown("TaxRegistrationType LOC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, taxRegistrationType: "LOC"));

			AssertNoExceptionThrown("Branch B01", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, branchCode: "B01"));
			AssertNoExceptionThrown("Branch B01 in wrong company", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, branchCode: "B01", companyPK: otherCompanyPk));        // Trigger does not validate if branch belongs to company
			AssertNoExceptionThrown("Branch B02", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, branchCode: "B02", companyPK: otherCompanyPk));

			// ALL - ('BIL','SUP', 'OTR')
			AssertNoExceptionThrown("JobType ALL, POS Rule BIL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "ALL", placeOfSupplyRule: "BIL"));
			AssertNoExceptionThrown("JobType ALL, POS Rule SUP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "ALL", placeOfSupplyRule: "SUP"));
			AssertNoExceptionThrown("JobType ALL, POS Rule OTR", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "ALL", placeOfSupplyRule: "OTR"));

			// SHP - ('BIL','SUP','OTR','PCF','DCF','PLC','DLV','PCA','DLA','ORI','DES','PTW','DTW','FPC','LPC','CPL','CPD','CSA','CRA')
			AssertNoExceptionThrown("JobType SHP, POS Rule BIL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "BIL"));
			AssertNoExceptionThrown("JobType SHP, POS Rule SUP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "SUP"));
			AssertNoExceptionThrown("JobType SHP, POS Rule OTR", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "OTR"));
			AssertNoExceptionThrown("JobType SHP, POS Rule PCF", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "PCF"));
			AssertNoExceptionThrown("JobType SHP, POS Rule DCF", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "DCF"));
			AssertNoExceptionThrown("JobType SHP, POS Rule PLC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "PLC"));
			AssertNoExceptionThrown("JobType SHP, POS Rule DLV", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "DLV"));
			AssertNoExceptionThrown("JobType SHP, POS Rule PCA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "PCA"));
			AssertNoExceptionThrown("JobType SHP, POS Rule DLA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "DLA"));
			AssertNoExceptionThrown("JobType SHP, POS Rule ORI", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "ORI"));
			AssertNoExceptionThrown("JobType SHP, POS Rule DES", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "DES"));
			AssertNoExceptionThrown("JobType SHP, POS Rule PTW", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "PTW"));
			AssertNoExceptionThrown("JobType SHP, POS Rule DTW", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "DTW"));
			AssertNoExceptionThrown("JobType SHP, POS Rule FPC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "FPC"));
			AssertNoExceptionThrown("JobType SHP, POS Rule LPC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "LPC"));
			AssertNoExceptionThrown("JobType SHP, POS Rule CPL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "CPL"));
			AssertNoExceptionThrown("JobType SHP, POS Rule CPD", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "CPD"));
			AssertNoExceptionThrown("JobType SHP, POS Rule CSA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "CSA"));
			AssertNoExceptionThrown("JobType SHP, POS Rule CRA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "SHP", placeOfSupplyRule: "CRA"));

			// FCN - ('BIL','SUP','OTR','CPL','CPD','CSA','CRA','PCF','DCF','PCT','DCT')
			AssertNoExceptionThrown("JobType FCN, POS Rule BIL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "FCN", placeOfSupplyRule: "BIL"));
			AssertNoExceptionThrown("JobType FCN, POS Rule SUP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "FCN", placeOfSupplyRule: "SUP"));
			AssertNoExceptionThrown("JobType FCN, POS Rule OTR", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "FCN", placeOfSupplyRule: "OTR"));
			AssertNoExceptionThrown("JobType FCN, POS Rule CPL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "FCN", placeOfSupplyRule: "CPL"));
			AssertNoExceptionThrown("JobType FCN, POS Rule CPD", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "FCN", placeOfSupplyRule: "CPD"));
			AssertNoExceptionThrown("JobType FCN, POS Rule CSA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "FCN", placeOfSupplyRule: "CSA"));
			AssertNoExceptionThrown("JobType FCN, POS Rule CRA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "FCN", placeOfSupplyRule: "CRA"));

			// BRK - ('BIL','SUP','OTR','PLO','PDI','PFA','POR','PFD','FPC','LPC','PLC','DLV')
			AssertNoExceptionThrown("JobType BRK, POS Rule BIL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "BIL"));
			AssertNoExceptionThrown("JobType BRK, POS Rule SUP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "SUP"));
			AssertNoExceptionThrown("JobType BRK, POS Rule OTR", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "OTR"));
			AssertNoExceptionThrown("JobType BRK, POS Rule PLO", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "PLO"));
			AssertNoExceptionThrown("JobType BRK, POS Rule PDI", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "PDI"));
			AssertNoExceptionThrown("JobType BRK, POS Rule PFA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "PFA"));
			AssertNoExceptionThrown("JobType BRK, POS Rule POR", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "POR"));
			AssertNoExceptionThrown("JobType BRK, POS Rule PFD", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "PFD"));
			AssertNoExceptionThrown("JobType BRK, POS Rule FPC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "FPC"));
			AssertNoExceptionThrown("JobType BRK, POS Rule LPC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "LPC"));
			AssertNoExceptionThrown("JobType BRK, POS Rule PLC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "PLC"));
			AssertNoExceptionThrown("JobType BRK, POS Rule DLV", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "BRK", placeOfSupplyRule: "DLV"));

			// QSH - ('BIL','SUP','OTR','ORI','DES','PLC','PCF','DCF','LOA','DIS','PCT','DCT')
			AssertNoExceptionThrown("JobType QSH, POS Rule BIL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "BIL"));
			AssertNoExceptionThrown("JobType QSH, POS Rule SUP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "SUP"));
			AssertNoExceptionThrown("JobType QSH, POS Rule OTR", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "OTR"));
			AssertNoExceptionThrown("JobType QSH, POS Rule ORI", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "ORI"));
			AssertNoExceptionThrown("JobType QSH, POS Rule DES", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "DES"));
			AssertNoExceptionThrown("JobType QSH, POS Rule PLC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "PLC"));
			AssertNoExceptionThrown("JobType QSH, POS Rule PCF", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "PCF"));
			AssertNoExceptionThrown("JobType QSH, POS Rule DCF", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "DCF"));
			AssertNoExceptionThrown("JobType QSH, POS Rule LOA", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "LOA"));
			AssertNoExceptionThrown("JobType QSH, POS Rule DIS", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "DIS"));
			AssertNoExceptionThrown("JobType QSH, POS Rule PCT", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "PCT"));
			AssertNoExceptionThrown("JobType QSH, POS Rule DCT", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "QSH", placeOfSupplyRule: "DCT"));

			// WKI - ('BIL','SUP','OTR','CRP')
			AssertNoExceptionThrown("JobType WKI, POS Rule BIL", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "WKI", placeOfSupplyRule: "BIL"));
			AssertNoExceptionThrown("JobType WKI, POS Rule SUP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "WKI", placeOfSupplyRule: "SUP"));
			AssertNoExceptionThrown("JobType WKI, POS Rule OTR", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "WKI", placeOfSupplyRule: "OTR"));
			AssertNoExceptionThrown("JobType WKI, POS Rule CRP", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, jobType: "WKI", placeOfSupplyRule: "CRP"));

			AssertNoExceptionThrown("Parent AC, global charge code", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, parentPrefix: "AC", parentPK: globalChargeCodePk));
			AssertNoExceptionThrown("Parent AC, normal charge code", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, parentPrefix: "AC", parentPK: companyChargeCodePk));
			AssertNoExceptionThrown("Parent GRO", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, parentPrefix: "GRO", parentPK: placeOfSupplyGroupPk));

			AssertNoExceptionThrown("Default GC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate));
			AssertNoExceptionThrown("Other GC", () => InsertOrUpdatePOSJobConfigAndCleanUp(TestConnection, insertOrUpdate, companyPK: otherCompanyPk));
		}

		#endregion

		#region Cash Advance Defaulting

		public void TestTriggerForCashAdvanceDefaultingConfigInsert() => TestTriggerForCashAdvanceDefaultingConfig(InsertOrUpdate.Insert);

		public void TestTriggerForCashAdvanceDefaultingConfigUpdate() => TestTriggerForCashAdvanceDefaultingConfig(InsertOrUpdate.Update);

		void TestTriggerForCashAdvanceDefaultingConfig(InsertOrUpdate insertOrUpdate)
		{
			AssertExceptionThrownSafe("Missing Parent Company PK.", (connection) => InsertOrUpdateCashAdvanceDefaultingJobConfigAndCleanUp(connection, insertOrUpdate, companyPK: Guid.Empty));

			AssertExceptionThrownSafe("Incorrect Defaulting Option for Cash Advance Defaulting configuration.", (connection) => InsertOrUpdateCashAdvanceDefaultingJobConfigAndCleanUp(connection, insertOrUpdate, defaultingOption: "ZZZ"));

			AssertNoExceptionThrown("Parent Company PK not missing.", () => InsertOrUpdateCashAdvanceDefaultingJobConfigAndCleanUp(TestConnection, insertOrUpdate, companyPK: CompanyPK));

			AssertNoExceptionThrown("Defaulting Option ALL", () => InsertOrUpdateCashAdvanceDefaultingJobConfigAndCleanUp(TestConnection, insertOrUpdate, defaultingOption: "ALL"));
			AssertNoExceptionThrown("Defaulting Option NON", () => InsertOrUpdateCashAdvanceDefaultingJobConfigAndCleanUp(TestConnection, insertOrUpdate, defaultingOption: "NON"));
			AssertNoExceptionThrown("Defaulting Option INC", () => InsertOrUpdateCashAdvanceDefaultingJobConfigAndCleanUp(TestConnection, insertOrUpdate, defaultingOption: "INC"));
		}

		#endregion

		#region Helpers

		void AssertExceptionThrownSafe(string expectedExceptionMessage, Action<DbConnection> codeToRun)
		{
			using (var newConnection = Db.NewExtraConnectionToMainDb())
			{
				newConnection.BeginTransaction();
				AssertExceptionThrown<SqlException>("Should be: " + expectedExceptionMessage, expectedExceptionMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", delegate
				{ codeToRun(newConnection); });
				newConnection.RollbackTransaction();
			}
		}

		Guid InsertCFXJobConfig(DbConnection connection, Guid companyPK, string parentPrefix, Guid parentPK, string currencyCode, string ledger = "AR")
		{
			Guid pk = Guid.NewGuid();
			string sql = @"INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser) 
VALUES(@PK, @ConfigType, @GC, @Ledger, @ParentPrefix, @ParentPK, @JobType, @Direction, @Mode, @Code, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (DbCommand cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "CFX");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK == Guid.Empty ? DBNull.Value : companyPK);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK == Guid.Empty ? DBNull.Value : parentPK);
				cmd.AddParameter("@JobType", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Direction", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Mode", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Code", SqlDbType.Char, currencyCode);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		void UpdateCFXJobConfig(DbConnection connection, Guid companyPK, string parentPrefix, Guid parentPK, string currencyCode, string ledger = "AR")
		{
			var pk = Guid.Empty;
			try
			{
				pk = InsertCFXJobConfig(connection, TestDbHelper.DefaultCompanyPK, "", Guid.Empty, "");
			}
			catch (SqlException ex)
			{
				Fail("Unexpected exception on inserting: " + ex.Message);
			}

			string sql = @"UPDATE dbo.AccJobConfig SET JCF_ConfigType = @ConfigType, JCF_GC = @GC, JCF_Ledger = @Ledger, JCF_ParentTableCode = @ParentPrefix, JCF_ParentId = @ParentPK, JCF_Code = @Code, JCF_SystemLastEditTimeUtc = GETUTCDATE(), JCF_SystemLastEditUser = 'TST'
WHERE JCF_PK = @PK";

			using (DbCommand cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "CFX");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK == Guid.Empty ? DBNull.Value : companyPK);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK == Guid.Empty ? DBNull.Value : parentPK);
				cmd.AddParameter("@Code", SqlDbType.Char, currencyCode);
				cmd.ExecuteNonQuery();
			}
		}

		Guid InsertERTJobConfig(DbConnection connection, Guid companyPK, string parentPrefix, Guid parentPK, string pereferenceCode)
		{
			Guid pk = Guid.NewGuid();
			string sql = @"INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code2, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser) 
VALUES(@PK, @ConfigType, @GC, @Ledger, @ParentPrefix, @ParentPK, @JobType, @Direction, @Mode, @Code2, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (DbCommand cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "ERT");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK == Guid.Empty ? DBNull.Value : companyPK);
				cmd.AddParameter("@Ledger", SqlDbType.Char, "AR");
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK == Guid.Empty ? DBNull.Value : parentPK);
				cmd.AddParameter("@JobType", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Direction", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Mode", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Code2", SqlDbType.Char, pereferenceCode);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		Guid UpdateERTJobConfig(DbConnection connection, Guid companyPK, string parentPrefix, Guid parentPK, string pereferenceCode)
		{
			var pk = Guid.Empty;
			try
			{
				pk = InsertERTJobConfig(connection, TestDbHelper.DefaultCompanyPK, "", Guid.Empty, "TDR");
			}
			catch (SqlException ex)
			{
				Fail("Unexpected exception on inserting: " + ex.Message);
			}

			string sql = @"UPDATE dbo.AccJobConfig SET JCF_ConfigType = @ConfigType, JCF_GC = @GC, JCF_Ledger = @Ledger, JCF_ParentTableCode = @ParentPrefix, JCF_ParentId = @ParentPK, JCF_Code2 = @Code2, JCF_SystemLastEditTimeUtc = GETUTCDATE(), JCF_SystemLastEditUser = 'TST'
WHERE JCF_PK = @PK";

			using (DbCommand cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "ERT");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK == Guid.Empty ? DBNull.Value : companyPK);
				cmd.AddParameter("@Ledger", SqlDbType.Char, "AR");
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK == Guid.Empty ? DBNull.Value : parentPK);
				cmd.AddParameter("@JobType", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Direction", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Mode", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Code2", SqlDbType.Char, pereferenceCode);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		Guid InsertTESJobConfig(DbConnection connection, Guid companyPK, string parentPrefix, Guid parentPK, string templateCode, string ledger = LedgerTypeCodes.AccountsReceivable, bool createTemplateFileRecord = false)
		{
			if (createTemplateFileRecord)
			{
				InsertTemplateFile(connection, companyPK, templateCode, ledger);
			}

			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser) 
VALUES(@PK, @ConfigType, @GC, @Ledger, @ParentPrefix, @ParentPK, @JobType, @Direction, @Mode, @Code, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "TES");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK == Guid.Empty ? DBNull.Value : companyPK);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK == Guid.Empty ? DBNull.Value : parentPK);
				cmd.AddParameter("@JobType", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Direction", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Mode", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Code", SqlDbType.Char, templateCode);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		void UpdateTESJobConfig(DbConnection connection, Guid companyPK, string parentPrefix, Guid parentPK, string templateCode, Guid configPK, string ledger = LedgerTypeCodes.AccountsReceivable)
		{
			var sql = @"UPDATE dbo.AccJobConfig SET JCF_ConfigType = @ConfigType, JCF_GC = @GC, JCF_Ledger = @Ledger, JCF_ParentTableCode = @ParentPrefix, JCF_ParentId = @ParentPK, JCF_JobType = @JobType, JCF_Code = @Code,
							JCF_TransportMode = @Mode, JCF_SystemLastEditTimeUtc = GETUTCDATE(), JCF_SystemLastEditUser = 'TST' WHERE JCF_PK = @PK";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, configPK);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "TES");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK == Guid.Empty ? DBNull.Value : companyPK);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK == Guid.Empty ? DBNull.Value : parentPK);
				cmd.AddParameter("@JobType", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Mode", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Code", SqlDbType.Char, templateCode);
				cmd.ExecuteNonQuery();
			}
		}

		void InsertTemplateFile(DbConnection connection, Guid companyPK, string templateCode, string ledger = LedgerTypeCodes.AccountsReceivable)
		{
			var pk = Guid.NewGuid();
			var testValue = "This is a test value.";
			var fileData = Encoding.UTF8.GetBytes(testValue);

			var sql = @"INSERT INTO dbo.AccTemplateFileStorage (TFS_PK, TFS_GC, TFS_Code, TFS_Ledger, TFS_Description, TFS_FileName, TFS_IsActive, TFS_FileData, TFS_SystemCreateTimeUtc, TFS_SystemCreateUser, TFS_SystemLastEditTimeUtc, TFS_SystemLastEditUser) 
														VALUES(@PK, @GC, @Code, @Ledger, @Description, @FileName, @IsActive, @FileData, @SystemCreateTime, @SystemCreateUser, @SystemLastEditTime, @SystemLastEditUser)";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK == Guid.Empty ? DBNull.Value : companyPK);
				cmd.AddParameter("@Code", SqlDbType.Char, templateCode);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@Description", SqlDbType.VarChar, "Test");
				cmd.AddParameter("@FileName", SqlDbType.VarChar, "TestFile.xslt");
				cmd.AddParameter("@IsActive", SqlDbType.Bit, 1);
				cmd.AddParameter("@FileData", SqlDbType.VarBinary, fileData);
				cmd.AddParameter("@SystemCreateTime", SqlDbType.SmallDateTime, DateTime.UtcNow);
				cmd.AddParameter("@SystemCreateUser", SqlDbType.Char, "~BP");
				cmd.AddParameter("@SystemLastEditTime", SqlDbType.SmallDateTime, DateTime.UtcNow);
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.Char, "~BP");
				cmd.ExecuteNonQuery();
			}
		}

		static Guid InsertPOSJobConfig(DbConnection connection, Guid? companyPK = null, string parentPrefix = "", Guid? parentPK = null, string ledger = "AR", string jobType = "ALL", string incoTerm = "", string serviceDirection = "ALL", string transportMode = "ALL", string taxRegistrationType = "", string branchCode = "", string supplyType = "", string placeOfSupplyRule = "BIL")
		{
			var gc_pk = companyPK ?? TestDbHelper.DefaultCompanyPK;

			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_IncoTerm, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_Code2, JCF_SupplyType, JCF_Code3, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser) 
VALUES(@PK, @ConfigType, @GC, @Ledger, @ParentPrefix, @ParentPK, @JobType, @IncoTerm, @Direction, @Mode, @Code, @Code2, @SupplyType, @Code3, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "POS");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, gc_pk == Guid.Empty ? DBNull.Value : gc_pk);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK ?? (object)DBNull.Value);
				cmd.AddParameter("@JobType", SqlDbType.Char, jobType);
				cmd.AddParameter("@IncoTerm", SqlDbType.Char, incoTerm);
				cmd.AddParameter("@Direction", SqlDbType.Char, serviceDirection);
				cmd.AddParameter("@Mode", SqlDbType.Char, transportMode);
				cmd.AddParameter("@Code", SqlDbType.VarChar, taxRegistrationType);
				cmd.AddParameter("@Code2", SqlDbType.VarChar, branchCode);
				cmd.AddParameter("@SupplyType", SqlDbType.VarChar, supplyType);
				cmd.AddParameter("@Code3", SqlDbType.VarChar, placeOfSupplyRule);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		static void UpdatePOSJobConfig(DbConnection connection, Guid pk, Guid? companyPK = null, string parentPrefix = "", Guid? parentPK = null, string ledger = "AR", string jobType = "ALL", string incoTerm = "", string serviceDirection = "ALL", string transportMode = "ALL", string taxRegistrationType = "", string branchCode = "", string supplyType = "", string placeOfSupplyRule = "BIL")
		{
			var gc_pk = companyPK ?? TestDbHelper.DefaultCompanyPK;

			var sql = @"UPDATE dbo.AccJobConfig
						SET JCF_GC = @GC,
							JCF_Ledger = @Ledger,
							JCF_ParentTableCode = @ParentPrefix,
							JCF_ParentId = @ParentPK,
							JCF_JobType = @JobType,
							JCF_IncoTerm =@IncoTerm,
							JCF_ServiceDirection = @Direction,
							JCF_TransportMode = @Mode,
							JCF_Code = @Code,
							JCF_Code2 = @Code2,
							JCF_SupplyType = @SupplyType,
							JCF_Code3 = @Code3,
							JCF_SystemLastEditTimeUtc = GETUTCDATE(),
							JCF_SystemLastEditUser = 'TST'
						WHERE JCF_PK = @PK";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, gc_pk == Guid.Empty ? DBNull.Value : gc_pk);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK ?? (object)DBNull.Value);
				cmd.AddParameter("@JobType", SqlDbType.Char, jobType);
				cmd.AddParameter("@IncoTerm", SqlDbType.Char, incoTerm);
				cmd.AddParameter("@Direction", SqlDbType.Char, serviceDirection);
				cmd.AddParameter("@Mode", SqlDbType.Char, transportMode);
				cmd.AddParameter("@Code", SqlDbType.VarChar, taxRegistrationType);
				cmd.AddParameter("@Code2", SqlDbType.VarChar, branchCode);
				cmd.AddParameter("@SupplyType", SqlDbType.VarChar, supplyType);
				cmd.AddParameter("@Code3", SqlDbType.VarChar, placeOfSupplyRule);
				cmd.ExecuteNonQuery();
			}
		}

		static void InsertOrUpdatePOSJobConfigAndCleanUp(DbConnection connection, InsertOrUpdate insertOrUpdate, Guid? companyPK = null, string parentPrefix = "", Guid? parentPK = null, string ledger = "AR", string jobType = "ALL", string incoTerm = "", string serviceDirection = "ALL", string transportMode = "ALL", string taxRegistrationType = "", string branchCode = "", string supplyType = "", string placeOfSupplyRule = "BIL")
		{
			var pk = Guid.Empty;
			if (insertOrUpdate == InsertOrUpdate.Insert)
			{
				pk = InsertPOSJobConfig(
					connection,
					companyPK: companyPK,
					parentPrefix: parentPrefix,
					parentPK: parentPK,
					ledger: ledger,
					jobType: jobType,
					incoTerm: incoTerm,
					serviceDirection: serviceDirection,
					transportMode: transportMode,
					taxRegistrationType: taxRegistrationType,
					branchCode: branchCode,
					supplyType: supplyType,
					placeOfSupplyRule: placeOfSupplyRule);
				AssertNotEquals("Insert should create PK", Guid.Empty, pk);
			}
			else
			{
				AssertNoExceptionThrown("Initial insert should always succeed when testing Update", () => pk = InsertPOSJobConfig(connection));
				AssertNotEquals("Initial insert should assign PK", Guid.Empty, pk);

				UpdatePOSJobConfig(
					connection,
					pk,
					companyPK: companyPK,
					parentPrefix: parentPrefix,
					parentPK: parentPK,
					ledger: ledger,
					jobType: jobType,
					incoTerm: incoTerm,
					serviceDirection: serviceDirection,
					transportMode: transportMode,
					taxRegistrationType: taxRegistrationType,
					branchCode: branchCode,
					supplyType: supplyType,
					placeOfSupplyRule: placeOfSupplyRule);
			}

			connection.ExecuteNonQuery($"DELETE FROM dbo.AccJobConfig WHERE JCF_PK = '{pk}'");
		}

		static void InsertOrUpdateCashAdvanceDefaultingJobConfigAndCleanUp(DbConnection connection, InsertOrUpdate insertOrUpdate, Guid? companyPK = null, string parentPrefix = "", Guid? parentPK = null, string ledger = "AR", string jobType = "ALL", string serviceDirection = "ALL", string transportMode = "ALL", string defaultingOption = "ALL")
		{
			var pk = Guid.Empty;
			if (insertOrUpdate == InsertOrUpdate.Insert)
			{
				pk = InsertCashAdvanceDefaultingJobConfig(
					connection,
					companyPK: companyPK,
					parentPrefix: parentPrefix,
					parentPK: parentPK,
					ledger: ledger,
					jobType: jobType,
					serviceDirection: serviceDirection,
					transportMode: transportMode,
					defaultingOption: defaultingOption
				);
				AssertNotEquals("Insert should create PK", Guid.Empty, pk);
			}
			else
			{
				AssertNoExceptionThrown("Initial insert should always succeed when testing Update", () => pk = InsertCashAdvanceDefaultingJobConfig(connection));
				AssertNotEquals("Initial insert should assign PK", Guid.Empty, pk);

				UpdateCashAdvanceDefaultingJobConfig(
					connection,
					pk,
					companyPK: companyPK,
					parentPrefix: parentPrefix,
					parentPK: parentPK,
					ledger: ledger,
					jobType: jobType,
					serviceDirection: serviceDirection,
					transportMode: transportMode,
					defaultingOption: defaultingOption);
			}

			connection.ExecuteNonQuery($"DELETE FROM dbo.AccJobConfig WHERE JCF_PK = '{pk}'");
		}

		static Guid InsertCashAdvanceDefaultingJobConfig(DbConnection connection, Guid? companyPK = null, string parentPrefix = "", Guid? parentPK = null, string ledger = "AR", string jobType = "ALL", string serviceDirection = "ALL", string transportMode = "ALL", string defaultingOption = "ALL")
		{
			var gc_pk = companyPK ?? TestDbHelper.DefaultCompanyPK;

			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_Code2, JCF_Code3, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser) 
VALUES(@PK, @ConfigType, @GC, @Ledger, @ParentPrefix, @ParentPK, @JobType, @Direction, @Mode, @Code, '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "CAD");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, gc_pk == Guid.Empty ? DBNull.Value : gc_pk);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK ?? (object)DBNull.Value);
				cmd.AddParameter("@JobType", SqlDbType.Char, jobType);
				cmd.AddParameter("@Direction", SqlDbType.Char, serviceDirection);
				cmd.AddParameter("@Mode", SqlDbType.Char, transportMode);
				cmd.AddParameter("@Code", SqlDbType.VarChar, defaultingOption);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		static void UpdateCashAdvanceDefaultingJobConfig(DbConnection connection, Guid pk, Guid? companyPK = null, string parentPrefix = "", Guid? parentPK = null, string ledger = "AR", string jobType = "ALL", string serviceDirection = "ALL", string transportMode = "ALL", string defaultingOption = "ALL")
		{
			var gc_pk = companyPK ?? TestDbHelper.DefaultCompanyPK;

			var sql = @"UPDATE dbo.AccJobConfig
						SET JCF_GC = @GC,
							JCF_Ledger = @Ledger,
							JCF_ParentTableCode = @ParentPrefix,
							JCF_ParentId = @ParentPK,
							JCF_JobType = @JobType,
							JCF_ServiceDirection = @Direction,
							JCF_TransportMode = @Mode,
							JCF_Code = @Code,
							JCF_SystemLastEditTimeUtc = GETUTCDATE(),
							JCF_SystemLastEditUser = 'TST'
						WHERE JCF_PK = @PK";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, gc_pk == Guid.Empty ? DBNull.Value : gc_pk);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK ?? (object)DBNull.Value);
				cmd.AddParameter("@JobType", SqlDbType.Char, jobType);
				cmd.AddParameter("@Direction", SqlDbType.Char, serviceDirection);
				cmd.AddParameter("@Mode", SqlDbType.Char, transportMode);
				cmd.AddParameter("@Code", SqlDbType.VarChar, defaultingOption);
				cmd.ExecuteNonQuery();
			}
		}

		enum InsertOrUpdate
		{
			Insert = 1,
			Update
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper = new TestDbHelper(TestConnection);
			CompanyPK = TestDbHelper.DefaultCompanyPK;
		}

		TestDbHelper TestHelper;
		Guid CompanyPK;
	}
}

