using System.Data;
using System.Linq;
using CargoWise.Data;
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
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_VATAnalysisAndSummaryForEUCompaniesTest : ScriptTest
	{
		#region Brexit

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
			SetupCashVATLine(invoice, TestObjectCreator.GST1.PK, 20);

			CreateInvoice("002A", brexitDate, AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code);

			invoice = CreateInvoice("002C", brexitDate, AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code);
			SetupCashVATLine(invoice, TestObjectCreator.GST1.PK, 20);

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
				var headers = new[] { "AH_TransactionNum", "EUVATRegistrationNumber", "DebtorCreditorCountry" };

				var result = RunScript(LedgerTypes.AccountsPayable, transactionPostDate: transactionPostDate);

				var lines = new[] {
					new object[] { "001A", transactionRegNoToAssert, "Other EU" },
					new object[] { "002A", string.Empty, "Outside EU" },
				};

				AssertDataTableAllRowsByKeyColumns("", result, headers, lines);

				result = RunScript(LedgerTypes.AccountsPayable);

				lines = new[] {
					new object[] { "001C", transactionRegNoToAssert, "Other EU" },
					new object[] { "002C", string.Empty, "Outside EU" },
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

		public void TestShowRowsHaveGSTVATButLineAmountZero()
		{
			DirectPayment payment = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 100M, 10M, 0M, 0M);
			DirectReceipt receipt = TestObjectCreator.CreateDirectReceipt(ZDateTime.Now, 0M, 50M, 0M, 200M);

			Factory.Save();

			DataTable result = RunScript(LedgerTypes.CashBook);
			AssertEquals("Should have 3 Rows", 3, result.Rows.Count);
			AssertEquals("Should Find Row With Zero Line Amount And Have GSTVAT Value", 2, result.Select("TaxBaseAmount = 0").Length);
		}

		public void TestEUVATRegistrationNumber()
		{
			GlbCompany loginCompanyEU = TestObjectCreator.CreateNewCompany("NNN", "NL");
			GlbBranch branchNL = TestObjectCreator.CreateBranch("NBR", "branch for NL", loginCompanyEU);
			Factory.Save();

			OrgHeader orgNLAMS = TestObjectCreator.CreateOrgHeader("NLAMS", false, true, "NLAMS");
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
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, GlbBranch.CurrentBranch.PK);
			line.AL_GB = branchNL.PK;
			invoice.AH_GB = branchNL.PK;
			Factory.Save();

			DataTable result = RunScript(LedgerTypes.CashBook, loginCompanyEU);
			AssertEquals("EU Login Country / Invoice 1 - Tax Registration Number for the current login country", "NL3123", result.Rows[0]["EUVATRegistrationNumber"]);

			ARInvoice invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV2", TestObjectCreator.AUD, 200M, orgFRLON);
			line = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
			line.AL_GB = branchNL.PK;
			invoice2.AH_GB = branchNL.PK;
			Factory.Save();

			result = RunScript(LedgerTypes.CashBook, loginCompanyEU);
			AssertEquals("EU Login Country / Invoice 2 - Fall back to Tax Registration Number for the home EU Country", "FR54564", result.Rows[1]["EUVATRegistrationNumber"]);

			ARInvoice invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV3", TestObjectCreator.AUD, 200M, orgAUABC);
			line = TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.AUD, 1M, 300M, GlbBranch.CurrentBranch.PK);
			line.AL_GB = branchNL.PK;
			invoice3.AH_GB = branchNL.PK;
			Factory.Save();

			result = RunScript(LedgerTypes.CashBook, loginCompanyEU);
			AssertEquals("EU Login Country / Invoice 3 - Fallback to Empty", "", result.Rows[2]["EUVATRegistrationNumber"]);
		}

		public void TestEUVATRegistrationNumberForEUN()
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
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, GlbBranch.CurrentBranch.PK);
				line.AL_GB = branchGR.PK;
				ARInvoice invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV2", TestObjectCreator.AUD, 200M, orgGRSKY);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
				line2.AL_GB = branchGR.PK;
				ARInvoice invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV3", TestObjectCreator.AUD, 200M, orgGRAPE);
				var line3 = TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
				line3.AL_GB = branchGR.PK;
				ARInvoice invoice4 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV4", TestObjectCreator.AUD, 200M, orgGRAPE2);
				var line4 = TestObjectCreator.CreateInvoiceLine(invoice4, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
				line4.AL_GB = branchGR.PK;
				Factory.Save();

				DataTable result = RunScript(LedgerTypes.CashBook, loginCompanyGR);
				AssertEquals("Result Rows.Count", 4, result.Rows.Count);
				AssertEquals("Greece Login Country / Invoice 1 - Tax Registration Number", "EL999999", result.Rows[0]["EUVATRegistrationNumber"]);
				AssertEquals("Greece Login Country / Invoice 2 - Tax Registration Number", "EL654321", result.Rows[1]["EUVATRegistrationNumber"]);
				AssertEquals("Greece Login Country / Invoice 3 - Tax Registration Number", "FRGR123456", result.Rows[2]["EUVATRegistrationNumber"]);
				AssertEquals("Greece Login Country / Invoice 4 - Tax Registration Number", "FR999999", result.Rows[3]["EUVATRegistrationNumber"]);
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

				ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.AUD, 100M, orgFRBAR);
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, GlbBranch.CurrentBranch.PK);
				line.AL_GB = branchIT.PK;
				ARInvoice invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV2", TestObjectCreator.AUD, 200M, orgFRBAR2);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
				line2.AL_GB = branchIT.PK;
				ARInvoice invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV3", TestObjectCreator.AUD, 200M, orgFRCAN);
				var line3 = TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
				line3.AL_GB = branchIT.PK;
				ARInvoice invoice4 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV4", TestObjectCreator.AUD, 200M, orgFRCAR2);
				var line4 = TestObjectCreator.CreateInvoiceLine(invoice4, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
				line4.AL_GB = branchIT.PK;
				ARInvoice invoice5 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV5", TestObjectCreator.AUD, 200M, orgFRCAR3);
				var line5 = TestObjectCreator.CreateInvoiceLine(invoice5, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
				line5.AL_GB = branchIT.PK;
				ARInvoice invoice6 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV6", TestObjectCreator.AUD, 200M, orgFRCAR4);
				var line6 = TestObjectCreator.CreateInvoiceLine(invoice6, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
				line6.AL_GB = branchIT.PK;
				Factory.Save();

				DataTable result = RunScript(LedgerTypes.CashBook, loginCompanyIT);
				AssertEquals("Result Rows.Count", 6, result.Rows.Count);
				AssertEquals("Italy Login Country / Invoice 1 - Tax Registration Number", "IT999999", result.Rows[0]["EUVATRegistrationNumber"]);
				AssertEquals("Italy Login Country / Invoice 2 - Tax Registration Number", "IT999999", result.Rows[1]["EUVATRegistrationNumber"]);
				AssertEquals("Italy Login Country / Invoice 3 - Tax Registration Number", "FR123456", result.Rows[2]["EUVATRegistrationNumber"]);
				AssertEquals("Italy Login Country / Invoice 4 - Tax Registration Number", "FR123456", result.Rows[3]["EUVATRegistrationNumber"]);
				AssertEquals("Italy Login Country / Invoice 5 - Tax Registration Number", "EL654321", result.Rows[4]["EUVATRegistrationNumber"]);
				AssertEquals("Italy Login Country / Invoice 6 - Tax Registration Number", "EL654321", result.Rows[5]["EUVATRegistrationNumber"]);
			}
		}

		public void TestTaxBaseAmountCalculatedProperly()
		{
			GlbCompany loginCompanyEU = TestObjectCreator.CreateNewCompany("NNN", "NL");
			GlbBranch branchNL = TestObjectCreator.CreateBranch("NBR", "branch for NL", loginCompanyEU);
			Factory.Save();

			OrgHeader orgNLAMS = TestObjectCreator.CreateOrgHeader("NLAMS", false, true, "NLAMS");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgNLAMS, OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Constants.CountryCodes.Netherlands, "3123");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgNLAMS, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Constants.CountryCodes.Australia, "41065894724");
			Factory.Save();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.AUD, 1m, orgNLAMS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, GlbBranch.CurrentBranch.PK);
			line.AL_GB = branchNL.PK;
			invoice.AH_GB = branchNL.PK;
			Factory.Save();

			DataTable result = RunScript(LedgerTypes.CashBook, loginCompanyEU);
			AssertEquals("TaxBaseAmount for Invoice only", 100m, ZDecimal.Parse(result.Rows[0]["TaxBaseAmount"].ToString()));

			var payment = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(invoice, ZDateTime.Now, 60m, "M0001");
			payment.AH_GB = invoice.AH_GB;
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			var cashBasis = Factory.New<AccCashBasisVAT>();
			cashBasis.YC_AL_TransactionLine = line.PK;
			cashBasis.YC_TaxBaseAmount = 60m;
			cashBasis.YC_TaxAmount = line.AL_GSTVAT;
			cashBasis.YC_PostDate = Factory.LoadTop1<AccTransactionMatchLink>(new CargoWise.EntityFramework.ZQuery(Enterprise.ZArchitecture.Schema.AccTransactionMatchLinkSchema.AP_MatchGroupNum, "M0001")).AP_MatchDate;
			cashBasis.YC_MatchGroupNum = "M0001";
			Factory.Save();

			result = RunScript(LedgerTypes.CashBook, loginCompanyEU);
			AssertEquals("TaxBaseAmount for Invoice only", 60m, ZDecimal.Parse(result.Rows[0]["TaxBaseAmount"].ToString()));
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

				var result = RunScript(LedgerTypes.CashBook);
				AssertEquals("Should have 2 rows", 2, result.Rows.Count);

				var inputRow = result.Rows[0];
				AssertEquals("INPUT - TaxBaseAmount", -123.456m, inputRow["TaxBaseAmount"]);
				AssertEquals("INPUT - TaxAmount", -12.346m, inputRow["TaxAmount"]);

				var outputRow = result.Rows[1];
				AssertEquals("OUTPUT - TaxBaseAmount", 123.456m, outputRow["TaxBaseAmount"]);
				AssertEquals("OUTPUT - TaxAmount", 12.346m, outputRow["TaxAmount"]);
			}
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

				var result = RunScript(LedgerTypes.CashBook);
				AssertEquals("Should have 2 rows", 2, result.Rows.Count);

				var inputRow = result.Rows[0];
				AssertEquals("INPUT - TaxBaseAmount", -1000m, inputRow["TaxBaseAmount"]);
				AssertEquals("INPUT - TaxAmount", -11m, inputRow["TaxAmount"]);

				var outputRow = result.Rows[1];
				AssertEquals("OUTPUT - TaxBaseAmount", 1000m, outputRow["TaxBaseAmount"]);
				AssertEquals("OUTPUT - TaxAmount", 11m, outputRow["TaxAmount"]);
			}
		}

		public void TestGST_WithRecoverableVAT_AR()
		{
			SetupInvoice("003", "AR", TestObjectCreator.GST1.PK, 100M, true);

			var headers = new[] { "TaxBaseAmount", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable" };
			var lines = new[] { new object[] { 100M, 10M, 10M, 0M } };

			DataTable result = RunScript("");
			AssertDataTableAllRows("AccountsReceivable", result, headers, lines);
		}

		public void TestGST_WithRecoverableVAT_AP()
		{
			SetupInvoice("004", "AP", TestObjectCreator.GST1.PK, 100M, true);

			var headers = new[] { "TaxBaseAmount", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable" };

			DataTable result = RunScript("");

			var lines = new[] { new object[] { -100M, -10M, -8M, -2M } };
			AssertDataTableAllRows("AccountsPayable", result, headers, lines);
		}

		public void TestGST_WithRecoverableVAT_WithCashVAT_AR()
		{
			SetupInvoiceWithCashVATLine("AR", TestObjectCreator.GST1.PK, 150M, 100M, true);

			var headers = new[] { "TaxBaseAmount", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable" };
			var lines = new[] { new object[] { 90.91M, 9.09M, 9.09M, 0M } };

			DataTable result = RunScript("");
			AssertDataTableAllRows("AccountsReceivable", result, headers, lines);
		}

		public void TestGST_WithRecoverableVAT_WithCashVAT_AP()
		{
			SetupInvoiceWithCashVATLine("AP", TestObjectCreator.GST1.PK, 150M, 100M, true);

			var headers = new[] { "TaxBaseAmount", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable" };

			DataTable result = RunScript("");

			var lines = new[] { new object[] { -90.91M, -9.09M, -7.27M, -1.82M } };
			AssertDataTableAllRows("AccountsPayable", result, headers, lines);
		}

		public void TestREV_WithRecoverableVAT_AR()
		{
			SetupInvoice("003", "AR", TestObjectCreator.REV.PK, 100M, true);

			var headers = new[] { "InputOutput", "TaxBaseAmount", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable" };
			var lines = new[] { new object[] { "OUTPUT", 100M, 0M, 0M, 0M } };

			DataTable result = RunScript("");
			AssertDataTableAllRowsByKeyColumns("AccountsReceivable", result, headers, lines);
		}

		public void TestREV_WithRecoverableVAT_AP()
		{
			SetupInvoice("004", "AP", TestObjectCreator.REV.PK, 100M, true);

			var headers = new[] { "InputOutput", "TaxBaseAmount", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable" };

			DataTable result = RunScript("");

			var lines = new[]
			{
				new object[] { "OUTPUT", 100M, 10M, 10M, 0M },
				new object[] { "INPUT", -100M, -10M, -8M, -2M }
			};
			AssertDataTableAllRowsByKeyColumns("AccountsPayable", result, headers, lines);
		}

		public void TestREV_WithRecoverableVAT_WithCashVAT_AR()
		{
			SetupInvoiceWithCashVATLine("AR", TestObjectCreator.REV.PK, 150M, 100M, true);

			var headers = new[] { "InputOutput", "TaxBaseAmount", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable" };
			var lines = new[] { new object[] { "OUTPUT", 100M, 0M, 0M, 0M } };

			DataTable result = RunScript("");
			AssertDataTableAllRowsByKeyColumns("AccountsReceivable", result, headers, lines);
		}

		public void TestREV_WithRecoverableVAT_WithCashVAT_AP()
		{
			SetupInvoiceWithCashVATLine("AP", TestObjectCreator.REV.PK, 150M, 100M, true);

			var headers = new[] { "InputOutput", "TaxBaseAmount", "TaxAmount", "TaxAmountRecoverable", "TaxAmountNotRecoverable" };

			DataTable result = RunScript("");

			var lines = new[]
			{
				new object[] { "OUTPUT", 100M, 10M, 10M, 0M },
				new object[] { "INPUT", -100M, -10M, -8M, -2M }
			};
			AssertDataTableAllRowsByKeyColumns("AccountsPayable", result, headers, lines);
		}

		public void TestConsolidatedInvoiceRefField_TransactionReferenceField()
		{
			var expectedConsolidatedInvoiceRef = "00001000";
			var expectedTransactionReference = "TrxRef456";

			ZGuid expectedTax = TestObjectCreator.INP7.PK;
			APInvoice invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1.0m, 100.00m, 0.00m, 7.00m, 100.00m, 0.00m, 7.00m);
			invoice.Lines[0].AL_AT = expectedTax;
			invoice.AH_TransactionReference = expectedTransactionReference;

			DirectPayment payment = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 100M, 10M, 0M, 0M);
			payment.AH_ConsolidatedInvoiceRef = expectedConsolidatedInvoiceRef;
			payment.AH_TransactionReference = expectedTransactionReference;

			DirectReceipt receipt = TestObjectCreator.CreateDirectReceipt(ZDateTime.Now, 100m, 10m, 4000m, 400m);
			receipt.AH_ConsolidatedInvoiceRef = expectedConsolidatedInvoiceRef;
			receipt.AH_TransactionReference = expectedTransactionReference;
			Factory.Save();
			var result = RunScript(LedgerTypes.CashBook).AsEnumerable();
			AssertEquals("Should have 4 Rows", 4, result.Count());
			var row = result.FirstOrDefault(x => x.Field<string>("TaxID") == "ZZINP7");
			AssertNotNull(row);
			AssertEquals("INV/CRD/ADJ must return ConsolidatedInvoiceRef as set i.e 00001000", expectedConsolidatedInvoiceRef, row.Field<string>("ConsolidatedInvoiceRef"));
			AssertEquals("INV/CRD/ADJ must return TransactionReference as set i.e. TrxRef456", expectedTransactionReference, row.Field<string>("ComplianceDocumentNumber"));

			foreach (var dataRow in result.Where(x => x.Field<string>("TaxID") != "ZZINP7"))
			{
				AssertEquals("DPY/DRC must return ConsolidatedInvoiceRef = empty", ZString.Empty, dataRow.Field<string>("ConsolidatedInvoiceRef"));
				AssertEquals("DPY/DRC must return TransactionReference = empty", ZString.Empty, dataRow.Field<string>("ComplianceDocumentNumber"));
			}
		}

		public void TestDebtorCreditorCountryLocation_AH_OA_InvoiceAddressOverrideNotNull()
		{
			var company = TestObjectCreator.CreateNewCompany("DGB", "GB");
			company.GC_Name = "Great Britain Company";
			var branch = TestObjectCreator.CreateBranch("LON", "branch for GB", company);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var org = TestObjectCreator.CreateOrgHeader("Test1", true, false, "AUSYD");

				var address1 = org.Addresses.AddNew();
				address1.OA_RL_NKRelatedPortCode = "AUSYD";
				address1.OA_State = "NSW";
				address1.OA_Address1 = "SYDNEY";

				var address2 = org.Addresses.AddNew();
				address2.OA_RL_NKRelatedPortCode = "ITMIL";
				address2.OA_State = "LBD";
				address2.OA_Address1 = "MILAN";

				var address3 = org.Addresses.AddNew();
				address3.OA_RL_NKRelatedPortCode = "GBLON";
				address3.OA_State = "LND";
				address3.OA_Address1 = "LONDON";

				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV1", TestObjectCreator.AUD, 1.0m, 100M, 0.00m, 7.00m, 100.00m, 0.00m, 7.00m, org);
				Factory.Save();

				//Outside EU : AU
				invoice.AH_OA_InvoiceAddressOverride = address1.PK;
				Factory.Save();

				var result = RunScript("AP", company);
				var headers = new[] { "DebtorCreditorCountry" };
				var lines = new[] { new object[] { "Outside EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//Other EU : IT
				invoice.AH_OA_InvoiceAddressOverride = address2.PK;
				Factory.Save();

				result = RunScript("AP", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Other EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//Great Britain Company : GB
				invoice.AH_OA_InvoiceAddressOverride = address3.PK;
				Factory.Save();

				result = RunScript("AP", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Great Britain Company" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);
			}
		}

		public void TestDebtorCreditorCountryLocation_AH_OA_InvoiceAddressOverrideIsNull()
		{
			var company = TestObjectCreator.CreateNewCompany("DGB", "GB");
			company.GC_Name = "Great Britain Company";
			var branch = TestObjectCreator.CreateBranch("LON", "branch for GB", company);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var org = TestObjectCreator.CreateOrgHeader("Test1", true, false, "AUSYD");
				org.OH_IsGlobalAccount = true;

				var address1 = org.Addresses.AddNew();
				address1.OA_RL_NKRelatedPortCode = "AUSYD";
				address1.OA_State = "NSW";
				address1.OA_Address1 = "SYDNEY";

				var address2 = org.Addresses.AddNew();
				address2.OA_RL_NKRelatedPortCode = "ITMIL";
				address2.OA_State = "LBD";
				address2.OA_Address1 = "MILAN";

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.AUD, 100M, org);
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, GlbBranch.CurrentBranch.PK);
				line.AL_GB = branch.PK;

				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				invoice.AH_JH = job.PK;
				Factory.Save();

				//Ledger: AR
				//JH_OA_LocalChargesAddr
				invoice.Job.JH_OA_LocalChargesAddr = address1.PK;
				Factory.Save();

				var result = RunScript("AR", company);
				var headers = new[] { "DebtorCreditorCountry" };
				var lines = new[] { new object[] { "Outside EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//JH_OA_AgentCollectAddr
				invoice.Job.JH_OA_LocalChargesAddr = ZGuid.Empty;
				invoice.Job.JH_OA_AgentCollectAddr = address2.PK;
				Factory.Save();

				result = RunScript("AR", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Other EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//Capabilty: Office
				invoice.Job.JH_OA_LocalChargesAddr = ZGuid.Empty;
				invoice.Job.JH_OA_AgentCollectAddr = ZGuid.Empty;
				address1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
				Factory.Save();

				result = RunScript("AR", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Outside EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//Capabilty: Postal
				invoice.Job.JH_OA_LocalChargesAddr = ZGuid.Empty;
				invoice.Job.JH_OA_AgentCollectAddr = ZGuid.Empty;
				address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Postal);
				Factory.Save();

				result = RunScript("AR", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Other EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//Capabilty: Receivables
				invoice.Job.JH_OA_LocalChargesAddr = ZGuid.Empty;
				invoice.Job.JH_OA_AgentCollectAddr = ZGuid.Empty;
				address1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
				Factory.Save();

				result = RunScript("AR", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Outside EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//Login company
				var address3 = org.Addresses.AddNew();
				address3.OA_RL_NKRelatedPortCode = "GBLON";
				address3.OA_State = "LND";
				address3.OA_Address1 = "LONDON";

				address1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
				address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
				address3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
				Factory.Save();

				result = RunScript("AR", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Great Britain Company" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);
			}
		}

		public void TestDebtorCreditorCountryLocation_Language()
		{
			var company = TestObjectCreator.CreateNewCompany("DGB", "GB");
			company.GC_Name = "Great Britain Company";
			var branch = TestObjectCreator.CreateBranch("LON", "branch for GB", company);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var org = TestObjectCreator.CreateOrgHeader("Test1", true, false, "AUSYD");

				var address1 = org.Addresses.AddNew();
				address1.OA_RL_NKRelatedPortCode = "AUSYD";
				address1.OA_State = "NSW";
				address1.OA_Address1 = "SYDNEY";
				address1.OA_Language = "111";
				address1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

				var address2 = org.Addresses.AddNew();
				address2.OA_RL_NKRelatedPortCode = "ITMIL";
				address2.OA_State = "LBD";
				address2.OA_Address1 = "MILAN";
				address2.OA_Language = "222";
				address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

				var address3 = org.Addresses.AddNew();
				address3.OA_RL_NKRelatedPortCode = "CNAAA";
				address3.OA_State = "AAA";
				address3.OA_Address1 = "AAAAA";
				address3.OA_Language = "333";
				address3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV1", TestObjectCreator.AUD, 1.0m, 100M, 0.00m, 7.00m, 100.00m, 0.00m, 7.00m, org);
				Factory.Save();

				//Outside EU
				org.OH_Language = "111";
				Factory.Save();

				var result = RunScript("AP", company);
				var headers = new[] { "DebtorCreditorCountry" };
				var lines = new[] { new object[] { "Outside EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//Other EU
				org.OH_Language = "222";
				Factory.Save();

				result = RunScript("AP", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Other EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//Great Britain Company
				org.OH_Language = "333";
				Factory.Save();

				result = RunScript("AP", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Outside EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);

				//Default language: EN-US
				org.OH_Language = Core.SharedConstants.Languages.ChineseSimplified;
				address2.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
				Factory.Save();

				result = RunScript("AP", company);
				headers = new[] { "DebtorCreditorCountry" };
				lines = new[] { new object[] { "Other EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);
			}
		}

		public void TestDebtorCreditorCountryLocation_OA_RL_NKRelatedPortCodeIsEmpty()
		{
			var company = TestObjectCreator.CreateNewCompany("DGB", "GB");
			company.GC_Name = "Great Britain Company";
			var branch = TestObjectCreator.CreateBranch("LON", "branch for GB", company);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var org = TestObjectCreator.CreateOrgHeader("Test1", true, false, "ITMIL");
				org.OH_RL_NKClosestPort = "ITMIL";

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.AUD, 100M, org);
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, GlbBranch.CurrentBranch.PK);
				line.AL_GB = branch.PK;

				Factory.Save();

				var sql = string.Format("Update dbo.OrgAddress Set OA_RL_NKRelatedPortCode = '' Where OA_OH = '{0}'", org.PK);
				Db.Connection.ExecuteNonQuery(sql);

				var result = RunScript("AR", company);
				var headers = new[] { "DebtorCreditorCountry" };
				var lines = new[] { new object[] { "Other EU" } };
				AssertDataTableAllRows("AccountsPayable", result, headers, lines);
			}
		}

		public void TestGetSectionFromLine()
		{
			var invoice = SetupInvoice("INV01", "AR", TestObjectCreator.GST1.PK, 100M, true);
			AssertSectionValue(TestObjectCreator.GST1.AT_Code, "Has VAT");

			Factory.Save();
			AssertSectionValue(TestObjectCreator.GST1.AT_Code, "Has VAT");

			invoice = SetupInvoice("INV02", "AR", TestObjectCreator.GSTFREE1.PK, 100, true);
			AssertSectionValue(TestObjectCreator.GSTFREE1.AT_Code, "No VAT");

			AssertSectionValue(TestObjectCreator.GSTFREE1.AT_Code, "No VAT");

			void AssertSectionValue(string code, string expectedValue)
			{
				var result = RunScript("").AsEnumerable();
				var row = result.FirstOrDefault(x => x.Field<string>("TaxID") == code);
				AssertNotNull(row);
				AssertEquals(expectedValue, row.Field<string>("Section"));
			}
		}

		public void TestGetTaxRateFromLineAndNotFromRefDb()
		{
			var inv = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1M);
			inv.AH_OH = TestObjectCreator.AALSHI.PK;
			var line = TestObjectCreator.CreateInvoiceLine(inv, TestObjectCreator.AUD, 1M, 100m);
			line.AL_AT = TestObjectCreator.GSTANDQST1.PK;
			line.AL_TaxRateNumerator = 6;
			line.AL_TaxRateDenominator = 1;
			line.AL_TaxExtraRateNumerator = 8;
			line.AL_TaxExtraRateDenominator = 1;
			Factory.Save();

			AssertEquals(9.5m, TestObjectCreator.GSTANDQST1.GetExtraRate_ForTestOnly());
			AssertEquals(5m, TestObjectCreator.GSTANDQST1.GetRate_ForTestOnly());

			var result = RunScript("").AsEnumerable();
			var row = result.FirstOrDefault(x => x.Field<string>("TaxID") == TestObjectCreator.GSTANDQST1.AT_Code);
			AssertNotNull(row);
			AssertEquals(6m, row.Field<decimal>("TaxRate"));
			AssertEquals(8m, row.Field<decimal>("ExtraTaxRate"));
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

		InvoicingBase SetupInvoice(string invoiceNumber, string invoiceType, ZGuid taxCodePK, ZDecimal lineAmount, bool withRecoverableVAT = false, decimal? inputVATRecoverable = null)
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

			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, lineAmount);
			line.AL_AT = taxCodePK;
			if (withRecoverableVAT)
			{
				line.AL_InputGSTVATRecoverable = inputVATRecoverable != null ? inputVATRecoverable.Value : 0.8M;
			}

			Factory.Save();

			return invoice;
		}

		void SetupInvoiceWithCashVATLine(string invoiceType, ZGuid taxCodePK, ZDecimal lineAmount, ZDecimal paidAmount, bool withRecoverableVAT = false, decimal? inputVATRecoverable = null)
		{
			InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(invoiceType == "AR" ? typeof(ARInvoice) : typeof(APInvoice), lineAmount, 0, false);
			SetupCashVATLine(invoice, taxCodePK, paidAmount, withRecoverableVAT, inputVATRecoverable);

			Factory.Save();
		}

		void SetupCashVATLine(InvoicingBase invoice, ZGuid taxCodePK, ZDecimal paidAmount, bool withRecoverableVAT = false, decimal? inputVATRecoverable = null)
		{
			var line = invoice.Lines[0];
			line.AL_AT = ZGuid.Empty;
			line.AL_AT = taxCodePK;
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

		DataTable RunScript(string ledgerType = "", GlbCompany loginCompany = null, ZDateTime? transactionPostDate = null)
		{
			loginCompany = loginCompany ?? GlbCompany.CurrentCompany;
			string currentCountry = loginCompany.GC_RN_NKCountryCode;
			string currentCountryTaxRegistrationOrgCusCode = Country.GetConsumptionTaxRegistrationOrgCusCode(currentCountry);
			var reportDate = transactionPostDate ?? ZDateTime.Today;

			return DataUtils.GetDataTableFromQuery(Db.Connection, $@"
SELECT * 
FROM Report_VATAnalysisAndSummaryForEUCompanies(	
'{currentCountry}',	--@CurrentCountry
'{loginCompany.GC_Name}',	--@CurrentCountryName
'{loginCompany.PK}',	--@CompanyPK
NULL,	--@Period
'{reportDate.AddDays(-1).ToISO8601String()}',	--@StartDate
'{reportDate.AddDays(1).ToISO8601String()}',	--@EndDate
'{ledgerType}',	--@TransactionLedger
'',		--@TaxID
'',		--@BranchList
'{currentCountryTaxRegistrationOrgCusCode}',	--@CurrentCountryTaxRegistrationOrgCusCode
null	--@BranchPK
)"
				);
		}
	}
}

