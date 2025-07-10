using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	[SuppressBindingMemberBashingTest]
	public partial class AttachDocumentForm : ZChildForm
	{
		public AttachDocumentForm()
		{
			InitializeComponent();
			ChangeFormBorderStyleForRendering();
		}

		public AttachDocumentForm(SupportIncidentCloseAction action, string formTitle)
			: base(action)
		{
			this.Text = formTitle;
			DragDrop += new DragEventHandler(AttachDocumentForm_DragDrop);
			DragOver += new DragEventHandler(AttachDocumentForm_DragOver);
			ChangeFormBorderStyleForRendering();
			IncidentAction.SetERequestStatusOnlyInfo.ValueChanged += new EventHandler(SetERequestStatusOnly_ValueChanged);
		}

		SupportIncidentCloseAction IncidentAction
		{
			get { return (SupportIncidentCloseAction)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			InitializeComponent();
			base.InitializeComponent();
			resolutionCommentTextBoxSpellChecker = SpellChecker.InitialiseSpellcheck(ResolutionCommentTextBox, "AttachDocumentForm_ResolutionCommentTextBox");
			UpdateKnownNames();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
		}

		protected SpellChecker resolutionCommentTextBoxSpellChecker;

		void UpdateKnownNames()
		{
			var knownNames = new List<string>();

			knownNames.Add(IncidentAction.Incident?.Contact?.Name);
			knownNames.Add(IncidentAction.Incident?.ClientName);
			knownNames.Add(IncidentAction.Incident?.FeatureRequestContact?.Name);
			knownNames.Add(IncidentAction.Incident?.FeatureRequestClientName);
			IncidentAction.Incident?.EConversation.ExistingConversation?.Staff.ForEach(staff => knownNames.Add(staff.Parent?.Name));
			IncidentAction.Incident?.EConversation.ExistingConversation?.RelatedParties.ForEach(staff =>
			{
				knownNames.Add(staff.Parent?.Name);
				knownNames.Add(staff.Parent?.OrganisationName);
			});
			IncidentAction.Incident?.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(task => knownNames.Add(task.StaffName));

			resolutionCommentTextBoxSpellChecker.UpdateWordsToIgnore(knownNames.Where(knownName => !string.IsNullOrWhiteSpace(knownName)));
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void AttachDocumentForm_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		void AttachDocumentForm_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
			{
				String[] selectedFilePaths = e.Data.GetData(DataFormats.FileDrop) as String[];
				if (selectedFilePaths != null && selectedFilePaths.Length > 0)
				{
					FileAttributes attr = File.GetAttributes(selectedFilePaths[0]);
					if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
					{
						Globals.Message.ShowError("Cannot attach folder.");
					}
					else
					{
						AttachToEDocs(selectedFilePaths[0]);
					}
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			var setERequestStatusOnly = IncidentAction.SetERequestStatusOnly;
			if (attachedEdoc == null && !setERequestStatusOnly)
			{
				Globals.Message.ShowError("Please find estimate attached.");
			}
			else if (attachedEdoc != null || setERequestStatusOnly)
			{
				((SupportIncidentAction)BusinessEntity).SynchroniseToIncident();
				DialogResult = DialogResult.OK;
			}
			else
			{
				DialogResult = DialogResult.None;
			}
		}

		void SetERequestStatusOnly_ValueChanged(object sender, EventArgs e)
		{
			var setERequestStatusOnly = IncidentAction.SetERequestStatusOnly;

			if (setERequestStatusOnly)
			{
				DisabledCommentStore = IncidentAction.Comment;
				IncidentAction.Comment = string.Empty;
			}
			else
			{
				if (!DisabledCommentStore.IsEmpty)
				{
					IncidentAction.Comment = DisabledCommentStore;
				}
			}

			this.filePathBox.Enabled = !setERequestStatusOnly;
			this.addEdocButton.Enabled = !setERequestStatusOnly;
			this.ResolutionCommentTextBox.Enabled = !setERequestStatusOnly;
		}

		ZString DisabledCommentStore { get; set; }

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			(attachedEdoc as BusinessObject)?.Delete();
			DialogResult = DialogResult.Cancel;
		}

		IeDoc AttachToEDocs(string filename)
		{
			if (!string.IsNullOrEmpty(filename))
			{
				try
				{
					ZQuery query = new ZQuery(RefDocTypeSchema.RT_DocType, DocumentType);
					query.AddToFilter(RefDocTypeSchema.RT_IsActive, true);
					query.AddToFilter(RefDocTypeSchema.RT_IsPublished, true);
					RefDocType docType = IncidentAction.Incident.Factory.LoadTop1<RefDocType>(query);

					if (docType != null)
					{
						var doc = IncidentAction.Incident.DocManagerInfo.AddFileOrDocument(filename, DocumentType);
						IncidentAction.eDoc = doc;
						IncidentAction.Incident.RegisterEditableChildObject(doc.ParentMain);
						IncidentAction.Incident.Request.RegisterEditableChildObject(doc.ParentMain);
						return doc;
					}
					else
					{
						Globals.Message.ShowError(string.Format("Document type {0} is not found, or not active, or not published.", DocumentType));
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var errorMessage = string.Format("Failed to attach file {0} to eDocs.", filename);
					Globals.Message.ShowError(errorMessage);
					ErrorReporter.ReportOnce("AttachDocumentForm - AttachToEDocs", errorMessage, ex);
				}
			}
			else
			{
				Globals.Message.ShowError("Please select a file.");
			}

			return null;
		}

		string DocumentType
		{
			get
			{
				if (IncidentAction.SendDevelopmentEstimate)
				{
					return "SES";
				}
				else if (IncidentAction.SendSoftwareQuote)
				{
					return "SQU";
				}
				else
				{
					return "COR";
				}
			}
		}

		IeDoc attachedEdoc;
		void addEdocButton_Click(object sender, EventArgs e)
		{
			eDocsUserControl.ShowDialogAndAddValidFiles(false, filePathBox.Text, files =>
			{
				if (files != null && files.Length > 0)
				{
					(attachedEdoc as BusinessObject)?.Delete();
					attachedEdoc = AttachToEDocs(files[0]);
					if (attachedEdoc != null)
					{
						filePathBox.Text = files[0];
						CloseButton.Enabled = true;
					}
				}
			});
		}
	}
}
