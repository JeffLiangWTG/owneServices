using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.GUI
{
	[SuppressCheckControlModuleId]
	public partial class ZAllocateDocumentsForm : ZForm
	{
		public ZAllocateDocumentsForm(AllocateDocumentsManager dataSource)
			: base(dataSource)
		{
			InitializeComponent();

			DisposableLeakListener.Instance.RegisterDisposable(this);
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			Manager.NotifyNonPreviewableFilesNotAdded += Manager_NotifyNonTifFilesNotAdded;
			Manager.DocumentPermanentlyDeleted += new EventHandler<FilePermanentlyDeletedEventArgs>(Manager_DocumentPermanentlyDeleted);
			Manager.NotifyPermanentlyDeletingDocumentsInProgress += LoadDeletePermanentlyProgressForm;
			Manager.NotifyPermanentlyDeletingDocumentsComplete += CloseDeletePermanentlyProgressForm;

#if !WINZOR
			InitialisePreviewTab();
#endif
			InitialiseUnallocatedTab();
			InitialiseAllocatedTab();
		}

#if !WINZOR
		void InitialisePreviewTab()
		{
			fileBrowserControl.FileSelected += (o, e) => UpdatePreviewPane();
			fileBrowserControl.AddFileMenuItem(new ZMenuItem(ResString.GetMultilingualString("d300687f-e33e-4280-8762-925b02da3406", "Import Selected Files"), (o, e) => ImportSelectedFiles()));
		}

		void ImportSelectedFiles()
		{
			var selectedFiles = fileBrowserControl.SelectedFiles.ToList();
			if (selectedFiles.Count > 0)
			{
				var importer = Manager.FileImporterForImport;
				ImportFiles(importer, failedImports => importer.ImportFiles(selectedFiles, failedImports), selectedFiles.Count);
			}
			else
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("25ee69da-1b13-4cfb-b936-f41a7729fbd4", "Please select something to import"));
			}
		}
