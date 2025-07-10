using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	sealed class CusHAWBGuiEventHandlersTest : TestCaseWithClientSpecificDocuments
	{
		[ExpectNoExceptions]
		public void TestHookUnhookWithNullCusHAWB()
		{
			using (CusHAWBGuiEventHandlers helper = new CusHAWBGuiEventHandlers(null))
			{
				helper.HookEvents();
				helper.UnhookEvents();
			}
		}

		public void TestHandlesIsRedirectedChanging()
		{
			GuiEventHandlers.HookEvents();
			CusHAWB.IsRedirected = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			CusHAWB.IsRedirected = false;
			AssertEquals("Should still be true, the event is canceled", true, CusHAWB.IsRedirected);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			CusHAWB.IsRedirected = false;
			AssertEquals("Should be false, the event is not canceled", false, CusHAWB.IsRedirected);
			string expectedMessage = "Removing the override will reset your Delivery Address details.\r\nYou will lose changes that you have made to the Delivery Address Redirection.";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GuiEventHandlers.UnhookEvents();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			CusHAWB.IsRedirected = true;
			CusHAWB.IsRedirected = false;
			AssertEquals("Nothing should happen once the event is unhooked", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestHandlesRebillFlagChanging()
		{
			GuiEventHandlers.HookEvents();
			CusHAWB.RebillFlag = RebillFlags.Unflagged;
			AssertNull("dialog should not be shown when unflagged", ZFormModaliser.LastFormShownDialogForTest);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			CusHAWB.RebillFlag = RebillFlags.IsAbandoned;
			AssertEquals(typeof(CustomFlagForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals(RebillFlags.IsAbandoned, CusHAWB.RebillFlag);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			CusHAWB.RebillFlag = RebillFlags.IsRTS;
			AssertEquals(typeof(RTSTranshipmentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should be canceled and rebill flag should not change", RebillFlags.IsAbandoned, CusHAWB.RebillFlag);
			GuiEventHandlers.UnhookEvents();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			CusHAWB.RebillFlag = RebillFlags.IsRTS;
			AssertEquals(typeof(RTSTranshipmentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Nothing should happen once the event is unhooked", RebillFlags.IsRTS, CusHAWB.RebillFlag);
		}

		public void TestHandlesQueryShipmentHeldLetterDetails()
		{
			var documentEventSource = new MockDocumentEventSource();
			CusHAWB.DocumentSupporter.Initialise(documentEventSource);
			var shipmentHeldLetter = new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			var e = new DocumentCancelEventArgs(shipmentHeldLetter);
			GuiEventHandlers.HookEvents();
			try
			{
				ZFormModaliser.ShowDialogsInTest = true;
				documentEventSource.FireDocumentPrintRequested(e);
				AssertEquals("The user should have been prompted to enter the details", "I want to", CusHAWB.ShipmentHeldLetterDetails.ReasonText);
				CusHAWB.ShipmentHeldLetterDetails.ReasonText = "";
				GuiEventHandlers.UnhookEvents();
				documentEventSource.FireDocumentPrintRequested(e);
				AssertEquals("Nothing should happen once the event is unhooked", "", CusHAWB.ShipmentHeldLetterDetails.ReasonText);
			}
			finally
			{
				ZFormModaliser.ShowDialogsInTest = false;
			}
		}

		public void TestHandlesPrintBatchItemQueued()
		{
			UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			GuiEventHandlers.HookEvents();
			currentPrintBatch.QueueForBatchPrintAndSave(CusHAWB, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK, false);
			AssertEquals("The user should NOT be notified (NotifyUser=false)", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			currentPrintBatch.QueueForBatchPrintAndSave(CusHAWB, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
			AssertEquals("The user should be notified (NotifyUser=true)", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals("The user should be notified of the batch number", true, UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("batch number 1") != -1);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GuiEventHandlers.UnhookEvents();
			currentPrintBatch.QueueForBatchPrintAndSave(CusHAWB, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
			AssertEquals("Nothing should happen once the event is unhooked", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		[ExpectNoExceptions]
		public void TestShowShipmentHeldLetterForm_WithNullCusHAWB()
		{
			using (CusHAWBGuiEventHandlers guiEventHandlers = new CusHAWBGuiEventHandlers(null))
			{
				guiEventHandlers.RunPreSaveDialogs(CusHAWB);
			}
		}

		public void TestShowShipmentHeldLetterForm_WhenUserPressesCancel()
		{
			CusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			CusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			CusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			CusHAWB.ShipmentHeldLetterDetails.ReasonText = "OldReason";
			AssertNoCusHAWBValidationErrors();
			try
			{
				ZFormModaliser.ShowDialogsInTest = true;
				GuiEventHandlers.ShipmentHeldLetterFormDialogResult = DialogResult.Cancel;
				AssertEquals("Should not continue with save if the user cancels the dialog", ContinueWithSave.No, GuiEventHandlers.RunPreSaveDialogs(CusHAWB));
				AssertEquals("Any user edits on the form should be reverted", "OldReason", CusHAWB.ShipmentHeldLetterDetails.ReasonText);
			}
			finally
			{
				ZFormModaliser.ShowDialogsInTest = false;
			}
		}

		public void TestShowShipmentHeldLetterForm_WhenUserPressesOK()
		{
			CusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			CusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			CusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			AssertNoCusHAWBValidationErrors();
			try
			{
				ZFormModaliser.ShowDialogsInTest = true;
				GuiEventHandlers.ShipmentHeldLetterFormDialogResult = DialogResult.OK;
				AssertEquals("Should continue if the user pressed OK", ContinueWithSave.Yes, GuiEventHandlers.RunPreSaveDialogs(CusHAWB));
				AssertEquals("Details should be populated by the user", "I want to", CusHAWB.ShipmentHeldLetterDetails.ReasonText);
			}
			finally
			{
				ZFormModaliser.ShowDialogsInTest = false;
			}
		}

		public void TestShowShipmentHeldLetterForm_WhenHeldLetterNotRequired()
		{
			CusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			AssertNoCusHAWBValidationErrors();
			AssertEquals("Should continue if the dialog is not required to be shown", ContinueWithSave.Yes, GuiEventHandlers.RunPreSaveDialogs(CusHAWB));
			AssertEquals("Should not show the form if no queue movement was required", false, GuiEventHandlers.WasShipmentHeldLetterFormShown);
		}

		public void TestShowShipmentHeldLetterForm_WithCusHAWBValidationErrors()
		{
			CusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			CusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing;
			CusHAWB.CS_OH_Consignee = ZGuid.Invalid;
			AssertEquals("The CusHAWB should have validation errors for the test", true, CusHAWB.HasErrors);
			AssertEquals("Should 'continue' to ValidateAndSave if there are validation errors, so the form can show these errors", ContinueWithSave.Yes, GuiEventHandlers.RunPreSaveDialogs(CusHAWB));
		}

		void AssertNoCusHAWBValidationErrors()
		{
			CusHAWB.RunPreSaveValidation();
			AssertNoErrors("CusHAWB should not have validation errors for the test", CusHAWB);
		}

		#region Test Classes
		class TestUPECusHAWB : UPECusHAWB
		{
			public TestUPECusHAWB(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new CusHAWBDocumentSupporter DocumentSupporter
			{
				get
				{
					return base.DocumentSupporter;
				}
			}
		}

		class MockDocumentEventSource : IDocumentEvents
		{
			public event DocumentCancelEventHandler DocumentPrintRequested;
			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public event DocumentPrintedEventHandler DocumentPrinted;
			public void FireDocumentPrintRequested(DocumentCancelEventArgs e)
			{
				DocumentPrintRequested(this, e);
			}

			public void FireDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed(this, e);
			}

			public void FireDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrePrinted(this, e);
			}

			public void FireDocumentPrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrinted(this, e);
			}
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		TestCusHAWBGuiEventHandlers GuiEventHandlers
		{
			get
			{
				if (fGuiEventHandlers == null)
				{
					fGuiEventHandlers = new TestCusHAWBGuiEventHandlers(CusHAWB);
					fGuiEventHandlers.RunPreSaveDialogs_CallBase = true;
				}

				return fGuiEventHandlers;
			}
		}
		TestCusHAWBGuiEventHandlers fGuiEventHandlers;

		TestUPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.NewWithValidTestData<TestUPECusHAWB>();
					fCusHAWB.CS_OA_ConsigneeAddress = fCusHAWB.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
					fCusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
					fCusHAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
					fCusHAWB.CS_HAWB = "TESTHAWB";
					fCusHAWB.CS_GoodsDescription = "THINGS";
					fCusHAWB.CS_ResponsiblePartyID = "111";
					fCusHAWB.CS_RL_NKOrigin = "NZAKL";
					fCusHAWB.CS_RL_NKDestination = "AUSYD";
					fCusHAWB.CS_Weight = 1m;
					fCusHAWB.CS_WeightUQ = "KG";
					fCusHAWB.CS_PiecesManifested = 1;
					fCusHAWB.CS_GoodsValue = 1m;

					var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
					fCusHAWB.CS_CM = mawb.PK;
				}

				return fCusHAWB;
			}
		}
		TestUPECusHAWB fCusHAWB;

		protected override void TearDown()
		{
			base.TearDown();
			GuiEventHandlers.Dispose();
		}
		#endregion
	}
}
