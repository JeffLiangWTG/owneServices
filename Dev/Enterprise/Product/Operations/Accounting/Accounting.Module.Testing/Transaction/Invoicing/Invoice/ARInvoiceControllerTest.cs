using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARInvoiceController))]
	public class ARInvoiceControllerTest : CreditNoteInvoiceControllerTestCase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARInvoice;
		}

		protected override Type GetExpectedBusinessObjectType()
		{
			return typeof(ARInvoice);
		}

		protected override Type GetExpectedFormType()
		{
			return typeof(InvoiceForm);
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesInvoice; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewReceivablesInvoice; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}

		protected override void AssertSecurityOverrideProviderTypes(InvoicingBase businessObject)
		{
			AssertEquals(typeof(ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider), SecurityOverrideProviderSource.Get(businessObject.OriginalTransaction).Provider.GetType());
			AssertEquals(typeof(ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider), SecurityOverrideProviderSource.Get(businessObject).Provider.GetType());
		}

		public void TestTemplateCopyFormIsInEditMode()
		{
			TestTemplateCopyFormIsInEditModeCore();
		}

		public void TestSecurityOverrideReverseReceivablesInvoiceWhenAPTransactionsArePaid_Allowed()
		{
			TestSecurityOverrideReverseReceivablesInvoiceWhenAPTransactionsArePaid<APInvoice>(true, true, () => AssertNull("Should NOT show login form", ZFormModaliser.LastFormShownDialogForTest));
		}

		public void TestSecurityOverrideReverseReceivablesInvoiceWhenAPTransactionsArePaid_Denied()
		{
			TestSecurityOverrideReverseReceivablesInvoiceWhenAPTransactionsArePaid<APInvoice>(true, false, () => AssertEquals("Should show login form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType()));
		}

		public void TestSecurityOverrideReverseReceivablesInvoiceWhenAPTransactionsArePaid_DeniedButNotPaid()
		{
			TestSecurityOverrideReverseReceivablesInvoiceWhenAPTransactionsArePaid<APInvoice>(false, false, () => AssertNull("Should NOT show login form", ZFormModaliser.LastFormShownDialogForTest));
		}

		public void TestSecurityOverrideReverseReceivablesInvoiceWhenAPTransactionsArePaid_DeniedPaidButNotAnInvoice()
		{
			TestSecurityOverrideReverseReceivablesInvoiceWhenAPTransactionsArePaid<APCreditNote>(true, false, () => AssertNull("Should NOT show login form", ZFormModaliser.LastFormShownDialogForTest));
		}

		public override void TestGetCheckPointForDelete()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_TransactionCategory = "FIN";
			Assert("Precondition: Standard AR Invoice", !invoice.IsSelfBillingInvoice);
			AssertEquals("Should be ReverseReceivablesInvoice", Env.Security.ReverseReceivablesInvoice, Controller.GetCheckPointForDelete(invoice));

			invoice.AH_TransactionCategory = "SBR";
			Assert("Precondition: Self Billing AR Invoice", invoice.IsSelfBillingInvoice);
			AssertEquals("Should be ReverseReceivablesSelfBilledInvoice", Env.Security.ReverseReceivablesSelfBilledInvoice, Controller.GetCheckPointForDelete(invoice));

			AssertEquals("No CheckPoint for multiple reversing bizo", Env.Security.None, Controller.GetCheckPointForDelete(new MultipleReversingProviderForHeader()));
		}

		public void TestSecurityOverrideReverseReceivablesInvoiceForMultipleReversing()
		{
			var arInvoice1 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);
			var arInvoice2 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);

			Assert("Precondition: Standard Invoice", !arInvoice1.IsSelfBillingInvoice);
			Assert("Precondition: Standard Invoice", !arInvoice2.IsSelfBillingInvoice);

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;
			Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.IsAllowed = true;
			ZFormModaliser.LastFormShownDialogForTest = null;

			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice1);
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice2);
			AssertNull("Precondition: ", multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate);
			AssertNull("Precondition: ", multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertNull("Precondition: ", multipleReversingProvider.GetSecurityOverrideProvider(null));

			Enterprise.Security.Testing.SecurityTestObject.CreateTestUser(true, Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.Code, "US1", "User1", "pass");

			int loginFormShownCount = 0;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var loginForm = form as LoginForm;
				if (loginForm != null)
				{
					loginFormShownCount++;

					loginForm.DoLoginForTest("User1", "pass");
					ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;
				}
			});
			try
			{
				foreach (BusinessObject bizo in multipleReversingProvider)
				{
					AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				}
				AssertEquals("A Login Form should be shown once", 1, loginFormShownCount);
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}

			AssertNotNull("MultipleReversingProvider.PaidRelatedInvoicesSecurityCertificate must be set to a value.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate);
			AssertNull("MultipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate must not be set as all transaction are standard invoice", multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertNotNull("MultipleReversingProvider.SecurityOverrideProviderForARInvoices must be set to a value.",
				multipleReversingProvider.GetSecurityOverrideProvider(null));
			AssertEquals(typeof(ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider), multipleReversingProvider.GetSecurityOverrideProvider(null).GetType());

			var arInvoiceInLocalFactory1 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice1.PK);
			var arInvoiceInLocalFactory2 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice2.PK);
			AssertEquals("Invoice1 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory1.SecurityOverrideProvider);
			AssertEquals("Invoice2 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory2.SecurityOverrideProvider);

			AssertEquals("Invoice1 PaidRelatedInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate, arInvoiceInLocalFactory1.PaidRelatedInvoicesSecurityCertificate);
			AssertNull("Invoice1 PaidRelatedSelfBilledInvoicesSecurityCertificate must not be set as it is standard invoice", arInvoiceInLocalFactory1.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertEquals("Invoice2 PaidRelatedInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate, arInvoiceInLocalFactory2.PaidRelatedInvoicesSecurityCertificate);
			AssertNull("Invoice2 PaidRelatedSelfBilledInvoicesSecurityCertificate must not be set as it is standard invoice", arInvoiceInLocalFactory2.PaidRelatedSelfBilledInvoicesSecurityCertificate);

			AssertEquals("Should show login form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestSecurityOverrideReverseReceivablesSelfBilliedInvoiceForMultipleReversing()
		{
			var arInvoice1 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);
			arInvoice1.AH_TransactionCategory = "SBR";
			var arInvoice2 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);
			arInvoice2.AH_TransactionCategory = "SBR";
			Factory.Save();

			Assert("Precondition: Self-Billed Invoice", arInvoice1.IsSelfBillingInvoice);
			Assert("Precondition: Self-Billed Invoice", arInvoice2.IsSelfBillingInvoice);
			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = true;
			Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.IsAllowed = false;
			ZFormModaliser.LastFormShownDialogForTest = null;

			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice1);
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice2);
			AssertNull("Precondition: ", multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate);
			AssertNull("Precondition: ", multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertNull("Precondition: ", multipleReversingProvider.GetSecurityOverrideProvider(null));

			Enterprise.Security.Testing.SecurityTestObject.CreateTestUser(true, Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.Code, "US1", "User1", "pass");

			int loginFormShownCount = 0;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var loginForm = form as LoginForm;
				if (loginForm != null)
				{
					loginFormShownCount++;

					loginForm.DoLoginForTest("User1", "pass");
					ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;
				}
			});
			try
			{
				foreach (BusinessObject bizo in multipleReversingProvider)
				{
					AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				}
				AssertEquals("A Login Form should be shown once", 1, loginFormShownCount);
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}

			AssertNull("MultipleReversingProvider.PaidRelatedInvoicesSecurityCertificate must not be set as all transaction are self billed invoice", multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate);
			AssertNotNull("MultipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate must be set to a value.",
				multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertNotNull("MultipleReversingProvider.SecurityOverrideProviderForARInvoices must be set to a value.",
				multipleReversingProvider.GetSecurityOverrideProvider(null));
			AssertEquals(typeof(ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider), multipleReversingProvider.GetSecurityOverrideProvider(null).GetType());

			var arInvoiceInLocalFactory1 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice1.PK);
			var arInvoiceInLocalFactory2 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice2.PK);
			AssertEquals("Invoice1 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory1.SecurityOverrideProvider);
			AssertEquals("Invoice2 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory2.SecurityOverrideProvider);

			AssertNull("Invoice1 PaidRelatedInvoicesSecurityCertificate must not be set as it is self billed invoice", arInvoiceInLocalFactory1.PaidRelatedInvoicesSecurityCertificate);
			AssertEquals("Invoice1 PaidRelatedSelfBilledInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate, arInvoiceInLocalFactory1.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertNull("Invoice2 PaidRelatedInvoicesSecurityCertificate must not be set as it is self billed invoice", arInvoiceInLocalFactory2.PaidRelatedInvoicesSecurityCertificate);
			AssertEquals("Invoice2 PaidRelatedSelfBilledInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate, arInvoiceInLocalFactory2.PaidRelatedSelfBilledInvoicesSecurityCertificate);

			AssertEquals("Should show login form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestSecurityOverrideReverseARSelfBilledAndStandardInvWhenGrantingUserHasBothSecurityRight()
		{
			var arInvoice1 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);
			arInvoice1.AH_TransactionCategory = "FIN";
			var arInvoice2 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);
			arInvoice2.AH_TransactionCategory = "SBR";
			Factory.Save();

			Assert("Precondition: Standard Invoice", !arInvoice1.IsSelfBillingInvoice);
			Assert("Precondition: Self-Billed Invoice", arInvoice2.IsSelfBillingInvoice);

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;
			Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.IsAllowed = false;
			ZFormModaliser.LastFormShownDialogForTest = null;

			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice1);
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice2);
			AssertNull("Precondition: ", multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate);
			AssertNull("Precondition: ", multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertNull("Precondition: ", multipleReversingProvider.GetSecurityOverrideProvider(null));

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_LoginName = "User1";
			testUser.StaffPlainTextPassword = "pass";
			testUser.GS_ChangePasswordAtNextLogin = false;

			var invoiceSecurity = Factory.New<GlbSecurity>();
			invoiceSecurity.GU_GS = testUser.PK;
			invoiceSecurity.GU_SecurityRight = Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.Code;
			invoiceSecurity.GU_SecurityItemIsAllowed = true;

			var selfBilledInvoiceSecurity = Factory.New<GlbSecurity>();
			selfBilledInvoiceSecurity.GU_GS = testUser.PK;
			selfBilledInvoiceSecurity.GU_SecurityRight = Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.Code;
			selfBilledInvoiceSecurity.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			int loginFormShownCount = 0;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var loginForm = form as LoginForm;
				if (loginForm != null)
				{
					loginFormShownCount++;

					loginForm.DoLoginForTest("User1", "pass");
					ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;
				}
			});
			try
			{
				foreach (BusinessObject bizo in multipleReversingProvider)
				{
					AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				}
				AssertEquals("A Login Form should be shown once as the user1 has both security right", 1, loginFormShownCount);
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}

			AssertNotNull("MultipleReversingProvider.PaidRelatedInvoicesSecurityCertificate must be set to a value.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate);
			AssertNotNull("MultipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate must be set to a value.",
				multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertNotNull("MultipleReversingProvider.SecurityOverrideProviderForARInvoices must be set to a value.",
				multipleReversingProvider.GetSecurityOverrideProvider(null));
			AssertEquals(typeof(ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider), multipleReversingProvider.GetSecurityOverrideProvider(null).GetType());

			var arInvoiceInLocalFactory1 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice1.PK);
			var arInvoiceInLocalFactory2 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice2.PK);
			AssertEquals("Invoice1 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory1.SecurityOverrideProvider);
			AssertEquals("Invoice2 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory2.SecurityOverrideProvider);

			AssertEquals("Invoice1 PaidRelatedInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate, arInvoiceInLocalFactory1.PaidRelatedInvoicesSecurityCertificate);
			AssertNull("Invoice1 PaidRelatedSelfBilledInvoicesSecurityCertificate must not be set as it is standard invoice", arInvoiceInLocalFactory1.PaidRelatedSelfBilledInvoicesSecurityCertificate);

			AssertNull("Invoice2 PaidRelatedInvoicesSecurityCertificate must not be set as it is self billed invoice", arInvoiceInLocalFactory2.PaidRelatedInvoicesSecurityCertificate);
			AssertEquals("Invoice2 PaidRelatedSelfBilledInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate, arInvoiceInLocalFactory2.PaidRelatedSelfBilledInvoicesSecurityCertificate);

			AssertEquals("Should show login form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestSecurityOverrideReverseARSelfBilledAndStandardInvWhenGrantingUserNotHavingBothSecurityRight()
		{
			var arInvoice1 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);
			arInvoice1.AH_TransactionCategory = "FIN";
			var arInvoice2 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);
			arInvoice2.AH_TransactionCategory = "SBR";
			Factory.Save();

			Assert("Precondition: Standard Invoice", !arInvoice1.IsSelfBillingInvoice);
			Assert("Precondition: Self-Billed Invoice", arInvoice2.IsSelfBillingInvoice);

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;
			Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.IsAllowed = false;
			ZFormModaliser.LastFormShownDialogForTest = null;

			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice1);
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice2);
			AssertNull("Precondition: ", multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate);
			AssertNull("Precondition: ", multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertNull("Precondition: ", multipleReversingProvider.GetSecurityOverrideProvider(null));

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_LoginName = "User1";
			testUser.StaffPlainTextPassword = "pass";

			var invoiceSecurity = Factory.New<GlbSecurity>();
			invoiceSecurity.GU_GS = testUser.PK;
			invoiceSecurity.GU_SecurityRight = Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.Code;
			invoiceSecurity.GU_SecurityItemIsAllowed = true;

			var selfBilledInvoiceSecurity = Factory.New<GlbSecurity>();
			selfBilledInvoiceSecurity.GU_GS = testUser.PK;
			selfBilledInvoiceSecurity.GU_SecurityRight = Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.Code;
			selfBilledInvoiceSecurity.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			int loginFormShownCount = 0;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var loginForm = form as LoginForm;
				if (loginForm != null)
				{
					loginFormShownCount++;

					loginForm.DoLoginForTest("User1", "pass");
					ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;
				}
			});
			try
			{
				foreach (BusinessObject bizo in multipleReversingProvider)
				{
					AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				}
				AssertEquals("A Login Form should be shown twice as the user1 have only security right for standard invoice", 2, loginFormShownCount);
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}

			AssertNotNull("MultipleReversingProvider.PaidRelatedInvoicesSecurityCertificate must be set to a value.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate);
			AssertNotNull("MultipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate must be set to a value.",
				multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate);
			AssertNotNull("MultipleReversingProvider.SecurityOverrideProviderForARInvoices must be set to a value.",
				multipleReversingProvider.GetSecurityOverrideProvider(null));
			AssertEquals(typeof(ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider), multipleReversingProvider.GetSecurityOverrideProvider(null).GetType());

			var arInvoiceInLocalFactory1 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice1.PK);
			var arInvoiceInLocalFactory2 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice2.PK);
			AssertEquals("Invoice1 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory1.SecurityOverrideProvider);
			AssertEquals("Invoice2 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory2.SecurityOverrideProvider);

			AssertEquals("Invoice1 PaidRelatedInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate, arInvoiceInLocalFactory1.PaidRelatedInvoicesSecurityCertificate);
			AssertNull("Invoice1 PaidRelatedSelfBilledInvoicesSecurityCertificate must not be set as it is standard invoice", arInvoiceInLocalFactory1.PaidRelatedSelfBilledInvoicesSecurityCertificate);

			AssertNull("Invoice2 PaidRelatedInvoicesSecurityCertificate must not be set as it is self billed invoice", arInvoiceInLocalFactory2.PaidRelatedInvoicesSecurityCertificate);
			AssertEquals("Invoice2 PaidRelatedSelfBilledInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate, arInvoiceInLocalFactory2.PaidRelatedSelfBilledInvoicesSecurityCertificate);

			AssertEquals("Should show login form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestPaidRelatedInvoicesSecurityProviderIsSharedForMultipleReversing()
		{
			ARInvoice arInvoice1 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);
			ARInvoice arInvoice2 = CreateARInvoiceWhenAPTransactionsArePaid<APInvoice>(true);

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;
			ZFormModaliser.LastFormShownDialogForTest = null;

			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice1);
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice2);

			int loginFormShownCount = 0;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (form is LoginForm)
				{
					loginFormShownCount++;
				}
			});
			try
			{
				foreach (BusinessObject bizo in multipleReversingProvider)
				{
					AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				}
				AssertEquals("A Login Form should be shown once", 1, loginFormShownCount);
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}

			AssertNotNull("MultipleReversingProvider.PaidRelatedInvoicesSecurityCertificate must be set to a value.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate);
			AssertNotNull("MultipleReversingProvider.SecurityOverrideProviderForARInvoices must be set to a value.",
				multipleReversingProvider.GetSecurityOverrideProvider(null));
			AssertEquals(typeof(ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider), multipleReversingProvider.GetSecurityOverrideProvider(null).GetType());

			ARInvoice arInvoiceInLocalFactory1 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice1.PK);
			ARInvoice arInvoiceInLocalFactory2 = AccountingTransactionController.Factory.Load<ARInvoice>(arInvoice2.PK);
			AssertEquals("Invoice1 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory1.SecurityOverrideProvider);
			AssertEquals("Invoice2 SecurityOverrideProviderForARInvoices must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.GetSecurityOverrideProvider(null), arInvoiceInLocalFactory2.SecurityOverrideProvider);
			AssertEquals("Invoice1 PaidRelatedInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate, arInvoiceInLocalFactory1.PaidRelatedInvoicesSecurityCertificate);
			AssertEquals("Invoice2 PaidRelatedInvoicesSecurityCertificate must be set to a value from MultipleReversingProvider to avoid multiple show login form.",
				multipleReversingProvider.PaidRelatedInvoicesSecurityCertificate, arInvoiceInLocalFactory2.PaidRelatedInvoicesSecurityCertificate);

			AssertEquals("Should show login form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		[TestDate(2015, 1, 1)]
		public void TestApprovingDetailsAreAddedToReverseCreditNote()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
			Factory.Save();

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
			AssertEquals(0, approvalRequestFortransaction.Length);

			SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "US1", "User1", "pass");
			transaction.ApprovalDate = ZDateTime.Now.AddDays(1);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				SecurityOverrideProviderSource.Get(transaction).Provider = new InvoicingSecurityOverrideProvider();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is LoginForm loginForm)
					{
						loginForm.DoLoginForTest("User1", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});
				using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
				{
					AssertNotNull(reverseForm);
					reverseForm.FireSaveButton();
				}
				var newFactory = new BusinessObjectFactory();
				var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
				Assert(transactionInNewFactory.IsReversed);

				var creditNotes = Factory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_SystemCreateTimeUtc, new DateTime(2015, 1, 1)));
				var reversedCreditNote = creditNotes.First(x => x.IsReversalTransaction);
				var reversedNoteATHLogs = reversedCreditNote.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == Events.Authorised.Code).Cast<StmALog>();
				AssertEquals("Should contain authorisation log when creating a reverse credit note", 1, reversedNoteATHLogs.Count());
				AssertContains("Should contain log for User1", "User1", reversedNoteATHLogs.First().SL_Reference);
				AssertEquals("Approval should carry over", new ZDateTime(2015, 1, 2), reversedNoteATHLogs.First().SL_EventTime);
			}
		}

		protected override CreditNoteInvoiceController GetController()
		{
			return new ARInvoiceController();
		}

		protected override bool IsNewAllowed
		{
			get { return Env.Security.NewReceivablesInvoice.IsAllowed; }
			set { Env.Security.NewReceivablesInvoice.IsAllowed = value; }
		}

		#region Implementation

		void TestSecurityOverrideReverseReceivablesInvoiceWhenAPTransactionsArePaid<T>(bool isPaid, bool isAllowed, AnonymousMethod assertionCode) where T : InvoicingBase
		{
			ARInvoice arInvoice = CreateARInvoiceWhenAPTransactionsArePaid<T>(isPaid);

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = isAllowed;
			ZFormModaliser.LastFormShownDialogForTest = null;
			using (Controller.ShowDeleteForm(arInvoice))
			{
				assertionCode();
			}
		}

		ARInvoice CreateARInvoiceWhenAPTransactionsArePaid<T>(bool isPaid) where T : InvoicingBase
		{
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			T relatedAP = Factory.NewWithValidTestData<T>();
			TestObjectCreator.AALSHI.CompanyData.SetARTaxApplicable(false);
			TestObjectCreator.AALSHI.CompanyData.SetAPTaxApplicable(false);
			arInvoice.AH_OH = relatedAP.AH_OH = TestObjectCreator.AALSHI.PK;
			InvoicingLineBase arLine = (InvoicingLineBase)arInvoice.Lines.AddNew();
			InvoicingLineBase apLine = (InvoicingLineBase)relatedAP.Lines.AddNew();
			arLine.AL_AC = apLine.AL_AC = TestObjectCreator.CC1.PK;
			arLine.AL_JH = apLine.AL_JH = TestObjectCreator.Job1.PK;
			arLine.AL_GB = apLine.AL_GB = GlbBranch.CurrentBranch.PK;
			arLine.AL_GE = apLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			arLine.AL_OSExTaxAmount = 5m;
			apLine.AL_OSExTaxAmount = 5m;

			//arLine.AL_LineAmount = 
			//    arLine.AL_OverseasTotal = 
			//    arLine.AL_OSAmount = 
			//    arLine.AL_OSExTaxAmount = 
			//    apLine.AL_OverseasTotal = 
			//    apLine.AL_LineAmount = 
			//    apLine.AL_OSAmount = 
			//    apLine.AL_OSExTaxAmount = 5m;

			arInvoice.AH_OutstandingAmount = arInvoice.AH_InvoiceAmount + arInvoice.AH_GSTAmount;
			relatedAP.AH_OutstandingAmount = relatedAP.AH_InvoiceAmount + relatedAP.AH_GSTAmount;
			arInvoice.AH_FullyPaidDate = ZDateTime.Empty;
			relatedAP.AH_FullyPaidDate = ZDateTime.Empty;
			AssertEquals(arInvoice.AH_OSTotal, arInvoice.AH_InvoiceAmount + arInvoice.AH_GSTAmount);
			AssertEquals(relatedAP.AH_OSTotal, relatedAP.AH_InvoiceAmount + relatedAP.AH_GSTAmount);
			TestObjectCreator.CreateJobCharge(arLine, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(apLine, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			if (isPaid)
			{
				APMatchingBase matchingBase = new APMatchingBase(Factory) { PrimaryOrganization = TestObjectCreator.AALSHI.PK };
				matchingBase.MatchedTransactions.Add(relatedAP);
				TransactionHeader discount = matchingBase.GetMiscellaneousTransaction(TransactionTypes.Discount);
				matchingBase.MatchedTransactions.Add(discount);
				((IMatching)relatedAP).OSPartialPaymentAmount = relatedAP.AH_OutstandingAmount;
				discount.BindableOSAmount = -relatedAP.AH_OutstandingAmount;
				matchingBase.MatchAndClearTransactions();
			}

			Factory.Save();

			AssertCollectionContains("AP invoice should become related to the AR Invoice", relatedAP, arInvoice.RelatedInvoices);

			return arInvoice;
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion

	}
}
