using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.GUI;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI
{
	public class UPEAUCustomsDeclarationForm : ZAUCustomsDeclarationForm
	{
		public UPEAUCustomsDeclarationForm(UPEJobDeclaration declaration)
			: base(declaration)
		{
			PlugIns.Add(ControllerIDs.ProcessQueue);
			HookEvents();
		}

		public new UPEJobDeclaration Declaration
		{
			get { return (UPEJobDeclaration)base.Declaration; }
		}

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl()
		{
			return new UPEAUBrokerageUserControl();
		}

		public override string FormCaption
		{
			get { return base.FormCaption + " - " + ((UPEJobDeclaration)BusinessEntity).JE_HouseBill; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			FirstTimeShown = true;
			ShowRelatedCustomsHandlingNoteIfExists();
			Declaration.ErrorAutoSendingLetterOfAuthority += new UPEJobDeclaration.AutoSendingLetterOfAuthority(Declaration_ErrorAutoSendingLetterOfAuthority);
			Declaration.IsRefundEnquiryTempInfo.ValueChanged += new EventHandler(RefundEnquiryChanged);
		}

		void RefundEnquiryChanged(object sender, EventArgs e)
		{
			if (!Declaration.IsRefundEnquiry && Declaration.RelatedOwner != null)
			{
				if (DialogResult.OK == ZFormModaliser.ShowDialogAndDispose(new RefundEnquiryForm(Declaration.RefundWrapper)))
				{
					Declaration.IsRefundEnquiry = true;
				}
			}
		}

		void Declaration_ErrorAutoSendingLetterOfAuthority(ZString errorMessage)
		{
			Globals.Message.ShowError(errorMessage);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (FirstTimeShown)
			{
				FindImporterJobNumbers();
				if (Declaration.HasAlerts)
				{
					AlertForm = new AlertForm(new Alert(Declaration.AlertsList, BusinessEntity.Factory));
					AlertForm.Show();
				}
				FirstTimeShown = false;
			}
		}

		bool FirstTimeShown;
		AlertForm AlertForm;

		#region Importer Has Uncompleted Declarations

		void FindImporterJobNumbers()
		{
			if (Declaration.Importer != null &&
				Declaration.Importer.UncompletedJobNumbers.Count > 0)
			{
				ZString alerts = ZString.Empty;
				foreach (string jobNumber in Declaration.Importer.UncompletedJobNumbers)
				{
					if (Declaration.JE_DeclarationReference != jobNumber)
					{
						alerts += jobNumber + System.Environment.NewLine;
					}
				}
				if (ZString.Empty != alerts)
				{
					Declaration.AlertsList.Add(ZString.Format("\nImporter {0} has incompleted declaration(s):\n{1}", Declaration.Importer.OH_FullNameTruncated, alerts));
				}
			}
		}

		#endregion

		#region ShowCommercialInvoiceIfAvailable

		protected virtual ImageManager GetImageManager(string filename, StorageDocsBase document, GraphicalDisplayForm displayForm)
		{
			return new ImageManager(filename, document, displayForm);
		}

		void Manager_ImageDialogClose(object sender, EventArgs e)
		{
			var manager = sender as ImageManager;
			manager.ImageDialogClose -= new EventHandler(Manager_ImageDialogClose);
			manager.Close();
			manager.Dispose();
		}

		protected virtual IReadOnlyList<Rectangle> ScreenInfos
		{
			get { return CachedScreenInfo.Instance.ScreenInfos; }
		}

		protected virtual void MaximizeForm(Form form)
		{
			form.WindowState = FormWindowState.Maximized;
		}

		#endregion

		#region ShowRelatedCustomsHandlingNoteIfExists

		void ShowRelatedCustomsHandlingNoteIfExists()
		{
			StmNote customsHandlingNote = FindFirstCustomsHandlingNote();
			if (customsHandlingNote != null)
			{
				UserIdleWorker.Flush();
				Declaration.Notes.ShowRelatedNotes = true;
				CustomsBrokerageUserControl.GetStmNoteControl().FocusOnTabPage();
				CustomsBrokerageUserControl.GetStmNoteControl().SelectedNote = customsHandlingNote;
				Declaration.AlertsList.Add("Customs Handling Notes Exist");
			}
		}

		StmNote FindFirstCustomsHandlingNote()
		{
			if (Declaration.Importer != null)
			{
				foreach (StmNote note in Declaration.Importer.Notes.FindByDescription(PredefinedNoteTypes.Instance.ImportCustomsHandlingNotes.Description))
				{
					return note;
				}
			}
			return null;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (AlertForm != null)
				{
					AlertForm.Close();
					AlertForm.Dispose();
				}
				if (CusHAWBGuiEventHelper != null)
				{
					CusHAWBGuiEventHelper.Dispose();
				}

				if (UPEProcessQueueGuiEventHelper != null)
				{
					UPEProcessQueueGuiEventHelper.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Event Handlers

		CusHAWBGuiEventHandlers CusHAWBGuiEventHelper;
		UPEProcessQueueGuiEventHandlers UPEProcessQueueGuiEventHelper;

		void HookEvents()
		{
			CusHAWBGuiEventHelper = GetCusHAWBGuiEventHandlers();
			CusHAWBGuiEventHelper.HookEvents();

			UPEProcessQueueGuiEventHelper = GetUPEProcessQueueGuiEventHandlers();
			UPEProcessQueueGuiEventHelper.HookEvents();
		}

		protected virtual CusHAWBGuiEventHandlers GetCusHAWBGuiEventHandlers()
		{
			return new CusHAWBGuiEventHandlers(Declaration.FirstCusHAWB);
		}

		UPEProcessQueueGuiEventHandlers GetUPEProcessQueueGuiEventHandlers()
		{
			return new UPEProcessQueueGuiEventHandlers(Declaration.CurrentQueue);
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = CusHAWBGuiEventHelper.RunPreSaveDialogs(BusinessEntity);
			if (result == ContinueWithSave.Yes)
			{
				result = base.ValidateAndSave();
				Activate();
			}
			return result;
		}

		protected virtual ZString GetTempFileForImage(StorageDocsBase document)
		{
			return document.SaveToTempFile();
		}
		#endregion
	}
}
