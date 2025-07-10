using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_TaxTransactionAnalysisbyTransLineTest : ScriptTest
	{
		#region Brexit

		[TestDate(2019, 03, 19)]
		public void TestRunOnBrexitDate_TransactionCountry()
		{
			var brexitDate = TestObjectCreator.SetupPostBrexitData();
			var orgHeader = TestObjectCreator.Creditor1;
			SetupHeaderWithClosestPortAndCustomsCode(orgHeader, "DE25B", Core.Constants.CountryCodes.Germany, "111111_DE", Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Germany));

			CreateInvoicesForBrexit(brexitDate, orgHeader);

			Factory.Save();

			AssertTransactionRegNoForBrexit(brexitDate, Core.Constants.CountryCodes.UnitedKingdom, "DE111111_DE");
		}

		[TestDate(2019, 03, 19)]
		public void TestRunBeforeBrexitDate_OrgCountry()
		{
			var brexitDate = TestObjectCreator.SetupPostBrexitData();
			var orgHeader = TestObjectCreator.Creditor1;
			SetupHeaderWithClosestPortAndCustomsCode(orgHeader, "GBLON", Core.Constants.CountryCodes.UnitedKingdom, "111111_GB", Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.UnitedKingdom));

			CreateInvoicesForBrexit(brexitDate, orgHeader);

			Factory.Save();

			AssertTransactionRegNoForBrexit(brexitDate, Core.Constants.CountryCodes.Germany, "GB111111_GB");
		}

		void CreateInvoicesForBrexit(ZDateTime brexitDate, OrgHeader orgHeader)
		{
			TestObjectCreator.SetupCashBasisVAT();

			CreateInvoice("001A", brexitDate.AddDays(-1), AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code);

			var invoice = CreateInvoice("001C", brexitDate.AddDays(-1), AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code);
			TestHelper.SetupCashVATLine(invoice, TestObjectCreator.GST1.PK, 20);

			CreateInvoice("002A", brexitDate, AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code);

			invoice = CreateInvoice("002C", brexitDate, AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code);
			TestHelper.SetupCashVATLine(invoice, TestObjectCreator.GST1.PK, 20);

			InvoicingBase CreateInvoice(string invoiceNumber, ZDateTime postDate, string vatBasis)
			{
				var inv = TestObjectCreator.CreateInvoice(typeof(APInvoice), invoiceNumber, TestObjectCreator.GBP, organisation: orgHeader);
				inv.AH_PostDate = postDate;
				var line = TestObjectCreator.CreateInvoiceLine(inv, TestObjectCreator.GLHeader1.PK, 100M);
				line.AL_GSTVATBasis = vatBasis;

				return inv;
			}
		}

		void AssertTransactionRegNoForBrexit(ZDateTime transactionPostDate, string countryCodeToTemporarilySwitch, string transactionRegNoToAssert)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeToTemporarilySwitch))
			{
				var headers = new[] { "AH_TransactionNum", "OK_CustomsRegNo" };

				var result = RunScript(LedgerTypes.AccountsPayable, transactionPostDate: transactionPostDate);

				var lines = new[] {
					new object[] { "001A", transactionRegNoToAssert },
					new object[] { "002A", string.Empty },
				};

				AssertDataTableAllRowsByKeyColumns("", result, headers, lines);

				result = RunScript(LedgerTypes.AccountsPayable);

				lines = new[] {
					new object[] { "001C", transactionRegNoToAssert },
					new object[] { "002C", string.Empty },
				};

				AssertDataTableAllRowsByKeyColumns("", result, headers, lines);
			}
		}

		static void SetupHeaderWithClosestPortAndCustomsCode(OrgHeader header, string closestPort, string countryCode, ZString customsRegNo, string codeType)
		{
			header.OH_RL_NKClosestPort = closestPort;
			header.CompanyData.SetAPTaxApplicable(true);
			var taxCode = header.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = countryCode;
			taxCode.OK_CustomsRegNo = customsRegNo;
			taxCode.OK_CodeType = codeType;
		}

		#endregion

		public void TestOrganisationCountry()
		{
			var krPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, Constants.CountryCodes.KoreaSouth));
			var krOrg = TestObjectCreator.CreateOrgHeader("KROrg", false, true, krPort.RL_Code);
			var auPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, Constants.CountryCodes.Australia));
			var auOrg = TestObjectCreator.CreateOrgHeader("AUOrg", false, true, auPort.RL_Code);
			Factory.Save();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.AUD, 100M, krOrg);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M);
			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsReceivable);
			AssertEquals(Constants.CountryCodes.KoreaSouth, result.Rows[0]["OrgCountryCode"]);
			AssertEquals("Korea, Republic of", result.Rows[0]["OrgCountryName"]);
			AssertEquals("Foreign", result.Rows[0]["IsTransactionOrgLocalForeign"]);

			invoice.AH_OH = auOrg.PK;
			Factory.Save();
			result = RunScript(LedgerTypes.AccountsReceivable);
			AssertEquals(Constants.CountryCodes.Australia, result.Rows[0]["OrgCountryCode"]);
			AssertEquals("Australia", result.Rows[0]["OrgCountryName"]);
			AssertEquals("Local", result.Rows[0]["IsTransactionOrgLocalForeign"]);
		}

		public void TestCustomsRegNoForEULoginCompany()
		{
			GlbCompany loginCompanyEU = TestObjectCreator.CreateNewCompany("NNN", "NL");
			GlbBranch branchNL = TestObjectCreator.CreateBranch("NBR", "branch for NL", loginCompanyEU);
			Factory.Save();

			OrgHeader orgNLAMS = TestObjectCreator.CreateOrgHeader("NLAMS", false, true, null);
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgNLAMS, OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Constants.CountryCodes.Netherlands, "3123");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgNLAMS, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Constants.CountryCodes.Australia, "41065894724");
			OrgHeader orgFRLON = TestObjectCreator.CreateOrgHeader("GBLON", false, true, "FR2BC");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgFRLON, OrgCusCode.FranceCodeTypes.TVA, Constants.CountryCodes.France, "54564");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgFRLON, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Constants.CountryCodes.Australia, "41065894724");
			OrgHeader orgAUABC = TestObjectCreator.CreateOrgHeader("AUABC", false, true, "AUBNE");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgAUABC, ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, Constants.CountryCodes.Italy, "987842");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgAUABC, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Constants.CountryCodes.Australia, "41065894724");
			Factory.Save();

			ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.AUD, 100M, orgNLAMS);
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, branchNL.PK);
			invoice.AH_GB = branchNL.PK;
			Factory.Save();

			DataTable result = RunScript(LedgerTypes.AccountsReceivable, loginCompanyEU);
			AssertEquals("EU Login Country / Invoice 1 - Tax Registration Number for the current login country", "NL3123", result.Rows[0]["OK_CustomsRegNo"]);

			ARInvoice invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV2", TestObjectCreator.AUD, 200M, orgFRLON);
			line = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 200M, branchNL.PK);
			invoice2.AH_GB = branchNL.PK;
			Factory.Save();

			result = RunScript(LedgerTypes.AccountsReceivable, loginCompanyEU);
			AssertEquals("EU Login Country / Invoice 2 - Fall back to Tax Registration Number for the home EU Country", "FR54564", result.Rows[0]["OK_CustomsRegNo"]);

			ARInvoice invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV3", TestObjectCreator.AUD, 300M, orgAUABC);
			line = TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.AUD, 1M, 300M, branchNL.PK);
			invoice3.AH_GB = branchNL.PK;
			Factory.Save();

			result = RunScript(LedgerTypes.AccountsReceivable, loginCompanyEU);
			AssertEquals("EU Login Country / Invoice 3 - Fallback to Empty", "", result.Rows[1]["OK_CustomsRegNo"]);
		}

		public void TestCustomsRegNoForNonEULoginCompany()
		{
			GlbCompany loginCompanyNonEU = TestObjectCreator.CreateNewCompany("AAA", "AU");
			GlbBranch branchAU = TestObjectCreator.CreateBranch("ABR", "branch for AU", loginCompanyNonEU);
			Factory.Save();

			OrgHeader orgAUABC = TestObjectCreator.CreateOrgHeader("AUABC", false, true, null);
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgAUABC, OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Constants.CountryCodes.Netherlands, "3123");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgAUABC, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Constants.CountryCodes.Australia, "41065894724");
			OrgHeader orgITXYZ = TestObjectCreator.CreateOrgHeader("ITXYZ", false, true, null);
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgITXYZ, OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Constants.CountryCodes.Italy, "3123");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgITXYZ, OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Constants.CountryCodes.Germany, "78484");
			Factory.Save();

			ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.AUD, 100M, orgAUABC);
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, branchAU.PK);
			invoice.AH_GB = branchAU.PK;
			Factory.Save();

			DataTable result = RunScript(LedgerTypes.AccountsReceivable, loginCompanyNonEU);
			AssertEquals("Non-EU Login Country / Invoice 1 - Tax Registration Number for the current login country", "41065894724", result.Rows[0]["OK_CustomsRegNo"]);

			invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV2", TestObjectCreator.AUD, 100M, orgITXYZ);
			line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, branchAU.PK);
			invoice.AH_GB = branchAU.PK;
			Factory.Save();

			result = RunScript(LedgerTypes.AccountsReceivable, loginCompanyNonEU);
			AssertEquals("Non-EU Login Country / Invoice 2 - Fallback to Empty", "", result.Rows[1]["OK_CustomsRegNo"]);
		}

		public void TestCustomsTaxRegistrationNumberForEUN()
		{
			GlbCompany loginCompanyGR = TestObjectCreator.CreateNewCompany("ATH", "GR");
			GlbBranch branchGR = TestObjectCreator.CreateBranch("BRN", "branch for GR", loginCompanyGR);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchGR.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				OrgHeader orgGRAND = TestObjectCreator.CreateOrgHeader("GRAND", false, true, "FR2BC");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgGRAND, OrgCusCode.GreeceCodeTypes.AFM, Constants.CountryCodes.Greece, "999999");
				OrgHeader orgGRSKY = TestObjectCreator.CreateOrgHeader("GRSKY", false, true, "FR2BC");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgGRSKY, OrgCusCode.GreeceCodeTypes.AFM, Constants.CountryCodes.Greece, "EL654321");
				OrgHeader orgGRAPE = TestObjectCreator.CreateOrgHeader("GRAPE", false, true, "FR2BC");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgGRAPE, OrgCusCode.FranceCodeTypes.TVA, Constants.CountryCodes.France, "GR123456");
				OrgHeader orgGRAPE2 = TestObjectCreator.CreateOrgHeader("GRAPE", false, true, "FR2BC");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgGRAPE2, OrgCusCode.FranceCodeTypes.TVA, Constants.CountryCodes.France, "FR999999");

				ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.AUD, 100M, orgGRAND);
				InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, branchGR.PK);
				ARInvoice invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV2", TestObjectCreator.AUD, 200M, orgGRSKY);
				line = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 200M, branchGR.PK);
				ARInvoice invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV3", TestObjectCreator.AUD, 200M, orgGRAPE);
				line = TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.AUD, 1M, 200M, branchGR.PK);
				ARInvoice invoice4 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV4", TestObjectCreator.AUD, 200M, orgGRAPE2);
				line = TestObjectCreator.CreateInvoiceLine(invoice4, TestObjectCreator.AUD, 1M, 200M, branchGR.PK);
				Factory.Save();

				DataTable result = RunScript(LedgerTypes.AccountsReceivable, loginCompanyGR);
				AssertEquals("Greece Login Country / Invoice 1 - Tax Registration Number", "EL999999", result.Rows[0]["OK_CustomsRegNo"]);
				AssertEquals("Greece Login Country / Invoice 2 - Tax Registration Number", "EL654321", result.Rows[1]["OK_CustomsRegNo"]);
				AssertEquals("Greece Login Country / Invoice 3 - Tax Registration Number", "FRGR123456", result.Rows[2]["OK_CustomsRegNo"]);
				AssertEquals("Greece Login Country / Invoice 4 - Tax Registration Number", "FR999999", result.Rows[3]["OK_CustomsRegNo"]);
			}

			GlbCompany loginCompanyIT = TestObjectCreator.CreateNewCompany("ITC", "IT");
			GlbBranch branchIT = TestObjectCreator.CreateBranch("ITB", "branch for IT", loginCompanyIT);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchIT.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				OrgHeader orgFRBAR = TestObjectCreator.CreateOrgHeader("IT2BRO", false, true, "FR2BC");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgFRBAR, OrgCusCode.CodeTypes.IVA, Constants.CountryCodes.Italy, "999999");
				OrgHeader orgFRBAR2 = TestObjectCreator.CreateOrgHeader("IT2BRO", false, true, "FR2BC");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgFRBAR2, OrgCusCode.CodeTypes.IVA, Constants.CountryCodes.Italy, "IT999999");
				OrgHeader orgFRCAN = TestObjectCreator.CreateOrgHeader("FR2BC", false, true, "FR2BC");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgFRCAN, OrgCusCode.FranceCodeTypes.TVA, Constants.CountryCodes.France, "123456");
				OrgHeader orgFRCAR2 = TestObjectCreator.CreateOrgHeader("FR2BC", false, true, "FR2BC");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgFRCAR2, OrgCusCode.FranceCodeTypes.TVA, Constants.CountryCodes.France, "FR123456");
				OrgHeader orgFRCAR3 = TestObjectCreator.CreateOrgHeader("GRAPE", false, true, "GRAPE");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgFRCAR3, OrgCusCode.GreeceCodeTypes.AFM, Constants.CountryCodes.Greece, "654321");
				OrgHeader orgFRCAR4 = TestObjectCreator.CreateOrgHeader("GRAPE", false, true, "GRAPE");
				TestObjectCreator.SetCustomsCodeForOrgHeader(orgFRCAR4, OrgCusCode.GreeceCodeTypes.AFM, Constants.CountryCodes.Greece, "EL654321");

				ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV5", TestObjectCreator.AUD, 100M, orgFRBAR);
				InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, branchIT.PK);
				ARInvoice invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV6", TestObjectCreator.AUD, 200M, orgFRBAR2);
				line = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 200M, branchIT.PK);
				ARInvoice invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV7", TestObjectCreator.AUD, 200M, orgFRCAN);
				line = TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.AUD, 1M, 200M, branchIT.PK);
				ARInvoice invoice4 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV8", TestObjectCreator.AUD, 200M, orgFRCAR2);
				line = TestObjectCreator.CreateInvoiceLine(invoice4, TestObjectCreator.AUD, 1M, 200M, branchIT.PK);
				ARInvoice invoice5 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV9", TestObjectCreator.AUD, 200M, orgFRCAR3);
				line = TestObjectCreator.CreateInvoiceLine(invoice5, TestObjectCreator.AUD, 1M, 200M, branchIT.PK);
				ARInvoice invoice6 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV10", TestObjectCreator.AUD, 200M, orgFRCAR4);
				line = TestObjectCreator.CreateInvoiceLine(invoice6, TestObjectCreator.AUD, 1M, 200M, branchIT.PK);
				Factory.Save();

				DataTable result = RunScript(LedgerTypes.AccountsReceivable, loginCompanyIT);
				AssertEquals("Italy Login Country / Invoice 1 - Tax Registration Number", "IT999999", result.Rows[0]["OK_CustomsRegNo"]);
				AssertEquals("Italy Login Country / Invoice 2 - Tax Registration Number", "IT999999", result.Rows[1]["OK_CustomsRegNo"]);
				AssertEquals("Italy Login Country / Invoice 3 - Tax Registration Number", "FR123456", result.Rows[2]["OK_CustomsRegNo"]);
				AssertEquals("Italy Login Country / Invoice 4 - Tax Registration Number", "FR123456", result.Rows[3]["OK_CustomsRegNo"]);
				AssertEquals("Italy Login Country / Invoice 5 - Tax Registration Number", "EL654321", result.Rows[4]["OK_CustomsRegNo"]);
				AssertEquals("Italy Login Country / Invoice 6 - Tax Registration Number", "EL654321", result.Rows[5]["OK_CustomsRegNo"]);
			}
		}

		public void TestShowRowsHaveGSTVATButLineAmountZero()
		{
			DirectPayment payment1 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 100M, 10M, 0M, 0M);
			DirectPayment payment2 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 0M, 50M, 0M, 200M);

			Factory.Save();

			DataTable result = RunScript(LedgerTypes.CashBook);
			AssertEquals("Should have 3 Rows", 3, result.Rows.Count);
			AssertEquals("Should Find Row With Zero Line Amount And Have GSTVAT Value", 2, result.Select("LocalAmount = 0").Length);
		}

		public void TestShowCorrectAmountForChinaInputClaimTax()
		{
			ZGuid expectedTax = TestObjectCreator.INP7.PK;
			APInvoice invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.USD, 2m, 200.00m, 0.00m, 14.00m, 100.00m, 0.00m, 7.00m);
			invoice.Lines[0].AL_AT = expectedTax;
			Factory.Save();

			DataTable result = RunScript(LedgerTypes.AccountsPayable);
			AssertEquals("Should have 1 Rows", 1, result.Rows.Count);
			AssertEquals("TaxAmount", -7.53m, result.Rows[0]["TaxAmount"]);
			AssertEquals("ExtraTaxAmount", -7.53m, result.Rows[0]["ExtraTaxAmount"]);
			AssertEquals("GSTVATTaxAmount", 0m, result.Rows[0]["GSTVATTaxAmount"]);
			AssertEquals("LocalAmount", -100m, result.Rows[0]["LocalAmount"]);
			AssertEquals("GrossLocalValue", -107.53m, result.Rows[0]["GrossLocalValue"]);
			AssertEquals("GrossOSValue", -215.06M, result.Rows[0]["GrossOSValue"]);
		}

		public void TestNoErrorWithTaxRateNumeratorUpTo10Numbers()
		{
			var invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI, ZDateTime.Now);
			invoice1.AH_TransactionNum = "INV001";
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M);
			line1.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			line1.AL_TaxRateNumerator = 1234567890;
			line1.AL_TaxRateDenominator = 1000000000;
			Factory.Save();

			DataTable result = RunScript(LedgerTypes.AccountsPayable);
			AssertEquals("Should have 1 Row", 1, result.Rows.Count);
		}

		public void TestShowCorrectAmountForQSTAndQCT()
		{
			TestHelper.SetupInvoice("001", LedgerTypes.AccountsReceivable, TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 200M, 0M);
			TestHelper.SetupInvoice("002", LedgerTypes.AccountsPayable, TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 240M, 0M);
			TestHelper.SetupInvoice("003", LedgerTypes.AccountsReceivable, TestObjectCreator.GSTANDQST1.PK, 140M, 140M);
			TestHelper.SetupInvoice("004", LedgerTypes.AccountsPayable, TestObjectCreator.GSTANDQST1.PK, 320M, 0M);

			AsserAmountsWithExtraTax();
		}

		public void TestShowCorrectAmountForQSTAndQCT_SER()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var gSTANDQSTBASEDONQCT = TestObjectCreator.GSTANDQSTBASEDONQCT;
				gSTANDQSTBASEDONQCT.AT_Type = AccTaxRate.Types.ServiceTax;

				var gSTANDQST1 = TestObjectCreator.GSTANDQST1;
				gSTANDQST1.AT_Type = AccTaxRate.Types.ServiceTax;

				TestHelper.SetupInvoice("001", LedgerTypes.AccountsReceivable, gSTANDQSTBASEDONQCT.PK, 200M, 0M);
				TestHelper.SetupInvoice("002", LedgerTypes.AccountsPayable, gSTANDQSTBASEDONQCT.PK, 240M, 0M);
				TestHelper.SetupInvoice("003", LedgerTypes.AccountsReceivable, gSTANDQST1.PK, 140M, 140M);
				TestHelper.SetupInvoice("004", LedgerTypes.AccountsPayable, gSTANDQST1.PK, 320M, 0M);

				AsserAmountsWithExtraTax();
			}
		}

		public void TestShowCorrectAmountForQSTAndSTA()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var staTax = TestObjectCreator.GSTANDQSTBASEDONQCT;
				staTax.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;

				TestHelper.SetupInvoice("001", LedgerTypes.AccountsReceivable, staTax.PK, 200M, 0M);
				TestHelper.SetupInvoice("002", LedgerTypes.AccountsPayable, staTax.PK, 240M, 0M);
				TestHelper.SetupInvoice("003", LedgerTypes.AccountsReceivable, TestObjectCreator.GSTANDQST1.PK, 140M, 140M);
				TestHelper.SetupInvoice("004", LedgerTypes.AccountsPayable, TestObjectCreator.GSTANDQST1.PK, 320M, 0M);

				AsserAmountsWithExtraTax();
			}
		}

		void AsserAmountsWithExtraTax()
		{
			DataTable result = RunScript(LedgerTypes.AccountsPayable);
			AssertEquals("Should have 2 Rows", 2, result.Rows.Count);

			var rowsToTest = result.Select("", "AH_TRANSACTIONNUM ASC");

			AssertEquals("TaxAmount", -17.97M, rowsToTest[0]["TaxAmount"]);
			AssertEquals("ExtraTaxAmount", -11.97M, rowsToTest[0]["ExtraTaxAmount"]);
			AssertEquals("GSTVATTaxAmount", -6m, rowsToTest[0]["GSTVATTaxAmount"]);
			AssertEquals("LocalAmount", -120m, rowsToTest[0]["LocalAmount"]);
			AssertEquals("GrossLocalValue", -137.97m, rowsToTest[0]["GrossLocalValue"]);
			AssertEquals("GrossOSValue", -275.94m, rowsToTest[0]["GrossOSValue"]);
			AssertEquals("TaxAmount", -23.96M, rowsToTest[1]["TaxAmount"]);
			AssertEquals("ExtraTaxAmount", -15.96M, rowsToTest[1]["ExtraTaxAmount"]);
			AssertEquals("GSTVATTaxAmount", -8m, rowsToTest[1]["GSTVATTaxAmount"]);
			AssertEquals("LocalAmount", -160m, rowsToTest[1]["LocalAmount"]);
			AssertEquals("GrossLocalValue", -183.96m, rowsToTest[1]["GrossLocalValue"]);
			AssertEquals("GrossOSValue", -367.92m, rowsToTest[1]["GrossOSValue"]);
		}

		public void TestShowCorrectAmountForGST()
		{
			TestHelper.SetupInvoice("001", LedgerTypes.AccountsReceivable, TestObjectCreator.GST1.PK, 200M, 0M);
			TestHelper.SetupInvoice("002", LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 200M, 0M);

			DataTable result = RunScript(LedgerTypes.AccountsPayable);
			AssertEquals("Should have 1 Row", 1, result.Rows.Count);

			var rowsToTest = result.Select();
			AssertEquals("TaxAmount", -10M, rowsToTest[0]["TaxAmount"]);
			AssertEquals("ExtraTaxAmount", -0M, rowsToTest[0]["ExtraTaxAmount"]);
			AssertEquals("GSTVATTaxAmount", -10m, rowsToTest[0]["GSTVATTaxAmount"]);
			AssertEquals("LocalAmount", -100m, rowsToTest[0]["LocalAmount"]);
			AssertEquals("GrossLocalValue", -110m, rowsToTest[0]["GrossLocalValue"]);
			AssertEquals("GrossOSValue", -220m, rowsToTest[0]["GrossOSValue"]);
		}

		public void TestCalculatedREVAmountsUseCurrencyRounding()
		{
			var currency = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.Bahrain);
			Assert("Currency is rounded to 3 decimal places", currency.RX_SubUnitRatio == 1000);
			var newCompany = TestObjectCreator.CreateNewCompany("TST", Enterprise.Core.Constants.CountryCodes.Bahrain);
			newCompany.GC_RX_NKLocalCurrency = currency.RX_Code;
			var newBranch = TestObjectCreator.CreateNewBranch(newCompany, "TST");
			var taxRate = TestObjectCreator.CreateTaxRate("REV", "Reverse Rated Test", 10);
			taxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), TestObjectCreator.FESDepartment.PK.ToGuid()))
			{
				var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Desc", GlbCompany.CurrentCompany.LocalCurrency, 123.456m, TestObjectCreator.Creditor1, GlbCompany.CurrentCompany.LocalCurrency, 0m, TestObjectCreator.AALSHI);
				charge.JR_APInvoiceNum = "1234";
				charge.JR_APInvoiceDate = ZDateTime.Today;
				charge.JR_AT_CostGSTRate = taxRate.PK;
				Factory.Save();
				var postManager = new InvoicingPostManager(TestObjectCreator.Job1);
				postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
				Factory.Save();

				var result = RunScript(LedgerTypes.AccountsPayable);
				AssertEquals("Should have 2 rows", 2, result.Rows.Count);

				var inputRow = result.Rows[0];
				AssertEquals("INPUT - TaxAmount", -12.346m, inputRow["TaxAmount"]);
				AssertEquals("INPUT - ExtraTaxAmount", 0M, inputRow["ExtraTaxAmount"]);
				AssertEquals("INPUT - GSTVATTaxAmount", -12.346m, inputRow["GSTVATTaxAmount"]);
				AssertEquals("INPUT - LocalAmount", -123.456m, inputRow["LocalAmount"]);
				AssertEquals("INPUT - GrossLocalValue", -135.802m, inputRow["GrossLocalValue"]);
				AssertEquals("INPUT - GrossOSValue", -135.802m, inputRow["GrossOSValue"]);

				var outputRow = result.Rows[1];
				AssertEquals("OUTPUT - TaxAmount", 12.346m, outputRow["TaxAmount"]);
				AssertEquals("OUTPUT - ExtraTaxAmount", 0M, outputRow["ExtraTaxAmount"]);
				AssertEquals("OUTPUT - GSTVATTaxAmount", 12.346m, outputRow["GSTVATTaxAmount"]);
				AssertEquals("OUTPUT - LocalAmount", 123.456m, outputRow["LocalAmount"]);
				AssertEquals("OUTPUT - GrossLocalValue", 135.802m, outputRow["GrossLocalValue"]);
				AssertEquals("OUTPUT - GrossOSValue", 135.802m, outputRow["GrossOSValue"]);
			}
		}

		public void TestGSTANDQST_WithRecoverableVAT()
		{
			TestHelper.SetupInvoice("003", "AR", TestObjectCreator.GSTANDQST1.PK, 200M, 0M, true);
			TestHelper.SetupInvoice("004", "AP", TestObjectCreator.GSTANDQST1.PK, 200M, 0M, true);

			var headers = new[] { "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable", "GSTVATTaxAmount", "GSTVATTaxAmountRecoverable", "GSTVATTaxAmountNotRecoverable", "ExtraTaxAmount", "ExtraTaxAmountRecoverable", "ExtraTaxAmountNotRecoverable", "GSTVATTaxAmountRecoverablePercentage", "LocalAmount", "GrossLocalValue", "GrossOSValue" };
			var lines = new[] { new object[] { 14.98M, 14.98M, 0M, 5M, 5M, 0M, 9.98M, 9.98M, 0M, 100M, 100M, 114.98M, 229.96M } };

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertDataTableAllRows("AccountsReceivable", result, headers, lines);

			lines = new[] { new object[] { -14.98M, -11.98M, -3M, -5M, -4M, -1M, -9.98M, -7.98M, -2M, 80M, -100M, -114.98M, -229.96M } };
			result = RunScript(LedgerTypes.AccountsPayable);
			AssertDataTableAllRows("AccountsPayable", result, headers, lines);
		}

		public void TestGSTANDQST_WithRecoverableVAT_WithCashVAT()
		{
			TestHelper.SetupInvoiceWithCashVATLine("AR", TestObjectCreator.GSTANDQST1.PK, 300M, 100M, true);
			TestHelper.SetupInvoiceWithCashVATLine("AP", TestObjectCreator.GSTANDQST1.PK, 300M, 100M, true);

			var headers = new[] { "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable", "GSTVATTaxAmount", "GSTVATTaxAmountRecoverable", "GSTVATTaxAmountNotRecoverable", "ExtraTaxAmount", "ExtraTaxAmountRecoverable", "ExtraTaxAmountNotRecoverable", "GSTVATTaxAmountRecoverablePercentage", "LocalAmount", "GrossLocalValue", "GrossOSValue" };
			var lines = new[] { new object[] { 13.03M, 13.03M, 0M, 4.35M, 4.35M, 0M, 8.68M, 8.68M, 0M, 100M, 86.97M, 100M, 200M } };

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertDataTableAllRows("AccountsReceivable", result, headers, lines);

			lines = new[] { new object[] { -13.03M, -10.42M, -2.61M, -4.35M, -3.48M, -0.87M, -8.68M, -6.94M, -1.74M, 80M, -86.97M, -100M, -200M } };
			result = RunScript(LedgerTypes.AccountsPayable);
			AssertDataTableAllRows("AccountsPayable", result, headers, lines);
		}

		public void TestGSTANDQST_WithRecoverableVAT_CorrectCalculationSequence()
		{
			TestHelper.SetupInvoice("001", "AP", TestObjectCreator.GSTANDQSTBASEDONQCT.PK, 400M, 0M, true, 0.7777M);

			var headers = new[] { "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable", "GSTVATTaxAmount", "GSTVATTaxAmountRecoverable", "GSTVATTaxAmountNotRecoverable", "ExtraTaxAmount", "ExtraTaxAmountRecoverable", "ExtraTaxAmountNotRecoverable", "GSTVATTaxAmountRecoverablePercentage", "LocalAmount", "GrossLocalValue", "GrossOSValue" };
			var lines = new[] { new object[] { -29.95M, -23.29M, -6.66M, -10M, -7.78M, -2.22M, -19.95M, -15.52M, -4.43M, 77.8M, -200M, -229.95M, -459.90M } }; //If recoverable from Total tax calculated first then -15.51. If split GST and Extra Tax from Total first, then -15.52.
			DataTable result = RunScript(LedgerTypes.AccountsPayable);
			AssertDataTableAllRows("AccountsPayable", result, headers, lines);
		}

		public void TestSERANDEDU_WithRecoverableVAT_WithCashVAT()
		{
			TestHelper.SetupInvoiceWithCashVATLine("AR", TestObjectCreator.SERANDEDU1.PK, 300M, 100M, true);
			TestHelper.SetupInvoiceWithCashVATLine("AP", TestObjectCreator.SERANDEDU1.PK, 300M, 100M, true);

			var headers = new[] { "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable", "GSTVATTaxAmount", "GSTVATTaxAmountRecoverable", "GSTVATTaxAmountNotRecoverable", "ExtraTaxAmount", "ExtraTaxAmountRecoverable", "ExtraTaxAmountNotRecoverable", "GSTVATTaxAmountRecoverablePercentage", "LocalAmount", "GrossLocalValue", "GrossOSValue" };
			var lines = new[] { new object[] { -9.34M, -7.47M, -1.87M, -9.07M, -7.26M, -1.81M, -0.27M, -0.22M, -0.05M, 80.04M, -90.66M, -100M, -200M } };
			DataTable result = RunScript(LedgerTypes.AccountsPayable);
			AssertDataTableAllRows("AccountsPayable", result, headers, lines);
		}

		public void TestREV_WithRecoverableVAT()
		{
			TestHelper.SetupInvoice("003", "AR", TestObjectCreator.REV.PK, 200M, 0M, true);
			TestHelper.SetupInvoice("004", "AP", TestObjectCreator.REV.PK, 200M, 0M, true);

			var headers = new[] { "InOutput", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable", "GSTVATTaxAmount", "GSTVATTaxAmountRecoverable", "GSTVATTaxAmountNotRecoverable", "ExtraTaxAmount", "ExtraTaxAmountRecoverable", "ExtraTaxAmountNotRecoverable", "GSTVATTaxAmountRecoverablePercentage", "LocalAmount", "GrossLocalValue", "GrossOSValue" };
			var lines = new[] { new object[] { "OUTPUT", 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 100M, 100M, 200M } };

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertDataTableAllRowsByKeyColumns("AccountsReceivable", result, headers, lines);

			lines = new[]
			{
				new object[] { "OUTPUT", 10M, 10M, 0M, 10M, 10M, 0M, 0M, 0M, 0M, 100M, 100M, 110M, 220M },
				new object[] { "INPUT", -10M, -8M, -2M, -10M, -8M, -2M, 0M, 0M, 0M, 80M, -100M, -110M, -220M }
			};
			result = RunScript(LedgerTypes.AccountsPayable);
			AssertDataTableAllRowsByKeyColumns("AccountsPayable", result, headers, lines);
		}

		public void TestREV_WithRecoverableVAT_WithCashVAT()
		{
			TestHelper.SetupInvoiceWithCashVATLine("AR", TestObjectCreator.REV.PK, 300M, 100M, true);
			TestHelper.SetupInvoiceWithCashVATLine("AP", TestObjectCreator.REV.PK, 300M, 100M, true);

			var headers = new[] { "InOutput", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable", "GSTVATTaxAmount", "GSTVATTaxAmountRecoverable", "GSTVATTaxAmountNotRecoverable", "ExtraTaxAmount", "ExtraTaxAmountRecoverable", "ExtraTaxAmountNotRecoverable", "GSTVATTaxAmountRecoverablePercentage", "LocalAmount", "GrossLocalValue", "GrossOSValue" };
			var lines = new[] { new object[] { "OUTPUT", 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 100M, 100M, 200M } };

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertDataTableAllRowsByKeyColumns("AccountsReceivable", result, headers, lines);

			lines = new[]
			{
				new object[] { "OUTPUT", 10M, 10M, 0M, 10M, 10M, 0M, 0M, 0M, 0M, 100M, 100M, 110M, 220M },
				new object[] { "INPUT", -10M, -8M, -2M, -10M, -8M, -2M, 0M, 0M, 0M, 80M, -100M, -110M, -220M }
			};
			result = RunScript(LedgerTypes.AccountsPayable);
			AssertDataTableAllRowsByKeyColumns("AccountsPayable", result, headers, lines);
		}

		public void TestTaxAmount_WithDecimal()
		{
			var newCompany = TestObjectCreator.CreateNewCompany("TST", Enterprise.Core.Constants.CountryCodes.Bahrain);
			var newBranch = TestObjectCreator.CreateNewBranch(newCompany, "TST");
			var taxRate = TestObjectCreator.CreateTaxRate("REV", "Reverse Rated Test", 11, 10);
			taxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), TestObjectCreator.FESDepartment.PK.ToGuid()))
			{
				var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Desc", GlbCompany.CurrentCompany.LocalCurrency, 1000m, TestObjectCreator.Creditor1, GlbCompany.CurrentCompany.LocalCurrency, 0m, TestObjectCreator.AALSHI);
				charge.JR_APInvoiceNum = "1000";
				charge.JR_APInvoiceDate = ZDateTime.Today;
				charge.JR_AT_CostGSTRate = taxRate.PK;
				Factory.Save();
				var postManager = new InvoicingPostManager(TestObjectCreator.Job1);
				postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
				Factory.Save();

				var result = RunScript(LedgerTypes.AccountsPayable);
				AssertEquals("Should have 2 rows", 2, result.Rows.Count);

				var inputRow = result.Rows[0];
				AssertEquals("INPUT - TaxAmount", -11m, inputRow["TaxAmount"]);

				var outputRow = result.Rows[1];
				AssertEquals("OUTPUT - TaxAmount", 11m, outputRow["TaxAmount"]);
			}
		}

		public void TestConsolidatedInvoiceRefField_TransactionReferenceField()
		{
			var expectedConsolidatedInvoiceRef = "00001000";
			var expectedTransactionReference = "TrxRef456";

			ZGuid expectedTax = TestObjectCreator.INP7.PK;
			APInvoice invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1.0m, 100.00m, 0.00m, 7.00m, 100.00m, 0.00m, 7.00m);
			invoice.Lines[0].AL_AT = expectedTax;
			invoice.AH_TransactionReference = expectedTransactionReference;
			Factory.Save();
			DataTable result = RunScript(LedgerTypes.AccountsPayable);
			AssertEquals("Should have 1 Rows", 1, result.Rows.Count);
			AssertEquals("INV/CRD/ADJ must return ConsolidatedInvoiceRef as set i.e 00001000", "00001000", result.Rows[0]["ConsolidatedInvoiceRef"]);
			AssertEquals("INV/CRD/ADJ must return TransactionReference as set i.e. TrxRef456", expectedTransactionReference, result.Rows[0]["ComplianceDocumentNumber"]);

			DirectPayment payment = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 100M, 10M, 0M, 0M);
			payment.AH_ConsolidatedInvoiceRef = expectedConsolidatedInvoiceRef;
			payment.AH_TransactionReference = expectedTransactionReference;
			DirectReceipt receipt = TestObjectCreator.CreateDirectReceipt(ZDateTime.Now, 100m, 10m, 4000m, 400m);
			receipt.AH_ConsolidatedInvoiceRef = expectedConsolidatedInvoiceRef;
			receipt.AH_TransactionReference = expectedTransactionReference;
			Factory.Save();
			result = RunScript(LedgerTypes.CashBook);
			AssertEquals("Should have 3 Rows", 3, result.Rows.Count);
			for (int index = 0; index < result.Rows.Count; index++)
			{
				AssertEquals("DPY/DRC must return ConsolidatedInvoiceRef = empty", ZString.Empty, result.Rows[index]["ConsolidatedInvoiceRef"]);
				AssertEquals("DPY/DRC must return TransactionReference = empty", ZString.Empty, result.Rows[index]["ComplianceDocumentNumber"]);
			}
		}

		public void TestNewColunmsForIndiaGSTReport()
		{
			var testOrg = TestObjectCreator.CreateOrgHeader("TestOrg", true, true, true, true, true, true);
			var testOrgAddress = testOrg.Addresses[0];
			testOrgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			testOrgAddress.State = "BBB";

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchOrgProxy.OH_Code = "Br Org Proxy";
			TestObjectCreator.SetCustomsCodeForOrgHeader(branchOrgProxy, "GST", Constants.CountryCodes.India, "India Customs Code");
			var branchOrgProxyAddress = branchOrgProxy.Addresses[0];
			branchOrgProxyAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			branchOrgProxyAddress.State = "AAA";

			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
			GlbBranch.CurrentBranch.Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, testOrg, ZDateTime.Now);
			invoice1.AH_TransactionNum = "INV001";
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M);
			line1.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			line1.AL_GovtChargeCode = "Govt Chg Code 1";
			line1.AL_Desc = "Govt Chg Code 1";

			var line2 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M);
			line2.AL_AT = TestObjectCreator.RAX.PK;
			line2.AL_GovtChargeCode = "Govt Chg Code 2";
			line2.AL_Desc = "Govt Chg Code 2";

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, testOrg, ZDateTime.Now);
			invoice2.AH_TransactionNum = "INV002";
			invoice2.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M);
			line3.AL_AT = TestObjectCreator.SVAT2.PK;
			line3.AL_GovtChargeCode = "Govt Chg Code 3";
			line3.AL_Desc = "Govt Chg Code 3";

			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsPayable, headerBranchList: GlbBranch.CurrentBranch.PK.ToString());
			AssertEquals("Should have 2 Rows", 2, result.Rows.Count);

			//line1
			var row1 = result.Rows.Cast<DataRow>().FirstOrDefault(x => x["TaxType"].ToString() == TestObjectCreator.GSTWithExtraRate.AT_Type);
			AssertNotNull(row1);
			AssertEquals("INV001", row1["AH_TransactionNum"].ToString());
			AssertEquals("RAT", row1["TaxType"].ToString());
			AssertEquals(12M, Decimal.Parse(row1["TaxRate"].ToString()));
			AssertEquals("EDU", row1["ExtraTaxRateType"].ToString());
			AssertEquals(3M, Decimal.Parse(row1["ExtraTaxRate"].ToString()));
			AssertEquals(-229.36M, Decimal.Parse(row1["AH_TotalAmountIncludingTax"].ToString()));
			AssertEquals(-200.00M, Decimal.Parse(row1["AH_InvoiceAmount"].ToString()));
			AssertEquals(-29.36M, Decimal.Parse(row1["AH_GSTAmount"].ToString()));
			AssertNotNull(row1["AH_InvoiceDate"].ToString());
			AssertEquals("BNE", row1["TransactionHeaderBranchCode"].ToString());
			AssertEquals("Br Org Proxy", row1["HeaderBranchOrgProxyCode"].ToString());
			AssertEquals("AAA", row1["HeaderBranchOrgProxyState"].ToString());
			AssertEquals("India Customs Code", row1["HeaderBranchTaxRegistration"].ToString());
			AssertEquals("ZTestOrg", row1["TransactionOrgCode"].ToString());
			AssertEquals("Test Company Name", row1["TransactionOrgFullName"].ToString());
			AssertEquals("BUS", row1["TransactionOrgCategory"].ToString());
			AssertEquals("AU", row1["OrgMainOfficeAddressCountryCode"].ToString());
			AssertEquals("BBB", row1["OrgMainOfficeAddressState"].ToString());
			AssertEquals("Govt Chg Code 1", row1["AL_GovtChargeCode"].ToString());
			AssertEquals("BNE", row1["AL_LineBranchCode"].ToString());
			AssertNotNull(row1["ParentTransactionPostDate"].ToString());
			AssertNotNull(row1["ParentTransactionInvoiceDate"].ToString());

			//line2
			var row2 = result.Rows.Cast<DataRow>().FirstOrDefault(x => x["TaxType"].ToString() == TestObjectCreator.RAX.AT_Type);
			AssertNotNull(row2);
			AssertEquals("INV001", row2["AH_TransactionNum"].ToString());
			AssertEquals("RAX", row2["TaxType"].ToString());
			AssertEquals(17M, Decimal.Parse(row2["TaxRate"].ToString()));
			AssertEquals("", row2["ExtraTaxRateType"].ToString());
			AssertEquals(0M, Decimal.Parse(row2["ExtraTaxRate"].ToString()));
			AssertEquals("Govt Chg Code 2", row2["AL_GovtChargeCode"].ToString());
		}

		public void TestGetRateFromLine()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1M, 35M, 25M, 4M, 3M, 7M, 2M);
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M);
			line1.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;

			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 88M);
			line2.AL_AT = TestObjectCreator.REF.PK;
			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsPayable).AsEnumerable();
			AssertResult();

			TestObjectCreator.GSTWithExtraRate.SetRate_ForTestOnly(16, 4);
			TestObjectCreator.GSTWithExtraRate.SetExtraRate_ForTestOnly(36, 6);

			TestObjectCreator.REF.SetRate_ForTestOnly(125, 10);
			TestObjectCreator.REF.SetExtraRate_ForTestOnly(95, 6);
			Factory.Save();

			result = RunScript(LedgerTypes.AccountsPayable).AsEnumerable();
			AssertResult();

			void AssertResult()
			{
				AssertLineResult("-", 0m, 0m);
				AssertLineResult(TestObjectCreator.GSTWithExtraRate.AT_Type, 12M, 3M);
				AssertLineResult(TestObjectCreator.REF.AT_Type, 9M, 112.5M);
			}

			void AssertLineResult(string type, decimal? expectedTaxRate, decimal? expectedExtraTaxRate)
			{
				var row = result.FirstOrDefault(x => x["TaxType"].ToString() == type);
				AssertNotNull(row);
				AssertEquals(expectedTaxRate, row.Field<decimal?>("TaxRate"));
				AssertEquals(expectedExtraTaxRate, row.Field<decimal?>("ExtraTaxRate"));
			}
		}

		public void TestComplianceDocumentNumberAndSubType()
		{
			var apInv = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			var apInvLine = TestObjectCreator.CreateInvoiceLine(apInv, TestObjectCreator.AUD, 1M, 100M);
			apInvLine.AL_AT = TestObjectCreator.GST1.PK;
			var apInvoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine);
			apInvoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			apInvoiceDocumentHeader.ADH_ComplianceSubType = "TXI";
			apInvoiceDocumentHeader.ADH_ReportingPeriod = 201802;

			var apInv1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV002", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			apInv1.AH_TransactionReference = "12345678";
			apInv1.AH_ComplianceSubType = "TCR";
			var apInvLine1 = TestObjectCreator.CreateInvoiceLine(apInv1, TestObjectCreator.AUD, 1M, 100M);
			apInvLine1.AL_AT = TestObjectCreator.GST1.PK;

			var arInv = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV003", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			var arInvLine = TestObjectCreator.CreateInvoiceLine(arInv, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine.AL_AT = TestObjectCreator.GST1.PK;

			var arInvoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010003", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine);
			arInvoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader.ADH_ComplianceSubType = "TXI";
			arInvoiceDocumentHeader.ADH_ReportingPeriod = 201804;

			var arInv1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV004", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			arInv1.AH_TransactionReference = "87654321";
			arInv1.AH_ComplianceSubType = "TCR";
			var arInvLine1 = TestObjectCreator.CreateInvoiceLine(arInv1, TestObjectCreator.AUD, 1M, 100M);
			arInvLine1.AL_AT = TestObjectCreator.GST1.PK;

			Factory.Save();

			var result = RunScript();
			AssertEquals("Should have 4 rows", 4, result.Rows.Count);

			var inputRow = result.Rows[0];
			AssertEquals("ComplianceSubType", "TXI", inputRow["ComplianceSubType"]);
			AssertEquals("ComplianceDocumentNumber", "TX00090001", inputRow["ComplianceDocumentNumber"]);

			var inputRow1 = result.Rows[1];
			AssertEquals("ComplianceSubType", "TCR", inputRow1["ComplianceSubType"]);
			AssertEquals("ComplianceDocumentNumber", "12345678", inputRow1["ComplianceDocumentNumber"]);

			var inputRow2 = result.Rows[2];
			AssertEquals("ComplianceSubType", "TXI", inputRow2["ComplianceSubType"]);
			AssertEquals("ComplianceDocumentNumber", "T00010003", inputRow2["ComplianceDocumentNumber"]);

			var inputRow3 = result.Rows[3];
			AssertEquals("ComplianceSubType", "TCR", inputRow3["ComplianceSubType"]);
			AssertEquals("ComplianceDocumentNumber", "87654321", inputRow3["ComplianceDocumentNumber"]);
		}

		public void TestTransactionsWithVoidedComplianceDocumentHeader()
		{
			var arInv1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			var arInvLine1 = TestObjectCreator.CreateInvoiceLine(arInv1, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine1.AL_AT = TestObjectCreator.GST1.PK;

			var arInvoiceDocumentHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, "desc", arInvLine1);
			arInvoiceDocumentHeader1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader1.ADH_ComplianceSubType = "TDP";
			arInvoiceDocumentHeader1.ADH_ReportingPeriod = 201804;
			arInvoiceDocumentHeader1.Void();
			Factory.Save();

			var arInvoiceDocumentHeader2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010002", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, "desc", arInvLine1);
			arInvoiceDocumentHeader2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader2.ADH_ComplianceSubType = "TXC";
			arInvoiceDocumentHeader2.ADH_ReportingPeriod = 201804;
			Factory.Save();

			var arInv2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			var arInvLine2 = TestObjectCreator.CreateInvoiceLine(arInv2, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine2.AL_AT = TestObjectCreator.GST1.PK;

			var arInvoiceDocumentHeader3 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010003", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, "desc", arInvLine2);
			arInvoiceDocumentHeader3.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader3.ADH_ComplianceSubType = "TDP";
			arInvoiceDocumentHeader3.ADH_ReportingPeriod = 201804;
			arInvoiceDocumentHeader3.Void();
			Factory.Save();

			var arInvoiceDocumentHeader4 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010004", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, "desc", arInvLine2);
			arInvoiceDocumentHeader4.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader4.ADH_ComplianceSubType = "TDP";
			arInvoiceDocumentHeader4.ADH_ReportingPeriod = 201804;
			arInvoiceDocumentHeader4.Void();
			Factory.Save();

			var result = RunScript();
			AssertEquals("Should contain 2 transactions", 2, result.Rows.Count);
			AssertEquals("First transaction is for first invoice", arInv1.AH_TransactionNum, result.Rows[0]["AH_TransactionNum"]);
			AssertEquals("ComplianceDoumentNumber is of non-voided compliance document", "T00010002", result.Rows[0]["ComplianceDocumentNumber"]);
			AssertEquals("Compliance Sub Type is of non-voided compliance document", arInvoiceDocumentHeader2.ADH_ComplianceSubType, result.Rows[0]["ComplianceSubType"]);

			AssertEquals("Second transaction is for second invoice", arInv2.AH_TransactionNum, result.Rows[1]["AH_TransactionNum"]);
			AssertNullOrEmptyOrWhitespace("Transaction with voided compliance document has no ComplianceDocumentNumber", (string)result.Rows[1]["ComplianceDocumentNumber"]);
			AssertNullOrEmptyOrWhitespace("Transaction with voided compliance document has no Compliance Sub Type", (string)result.Rows[1]["ComplianceSubType"]);
		}

		public void TestHeaderAndLineTaxBranch()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var taxBranch = TestObjectCreator.CreateNewBranch(company, "TST");
			Factory.Save();

			var invoice = TestHelper.SetupInvoice("001", LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 300M, 100M, isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranch.PK;
			TestHelper.SetupInvoice("002", LedgerTypes.AccountsReceivable, TestObjectCreator.GST2.PK, 240M, 0M, isSaveWithFactory: false);
			TestHelper.SetupInvoice("003", LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 140M, 100M, isSaveWithFactory: false);
			Factory.Save();

			invoice = TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsReceivable, TestObjectCreator.GST2.PK, 300M, 100M, true, isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranch.PK;
			TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 200M, 50M, true, transactionNumber: "011", isSaveWithFactory: false);
			TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsReceivable, TestObjectCreator.GST1.PK, 100M, 20M, true, isSaveWithFactory: false);
			Factory.Save();

			AssertEquals("Pre-conditoin", "BNE", GlbBranch.CurrentBranch.GB_Code);

			var headers = new[] { "TransactionHeaderBranchCode", "TransactionHeaderTaxBranchCode", "AL_LineBranchCode", "AL_LineTaxBranchCode", "AH_Ledger", "TaxID", "AH_TransactionNum", "LocalAmount", "TAXAmount" };
			var lines = new[]
			{
				new object[] { "BNE", "TST", "BNE", "TST", "AP", "ZZGST1", "001", -150m, -15m },	// line-1 is with ex rate 2
				new object[] { "BNE", "TST", "BNE", "TST", "AP", "ZZGST1", "001", -100m, -10m }, // line-2 is with ex rate 1
				new object[] { "BNE", "TST", "BNE", "TST", "AR", "ZZGST2", "00001001", 83.33m, 16.67m }
			};

			var result = RunScript("ALL", headerTaxBranchList: taxBranch.PK.ToString());
			AssertDataTableAllRows("Header Tax Branch", result, headers, lines);

			result = RunScript("ALL", lineTaxBranchList: taxBranch.PK.ToString());
			AssertDataTableAllRows("Line Tax Branch", result, headers, lines);
		}

		public void TestHeaderAndLineTaxBranchWithMultipleTaxBranchFilter()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var taxBranchA = TestObjectCreator.CreateNewBranch(company, "TS1");
			var taxBranchB = TestObjectCreator.CreateNewBranch(company, "TS2");
			Factory.Save();

			var invoice = TestHelper.SetupInvoice("001", LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 300M, 100M, isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranchA.PK;

			invoice = TestHelper.SetupInvoice("002", LedgerTypes.AccountsReceivable, TestObjectCreator.GST2.PK, 240M, 0M, isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranchA.PK;
			invoice.Lines.OfType<InvoicingLineBase>().ForEach(x => x.AL_GB_TaxBranch = ZGuid.Empty); // For test only

			invoice = TestHelper.SetupInvoice("003", LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 140M, 100M, isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranchB.PK;

			invoice = TestHelper.SetupInvoice("004", LedgerTypes.AccountsReceivable, TestObjectCreator.GST1.PK, 160M, 0M, isSaveWithFactory: false);

			SaveWithCriticalValidationSuspended(Factory);

			invoice = TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsReceivable, TestObjectCreator.GST2.PK, 300M, 100M, true, isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranchA.PK;

			invoice = TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 200M, 50M, true, transactionNumber: "011", isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranchB.PK;

			invoice = TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsReceivable, TestObjectCreator.GST1.PK, 100M, 20M, true, isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranchB.PK;
			invoice.Lines.OfType<InvoicingLineBase>().ForEach(x => x.AL_GB_TaxBranch = ZGuid.Empty); // For test only

			invoice = TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsPayable, TestObjectCreator.GST2.PK, 160M, 0M, true, transactionNumber: "012", isSaveWithFactory: false);

			SaveWithCriticalValidationSuspended(Factory);

			AssertEquals("Pre-conditoin", "BNE", GlbBranch.CurrentBranch.GB_Code);

			var headers = new[] { "TransactionHeaderBranchCode", "TransactionHeaderTaxBranchCode", "AL_LineBranchCode", "AL_LineTaxBranchCode", "AH_Ledger", "TaxID", "AH_TransactionNum", "LocalAmount", "TAXAmount" };
			var lines = new[]
			{
				new object[] { "BNE", "TS2", "BNE", "TS2", "AP", "ZZGST1", "003", -70m, -7m },	// line-1 is with ex rate 2
				new object[] { "BNE", "TS2", "BNE", "TS2", "AP", "ZZGST1", "003", -100m, -10m }, // line-2 is with ex rate 1
				new object[] { "BNE", "TS2", "BNE", "TS2", "AP", "ZZGST1", "011", -45.45m, -4.55m }
			};

			var result = RunScript("ALL", headerTaxBranchList: $"{taxBranchA.PK.ToString()}, {taxBranchB.PK.ToString()}", lineTaxBranchList: taxBranchB.PK.ToString());
			AssertDataTableAllRows("Header Tax Branch", result, headers, lines);
		}

		[TestDate(2023, 12, 13)]
		public void TestIsDSBInvoice_WhenInvoiceTypeIsDisbursement()
		{
			//Arrange
			string[] disbursementInvoiceTypes = AccTransactionHeader.DisbursementInvoiceTypes;
			foreach (var type in disbursementInvoiceTypes)
			{
				var invoice = TestHelper.SetupInvoice(type + "-001", LedgerTypes.AccountsReceivable, TestObjectCreator.GST1.PK, 300M, 0M, isSaveWithFactory: false);
				invoice.AH_TransactionCategory = type;
			}

			Factory.Save();

			//Act
			var result = RunScript(LedgerTypes.AccountsReceivable);

			//Assert
			AssertEquals($"Should have {disbursementInvoiceTypes.Length} rows", disbursementInvoiceTypes.Length, result.Rows.Count);
			foreach (var row in result.AsEnumerable())
			{
				AssertEquals("IsDSBInvoice value should be true for all disbursement invoices", true, row.Field<bool>("IsDSBInvoice"));
			}
		}

		[TestDate(2023, 12, 13)]
		public void TestIsDSBInvoice_WhenInvoiceTypeIsNotDisbursement()
		{
			//Arrange
			var invoiceTypes = new InvoiceTypesList().GetAllCodes();
			var nonDisbursementInvoiceTypes = invoiceTypes.Where(t => !AccTransactionHeader.IsDisbursementInvoiceType(t));

			//test a few random types. they are not actual types, but they can be entered into data table records manually
			nonDisbursementInvoiceTypes = nonDisbursementInvoiceTypes.Concat(new[] { "", "   ", "???" }).ToList();
			foreach (var type in nonDisbursementInvoiceTypes)
			{
				var invoice = TestHelper.SetupInvoice(type + "-001", LedgerTypes.AccountsReceivable, TestObjectCreator.GST1.PK, 300M, 0M, isSaveWithFactory: false);
				invoice.AH_TransactionCategory = type;
			}

			Factory.Save();

			//Act
			var result = RunScript(LedgerTypes.AccountsReceivable);

			//Assert
			AssertEquals($"Should have {nonDisbursementInvoiceTypes.Count()} rows", nonDisbursementInvoiceTypes.Count(), result.Rows.Count);
			foreach (var row in result.AsEnumerable())
			{
				AssertEquals("IsDSBInvoice value should be false for all non-disbursement invoices", false, row.Field<bool>("IsDSBInvoice"));
			}
		}

		#region Implementation

		internal class TaxTransactionAnalysisReportTestHelper
		{
			public TaxTransactionAnalysisReportTestHelper(TestObjectCreator testObjectCreator)
			{
				TestObjectCreator = testObjectCreator;
			}

			public InvoicingBase SetupInvoice(string invoiceNumber, string invoiceType, ZGuid taxCodePK, ZDecimal lineAmount1, ZDecimal lineAmount2, bool withRecoverableVAT = false, decimal? inputVATRecoverable = null, bool isSaveWithFactory = true)
			{
				InvoicingBase invoice;

				if (invoiceType == LedgerTypes.AccountsReceivable)
				{
					invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceNumber, TestObjectCreator.AUD, 1M);
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				}
				else
				{
					invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), invoiceNumber, TestObjectCreator.AUD, 1M);
					invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
				}

				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, lineAmount1, GlbBranch.CurrentBranch.PK, false);
				line.AL_AT = taxCodePK;
				line.AL_Desc = "Tax line 1";
				if (withRecoverableVAT)
				{
					line.AL_InputGSTVATRecoverable = inputVATRecoverable != null ? inputVATRecoverable.Value : 0.8M;
				}

				if (lineAmount2 != 0M)
				{
					line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, lineAmount2, GlbBranch.CurrentBranch.PK, false);
					line.AL_AT = taxCodePK;
					line.AL_Desc = "Tax line 2";
				}

				if (isSaveWithFactory)
				{
					TestObjectCreator.Factory.Save();
				}

				return invoice;
			}

			public InvoicingBase SetupInvoiceWithCashVATLine(string invoiceType, ZGuid taxCodePK, ZDecimal lineAmount, ZDecimal paidAmount, bool withRecoverableVAT = false, decimal? inputVATRecoverable = null, string transactionNumber = null, bool isSaveWithFactory = true)
			{
				InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(invoiceType == "AR" ? typeof(ARInvoice) : typeof(APInvoice), lineAmount, 0, false);
				SetupCashVATLine(invoice, taxCodePK, paidAmount, withRecoverableVAT, inputVATRecoverable);

				if (!string.IsNullOrEmpty(transactionNumber))
				{
					invoice.AH_TransactionNum = transactionNumber;
				}

				if (isSaveWithFactory)
				{
					TestObjectCreator.Factory.Save();
				}

				return invoice;
			}

			public void SetupCashVATLine(InvoicingBase invoice, ZGuid taxCodePK, ZDecimal paidAmount, bool withRecoverableVAT = false, decimal? inputVATRecoverable = null)
			{
				var line = invoice.Lines[0];
				line.AL_AT = ZGuid.Empty;
				line.AL_AT = taxCodePK;
				line.AL_RX_NKTransactionCurrency = "USD";
				line.AL_ExchangeRate = 2m;
				if (withRecoverableVAT)
				{
					line.AL_InputGSTVATRecoverable = inputVATRecoverable != null ? inputVATRecoverable.Value : 0.8M;
				}

				AssertEquals("Precondition: AL_GSTVATBasis", AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code, line.AL_GSTVATBasis);

				if ((invoice.AH_Ledger == "AP" && invoice.AH_TransactionType != "CRD") || (invoice.AH_Ledger == "AR" && invoice.AH_TransactionType == "CRD"))
				{
					TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice, ZDateTime.Now, -paidAmount);
				}
				else
				{
					TestObjectCreator.CreateMatchLinkToPayARInvoice(invoice, ZDateTime.Now, paidAmount);
				}
			}

			readonly TestObjectCreator TestObjectCreator;
		}

		void SaveWithCriticalValidationSuspended(BusinessObjectFactory factory)
		{
			// In some UTs, we intend to create transactions with different line / header tax branch for testing purpose. Hence will skip CV check.
			using (new DisposableAction(() => SuspendCriticalValidationAttribute.IsActive = true, () => SuspendCriticalValidationAttribute.IsActive = false))
			{
				factory.Save();
			}
		}

		DataTable RunScript(string ledgerType = "", GlbCompany loginCompany = null, string headerBranchList = "", ZDateTime? transactionPostDate = null, string headerTaxBranchList = "", string lineTaxBranchList = "")
		{
			loginCompany = loginCompany ?? GlbCompany.CurrentCompany;
			string currentCountry = loginCompany.GC_RN_NKCountryCode;
			string currentCountryTaxRegistrationOrgCusCode = Country.GetConsumptionTaxRegistrationOrgCusCode(currentCountry);
			var reportDate = transactionPostDate ?? ZDateTime.Today;

			var result = DataUtils.GetDataTableFromQuery(Db.Connection, $@"
SELECT * 
FROM Report_TaxTransactionAnalysisbyTransLine(
'{currentCountry}',	--@CurrentCountry
'{loginCompany.PK}',	--@CompanyPK
NULL,		--@Period
'{reportDate.AddDays(-1).ToISO8601String()}',	--@StartDate
'{reportDate.AddDays(1).ToISO8601String()}',	--@EndDate
'',		--@InOutPut
'{ledgerType}',	--@TransactionLedger
'',		--@TaxID
'',		--@TaxMsg
'{headerBranchList}',	--@HeaderBranchList
'{headerTaxBranchList}',	--@HeaderTaxBranchList
'',		--@LineBranchList
'{lineTaxBranchList}',	--@LineTaxBranchList
'{currentCountryTaxRegistrationOrgCusCode}'	--@CurrentCountryTaxRegistrationOrgCusCode
) ORDER BY TransactionOrgCode
	-- To ensure correct ordering of rows in all tests, and assertion in AssertRowOrderWillNeverChange()
	, AH_TransactionNum
	, LineDescription
	, LocalAmount
"
				);
			AssertRowOrderWillNeverChange(result);
			return result;
		}

		void AssertRowOrderWillNeverChange(DataTable table)
		{
			var nonUniqueRows = table.Rows.Cast<DataRow>()
				.GroupBy(x => new
				{
					TransactionOrgCode = x.Field<string>("TransactionOrgCode"),
					AH_TransactionNum = x.Field<string>("AH_TransactionNum"),
					LineDescription = x.Field<string>("LineDescription"),
					LocalAmount = x.Field<decimal>("LocalAmount"),
				})
				.Where(x => x.Count() > 1);
			if (nonUniqueRows.Any())
			{
				var nonUniqueKeys = string.Join("\n", nonUniqueRows.Select(x => x.Key));
				Assert($@"{nonUniqueRows.Count()} non-unique rows detected in result set:
{nonUniqueKeys}

So that the order of result set rows is deterministic, your test data must be unique by TransactionOrgCode, AH_TransactionNum, LineDescription, LocalAmount.", false);
			}
		}

		TaxTransactionAnalysisReportTestHelper TestHelper => taxTransactionReportTestHelper ?? (taxTransactionReportTestHelper = new TaxTransactionAnalysisReportTestHelper(TestObjectCreator));
		TaxTransactionAnalysisReportTestHelper taxTransactionReportTestHelper;

		#endregion
	}
}


