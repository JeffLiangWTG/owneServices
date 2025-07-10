using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_TransactionSummaryVATAnalysisByReceivableAccountTest : ScriptTest
	{
		#region Brexit

		[TestDate(2019, 03, 19)]
		public void TestRunBeforeBrexitDate_OrgCountry()
		{
			var brexitDate = TestObjectCreator.SetupPostBrexitData();
			var orgHeader = TestObjectCreator.Debtor;
			SetupHeaderWithClosestPortAndCustomsCode(orgHeader, "GBLON", Core.Constants.CountryCodes.UnitedKingdom, "111111_GB", Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.UnitedKingdom));

			CreateInvoicesForBrexit(brexitDate, orgHeader);

			Factory.Save();

			AssertTransactionRegNoForBrexit(brexitDate, Core.Constants.CountryCodes.Germany, "GB111111_GB");
		}

		void CreateInvoicesForBrexit(ZDateTime brexitDate, OrgHeader orgHeader)
		{
			TestObjectCreator.SetupCashBasisVAT();

			CreateInvoice("001A", 10, brexitDate.AddDays(-1), AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code);

			var invoice = CreateInvoice("001C", 110, brexitDate.AddDays(-1), AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code);
			SetupCashVATLine(invoice, TestObjectCreator.GST1.PK, 11 + 11 * 0.1);

			CreateInvoice("002A", 20, brexitDate, AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code);

			invoice = CreateInvoice("002C", 220, brexitDate, AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code);
			SetupCashVATLine(invoice, TestObjectCreator.GST1.PK, 22 + 22 * 0.1);

			InvoicingBase CreateInvoice(string invoiceNumber, ZDecimal amount, ZDateTime postDate, string vatBasis)
			{
				var inv = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceNumber, TestObjectCreator.GBP, organisation: orgHeader);
				inv.AH_PostDate = postDate;
				var line = TestObjectCreator.CreateInvoiceLine(inv, TestObjectCreator.GLHeader1.PK, amount);
				line.AL_GSTVATBasis = vatBasis;

				return inv;
			}
		}

		void AssertTransactionRegNoForBrexit(ZDateTime transactionPostDate, string countryCodeToTemporarilySwitch, string transactionRegNoToAssert)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeToTemporarilySwitch))
			{
				var headers = new[] { "TotalTaxBase", "EUVATRegistrationNumber", "EUVATRegistrationCountry" };

				var result = RunScript(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, transactionPostDate: transactionPostDate);

				var lines = new[] {
					new object[] { 10m, transactionRegNoToAssert, "GB" },
					new object[] { 20m, string.Empty, "" },
				};

				AssertDataTableAllRowsByKeyColumns("", result, headers, lines);

				result = RunScript(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				lines = new[] {
					new object[] { 11m, transactionRegNoToAssert, "GB" },
					new object[] { 22m, string.Empty, "" },
				};

				AssertDataTableAllRowsByKeyColumns("", result, headers, lines);
			}
		}

		static void SetupHeaderWithClosestPortAndCustomsCode(OrgHeader header, string closestPort, string countryCode, ZString customsRegNo, string codeType)
		{
			header.OH_RL_NKClosestPort = closestPort;
			header.CompanyData.SetARTaxApplicable(true);
			var taxCode = header.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = countryCode;
			taxCode.OK_CustomsRegNo = customsRegNo;
			taxCode.OK_CodeType = codeType;
		}

		void SetupCashVATLine(InvoicingBase invoice, ZGuid taxCodePK, ZDecimal paidAmount)
		{
			var line = invoice.Lines[0];
			line.AL_AT = ZGuid.Empty;
			line.AL_AT = taxCodePK;

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

		#endregion

		public void TestCurrentCountryTaxRegistrationOrgCusCode()
		{
			GlbCompany fRLoginCompany = TestObjectCreator.CreateNewCompany("FRC", "FR");

			OrgHeader orgNLAMS = TestObjectCreator.CreateOrgHeader("NLAMS", false, true, "NLAMS");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgNLAMS, OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Constants.CountryCodes.Netherlands, "3123");
			OrgHeader orgGBLON = TestObjectCreator.CreateOrgHeader("GBLON", false, true, "GBLON");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgGBLON, OrgCusCode.CodeTypes.VATCode, Constants.CountryCodes.UnitedKingdom, "6546");
			OrgHeader orgFR2BC = TestObjectCreator.CreateOrgHeader("FR2BC", false, true, "FR2BC");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgFR2BC, OrgCusCode.FranceCodeTypes.TVA, Constants.CountryCodes.France, "1234");
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgFR2BC, ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, Constants.CountryCodes.Italy, "987842");
			Factory.Save();

			ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.AUD, 100M, orgNLAMS);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, GlbBranch.CurrentBranch.PK);
			invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV2", TestObjectCreator.AUD, 200M, orgGBLON);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M, GlbBranch.CurrentBranch.PK);
			invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV3", TestObjectCreator.AUD, 200M, orgFR2BC);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 300M, GlbBranch.CurrentBranch.PK);
			Factory.Save();

			DataTable result = RunScript(fRLoginCompany.GC_RN_NKCountryCode);

			AssertEquals("Result Rows.Count", 3, result.Rows.Count);
			DataRow[] gbRows = result.Select("RN_Code = 'GB'");
			AssertEquals("gbRows.Count", 1, gbRows.Length);
			AssertDataRow(gbRows[0], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "GB", "GB6546" });
			DataRow[] nlRows = result.Select("RN_Code = 'NL'");
			AssertEquals("nlRows.Count", 1, nlRows.Length);
			AssertDataRow(nlRows[0], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "NL", "NL3123" });
			DataRow[] frRows = result.Select("RN_Code = 'FR'");
			AssertEquals("gbRows.Count", 1, frRows.Length);
			AssertDataRow(frRows[0], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "FR", "FR1234" });
		}

		public void TestVatRegistrationNumberForEUN()
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

				DataTable result = RunScript(loginCompanyGR.GC_RN_NKCountryCode);
				AssertEquals("Result Rows.Count", 4, result.Rows.Count);

				DataRow[] frRows = result.Select("RN_Code = 'FR'");
				AssertEquals("frRows.Count", 4, frRows.Length);
				AssertDataRow(frRows[0], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "GR", "EL999999" });
				AssertDataRow(frRows[1], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "GR", "EL654321" });
				AssertDataRow(frRows[2], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "FR", "FRGR123456" });
				AssertDataRow(frRows[3], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "FR", "FR999999" });
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

				DataTable result = RunScript(loginCompanyIT.GC_RN_NKCountryCode);
				AssertEquals("Result Rows.Count", 6, result.Rows.Count);

				DataRow[] frRows = result.Select("RN_Code = 'FR'");
				AssertEquals("frRows.Count", 4, frRows.Length);
				AssertDataRow(frRows[0], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "IT", "IT999999" });
				AssertDataRow(frRows[1], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "IT", "IT999999" });
				AssertDataRow(frRows[2], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "FR", "FR123456" });
				AssertDataRow(frRows[3], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "FR", "FR123456" });

				DataRow[] grRows = result.Select("RN_Code = 'GR'");
				AssertEquals("grRows.Count", 2, grRows.Length);
				AssertDataRow(grRows[0], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "GR", "EL654321" });
				AssertDataRow(grRows[1], new[] { "EUVATRegistrationCountry", "EUVATRegistrationNumber" }, new[] { "GR", "EL654321" });
			}
		}

		public void TestGetTaxRateFromLine()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001",TestObjectCreator.AUD,15M,TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 40M);
			line.AL_AT = TestObjectCreator.GST1.PK;
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 25M);
			line2.AL_AT = TestObjectCreator.CAP.PK;
			Factory.Save();

			AssertResult();

			TestObjectCreator.GST1.SetRate_ForTestOnly(0, 1);
			Factory.Save();

			AssertResult();

			void AssertResult()
			{
				var result = RunScript(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals(65M, result.Rows[0].Field<decimal>("TotalTaxBaseRated"));
				AssertEquals(0M, result.Rows[0].Field<decimal>("TotalTaxBaseZeroRated"));
			}
		}

		DataTable RunScript(string currentCountryCode, ZDateTime? transactionPostDate = null)
		{
			var reportPostDate = transactionPostDate ?? ZDateTime.Today;

			return DataUtils.GetDataTableFromQuery(Db.Connection, $@"
SELECT * 
FROM Report_TransactionSummaryVATAnalysisByReceivableAccount(
'{GlbCompany.CurrentCompany.PK}',	--@CurrentCompany
'{reportPostDate.AddDays(-1).ToISO8601String()}',	--@PostDateFrom
'{reportPostDate.AddDays(1).ToISO8601String()}',	--@PostDateTo
'{ZDateTime.Today.AddDays(-1).ToISO8601String()}',	--@InvoiceDateFrom
'{ZDateTime.Today.AddDays(1).ToISO8601String()}',	--@InvoiceDateTo
'',		--@TaxType
'{Country.GetConsumptionTaxRegistrationOrgCusCode(currentCountryCode)}'	--@CurrentCountryTaxRegistrationOrgCusCode
)"
				);
		}
	}
}


