using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	class FileGeneratorITAFileFormatDataHelperTest : TestCaseWithFactory
	{
		public void TestExceptionWhenCollectionBatchOrBankAccountOrCompanyIsNull()
		{
			var datetime = ZDateTime.Now;
			AssertNotNull("Precondition:", CollectionBatch.BankAccount);
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: glbCompany", () => new FileGeneratorITAFileFormatDataHelper(CollectionBatch, null, false, datetime));
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: glbCompany", () => new FileGeneratorITAFileFormatDataHelper(CollectionBatch, null, true, datetime));

			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: accCollectionBatch", () => new FileGeneratorITAFileFormatDataHelper(null, Company, false, datetime));
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: accCollectionBatch", () => new FileGeneratorITAFileFormatDataHelper(null, Company, true, datetime));

			AssertNoExceptionThrown(() => new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, false, datetime));
			AssertNoExceptionThrown(() => new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, datetime));

			CollectionBatch.ACB_AB = ZGuid.Empty;
			AssertNull("Precondition:", CollectionBatch.BankAccount);
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: BankAccount", () => new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, false, datetime));
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: BankAccount", () => new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, datetime));

			CollectionBatch.ACB_AB = BankAccount.PK;
			AssertNoExceptionThrown(() => new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, false, datetime));
			AssertNoExceptionThrown(() => new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, datetime));
		}

		[TestDate(2022, 08, 30)]
		public void TestGetHeaderRecord_IsProductionSystem_True()
		{
			AssertNotNull("Precondition:", CollectionBatch.BankAccount);
			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);

			var actualBeginString = collectionBatchFileGenerator.GetHeaderRecord(1);
			AssertStartsWith("", "01REMESSA01COBRANCA       ", actualBeginString);
		}

		public void TestGetHeaderRecord_IsProductionSystem_False()
		{
			AssertNotNull("Precondition:", CollectionBatch.BankAccount);
			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, false, ZDateTime.Now);

			var actualBeginString = collectionBatchFileGenerator.GetHeaderRecord(1);
			AssertStartsWith("", "01TESTE  01COBRANCA       ", actualBeginString);
		}

		[TestDate(2022, 08, 30)]
		public void TestGetHeaderRecord()
		{
			AssertNotNull("Precondition:", CollectionBatch.BankAccount);
			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			int num = 1;

			SetupBankAccount("", "2", "", "BANK Account Name", "2", "BankName");
			AssertGetHeaderDetails("    0000002 ", "BANK Account Name             002BankName       ");

			SetupBankAccount("44", "33", "1", "BANK Account Name TestTestTest", "555", "BankNameTestSet");
			AssertGetHeaderDetails("004400000331", "BANK Account Name TestTestTest555BankNameTestSet");

			SetupBankAccount("444444", "3333333", "1111", "BANK Account Name TestTestAAAAAAAA", "555", "BankNameTest BankNameTest");
			AssertGetHeaderDetails("444400333331", "BANK Account Name TestTestAAAA555BankNameTest Ba");

			void AssertGetHeaderDetails(string expectedBankAccount_BSBAccNumUniqueAccNum, string expectedBankAccountNameAbbrevBankName)
			{
				var expectedString = "01REMESSA01COBRANCA       "
					+ expectedBankAccount_BSBAccNumUniqueAccNum
					+ "        "
					+ expectedBankAccountNameAbbrevBankName
					+ "300822"
					+ string.Empty.PadRight(294)
					+ "000001";

				var actualresult = collectionBatchFileGenerator.GetHeaderRecord(num);
				AssertMultilineASCIIEquals(expectedString, actualresult);
			}
		}

		[TestDate(2022, 08, 30)]
		public void TestGetDetailsRecord_TransactionDebtorMainOfficeAddressDetails()
		{
			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			SetupDebtor_MainOfficeAddress("Organisation Full Name", "Address1", "Address2", "123456", "City", "KA");
			AssertContainsDebtorMainOfficeDetails("Organisation Full Name        ", "Address1                                Address2    123456  City           KA");

			SetupDebtor_MainOfficeAddress("Organisation Full Name Test Name", "Address1 - 5th Main, 4th Street, Tets Layout", "Address2 Test", "123456789", "City Karntk Test", "KARNTKA");
			AssertContainsDebtorMainOfficeDetails("Organisation Full Name Test Na", "Address1 - 5th Main, 4th Street, Tets LaAddress2 Tes12345678City Karntk TesKA");

			void AssertContainsDebtorMainOfficeDetails(string expectedFullName, string expectedAddress1Address2PostCodeCityState)
			{
				var expectedString = expectedFullName
					+ "          "
					+ expectedAddress1Address2PostCodeCityState
					+ "000000000000000000000000000000";

				var actualsResult = collectionBatchFileGenerator.GetDetailsRecord(ARInvoiceInLocalCurrency, 3);
				AssertContains(expectedString, actualsResult);
			}
		}

		public void TestGetDetailsRecord_CustomsRegNoOfBRCNPJFromCompanyOrgProxy_WhenBankAccountBranchIsNull()
		{
			SetupBankAccount("5555", "33333", "8", "BANK Account Name Test", "555", "BankNameTest");

			BankAccount.AB_GB = ZGuid.Empty;
			AssertNull("Precondition: Branch", BankAccount.Branch);

			Company.GC_OH_OrgProxy = CompanyOrgProxy.PK;
			AssertNotNull("Precondition: Company.OrgProxy", Company.OrgProxy);

			Company.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR#23|45,88.88%", Core.Constants.CountryCodes.Brazil);
			AssertNotNull("Precondition: OrgCusCodes of BR CNPJ", Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));

			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			var actualsResult = collectionBatchFileGenerator.GetDetailsRecord(ARInvoiceInLocalCurrency, 2);
			AssertContains("10223458888      555500333338", actualsResult);
		}

		public void TestGetDetailsRecord_CustomsRegNoOfBRCNPJFromBranchOrgProxy_WhenBankAccountBranchIsNotNull()
		{
			SetupBankAccount("5555", "33333", "8", "BANK Account Name Test", "555", "BankNameTest");

			AssertNotNull("Precondition: Branch", BankAccount.Branch);
			AssertEquals(Branch.PK, BankAccount.AB_GB);

			AssertNotNull("Precondition: Branch.OrgProxy", BankAccount.Branch.OrgProxy);
			AssertEquals(BranchOrgProxy.PK, Branch.GB_OH_OrgProxy);

			Branch.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "AB12#44,44.4488%", Core.Constants.CountryCodes.Brazil);
			AssertNotNull("Precondition: OrgCusCodes of BR CNPJ", Branch.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));

			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			var actualsResult = collectionBatchFileGenerator.GetDetailsRecord(ARInvoiceInLocalCurrency, 2);
			AssertContains("1021244444488    555500333338", actualsResult);
		}

		[TestDate(2022, 08, 30)]
		public void TestGetDetailsRecord_TotalInvoiceAmountWhenTransactionPostedInForeignCurrency()
		{
			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			CollectionOrderLine.AOL_AH = ARInvoiceInForeignCurrency.PK;

			AssertGetDetailsRecord_TotalInvoiceWhenTransactionPostedInForeignCurrency(400, 200, "0000040000000", "0000000020000");

			AssertGetDetailsRecord_TotalInvoiceWhenTransactionPostedInForeignCurrency(444.44, 222.22, "0000044444000", "0000000022222");

			AssertGetDetailsRecord_TotalInvoiceWhenTransactionPostedInForeignCurrency(666666664444.66, 333333332222.33, "6666444466000", "3333333222233");

			void AssertGetDetailsRecord_TotalInvoiceWhenTransactionPostedInForeignCurrency(ZDecimal osTotalAmount, ZDecimal localTotalAmount, string expectedTotalInvoiceInForeignCurrency, string expectedTotalInvoiceInLocalCurrency)
			{
				ARInvoiceInForeignCurrency.Lines[0].AL_OSExTaxAmount = osTotalAmount;

				var expectedPartofString = expectedTotalInvoiceInForeignCurrency
					+ "021                     I0100ARINV444300822"
					+ expectedTotalInvoiceInLocalCurrency;

				var actualsResult = collectionBatchFileGenerator.GetDetailsRecord(ARInvoiceInForeignCurrency, 2);
				AssertContains(expectedPartofString, actualsResult);
			}
		}

		[TestDate(2022, 08, 30)]
		public void TestGetDetailsRecord_TotalInvoiceAmountWhenTransactionPostedInLocalCurrency()
		{
			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			CollectionOrderLine.AOL_AH = ARInvoiceInLocalCurrency.PK;

			AssertGetDetailsRecord_TotalInvoiceWhenTransactionPostedInLocalCurrency(400, "0000000000000", "0000000040000");

			AssertGetDetailsRecord_TotalInvoiceWhenTransactionPostedInLocalCurrency(444.44, "0000000000000", "0000000044444");

			AssertGetDetailsRecord_TotalInvoiceWhenTransactionPostedInLocalCurrency(666666664444.66, "0000000000000", "6666666444466");

			void AssertGetDetailsRecord_TotalInvoiceWhenTransactionPostedInLocalCurrency(ZDecimal osTotalAmount, string expectedTotalInvoiceInForeignCurrency, string expectedTotalInvoiceInLocalCurrency)
			{
				ARInvoiceInLocalCurrency.Lines[0].AL_OSExTaxAmount = osTotalAmount;

				var expectedPartofString = expectedTotalInvoiceInForeignCurrency
					+ "021                     I0100ARINV111300822"
					+ expectedTotalInvoiceInLocalCurrency;

				var actualsResult = collectionBatchFileGenerator.GetDetailsRecord(ARInvoiceInLocalCurrency, 2);
				AssertContains(expectedPartofString, actualsResult);
			}
		}

		[TestDate(2022, 08, 30)]
		public void TestGetDetailsRecord_OrgCusCodeTypeOfReceivablesOrganizationOfTransaction_WhenBRCNPJOrCPFType()
		{
			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			AssertNotNull("Precondition: ", ARInvoiceInLocalCurrency.Header);
			AssertNotNull("Precondition:", ARInvoiceInLocalCurrency.Header.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ));

			CusCode_CNPJ.OK_CustomsRegNo = "1#23|45,67.89%";
			SetupBankAccount("5555", "34567", "5", "BANK Account Name Test", "555", "BankNameTest");
			SetupDebtor_MainOfficeAddress("Organisation Full Name", "Address1", "Address2", "123456", "City", "KA");
			AssertGetDetailRecord(collectionBatchFileGenerator, ARInvoiceInLocalCurrency, 2, "1021244444488    555500345675"
				, "INVREF1 0000000000000"
				, "00ARINV1113008220000000020000555"
				, "300822"
				, "02123456789     Organisation Full Name        "
				, "Address1                                Address2    123456  City           KA");

			ARInvoiceInLocalCurrency.Header.CustomsCodes.Remove(CusCode_CNPJ);
			AssertNull("Precondition:", ARInvoiceInLocalCurrency.Header.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ));
			AssertNotNull("Precondition:", ARInvoiceInLocalCurrency.Header.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration));

			CusCode_CPF.OK_CustomsRegNo = "888#888|88,88.99%";
			AssertGetDetailRecord(collectionBatchFileGenerator, ARInvoiceInLocalCurrency, 2, "1021244444488    555500345675"
				, "INVREF1 0000000000000"
				, "00ARINV1113008220000000020000555"
				, "300822"
				, "01888888888899  Organisation Full Name        "
				, "Address1                                Address2    123456  City           KA");
		}

		[TestDate(2022, 08, 30)]
		public void TestGetDetailsRecord_TransactionPostedInLocalOrForeignCurrency()
		{
			var collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			CusCode_CNPJ.OK_CustomsRegNo = "1#11.98%";
			SetupBankAccount("44", "33", "1", "BANK Account Name TestTestTest", "555", "BankNameTestSet");
			SetupDebtor_MainOfficeAddress("Organisation Full Name", "Address1", "Address2", "123456", "City", "KA");
			AssertGetDetailRecord(collectionBatchFileGenerator, ARInvoiceInLocalCurrency, 2, "1021244444488    004400000331"
				, "INVREF1 0000000000000"
				, "00ARINV1113008220000000020000555"
				, "300822"
				, "0211198         Organisation Full Name        "
				, "Address1                                Address2    123456  City           KA");

			CollectionOrderLine.AOL_AH = ARInvoiceInForeignCurrency.PK;
			AssertGetDetailRecord(collectionBatchFileGenerator, ARInvoiceInForeignCurrency, 2, "1021244444488    004400000331"
				, "INVREF2 0000040000000"
				, "00ARINV4443008220000000020000555"
				, "300822"
				, "0211198         Organisation Full Name        "
				, "Address1                                Address2    123456  City           KA");
		}

		void AssertGetDetailRecord(FileGeneratorITAFileFormatDataHelper collectionBatchFileGenerator, TransactionHeader transaction, int rownum, string expectedStringWithcustomsRegNo_BRCNPJ, string expectedBankAccount_BSBAccNumUniqueAccNum, string expectedTrNumDueDateTotalInvInLocalBankAccAbbrev, string expectedInvoiceDate, string expectedRegTypeCustomsRegNo_OrgFullName, string expectedAddress1Address2PostCodeCityState)
		{
			var expectedString = expectedStringWithcustomsRegNo_BRCNPJ
				+ "    0000                         "
				+ expectedBankAccount_BSBAccNumUniqueAccNum
				+ "021                     I01"
				+ expectedTrNumDueDateTotalInvInLocalBankAccAbbrev
				+ "0000001N"
				+ expectedInvoiceDate
				+ "00000000000000000000000000000000000000000000000000000000000000"
				+ expectedRegTypeCustomsRegNo_OrgFullName
				+ "          "
				+ expectedAddress1Address2PostCodeCityState
				+ "000000000000000000000000000000    00000000 "
				+ rownum.ToString().PadLeft(6, '0');

			var actualResult = collectionBatchFileGenerator.GetDetailsRecord(transaction, rownum);
			AssertMultilineASCIIEquals(expectedString, actualResult);
		}

		public void TestGetTrailerRecord()
		{
			var expectedString = "9" + string.Empty.PadRight(393);
			var collectionBatchFileGenerator1 = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, false, ZDateTime.Now);
			var collectionBatchFileGenerator2 = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);

			AssertGetTrailerRecord(4, expectedString, "000004");
			AssertGetTrailerRecord(4444, expectedString, "004444");

			void AssertGetTrailerRecord(int num, string expectedString1, string expectedString2)
			{
				var actualresult = collectionBatchFileGenerator1.GetTrailerRecord(num);
				AssertContains(expectedString1, actualresult);

				actualresult = collectionBatchFileGenerator2.GetTrailerRecord(num);
				AssertContains(expectedString2, actualresult);
			}
		}

		[TestDate(2022, 08, 30)]
		public void TestICollectionBatchFileGeneratorImplementation_GetFileData()
		{
			AssertNotNull("Precondition:", CollectionBatch.BankAccount);
			AssertNotNull("Precondition: ", CollectionBatch.CollectionOrders.Any());
			AssertEquals("Precondition: ", CollectionOrder, CollectionBatch.CollectionOrders.FirstOrDefault(x => x.IncludeInBatch));
			AssertNotNull("Precondition: ", CollectionBatch.CollectionOrders.FirstOrDefault(x => x.IncludeInBatch).CollectionOrderLines.Any());

			AssertEquals(ARInvoiceInLocalCurrency.PK, CollectionOrderLine.AOL_AH);
			AssertNotNull("Precondition: ", ARInvoiceInLocalCurrency.Header);
			AssertNotNull("Precondition:", ARInvoiceInLocalCurrency.Header.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ));

			ICollectionBatchFileGenerator collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			SetupBankAccount("44", "33", "1", "BANK Account Name TestTestTest", "555", "BankNameTestSet");
			CusCode_CNPJ.OK_CustomsRegNo = "1#11.98%";
			SetupDebtor_MainOfficeAddress("Organisation Full Name", "Address1", "Address2", "123456", "City", "KA");

			var expectedHeaderRecordString = "01REMESSA01COBRANCA       004400000331        BANK Account Name TestTestTest555BankNameTestSet300822"
				+ string.Empty.PadRight(294)
				+ "000001";

			var expectedDetailsRecordString = "1021244444488    004400000331    0000                         INVREF1 0000000000000021                     I0100ARINV11130082200000000200005550000001N300822000000000000000000000000000000000000000000000000000000000000000211198         Organisation Full Name                  Address1                                Address2    123456  City           KA000000000000000000000000000000    00000000 "
				+ "000002";

			var expectedTrailerRecordString = "9"
				+ string.Empty.PadRight(393)
				+ "000003";

			var expectedResult = string.Format(@"{0}
{1}
{2}"
, expectedHeaderRecordString, expectedDetailsRecordString, expectedTrailerRecordString);

			var actualResult = collectionBatchFileGenerator.GetFileData();
			AssertMultilineASCIIEquals(expectedResult, actualResult);
		}

		[TestDate(2022, 08, 30, 15, 48, 53)]
		public void TestICollectionBatchFileGeneratorImplementation_AttachFileToEdoc()
		{
			ICollectionBatchFileGenerator collectionBatchFileGenerator = new FileGeneratorITAFileFormatDataHelper(CollectionBatch, Company, true, ZDateTime.Now);
			var eDocs = ((IDocManagerSupport)CollectionBatch).DocManagerInfo.AllEDocs;

			AssertAttachFileToEdoc("Edocs not saved", false, null, 0);

			AssertAttachFileToEdoc("Edocs not saved", false, string.Empty, 0);

			AssertAttachFileToEdoc("Edocs saved", true, "Test file data", 1);
			AssertEquals("DocumentType: ", Core.Constants.RefDocTypes.MiscellaneousDocument, eDocs[0].DocType);
			AssertEquals("FileName: ", "RemessaITA30082022_154853.txt", eDocs[0].FileName);
			AssertEquals("Description: ", "Miscellaneous Document", eDocs[0].Description);
			AssertEquals("IsPublished: ", true, eDocs[0].IsPublished);
			AssertEquals("IsSystemGenerated: ", true, eDocs[0].IsSystemGenerated);

			void AssertAttachFileToEdoc(string message, bool expectedResult, string fileData, int expectedeDocsFilesSaved)
			{
				var actualResult = collectionBatchFileGenerator.AttachFileToEdoc(fileData);
				AssertEquals(message, expectedResult, actualResult);
				AssertEquals("Total Files count: ", expectedeDocsFilesSaved, eDocs.Count);
			}
		}

		void SetupBankAccount(string bSBNum, string accountNum, string fullAccountNum, string bankAccountName, string bankAbbreviation, string bankName)
		{
			BankAccount.AB_BSB = bSBNum;
			BankAccount.AB_AccountNum = accountNum;
			BankAccount.AB_FullAccountNumber = fullAccountNum;
			BankAccount.AB_BankAccountName = bankAccountName;
			BankAccount.AB_BankAbbreviation = bankAbbreviation;
			BankAccount.AB_BankName = bankName;
		}

		void SetupDebtor_MainOfficeAddress(string orgFullName, string orgAddress1, string orgAddress2, string orgAddressPostCode, string city, string state)
		{
			DebtorCC.OH_FullName = orgFullName;
			DebtorCC_MainAddress.OA_Address1 = orgAddress1;
			DebtorCC_MainAddress.OA_Address2 = orgAddress2;
			DebtorCC_MainAddress.OA_PostCode = orgAddressPostCode;
			DebtorCC_MainAddress.OA_City = city;
			DebtorCC_MainAddress.OA_State = state;
		}

		[TestDate(2022, 08, 30)]
		protected override void SetUp()
		{
			base.SetUp();

			CompanyOrgProxy = TestObjectCreator.CreateOrgHeader("DebtorCOP", false, true);
			BranchOrgProxy = TestObjectCreator.CreateOrgHeader("DebtorBOP", false, true);

			Company = TestObjectCreator.CreateNewCompany("DCC", Core.Constants.CountryCodes.Brazil);
			Branch = TestObjectCreator.CreateNewBranch(Company, "TTT");
			Branch.GB_OH_OrgProxy = BranchOrgProxy.PK;
			Branch.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "AA12#44,44.4488%", Core.Constants.CountryCodes.Brazil);

			DebtorCC = TestObjectCreator.CreateOrgHeader("DebtorCC", false, true);
			DebtorCC_MainAddress = DebtorCC.Addresses[0];
			DebtorCC_MainAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			CusCode_CPF = DebtorCC.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "BR0001", Core.Constants.CountryCodes.Brazil);
			CusCode_CNPJ = DebtorCC.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR0001", Core.Constants.CountryCodes.Brazil);

			BankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			BankAccount.AB_GB = Branch.PK;

			ARInvoiceInLocalCurrency = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1.0m, 200m, 0m, 200m, 0m);
			ARInvoiceInLocalCurrency.AH_OH = DebtorCC.PK;
			ARInvoiceInLocalCurrency.InvoiceRemittanceReference = "INVREF1";

			ARInvoiceInForeignCurrency = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.USD, 2.0m, 400m, 0m, 200m, 0m);
			ARInvoiceInForeignCurrency.AH_OH = DebtorCC.PK;
			ARInvoiceInForeignCurrency.InvoiceRemittanceReference = "INVREF2";
			Factory.Save();

			ARInvoiceInLocalCurrency.AH_TransactionNum = "ARINV111";
			ARInvoiceInForeignCurrency.AH_TransactionNum = "ARINV444";
			CollectionBatch = TestObjectCreator.CreateCollectionBatch(BankAccount, Company, "B000010", 100, false);
			CollectionBatch.ACB_CollectionFileFormat = CollectionFileFormatList.Codes.itauBank;
			CollectionOrder = TestObjectCreator.CreateCollectionOrder(CollectionBatch, ZDate.Today, DebtorCC, "ORD001", 100, false);
			CollectionOrder.IncludeInBatch = true;
			CollectionOrderLine = TestObjectCreator.CreateCollectionOrderLine(CollectionOrder, ARInvoiceInLocalCurrency, false);
		}

		GlbCompany Company;
		GlbBranch Branch;
		OrgHeader CompanyOrgProxy;
		OrgHeader BranchOrgProxy;
		OrgHeader DebtorCC;
		OrgAddress DebtorCC_MainAddress;
		OrgCusCode CusCode_CPF;
		OrgCusCode CusCode_CNPJ;
		AccBankAccount BankAccount;
		AccCollectionBatch CollectionBatch;
		AccCollectionOrder CollectionOrder;
		AccCollectionOrderLine CollectionOrderLine;
		InvoicingBase ARInvoiceInForeignCurrency;
		InvoicingBase ARInvoiceInLocalCurrency;

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
	}
}
