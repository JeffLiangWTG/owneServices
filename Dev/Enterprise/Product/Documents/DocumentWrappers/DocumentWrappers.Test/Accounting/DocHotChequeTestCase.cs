using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocHotCheque))]
	sealed class DocHotChequeTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { ChequeWrapper };
		}

		public void TestSetTemplateConstants()
		{
			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.FirstLinePaymentWidth, 9);

			ChequeWrapper.SetTemplateConstants(constants);
			AssertEquals("FirstLinePaymentWidth", 9, ChequeWrapper.FirstLinePaymentWidth);
		}

		public void TestCheque()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Cheque.AQ_Amount = 2345.67M;
			ChequeWrapper = DocHotCheque.New(Cheque, Factory);
			AssertNotNull(ChequeWrapper.Cheque);
			AssertEquals("TWO THOUSAND, THREE HUNDRED AND FORTY FIVE DOLLARS AND 67 CENTS", ChequeWrapper.Cheque.ChequeAmountInWords);
		}

		public void TestTransactionType()
		{
			AssertEquals("HCQ", ChequeWrapper.TransactionType);
		}

		public void TestChequePayTo()
		{
			Cheque.AQ_ChequePayee = "Me";
			AssertEquals("Me", ChequeWrapper.Cheque.ChequePayTo);
		}

		public void TestChequeAmount()
		{
			Cheque.AQ_Amount = 100M;
			AssertEquals(100M, ChequeWrapper.Cheque.ChequeAmount);
		}

		public void TestChequeDate()
		{
			Cheque.AQ_ChequeDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, ChequeWrapper.ChequeDate);
		}

		public void TestChequeNumber()
		{
			Cheque.AQ_ChequeNumber = "12345";
			AssertEquals("12345", ChequeWrapper.ChequeNumber);
		}

		public void TestChequePayee()
		{
			Cheque.AQ_ChequePayee = "Mike";
			AssertEquals("Mike", ChequeWrapper.ChequePayee);
		}

		public void TestAmount()
		{
			AssertEquals(0M, ChequeWrapper.Amount);
			Cheque.AQ_Amount = 100M;
			AssertEquals(100M, ChequeWrapper.Amount);
		}
		public void TestCancelled()
		{
			AssertEquals(ZBool.False, ChequeWrapper.Cancelled);
			Cheque.AQ_Cancelled = ZBool.True;
			Assert(ChequeWrapper.Cancelled);
		}

		public void TestDescription()
		{
			Cheque.AQ_Description = "No description";
			AssertEquals("No description", ChequeWrapper.Description);
		}

		public void TestStaff()
		{
			AssertNull(ChequeWrapper.Staff);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Mikemike";
			Cheque.AQ_GS_NKResponsibleStaff = staff.GS_Code;
			AssertEquals("Mikemike", ChequeWrapper.Staff.FullName);
		}

		public void TestHouseBill()
		{
			Cheque.AQ_HouseBill = "HBL123";
			AssertEquals("HBL123", ChequeWrapper.HouseBill);
		}

		public void TestJob()
		{
			AssertNull(ChequeWrapper.JobHeader);
			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_JobNum = "JHJOBNUM";
			Cheque.AQ_JH = header.PK;
			AssertEquals("JHJOBNUM", ChequeWrapper.JobHeader.JobNumber);
		}

		public void TestMasterBill()
		{
			Cheque.AQ_MasterBill = "MASTER123";
			AssertEquals("MASTER123", ChequeWrapper.MasterBill);
		}

		public void TestOrganisation()
		{
			AssertNull(ChequeWrapper.Organisation);
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "FULLNAME";
			Cheque.AQ_OH = org.PK;
			AssertEquals("FULLNAME", ChequeWrapper.Organisation.Name);
		}

		public void TestRemittanceAdviceContact()
		{
			AssertEquals("Should be empty", ZString.Empty, ChequeWrapper.RemittanceAdviceContact);
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "FULLNAME";
			OrgAddress address = org.Addresses.AddNew();
			address.OA_City = "Chicago";
			address.OA_PostCode = "123456";
			address.OA_State = "IL";
			Cheque.AQ_OH = org.PK;
			AssertEquals("RemittanceAdviceContact should return Organisation.PostalAddress", ChequeWrapper.Organisation.PostalAddress, ChequeWrapper.RemittanceAdviceContact);
		}

		public void TestChequePayToWithAddress()
		{
			AssertEquals("", ChequeWrapper.RemittanceAdviceContact);

			var aPOrg = Factory.New<OrgHeader>();
			aPOrg.OH_Code = "AP_ORG TEST";
			aPOrg.MainAddress.OA_Address1 = "AP_ORG Address";
			aPOrg.OH_FullName = "AP ORG";

			var contact = aPOrg.Contacts.AddNew();
			contact.OC_ContactName = "Jamie";
			OrgDocument doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Payables.Code;

			Cheque.AQ_OH = aPOrg.PK;
			Factory.Save();

			Assert(!ChequeWrapper.RemittanceAdviceContact.Contains("JAMIE"));

			OrgAddress payablesAddress = aPOrg.Addresses.AddNew();
			payablesAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			payablesAddress.OA_OH = aPOrg.PK;
			payablesAddress.OA_Address1 = "AP_ORG Payables Address";
			Factory.Save();

			Assert(!ChequeWrapper.RemittanceAdviceContact.Contains("JAMIE"));
			AssertEquals(ChequeWrapper.ChequePayToWithAddress, "AP ORG\nAP_ORG PAYABLES ADDRESS");
		}

		public void TestTransactionNumber()
		{
			AssertEquals("TransactionNumber", ZString.Empty, ChequeWrapper.TransactionNumber);
			TransactionHeader transactionHeader = Factory.New<APPayment>();
			transactionHeader.AH_TransactionNum = "1234";
			Cheque.AQ_AH = transactionHeader.PK;
			AssertEquals("TransactionNumber", "1234", ChequeWrapper.TransactionNumber);
		}

		public void TestBankAccount()
		{
			AccBankAccount bank = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			AccChequeBook chequeBook = TestObjectCreator.CreateChequeBook("Test Chequebook", 1000, bank);
			Factory.Save();

			Cheque.AQ_AK = ZGuid.Empty;
			AssertNull("BankAccount", ChequeWrapper.BankAccount);

			Cheque.AQ_AK = chequeBook.PK;
			AssertNotNull("BankAccount", ChequeWrapper.BankAccount.PK);
			AssertEquals("BankAccount", "Test Bank", ChequeWrapper.BankAccount.BankName);
		}

		public void TestMICRNumber()
		{
			AccBankAccount bank = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			bank.AB_BSB = "123456789-";
			bank.AB_AccountNum = "010203040506-";
			AccChequeBook chequeBook = TestObjectCreator.CreateChequeBook("Test Chequebook", 1000, bank);
			Cheque.AQ_AK = chequeBook.PK;
			Cheque.AQ_ChequeNumber = "1357924680-";
			Factory.Save();

			AssertEquals("MICRNumber", "C1357924680DC A123456789DA 010203040506DC", ChequeWrapper.MICRNumber);
		}

		public void TestRoutingTransitNumber()
		{
			TransactionHeader transactionHeader = Factory.New<APPayment>();
			transactionHeader.AH_AB = ZGuid.Empty;
			Cheque.AQ_AH = transactionHeader.PK;
			Cheque.AQ_ChequeNumber = "1357924680";
			AssertNull("BankAccount", transactionHeader.BankAccount);
			AssertEquals("RoutingTransitNumber", ZString.Empty, ChequeWrapper.RoutingTransitNumber);

			var bank = Factory.New<AccBankAccount>();
			bank.AB_BankName = "Test Bank Account";
			bank.AB_BSB = "123456";
			bank.AB_BankAddress = "123 Address Test";
			bank.AB_GC = GlbCompany.CurrentCompany.PK;
			bank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_AB = bank.PK;

			AccChequeBook chequeBook = TestObjectCreator.CreateChequeBook("Test ChequeBook", 1000, bank);
			Cheque.AQ_AK = chequeBook.PK;

			AssertEquals("RoutingTransitNumber", "123456", ChequeWrapper.RoutingTransitNumber);
		}

		public void TestRoutingTransitNumberFractionForm()
		{
			TransactionHeader transactionHeader = Factory.New<APPayment>();
			transactionHeader.AH_AB = ZGuid.Empty;
			Cheque.AQ_AH = transactionHeader.PK;
			Cheque.AQ_ChequeNumber = "1357924680";
			AssertNull("BankAccount", ChequeWrapper.BankAccount);
			AssertEquals("RoutingTransitNumberFractionForm", "/", ChequeWrapper.RoutingTransitNumberFractionForm);

			var bank = Factory.New<AccBankAccount>();
			bank.AB_BankName = "Test Bank Account";
			bank.AB_BSB = "123456789";
			bank.AB_BankAddress = "123 Address Test";
			bank.AB_GC = GlbCompany.CurrentCompany.PK;
			bank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_AB = bank.PK;
			AccChequeBook chequeBook = TestObjectCreator.CreateChequeBook("Test ChequeBook", 1000, bank);
			Cheque.AQ_AK = chequeBook.PK;
			AssertEquals("RoutingTransitNumberFractionForm", "5678/1234", ChequeWrapper.RoutingTransitNumberFractionForm);
			bank.AB_BSB = "123456";
			AssertEquals("RoutingTransitNumberFractionForm", "56/1234", ChequeWrapper.RoutingTransitNumberFractionForm);
			bank.AB_BSB = "123";
			AssertEquals("RoutingTransitNumberFractionForm", "/123", ChequeWrapper.RoutingTransitNumberFractionForm);
		}

		AccHotCheque Cheque;
		DocHotCheque ChequeWrapper;
		protected override void SetUp()
		{
			Cheque = Factory.New<AccHotCheque>();
			ChequeWrapper = DocHotCheque.New(Cheque, Factory);
			AssertNotNull("Created ChequeWrapper should not be null", ChequeWrapper);
			base.SetUp();
		}
	}
}
