using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class IntercompanyTransactionImportEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(IntercompanyTransactionImportEmail);
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestArgumentExceptionWhenNullPassed()
		{
			IntercompanyTransactionImportEmail testEmail = new IntercompanyTransactionImportEmail(null); // Should throw exception on construction of email with null imported transaction
		}

		public void TestSubject()
		{
			ZString expectedSubject = String.Format("Failed to post {0} {1} {2} ({3} {4}) imported from other Group Company", Ledger, TransactionType, TransactionNumber, CurrencyCode, OverseasTotalAmount.ToString("N2"));
			AssertEquals("Subject of email", expectedSubject, ImportEmail.GetSubject_ForTestOnly());
		}

		public void TestBody()
		{
			ZString expectedBody = String.Format(@"An attempt to post an Intercompany Imported {0} {1} {2} for {3} {4} has failed because of the following validation errors:


Transaction Lines with validation errors:
Line 1
Error - GenericCharge: Please enter a value.
Error - GenericCharge:  At least a Charge Code or GL Header must be entered.
Error - AL_AC:  At least a Charge Code or GL Header must be entered.
Error - AL_AG:  At least a Charge Code or GL Header must be entered.
", Ledger, TransactionType, TransactionNumber, CurrencyCode, OverseasTotalAmount.ToString("N2"));
			AssertEquals("Body of email", expectedBody, ImportEmail.GetBody_ForTestOnly());
		}

		public void TestBaseOnNotImportedTransactionsEmailBodyAndSubject()
		{
			var company = TestObjectCreator.CreateNewCompany("TSC");
			var branch = TestObjectCreator.CreateNewBranch(company, "TSB");
			var errorMessage = "Intercompany Invoice cannot be auto-imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy. Please try to use manual import.";
			var transactionToImport = TestObjectCreator.CreateARInvoice<ARInvoice>(TransactionNumber, TestObjectCreator.LocalCurrency, 1m, TestObjectCreator.Agent);
			transactionToImport.AH_GC = company.PK;
			transactionToImport.AH_GB = branch.PK;
			transactionToImport.AH_GE = TestObjectCreator.FESDepartment.PK;
			transactionToImport.AH_RX_NKTransactionCurrency = "AUD";
			var email = new IntercompanyTransactionImportEmail(transactionToImport, errorMessage);
			var expectedBody = @"An attempt to import an invoice with organization ZAgent in company [TSC] branch [TSB] department [FES] has failed because of the following error:
Intercompany Invoice cannot be auto-imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy. Please try to use manual import.";
			AssertEquals("Body of email", expectedBody, email.GetBody_ForTestOnly());
			var expectSubject = "Failed to auto-import AR INV 00001234 (AUD 0.00) from other Group Company";
			AssertEquals("Subject of email", expectSubject, email.GetSubject_ForTestOnly());
		}

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public override void TestSend()
		{
			Guid groupPk = StaffGroupPK.ToGuid();
			Factory.Save();
			AccountingConfigurationRegistry.Instance.IntercompanyTransactionsImportNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			ImportEmail.Send();
			AssertEquals("Shouldn't have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AccountingConfigurationRegistry.Instance.IntercompanyTransactionsImportNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupPk); // to check empty handling
			ImportEmail.Send();
			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public override void TestRenderAndSend()
			=> base.TestRenderAndSend();

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			SetupStaffMemberEmailAddress();
			ImportedTransaction = TestObjectCreator.CreateAPInvoice<APInvoice>(TransactionNumber, RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCode), 1m, OverseasTotalAmount, 0m, 0m, OverseasTotalAmount, 0m, 0m, false);
			ImportedTransaction.Lines[0].AL_AC = new ZGuid();
			ImportEmail = new IntercompanyTransactionImportEmail(ImportedTransaction);
		}

		protected override void TearDown()
		{
			base.TearDown();
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		TestObjectCreator TestObjectCreator;
		IntercompanyTransactionImportEmail ImportEmail;
		InvoicingBase ImportedTransaction;
		protected ZString TransactionType
		{
			get
			{
				return "INV";
			}
		}

		protected ZString TransactionNumber
		{
			get
			{
				return "00001234";
			}
		}

		protected ZString Ledger
		{
			get
			{
				return "AP";
			}
		}

		protected ZString CurrencyCode
		{
			get
			{
				return "AUD";
			}
		}

		protected ZDecimal OverseasTotalAmount
		{
			get
			{
				return 2250.00m;
			}
		}

		protected ZDateTime PostDate
		{
			get
			{
				return ZDateTime.Now;
			}
		}

		protected ZDateTime TransactionDate
		{
			get
			{
				return ZDateTime.Now;
			}
		}

		protected ZGuid StaffGroupPK
		{
			get
			{
				GlbGroup group = Factory.New<GlbGroup>();
				group.Staff.Add(Factory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK)));
				return group.PK;
			}
		}

		protected ZString EmailAddress
		{
			get
			{
				return new ZString("blahblah@whatever.example");
			}
		}

		protected void SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK));
			currentStaffMember.GS_EmailAddress = EmailAddress;
			staffMemberFactory.Save();
		}
	}
}
