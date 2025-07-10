using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APInvoiceController))]
	class APInvoiceControllerTest : CreditNoteInvoiceControllerTestCase
	{
		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReversePayablesInvoice; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewPayablesInvoice; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewPayablesTransaction; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewPayablesTransaction; }
		}

		public void TestPaymentToReverse()
		{
			ZGuid groupGuid = ZGuid.NewZGuid();
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_TransactionBelongsToGroup = groupGuid;
			aPInv.AH_TransactionCount = 1;
			Factory.Save();

			APInvController = Controller as APInvoiceController;
			APInvController.Reversing_ForTestOnly = (APInvoiceReversing)new ReversingFactory().NewReversing(aPInv);

			AssertNull("Reverser should not have PaymentToReverse set", ((APInvoiceReversing)APInvController.Reversing_ForTestOnly).PaymentToReverse);

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_TransactionCount = 2;
			Factory.Save();

			APInvController = GetNewControllerWithReversing(aPInv);
			AssertNull("Reverser should not have PaymentToReverse set", ((APInvoiceReversing)APInvController.Reversing_ForTestOnly).PaymentToReverse);

			aPPay.AH_TransactionBelongsToGroup = groupGuid;
			Factory.Save();

			APInvController = GetNewControllerWithReversing(aPInv);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("PaymentToReverse should be APPay", aPPay.PK, ((APInvoiceReversing)APInvController.Reversing_ForTestOnly).PaymentToReverse.PK);
		}

		public void TestGetCheckPointForNew()
		{
			AssertEquals("Should be NewPayablesCreditNote", ExpectedCheckPointForNew, Controller.GetCheckPointForNew(this.GetFormBusinessEntity()));

			InvoicingBase approval = Factory.NewWithValidTestData<UAInvoice>();
			Factory.Save();
			approval.AH_Ledger = LedgerTypes.AccountsPayable;
			approval.AH_TransactionType = TransactionTypes.Invoice;
			approval = Factory.Load<APInvoice>(approval.PK);

			AssertEquals("Should be None", Env.Security.None, Controller.GetCheckPointForNew(approval));
		}

		public override void TestGetCheckPointForDelete()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_TransactionCategory = "STD";
			Assert("Precondition: Standard AP Invoice", !invoice.IsSelfBillingInvoice);
			AssertEquals("Should be ReversePayablesInvoice", Env.Security.ReversePayablesInvoice, Controller.GetCheckPointForDelete(invoice));

			invoice.AH_TransactionCategory = "SBC";
			Assert("Precondition: Self Billing AP Invoice", invoice.IsSelfBillingInvoice);
			AssertEquals("Should be ReverseSelfBilledPayablesInvoice", Env.Security.ReverseSelfBilledPayablesInvoice, Controller.GetCheckPointForDelete(invoice));

			AssertEquals("No CheckPoint for multiple reversing bizo", Env.Security.None, Controller.GetCheckPointForDelete(new MultipleReversingProviderForHeader()));
		}

		public void TestDefaultChargeCodeBehaviour()
		{
			ZGuid groupGuid = ZGuid.NewZGuid();
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_TransactionBelongsToGroup = groupGuid;
			aPInv.AH_TransactionCount = 1;
			Factory.Save();

			OrgHeader orgWithDefaultCharge = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Margin;
			charge.AC_AG_CostAccount = ObjectCreator.GLHeader1.PK;
			orgWithDefaultCharge.CompanyData.OB_AC_APDefaultChargeCode = charge.PK;
			OrgHeader orgWithoutDefaultCharge = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			APInvController = Controller as APInvoiceController;
			using (var form = (GUI.InvoiceForm)APInvController.ShowNewForm())
			{
				var invoice = (APInvoice)form.BusinessEntity;
				invoice.AH_OH = orgWithDefaultCharge.PK;
				AssertEquals("Creates default charge line", 1, invoice.Lines.Count);
				AssertEquals("Creates default charge code", charge.PK, invoice.Lines[0].AL_AC);
			}
		}

		TestObjectCreator fObjectCreator;
		protected TestObjectCreator ObjectCreator
		{
			get { return fObjectCreator ?? (fObjectCreator = new TestObjectCreator(Factory)); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APInvoice;
		}

		protected override Type GetExpectedBusinessObjectType()
		{
			return typeof(APInvoice);
		}

		protected override Type GetExpectedFormType()
		{
			return typeof(GUI.InvoiceForm);
		}

		APInvoiceController GetNewControllerWithReversing(IReversing transaction)
		{
			APInvoiceController aPInvController = (APInvoiceController)ZControllerFactory.Create(ControllerIDs.APInvoice);
			aPInvController.Reversing_ForTestOnly = (APInvoiceReversing)new ReversingFactory().NewReversing(transaction);
			return aPInvController;
		}

		protected override InvoicingBase ChangeStateOfParentTransactionHeaderRow()
		{
			var transaction = Factory.NewWithValidTestData<APInvoice>();
			transaction.SaveAsIncomplete();
			return transaction;
		}

		protected override string MessageToShowWhenControllerMismatchForBusinessEntity => "The AP invoice cannot be displayed because another user has changed or deleted the record. Closing and re-opening this window will refresh your data.";

		APInvoiceController APInvController;

		#region INavigationControllerIDProvider

		public void TestGetValidControllerID()
		{
			var controller = Controller as INavigationControllerIDProvider;

			var nonRelatedBizO = Factory.NewWithValidTestData<ARInvoice>();
			var creditNote = Factory.NewWithValidTestData<APCreditNote>();
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var incompleteInvoice = Factory.NewWithValidTestData<APInvoice>();
			incompleteInvoice.SaveAsIncomplete();
			var invoicePending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoicePending.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;

			AssertEquals(string.Format("ControllerID for non-related bizO should be {0}.", Controller.ID), Controller.ID, controller.GetValidControllerID(nonRelatedBizO));
			AssertEquals("ControllerID for AP Credit Note bizO should be APCreditNote.", ControllerIDs.APCreditNote, controller.GetValidControllerID(creditNote));
			AssertEquals("ControllerID for AP Invoice bizO should be APInvoice.", ControllerIDs.APInvoice, controller.GetValidControllerID(invoice));
			AssertEquals("ControllerID for AP Incomplete Invoice bizO should be APIncompleteInvoice.", ControllerIDs.APIncompleteInvoice, controller.GetValidControllerID(incompleteInvoice));
			AssertEquals("ControllerID for Transaction Pending Allocation bizO should be TransactionsPendingAllocation.", ControllerIDs.TransactionsPendingAllocation, controller.GetValidControllerID(invoicePending));
		}

		public virtual void TestGetFormWithAPIncompletInvoice()
		{
			var dataSource = Factory.NewWithValidTestData<APInvoice>();
			dataSource.SaveAsIncomplete();

			var controller = Controller as APInvoiceController;
			using (var form = controller.GetForm_ForTest(dataSource))
			{
				var expectedErrorMessage =
					string.Format(@"Message: ZController {0} is handling an invalid bizO Enterprise.Accounting.Business.ARAP.Invoicing.APInvoice. The correct controller ID should be APIncompleteInvoice
AH_TransactionType: INI
AH_Ledger: IN
Details:
Header: PK = {1}", Controller.ID, dataSource.PK.ToString());
				AssertEquals("Should have a developer notification exception.", 1, ExceptionReporterTestListener.Instance.Count);
				Assert("The developer notification exception should be about using invalid controller.", ExceptionReporterTestListener.Instance[0].InnerException.Message.Contains(expectedErrorMessage));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShowErrorMessageWhenInvalidDataSource()
		{
			var invalidSource = ChangeStateOfParentTransactionHeaderRow();

			Factory.Save();

			using (var form = Controller.ShowViewForm(invalidSource))
			{
				var expectedErrorMessage = Controller.AlreadyDeletedOrIrreversiblyChangedMessage;
				AssertEquals("Should not have a developer notification exception.", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldLoadBusinessObject()
		{
			var controller = Controller as APInvoiceController;
			AssertEquals(true, controller.ShouldLoadBusinessObject);
		}

		#endregion
	}
}
