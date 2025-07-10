using System;
using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	public class MultiCompaniesGLJournalFlatFileConverterTest : MultiCompaniesGLJournalFlatFileConverterBaseTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;
			var sY1Branch = testObjectCreator.CreateNewBranch(company, "SY1");

			testObjectCreator.CreateTestPeriodsForEntireYear(company, 2022);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var valueObject = AssertImport_NoError("ValidGJLJournal.csv", 2);

				var firstGLJournal = valueObject[0];
				var firstGLJDetail = firstGLJournal.GLDetail;
				AssertEquals("SY1", firstGLJDetail.Branch);
				AssertEquals("GJL", firstGLJDetail.JournalType.ToString());
				AssertEquals("DEPRECIATION OF COMPUTER EQUIPMENT FOR THE MONTH OF JUNE1", firstGLJDetail.Description);
				AssertEquals("202206", firstGLJDetail.InPeriod);
				AssertEquals("202207", firstGLJDetail.OutPeriod);
				AssertEquals("ELM", firstGLJDetail.Presentation);
				AssertEquals(ZDate.Empty, firstGLJDetail.InPeriodDate);
				AssertEquals(ZDate.Empty, firstGLJDetail.OutPeriodDate);
				AssertEquals("First GL Jounal should have 2 lines.", 2, firstGLJournal.JournalLines.Count);
				var firstGLJLine0 = firstGLJournal.JournalLines[0];
				AssertEquals("2010.00.00", firstGLJLine0.Account);
				AssertEquals("SY1", firstGLJLine0.Branch);
				AssertEquals("BRN", firstGLJLine0.Department);
				AssertEquals(new ZDecimal(20500), firstGLJLine0.LocalAmount.Value);
				AssertEquals("CNY", firstGLJLine0.Currency);
				AssertEquals(new ZDecimal(51000), firstGLJLine0.Amount.Value);
				AssertEquals("DR", firstGLJLine0.DRCR.ToString());
				AssertEquals("Description1", firstGLJLine0.Description);
				var firstGLJSubAccs0 = firstGLJLine0.SubAccounts;
				AssertEquals("First line of the first journal has 4 sub account.", 4, firstGLJSubAccs0.Count);
				AssertEquals("ORG", firstGLJSubAccs0[0].Type.Code);
				AssertEquals("2010.00.01", firstGLJSubAccs0[0].Code);
				AssertEquals("SEG", firstGLJSubAccs0[1].Type.Code);
				AssertEquals("2010.00.02", firstGLJSubAccs0[1].Code);
				AssertEquals("STR", firstGLJSubAccs0[2].Type.Code);
				AssertEquals("AAA", firstGLJSubAccs0[2].Code);
				AssertEquals("SGP", firstGLJSubAccs0[3].Type.Code);
				AssertEquals("2010.00.04", firstGLJSubAccs0[3].Code);
				AssertEquals("Org1", firstGLJLine0.Organisation.EDICode);
				var firstGLJLine1 = firstGLJournal.JournalLines[1];
				AssertEquals("2020.00.00", firstGLJLine1.Account);
				AssertEquals("SY1", firstGLJLine1.Branch);
				AssertEquals("BRN", firstGLJLine1.Department);
				AssertEquals(new ZDecimal(20500), firstGLJLine1.LocalAmount.Value);
				AssertEquals("CNY", firstGLJLine1.Currency);
				AssertEquals(new ZDecimal(51000), firstGLJLine1.Amount.Value);
				AssertEquals("CR", firstGLJLine1.DRCR.ToString());
				AssertEquals("Description2", firstGLJLine1.Description);
				var firstGLJSubAccs1 = firstGLJLine1.SubAccounts;
				AssertEquals("Second line of the first journal has 4 sub account.", 4, firstGLJSubAccs1.Count);
				AssertEquals("ORG", firstGLJSubAccs1[0].Type.Code);
				AssertEquals("2020.00.01", firstGLJSubAccs1[0].Code);
				AssertEquals("SEG", firstGLJSubAccs1[1].Type.Code);
				AssertEquals("2020.00.02", firstGLJSubAccs1[1].Code);
				AssertEquals("STR", firstGLJSubAccs1[2].Type.Code);
				AssertEquals("BBB", firstGLJSubAccs1[2].Code);
				AssertEquals("SGP", firstGLJSubAccs1[3].Type.Code);
				AssertEquals("2020.00.04", firstGLJSubAccs1[3].Code);
				AssertEquals("Org2", firstGLJLine1.Organisation.EDICode);

				var secondGLJournal = valueObject[1];
				var secondGLJDetail = secondGLJournal.GLDetail;
				AssertEquals("TST", secondGLJDetail.Branch);
				AssertEquals("GJL", secondGLJDetail.JournalType.ToString());
				AssertEquals("DEPRECIATION OF COMPUTER EQUIPMENT FOR THE MONTH OF JUNE2", secondGLJDetail.Description);
				AssertEquals("202206", secondGLJDetail.InPeriod);
				AssertEquals("202207", secondGLJDetail.OutPeriod);
				AssertEquals("ELM", secondGLJDetail.Presentation);
				AssertEquals(ZDate.Empty, secondGLJDetail.InPeriodDate);
				AssertEquals(ZDate.Empty, secondGLJDetail.OutPeriodDate);
				AssertEquals("Second GL Jounal should have 2 lines.", 2, secondGLJournal.JournalLines.Count);
				var secondGLJLine0 = secondGLJournal.JournalLines[0];
				AssertEquals("2010.00.00", secondGLJLine0.Account);
				AssertEquals("TST", secondGLJLine0.Branch);
				AssertEquals("BRN", secondGLJLine0.Department);
				AssertEquals(new ZDecimal(20500), secondGLJLine0.LocalAmount.Value);
				AssertEquals("AUD", secondGLJLine0.Currency);
				AssertEquals(new ZDecimal(51000), secondGLJLine0.Amount.Value);
				AssertEquals("DR", secondGLJLine0.DRCR.ToString());
				AssertEquals("Description3", secondGLJLine0.Description);
				var secondGLJSubAccs0 = secondGLJLine0.SubAccounts;
				AssertEquals("First line of the first journal has 4 sub account.", 4, secondGLJSubAccs0.Count);
				AssertEquals("ORG", secondGLJSubAccs0[0].Type.Code);
				AssertEquals("2010.00.05", secondGLJSubAccs0[0].Code);
				AssertEquals("SEG", secondGLJSubAccs0[1].Type.Code);
				AssertEquals("2010.00.06", secondGLJSubAccs0[1].Code);
				AssertEquals("STR", secondGLJSubAccs0[2].Type.Code);
				AssertEquals("CCC", secondGLJSubAccs0[2].Code);
				AssertEquals("SGP", secondGLJSubAccs0[3].Type.Code);
				AssertEquals("2010.00.08", secondGLJSubAccs0[3].Code);
				AssertEquals("Org3", secondGLJLine0.Organisation.EDICode);
				var secondGLJLine1 = secondGLJournal.JournalLines[1];
				AssertEquals("2020.00.00", secondGLJLine1.Account);
				AssertEquals("TST", secondGLJLine1.Branch);
				AssertEquals("BRN", secondGLJLine1.Department);
				AssertEquals(new ZDecimal(20500), secondGLJLine1.LocalAmount.Value);
				AssertEquals("AUD", secondGLJLine1.Currency);
				AssertEquals(new ZDecimal(51000), secondGLJLine1.Amount.Value);
				AssertEquals("CR", secondGLJLine1.DRCR.ToString());
				AssertEquals("Description4", secondGLJLine1.Description);
				var secondGLJSubAccs1 = secondGLJLine1.SubAccounts;
				AssertEquals("Second line of the second journal has 4 sub account.", 4, secondGLJSubAccs1.Count);
				AssertEquals("ORG", secondGLJSubAccs1[0].Type.Code);
				AssertEquals("2020.00.05", secondGLJSubAccs1[0].Code);
				AssertEquals("SEG", secondGLJSubAccs1[1].Type.Code);
				AssertEquals("2020.00.06", secondGLJSubAccs1[1].Code);
				AssertEquals("STR", secondGLJSubAccs1[2].Type.Code);
				AssertEquals("DDD", secondGLJSubAccs1[2].Code);
				AssertEquals("SGP", secondGLJSubAccs1[3].Type.Code);
				AssertEquals("2020.00.08", secondGLJSubAccs1[3].Code);
				AssertEquals("Org4", secondGLJLine1.Organisation.EDICode);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithORG()
		{
			testObjectCreator.CreateOrgHeader("org1", false, true);
			var validAttributeFileName = "ValidGLJournalWithORG.csv";
			var errorMessage = "Invalid Attribute Value. A valid Attribute Value must be the code of a valid Receivable Organization (if Parent Account is an AR Control Account) or a valid Payable Organization (if Parent Account is an AP Control Account).";
			var inValidAttributeFileName = "ValidGLJournalWithInvalidORG.csv";
			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, inValidAttributeFileName, errorMessage, true);
			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, inValidAttributeFileName, errorMessage, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithOCG()
		{
			var validAttributeFileName = "ValidGLJournalWithOCG.csv";
			var errorMessage = "Invalid Attribute Value. A valid Attribute Value must be one of the following Class defined in Consolidated Accounting Category List registry or value 'NAV'.";
			var inValidAttributeFileName = "ValidGLJournalWithInvalidOCG.csv";

			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, inValidAttributeFileName, errorMessage, true);
			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, inValidAttributeFileName, errorMessage, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithLFO()
		{
			var validAttributeFileName = "ValidGLJournalWithLFO.csv";
			var errorMessage = "Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC' ,'FOR' or 'NAV'.\r\nLOC = Local.\r\nFOR = Foreign.\r\nNAV = No Attribute Value.";
			var inValidAttributeFileName = "ValidGLJournalWithInvalidLFO.csv";

			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, inValidAttributeFileName, errorMessage, true);
			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, inValidAttributeFileName, errorMessage, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithLFE()
		{
			var validAttributeFileName = "ValidGLJournalWithLFE.csv";
			var errorMessage = "Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC', 'WEU' ,'OEU' or 'NAV'.\r\nLOC = Local.\r\nWEU = Within EU.\r\nOEU = Outside EU.\r\nNAV = No Attribute Value.";
			var inValidAttributeFileName = "ValidGLJournalWithInvalidLFE.csv";

			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, inValidAttributeFileName, errorMessage, true);
			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, inValidAttributeFileName, errorMessage, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithTIC()
		{
			var validAttributeFileName = "ValidGLJournalWithTIC.csv";
			var errorMessage = "Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'STI' or 'ETI' or 'NAV'.\r\nSTI = Standard Tax IDs.\r\nETI = Tax ID with Extra Tax.\r\nNAV = No Attribute Value.";
			var inValidAttributeFileName = "ValidGLJournalWithInvalidTIC.csv";

			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, inValidAttributeFileName, errorMessage, true);
			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, inValidAttributeFileName, errorMessage, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithSPR()
		{
			var validAttributeFileName = "ValidGLJournalWithSPR.csv";
			var errorMessage = "Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'SPS' , 'SPR' or 'NAV'.\r\nSPS = Sales/Purchases.\r\nSPR = Sales/Purchases Returns.\r\nNAV = No Attribute Value.";
			var inValidAttributeFileName = "ValidGLJournalWithInvalidSPR.csv";

			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, inValidAttributeFileName, errorMessage, true);
			TestImportAttribute(validAttributeFileName, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, inValidAttributeFileName, errorMessage, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		void TestImportAttribute(string validAttributeFileName, string attr, string inValidAttributeFileName, string inValidErrorMessage, bool enableReportingBooksFeature)
		{
			var result = new GLJournalCollection();
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableReportingBooksFeature);

			if (enableReportingBooksFeature)
			{
				AssertImport_ErrorMessage(validAttributeFileName, GetErrorMessageWithLocation(1, 1, false, "EDI", attr + " attribute value cannot be specified for GL Account '2010.00.00'. Please check your dissection configuration."));
			}
			else
			{
				ImportNoError(validAttributeFileName);
			}

			if (!GlHeader.AlternateGLAccountDissections.Any())
			{
				SetUpForImportAttribute(attr);
			}

			var dissection = GlHeader.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().First();

			if (attr != AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
			{
				ImportNoError(validAttributeFileName);
				AssertImport_NoError("ValidGJLJournalWithEmptyAttribute.csv", 1);
			}
			else
			{
				if (enableReportingBooksFeature)
				{
					AssertImport_ErrorMessage(validAttributeFileName, GetErrorMessageWithLocation(1, 1, false, "EDI", inValidErrorMessage));
				}
				else
				{
					ImportNoError(validAttributeFileName);
				}

				AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlHeader.PK.ToGuid());
				Factory.Save();
				ImportNoError(validAttributeFileName);
			}
			dissection.ADC_SeparateNumbering = true;
			Factory.Save();
			ImportNoError(validAttributeFileName);

			if (enableReportingBooksFeature)
			{
				AssertImport_ErrorMessage("ValidGJLJournalWithEmptyAttribute.csv", GetErrorMessageWithLocation(1, 1, false, "EDI", attr + " attribute value must be specified for GL Account '2010.00.00' because separate numbering is required. Please check your dissection configuration."));
				AssertImport_ErrorMessage(inValidAttributeFileName, GetErrorMessageWithLocation(1, 1, false, "EDI", inValidErrorMessage));
			}
			else
			{
				AssertImport_NoError("ValidGJLJournalWithEmptyAttribute.csv", 1);
				ImportNoError(inValidAttributeFileName);
			}

			void ImportNoError(string filename)
			{
				var result = AssertImport_NoError(filename, 1);
				switch (attr)
				{
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG:
						AssertEquals(enableReportingBooksFeature, result.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => !string.IsNullOrEmpty(y.ORG))));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG:
						AssertEquals(enableReportingBooksFeature, result.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => !string.IsNullOrEmpty(y.OCG))));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE:
						AssertEquals(enableReportingBooksFeature, result.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => !string.IsNullOrEmpty(y.LFE))));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO:
						AssertEquals(enableReportingBooksFeature, result.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => !string.IsNullOrEmpty(y.LFO))));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC:
						AssertEquals(enableReportingBooksFeature, result.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => !string.IsNullOrEmpty(y.TIC))));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR:
						AssertEquals(enableReportingBooksFeature, result.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => !string.IsNullOrEmpty(y.SPR))));
						break;
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithPostDate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;
			var sY1Branch = testObjectCreator.CreateNewBranch(company, "SY1");

			testObjectCreator.CreateTestPeriodsForEntireYear(company, 2022);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var valueObject = AssertImport_NoError("ValidGJLJournal_PostDate.csv", 1);

				var firstGLJDetail = valueObject[0].GLDetail;
				CombineAssertions(() =>
				{
					AssertEquals("202206", firstGLJDetail.InPeriod);
					AssertEquals("20220630", firstGLJDetail.InPeriodDate.ToString("yyyyMMdd"));
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExchangeRateWhenUseForeignCurrencyAndPostDate()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.PeriodEndRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, gLJournalExchangeRateType);
			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var glHeader1 = testObjectCreator.GetGLAccountFromDB("1010.10.10");
			glHeader1.AG_AccountType = AccountType.BalanceSheetAccount;

			var glHeader2 = testObjectCreator.GetGLAccountFromDB("1010.10.20");
			glHeader2.AG_AccountType = AccountType.ProfitAndLossAccount;

			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 2m, new ZDateTime(2022, 11, 1), new ZDateTime(2022, 11, 15));
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 10m, new ZDateTime(2022, 11, 16), new ZDateTime(2022, 11, 30));
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, ExchangeRateTypes.Code.PeriodEndRate, 5m, new ZDateTime(2022, 11, 30), new ZDateTime(2022, 11, 30));
			Factory.Save();

			var fileName = "ValidGJLJournal_PostDate_UseForeignCurrencyAndPostDate0715.csv";
			var (valueObject, notificationBuffer) = ImportFile(BasePath + fileName);
			AssertEquals(false, notificationBuffer.HasErrors);
			AssertEquals(2000 / 5m, valueObject[0].JournalLines[0].LocalAmount.Value);
			AssertEquals(4000 / 2m, valueObject[0].JournalLines[1].LocalAmount.Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithReverseDate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;
			var sY1Branch = testObjectCreator.CreateNewBranch(company, "SY1");

			testObjectCreator.CreateTestPeriodsForEntireYear(company, 2022);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var valueObject = AssertImport_NoError("ValidGJLJournal_ReverseDate.csv", 1);

				var firstGLJDetail = valueObject[0].GLDetail;
				CombineAssertions(() =>
				{
					AssertEquals("202206", firstGLJDetail.InPeriod);
					AssertEquals("20220630", firstGLJDetail.InPeriodDate.ToString("yyyyMMdd"));
					AssertEquals("202207", firstGLJDetail.OutPeriod);
					AssertEquals("20220701", firstGLJDetail.OutPeriodDate.ToString("yyyyMMdd"));
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidJournalType()
		{
			AssertImport_ErrorMessage("InvalidGJLJournal_InvalidJournalType.csv", GetErrorMessageWithLocation(1, 0, false, "EDI", "Invalid Journal Type XXX in CSV file."));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_MissingLocalOrOSAmountForForeignCurrency_InferringMissingData()
		{
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.SellRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);
			var glHeader1 = testObjectCreator.GetGLAccountFromDB("2010.00.00");
			glHeader1.AG_AccountType = AccountType.BalanceSheetAccount;

			var glHeader2 = testObjectCreator.GetGLAccountFromDB("2020.00.00");
			glHeader2.AG_AccountType = AccountType.BalanceSheetAccount;
			Factory.Save();

			var periodEndDate = new ZDateTime(2022, 7, 31);
			testObjectCreator.CreateTestPeriodsForEntireYear(2022);
			testObjectCreator.CreateExchangeRate(testObjectCreator.CNY, ExchangeRateTypes.Code.BuyRate, 6m, periodEndDate, periodEndDate);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, testBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);
				testObjectCreator.CreateTestPeriodsForEntireYear(2022);
				testObjectCreator.CreateExchangeRate(testObjectCreator.CNY, ExchangeRateTypes.Code.BuyRate, 3m, periodEndDate, periodEndDate);
			}

			var errorRow6 = GetErrorMessageWithLocation(1, 5, false, "TST", "BUY exchange rate is not setup for currency USD in period 202207.");
			var errorRow9 = GetErrorMessageWithLocation(1, 8, false, "TST", "BUY exchange rate is not setup for currency USD in period 202207.");

			var valueObject = AssertImport_ErrorMessage("GJLJournalWithMissingData.csv", errorRow6 + errorRow9);

			AssertEquals("Should be able to infer missing Local Amount with provided OS Amount and PER for line 4", 1m, valueObject[0].JournalLines[3].LocalAmount.Value);
			AssertEquals("Should be able to infer missing Local Amount with provided OS Amount and PER for journal 2 line 2", 2m, valueObject[1].JournalLines[1].LocalAmount.Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_FileHeader()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, testBranch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var valueObject = new GLJournalCollection();
				var notificationBuffer = new NotificationBuffer();
				var converter = new MultiCompaniesGLJournalFlatFileConverter(notificationBuffer, Factory);
				using (var reader = new StreamReader(BasePath + "ValidGJLJournalWithFileHeader.csv"))
				{
					converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
				}

				AssertEquals("Should have 3 GL Journals.", 3, valueObject.Count);

				var firstGLJournal = valueObject[0];
				AssertEquals(testBranch1.GB_Code, firstGLJournal.GLDetail.Branch);
				AssertHeaderInfo(firstGLJournal);

				var secondGLJournal = valueObject[1];
				AssertEquals(testBranch3.GB_Code, secondGLJournal.GLDetail.Branch);
				AssertHeaderInfo(secondGLJournal);

				var thirdGLJournal = valueObject[2];
				AssertHeaderInfo(thirdGLJournal);

				void AssertHeaderInfo(GLJournal journal)
				{
					AssertEquals("GJL", journal.GLDetail.JournalType.ToString());
					AssertEquals("DEPRECIATION OF COMPUTER EQUIPMENT FOR THE MONTH OF JUNE2", journal.GLDetail.Description);
					AssertEquals("202206", journal.GLDetail.InPeriod);
					AssertEquals("202207", journal.GLDetail.OutPeriod);
					AssertEquals("ELM", journal.GLDetail.Presentation);
					AssertEquals(ZDate.Empty, journal.GLDetail.InPeriodDate);
					AssertEquals(ZDate.Empty, journal.GLDetail.OutPeriodDate);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_FileHeader_InvalidBranchAndCompany()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, testBranch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				testBranch1.GB_IsActive = false;
				testBranch1.Company.GC_IsActive = false;

				var expectedErrorMessage = @"Error: File.Line[1].TST - The branch code TST is invalid or inactive.
Error: File.Line[1].TST - The company code TST is invalid or inactive.
Error: File.Line[2].XXX - The branch code XXX is invalid or inactive.
Error: File.Line[2].XXX - The company code XXX is invalid or inactive.
Error: File.Line[3].TS2 - The branch code TS3 is not valid in company TS2.
Error: File.Line[3].TS2 - Journal line company code 'TS2' does not match journal header company code 'TS3'." + "\r\n";
				AssertImport_ErrorMessage("InvalidValidGJLJournalWithFileHeader_InvalidCompanyAndBranch.csv", expectedErrorMessage);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_FileHeader_MissingLocalOrOSAmountForForeignCurrency_InferringMissingData()
		{
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.SellRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);
			var glHeader1 = testObjectCreator.GetGLAccountFromDB("2010.00.00");
			glHeader1.AG_AccountType = AccountType.BalanceSheetAccount;

			var glHeader2 = testObjectCreator.GetGLAccountFromDB("2020.00.00");
			glHeader2.AG_AccountType = AccountType.BalanceSheetAccount;
			Factory.Save();

			var periodEndDate = new ZDateTime(2022, 7, 31);
			testObjectCreator.CreateTestPeriodsForEntireYear(2022);
			testObjectCreator.CreateExchangeRate(testObjectCreator.CNY, ExchangeRateTypes.Code.BuyRate, 6m, periodEndDate, periodEndDate);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, testBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);
				testObjectCreator.CreateTestPeriodsForEntireYear(2022);
				testObjectCreator.CreateExchangeRate(testObjectCreator.CNY, ExchangeRateTypes.Code.BuyRate, 3m, periodEndDate, periodEndDate);
			}

			var errorAtRow3 = GetErrorMessageWithLocation(0, 2, false, "EDI", "BUY exchange rate is not setup for currency USD in period 202207.");
			var errorAtRow5 = GetErrorMessageWithLocation(0, 4, false, "TST", "BUY exchange rate is not setup for currency USD in period 202207.");

			var valueObject = AssertImport_ErrorMessage("ValidGJLJournalWithFileHeader_MissingData.csv", errorAtRow3 + errorAtRow5);

			AssertEquals("Should be able to infer missing Local Amount with provided OS Amount and PER for line 1", 1m, valueObject[0].JournalLines[0].LocalAmount.Value);
			AssertEquals("Should be able to infer missing Local Amount with provided OS Amount and PER for line 2", 2m, valueObject[1].JournalLines[0].LocalAmount.Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidCompanyCode()
		{
			AssertImport_ErrorMessage("InvalidGJLJournal_InvalidCompany.csv", GetErrorMessageWithLocation(1, 0, false, "AAA", "The company code AAA is invalid or inactive."));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidBranchCode()
		{
			AssertImport_ErrorMessage("InvalidGJLJournal_InvalidBranch.csv", GetErrorMessageWithLocation(1, 0, false, "TST", "The branch code AAA is invalid or inactive."));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidCSV()
		{
			AssertImport_ErrorMessage("InvalidGJLJournal.csv", GetErrorMessageWithRowNumber(2, "Line type TEAPOT is invalid."));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidLineCompany()
		{
			var expectedError = GetErrorMessageWithLocation(1, 1, false, "EDI", "The company code AAA is invalid or inactive.")
				+ GetErrorMessageWithLocation(1, 1, false, "EDI", "The branch code SYD is not valid in company AAA.")
				+ GetErrorMessageWithLocation(1, 1, false, "EDI", "Journal line company code 'AAA' does not match journal header company code 'EDI'.");

			AssertImport_ErrorMessage("InvalidGJLJournal_InvalidLineCompany.csv", expectedError);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InactiveLineCompany()
		{
			var inactiveCompany = testObjectCreator.CreateNewCompany("TS4");
			inactiveCompany.GC_IsActive = false;

			testObjectCreator.CreateTestPeriodsForEntireYear(inactiveCompany, 2022);
			Factory.Save();

			var expectedError = GetErrorMessageWithLocation(1, 1, false, "EDI", "The company code TS4 is invalid or inactive.")
				+ GetErrorMessageWithLocation(1, 1, false, "EDI", "Journal line company code 'TS4' does not match journal header company code 'EDI'.");

			AssertEquals(false, inactiveCompany.GC_IsActive);
			AssertImport_ErrorMessage("InvalidGJLJournal_InactiveLineCompany.csv", expectedError);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidLineBranch()
		{
			var expectedError = GetErrorMessageWithLocation(1, 1, false, "EDI", "The branch code SYD is not valid in company TST.")
				+ GetErrorMessageWithLocation(1, 1, false, "EDI", "Journal line company code 'TST' does not match journal header company code 'EDI'.")
				+ GetErrorMessageWithLocation(1, 2, false, "EDI", "The branch code CCC is invalid or inactive.");

			AssertImport_ErrorMessage("InvalidGJLJournal_InvalidLineBranch.csv", expectedError);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InactiveLineBranch()
		{
			testBranch1.GB_IsActive = false;
			Factory.Save();

			var expectedError = GetErrorMessageWithLocation(1, 1, false, "EDI", "The branch code TST is invalid or inactive.")
				+ GetErrorMessageWithLocation(1, 1, false, "EDI", "There is no active branch in company TST.")
				+ GetErrorMessageWithLocation(1, 1, false, "EDI", "Journal line company code 'TST' does not match journal header company code 'EDI'.");

			AssertEquals(false, testBranch1.GB_IsActive);
			AssertImport_ErrorMessage("InvalidGJLJournal_InactiveLineBranch.csv", expectedError);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_EmptyLineDepartment()
		{
			AssertImport_ErrorMessage("InvalidGJLJournal_EmptyLineDepartment.csv", GetErrorMessageWithLocation(1, 1, false, "TST", "The department code must not be empty."));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidLineDepartment()
		{
			AssertImport_ErrorMessage("InvalidGJLJournal_InvalidLineDepartment.csv", GetErrorMessageWithLocation(1, 1, false, "TST", "The department code TST is invalid or inactive."));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InactiveLineDepartment()
		{
			var department = testObjectCreator.CreateDepartment("TST");
			department.GE_IsActive = false;
			Factory.Save();

			AssertEquals(false, department.GE_IsActive);
			AssertImport_ErrorMessage(@"InvalidGJLJournal_InactiveLineDepartment.csv", GetErrorMessageWithLocation(1, 1, false, "TST", "The department code TST is invalid or inactive."));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_FirstActiveBranch()
		{
			var testCompany = testObjectCreator.CreateNewCompany("TS5");
			var inactiveBranch = testObjectCreator.CreateNewBranch(testCompany, "TIB");
			var testFirstActiveBranch = testObjectCreator.CreateNewBranch(testCompany, "TA1");
			var testCurrentBranch = testObjectCreator.CreateNewBranch(testCompany, "TA2");
			inactiveBranch.GB_IsActive = false;

			testObjectCreator.CreateTestPeriodsForEntireYear(testCompany, 2022);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, testCurrentBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var importedJournals1 = AssertImport_NoError("ValidGJLJournal_NoCompanyAndBranch.csv", 1);

				AssertEquals("Should use first active branch of current company.", testFirstActiveBranch.GB_Code, importedJournals1[0].GLDetail.Branch);
			}

			var importedJournals2 = AssertImport_NoError("ValidGJLJournal_CompanyOnly.csv", 1);

			AssertNotEquals("Pre: Current company is not the test company", testCompany.GC_Code, GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Should use first active branch of specified company.", testFirstActiveBranch.GB_Code, importedJournals2[0].GLDetail.Branch);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_ValidLineBranch()
		{
			testObjectCreator.CreateNewBranch(testBranch1.Company, "TB1");

			var errorAtRow3 = GetErrorMessageWithLocation(1, 3, false, "TST", "The branch code TS2 is not valid in company TST.");
			var errorAtRow4 = GetErrorMessageWithLocation(1, 4, false, "TST", "Journal line company code 'TS3' does not match journal header company code 'TST'.");

			var importedJournals = AssertImport_ErrorMessage("InvalidGJLJournal_LineBranch.csv", errorAtRow3 + errorAtRow4);
			var importedJournal = importedJournals[0];

			AssertEquals("Line 2 should take specified branch", "TB1", importedJournal.JournalLines[1].Branch);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_ReportErrorPositionPrecisely()
		{
			var errorAtRow12 = GetErrorMessageWithLocation(3, 1, false, "EDI", "PER exchange rate is not setup for currency USD in period 202206.");
			var errorAtRow18 = GetErrorMessageWithLocation(4, 2, false, "EDI", "PER exchange rate is not setup for currency USD in period 202206.");

			AssertImport_ErrorMessage("InvalidGJLJournal_TestErrorWithRowNumber.csv", errorAtRow12 + errorAtRow18);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidNoJournalCSV()
		{
			AssertImport_ErrorMessage("InvalidGJLJournal_SeparateJournalLineError.csv", "Error: There is no GLJF nor GLJH record in the file.\r\n");
		}

		public void TestImport_InvalidEmptyJournalCSV()
		{
			var valueObject = new GLJournalCollection();
			var notificationBuffer = new NotificationBuffer();
			var converter = new MultiCompaniesGLJournalFlatFileConverter(notificationBuffer, Factory);

			AssertEquals(false, notificationBuffer.HasErrors);

			using (var emptyFile = TempFile.New())
			using (var reader = new StreamReader(emptyFile.Filename))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(true, notificationBuffer.HasErrors);
			AssertEquals("Error: The file does not contain any record.\r\n", notificationBuffer.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_NoHeaderLine()
		{
			AssertImport_ErrorMessage("InvalidGJLJournal_NoHeaderLine.csv", "Error: There is no GLJF nor GLJH record in the file.\r\n");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_MultipleFileHeaderLines()
		{
			AssertImport_ErrorMessage("InvalidValidGJLJournalWithFileHeader_MultipleHeaderLines.csv", "Error: The file contains multiple GJLF records which is not supported.\r\n");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_TwoHeaderTypes()
		{
			AssertImport_ErrorMessage("InvalidGJLJournal_DifferentHeaderLines.csv", "Error: The file contains both GJLF and GJLH records which is not supported.\r\n");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidGLAccount()
		{
			var errorAtLine1 = GetErrorMessageWithLocation(1, 1, false, "EDI", "The account number XXX is invalid or inactive.");
			var errorAtLine2 = GetErrorMessageWithLocation(1, 2, false, "EDI", "The account number must not be empty.");

			AssertImport_ErrorMessage("InvalidGJLJournal_InvalidGLAccount.csv", errorAtLine1 + errorAtLine2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidDecimal()
		{
			var errorAtLine1 = GetErrorMessageWithLocation(1, 1, false, "EDI", "Local currency AUD only allows entering amounts up to 2 decimal places. Local Amount: 20500.001");
			var errorAtLine2 = GetErrorMessageWithLocation(1, 1, false, "EDI", "Currency CNY only allows entering amounts up to 2 decimal places. Amount: 51000.001");

			AssertImport_ErrorMessage("InvalidGJLJournal_InvalidDecimal.csv", errorAtLine1 + errorAtLine2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidSubAccount()
		{
			var errorAtLine1 = GetErrorMessageWithLocation(1, 1, true, "EDI", "The Sub Account Code '2010.00.04' is invalid or inactive.");
			var errorAtLine2 = GetErrorMessageWithLocation(1, 2, true, "EDI", "The Sub Account Type 'XXX' is invalid.");

			AssertImport_ErrorMessage("InvalidGJLJournal_InvalidSubAccount.csv", errorAtLine1 + errorAtLine2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFlatFileLackDateAndInvalidPostPeriod()
		{
			AssertImportFlatFileLackDate("InvalidGJLJournal_LackDateAndInvalidPostPeriod.csv", TransactionTypes.GLStandardJournal, exceptPeriod1: "180001", exceptPeriod2: "180001A");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFlatFileLackDateAndInvalidReversePeriod_AJL()
		{
			AssertImportFlatFileLackDate("InvalidAJLJournal_LackDateAndInvalidReversePeriod.csv", TransactionTypes.GLAutoJournal, exceptPeriod1: "180001", exceptPeriod2: "180001A");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFlatFileLackDateAndInvalidReversePeriod_RJL()
		{
			AssertImportFlatFileLackDate("InvalidRJLJournal_LackDateAndInvalidReversePeriod.csv", TransactionTypes.GLReversingJournal, exceptPeriod1: "180001", exceptPeriod2: "180001A");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFlatFileLackBothDateAndPostPeriod()
		{
			AssertImportFlatFileLackDate("InvalidGJLJournal_LackBothDateAndPostPeriod.csv", TransactionTypes.GLStandardJournal, isLackBothDateAndPeriod: true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFlatFileLackBothDateAndReversePeriod_AJL()
		{
			AssertImportFlatFileLackDate("InvalidAJLJournal_LackBothDateAndReversePeriod.csv", TransactionTypes.GLAutoJournal, isLackBothDateAndPeriod: true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFlatFileLackBothDateAndReversePeriod_RJL()
		{
			AssertImportFlatFileLackDate("InvalidRJLJournal_LackBothDateAndReversePeriod.csv", TransactionTypes.GLReversingJournal, isLackBothDateAndPeriod: true);
		}

		void AssertImportFlatFileLackDate(string fileName, string journalType, string exceptPeriod1 = "", string exceptPeriod2 = "", bool isLackBothDateAndPeriod = false)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;
			var sY1Branch = testObjectCreator.CreateNewBranch(company, "SY1");

			testObjectCreator.CreateTestPeriodsForEntireYear(company, 2022);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var valueObject = new GLJournalCollection();
				var notificationBuffer = new NotificationBuffer();
				var converter = new MultiCompaniesGLJournalFlatFileConverter(notificationBuffer, Factory);

				AssertEquals(false, notificationBuffer.HasErrors);

				using (var reader = new StreamReader(BasePath + fileName))
				{
					converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);

					string periodType;

					if (journalType == TransactionTypes.GLReversingJournal || journalType == TransactionTypes.GLAutoJournal)
					{
						periodType = "Reverse/Ending";
					}
					else
					{
						periodType = "Post";
					}

					if (isLackBothDateAndPeriod)
					{
						if (periodType == "Post")
						{
							AssertEquals("Error: Header[1].DAN - No Post Date or Post Period specified. Please specify at least one of the values.", notificationBuffer.Events[0].Message);
						}
						else
						{
							AssertEquals("Error: Header[1].DAN - No Reverse/Ending Period specified. Please specify a value.", notificationBuffer.Events[0].Message);
						}
					}
					else
					{
						AssertEquals("Error: Header[1].DAN - " + periodType + " period " + exceptPeriod1 + " is invalid. Please check specified value against your Period Management setup.", notificationBuffer.Events[0].Message);

						if (!string.IsNullOrEmpty(exceptPeriod2))
						{
							AssertEquals("Error: Header[2].DAN - " + periodType + " period " + exceptPeriod2 + " is invalid. Please check specified value against your Period Management setup.", notificationBuffer.Events[1].Message);
						}
					}
				}
			}
		}

		GLJournalCollection AssertImport_ErrorMessage(string fileName, string errorMessage)
		{
			var (valueObject, notificationBuffer) = ImportFile(BasePath + fileName);

			CombineAssertions(() =>
			{
				AssertEquals("Error is expected.", true, notificationBuffer.HasErrors);
				AssertEquals("Error text match with expected", errorMessage, notificationBuffer.AsString);
			});

			return valueObject;
		}

		GLJournalCollection AssertImport_NoError(string fileName, int importedJournal)
		{
			var (valueObject, notificationBuffer) = ImportFile(BasePath + fileName);

			CombineAssertions(() =>
			{
				AssertEquals("No error expected", false, notificationBuffer.HasErrors);
				AssertEquals("Should import {importedJournal} journal(s)", importedJournal, valueObject.Count);
			});

			return valueObject;
		}

		(GLJournalCollection, NotificationBuffer) ImportFile(string fileName)
		{
			var valueObject = new GLJournalCollection();
			var notificationBuffer = new NotificationBuffer();
			var converter = new MultiCompaniesGLJournalFlatFileConverter(notificationBuffer, Factory);

			AssertEquals(false, notificationBuffer.HasErrors);

			using (var reader = new StreamReader(fileName))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			return (valueObject, notificationBuffer);
		}

		AccAlternateGLAccountDissection SetUpForImportAttribute(string attr)
		{
			var chart = testObjectCreator.CreateAlternateChart("MGT");
			testObjectCreator.CreateAccAlternateChartFormat(chart, 1, "9");
			var dissection = testObjectCreator.CreateAccAlternateGLAccountDissection(GlHeader, chart.PK, attr, false);
			Factory.Save();
			return dissection;
		}

		#region Create File and Import

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Assert contains BaseSourcePath")]
		protected override void AssertImport_ErrorMessage(string errorMessage, GLTestData data)
		{
			var fileName = Env.GetTempFileName(Env.TempPath, "csv");
			CreateTestData(fileName, data);

			try
			{
				var (valueObject, notificationBuffer) = ImportFile(fileName);
				CombineAssertions(() =>
				{
					AssertEquals("Error is expected.", true, notificationBuffer.HasErrors);
					AssertEquals("Error text match with expected", errorMessage, notificationBuffer.AsString);
				});
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Assert contains BaseSourcePath")]
		void CreateTestData(string fileName, GLTestData data)
		{
			using (var writer = new StreamWriter(fileName))
			{
				writer.WriteLine("GLJH," + data.CompanyCode + ",,GJL,,,DEPRECIATION OF COMPUTER EQUIPMENT FOR THE MONTH OF JUNE1,ELM," + data.PostDate + ",");
				writer.WriteLine("GLJL," + data.GLAccount + ",," + data.BranchCode + ",BRN,20500," + data.Currency + ",51000,Description1,Org1,ORG,2010.00.01,SEG,2010.00.02");
				writer.WriteLine("GLJL,2020.00.00,," + data.BranchCode + ",BRN,-20500," + data.Currency + ",-51000,Description2,Org2,ORG,2020.00.01,SEG,2020.00.02");
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			var org1 = testObjectCreator.CreateOrgHeader("2010.00.01", true, true, true, true, true, true);
			org1.OH_Code = "2010.00.01";
			var org2 = testObjectCreator.CreateOrgHeader("2020.00.01", true, true, true, true, true, true);
			org2.OH_Code = "2020.00.01";
			var org3 = testObjectCreator.CreateOrgHeader("2010.00.05", true, true, true, true, true, true);
			org3.OH_Code = "2010.00.05";
			var org4 = testObjectCreator.CreateOrgHeader("2020.00.05", true, true, true, true, true, true);
			org4.OH_Code = "2020.00.05";
			testObjectCreator.CreateSalesGroup("2010.00.02");
			testObjectCreator.CreateSalesGroup("2020.00.02");
			testObjectCreator.CreateSalesGroup("2010.00.06");
			testObjectCreator.CreateSalesGroup("2020.00.06");
			testObjectCreator.CreateStaff("AAA");
			testObjectCreator.CreateStaff("BBB");
			testObjectCreator.CreateStaff("CCC");
			testObjectCreator.CreateStaff("DDD");
			testObjectCreator.CreateStaffGroup("2010.00.04");
			testObjectCreator.CreateStaffGroup("2020.00.04");
			testObjectCreator.CreateStaffGroup("2010.00.08");
			testObjectCreator.CreateStaffGroup("2020.00.08");

			var company1 = testObjectCreator.CreateNewCompany("TST");
			testBranch1 = testObjectCreator.CreateNewBranch(company1, "TST");
			testObjectCreator.CreateTestPeriodsForEntireYear(company1, 2022);

			var company2 = testObjectCreator.CreateNewCompany("TS2");
			testBranch2 = testObjectCreator.CreateNewBranch(company2, "TS2");
			testObjectCreator.CreateTestPeriodsForEntireYear(company2, 2022);

			var company3 = testObjectCreator.CreateNewCompany("TS3");
			testBranch3 = testObjectCreator.CreateNewBranch(company3, "TS3");
			testObjectCreator.CreateTestPeriodsForEntireYear(company3, 2022);

			testObjectCreator.CreateTestPeriodsForEntireYear(2022);
			GlHeader = testObjectCreator.CreateGLHeader("2010.00.00");

			Factory.Save();
		}

		string GetErrorMessageWithLocation(int headerNum, int lineNum, bool isSubAccountMessage, string companyCode, string message)
		{
			var result = "Error: ";
			if (headerNum == 0)
			{
				result += "File";
			}
			else
			{
				result += $"Header[{headerNum}]";
			}

			if (lineNum != 0)
			{
				result += $".Line[{lineNum}]";
			}

			if (isSubAccountMessage)
			{
				result += ".Sub Account";
			}

			result += $".{companyCode} - {message}\r\n";

			return result;
		}

		string GetErrorMessageWithRowNumber(int row, string message)
		{
			return $"Error: Row {row} - {message}\r\n";
		}

		string BasePath => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\";

		GlbBranch testBranch1;
		GlbBranch testBranch2;
		GlbBranch testBranch3;
		AccGLHeader GlHeader;
	}
}