#endif

		void InitialiseUnallocatedTab()
		{
			UnallocatedGridControl.Grid.ShowSplitDocumentMenuItem = true;
			UnallocatedGridControl.Grid.DragDropTarget = Manager;
			UnallocatedGridControl.Grid.DocumentManipulationTarget = Manager;
			UnallocatedGridControl.BindingFinished += new EventHandler(GridControl_BindingFinished);
			UnallocatedGridControl.Grid.RebuildContextMenu();

#if !WINZOR
			importBoundButton2.Click += ImportBoundButton_Click;
			importConfigurationBoundButton2.Click += ImportConfigurationBoundButton_Click;
#endif
		}

		void InitialiseAllocatedTab()
		{
			AllocatedGridControl.Grid.DragDropTarget = Manager;
			AllocatedGridControl.Grid.DocumentManipulationTarget = Manager;
			AllocatedGridControl.BindingFinished += new EventHandler(GridControl_BindingFinished);
			AllocatedGridControl.Grid.RebuildContextMenu();
		}

		#region GUI Setup

		public override string FormVerb
		{
			get => "";
		}

		public override void ShowDataMagicForm()
		{
			ShowDataMagicForm(((INeedDataSet)Manager.UnallocatedDocuments).Data);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
			UpdatePreviewPane();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);

				if (PreviewPaneImageManager != null)
				{
					PreviewPaneImageManager.Dispose();
				}

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Key Management

		protected override void OnKeyDown(KeyEventArgs e)
		{
			e.Handled = ProcessAltA(e.KeyData);
			base.OnKeyDown(e);
		}

		protected internal bool ProcessAltA(Keys keyData)
		{
			if (keyData == (Keys.Alt | Keys.A) && !Manager.ReadOnly)
			{
				AllocateDocuments();
				return true;
			}
			return false;
		}

		#endregion

		#region Form Events

		void AllocateBoundButton_Click(object sender, EventArgs e)
		{
			AllocateDocuments();
		}

		void AllocateDocuments()
		{
			Cursor.Current = Cursors.WaitCursor;
			Manager.NotifyNoAllocationsDone += new EventHandler(Manager_NotifyNoAllocationsDone);
			try
			{
				Manager.AllocateDocuments();
			}
			catch (CanNotCreateSDDatabaseException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
			Manager.NotifyNoAllocationsDone -= new EventHandler(Manager_NotifyNoAllocationsDone);
		}

		void CreateNewJobButton_Click(object sender, EventArgs e)
		{
			if (UnallocatedGridControl.Grid.SelectedRowCount == 0)
			{
				Globals.Message.Show(Res.GetString("a7de2f60-f8e9-4429-b495-444130d4451a", "Please select the document you want to create new job for."));
			}
			else
			{
				var selectedDocuments = UnallocatedGridControl.Grid.GetSelectedElements<StorageDocsBase>();
				var jobTypes = selectedDocuments.Select(p => p.SM_Type).Distinct().ToList();
				if (jobTypes.Count == 0)
				{
					Globals.Message.Show(Res.GetString("93a87d01-da03-443f-b52e-72ced9634a86", "Please specify the job type on documents before creating new job."));
				}
				else if (jobTypes.Count > 1)
				{
					Globals.Message.Show(Res.GetString("636c0c2a-cc9e-4e4f-bfa3-5d36c58172f7", "Cannot create new job because selected documents have different job types."));
				}
				else
				{
					OpenCreateNewBizOFormAndGetParentPK(selectedDocuments);
				}
			}
		}

		void OpenCreateNewBizOFormAndGetParentPK(StorageDocsBase[] documents)
		{
			var moduleData = AssemblyDataLookup.AllAssemblyData.GetAssemblyDataFromDocManagerCode(documents[0].SM_Type);
			if (moduleData == null || moduleData.ModuleID == null || moduleData.ModuleID == ModuleIDs.NotAssigned)
			{
				Globals.Message.Show(Res.GetString("4630B807-B115-4F41-98F2-15CF6818BC73", "Cannot create new job because selected documents has an invalid job type."));
			}
			else
			{
				var module = (ZFilterModule)ZModuleFactory.Instance.Create(moduleData.ModuleID);
				var form = ((IShowNewForm)module).ShowNewForm();
				if (form != null)
				{
					form.Closed += delegate(object sender, EventArgs e) { SetParentIDIfJobIsSaved(sender, documents); };
				}
			}
		}

		void SetParentIDIfJobIsSaved(object sender, StorageDocsBase[] documents)
		{
			var bizO = ((ZForm)sender).BusinessEntity as BusinessObject;
			if (bizO != null && bizO.IsInDatabase)
			{
				foreach (var document in documents)
				{
					document.SC_ParentID = bizO.PK;
				}
			}
		}

		void UnallocateBoundButton_Click(object sender, EventArgs e)
		{
			DocumentsZGrid grid = AllocatedGridControl.Grid;
			DisableUpdatePreviewPane = true;
			grid.UnallocateDocuments(grid.SelectedElements);
			DisableUpdatePreviewPane = false;
			UpdatePreviewPane();
		}

		bool DisableUpdatePreviewPane;

		bool CanUpdatePreviewPane
		{
			get
			{
				return !DisableUpdatePreviewPane && !AllocatedGridControl.Grid.IsPerformingBulkOperation && !UnallocatedGridControl.Grid.IsPerformingBulkOperation;
			}
		}

		#endregion

		#region Scan From Device

#if !WINZOR
		void ShowAccessDeniedMessage()
		{
			Globals.Message.Show(Res.GetString("7b71ae6f-c5e4-43e7-91ed-5929e442432e", "You don't have the security permissions to access this feature. Please see your system administrator if you require access."));
		}
#endif

		#endregion

		#region Permanently Delete Progress Form

		ProgressForm permanentlyDeleteProgressForm;

		void Manager_DocumentPermanentlyDeleted(object sender, FilePermanentlyDeletedEventArgs e)
		{
			permanentlyDeleteProgressForm.PercentComplete = e.Total > 0 ? Math.Min(e.Completed * 100 / e.Total, 100) : 0;
		}

		internal void LoadDeletePermanentlyProgressFormForTesting(ProgressForm form)
		{
			permanentlyDeleteProgressForm = form;
			LoadDeletePermanentlyProgressForm(this, EventArgs.Empty);
		}

		void LoadDeletePermanentlyProgressForm(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			permanentlyDeleteProgressForm = new ProgressForm();
			permanentlyDeleteProgressForm.ShowProgressBar = true;
			permanentlyDeleteProgressForm.Status = Res.GetString("BD3F95CD-7CA8-419D-B9E7-500CD44A56C2", "Permanently deleting selected eDocs ...");
			ZFormModaliser.Show(permanentlyDeleteProgressForm, this);

			permanentlyDeleteProgressForm.Invalidate();
			permanentlyDeleteProgressForm.Update();
		}

		void CloseDeletePermanentlyProgressForm(object sender, EventArgs e)
		{
			permanentlyDeleteProgressForm.Close();
			permanentlyDeleteProgressForm.Dispose();

			Cursor.Current = Cursors.Default;
			OnNewDocumentSelected();
		}

		#endregion

		#region Import progress form

#if !WINZOR
		ProgressForm ProgressForm;

		void Manager_SingleFileImported(object sender, FileImportedEventArgs e)
		{
			ProgressForm.PercentComplete = Math.Min(e.Completed * 100 / e.Total, 100);
		}

		internal void Manager_SingleFileImportedForTesting(ProgressForm progressForm)
		{
			this.ProgressForm = progressForm;
			Manager_SingleFileImported(this, new FileImportedEventArgs(111, 100));
		}

		void ProgressForm_Cancelled(object sender, EventArgs e)
		{
			Manager.FileImporterForImport.ImportCancelled = true;
		}

		void ImportBoundButton_Click(object sender, EventArgs e)
		{
			Manager.FileImporterForImport.SingleFileImported += new FileImportedEventHandler(Manager_SingleFileImported);
			try
			{
				if (Env.Security.AllocateDocumentsModify.IsAllowed)
				{
					var importer = Manager.FileImporterForImport;
					importer.ImportCancelled = false;

					if (ZFormModaliser.ShowDialogAndDispose(importer.IsUsingCoverSheet ? new ImportDirectoryForm(importer) : new ImportDirectoryWithoutCoverSheetForm(importer)) == DialogResult.OK)
					{
						ImportBoundButtonCore(importer);
					}
				}
				else
				{
					ShowAccessDeniedMessage();
				}
			}
			finally
			{
				Manager.FileImporterForImport.SingleFileImported -= new FileImportedEventHandler(Manager_SingleFileImported);
			}
		}

		internal void ImportBoundButtonCore(FileImporter importer)
		{
			ImportFiles(importer, failedImports => importer.ImportFromDirectory(importer.DefaultImportDirectory, failedImports), importer.FileCount);
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Baseline")]
		void ImportFiles(FileImporter importer, Func<List<string>, int> importFunction, int totalFileCount)
		{
			Cursor.Current = Cursors.WaitCursor;
			var failedImports = new List<String>();
			int processedFileCount = 0;
			try
			{
				ProgressForm = new ProgressForm();
				ProgressForm.Cancelled += new EventHandler(ProgressForm_Cancelled);
				ProgressForm.ShowProgressBar = true;
				ProgressForm.ShowCancelButton = true;
				ProgressForm.Status = Res.GetString("4d8affcd-f9fc-44a4-8f49-1f2b38eac314", "Import Starting...");
				ZFormModaliser.Show(ProgressForm, this);
				Application.DoEvents();

				try
				{
					processedFileCount = importFunction(failedImports);
				}
				catch (CanNotCreateSDDatabaseException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
				finally
				{
					ProgressForm.Close();
					ProgressForm.Dispose();
				}
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ShowPostImportMessages(importer, processedFileCount, totalFileCount, failedImports);
				OnNewDocumentSelected();
			}
		}

		protected internal void ShowPostImportMessages(FileImporter importer, int processedFileCount, int totalFileCount, List<string> failedImports)
		{
			var msg = new ZStringBuilder();
			msg.AppendLine(Res.GetString("ed14c8b3-1419-4204-a53e-7dc3ba91647a", "Number of Import Files Processed: {0} of {1}", processedFileCount, totalFileCount));

			if (failedImports.Any())
			{
				failedImports.ForEach(a => msg.AppendLine(a));
				Globals.Message.ShowInformation(msg.ToString(), Res.GetString("4b58c98c-e90a-46f6-9f67-d56d9b4d6d95", "Import Finished With Errors"));
			}
			else
			{
				Globals.Message.ShowInformation(msg.ToString(), Res.GetString("720e4092-e05d-4002-a8e9-a5ba3412d616", "Import Finished"));
			}

			if (importer.FilesNotDeletedOnImport.Count > 0)
			{
				string[] filenamesNotDeleted = new string[importer.FilesNotDeletedOnImport.Count];
				importer.FilesNotDeletedOnImport.CopyTo(filenamesNotDeleted, 0);
				string fileList = new FileListFormatter().FormatListToString(filenamesNotDeleted, false);
				Globals.Message.ShowInformation(Res.GetString("8e0bc4bf-7e8b-4dd1-bace-0cce73ab6734", "The following files were not deleted when they were imported, because they were still open by other programs. \r\nYou will have to manually delete these files to remove them from the directory.\r\n{0}", fileList));
				importer.FilesNotDeletedOnImport.Clear();
			}
		}

		void ImportConfigurationBoundButton_Click(object sender, EventArgs e)
		{
			if (Env.Security.DocManagerImportConfiguration.IsAllowed)
			{
				ImportConfigurationForm configForm = new ImportConfigurationForm(Manager.FileImporterForConfigSettings);
				ZFormModaliser.Show(configForm, this);
			}
			else
			{
				ShowAccessDeniedMessage();
			}
		}
#endif

		#endregion

		#region Image Preview Pane

		ImageManager PreviewPaneImageManager;

		#region Preview Pane
		protected internal GraphicalDisplayControl PreviewPane
		{
			get
			{
				if (fPreviewPane == null)
				{
					fPreviewPane = new GraphicalDisplayControl(Manager.ReadOnly);
					fPreviewPane.Dock = DockStyle.Fill;
					fPreviewPane.DockInside(GraphicalDisplayControlPanel);
				}
				return fPreviewPane;
			}
		}
		GraphicalDisplayControl fPreviewPane;
		#endregion

		void UpdatePreviewPane()
		{
			if (CanUpdatePreviewPane)
			{
				PreviewPane.SuspendLayout();
				string filename = string.Empty;
				try
				{
					var selectedItem = GetSelectedItem();
					var documentType = AllocateEDocsHelper.GetPreviewableDocumentType(selectedItem);
					PreviewPane.SetThumbnailPanelContextMenuReadOnly(documentType);

					if (PreviewPaneImageManager != null)
					{
						PreviewPaneImageManager.Dispose();
					}

					if (documentType == PreviewableDocumentType.NoneFile)
					{
						PreviewPane.Close();
					}
					else
					{
						filename = selectedItem.SaveToTempFile();
						PreviewPaneImageManager = new ImageManager(filename, selectedItem, PreviewPane);
						PreviewPaneImageManager.Tag = selectedItem.PK;
						PreviewPaneImageManager.Disposing += new EventHandler(PreviewPaneImageManager_Disposing);
						PreviewPaneImageManager.MoveToNewDocument += new MoveToNewDocumentEventHandler(PreviewPaneImageManager_MoveToNewDocument);
						PreviewPaneImageManager.DocumentEmpty += new EventHandler(PreviewPaneImageManager_DocumentEmpty);
						PreviewPaneImageManager.ManagedFileChanged += new DocumentChangedEventHandler(PreviewPaneImageManager_ManagedFileChanged);
						PreviewPane.Document = selectedItem;
					}
				}
				catch (Exception ex)
				{
					ClearPreviewPane();

					if (ex.Find<IOException>() != null)
					{
						Globals.Message.ShowWarning(Res.GetString("8775b417-36fe-42b6-a2df-d4a47bf7b66e", "This file could not be read"));
					}
					else if (ex.Find<UnauthorizedAccessException>() != null)
					{
						Globals.Message.ShowWarning(Res.GetString("664ca5c9-efda-420f-a732-425122273190", "You do not have permission to open this file"));
					}
					else if (ex.Find<CorruptedDocumentException>() != null)
					{
						ShowCorruptedFileImageOnPreviewPane();
					}
					else
					{
						throw;
					}
				}
				finally
				{
					PreviewPane.ResumeLayout(false);
				}
			}
		}

		internal void ShowCorruptedFileImageOnPreviewPane()
		{
			var corruptedFileImageData = GetCorruptedFileImageData();
			if (corruptedFileImageData.Any())
			{
				var selectedItem = Manager.MasterFactory.New<StorageDocsUnallocated>();
				selectedItem.SC_DataType = "UNA";
				selectedItem.SC_ImageData = corruptedFileImageData;

				PreviewPaneImageManager = new ImageManager(selectedItem.SaveToTempFile(), selectedItem, PreviewPane) { Tag = selectedItem.PK };
				PreviewPane.Document = selectedItem;
			}

			PreviewPane.SetThumbnailVisible(false);
			PreviewPane.SetReadOnly(true);
		}

		[SuppressMessage("CargoWiseOne", "CW1120:DoNotGetIconsImagesFromRexOrResourcesFile", Justification = "This is DocumentScanning GUI control")]
		internal byte[] GetCorruptedFileImageData()
		{
			var resources = new ResourceManager(typeof(GraphicalDisplayControl));
			var corruptedFileImage = (Image)resources.GetObject("CorruptedFileImage"); // This is DocumentScanning GUI control

			using (var memoryStream = new MemoryStream())
			{
				corruptedFileImage?.Save(memoryStream, corruptedFileImage.RawFormat);
				return memoryStream.ToArray();
			}
		}

		StorageDocsBase GetSelectedItem()
		{
#if !WINZOR
			if (DocumentAllocationTabControl.SelectedTab == previewTabPage)
			{
				if (!string.IsNullOrEmpty(fileBrowserControl.SelectedFile))
				{
					var doc = Manager.MasterFactory.New<TempStorageDocs>();
					doc.SC_DataType = "UNA";
					doc.SC_FileName = Path.GetFileName(fileBrowserControl.SelectedFile);
					doc.SC_ImageData = File.ReadAllBytes(fileBrowserControl.SelectedFile);

					return doc;
				}
				return null;
			}
			else
#endif
			if (DocumentAllocationTabControl.SelectedTab == AllocatedDocumentsTabPage)
			{
				return AllocatedGridControl.Grid.CurrentElement;
			}
			else
			{
				return UnallocatedGridControl.Grid.CurrentElement;
			}
		}

		void OnNewDocumentSelected() => UpdatePreviewPane();

		void ClearPreviewPane()
		{
			if (PreviewPaneImageManager != null)
			{
				PreviewPaneImageManager.Dispose();
			}

			PreviewPaneImageManager = null;
			PreviewPane.Close();
		}

		void DocumentGrid_CurrentChanged(object sender, EventArgs e)
		{
			OnNewDocumentSelected();
		}

		void DocumentAllocationTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			ClearPreviewPane();
			OnNewDocumentSelected();
		}

		#endregion

		#region Notification events from Image Manager

		void Manager_NotifyNoAllocationsDone(object sender, EventArgs e)
		{
			Globals.Message.Show(Res.GetString("8e734146-d3a0-4ff0-8695-daa76a4f6dc7", "No eDocs were allocated. Please ensure that the Type, Doc Type, Description and Allocate To fields have been filled in and the eDoc has no errors."), Res.GetString("3e5fa63a-05e8-4b56-a1c1-2066f6f2dba2", "No eDocs Allocated"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		void Manager_NotifyNoFileSelectedForAppend(object sender, EventArgs e)
		{
			Globals.Message.Show(Res.GetString("44cf12ce-0449-43e0-93b3-750fb0748874", "Invalid option - no eDoc selected to append to. Please select an eDoc from the grid."), Res.GetString("61cfd572-a194-408d-903f-06cee8d4c69e", "No eDoc Selected"), MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		void Manager_NotifyNonTifFilesNotAdded(object sender, EventArgs e)
		{
			Globals.Message.Show(Res.GetString("7c9b550e-b100-4177-94a4-7f417627df5d", "You cannot add Non-TIF files here. Please add them directly to the eDocs tab of the relevant form instead."));
		}

		#endregion

		#region Image Manager events

		void PreviewPaneImageManager_Disposing(object sender, EventArgs e)
		{
			ImageManager manager = sender as ImageManager;
			manager.Disposing -= new EventHandler(PreviewPaneImageManager_Disposing);
			manager.MoveToNewDocument -= new MoveToNewDocumentEventHandler(PreviewPaneImageManager_MoveToNewDocument);
			manager.DocumentEmpty -= new EventHandler(PreviewPaneImageManager_DocumentEmpty);
			manager.ManagedFileChanged -= new DocumentChangedEventHandler(PreviewPaneImageManager_ManagedFileChanged);
		}

		void PreviewPaneImageManager_MoveToNewDocument(object sender, MoveToNewDocumentEventArgs e)
		{
			if (e.SerializableEDocs.Count > 0)
			{
				Manager.CreateNewDocument(e.SerializableEDocs[0]);
			}
		}

		void PreviewPaneImageManager_ManagedFileChanged(object sender, DocumentChangedEventArgs e)
		{
			ImageManager imgManager = (ImageManager)sender;
			Manager.UpdateDocumentImage((ZGuid)imgManager.Tag, imgManager.ManagedImageFilename, e);
		}

		void PreviewPaneImageManager_DocumentEmpty(object sender, EventArgs e)
		{
			ImageManager imgManager = (ImageManager)sender;
			Globals.Message.Show(Res.GetString("040c4f47-bf99-424c-9011-0b0e68381751", "This eDoc is now empty and will be deleted."), Res.GetString("0e29aeb1-a91b-4b96-a785-4e84757aa192", "Empty eDoc"), MessageBoxButtons.OK, MessageBoxIcon.None);
			Manager.DeleteDocument((ZGuid)imgManager.Tag);
		}

		#endregion

		#region Actions

		protected AllocateDocumentsManager Manager => (AllocateDocumentsManager)BusinessEntity;

		void GridControl_BindingFinished(object sender, EventArgs e)
		{
			ZGrid grid = ((BindableGridControl)sender).Grid;

			grid.FindBoxColumnModuleShowing += new EventHandler<FindBoxColumnModuleShowingEventArgs>(Grid_FindBoxColumnModuleShowing);

			grid.ListManager.CurrentChanged += new EventHandler(DocumentGrid_CurrentChanged);
			(grid as DocumentsZGrid).BulkOperationComplete += new EventHandler(ZAllocateDocumentsForm_BulkOperationComplete);
			if (grid.ListManager.Count > 0)
			{
				OnNewDocumentSelected();
			}
		}

		void ZAllocateDocumentsForm_BulkOperationComplete(object sender, EventArgs e) => UpdatePreviewPane();

		internal void Grid_FindBoxColumnModuleShowing(object sender, FindBoxColumnModuleShowingEventArgs e)
		{
			try
			{
				e.ModuleID = ((StorageDocsUnallocated)e.CurrentBusinessObject).ModuleID;
			}
			catch (NullReferenceException ex)
			{
				string eventValue = e != null ? e.ToString() : "NULL";
				string bizOValue = e == null ? (NoResString)"Not Accessible" : e.CurrentBusinessObject != null ? e.CurrentBusinessObject.ToString() : "NULL";
				string msg = string.Format((NoResString)"Message: {0}\r\n e: {1}\r\ne.CurrentBusinessObject: {2}", ex.Message, eventValue, bizOValue); // Error Report message.
				ErrorReporter.ReportOnce("NullRefOnFindBoxColumnModuleShowing", msg, ex);
			}
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			if (Manager.HasChanges)
			{
				DialogResult answer = Globals.Message.Show(Res.GetString("5f91c2bc-3948-4f0c-bfb6-9f7ec9efed5c", "This form needs to be saved before it can be reloaded. Save now?"), Res.GetString("910028dd-36fd-4564-a5d0-d0011721b71b", "Save Form"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);

				if (answer == DialogResult.Yes)
				{
					try
					{
						Manager.MasterFactory.Save();
						ReloadUnallocatedDocuments();
					}
					catch (ZSaveException ex)
					{
						HandleSaveException(ex);
					}
				}
			}
			else
			{
				ReloadUnallocatedDocuments();
			}
		}

		void ReloadUnallocatedDocuments()
		{
			Manager.MasterFactory.ClearQueryCache();
			Manager.UnallocatedDocuments.Load();
		}

		protected override void OnDragDrop(DragEventArgs e) => AllocatedGridControl.Grid.OnDragDropForRemote(e);

		#endregion

		void filterButton_Click(object sender, EventArgs e) => Manager.LoadUnallocatedDocumentsWithFiltering();

		void clearButton_Click(object sender, EventArgs e) => Manager.ClearUnallocatedDocumentsFilterValues();
	}
}
