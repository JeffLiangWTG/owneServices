using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.Accounting.Business.EmailNotification;

	class Test : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(ZTransactionReversedEmail);
			}
		}

		[ExpectException(typeof(InvalidReverseTransactionException))]
		public void TestArgumentExceptionWhenInvalidIReversingPassed()
		{
			//TestIReverseTransaction InvalidOriginalTransaction = new TestIReverseTransaction();
			//SetupITransaction(InvalidOriginalTransaction, OriginalTransactionTransactionNumber); //Setup the ITransaction, but don't set the reversing transaction
			InvoicingBase originalTransaction = Creator.CreateAPInvoice<APInvoice>("0001", Creator.AUD, 1m, 100m, 0m, 0m, 0m, 0m, 0m);
			IReversing unreversedTransaction = originalTransaction;
			ZTransactionReversedEmail testEmail = new ZTransactionReversedEmail(unreversedTransaction); // Should throw exception on construction of email with invalid reversed transaction
		}

		public void TestSubject()
		{
			ZTransactionReversedEmail reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			ZString expectedSubject = String.Format("{0} {1} {2} ({3}  {4} {5}) has been reversed by {6} {7} {8} ({9} {10} {11})", OriginalTransaction.Ledger, OriginalTransaction.TransactionType, OriginalTransaction.TransactionNumber, OriginalTransaction.TransactionDate.ToShortDateString(), OriginalTransaction.CurrencyCode, OriginalTransaction.OverseasTotalAmount.ToString("N2"), ReverseTransaction.Ledger, ReverseTransaction.TransactionType, ReverseTransaction.TransactionNumber, ReverseTransaction.TransactionDate.ToShortDateString(), ReverseTransaction.CurrencyCode, ReverseTransaction.OverseasTotalAmount.ToString("N2"));
			reversedEmail.Send();
			AssertEquals("Subject of email", expectedSubject, reversedEmail.Subject);
		}

		[ExpectNoExceptions]
		public void TestClass_WhenCurrentStaffIsNull_ShouldNotThrow()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(GlbStaff.CurrentUser);
				ZTransactionReversedEmail reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			}
		}

		public void TestBody()
		{
			ZTransactionReversedEmail reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			ZString expectedBody = String.Format("{0} {1} {2} dated {3} for {4} {5} " + "posted to {6} by {7} ({8}) has been reversed by " + "{9} {10} {11} dated {12} for {13} {14} posted to {15} by {16} ({17})" + System.Environment.NewLine + System.Environment.NewLine + "Reason: {18}", OriginalTransaction.Ledger, OriginalTransaction.TransactionType, OriginalTransaction.TransactionNumber, OriginalTransaction.TransactionDate.ToShortDateString(), OriginalTransaction.CurrencyCode, OriginalTransaction.OverseasTotalAmount.ToString("N2"), OriginalTransaction.PostDate.ToShortDateString(), GlbStaff.CurrentUser.GS_LoginName, GlbStaff.CurrentUser.GS_FullName, ReverseTransaction.Ledger, ReverseTransaction.TransactionType, ReverseTransaction.TransactionNumber, OriginalTransaction.TransactionDate.ToShortDateString(), OriginalTransaction.CurrencyCode, OriginalTransaction.OverseasTotalAmount.ToString("N2"), OriginalTransaction.PostDate.ToShortDateString(), GlbStaff.CurrentUser.GS_LoginName, GlbStaff.CurrentUser.GS_FullName, ReverseTransaction.ReversingReason);
			reversedEmail.Send();
			AssertEquals("Body of email", expectedBody, reversedEmail.Body);
		}

		public override void TestSend()
		{
			Guid groupPk = StaffGroupPK.ToGuid();
			Factory.Save();
			AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			ReversedEmail.Send();
			AssertEquals("Shouldn't have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupPk); // to check empty handling
			ReversedEmail.Send();
			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public override void TestRenderAndSend()
			=> base.TestRenderAndSend();

		public void TestOriginalTransactionAmountDecimalPlaces()
		{
			((InvoicingBase)OriginalTransaction).AH_RX_NKTransactionCurrency = "IRQ";
			ZTransactionReversedEmail reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			reversedEmail.Send();
			AssertContains("Currency Code", "IRQ 0.00 posted", reversedEmail.Body);
			((InvoicingBase)OriginalTransaction).AH_RX_NKTransactionCurrency = "XXX";
			reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			reversedEmail.Send();
			AssertContains("Currency Code", "XXX 0.00 posted", reversedEmail.Body);
			((InvoicingBase)OriginalTransaction).AH_RX_NKTransactionCurrency = "RUR";
			reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			reversedEmail.Send();
			AssertContains("Currency Code", "RUR 0.00 posted", reversedEmail.Body);
			((InvoicingBase)OriginalTransaction).AH_RX_NKTransactionCurrency = "IQD";
			reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			reversedEmail.Send();
			AssertContains("Currency Code", "IQD 0.000 posted", reversedEmail.Body);
		}

		public void TestReverseTransactionAmountDecimalPlaces()
		{
			((InvoicingBase)ReverseTransaction).AH_RX_NKTransactionCurrency = "RUR";
			ZTransactionReversedEmail reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			reversedEmail.Send();
			AssertContains("Currency Code", "RUR 0.00 posted to", reversedEmail.Body);
			((InvoicingBase)ReverseTransaction).AH_RX_NKTransactionCurrency = "IQD";
			reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			reversedEmail.Send();
			AssertContains("Currency Code", "IQD 0.000 posted to", reversedEmail.Body);
			((InvoicingBase)ReverseTransaction).AH_RX_NKTransactionCurrency = "XXX";
			reversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			reversedEmail.Send();
			AssertContains("Currency Code", "XXX 0.00 posted to", reversedEmail.Body);
		}

		public void TestCreator()
		{
			ReversedEmail.Send();
			AssertContains("OriginalTransactionCreator is CurrentUser", "(CargoWise Support) has been reversed", ReversedEmail.Body);
			GlbStaff creator = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			TransactionHeader original = null;
			using (Env.SetTemporaryUserContext(creator.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				original = Factory.NewWithValidTestData<APInvoice>();
				Factory.Save();
			}

			original.GenerateReverseTransaction(false);
			ZTransactionReversedEmail email = new ZTransactionReversedEmail(original);
			email.Send();
			AssertContains("OriginalTransactionCreator", "(" + creator.GS_FullName + ") has been reversed", email.Body);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			InvoicingBase originalTransaction = Creator.CreateAPInvoice<APInvoice>("0001", Creator.AUD, 1m, 100m, 0m, 0m, 0m, 0m, 0m);
			Factory.Save();
			OriginalTransaction = originalTransaction;
			OriginalTransaction.GenerateReverseTransaction(true);
			ReverseTransaction = OriginalTransaction.ReverseTransaction;
			((TransactionHeader)ReverseTransaction).AH_TransactionNum = "REVERSE0001";
			ReversedEmail = new ZTransactionReversedEmail(OriginalTransaction);
			SetupStaffMemberEmailAddress();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		ZTransactionReversedEmail ReversedEmail;
		IReversing OriginalTransaction;
		IReversing ReverseTransaction;
		TestObjectCreator Creator;
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
