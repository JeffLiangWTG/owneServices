using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using ErrorMessages = Enterprise.Accounting.Business.AccountingConstants.ChequeNumberAllocationErrorMessages;

namespace Enterprise.Accounting.GUI.ARAP.HotCheque.Testing
{
	public class AccHotChequeFormTest : TransactionCreatorBaseTest
	{
		AccChequeBook GetAutoPrintChequeBook()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_CurrentNo = 1;
			chequeBook.AK_LastNo = 2;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		[ExpectNoExceptions]
		public void TestLoadForm()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AccHotCheque bO = factory.New<AccHotCheque>();
			using (AccHotChequeForm form = new AccHotChequeForm(bO))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestAllowMaximumAmount()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AccHotCheque cheque = factory.New<AccHotCheque>();

			AccountingConfigurationRegistry.Instance.AccountingAllowUserToEnterMaximumAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (AccHotChequeForm form = new AccHotChequeForm(cheque))
			{
				form.Show();
				AssertEquals("Enabled", true, form.AQ_Calc_ActualAmountIndicatorBoundRadioButton.Enabled);
				AssertEquals("Enabled", true, form.AQ_Calc_MaximumAmountIndicatorBoundRadioButton.Enabled);
			}

			AccountingConfigurationRegistry.Instance.AccountingAllowUserToEnterMaximumAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (AccHotChequeForm form = new AccHotChequeForm(cheque))
			{
				form.Show();
				AssertEquals("Enabled", false, form.AQ_Calc_ActualAmountIndicatorBoundRadioButton.Enabled);
				AssertEquals("Enabled", false, form.AQ_Calc_MaximumAmountIndicatorBoundRadioButton.Enabled);
			}
		}

		public void TestPrintCheque()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			//StmTemplate Template = Factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_DataContext, "Cheques"));

			//Business.Base.AccStatement.BankStatement BankStatement = Factory.NewWithValidTestData<Business.Base.AccStatement.BankStatement>();
			//BankStatement.AB_SO_ChequeTemplate = Template.PK;

			AccChequeBook chequeBook = factory.NewWithValidTestData<AccChequeBook>();
			//ChequeBook.AK_AutoPrintCheque = true;
			//ChequeBook.AK_AB = BankStatement.PK;

			factory.Save();

			AccHotCheque cheque = factory.NewWithValidTestData<AccHotCheque>();
			cheque.AQ_ChequeNumber = "123";
			cheque.AQ_AK = chequeBook.PK;
			cheque.AQ_OH = factory.NewWithValidTestData<OrgHeader>().PK;
			cheque.AQ_ChequePayee = "Payee";
			cheque.AQ_Amount = 100;
			cheque.AQ_Description = "Description";
			cheque.AQ_GS_NKResponsibleStaff = GlbStaff.CurrentUser.GS_Code;

			using (AccHotChequeForm form = new AccHotChequeForm(cheque))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.PostingButtonsUserControl_Exposed.SaveButton.PerformClick();
				//AssertEquals("LastMessage.Text", "Do you want to print cheque?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cheque should not be printed.", false, cheque.AQ_Printed);

