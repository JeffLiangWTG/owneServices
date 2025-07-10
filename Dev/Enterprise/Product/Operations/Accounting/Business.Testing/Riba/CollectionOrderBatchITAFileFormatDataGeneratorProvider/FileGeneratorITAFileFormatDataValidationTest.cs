using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	class FileGeneratorITAFileFormatDataValidationTest : TestCaseWithFactory
	{
		public void TestExceptionWhenCollectionBatchOrBankAccountOrCompanyIsNull()
		{
			var datetime = ZDateTime.Now;
			var collectionBatch = CreateCollectionBatch(BankAccount);
			AssertNotNull("Precondition:", collectionBatch.BankAccount);
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: glbCompany", () => new FileGeneratorITAFileFormatDataValidation(collectionBatch, null, false, datetime));
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: glbCompany", () => new FileGeneratorITAFileFormatDataValidation(collectionBatch, null, true, datetime));

			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: accCollectionBatch", () => new FileGeneratorITAFileFormatDataValidation(null, Company, false, datetime));
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: accCollectionBatch", () => new FileGeneratorITAFileFormatDataValidation(null, Company, true, datetime));

			AssertNoExceptionThrown(() => new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, false, datetime));
			AssertNoExceptionThrown(() => new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, true, datetime));

			collectionBatch.ACB_AB = ZGuid.Empty;
			AssertNull("Precondition:", collectionBatch.BankAccount);
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: BankAccount", () => new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, false, datetime));
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: BankAccount", () => new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, true, datetime));

			collectionBatch.ACB_AB = BankAccount.PK;
			AssertNoExceptionThrown(() => new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, false, datetime));
			AssertNoExceptionThrown(() => new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, true, datetime));
		}

		public void TestValidationMessage_WhenBankAccountBranchIsNull()
		{
			var expectedMessage = "Your Branch or Login Company does not have a BR CJN, please record a BR CJN in the corresponding Organization Proxy.";

			var collectionBatch = CreateCollectionBatch(BankAccount);
			BankAccount.AB_GB = ZGuid.Empty;
			AssertNull("Precondition:", BankAccount.Branch);

			ICollectionBatchValidation collectionBatchValidation = new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, true, ZDateTime.Now);
			AssertNull("Precondition:", Company.OrgProxy);
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			Company.GC_OH_OrgProxy = CompanyOrgProxy.PK;
			AssertNotNull("Precondition:", Company.OrgProxy);
			Assert("Precondition:", !Company.OrgProxy.CustomsCodes.Any());
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			Company.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CMT, "BR1111", Core.Constants.CountryCodes.Brazil);
			Assert("Precondition:", Company.OrgProxy.CustomsCodes.Any());
			AssertNull("Precondition:", Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			Company.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR2222", Core.Constants.CountryCodes.Italy);
			AssertNull("Precondition:", Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			Company.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR3333", Core.Constants.CountryCodes.Brazil);
			AssertNotNull("Precondition:", Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertNotEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());
		}

		public void TestValidationMessage_WhenBankAccountBranchIsNotNull()
		{
			var expectedMessage = "Your Branch or Login Company does not have a BR CJN, please record a BR CJN in the corresponding Organization Proxy.";

			var collectionBatch = CreateCollectionBatch(BankAccount);
			ICollectionBatchValidation collectionBatchValidation = new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, true, ZDateTime.Now);

			BankAccount.AB_GB = Branch.PK;
			AssertNotNull("Precondition: Branch", BankAccount.Branch);
			AssertNull("Precondition: Branch.OrgProxy", BankAccount.Branch.OrgProxy);
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			Branch.GB_OH_OrgProxy = BranchOrgProxy.PK;
			AssertNotNull("Precondition: Branch.OrgProxy", BankAccount.Branch.OrgProxy);
			Assert("Precondition: OrgProxy.CustomsCodes", !Branch.OrgProxy.CustomsCodes.Any());
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			Branch.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CMT, "BR1111", Core.Constants.CountryCodes.Brazil);
			Assert("Precondition: OrgProxy.CustomsCodes", Branch.OrgProxy.CustomsCodes.Any());
			AssertNull("Precondition:", Branch.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			Branch.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR2222", Core.Constants.CountryCodes.Italy);
			AssertNull("Precondition:", Branch.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			Branch.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR3333", Core.Constants.CountryCodes.Brazil);
			AssertNotNull("Precondition:", Branch.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertNotEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());
		}

		public void TestValidationMessage_WhenAH_TransactionNumLengthGreaterThan10Digits_ForCollectionOrderWithSingleTransaction()
		{
			BankAccount.AB_GB = Branch.PK;
			Branch.GB_OH_OrgProxy = BranchOrgProxy.PK;
			Branch.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR0001", Core.Constants.CountryCodes.Brazil);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
			arInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			var collectionBatch = CreateCollectionBatch(BankAccount);
			ICollectionBatchValidation collectionBatchValidation = new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, true, ZDateTime.Now);
			Assert("Precondition: Batch CollectionOrder(s)", !collectionBatch.CollectionOrders.Any());
			AssertEquals(ZString.Empty, collectionBatchValidation.GetPreSaveValidationMessage());

			var collectionOrder = CreateCollectionOrder(collectionBatch);
			Assert("Precondition: Batch CollectionOrder(s)", collectionBatch.CollectionOrders.Any());
			Assert("Precondition: CollectionOrder(s) Line(s)", !collectionOrder.CollectionOrderLines.Any());
			AssertEquals(ZString.Empty, collectionBatchValidation.GetPreSaveValidationMessage());

			TestObjectCreator.CreateCollectionOrderLine(collectionOrder, arInvoice, false);
			Assert("Precondition: CollectionOrder(s) Line(s)", collectionOrder.CollectionOrderLines.Any());
			AssertEquals("CollectionOrderLines count", 1, collectionOrder.CollectionOrderLines.Count);

			arInvoice.AH_TransactionNum = "ARINV12345678";
			var transactionNumberLongerThan10Digits = arInvoice.AH_TransactionNum.Length > 10;
			Assert("Precondition: ", transactionNumberLongerThan10Digits);

			var expectedMessage = "The following selected transactions: 'ARINV12345678' have Transaction Numbers longer than 10 digits, please review your setups to make sure transaction number is limited to 10 digits.";
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			arInvoice.AH_TransactionNum = "AR12345678";
			transactionNumberLongerThan10Digits = arInvoice.AH_TransactionNum.Length > 10;
			Assert("Precondition: ", !transactionNumberLongerThan10Digits);
			AssertNotEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());
		}

		public void TestValidationMessage_WhenAH_TransactionNumLengthGreaterThan10Digits_ForCollectionOrderWithMultipleTransactions()
		{
			BankAccount.AB_GB = Branch.PK;
			Branch.GB_OH_OrgProxy = BranchOrgProxy.PK;
			Branch.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR0001", Core.Constants.CountryCodes.Brazil);

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
			arInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
			arInvoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			var collectionBatch = CreateCollectionBatch(BankAccount);
			ICollectionBatchValidation collectionBatchValidation = new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, true, ZDateTime.Now);
			Assert("Precondition: Batch CollectionOrder(s)", !collectionBatch.CollectionOrders.Any());
			AssertEquals(ZString.Empty, collectionBatchValidation.GetPreSaveValidationMessage());

			var collectionOrder = CreateCollectionOrder(collectionBatch);
			Assert("Precondition: Batch CollectionOrder(s)", collectionBatch.CollectionOrders.Any());
			Assert("Precondition: CollectionOrder(s) Line(s)", !collectionOrder.CollectionOrderLines.Any());
			AssertEquals(ZString.Empty, collectionBatchValidation.GetPreSaveValidationMessage());

			TestObjectCreator.CreateCollectionOrderLine(collectionOrder, arInvoice1, false);
			arInvoice1.AH_TransactionNum = "AR12345678910";
			var transactionNumberLongerThan10Digits = arInvoice1.AH_TransactionNum.Length > 10;
			Assert("Precondition: ", transactionNumberLongerThan10Digits);

			TestObjectCreator.CreateCollectionOrderLine(collectionOrder, arInvoice2, false);
			arInvoice2.AH_TransactionNum = "AR12366666666";
			transactionNumberLongerThan10Digits = arInvoice2.AH_TransactionNum.Length > 10;
			Assert("Precondition: ", transactionNumberLongerThan10Digits);

			AssertEquals("CollectionOrderLines count", 2, collectionOrder.CollectionOrderLines.Count);

			var expectedMessage = "The following selected transactions: 'AR12345678910', 'AR12366666666' have Transaction Numbers longer than 10 digits, please review your setups to make sure transaction number is limited to 10 digits.";
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			arInvoice1.AH_TransactionNum = "AR12345678";
			transactionNumberLongerThan10Digits = arInvoice1.AH_TransactionNum.Length > 10;
			Assert("Precondition: ", !transactionNumberLongerThan10Digits);
			expectedMessage = "The following selected transactions: 'AR12366666666' have Transaction Numbers longer than 10 digits, please review your setups to make sure transaction number is limited to 10 digits.";
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			arInvoice2.AH_TransactionNum = "AR12366666";
			transactionNumberLongerThan10Digits = arInvoice2.AH_TransactionNum.Length > 10;
			Assert("Precondition: ", !transactionNumberLongerThan10Digits);
			AssertNotEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());
		}

		public void TestValidationMessage_WhenReceivablesOrgOfTransactionMissingCNPJOrCPFTypeOrgCusCode_ForOrderWithSingleTransaction()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
			arInvoice.AH_OH = TestObjectCreator.Debtor.PK;
			Factory.Save();

			arInvoice.AH_TransactionNum = "ARINV12345";
			BankAccount.AB_GB = Branch.PK;
			Branch.GB_OH_OrgProxy = BranchOrgProxy.PK;
			Branch.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR0001", Core.Constants.CountryCodes.Brazil);

			var collectionBatch = CreateCollectionBatch(BankAccount);
			ICollectionBatchValidation collectionBatchValidation = new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, true, ZDateTime.Now);

			var collectionOrder = CreateCollectionOrder(collectionBatch);
			Assert("Precondition: Batch CollectionOrder(s)", collectionBatch.CollectionOrders.Any());

			TestObjectCreator.CreateCollectionOrderLine(collectionOrder, arInvoice, false);
			Assert("Precondition: CollectionOrderLine", collectionOrder.CollectionOrderLines.Any());
			AssertEquals("CollectionOrderLines count", 1, collectionOrder.CollectionOrderLines.Count);

			var expectedMessage = "The Receivables Organization of transactions 'ARINV12345' are missing BR CJN or BR CPF, please review.";
			var debtor = arInvoice.Header;
			Assert("Precondition:", !debtor.CustomsCodes.Any());
			debtor.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CMT, "BR1111", Core.Constants.CountryCodes.Brazil);
			Assert("Precondition:", debtor.CustomsCodes.Any());
			AssertNull("Precondition:", debtor.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertNull("Precondition:", debtor.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, Core.Constants.CountryCodes.Brazil));
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			debtor.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "BR2222", Core.Constants.CountryCodes.Brazil);
			AssertNull("Precondition:", debtor.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertNotNull("Precondition:", debtor.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, Core.Constants.CountryCodes.Brazil));
			AssertEquals(ZString.Empty, collectionBatchValidation.GetPreSaveValidationMessage());

			debtor.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR4444", Core.Constants.CountryCodes.Brazil);
			AssertNotNull("Precondition:", debtor.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertEquals(ZString.Empty, collectionBatchValidation.GetPreSaveValidationMessage());
		}

		public void TestValidationMessage_WhenReceivablesOrgOfTransactionMissingCNPJOrCPFTypeOrgCusCode_ForOrderWithMultipleTransactions()
		{
			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
			arInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
			arInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			arInvoice1.AH_TransactionNum = "ARINV4444";
			arInvoice2.AH_TransactionNum = "ARINV8888";

			BankAccount.AB_GB = Branch.PK;
			Branch.GB_OH_OrgProxy = BranchOrgProxy.PK;
			Branch.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR0001", Core.Constants.CountryCodes.Brazil);

			var collectionBatch = CreateCollectionBatch(BankAccount);
			ICollectionBatchValidation collectionBatchValidation = new FileGeneratorITAFileFormatDataValidation(collectionBatch, Company, true, ZDateTime.Now);

			var collectionOrder = CreateCollectionOrder(collectionBatch);
			Assert("Precondition: Batch CollectionOrder(s)", collectionBatch.CollectionOrders.Any());

			TestObjectCreator.CreateCollectionOrderLine(collectionOrder, arInvoice1, false);
			TestObjectCreator.CreateCollectionOrderLine(collectionOrder, arInvoice2, false);
			Assert("Precondition: CollectionOrderLine", collectionOrder.CollectionOrderLines.Any());
			AssertEquals("CollectionOrderLines count", 2, collectionOrder.CollectionOrderLines.Count);

			var expectedMessage = "The Receivables Organization of transactions 'ARINV4444', 'ARINV8888' are missing BR CJN or BR CPF, please review.";
			AssertNull("Precondition:", arInvoice1.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertNull("Precondition:", arInvoice1.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, Core.Constants.CountryCodes.Brazil));
			AssertNull("Precondition:", arInvoice2.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertNull("Precondition:", arInvoice2.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, Core.Constants.CountryCodes.Brazil));
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			arInvoice1.Header.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "BR4444", Core.Constants.CountryCodes.Brazil);
			expectedMessage = "The Receivables Organization of transactions 'ARINV8888' are missing BR CJN or BR CPF, please review.";
			AssertNotNull("Precondition:", arInvoice1.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Core.Constants.CountryCodes.Brazil));
			AssertEquals(expectedMessage, collectionBatchValidation.GetPreSaveValidationMessage());

			arInvoice2.Header.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "BR8888", Core.Constants.CountryCodes.Brazil);
			AssertNotNull("Precondition:", arInvoice2.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, Core.Constants.CountryCodes.Brazil));
			AssertEquals(ZString.Empty, collectionBatchValidation.GetPreSaveValidationMessage());
		}

		AccCollectionBatch CreateCollectionBatch(AccBankAccount bankAccount)
		{
			var batch = TestObjectCreator.CreateCollectionBatch(bankAccount, Company, "B000010", 100, false);
			batch.ACB_CollectionFileFormat = CollectionFileFormatList.Codes.itauBank;
			batch.ACB_AB = bankAccount.PK;

			return batch;
		}

		AccCollectionOrder CreateCollectionOrder(AccCollectionBatch batch)
		{
			var order = TestObjectCreator.CreateCollectionOrder(batch, ZDate.Today, TestObjectCreator.AALSHI, "ORD000010", 100, false);
			order.IncludeInBatch = true;

			return order;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Company = TestObjectCreator.CreateNewCompany("DCC", Core.Constants.CountryCodes.Brazil);
			Branch = TestObjectCreator.CreateNewBranch(Company, "TTT");
			CompanyOrgProxy = TestObjectCreator.CreateOrgHeader("DebtorCOP", false, true);
			BranchOrgProxy = TestObjectCreator.CreateOrgHeader("DebtorBOP", false, true);
			BankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			Factory.Save();
		}

		GlbCompany Company;
		GlbBranch Branch;
		OrgHeader CompanyOrgProxy;
		OrgHeader BranchOrgProxy;
		AccBankAccount BankAccount;

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
	}
}
