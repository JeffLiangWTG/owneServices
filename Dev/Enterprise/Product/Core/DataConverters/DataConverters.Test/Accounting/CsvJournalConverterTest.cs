using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DataConverters.Accounting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Accounting
{
	class CsvJournalConverterTest : TestCaseWithFactory
	{
		[TestDate(2003, 06, 05)]
		public void TestValidateOrganisationDebtorCreditor()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			Organisation.OH_IsDebtor = false;
			Organisation.OH_IsCreditor = true;
			Factory.Save();
			var line1 = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(line1, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			AssertEquals("Should be False", false, testConverter.IsValid);

			Organisation.OH_IsDebtor = true;
			Organisation.OH_IsCreditor = false;
			Factory.Save();
			var line2 = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"200.11\",\"200.11\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(line2, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			AssertEquals("Should be true", true, testConverter.IsValid);

			Organisation.OH_IsDebtor = false;
			Organisation.OH_IsCreditor = true;
			Factory.Save();
			var line3 = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"300.11\",\"300.11\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(line3, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			testConverter.CreateJournal();
			AssertEquals("Should be true", true, testConverter.IsValid);

			Organisation.OH_IsDebtor = true;
			Organisation.OH_IsCreditor = false;
			Factory.Save();
			var line4 = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"400.11\",\"400.11\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(line4, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			testConverter.CreateJournal();
			AssertEquals("Should be false", false, testConverter.IsValid);
		}

		[TestDate(2003, 06, 05)]
		public void TestValidateInActiveOrganisation()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			Organisation.OH_IsDebtor = true;
			Organisation.OH_IsActive = false;

			var line = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			AssertEquals("InActive Org, Should be False", false, testConverter.IsValid);

			Organisation.OH_IsActive = true;
			testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			AssertEquals("Should be true", true, testConverter.IsValid);
		}

		public void TestValidateBranchDepartmentCombinations()
		{
			var bbbDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			Organisation.OH_IsDebtor = true;
			Organisation.OH_IsActive = true;
			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);

			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { bbbDepartment });

			var line = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			var expectedError = string.Format(@"[Accounts Receivable Journal] Department: The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
				, Env.CurrentDepartment.Code, Env.CurrentBranch.Code);

			AssertContains("Must have the error", expectedError, testConverter.Errors);
		}

		public void TestIsValid()
		{
			var line = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));

			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert(testConverter.IsValid);
		}

		public void TestCreateJournal()
		{
			var line = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));

			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert(testConverter.IsValid);
			var journal = testConverter.CreateJournal();
			AssertNotNull(journal);
		}

		public void TestIsDuplicate()
		{
			var line = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));

			var testConverter1 = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert(testConverter1.IsValid);
			testConverter1.CreateJournal();
			Factory.Save();

			var testConverter2 = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert(testConverter2.IsDuplicate);
		}

		public void TestSQLHandlesEmbeddedSingleQuote()
		{
			var line = new OCsvLine(string.Format("\"{0}\",\"Transaction1 June '05\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));

			var testConverter1 = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert(testConverter1.IsValid);
			testConverter1.CreateJournal();
			Factory.Save();

			var testConverter2 = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert(testConverter2.IsDuplicate);
		}

		public void TestSQLHandlesKeyBoardCharacters()
		{
			var line = new OCsvLine(string.Format("\"{0}\",\"Trans_* June '05, & + 10% uplift @ 15/05/05 (#JS! = $78.90)\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));

			var testConverter1 = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert(testConverter1.IsValid);
			testConverter1.CreateJournal();
			Factory.Save();

			var testConverter2 = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert(testConverter2.IsDuplicate);
		}

		public void TestIsOrganisationValid()
		{
			var line = new OCsvLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));

			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert(testConverter.IsValid);

			var journal = testConverter.CreateJournal();
			Factory.Save();
			AssertEquals(Organisation.PK, journal.AH_OH);
		}

		[TestDate(2003, 06, 05)]
		public void TestErrors()
		{
			var line = new OCsvLine(string.Format("\"{0}\",\"\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));

			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			Assert(!testConverter.IsValid);
			Assert(!testConverter.Errors.IsEmpty);
		}
		[TestDate(2003, 06, 05)]
		public void TestValidateRowForOrgPK()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var validOrgLine = new OCsvLine(string.Format("\"{0}\",\"Desc\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validOrgLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			Assert("Should have found valid organisation", testConverter.IsValid);

			var invalidOrgLine = new OCsvLine(string.Format("\"QWERTY.SAFDG.0001\",\"Desc\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\""));
			testConverter = new CsvJournalConverter(invalidOrgLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			Assert("Invalid organisation should error", !testConverter.IsValid);

			var organisationOrgLegacyCode = Organisation.CustomsCodes.AddNew();
			organisationOrgLegacyCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			organisationOrgLegacyCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			organisationOrgLegacyCode.OK_CustomsRegNo = "LegacyCodeTest";
			Factory.Save();

			var orgLineWithLegacyCode = new OCsvLine(string.Format("\"LegacyCodeTest\",\"Desc\",\"20030605\",\"20030705\",\"AUD\",\"200.11\",\"200.11\""));
			testConverter = new CsvJournalConverter(orgLineWithLegacyCode, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			Assert("Should have found valid organisation via legacy code", testConverter.IsValid);
		}

		public void TestGetBranch()
		{
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Desc\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var testJournal = testConverter.CreateJournal();
			AssertEquals("Journal should be for Global branch", GlbBranch.CurrentBranch.PK, testJournal.AH_GB);

			validLine = new OCsvLine(string.Format("\"{0}\",\"Desc\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\",\"" + TestBranch.GB_Code + "\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testJournal = testConverter.CreateJournal();
			AssertEquals("Journal should be for Test branch", TestBranch.PK, testJournal.AH_GB);
		}

		public void TestForEqualityOfForeignAndLocalAmountWhenXRateOne()
		{
			Organisation.OH_IsCreditor = true;
			var validLine = new OCsvLine(string.Format("\"{0}\",\"00001002\",\"20130902\",\"20140329\",\"IDR\",\"189348.5\",\"189348.701\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);

			Assert("Due to too many decimal places the data is invalid.", !testConverter.IsValid);
			AssertEquals("Local Amount has a decimal part longer than allowed. Currency settings allows 2 decimal places 	Foreign Amount has a decimal part longer than allowed. Currency settings allows 0 decimal places ", testConverter.Errors);

			var testJournal = testConverter.CreateJournal();
			AssertNull(testJournal);
		}

		public void TestForEqualityOfForeignAndLocalAmountWhenLocalCurrencyAndCurrencyAreSame()
		{
			Organisation.OH_IsCreditor = true;

			var validLine = new OCsvLine(string.Format("\"{0}\",\"00001002\",\"20130902\",\"20140329\",\"AUD\",\"189348.5\",\"189348.6\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			Assert("As Currency and Local Currency are same, invoice amount and foreign amount should be equal.(Invalid)", !testConverter.IsValid);
			AssertEquals("As Currency and Local Currency are same,  Invoice Amount and Foreign Amount must be equal. ", testConverter.Errors);
			var testJournal = testConverter.CreateJournal();
			AssertNull(testJournal);

			validLine = new OCsvLine(string.Format("\"{0}\",\"00001002\",\"20130902\",\"20140329\",\"AUD\",\"189348.5\",\"189348.5\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			Assert("As Currency and Local Currency are same, invoice amount and foreign amount should be equal.(Valid)", testConverter.IsValid);
			testJournal = testConverter.CreateJournal();
			AssertNotNull(testJournal);
		}

		public void TestGetDepartment()
		{
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Desc\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var testJournal = testConverter.CreateJournal();
			AssertEquals("Journal should be for Global department", GlbDepartment.CurrentDepartment.PK, testJournal.AH_GE);

			validLine = new OCsvLine(string.Format("\"{0}\",\"Desc\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\",\"\",\"" + TestDeparment.GE_Code + "\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testJournal = testConverter.CreateJournal();
			AssertEquals("Journal should be for Test Department", TestDeparment.PK, testJournal.AH_GE);
		}

		[TestDate(2003, 06, 05)]
		public void TestGetCurrency()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"20030605\",\"20030705\"," + GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency.ToString() + ",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var testJournal = testConverter.CreateJournal();
			Assert(testConverter.IsValid);
			AssertEquals("Currency should be for Current Company Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testJournal.AH_RX_NKTransactionCurrency);

			validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"20030605\",\"20030705\",\"USA\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testJournal = testConverter.CreateJournal();
			Assert("Currency USA should be invalid", !testConverter.IsValid);
		}

		[TestDate(2003, 06, 05)]
		public void TestDescription()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var testJournal = testConverter.CreateJournal();
			Assert(testConverter.IsValid);
			AssertEquals("Description should be Test Inv", "Test Inv", testJournal.AH_Desc);

			validLine = new OCsvLine(string.Format("\"{0}\",\"21231\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testJournal = testConverter.CreateJournal();
			Assert(testConverter.IsValid);
			AssertEquals("Description should be 21231", "21231", testJournal.AH_Desc);

			validLine = new OCsvLine(string.Format("\"{0}\",\"\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testJournal = testConverter.CreateJournal();
			Assert("No description should be invalid", !testConverter.IsValid);

			var description = new string('d', AccTransactionHeaderSchema.AH_Desc.MaxLength + 1);
			validLine = new OCsvLine(string.Format("\"{0}\",\"" + description + "\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testJournal = testConverter.CreateJournal();
			Assert("Too long description", !testConverter.IsValid);
		}

		public void TestIsInDateRange()
		{
			var inValidLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"21030605\",\"21030722\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(inValidLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var testJournal = testConverter.CreateJournal();
			AssertEquals("Invalid Date", false, testConverter.IsValid);
			AssertContains("Is not a valid line because date it outside bounds of SmallDateTime in SQL", "The date '05-Jun-2103' is later than '06-Jun-2079', the limit for this field.", testConverter.Errors);
		}

		[TestDate(2003, 06, 05)]
		public void TestInvoiceDate()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"20030605\",\"20030722\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var testJournal = testConverter.CreateJournal();
			Assert(testConverter.IsValid);
			AssertEquals("Invoice Date", "2003-06-05 00:00:00.000", testJournal.AH_InvoiceDate.SqlFormat);
			AssertEquals("Due Date", "2003-07-22 00:00:00.000", testJournal.AH_DueDate.SqlFormat);
		}

		public void TestInvoiceDate_Invalid()
		{
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"2003-06-06\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			AssertEquals("Should be false", false, testConverter.IsValid);
		}

		public void TestInvalidDecimalFormatForLocalAmount()
		{
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"20030606\",\"20030705\",\"USD\",\"120.11\",\"ABC\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert("Should be false", !testConverter.IsValid);
			AssertEquals("Local Amount is in incorrect format. ", testConverter.Errors);
		}

		[TestDate(2003, 06, 06)]
		public void TestInvalidDecimalFormatForForeignAmount()
		{
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"20030606\",\"20030705\",\"USD\",\"ABC\",\"120.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert("Should be false", !testConverter.IsValid);
			AssertEquals("Foreign Amount is in incorrect format. ", testConverter.Errors);
		}

		public void TestMandatoryFieldLocalAmount()
		{
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"20030606\",\"20030705\",\"USD\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			Assert("Should be false", !testConverter.IsValid);
			AssertContains("[Accounts Receivable Journal] Local Amount: Please enter a Local Amount.", testConverter.Errors);
		}

		public void TestMandatoryFieldForeignAmount()
		{
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"20030606\",\"20030705\",\"USD\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			Assert("Should be false", !testConverter.IsValid);
			AssertContains("[Accounts Receivable Journal] Local Amount: Please enter a Local Amount.\n[Accounts Receivable Journal] Amount: Please enter an Amount.", testConverter.Errors);
		}

		[TestDate(2003, 06, 05)]
		public void TestIsDateWithinRange()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			Assert(testConverter.IsValid);

			validLine = new OCsvLine(string.Format("\"{0}\",\"Test Inv\",\"17200101\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(validLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			Assert("Date is less than 1/1/1753", !testConverter.IsValid);
		}

		public void TestARJournalAmountsAreCorrectSign()
		{
			var line = new OCsvLine(string.Format("\"{0}\",\"Desc\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();

			// Not affected by Multiplier

			AssertEquals("Invoice Amount should be positive", 100.11M, journal.AH_InvoiceAmount);
			AssertEquals("OS Total should be positive", 100.11M, journal.AH_OSTotal);
			AssertEquals("Outstanding Amount should be positive", 100.11M, journal.AH_OutstandingAmount);

			// Affected by Multiplier
			AssertEquals("LocalExTaxAmount for should be positive", 100.11M, journal.AH_LocalExTaxAmount);
			AssertEquals("OSExTaxAmount for AP Journal should be positive", 100.11M, journal.AH_OSExTaxAmount);
			AssertEquals("LocalOutstandingAmount should be positive", 100.11M, journal.AH_LocalOutstandingAmount);
		}

		public void TestAPJournalAmountsAreCorrectSign()
		{
			Organisation.OH_IsCreditor = true;

			var line = new OCsvLine(string.Format("\"{0}\",\"Desc\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();

			// Not affected by Multiplier
			AssertEquals("Invoice Amount should be negative", -100.11M, journal.AH_InvoiceAmount);
			AssertEquals("OS Total should be negative", -100.11M, journal.AH_OSTotal);
			AssertEquals("Outstanding Amount should be negative", -100.11M, journal.AH_OutstandingAmount);

			// Affected by Multiplier
			AssertEquals("LocalExTaxAmount for should be positive", 100.11M, journal.AH_LocalExTaxAmount);
			AssertEquals("OSExTaxAmount for AP Journal should be positive", 100.11M, journal.AH_OSExTaxAmount);
			AssertEquals("LocalOutstanding Amount should be positive", 100.11M, journal.AH_LocalOutstandingAmount);
		}

		public void TestBranchValidationForNonCurrentBranch()
		{
			var diffCompany = Factory.NewWithValidTestData<GlbCompany>();
			var diffCompanyBranch = GetNewBranch(diffCompany);
			var sameCompanyBranch = GetNewBranch(GlbCompany.CurrentCompany);
			Factory.Save();

			var diffCompanyBranchLine = new OCsvLine(string.Format("\"{0}\",\"DESC\",\"20050605\",\"20050705\",\"AUD\",\"100.11\",\"100.11\",\"" + diffCompanyBranch.GB_Code + "\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(diffCompanyBranchLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert("There should be errors on the JournalConverter since Branch is from a different company", !testConverter.Errors.IsEmpty);
			AssertEquals("The error should read as follows:", "Transaction branch must be from the current company. ", testConverter.Errors);
			Assert("The line should not be valid", !testConverter.IsValid);

			var sameCompanyBranchLine = new OCsvLine(string.Format("\"{0}\",\"DESC\",\"20050605\",\"20050705\",\"AUD\",\"100.11\",\"100.11\",\"" + sameCompanyBranch.GB_Code + "\"", Organisation.OH_Code));
			testConverter = new CsvJournalConverter(sameCompanyBranchLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			Assert("There should not be errors on the Journal Converter since Branch is for a different company", testConverter.Errors.IsEmpty);
			Assert("The line should be valid", testConverter.IsValid);
		}

		[TestDate(2003, 06, 05)]
		public void TestOrganisationError()
		{
			Organisation.OH_IsDebtor = false;
			Organisation.OH_IsCreditor = true;
			Organisation.OH_Code = "LGT";

			var organisationOrgLegacyCode = Organisation.CustomsCodes.AddNew();
			organisationOrgLegacyCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			organisationOrgLegacyCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			organisationOrgLegacyCode.OK_CustomsRegNo = "LegacyCodeTest";
			Factory.Save();

			var orgLineWithLegacyCode = new OCsvLine(string.Format("\"LegacyCodeTest\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\""));
			var testConverter = new CsvJournalConverter(orgLineWithLegacyCode, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			Assert("Should have found valid organisation via legacy code", !testConverter.IsValid);
			AssertContains("Incorrect Error Text", "[Accounts Receivable Journal] Account: The Organization must have an Organization Type of Receivables selected.", testConverter.Errors);
		}

		public void TestMultipleErrorText()
		{
			var orgLineWithLegacyCode = new OCsvLine(string.Format("\"LegacyCodeTest\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"0.00\",\"0.00\""));
			var testConverter = new CsvJournalConverter(orgLineWithLegacyCode, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			testConverter.CreateJournal();
			AssertEquals("Incorrect Error Text",
				Res.GetString("a4c572e8-44e1-4705-a29a-e6ef86bd14de", "Organization code is invalid: ({0}). ", "LegacyCodeTest"), testConverter.Errors);
		}

		public void TestIncorrectFormat()
		{
			var incorrectFormatLine = new OCsvLine(string.Format("\"TestOrg\",\"DESC\""));
			var testConverter = new CsvJournalConverter(incorrectFormatLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var expectedMessage = "Incorrect format: Expected [organization, description, invoice date, due date, currency]. The line is: '\"TestOrg\",\"DESC\"' \tOrganization code is invalid: (). \tCurrency code is invalid: ( ). \tTransaction branch must be from the current company. ";
			AssertEquals("Incorrect Format", expectedMessage, testConverter.Errors);
		}

		public void TestLocalAmountEqualOsAmountWhenRateIsOne()
		{
			var incorrectFormatLine = new OCsvLine(string.Format($"\"{Organisation.OH_Code}\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"-6326.05\",\"-6326.05\""));
			var testConverter = new CsvJournalConverter(incorrectFormatLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals(journal.AH_RX_NKTransactionCurrency, GlbCompany.CurrentCompany.LocalCurrency.Code);
			AssertEquals(journal.AH_InvoiceAmount, journal.AH_OSTotal);
		}

		[TestDate(2003, 06, 05)]
		public void TestPostDateWhenSetToFutureDate()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var csvLine = new OCsvLine(string.Format($"\"{Organisation.OH_Code}\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"6326.05\",\"6326.05\""));
			var testConverter = new CsvJournalConverter(csvLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			_ = testConverter.CreateJournal();
			Assert(testConverter.IsValid);
			Assert(testConverter.Errors.IsEmpty);

			testConverter = new CsvJournalConverter(csvLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now.AddDays(1));
			_ = testConverter.CreateJournal();
			Assert(!testConverter.IsValid);
			AssertEquals("[Accounts Receivable Journal] Invoice Post Date: The post date cannot be in the future", testConverter.Errors);
		}

		[TestDate(2003, 06, 05)]
		public void TestARJournalsWhenAmountsAreNegative()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var csvLine = new OCsvLine(string.Format($"\"{Organisation.OH_Code}\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"-6326.05\",\"-6326.05\""));
			var testConverter = new CsvJournalConverter(csvLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();
			Assert("Converter data should be valid",testConverter.IsValid);
		}

		[TestDate(2003, 06, 05)]
		public void TestAPJournalsWhenAmountsAreNegative()
		{
			Organisation.OH_IsCreditor = true;
			Factory.Save();
			AccountingPeriodTestHelper.SetupPeriods();
			var csvLine = new OCsvLine(string.Format($"\"{Organisation.OH_Code}\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"-6326.05\",\"-6326.05\""));
			var testConverter = new CsvJournalConverter(csvLine, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();
			Assert("Converter data should be valid",testConverter.IsValid);
		}

		[TestDate(2003, 06, 05)]
		public void TestAPJournalsDebitCreditSignIsCreditWhenAmountsAreNegative()
		{
			Organisation.OH_IsCreditor = true;
			Factory.Save();
			AccountingPeriodTestHelper.SetupPeriods();
			var csvLine = new OCsvLine(string.Format($"\"{Organisation.OH_Code}\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"-6326.05\",\"-6326.05\""));
			var testConverter = new CsvJournalConverter(csvLine, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();
			AssertEquals(DebitCreditDataEntry.CR, journal.DebitCreditSign);
		}

		[TestDate(2003, 06, 05)]
		public void TestAPJournalsDebitCreditSignIsDebitWhenAmountsArePositive()
		{
			Organisation.OH_IsCreditor = true;
			Factory.Save();
			AccountingPeriodTestHelper.SetupPeriods();
			var csvLine = new OCsvLine(string.Format($"\"{Organisation.OH_Code}\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"6326.05\",\"6326.05\""));
			var testConverter = new CsvJournalConverter(csvLine, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();
			AssertEquals(DebitCreditDataEntry.DR, journal.DebitCreditSign);
		}

		[TestDate(2003, 06, 05)]
		public void TestARJournalsDebitCreditSignIsDebitWhenAmountsAreNegative()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var csvLine = new OCsvLine(string.Format($"\"{Organisation.OH_Code}\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"-6326.05\",\"-6326.05\""));
			var testConverter = new CsvJournalConverter(csvLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();
			AssertEquals(DebitCreditDataEntry.DR, journal.DebitCreditSign);
		}

		[TestDate(2003, 06, 05)]
		public void TestARJournalsDebitCreditSignIsCreditWhenAmountsArePositive()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var csvLine = new OCsvLine(string.Format($"\"{Organisation.OH_Code}\",\"DESC\",\"20030605\",\"20030705\",\"AUD\",\"6326.05\",\"6326.05\""));
			var testConverter = new CsvJournalConverter(csvLine, Factory, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();
			AssertEquals(DebitCreditDataEntry.CR, journal.DebitCreditSign);
		}

		#region Implementation

		public void TestDifferentSignsGivesError()
		{
			var line = new OCsvLine(string.Format("\"{0}\",\"\",\"20030605\",\"20030705\",\"USD\",\"-100.11\",\"100.11\"", Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			testConverter.CreateJournal();

			Assert("Expecting Error Message: Local Amount must be the same sign as Foreign Amount. ", testConverter.Errors.Contains("Local Amount must be the same sign as Foreign Amount. "));
		}

		public void TestGetCurrencyPKForBranch()
		{
			var findACompanyThatUsesADifferentCurrency = new ZQuery(GlbCompanySchema.GC_RX_NKLocalCurrency, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			var company = Factory.LoadTop1<GlbCompany>(findACompanyThatUsesADifferentCurrency);

			var branch = company.Branches[0];

			Organisation.OH_IsCreditor = true;

			ZString input = "\"{0}\",\"Desc\",\"20030605\",\"20030705\",\"" + company.GC_RX_NKLocalCurrency + "\",\"100.11\",\"100.11\"" + branch.GB_Code + "\"";

			var line = new OCsvLine(string.Format(input, Organisation.OH_Code));
			var testConverter = new CsvJournalConverter(line, Factory, LedgerTypes.AccountsPayable, ZDateTime.Now);
			var journal = testConverter.CreateJournal();

			AssertEquals("Journal should use the currency obtained from the transaction branch, not the currenct branch or company", company.GC_RX_NKLocalCurrency, journal.AH_RX_NKTransactionCurrency);
		}

		protected RefCurrency USD;
		protected OrgHeader Organisation;
		protected GlbBranch TestBranch;
		protected GlbDepartment TestDeparment;

		protected override void SetUp()
		{
			base.SetUp();
			USD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			Organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Organisation.OH_IsDebtor = true;
			Organisation.OH_IsCreditor = false;
			Organisation.OH_Code = "Test Code";
			Factory.Save();

			TestBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
			TestDeparment = Factory.LoadTop1<GlbDepartment>(new ZQuery());
		}

		GlbBranch GetNewBranch(GlbCompany company)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			return branch;
		}

		AccountingPeriodTestHelper AccountingPeriodTestHelper
		{
			get { return accountingPeriodTestHelper ?? (accountingPeriodTestHelper = new AccountingPeriodTestHelper()); }
		}
		AccountingPeriodTestHelper accountingPeriodTestHelper;

		#endregion
	}
}