				//Cheque.HasChanges = true;
				//UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				//Form.PostingButtonsUserControl_Exposed.SaveButton.PerformClick();
				//AssertEquals("LastMessage.Text", "Do you want to print cheque?", UnitTestUserNotification.Instance.LastMessage.Text);
				//AssertEquals("Cheque should be printed.", true, Cheque.AQ_Printed);
			}
		}

		public void TestJobNumberFieldPickTheRightCompanyJob()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = TestObjectCreator.CreateShipment("00002001");
			Factory.Save();

			Job jobInDifferentCompany;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, creator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var factory2 = new BusinessObjectFactory();
				var creator2 = new TestObjectCreator(factory2);
				var shipment2 = factory2.Load<ForwardingShipment>(shipment.PK);

				jobInDifferentCompany = creator2.CreateJob(shipment2);
				factory2.Save();
			}

			var chequeBook = GetAutoPrintChequeBook();
			var cheque = Factory.NewWithValidTestData<AccHotCheque>();
			cheque.AQ_ChequeNumber = "123";
			cheque.AQ_AK = chequeBook.PK;
			cheque.AQ_OH = TestObjectCreator.Creditor1.PK;
			cheque.AQ_ChequePayee = "Payee";
			cheque.AQ_Amount = 100;
			cheque.AQ_Description = "Description";
			cheque.AQ_GS_NKResponsibleStaff = GlbStaff.CurrentUser.GS_Code;

			AssertResult(ZGuid.Missing);

			using (var job = creator.CreateJob(shipment))
			{
				AssertResult(job.PK);
			}

			void AssertResult(ZGuid expectedJobPK)
			{
				cheque.AQ_JH = ZGuid.Empty;

				using (AccHotChequeForm form = new AccHotChequeForm(cheque))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();

					AssertEquals(ZGuid.Empty, cheque.AQ_JH);
					var jobNumberField = form.Controls.Find("AQ_JHBoundFindBox", true)[0] as ZGuidFindBox;
					jobNumberField.Focus();
					jobNumberField.CurrentCode = "00002001";
					var otherField = form.Controls.Find("AQ_OHBoundFindBox", true)[0] as ZGuidFindBox;
					otherField.Focus();
					AssertEquals(expectedJobPK, cheque.AQ_JH);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestAutoPrintCheque()
		{
			AccChequeBook chequeBook = GetAutoPrintChequeBook();

			AccHotCheque cheque = Factory.NewWithValidTestData<AccHotCheque>();
			cheque.AQ_ChequeNumber = "123";
			cheque.AQ_AK = chequeBook.PK;
			cheque.AQ_OH = TestObjectCreator.Creditor1.PK;
			cheque.AQ_ChequePayee = "Payee";
			cheque.AQ_Amount = 100;
			cheque.AQ_Description = "Description";
			cheque.AQ_GS_NKResponsibleStaff = GlbStaff.CurrentUser.GS_Code;
			Assert("Cheque number should be empty so far", cheque.AQ_ChequeNumber.IsEmpty);
			Assert("Cheque should be AutoPrinted", ((IChequeNumberAutoAllocation)cheque).IsAutoAllocationEnabled);

			using (AccHotChequeForm form = new AccHotChequeForm(cheque))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();

				form.PostingButtonsUserControl_Exposed.SaveButton.PerformClick();
				AssertEquals("Cheque should be printed.", true, cheque.AQ_Printed);
				Assert("Cheque should be auto printed", ((IChequeNumberAutoAllocation)cheque).ChequeIsAutoPrinted);
				Assert("Cheque number should be auto allocated", ((IChequeNumberAutoAllocation)cheque).IsAllocationPerformed);
				AssertEquals("Cheque number should be updated on cheque", "1", cheque.AQ_ChequeNumber);

				cheque.ChequeBook.Reload();
				AssertEquals("Current number should be updated on chequebook", 2m, cheque.ChequeBook.AK_CurrentNo);
			}
		}

		[ExpectNoExceptions()]
		public void TestExceptionIsHandled_ChequeBookIsFull()
		{
			AccChequeBook chequeBook = GetAutoPrintChequeBook();
			AccHotCheque cheque = Factory.NewWithValidTestData<AccHotCheque>();
			cheque.AQ_ChequeNumber = "123";
			cheque.AQ_AK = chequeBook.PK;
			cheque.AQ_OH = TestObjectCreator.Creditor1.PK;
			cheque.AQ_ChequePayee = "Payee";
			cheque.AQ_Amount = 100;
			cheque.AQ_Description = "Description";
			cheque.AQ_GS_NKResponsibleStaff = GlbStaff.CurrentUser.GS_Code;

			Assert("Cheque should be AutoPrinted", ((IChequeNumberAutoAllocation)cheque).IsAutoAllocationEnabled);

			using (AccHotChequeForm form = new AccHotChequeForm(cheque))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();

				form.Test_DeactivateChequeBookOnAllocation = ZBool.True;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				form.PostingButtonsUserControl_Exposed.SaveButton.PerformClick();
				AssertEquals("There should be a message shown, saying that the Cheque Book is full.", ErrorMessages.ChequeBookIsFullExceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cheque should not be printed.", false, cheque.AQ_Printed);
				Assert("Cheque should not be auto printed", !((IChequeNumberAutoAllocation)cheque).ChequeIsAutoPrinted);
				Assert("Cheque number should not be auto allocated", !((IChequeNumberAutoAllocation)cheque).IsAllocationPerformed);
				Assert("Cheque number should be empty", cheque.AQ_ChequeNumber.IsEmpty);

				chequeBook.Reload();
				AssertEquals("Cheque book should not be incremented", 1m, chequeBook.AK_CurrentNo);
			}
		}

		public void TestShowConfirmationForDelete()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AccHotCheque cheque = factory.New<AccHotCheque>();
			factory.Save();

			using (AccHotChequeForm form = new AccHotChequeForm(cheque))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.PostingButtonsUserControl_Exposed.SaveAndCloseButton.PerformClick();
				AssertEquals("LastMessage.Text", "You are about to cancel this check. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Check should not be Cancelled.", false, cheque.AQ_Cancelled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PostingButtonsUserControl_Exposed.SaveAndCloseButton.PerformClick();
				AssertEquals("LastMessage.Text", "You are about to cancel this check. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cheque should be Cancelled.", true, cheque.AQ_Cancelled);
			}
		}

		public void TestCancelHotChequeVerb()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AccHotCheque hotCheque = factory.NewWithValidTestData<AccHotCheque>();
			using (AccHotChequeForm form = new AccHotChequeForm(hotCheque))
			{
				form.DisplayMode = ODisplayMode.Delete;
				AssertEquals(form.FormVerb, "Cancel");
			}
		}

		public void TestCancelButtonCaption()
		{
			using (AccHotChequeForm form = new AccHotChequeForm(new BusinessObjectFactory().NewWithValidTestData<AccHotCheque>()))
			{
				form.DisplayMode = ODisplayMode.Delete;
				AssertEquals(form.PostingButtonsUserControl_Exposed.CloseButton.Text, ZFormPostingButtonsStrategy.DefaultCloseButtonText);
			}
		}

		#region Test: Unable to edit a cheque that has just been posted.

		public void TestUnableToEditPostedCheque()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (AccHotChequeForm form = new AccHotChequeForm(HotCheque))
			{
				form.Show();
				form.PostingButtonsUserControl_Exposed.SaveButton.PerformClick();
				Assert("The cheque should have been saved.", !HotCheque.HasErrors);

				AccBankAccount account = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				AccTransactionHeader payment = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.Payment);
				payment.AH_AB = account.PK;
				payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				payment.AH_IsCancelled = false;
				payment.AH_ChequeOrReference = HotCheque.AQ_ChequeNumber;

				Factory.Save();

				AccHotCheque cheque2ndInstance = Factory.Load<AccHotCheque>(HotCheque.PK);

				Assert("Precondition", !form.AQ_DescriptionBoundTextBox_Exposed.ReadOnly);
				cheque2ndInstance.AQ_AH = payment.PK;
				Assert(form.AQ_DescriptionBoundTextBox_Exposed.ReadOnly);

				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.OK);
				form.PostingButtonsUserControl_Exposed.SaveButton.PerformClick();
				Assert("The cheque should have been ready to save.", !HotCheque.HasErrors);

				AssertEquals("Cheque should have been synchronised.", AccHotCheque.POSTED, HotCheque.AQ_Calc_ChequeStatus);
				AccHotChequeTestHelper.AssertReadOnly("Fields should all be read-only due to having been posted already.", HotCheque, true);
			}
		}

		#endregion

		public void TestConcurrentSaveWithAccHotChequeDeleted()
		{
			var cheque1 = Factory.NewWithValidTestData<AccHotCheque>();
			cheque1.AQ_ChequeNumber = "123";
			cheque1.AQ_AK = ChequeBook.PK;
			cheque1.AQ_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			cheque1.AQ_ChequePayee = "Payee";
			cheque1.AQ_Amount = 100;
			cheque1.AQ_Description = "Description";
			cheque1.AQ_GS_NKResponsibleStaff = GlbStaff.CurrentUser.GS_Code;
			cheque1.Factory.Save();

			var cheque2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<AccHotCheque>(cheque1.PK);

			using (var testForm = new AccHotChequeForm(cheque2))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				testForm.Show();
				cheque2.AQ_Amount = 123;
				Assert("Precondition", cheque2.HasChanges);

				TestConnection.ExecuteNonQuery(string.Format("DELETE FROM dbo.AccHotCheque WHERE AQ_PK='{0}'", cheque1.PK));
				AssertNull("AccHotCheque has been deleted.", new BusinessObjectFactory().Load(typeof(AccHotCheque), cheque1.PK));

				AssertNoExceptionThrown("Should not raise a NullReferenceException", () => testForm.FireSaveButton());
			}
		}

		public void TestHandleSaveExceptionDoesNotThrowNullReferenceException()
		{
			var cheque1 = Factory.NewWithValidTestData<HotChequeWithActionOnSaving>();
			cheque1.AQ_ChequeNumber = "123";
			cheque1.AQ_AK = ChequeBook.PK;
			cheque1.AQ_OH = TestObjectCreator.Creditor1.PK;
			cheque1.AQ_ChequePayee = "Payee";
			cheque1.AQ_Amount = 100;
			cheque1.AQ_Description = "Description";
			cheque1.AQ_GS_NKResponsibleStaff = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var cheque2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<AccHotCheque>(cheque1.PK);

			using (var testForm = new AccHotChequeForm(cheque1))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				testForm.Show();
				cheque1.AQ_Amount = 123m;

				cheque2.AQ_Amount = 321m;
				cheque2.Factory.Save();

				cheque1.ActionOnSaving = () => ((KForm)testForm).DataSource = null;

				AssertNoExceptionThrown("Should not raise a NullReferenceException", () => testForm.FireSaveButton());
				AssertEquals("No NullReferenceException reported", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		class HotChequeWithActionOnSaving : AccHotCheque
		{
			public HotChequeWithActionOnSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public Action ActionOnSaving { get; set; }

			protected override void OnFactorySaving()
			{
				base.OnFactorySaving();
				ActionOnSaving?.Invoke();
			}
		}
	}
}
