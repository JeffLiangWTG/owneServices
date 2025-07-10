using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	public class GLJournalForADAWConverterTest : MultiCompaniesGLJournalFlatFileConverterBaseTest
	{
		public void TestImport()
		{
			var convert = new GLJournalForADAWConverter(Notification, Factory, JournalHeaderForADAWCollection);

			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			AssertEquals(1, XsdGLJournalCollection.Count);

			var xsdJournal = XsdGLJournalCollection[0];
			AssertEquals(GLJournalGLDetailJournalType.GJL, xsdJournal.GLDetail.JournalType);
			AssertEquals("SYD", xsdJournal.GLDetail.Branch);
			AssertEquals("202211", xsdJournal.GLDetail.InPeriod);
			AssertEquals("Journal Description", xsdJournal.GLDetail.Description);
			AssertEquals("ELM", xsdJournal.GLDetail.Presentation);

			AssertEquals(1, xsdJournal.JournalLines.Count);
			var xsdJournalLine = xsdJournal.JournalLines[0];
			AssertEquals("1010.10.10", xsdJournalLine.Account);
			AssertEquals("SYD", xsdJournalLine.Branch);
			AssertEquals("BRN", xsdJournalLine.Department);
			AssertEquals(Xsd.GLJournalJournalLineDRCR.DR, xsdJournalLine.DRCR);
			AssertEquals(100M, xsdJournalLine.LocalAmount.Value);
			AssertEquals("AUD", xsdJournalLine.Currency);
			AssertEquals("Line Description", xsdJournalLine.Description);
			AssertEquals("CARGOWSYD", xsdJournalLine.Organisation.EDICode);

			AssertEquals(1, xsdJournalLine.SubAccounts.Count);
			var xsdSubAccount = xsdJournalLine.SubAccounts[0];
			AssertEquals("ORG", xsdSubAccount.Type.Code);
			AssertEquals("CARINT", xsdSubAccount.Code);
		}

		public void TestImportORGAttribute()
		{
			Action<GLJournalLineForADAW> setValidaValue = (line) => line.AttributeORG = AccountingMasterFilesConstants.NAV.Code;
			Action<GLJournalLineForADAW> setInValidaValue = (line) => line.AttributeORG = "NAV1";
			Action<GLJournalLineForADAW> setEmptyaValue = (line) => line.AttributeORG = "";
			var inValidErrorMessage = "Error: Header[1].Line[1].EDI - Invalid Attribute Value. A valid Attribute Value must be the code of a valid Receivable Organization (if Parent Account is an AR Control Account) or a valid Payable Organization (if Parent Account is an AP Control Account).\r\n";

			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, true);
			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, false);
		}

		public void TestImportOCGAttribute()
		{
			Action<GLJournalLineForADAW> setValidaValue = (line) => line.AttributeOCG = AccountingMasterFilesConstants.NAV.Code;
			Action<GLJournalLineForADAW> setInValidaValue = (line) => line.AttributeOCG = "NAV1";
			Action<GLJournalLineForADAW> setEmptyaValue = (line) => line.AttributeOCG = "";
			var inValidErrorMessage = "Error: Header[1].Line[1].EDI - Invalid Attribute Value. A valid Attribute Value must be one of the following Class defined in Consolidated Accounting Category List registry or value 'NAV'.\r\n";

			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, true);
			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, false);
		}

		public void TestImportLFOAttribute()
		{
			Action<GLJournalLineForADAW> setValidaValue = (line) => line.AttributeLFO = AccountingMasterFilesConstants.NAV.Code;
			Action<GLJournalLineForADAW> setInValidaValue = (line) => line.AttributeLFO = "NAV1";
			Action<GLJournalLineForADAW> setEmptyaValue = (line) => line.AttributeLFO = "";
			var inValidErrorMessage = "Error: Header[1].Line[1].EDI - Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC' ,'FOR' or 'NAV'.\r\nLOC = Local.\r\nFOR = Foreign.\r\nNAV = No Attribute Value.\r\n";

			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, true);
			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, false);
		}

		public void TestImportLFEAttribute()
		{
			Action<GLJournalLineForADAW> setValidaValue = (line) => line.AttributeLFE = AccountingMasterFilesConstants.NAV.Code;
			Action<GLJournalLineForADAW> setInValidaValue = (line) => line.AttributeLFE = "NAV1";
			Action<GLJournalLineForADAW> setEmptyaValue = (line) => line.AttributeLFE = "";
			var inValidErrorMessage = "Error: Header[1].Line[1].EDI - Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC', 'WEU' ,'OEU' or 'NAV'.\r\nLOC = Local.\r\nWEU = Within EU.\r\nOEU = Outside EU.\r\nNAV = No Attribute Value.\r\n";

			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, true);
			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, false);
		}

		public void TestImportTICAttribute()
		{
			Action<GLJournalLineForADAW> setValidaValue = (line) => line.AttributeTIC = AccountingMasterFilesConstants.NAV.Code;
			Action<GLJournalLineForADAW> setInValidaValue = (line) => line.AttributeTIC = "NAV1";
			Action<GLJournalLineForADAW> setEmptyaValue = (line) => line.AttributeTIC = "";
			var inValidErrorMessage = "Error: Header[1].Line[1].EDI - Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'STI' or 'ETI' or 'NAV'.\r\nSTI = Standard Tax IDs.\r\nETI = Tax ID with Extra Tax.\r\nNAV = No Attribute Value.\r\n";

			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, true);
			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, false);
		}

		public void TestImportSPRAttribute()
		{
			Action<GLJournalLineForADAW> setValidaValue = (line) => line.AttributeSPR = AccountingMasterFilesConstants.NAV.Code;
			Action<GLJournalLineForADAW> setInValidaValue = (line) => line.AttributeSPR = "NAV1";
			Action<GLJournalLineForADAW> setEmptyaValue = (line) => line.AttributeSPR = "";
			var inValidErrorMessage = "Error: Header[1].Line[1].EDI - Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'SPS' , 'SPR' or 'NAV'.\r\nSPS = Sales/Purchases.\r\nSPR = Sales/Purchases Returns.\r\nNAV = No Attribute Value.\r\n";

			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, true);
			TestImportAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, inValidErrorMessage, setValidaValue, setInValidaValue, setEmptyaValue, false);
		}

		public void TestImportWithoutIdentifier()
		{
			var collection = new GLJournalHeaderForADAWCollection(Factory, "");
			var convert = new GLJournalForADAWConverter(Notification, Factory, collection);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);
			AssertEquals(true, Notification.HasErrors);
			AssertEquals(@"Uploaded failed due to mapping error.
Please check import file values and file mappings.
", Notification.AsString);
		}

		public void TestValidateAccountNoThrowExceptionWhenCompanyIsNull()
		{
			GlHeader.AG_IsGlobal = false;
			GlHeader.CompanyFilters.AddNew();
			JournalHeaderForADAWCollection.Cast<GLJournalHeaderForADAW>().First().GLJournalLines[0].CompanyCode = "AAA";
			var collection = new GLJournalHeaderForADAWCollection(Factory, "");
			var convert = new GLJournalForADAWConverter(Notification, Factory, JournalHeaderForADAWCollection);
			AssertNoExceptionThrown(() => convert.ImportFlatFile(XsdGLJournalCollection, null, null));
		}

		public void TestExchangeRateWhenUseForeignCurrencyAndPostDate()
		{
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

			var collection = SetupJournalADAWCollection("GJL", "", "20221113", "", headerType: "F");
			var convert = new GLJournalForADAWConverter(Notification, Factory, collection);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);
			AssertEquals(false, Notification.HasErrors);
			AssertEquals(5m, XsdGLJournalCollection[0].JournalLines[0].ExchangeRate);
			AssertEquals(2m, XsdGLJournalCollection[0].JournalLines[1].ExchangeRate);
		}

		public void TestExchangeRateWhenUseForeignCurrencyAndIsLocalCurrecyForLoginCompany()
		{
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

			var company = testObjectCreator.CreateCompanyAndBranch("UYD");
			company.GC_RX_NKLocalCurrency = testObjectCreator.USD.Code;

			Factory.Save();

			var defaultCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var defaultBranchCode = GlbBranch.CurrentBranch.GB_Code;
			var defaultDepartmentCode = GlbDepartment.CurrentDepartment.GE_Code;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var collection = SetupJournalADAWCollection("GJL", "20221113", "20221113", "", headerType: "F");
				var journalLine1 = collection[0].GLJournalLines[0];
				journalLine1.CompanyCode = defaultCompanyCode;
				journalLine1.BranchCode = defaultBranchCode;
				journalLine1.DepartmentCode = defaultDepartmentCode;
				var journalLine2 = collection[0].GLJournalLines[1];
				journalLine2.CompanyCode = defaultCompanyCode;
				journalLine2.BranchCode = defaultBranchCode;
				journalLine2.DepartmentCode = defaultDepartmentCode;
				var convert = new GLJournalForADAWConverter(Notification, Factory, collection);
				convert.ImportFlatFile(XsdGLJournalCollection, null, null);
				
				AssertEquals(false, Notification.HasErrors);
				AssertEquals(5m, XsdGLJournalCollection[0].JournalLines[0].ExchangeRate);
				AssertEquals(2m, XsdGLJournalCollection[0].JournalLines[1].ExchangeRate);
			}
		}

		public void TestImportFileCollection()
		{
			var collection = new GLJournalHeaderForADAWCollection(Factory, "F");

			var fileHeader = collection.AddNew();
			fileHeader.JournalType = "General Journal";
			fileHeader.PostPeriod = "202211";
			fileHeader.JournalDescription = "Journal Description";

			var journalLine = fileHeader.GLJournalLines.AddNew();
			journalLine.CompanyCode = "EDI";
			journalLine.BranchCode = "SYD";
			journalLine.GLAccount = "1010.10.10";
			journalLine.DepartmentCode = "FEA";
			journalLine.Currency = "AUD";
			journalLine.Amount = "100";
			journalLine.JournalLineDescription = "Line Description1";

			var journalLine2 = fileHeader.GLJournalLines.AddNew();
			journalLine2.CompanyCode = "DEM";
			journalLine2.BranchCode = "DEM";
			journalLine2.GLAccount = "1010.10.20";
			journalLine2.DepartmentCode = "FEA";
			journalLine2.Currency = "AUD";
			journalLine2.Amount = "200";
			journalLine2.JournalLineDescription = "Line Description2";

			var convert = new GLJournalForADAWConverter(Notification, Factory, collection);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			AssertEquals(2, XsdGLJournalCollection.Count);

			AssertEquals(GLJournalGLDetailJournalType.GJL, XsdGLJournalCollection[0].GLDetail.JournalType);
			AssertEquals("SYD", XsdGLJournalCollection[0].GLDetail.Branch);
			AssertEquals("202211", XsdGLJournalCollection[0].GLDetail.InPeriod);
			AssertEquals("Journal Description", XsdGLJournalCollection[0].GLDetail.Description);

			AssertEquals(GLJournalGLDetailJournalType.GJL, XsdGLJournalCollection[1].GLDetail.JournalType);
			AssertEquals("DEM", XsdGLJournalCollection[1].GLDetail.Branch);
			AssertEquals("202211", XsdGLJournalCollection[1].GLDetail.InPeriod);
			AssertEquals("Journal Description", XsdGLJournalCollection[1].GLDetail.Description);
		}

		public void TestImportWithDate_GJL()
		{
			var collection = SetupJournalADAWCollection("GJL", "", "20220131", "");

			var convert = new GLJournalForADAWConverter(Notification, Factory, collection);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			AssertEquals(1, XsdGLJournalCollection.Count);

			var xsdJournal = XsdGLJournalCollection[0];
			AssertEquals("202201", xsdJournal.GLDetail.InPeriod);
			AssertEquals("", xsdJournal.GLDetail.OutPeriod);
			AssertEquals("20220131", xsdJournal.GLDetail.InPeriodDate.ToString("yyyyMMdd"));
			AssertEquals("", xsdJournal.GLDetail.OutPeriodDate.ToString("yyyyMMdd"));
		}

		public void TestImportWithDate_NJL()
		{
			var collection = SetupJournalADAWCollection("NJL", "", "20220131", "");

			var convert = new GLJournalForADAWConverter(Notification, Factory, collection);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			AssertEquals(1, XsdGLJournalCollection.Count);

			var xsdJournal = XsdGLJournalCollection[0];
			AssertEquals("202201", xsdJournal.GLDetail.InPeriod);
			AssertEquals("", xsdJournal.GLDetail.OutPeriod);
			AssertEquals("20220131", xsdJournal.GLDetail.InPeriodDate.ToString("yyyyMMdd"));
			AssertEquals("", xsdJournal.GLDetail.OutPeriodDate.ToString("yyyyMMdd"));
		}

		public void TestImportWithDate_RJL()
		{
			var collection = SetupJournalADAWCollection("RJL", "", "20220131", "20220201");
			var convert = new GLJournalForADAWConverter(Notification, Factory, collection);

			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			AssertEquals(1, XsdGLJournalCollection.Count);

			var xsdJournal = XsdGLJournalCollection[0];
			AssertEquals("202201", xsdJournal.GLDetail.InPeriod);
			AssertEquals("202202", xsdJournal.GLDetail.OutPeriod);
			AssertEquals("20220131", xsdJournal.GLDetail.InPeriodDate.ToString("yyyyMMdd"));
			AssertEquals("20220201", xsdJournal.GLDetail.OutPeriodDate.ToString("yyyyMMdd"));
		}

		public void TestImportWithDate_AJL()
		{
			var collection = SetupJournalADAWCollection("AJL", "", "20220131", "20220228");
			var convert = new GLJournalForADAWConverter(Notification, Factory, collection);

			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			AssertEquals(1, XsdGLJournalCollection.Count);

			var xsdJournal = XsdGLJournalCollection[0];
			AssertEquals("202201", xsdJournal.GLDetail.InPeriod);
			AssertEquals("202202", xsdJournal.GLDetail.OutPeriod);
			AssertEquals("20220131", xsdJournal.GLDetail.InPeriodDate.ToString("yyyyMMdd"));
			AssertEquals("20220228", xsdJournal.GLDetail.OutPeriodDate.ToString("yyyyMMdd"));
		}

		public void TestImport_InvalidHeaderCompany()
		{
			JournalHeaderForADAWCollection[0].CompanyCode = "XYZ";
			AssertImport_ErrorMessage("Error: Header[1].XYZ - The company code XYZ is invalid or inactive.\r\n", JournalHeaderForADAWCollection);
		}

		public void TestPopulateLineCompanyAndBranch()
		{
			JournalHeaderForADAWCollection[0].BranchCode = "";
			JournalHeaderForADAWCollection[0].GLJournalLines[0].BranchCode = "";
			var convert = new GLJournalForADAWConverter(Notification, Factory, JournalHeaderForADAWCollection);

			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			AssertEquals(1, XsdGLJournalCollection.Count);
			AssertEquals("Company's first active branch", "BNE", XsdGLJournalCollection[0].GLDetail.Branch);
			AssertEquals(1, XsdGLJournalCollection[0].JournalLines.Count);
			AssertEquals("Company's first active branch", "BNE", XsdGLJournalCollection[0].JournalLines[0].Branch);
		}

		public void TestImportWithOSCurrency()
		{
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.SellRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);
			ObjectCreator.GLHeader1.AG_AccountType = AccountType.BalanceSheetAccount;
			Factory.Save();

			var periodEndDate = new ZDateTime(2022, 11, 30);
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.5m, periodEndDate, periodEndDate);

			JournalHeaderForADAWCollection[0].GLJournalLines[0].GLAccount = ObjectCreator.GLHeader1.AG_AccountNum;
			JournalHeaderForADAWCollection[0].GLJournalLines[0].Currency = "USD";
			JournalHeaderForADAWCollection[0].GLJournalLines[0].Amount = "200";
			var convert = new GLJournalForADAWConverter(Notification, Factory, JournalHeaderForADAWCollection);

			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			AssertEquals(1, XsdGLJournalCollection.Count);
			AssertEquals(1, XsdGLJournalCollection[0].JournalLines.Count);
			AssertEquals(100m, XsdGLJournalCollection[0].JournalLines[0].LocalAmount.Value);
			AssertEquals("USD", XsdGLJournalCollection[0].JournalLines[0].Currency);
			AssertEquals(200m, XsdGLJournalCollection[0].JournalLines[0].Amount.Value);
		}

		public void TestImport_NoLines()
		{
			JournalHeaderForADAWCollection[0].GLJournalLines.RemoveAndDeleteAll();
			AssertImport_ErrorMessage("Error: Journal doesn't contain lines.\r\n", JournalHeaderForADAWCollection);
		}

		public void TestImport_InvalidJournalType()
		{
			JournalHeaderForADAWCollection[0].JournalType = "XXX";
			AssertImport_ErrorMessage("Error: Header[1].EDI - Invalid journal type: XXX.\r\n", JournalHeaderForADAWCollection);
		}

		public void TestImport_InvalidHeaderBranch()
		{
			JournalHeaderForADAWCollection[0].BranchCode = "DEM";
			AssertImport_ErrorMessage("Error: Header[1].EDI - The branch code DEM is not valid in company EDI.\r\n", JournalHeaderForADAWCollection);
		}

		public void TestImport_InvalidPeriod()
		{
			JournalHeaderForADAWCollection[0].PostPeriod = "202501";
			AssertImport_ErrorMessage("Error: Header[1].EDI - Post period 202501 is invalid. Please check specified value against your Period Management setup.\r\n", JournalHeaderForADAWCollection);
		}

		public void TestImport_AmountIsZero()
		{
			JournalHeaderForADAWCollection[0].GLJournalLines[0].LocalAmount = "0";
			JournalHeaderForADAWCollection[0].GLJournalLines[0].Amount = "0";
			AssertImport_ErrorMessage("Error: Header[1].Line[1].EDI - Either Amount or Local Amount must be provided with valid value.\r\n", JournalHeaderForADAWCollection);
		}

		public void TestImport_InvalidLineBranch()
		{
			JournalHeaderForADAWCollection[0].GLJournalLines[0].BranchCode = "XXX";
			AssertImport_ErrorMessage("Error: Header[1].Line[1].EDI - The branch code XXX is invalid or inactive.\r\n", JournalHeaderForADAWCollection);
		}

		public void TestImport_InvalidLineCompany()
		{
			JournalHeaderForADAWCollection[0].GLJournalLines[0].CompanyCode = "DEM";
			JournalHeaderForADAWCollection[0].GLJournalLines[0].BranchCode = "";
			AssertImport_ErrorMessage("Error: Header[1].Line[1].DEM - Journal line company code 'DEM' does not match journal header company code 'EDI'.\r\n", JournalHeaderForADAWCollection);
		}

		public void TestImport_InvalidSubAccount()
		{
			JournalHeaderForADAWCollection[0].GLJournalLines[0].SubAccountType1 = "ABC";
			JournalHeaderForADAWCollection[0].GLJournalLines[0].SubAccountValue1 = "Value";

			AssertImport_ErrorMessage("Error: Header[1].Line[1].Sub Account.EDI - The Sub Account Type 'ABC' is invalid.\r\n", JournalHeaderForADAWCollection);

			JournalHeaderForADAWCollection[0].GLJournalLines[0].SubAccountType1 = "ORG";
			JournalHeaderForADAWCollection[0].GLJournalLines[0].SubAccountValue1 = "Value";

			AssertImport_ErrorMessage("Error: Header[1].Line[1].Sub Account.EDI - The Sub Account Code 'Value' is invalid or inactive.\r\n", JournalHeaderForADAWCollection);
		}

		public void TestImportWithPostDate_InvalidPostDate()
		{
			AssertImport_ErrorMessage("Error: Header[1].EDI - The post date 'test' is invalid.\r\n", SetupJournalADAWCollection("GJL", "", "test", ""));
		}

		public void TestImportWithPostDate_InvalidReverseDate_RJL()
		{
			AssertImport_ErrorMessage("Error: Header[1].EDI - The reverse/end date 'test' is invalid.\r\n", SetupJournalADAWCollection("RJL", "", "20220101", "test"));
		}

		public void TestImportWithPostDate_InvalidReverseDate_AJL()
		{
			AssertImport_ErrorMessage("Error: Header[1].EDI - The reverse/end date 'test' is invalid.\r\n", SetupJournalADAWCollection("AJL", "", "20220101", "test"));
		}

		public void TestImportWithPostDate_DatesShouldNotFallInSamePeriod_RJL()
		{
			AssertImport_ErrorMessage("Error: Header[1].EDI - The reverse/end period must not be in the same period as the post period.\r\n", SetupJournalADAWCollection("RJL", "", "20220101", "20220101"));
		}

		public void TestImportWithPostDate_DatesShouldNotFallInSamePeriod_AJL()
		{
			AssertImport_ErrorMessage("Error: Header[1].EDI - The reverse/end period must not be in the same period as the post period.\r\n", SetupJournalADAWCollection("AJL", "", "20220101", "20220101"));
		}

		public void TestImportWithPostDate_BothDatesShouldBeLastDayForPeriod_AJL()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var journalADAWCollection = SetupJournalADAWCollection("AJL", "", "20220131", "20220215");
				AssertImport_ErrorMessage("Error: Header[1].EDI - Both the post date '20220131' and the end date '20220215' must be End Date of a period.\r\n", journalADAWCollection);
			}
		}

		public void TestImportWithPostDate_PostDateShouldBeLastDayForPeriod()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var journalADAWCollection = SetupJournalADAWCollection("GJL", "", "20220115", "");
				AssertImport_ErrorMessage("Error: Header[1].EDI - The post date '20220115' does not match the 'End Date' of a valid period or the relative period has not been setup. Please check the specified value against your Period Management setup.\r\n", journalADAWCollection);
			}
		}

		public void TestImportWithPostDate_ReverseDateShouldBeFirstDayForPeriod_RJL()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var journalADAWCollection = SetupJournalADAWCollection("RJL", "", "20220131", "20220215");
				AssertImport_ErrorMessage("Error: Header[1].EDI - The reverse date '20220215' must be Start Date of a period.\r\n", journalADAWCollection);
			}
		}

		public void TestImportWithPostDate_EndDateShouldBeLastDayForPeriod_AJL()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var journalADAWCollection = SetupJournalADAWCollection("AJL", "", "20220131", "20220215");
				AssertImport_ErrorMessage("Error: Header[1].EDI - The end date '20220215' must be End Date of a period.\r\n", journalADAWCollection);
			}
		}

		void AssertImport_ErrorMessage(string errorMessage, GLJournalHeaderForADAWCollection journalADAWCollection)
		{
			Notification.Clear();
			var convert = new GLJournalForADAWConverter(Notification, Factory, journalADAWCollection);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			CombineAssertions(() =>
			{
				AssertEquals(true, Notification.HasErrors);
				AssertEquals(errorMessage, Notification.AsString);
			});
		}

		protected override void AssertImport_ErrorMessage(string errorMessage, GLTestData data)
		{
			JournalHeaderForADAWCollection[0].PostDate = data.PostDate;
			JournalHeaderForADAWCollection[0].ReverseOrEndDate = data.ReverseOrEndDate;
			JournalHeaderForADAWCollection[0].PostPeriod = "";
			JournalHeaderForADAWCollection[0].CompanyCode = data.CompanyCode;
			JournalHeaderForADAWCollection[0].BranchCode = data.BranchCode;
			JournalHeaderForADAWCollection[0].GLJournalLines[0].CompanyCode = data.CompanyCode;
			JournalHeaderForADAWCollection[0].GLJournalLines[0].BranchCode = data.BranchCode;
			JournalHeaderForADAWCollection[0].GLJournalLines[0].GLAccount = data.GLAccount;
			JournalHeaderForADAWCollection[0].GLJournalLines[0].Currency = data.Currency;

			Notification.Clear();
			var convert = new GLJournalForADAWConverter(Notification, Factory, JournalHeaderForADAWCollection);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);

			CombineAssertions(() =>
			{
				AssertEquals(true, Notification.HasErrors);
				AssertEquals(errorMessage, Notification.AsString);
			});
		}

		public void TestImportGLJournalWithORGAttributeViaADAW_WhenLineHasNoCompanyAndBranch()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ObjectCreator.CreateTestPeriodsForEntireYear(2022);
			ObjectCreator.CreateAccAlternateGLAccountDissection(GlHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, false);
			var org = ObjectCreator.CreateOrgHeader("org1", false, true);
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlHeader.PK.ToGuid());
			Factory.Save();

			var line = JournalHeaderForADAWCollection[0].GLJournalLines[0];
			PopulateLine(line, org.OH_Code);

			var convert = new GLJournalForADAWConverter(Notification, Factory, JournalHeaderForADAWCollection);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);
			Assert(!Notification.HasErrors);

			XsdGLJournalCollection.Clear();
			var nonCurrentCompany = ObjectCreator.NonCurrentCompany;
			ObjectCreator.CreateTestPeriodsForEntireYear(nonCurrentCompany, 2022);
			Factory.Save();
			JournalHeaderForADAWCollection = SetupJournalADAWCollection("GJL", "202211", "", "");
			convert = new GLJournalForADAWConverter(Notification, Factory, JournalHeaderForADAWCollection);
			var header = JournalHeaderForADAWCollection[0];
			header.BranchCode = ObjectCreator.NonCurrentCompany.Branches[0].GB_Code;
			header.CompanyCode = "";
			PopulateLine(header.GLJournalLines[0], org.OH_Code);

			convert.ImportFlatFile(XsdGLJournalCollection, null, null);
			AssertContains($"Error: Header[1].Line[1].{nonCurrentCompany.GC_Code} - Invalid Attribute Value. A valid Attribute Value must be the code of a valid Receivable Organization (if Parent Account is an AR Control Account) or a valid Payable Organization (if Parent Account is an AP Control Account).\r\n", Notification.AsString);

			void PopulateLine(GLJournalLineForADAW line, ZString code)
			{
				line.AttributeORG = code;
				line.CompanyCode = "";
				line.BranchCode = "";
			}
		}

		void TestImportAttribute(string attr, string inValidErrorMessage, Action<GLJournalLineForADAW> setValidValue, Action<GLJournalLineForADAW> setInvalidValue, Action<GLJournalLineForADAW> setEmptyValue, bool enableReportingBooksFeature)
		{
			XsdGLJournalCollection = new GLJournalCollection();
			Notification.Clear();

			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableReportingBooksFeature);
			AssertEquals(1, JournalHeaderForADAWCollection.Count);
			AssertEquals(1, JournalHeaderForADAWCollection[0].GLJournalLines.Count);
			var line = JournalHeaderForADAWCollection[0].GLJournalLines[0];
			setValidValue(line);

			var convert = new GLJournalForADAWConverter(Notification, Factory, JournalHeaderForADAWCollection);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);
			if (enableReportingBooksFeature)
			{
				Assert(Notification.HasErrors);
				AssertEquals(@$"Error: Header[1].Line[1].EDI - {attr} attribute value cannot be specified for GL Account '1010.10.10'. Please check your dissection configuration.
", Notification.AsString);
			}
			else
			{
				Assert(!Notification.HasErrors);
			}
			Notification.Clear();

			var dissection = ObjectCreator.CreateAccAlternateGLAccountDissection(GlHeader, Chart.PK, attr, false);
			Factory.Save();
			setInvalidValue(line);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);
			if (enableReportingBooksFeature)
			{
				Assert(Notification.HasErrors);
				AssertEquals(inValidErrorMessage, Notification.AsString);
			}
			else
			{
				Assert(!Notification.HasErrors);
			}
			Notification.Clear();

			setEmptyValue(line);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);
			Assert(!Notification.HasErrors);

			setValidValue(line);

			if (attr == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
			{
				convert.ImportFlatFile(XsdGLJournalCollection, null, null);
				if (enableReportingBooksFeature)
				{
					Assert(Notification.HasErrors);
					AssertEquals(inValidErrorMessage, Notification.AsString);
				}
				else
				{
					Assert(!Notification.HasErrors);
				}
				Notification.Clear();

				ObjectCreator.AALSHI.OH_IsDebtor = true;
				AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlHeader.PK.ToGuid());
				Factory.Save();

				line.AttributeORG = ObjectCreator.AALSHI.OH_Code;
				convert.ImportFlatFile(XsdGLJournalCollection, null, null);
				Assert(!Notification.HasErrors);
				AssertEquals(enableReportingBooksFeature, XsdGLJournalCollection.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => y.ORG == ObjectCreator.AALSHI.OH_Code)));
			}
			else
			{
				convert.ImportFlatFile(XsdGLJournalCollection, null, null);
				Assert(!Notification.HasErrors);
				switch (attr)
				{
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG:
						AssertEquals(enableReportingBooksFeature, XsdGLJournalCollection.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => y.OCG == line.AttributeOCG)));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE:
						AssertEquals(enableReportingBooksFeature, XsdGLJournalCollection.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => y.LFE == line.AttributeLFE)));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO:
						AssertEquals(enableReportingBooksFeature, XsdGLJournalCollection.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => y.LFO == line.AttributeLFO)));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC:
						AssertEquals(enableReportingBooksFeature, XsdGLJournalCollection.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => y.TIC == line.AttributeTIC)));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR:
						AssertEquals(enableReportingBooksFeature, XsdGLJournalCollection.Cast<GLJournal>().Any(x => x.JournalLines.Cast<GLJournalJournalLine>().Any(y => y.SPR == line.AttributeSPR)));
						break;
				}
			}

			dissection.ADC_SeparateNumbering = true;
			Factory.Save();
			setEmptyValue(line);
			convert.ImportFlatFile(XsdGLJournalCollection, null, null);
			if (enableReportingBooksFeature)
			{
				Assert(Notification.HasErrors);
				AssertEquals(@$"Error: Header[1].Line[1].EDI - {attr} attribute value must be specified for GL Account '1010.10.10' because separate numbering is required. Please check your dissection configuration.
", Notification.AsString);
			}
			else
			{
				Assert(!Notification.HasErrors);
			}
		}

		AccGLHeader GlHeader;
		AccGLHeader GlHeader2;
		TestObjectCreator ObjectCreator;
		NotificationBuffer Notification;
		GLJournalHeaderForADAWCollection JournalHeaderForADAWCollection;
		GLJournalCollection XsdGLJournalCollection;
		AccAlternateChart Chart;

		protected override void SetUp()
		{
			base.SetUp();

			XsdGLJournalCollection = new GLJournalCollection();
			Notification = new NotificationBuffer();
			ObjectCreator = testObjectCreator;
			GlHeader = ObjectCreator.CreateGLHeader("1010.10.10");
			GlHeader2 = ObjectCreator.CreateGLHeader("1010.10.20");
			JournalHeaderForADAWCollection = SetupJournalADAWCollection("GJL", "202211", "", "");

			ObjectCreator.CreateTestPeriodsForEntireYear(2022);
			Chart = ObjectCreator.CreateAlternateChart("MGT");
			ObjectCreator.CreateAccAlternateChartFormat(Chart, 1, "9");
		}

		GLJournalHeaderForADAWCollection SetupJournalADAWCollection(string journalType, string postPeriod, string postDate, string reverseOrEndDate, string headerType = "H")
		{
			var journalHeaderForADAWCollection = new GLJournalHeaderForADAWCollection(Factory, headerType);

			var journalHeader = journalHeaderForADAWCollection.AddNew();

			if (headerType == "H")
			{
				journalHeader.JournalType = journalType;
				journalHeader.CompanyCode = "EDI";
				journalHeader.BranchCode = "SYD";
				journalHeader.PostPeriod = postPeriod;
				journalHeader.JournalDescription = "Journal Description";
				journalHeader.PresentationCategory = "ELM";
				journalHeader.PostDate = postDate;
				journalHeader.ReverseOrEndDate = reverseOrEndDate;

				var journalLine = journalHeader.GLJournalLines.AddNew();
				journalLine.CompanyCode = "EDI";
				journalLine.BranchCode = "SYD";
				journalLine.GLAccount = GlHeader.AccountNum;
				journalLine.DepartmentCode = "BRN";
				journalLine.Currency = "AUD";
				journalLine.LocalAmount = "100";
				journalLine.JournalLineDescription = "Line Description";
				journalLine.OrganisationCode = "CARGOWSYD";
				journalLine.SubAccountType1 = "ORG";
				journalLine.SubAccountValue1 = "CARINT";
			}
			else if (headerType == "F")
			{
				journalHeader.JournalType = journalType;
				journalHeader.PostPeriod = postPeriod;
				journalHeader.PostDate = postDate;
				journalHeader.JournalDescription = "Journal Description";

				var companyCode = GlbCompany.CurrentCompany.GC_Code;
				var branchCode = GlbBranch.CurrentBranch.GB_Code;
				var departmentCode = GlbDepartment.CurrentDepartment.GE_Code;

				var journalLine = journalHeader.GLJournalLines.AddNew();
				journalLine.CompanyCode = companyCode;
				journalLine.BranchCode = branchCode;
				journalLine.GLAccount = GlHeader.AccountNum;
				journalLine.DepartmentCode = departmentCode;
				journalLine.Currency = "USD";
				journalLine.Amount = "100";
				journalLine.JournalLineDescription = "Line Description1";

				var journalLine2 = journalHeader.GLJournalLines.AddNew();
				journalLine2.CompanyCode = companyCode;
				journalLine2.BranchCode = branchCode;
				journalLine2.GLAccount = GlHeader2.AccountNum;
				journalLine2.DepartmentCode = departmentCode;
				journalLine2.Currency = "USD";
				journalLine2.Amount = "-100";
				journalLine2.JournalLineDescription = "Line Description2";
			}

			return journalHeaderForADAWCollection;
		}
	}
}
