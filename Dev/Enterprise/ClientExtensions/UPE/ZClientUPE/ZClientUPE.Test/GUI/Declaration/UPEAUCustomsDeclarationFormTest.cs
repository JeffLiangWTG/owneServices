using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI.Testing
{
	sealed class UPEAUCustomsDeclarationFormTest : TestCaseWithFactory
	{
		public void TestFormCaption()
		{
			Declaration.JE_HouseBill = "HOUSEBILL";
			Assert(Form.FormCaption.EndsWith(" - HOUSEBILL"));
		}

		public void TestProcessQueuePlugInExists()
		{
			AssertNotNull("Process Queue Plug-in should exist", Form.PlugIns.GetPlugIn(ControllerIDs.ProcessQueue));
		}

		public void TestBrokerageUserControl()
		{
			using (BaseCustomsBrokerageUserControl control = Form.GetBrokerageUserControl())
			{
				AssertEquals(typeof(UPEAUBrokerageUserControl), control.GetType());
			}
		}

		public void TestImporterSupplierNotesShownToUserOnFormLoad()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "");
			StmNote relatedNote = importer.Notes.AddNew(false, PredefinedNoteTypes.Instance.ImportCustomsHandlingNotes.Description, "THIS NOTE SHOULD BE SELECTED");
			Declaration.JE_OH_Importer = importer.PK;
			Form.Show();
			Application.DoEvents();
			AssertEquals("Related notes should be visible", true, Declaration.Notes.ShowRelatedNotes);
			AssertEquals("Note tab page should be visible", true, ((Control)Form.CustomsBrokerageUserControl.GetStmNoteControl()).Visible);
			AssertEquals("CustomsHandlingNote", relatedNote, Form.CustomsBrokerageUserControl.GetStmNoteControl().SelectedNote);
		}

		public void TestShowCommercialInvoiceIfAvailable_WhenNoInvoiceToShow()
		{
			Form.Show();
			Application.DoEvents();
			AssertNull("Commercial invoice shouldnt be shown when it doesnt exist", CommercialInvoiceForm);
		}

		public void TestShowCommercialInvoiceIfAvailable_WhenNoAlternateMonitor()
		{
			CreateDeclarationCommercialInvoice();
			Form.SetScreenInfos(new Rectangle[] { CachedScreenInfo.Instance.PrimaryScreenInfo });
			Form.Show();
			Application.DoEvents();
			AssertNull("Commercial invoice shouldnt be shown when it doesnt exist", CommercialInvoiceForm);
		}

		public void TestShowCommercialInvoiceIfAvailable_WithCommercialInvoiceAndAlternateMonitor()
		{
			CreateDeclarationCommercialInvoice();
			var alternateMonitor = new Rectangle(1600, 1200, 1024, 768);
			Form.SetScreenInfos(new Rectangle[] { CachedScreenInfo.Instance.PrimaryScreenInfo, alternateMonitor });
			Form.Show();
			Application.DoEvents();
			AssertNull("Commercial invoice shouldnt be shown when viewing Custom Declarations", CommercialInvoiceForm);
		}

		public void TestShowAutoSendingLetterOfAuthorityErrorMessage()
		{
			UPEOrgHeader importer = Factory.NewWithValidTestData<UPEOrgHeader>();
			UPEAUCustomsDeclarationFormForTest form = new UPEAUCustomsDeclarationFormForTest(Factory.New<TestUPEJobDeclaration>());
			form.Declaration.JE_OH_Importer = importer.PK;
			form.Show();
			Application.DoEvents();
			Factory.Save();
			try
			{
				Assert(Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.ToString().IndexOf("Unable to Autosend the Lettter of Authority to the printer") > -1);
			}
			finally
			{
				form.Close();
				form.Dispose();
			}
		}

		public void TestNotifyMessageForImporterHasUncompletedDeclarations()
		{
			var jobDeclaration1 = Factory.NewWithValidTestData<UPEJobDeclaration>();
			var jobDeclaration2 = Factory.NewWithValidTestData<UPEJobDeclaration>();
			var importer = Factory.NewWithValidTestData<UPEOrgHeader>();
			((UPEOrgMiscServ)importer.MiscServ).LOAReceivedAuthorisingUPStoClearGoods = false;
			jobDeclaration1.JE_OH_Importer = importer.PK;
			jobDeclaration2.JE_OH_Importer = importer.PK;
			jobDeclaration1.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			jobDeclaration2.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Hold;
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			var form = new UPEAUCustomsDeclarationFormForTest(jobDeclaration1);
			form.Show();
			Application.DoEvents();
			try
			{
				AssertNotNull("alertForm should be shown", alertForm);
				Assert(alertForm.Alert.Alerts.IndexOf(ZString.Format("\nImporter {0} has incompleted declaration(s):\n", jobDeclaration2.Importer.OH_FullName)) > -1);
			}
			finally
			{
				form.Close();
				form.Dispose();
			}

			jobDeclaration1.JE_OH_Importer = importer.PK;
			jobDeclaration2.JE_OH_Importer = importer.PK;
			jobDeclaration1.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			jobDeclaration2.CurrentQueue.P4_CustomsQueue = ZString.Empty;
			Factory.Save();
			form = new UPEAUCustomsDeclarationFormForTest(jobDeclaration2);
			form.Show();
			Application.DoEvents();
			try
			{
				AssertNotNull("alertForm should be shown", alertForm);
				Assert(alertForm.Alert.Alerts.IndexOf(ZString.Format("\nImporter {0} has incompleted declaration(s):\n", jobDeclaration2.Importer.OH_FullName)) == -1);
			}
			finally
			{
				form.Close();
				form.Dispose();
			}
		}

		public void TestRefundEnquiryChanged()
		{
			Callout callout = Factory.NewWithValidTestData<Callout>();
			callout.CS_JE_CustomsFormalEntry = Declaration.PK;
			Declaration.IsRefundEnquiry = true;
			Form.Show();
			Application.DoEvents();
			Declaration.IsRefundEnquiry = false;
			AssertNull("Refund Enquiry Form should not be shown", refundEnquiryForm);
		}

		#region Event Handlers
		public void TestCusHAWBGuiHelper_EventHandlersHooked()
		{
			using (UPEAUCustomsDeclarationFormForTest form = new UPEAUCustomsDeclarationFormForTest(Declaration))
			{
				AssertEquals(true, form.CusHAWBGuiEventHandlers.IsEventHooked);
			}
		}

		public void TestValidateAndSave_RunPreSaveDialogs_ContinueWithSaveYes()
		{
			using (UPEAUCustomsDeclarationFormForTest form = new UPEAUCustomsDeclarationFormForTest(Declaration))
			{
				Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				RelatedCusHAWB.HasChanges = true;
				form.CusHAWBGuiEventHandlers.RunPreSaveDialogs_ContinueWithSave = ContinueWithSave.Yes;
				ContinueWithSave @continue = form.ValidateAndSave();
				AssertEquals("ContinueWithSave=Yes", ContinueWithSave.Yes, @continue);
				AssertEquals("Form should be saved", false, RelatedCusHAWB.HasChanges);
			}
		}

		public void TestValidateAndSave_RunPreSaveDialogs_ContinueWithSaveNo()
		{
			using (UPEAUCustomsDeclarationFormForTest form = new UPEAUCustomsDeclarationFormForTest(Declaration))
			{
				Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				RelatedCusHAWB.HasChanges = true;
				form.CusHAWBGuiEventHandlers.RunPreSaveDialogs_ContinueWithSave = ContinueWithSave.No;
				ContinueWithSave @continue = form.ValidateAndSave();
				AssertEquals("ContinueWithSave=No", ContinueWithSave.No, @continue);
				AssertEquals("Form should NOT be saved", true, RelatedCusHAWB.HasChanges);
			}
		}

		public void TestProcessQueueEventHooked()
		{
			using (UPEAUCustomsDeclarationFormForTest form = new UPEAUCustomsDeclarationFormForTest(Declaration))
			{
				AssertEquals(true, Declaration.CurrentQueue.SubscribedToEIRRaisedProcessing);
			}
		}

		#endregion
		#region Test Classes
		class UPEAUCustomsDeclarationFormForTest : UPEAUCustomsDeclarationForm
		{
			public UPEAUCustomsDeclarationFormForTest(UPEJobDeclaration declaration) : base(declaration)
			{
			}

			public new BaseCustomsBrokerageUserControl GetBrokerageUserControl()
			{
				return base.GetBrokerageUserControl();
			}

			public new ContinueWithSave ValidateAndSave()
			{
				return base.ValidateAndSave();
			}

			public TestCusHAWBGuiEventHandlers CusHAWBGuiEventHandlers
			{
				get
				{
					if (fCusHAWBGuiEventHandlers == null)
					{
						fCusHAWBGuiEventHandlers = new TestCusHAWBGuiEventHandlers(Declaration.FirstCusHAWB);
					}

					return fCusHAWBGuiEventHandlers;
				}
			}

			TestCusHAWBGuiEventHandlers fCusHAWBGuiEventHandlers;
			protected override CusHAWBGuiEventHandlers GetCusHAWBGuiEventHandlers()
			{
				return CusHAWBGuiEventHandlers;
			}

			public void SetScreenInfos(Rectangle[] value)
			{
				fScreenInfos = value;
			}

			public ImageManager ImageManagerExposed { get; private set; }

			protected override ImageManager GetImageManager(string filename, StorageDocsBase document, GraphicalDisplayForm displayForm)
			{
				ImageManagerExposed = base.GetImageManager(filename, document, displayForm);
				return ImageManagerExposed;
			}

			protected override IReadOnlyList<Rectangle> ScreenInfos
			{
				get
				{
					return fScreenInfos ?? CachedScreenInfo.Instance.ScreenInfos;
				}
			}

			Rectangle[] fScreenInfos;
			public bool FullScreenPreviewFormMaximized;
			protected override void MaximizeForm(Form form)
			{
				if (form is GraphicalDisplayForm)
				{
					FullScreenPreviewFormMaximized = true;
				}
				else
				{
					base.MaximizeForm(form);
				}
			}
		}

		class TestUPEJobDeclaration : UPEJobDeclaration
		{
			public TestUPEJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void PrintLetterOfAuthority(UPEPrintBatch currentPrintBatch, ZGuid menuItemPK)
			{
				base.PrintLetterOfAuthority(null, ZGuid.Empty);
			}
		}

		#endregion
		#region Implementation
		UPEAUCustomsDeclarationFormForTest Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = new UPEAUCustomsDeclarationFormForTest(Declaration);
				}

				return fForm;
			}
		}

		UPEAUCustomsDeclarationFormForTest fForm;
		GraphicalDisplayForm CommercialInvoiceForm
		{
			get
			{
				foreach (Form form in Application.OpenForms)
				{
					if (form is GraphicalDisplayForm)
					{
						return form as GraphicalDisplayForm;
					}
				}

				return null;
			}
		}

		RefundEnquiryForm refundEnquiryForm
		{
			get
			{
				foreach (Form form in Application.OpenForms)
				{
					if (form is RefundEnquiryForm)
					{
						return form as RefundEnquiryForm;
					}
				}

				return null;
			}
		}

		AlertForm alertForm
		{
			get
			{
				foreach (Form form in Application.OpenForms)
				{
					if (form is AlertForm)
					{
						return form as AlertForm;
					}
				}

				return null;
			}
		}

		UPECusHAWB RelatedCusHAWB
		{
			get
			{
				if (fRelatedCusHAWB == null)
				{
					fRelatedCusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
					fRelatedCusHAWB.CS_JE_CustomsFormalEntry = Declaration.PK;
				}

				return fRelatedCusHAWB;
			}
		}

		UPECusHAWB fRelatedCusHAWB;
		UPEJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<UPEJobDeclaration>();
				}

				return fDeclaration;
			}
		}

		UPEJobDeclaration fDeclaration;
		void CreateDeclarationCommercialInvoice()
		{
			var factory = new DocumentFactoryProvider().GetFactory(this.Factory);
			var parent = factory.New<StorageMain>();
			parent.SM_ParentFK = Declaration.PK;
			var document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
			Declaration.DocManagerInfo.Documents.Add(document);
			Declaration.DocManagerInfo.Save();
			AssertNotNull("Declaration.CommercialInvoiceImage should be available", Declaration.CommercialInvoiceImage);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (fForm != null)
			{
				if (fForm.ImageManagerExposed != null)
				{
					fForm.ImageManagerExposed.Dispose();
				}

				fForm.Dispose();
			}
		}
		#endregion
	}
}
