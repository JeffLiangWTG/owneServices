using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class eDocsUserControl : ZUserControl, IReadOnlyToggleControl
	{
		public eDocsUserControl(eDocsPlugIn plugIn)
		{
			//add ZBindingContext to fix loop with StorageMain property setters
			this.BindingContext = new ZBindingContext();
			InitializeComponent();
			PlugIn = plugIn;

			StorageDocsGrid.DeletingDocuments += StorageDocsGrid_DeletingDocuments;
			StorageDocsGrid.ColourDeciding += StorageDocsGrid_ColourDeciding;
			RelatedParentsGrid.ColourDeciding += RelatedParentsGrid_ColourDeciding;
			RebuildContextMenus();

			DepartmentSpecificCheckBox.AllowOverlap(ShowDeletedDocumentsCheckBox);
		}

		public eDocsPlugIn PlugIn
		{
			get => plugIn;
			set
			{
				plugIn = value;
				StorageDocsGrid.StorageDocImageViewer.Initialise(StorageDocsGrid, plugIn);

				StorageDocsGrid.DragDropTarget = plugIn;
				StorageDocsGrid.DocumentManipulationTarget = plugIn;
			}
		}

		eDocsPlugIn plugIn;

		#region Binding

		public override BindingContext BindingContext
		{
			get => base.BindingContext;

			set
			{
				base.BindingContext = value;
				StorageDocsGrid?.ResetCachedBindingContext();
				RelatedParentsGrid?.ResetCachedBindingContext();
			}
		}
		CurrencyManager RelatedParentsListManager => relatedParentsListManager ?? (relatedParentsListManager = (CurrencyManager)GetBindingManager("RelatedParentMains"));
		CurrencyManager relatedParentsListManager;

		CurrencyManager StorageDocsListManager => storageDocsListManager ?? (storageDocsListManager = (CurrencyManager)GetBindingManager("RelatedParentMains.eDocsView"));
		CurrencyManager storageDocsListManager;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (RelatedParentsListManager != null)
			{
				RelatedParentsListManager.CurrentChanged -= new EventHandler(RelatedParentsListManager_CurrentChanged);
			}

			if (StorageDocsListManager != null)
			{
				StorageDocsListManager.CurrentChanged -= new EventHandler(StorageDocsListManager_CurrentChanged);
			}

			if (dataSource != null)
			{
				documentPreviewEDocsControl.Rotation -= Rotate;
			}

			base.SetDataBinding(dataSource, dataMember);
			relatedParentsListManager = null;
			storageDocsListManager = null;

			if (RelatedParentsListManager != null)
			{
				RefreshStorageDocsPanelSecurityOverlayLabel();
				RelatedParentsListManager.CurrentChanged += new EventHandler(RelatedParentsListManager_CurrentChanged);
			}

			if (StorageDocsListManager != null)
			{
				StorageDocsListManager.CurrentChanged += new EventHandler(StorageDocsListManager_CurrentChanged);
			}

			if (dataSource != null)
			{
				UpdateFileIcons();
				UpdateDocumentPreview();
				documentPreviewEDocsControl.Rotation += Rotate;

				StorageMain storageMain = CurrentDataItem as StorageMain;

				bool showRequiredDocs = ShowRequiredDocuments(storageMain);
				RequiredDocumentsUserControl.Visible = showRequiredDocs;
				eDocsSplitContainer.Panel2Collapsed = !showRequiredDocs;

				GenerateNumberButton.Visible = storageMain != null && storageMain.DocManagerInfo != null && storageMain.DocManagerInfo.SupportsStorageNumberGeneration;
			}
		}

		bool ShowRequiredDocuments(StorageMain sm) => sm?.GetRequiredDocuments() != null;
		void eDocsUserControl_Resize(object sender, System.EventArgs e)
		{
			CheckAndResizeDocumentPreview();
		}

		void CheckAndResizeDocumentPreview()
		{
			var expectedWidth = ControlDpiScalingHelper.MarkAsScaled((int)(this.Size.Height * 0.8));

			if ((documentPreviewEDocsControl.Size.Height != this.Size.Height) || (documentPreviewEDocsControl.Size.Width != expectedWidth))
			{
				DocumentPreviewPanel.Size = ControlDpiScalingHelper.NewScaledSize(expectedWidth, this.Size.Height, false);
			}
		}

		internal void DisableInsert()
		{
			StorageDocsGrid.ShowPasteMenuItem = false;
			StorageDocsGrid.IsInsertAllowed = false;
			AddEDocsButton.Enabled = false;
		}

		void RelatedParentsListManager_CurrentChanged(object sender, EventArgs e)
		{
			PlugIn.CurrentParentMainOnGrid = (StorageMain)RelatedParentsGrid.ListManager.GetCurrent();
			PlugIn.RegisterEditableForRelatedEDocsIfNeeded();

			var documentsGridEditable = PlugIn.IsEditable;
			StorageDocsGrid.ReadOnly = !documentsGridEditable;
			StorageDocsGrid.IsEditable = documentsGridEditable;
			AddEDocsButton.ReadOnly = PlugIn.IsReadOnly || !PlugIn.IsInsertAllowed;

			RefreshStorageDocsPanelSecurityOverlayLabel();

			NotEditableLabel.Text = Res.GetString("c04258a3-6b1d-4b9a-83a5-b70efe0c101b", "Related eDocs are not editable from this form. To edit these eDocs, go to the form for {0} and click on the eDocs tab.", PlugIn.CurrentParentMainOnGrid.DocumentOwnerDescription);
			NotEditableLabel.Visible = !PlugIn.IsTopLevelParentSelected && StorageDocsGrid.Visible && !StorageDocsGrid.IsEditable && PlugIn.CurrentParentMainOnGrid.DocumentOwner.TableName != GlbPersonSchema.Constants.TableName;
		}

		void PreviewDocumentsCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			ToggleDocumentPreviewVisibility();
		}

		void ToggleDocumentPreviewVisibility()
		{
			var currentUserPK = EnvProxy.Instance.CurrentUser?.PK;
			if (PreviewDocumentsCheckBox.CheckState.Equals(CheckState.Unchecked))
			{
				if (currentUserPK.HasValue)
				{
					DocManagerRegistry.Instance.EDocsPreviewEnabled.SetValue(currentUserPK.Value, Guid.Empty, Guid.Empty, false);
				}
				this.DocumentPreviewPanel.Hide();
			}
			else
			{
				if (currentUserPK.HasValue)
				{
					DocManagerRegistry.Instance.EDocsPreviewEnabled.SetValue(currentUserPK.Value, Guid.Empty, Guid.Empty, true);
				}
				UpdateDocumentPreview();
				this.DocumentPreviewPanel.Show();
			}
		}

		void StorageDocsListManager_CurrentChanged(object sender, EventArgs e)
		{
			UpdateFileIcons();
			UpdateDocumentPreview();

#if DEBUG
			StorageDocsListManager_CurrentChangedHints++;
#endif
		}

		void UpdateDocumentPreview()
		{
			if (PreviewDocumentsCheckBox.CheckState.Equals(CheckState.Checked))
			{
				var currentElement = StorageDocsGrid.CurrentElement;
				documentPreviewEDocsControl.Close();

				if (currentElement == null)
				{
					//CurrentElement is null when the element in the list is null or when there are no elements in the list (represented by RowIndex == -1).
					//In the second case, we should simply close the documentPreview. In the first case, should we throw an error? Not sure why the document at a valid index would be null. Thoughts?
					documentPreviewEDocsControl.ReadOnly = true;
					return;
				}

				if (PreviewableDocumentHelper.IsSupported(currentElement.SC_DataType) && StorageDocsGrid.IsDocumentViewEnabled(currentElement))
				{
					previewableDocument = PreviewableDocumentHelper.GetPreviewableDocument(currentElement.SC_DataType, currentElement.SC_ImageData);
					documentPreviewEDocsControl.ReadOnly = false;
				}
				else
				{
					var badLoadReason = Res.GetString("7781D62E-3916-4294-8037-7ED3A4BF57AB", "This document format is not supported for previewing.");
					previewableDocument = new DummyPreviewable(badLoadReason);
					documentPreviewEDocsControl.ReadOnly = true;
				}

				RefreshPreview(currentElement);
			}
		}

		void RefreshPreview(StorageDocsBase currentElement)
		{
			try
			{
				documentPreviewEDocsControl.ShowFile(previewableDocument, currentElement);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				var displayFailedMessage = Res.GetString("79336767-4359-43C8-8B79-F9290F8D801E", "The preview cannot be displayed.");
				previewableDocument = new DummyPreviewable(displayFailedMessage);
				documentPreviewEDocsControl.ReadOnly = true;
				documentPreviewEDocsControl.ShowFile(previewableDocument, null);
			}
		}

		void Rotate(object sender, RotateEventArgs e)
		{
			previewableDocument.RotatePage(e.PageNumber, e.Clockwise);
			documentPreviewEDocsControl.ShowFile(previewableDocument, null);
		}

		void UpdateFileIcons()
		{
			StorageDocsBase currentElement = StorageDocsGrid.CurrentElement;
			DisposeIcon();
			IconPictureBox.Image = (currentElement != null) ? FileAssociationRetriever.GetIconForExtension(currentElement.SC_DataType) : null;
			ProgramNameLabel.Text = (currentElement != null) ? currentElement.SC_FriendlyFileDescription : ZString.Empty;
		}

		FileAssociationRetriever FileAssociationRetriever
		{
			get
			{
				if (fileAssociationRetriever == null)
				{
					fileAssociationRetriever = new FileAssociationRetriever();
				}
				return fileAssociationRetriever;
			}
		}

		FileAssociationRetriever fileAssociationRetriever;

		public GraphicalDisplayControl DocumentPreview
		{
			get
			{
				return documentPreviewEDocsControl;
			}
		}

		#endregion

		#region GUI Setup

		[Browsable(false)]
		public ZForm Form
		{
			get
			{
				if (parentForm == null)
				{
					parentForm = (ZForm)this.FindForm();
				}
				return parentForm;
			}
		}

		public void UpdateGraphicalDisplayFiles(Hashtable table)
		{
			IDictionaryEnumerator managerEnumerator = table.GetEnumerator();
			while (managerEnumerator.MoveNext())
			{
				ZGuid documentPK = (ZGuid)managerEnumerator.Key;
				string newImagePath = (string)managerEnumerator.Value;

				StorageDocsGrid.StorageDocImageViewer.UpdateGraphicalDisplayFile(documentPK, newImagePath);

				System.IO.File.Delete(newImagePath);
			}
		}

		public event EventHandler OnContextMenuBuilt;

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				RebuildContextMenus();
			}
		}

		void RebuildContextMenus()
		{
			StorageDocsGrid.RebuildContextMenu();
			OnContextMenuBuilt?.Invoke(this, EventArgs.Empty);
		}

		void StorageDocsGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			StorageDocsBase document = (StorageDocsBase)e.ObjectAtRow;

			if (document.SC_IsDeleted)
			{
				e.Colour = Color.Crimson;
			}

			if (document.RequiresUserToRead)
			{
				e.Colour = Color.SkyBlue;
			}
		}

		void RelatedParentsGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			if (((StorageMain)e.ObjectAtRow).IsTopLevelParent)
			{
				e.Colour = SystemColors.Info;
			}
		}

		public bool OnlyShowGrid
		{
			get => fOnlyShowGrid;
			set
			{
				RequiredDocumentsUserControl.Visible = !value;
				RelatedParentsGrid.Visible = !value;
				DescriptionAndIconPanel.Visible = !value;

				fOnlyShowGrid = value;
			}
		}

		bool fOnlyShowGrid;

		#endregion

		#region Image Displays

		void StorageDocsGrid_DeletingDocuments(object sender, DeletingDocumentsEventArgs e)
		{
			foreach (BusinessObject document in e.DocumentsToDelete)
			{
				StorageDocsGrid.StorageDocImageViewer.NotifyDocumentDeleted(document.PK);
			}
		}

		#endregion

		#region Actions

		void RefreshButton_Click(object sender, System.EventArgs e)
		{
			bool canReload = false;

			if (PlugIn.HostBusinessObject.HasChanges || !PlugIn.HostBusinessObject.IsInDatabase)
			{
				DialogResult answer = Globals.Message.Show(Res.GetString("85c74616-fc5d-4b99-b866-be0859b3b5dc", "This form needs to be saved before it can be reloaded. Save now?"), Res.GetString("d8761c20-16a1-4010-a0e9-e0e69dc4ad33", "Save Form"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);

				if (answer == DialogResult.Yes)
				{
					canReload = (Form.FireSaveButton() == ContinueWithSave.Yes);
				}
			}
			else
			{
				canReload = true;
			}

			if (canReload)
			{
				try
				{
					PlugIn.Reload();
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
				finally
				{
					if (PlugIn.HostBusinessObject != null && PlugIn.HostBusinessObject.IsDeleted)
					{
						Globals.Message.ShowError(Res.GetString("52606B93-7562-4D81-A70F-FA51DED086ED", "This {0} has been deleted by another user or process. This form will be closed now.", PlugIn.HostBusinessObject.HumanReadableName));
						Form.ForceClose();
					}
				}
			}
		}

		void ShowDeletedDocumentsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (ShowDeletedDocumentsCheckBox.Checked)
			{
				var storageMain = CurrentDataItem as StorageMain;
				if (storageMain != null && storageMain.eDocs.OfType<StorageDocsBase>().Any(x => x.SC_IsDeleted))
				{
					Globals.Message.ShowInformation(Res.GetString("8f7ac375-f1e5-4046-a464-0077bd8075a7", "The deleted files MUST be restored if you wish to edit the file content."));
				}
			}
		}

#if DEBUG
		internal bool SetControllerNullForTest;
#endif

		ZController GetControllerForDocumentOwner(IBusiness documentOwner)
		{
#if DEBUG
			if (Globals.IsTest && SetControllerNullForTest)
			{
				return null;
			}
#endif
			return ZControllerFactory.Instance.GetControllerForBizo(documentOwner);
		}

		public void DoubleClickOn_RelatedParents(object sender, EventArgs e)
		{
			if (Parent.FindForm() != null)
			{
				if (RelatedParentsGrid.ListManager.Position > -1)
				{
					var bizO = (StorageMain)RelatedParentsGrid.ListManager.GetCurrent();

					var documentOwner = bizO?.DocumentOwner;
					if (documentOwner != null)
					{
						var controller = GetControllerForDocumentOwner(documentOwner);
						if (controller != null)
						{
							try
							{
								controller.ShowEditForm(documentOwner);
							}
							catch (ModuleGuiNotSupportedException)
							{
								Globals.Message.ShowError(Res.GetString("1E97678D-25C8-428E-ACB1-677AD66AE3F2", "GUI MODULE not supported, contact support"));
							}
						}
						else
						{
							try
							{
								var moduleController = ZControllerFactory.Create(((ZForm)Parent.FindForm())?.ControllerID);
								var moduleDescription = moduleController?.ModuleID?.Description ?? (NoResString)"current";

								Globals.Message.ShowError(Res.GetString("D4D128B6-F53D-47A7-AA87-109319AD9B72", "Double click on Related eDocs is not supported in the {0} module. Please contact {1} support for more information.", moduleDescription, Core.Constants.ProductName), (NoResString)"Support");
							}
							catch (ModuleIDIsNullException ex)
							{
								var parentFormName = ((ZForm)Parent.FindForm())?.ToString() ?? "null";
								var documentOwnerName = documentOwner.GetType().FullName;
								ErrorReporter.ReportOnce("ZController_GetControllerForType_ModuleIDIsNullException",
									string.Format(System.Globalization.CultureInfo.InvariantCulture, "eDocsUserControl.DoubleClickOn_RelatedParents\r\nParent Form = {0}\r\nDocument Owner = {1}", parentFormName, documentOwnerName), ex);
							}
						}
					}
				}
#if DEBUG
				testDoubleClick = true;
#endif
			}
#if DEBUG
			else
			{
				testDoubleClick = false;
			}
#endif
		}

#if DEBUG
		[ThreadStatic] // REASON - TEST ONLY NEVER USED IN REALEASE NEED TO SHUT UP THE CODE ANALYSIS RULE
		internal static bool testDoubleClick;
#endif

		void AddEDocsButton_Click(object sender, System.EventArgs e)
		{
			ShowDialogAndAddValidFiles(true, string.Empty, (files) => PlugIn.Add(files));
		}

		public static void ShowDialogAndAddValidFiles(bool multiSelect, string initialFilename, Action<string[]> processSelected)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Multiselect = multiSelect;
				if (!string.IsNullOrEmpty(initialFilename))
				{
					dialog.FileName = initialFilename;
				}

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					string[] fileNames = dialog.ForceLocalFiles();
					AcceptableFileValidator fileValidator;
					try
					{
						fileValidator = new AcceptableFileValidator(false, fileNames);
					}
					catch (IOException ex)
					{
						Globals.Message.ShowError(ex.Message);
						return;
					}
					ZString message = fileValidator.GetInvalidFilesMessage();
					if (!message.IsEmpty)
					{
						Globals.Message.Show(message, Res.GetString("4f053c38-fe2a-4b60-b969-82a372795fac", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}

					var files = fileValidator.GetValidFiles();
					if (files != null && files.Length > 0)
					{
						processSelected(files);
					}
				}
			}
		}

		void GenerateNumberButton_Click(object sender, EventArgs e)
		{
			StorageMain storageMain = (CurrentDataItem as StorageMain);

			if (storageMain != null && storageMain.SM_PhysicalLocation.IsEmpty &&
				storageMain.DocManagerInfo.SupportsStorageNumberGeneration)
			{
				if (storageMain.DocManagerInfo.BusinessEntity != null &&
					storageMain.DocManagerInfo.BusinessEntity.IsInDatabase &&
					!storageMain.DocManagerInfo.BusinessEntity.HasChanges)
				{
					try
					{
						storageMain.Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
						storageMain.HasChanges = true;
						storageMain.Factory.Save();
					}
					catch (Exception ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
					finally
					{
						storageMain.Factory.Saving -= new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("7c69a0ee-22b4-448c-89d7-b58a6b655c63", "You must save this form before you can generate a storage number."));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("c368c174-70a7-4d4d-9c20-334b609ae3b6", "Storage location should be empty when generating a number."));
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			StorageMain storageMain = (CurrentDataItem as StorageMain);

			if (storageMain != null)
			{
				storageMain.SM_PhysicalLocation = storageMain.DocManagerInfo.GenerateStorageNumber();
			}
		}

		#endregion

		#region StorageDocsPanel Security Overlay

		void AddStorageDocsPanelSecurityOverlayLabel(string securityOverlayLabel)
		{
			StorageDocsGrid.Visible = false;
			DescriptionAndIconPanel.Visible = false;
			NotEditableLabel.Visible = false;
			if (storageDocsPanelSecurityOverlayLabel == null)
			{
				storageDocsPanelSecurityOverlayLabel = new ZLabel();
				storageDocsPanelSecurityOverlayLabel.Dock = DockStyle.Fill;
				storageDocsPanelSecurityOverlayLabel.BackColor = Color.Transparent;
				storageDocsPanelSecurityOverlayLabel.TextAlign = ContentAlignment.MiddleCenter;
				storageDocsPanelSecurityOverlayLabel.Text = securityOverlayLabel;
				this.DocStorageGroupBox.Controls.Add(storageDocsPanelSecurityOverlayLabel);
			}
			else
			{
				storageDocsPanelSecurityOverlayLabel.Text = securityOverlayLabel;
				storageDocsPanelSecurityOverlayLabel.Visible = true;
			}
		}

		void RemoveStorageDocsPanelSecurityOverlayLabel()
		{
			if (storageDocsPanelSecurityOverlayLabel != null)
			{
				storageDocsPanelSecurityOverlayLabel.Visible = false;
				StorageDocsGrid.Visible = true;
				DescriptionAndIconPanel.Visible = true;
			}
		}

		ZLabel storageDocsPanelSecurityOverlayLabel;

		void RefreshStorageDocsPanelSecurityOverlayLabel()
		{
			var currentParent = RelatedParentsGrid.ListManager?.GetCurrent() as StorageMain;

			if (currentParent != null)
			{
				var securityOverlayLabel = ZString.Empty;
				var edocsSecurity = currentParent.DocumentOwner as IEDocsSecurity;
				if (edocsSecurity != null)
				{
					securityOverlayLabel = !edocsSecurity.EdocsSecurityCheckpoint.IsAllowed
						? edocsSecurity.EdocsSecurityCheckpoint.ErrorMessageForNotAllowed
						: string.Empty;
				}

				if (securityOverlayLabel.IsEmpty)
				{
					RemoveStorageDocsPanelSecurityOverlayLabel();
				}
				else
				{
					AddStorageDocsPanelSecurityOverlayLabel(securityOverlayLabel);
				}
			}
		}

		#endregion

		#region IReadOnlyToggleControl Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				AddEDocsButton.ReadOnly = value;
				RefreshButton.ReadOnly = value;
				StorageDocsGrid.IsEditable = !value;
			}
		}
		bool readOnly;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsFixedReadOnly
		{
			get { return false; }
			set { Globals.Message.ShowDeveloperErrorOnce("eDocsUserControl.IsFixedReadOnly", "IsFixedReadOnly setter is not implemented", "IsFixedReadOnly"); }
		}

		#endregion

		#region Dispose

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "previewableDocument")]
		protected override void Dispose(bool isNotFinalizing)
		{
			previewableDocument?.Dispose();
			base.Dispose(isNotFinalizing);
		}

		#endregion

#if DEBUG
		public int StorageDocsListManager_CurrentChangedHints;
#endif

		#region Dispose

		void DisposeIcon()
		{
			if (IconPictureBox.Image != null)
			{
				IconPictureBox.Image.Dispose();
				IconPictureBox.Image = null;
			}
		}

		#endregion
	}
}
