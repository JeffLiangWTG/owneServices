using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class CalloutFormTest : TestCaseWithFactory
	{
		public void TestActiveQueueTypeSetToCommercialInConstuctor()
		{
			AssertEquals(ProcessQueueType.Enum.Commercial, Form.Callout.ActiveProcessQueueForBinding[0].QueueType);
		}

		[ExpectNoExceptions]
		public void TestSettingActiveQueueTypeDoesNotBlowUpIfNothingInCollection()
		{
			Callout.ActiveProcessQueueForBinding.RemoveAll();
			object lazyLoadForm = Form;
		}

		public void TestFormCaption()
		{
			Form.BusinessEntity.CS_HAWB = "HOUSEBILL";
			AssertEquals("Finance Item - HOUSEBILL", Form.FormCaption);
		}

		public void TestPlugInsExist()
		{
			AssertNotNull("Process Queue Plug-in should exist", Form.PlugIns.GetPlugIn(ControllerIDs.ProcessQueue));
			AssertNotNull("Process Queue Plug-in should exist", Form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			AssertNotNull("Process Queue Plug-in should exist", Form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
		}

		public void TestNewButtonNotShownOnForm()
		{
			Form.Show();
			Application.DoEvents();
			bool hasNewButton = Form.fApplyButton != null && Form.fApplyButton.Text.IndexOf("New") > -1;
			Assert("Form should not have new button", !hasNewButton);
		}

		public void TestAlerts()
		{
			OrgHeader billTo = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode uPSAccountNumber = billTo.CustomsCodes.AddNew();
			uPSAccountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			uPSAccountNumber.OK_CustomsRegNo = "TEST";
			billTo.Notes.AddNew(false, PredefinedNoteTypes.Instance.InvoiceDetails.Description, "INVOICING DETAILS NOTE - SHOULD NOT BE SELECTED");
			StmNote invoicingPreferenceNote = billTo.Notes.AddNew(false, PredefinedNoteTypes.Instance.InvoicingPreferences.Description, "INVOICING NOTE - SHOULD BE SELECTED");
			Callout.BillToAccountNumber = "TEST";
			Form.Show();
			Application.DoEvents();
			AssertEquals(true, Form.alertForm.Visible);
			AssertEquals("Invoicing Preferences Note Exists.", Callout.AlertsList[0]);
			AssertEquals("Related notes should be visible", true, Callout.Notes.ShowRelatedNotes);
		}

		public void TestShowRelatedInvoicingPreferencesNote()
		{
			OrgHeader billTo = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode uPSAccountNumber = billTo.CustomsCodes.AddNew();
			uPSAccountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			uPSAccountNumber.OK_CustomsRegNo = "TEST";
			billTo.Notes.AddNew(false, PredefinedNoteTypes.Instance.InvoiceDetails.Description, "INVOICING DETAILS NOTE - SHOULD NOT BE SELECTED");
			StmNote invoicingPreferenceNote = billTo.Notes.AddNew(false, PredefinedNoteTypes.Instance.InvoicingPreferences.Description, "INVOICING NOTE - SHOULD BE SELECTED");
			Callout.BillToAccountNumber = "TEST";
			Form.Show();
			Application.DoEvents();
			AssertEquals("Related notes should be visible", true, Callout.Notes.ShowRelatedNotes);
		}

		public void TestShowFinanceTab()
		{
			Form.Show();
			Application.DoEvents();
			AssertEquals("Finance Tab", true, Form.IsFinanceTabSelected);
		}

		public void TestChargesGoReadWriteOnCtrlAlt4()
		{
			AssertEquals("Charges grid should be read only initially for the test", true, ((DataGrid)Form.LineChargesGrid).ReadOnly);
			Form.SetFormModifierKeys(Keys.Control | Keys.Alt);
			Form.OnKeyUp(new KeyEventArgs(Keys.D4));
			AssertEquals("Charges grid should not be read only after key combination pressed", false, ((DataGrid)Form.LineChargesGrid).ReadOnly);
		}

		public void TestBISIDownloadNotRequiredOrForcedCaptionTextBoxShownIfDownloadNotRequired()
		{
			Callout.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			Form.Show();
			Application.DoEvents();
			AssertEquals("Download required", false, Form.BISIDownloadNotRequiredOrForcedCaptionTextBox.Visible);
			Callout.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			Form.MainTabControl.SelectedIndex = 0;
			Form.MainTabControl.SelectedIndex = 1;
			AssertEquals("Download not required", true, Form.BISIDownloadNotRequiredOrForcedCaptionTextBox.Visible);
			AssertEquals("Download not required text box should be covering the date downloaded from BISI date edit", Form.BisiDownloadDateEdit.Bounds, Form.BISIDownloadNotRequiredOrForcedCaptionTextBox.Bounds);
		}

		public void TestFormIsNotResizedToLessThanMinimumSizeWhenShown()
		{
			Form.Show();
			Application.DoEvents();
			Assert("Width has to be greater or equal to the minimum width", Form.Width >= 998);
			Assert("Height has to be greater or equal to the minimum height", Form.Height >= 724);
		}

		public void TestPartPaymentButtonClick()
		{
			Callout.CalloutPartPayment.MustAddNote = false;
			Form.Show();
			AssertEquals("PreCondition", false, Callout.HasChanges);
			Form.PartPaymentButton.PerformClick();
			AssertEquals(false, Callout.HasChanges);
			Callout.CalloutPartPayment.MustAddNote = true;
			AssertEquals("PreCondition", false, Callout.HasChanges);
			Form.PartPaymentButton.PerformClick();
			AssertEquals(true, Callout.HasChanges);
		}

		#region Event Handlers
		public void TestCusHAWBGuiHelper_EventHandlersHooked()
		{
			AssertEquals(true, Form.CusHAWBGuiEventHandlers.IsEventHooked);
		}

		public void TestPaymentMethodChanged()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Form.Callout.Payment.IsBPay = true;
			AssertEquals(typeof(CalloutBPayPaymentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			Form.Callout.Payment.IsOther = true;
			AssertEquals(typeof(CalloutOtherPaymentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			Form.Callout.Payment.IsCreditCard = true;
			AssertEquals(typeof(CalloutCreditCardPaymentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should not change the payment method because the user cancelled the form", false, Form.Callout.Payment.IsCheque);
			AssertEquals("Should not change the payment method because the user cancelled the form", true, Form.Callout.Payment.IsOther);
		}

		public void TestRebillAccountClass13AllowsOnlyCertainPaymentTypes()
		{
			UPEOrgHeader uPEOrgHeader = Factory.New<UPEOrgHeader>();
			uPEOrgHeader.CompanyData.OB_OJ_ARDebtorGroup = Factory.New(typeof(OrgDebtorGroup)).PK;
			uPEOrgHeader.CompanyData.ARDebtorGroup.OJ_Code = "13";
			uPEOrgHeader.AccountNumber = "TEST";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Form.Callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Form.Callout.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.CRBL.Codes._R0_Rebill;
			Form.Callout.CurrentQueue.P4_CustomAttrib8 = "TEST";
			Form.Callout.Payment.IsAccount = true;
			AssertEquals("Payment method must be Cheque, Credit or EFT if Rebill Account Class is 13", UnitTestUserNotification.Instance.LastMessage.Text);
			Form.Callout.Payment.IsOther = true;
			AssertEquals("Payment method must be Cheque, Credit or EFT if Rebill Account Class is 13", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestViewEntryPrint()
		{
			Form.Show();
			Application.DoEvents();
			Callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(Customs.Business.BaseJobDeclaration)).PK;
			Form.PerformViewEntryPrintItemClick();
			AssertNotNull("Entry Print document should be generated", Form.EntryPrintDocumentHelper.LastPrintTask);
		}

		public void TestValidateAndSave_RunPreSaveDialogs_ContinueWithSaveYes()
		{
			Callout.HasChanges = true;
			Callout.CurrentQueue.P4_QueueName = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			Callout.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
			Form.CusHAWBGuiEventHandlers.RunPreSaveDialogs_ContinueWithSave = ContinueWithSave.Yes;
			ContinueWithSave @continue = Form.ValidateAndSave();
			AssertEquals("ContinueWithSave=Yes", ContinueWithSave.Yes, @continue);
			AssertEquals("Form should be saved", false, Callout.HasChanges);
		}

		public void TestValidateAndSave_RunPreSaveDialogs_ContinueWithSaveNo()
		{
			Callout.HasChanges = true;
			Callout.CurrentQueue.P4_QueueName = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			Form.CusHAWBGuiEventHandlers.RunPreSaveDialogs_ContinueWithSave = ContinueWithSave.No;
			ContinueWithSave @continue = Form.ValidateAndSave();
			AssertEquals("ContinueWithSave=No", ContinueWithSave.No, @continue);
			AssertEquals("Form should NOT be saved", true, Callout.HasChanges);
		}

		#endregion
		#region Test Classes
		class TestCalloutForm : CalloutForm
		{
			public TestCalloutForm(Callout businessEntity) : base(businessEntity)
			{
			}

			public void PerformViewEntryPrintItemClick()
			{
				ActionsMenuItem.MenuItems.FindByText("View Entry Print").PerformClick();
			}

			public void FireLoadEvent()
			{
				OnLoad(EventArgs.Empty);
			}

			public new void OnKeyUp(KeyEventArgs e)
			{
				base.OnKeyUp(e);
			}

			public Callout Callout
			{
				get
				{
					return BusinessEntity;
				}
			}

			#region FormModifierKeys
			public void SetFormModifierKeys(Keys keys)
			{
				fFormModifierKeys = keys;
			}

			protected override Keys FormModifierKeys
			{
				get
				{
					return fFormModifierKeys;
				}
			}

			Keys fFormModifierKeys;
			#endregion
			#region Exposed Control Properties
			public new MenuItem QueuePrintInvoiceMenuItem
			{
				get
				{
					return base.QueuePrintInvoiceMenuItem;
				}
			}

			public new ZRadioButton ChequeRadioButton
			{
				get
				{
					return base.ChequeRadioButton;
				}
			}

			public new ZGrid LineChargesGrid
			{
				get
				{
					return base.LineChargesGrid;
				}
			}

			public new AlertForm alertForm
			{
				get
				{
					return base.alertForm;
				}
			}

			public bool IsFinanceTabSelected
			{
				get
				{
					return base.MainTabControl.SelectedTab == base.FinanceTabPage;
				}
			}

			public new ZTemplateTabControl MainTabControl
			{
				get
				{
					return base.MainTabControl;
				}
			}

			public new ZButton RefundEnquiryButton
			{
				get
				{
					return base.RefundEnquiryButton;
				}
			}

			public new ZTextBox BISIDownloadNotRequiredOrForcedCaptionTextBox
			{
				get
				{
					return base.BISIDownloadNotRequiredOrForcedCaptionTextBox;
				}
			}

			public new ZDateEdit BisiDownloadDateEdit
			{
				get
				{
					return base.BisiDownloadDateEdit;
				}
			}

			public new IButton fApplyButton
			{
				get
				{
					return base.fApplyButton;
				}
			}

			#endregion
			#region Event Handlers
			public TestCusHAWBGuiEventHandlers CusHAWBGuiEventHandlers
			{
				get
				{
					if (fCusHAWBGuiEventHandlers == null)
					{
						fCusHAWBGuiEventHandlers = new TestCusHAWBGuiEventHandlers(BusinessEntity);
					}

					return fCusHAWBGuiEventHandlers;
				}
			}

			TestCusHAWBGuiEventHandlers fCusHAWBGuiEventHandlers;
			protected override CusHAWBGuiEventHandlers GetCusHAWBGuiEventHandlers()
			{
				return CusHAWBGuiEventHandlers;
			}

			public readonly TestEntryPrintDocumentHelper EntryPrintDocumentHelper = new TestEntryPrintDocumentHelper();
			protected override EntryPrintDocumentHelper GetEntryPrintDocumentHelper()
			{
				return EntryPrintDocumentHelper;
			}

			public new ContinueWithSave ValidateAndSave()
			{
				return base.ValidateAndSave();
			}
			#endregion
		}

		class TestEntryPrintDocumentHelper : EntryPrintDocumentHelper
		{
			public PrintTask LastPrintTask;
			protected override void RunPrintTask(PrintTask task)
			{
				LastPrintTask = task;
			}
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Form.Dispose();
		}

		TestCalloutForm Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = new TestCalloutForm(Callout);
				}

				return fForm;
			}
		}

		TestCalloutForm fForm;
		Callout Callout
		{
			get
			{
				if (fCallout == null)
				{
					fCallout = Factory.Load<Callout>(HouseBill.PK);
				}

				return fCallout;
			}
		}

		Callout fCallout;
		UPECusHAWB HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = (UPECusHAWB)Master.ChildBills.AddNew();
					using (fHouseBill.SuspendSettingHasChanges())
					{
						fHouseBill.CS_HAWB = "HBILLNO";
					}
				}

				return fHouseBill;
			}
		}

		UPECusHAWB fHouseBill;
		CusMAWB Master
		{
			get
			{
				if (fMaster == null)
				{
					fMaster = Factory.New<CusMAWB>();
				}

				return fMaster;
			}
		}

		CusMAWB fMaster;
		#endregion
	}
}
