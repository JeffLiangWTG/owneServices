using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Interop.DataObjects;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.PrintProcessing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using Exception = System.Exception;
using Res = Enterprise.DocumentScanning.GUI.Res;
namespace Enterprise.DocumentScanning.PlugIn
{
	public class eDocsPlugIn :
		ZPlugInWithDragDropSupport,
		IDocumentManipulationSupport,
		IDragDropSupport,
		IEDocsPlugIn
	{
		public eDocsPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
			//ShowOverwriteOrCreateNewPrompt("something.txt");

			threadSentry = ThreadSentryProvider.GetThreadSentry(true, new eDocsPluginThreadSentryHelper(), allowChangingThreadOwnership: false);
		}

		#region IEDocsPlugIn

		public void ForceSetup()
		{
			Setup();
		}

		#endregion

		#region Delete

		public void DeleteDocument(ZGuid pK)
		{
			if (TopLevelParentMain.eDocs.FindByPK(pK) is StorageDocsBase bizOToDelete)
			{
				var reference = DocumentLogReferenceHelper.GetReference(bizOToDelete, Events.DocumentDeletedPermanently);
				var isInDatabase = bizOToDelete.IsInDatabase;
				var parentMain = bizOToDelete.ParentMain;
				TopLevelParentMain.eDocs.RemoveAndDelete(bizOToDelete);
				AddLogForDocument(reference, isInDatabase, parentMain, Events.DocumentDeletedPermanently);
			}
		}

		#endregion

		#region Document Image

		public void UpdateDocumentImage(ZGuid pK, string filename, DocumentChangedEventArgs args)
		{
			StorageDocs requiredDoc = TopLevelParentMain.Documents.FindByPK(pK) as StorageDocs;
			if (requiredDoc != null)
			{
				requiredDoc.SC_ImageData = new ZBlob(DocumentUtilities.GetFileAsBytes(filename));
			}
		}

		#endregion

		#region Add

		/// <summary>
		/// Add new eDocs to the plugin. Any image files will be converted to TIF format.
		/// </summary>
		/// <returns>The added eDocs</returns>
		public Dictionary<string, StorageDocsBase> Add(string[] filesToAdd)
		{
			EnsureCurrentThreadIsOwnerOfPlugin();

			var resultDictionary = new Dictionary<string, StorageDocsBase>(filesToAdd.Length);
			var main = GetStorageMainForAddingEDocs();
			main.Files.AskForOverwriteOrCreateNew += new OverwriteOrCreateNewEventHandler(Files_AskForOverwriteOrCreateNew);
			AcceptableFileValidator fileValidator;
			try
			{
				fileValidator = new AcceptableFileValidator(false, filesToAdd);
			}
			catch (IOException ex)
			{
				Globals.Message.ShowError(ex.Message);
				return resultDictionary;
			}
			var files = fileValidator.GetValidFiles();
			var duplicates = new List<string>();

			using (EDocsPluginAttachMonitor.StartMonitorEDocAttachment(this))
			{
#if DEBUG
				IsMonitoringDocumentAttachmentForDebug = true;
#endif
				foreach (var filename in files)
				{
					try
					{
						if (resultDictionary.ContainsKey(filename))
						{
							duplicates.Add(filename);
						}
						else
						{
							resultDictionary.Add(filename, AddFileToPlugIn(filename));
						}
					}
					catch (VirusDetectedException ex)
					{
						Globals.Message.ShowError(ex.VirusDetectedFriendlyMessage);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.Show(Res.GetString("3b4b92e9-3fa5-4189-b808-be31cb58c20d", "The file {0} could not be added. Check that it is present, valid and accessible.\r\nError Message: {1}\r\n{2}", filename, ex.Message, GetHumanReadableList(new[] { ex is FileAccessException ? ((FileAccessException)ex).Filename : "" }, false)));
					}
				}
			}
			if (duplicates.Count > 0)
			{
				Globals.Message.Show(Res.GetString("7110687c-c043-4c7f-8e1a-b91feb9093f7", "You cannot add files with similar filenames. Following files were added only once:") + "\r\n" + string.Join(", ", duplicates.ToArray()));
			}

			ShowInvalidFilesMessage(fileValidator.GetInvalidFilesMessage());
			main.Files.AskForOverwriteOrCreateNew -= new OverwriteOrCreateNewEventHandler(Files_AskForOverwriteOrCreateNew);

			return resultDictionary;
		}

		string GetHumanReadableList(string[] filenames, bool showFileNameOnly)
		{
			return new FileListFormatter().FormatListToString(filenames, showFileNameOnly);
		}

		void ShowInvalidFilesMessage(ZString message)
		{
			if (!message.IsEmpty)
			{
				Globals.Message.Show(message, Res.GetString("3428b3eb-94ec-485b-aad6-3a8ddd640629", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		void Files_AskForOverwriteOrCreateNew(object sender, Business.FilenameEventArgs e)
		{
			e.Action = ShowOverwriteOrCreateNewPrompt(e.Filename);
		}

		protected virtual FileAction ShowOverwriteOrCreateNewPrompt(string filename)
		{
			string filenameOnly = Path.GetFileName(filename);
			string prompt = Res.GetString("a24bd16f-a8cd-4bf1-b9c1-925985e8df2f", "The file '{0}' already exists on this form. Do you want to overwrite the existing file, or create a new file?", filenameOnly);
			using (OverwriteOrCreateNewForm form = new OverwriteOrCreateNewForm(prompt))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				return form.FileActionResult;
			}
		}

		StorageDocsBase AddFileToPlugIn(string filename)
		{
			StorageDocsBase result;

			var contents = DocumentUtilities.GetFileAsBytes(filename);
			var filenameOnly = Path.GetFileName(filename);
			var main = GetStorageMainForAddingEDocs();
			var addedDocument = main.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = filenameOnly,
				ShouldSupersedeOlderVersion = false,
			});

			if (addedDocument.SC_FileName.IsEmpty)
			{
				addedDocument.SC_FileName = filenameOnly;
			}

			if (IsMonitoringDocumentAttachment && DocumentConfigApplyingToAll != null)
			{
				addedDocument.SC_DocType = DocumentConfigApplyingToAll.SC_DocType;
				addedDocument.SC_Desc = DocumentConfigApplyingToAll.SC_Desc;
				addedDocument.SC_IsPublished = DocumentConfigApplyingToAll.SC_IsPublished;
				if (DocumentConfigApplyingToAll.IsCompanyBranchDepartmentSpecific)
				{
					addedDocument.IsCompanyBranchDepartmentSpecific = DocumentConfigApplyingToAll.IsCompanyBranchDepartmentSpecific;
					addedDocument.CompanyCode = DocumentConfigApplyingToAll.CompanyCode;
					addedDocument.BranchCode = DocumentConfigApplyingToAll.BranchCode;
					addedDocument.DepartmentCode = DocumentConfigApplyingToAll.DepartmentCode;
				}

				if (!string.IsNullOrWhiteSpace(Path.GetExtension(filename)))
				{
					AddLogForNewDocument(addedDocument);
				}
				else
				{
					ShowEditForm(addedDocument);
				}
			}
			else
			{
				if (UnattendedConfigProvider == null || !UnattendedConfigProvider.ApplyDocumentConfig(addedDocument))
				{
					ShowEditForm(addedDocument);
				}
			}

			if (!addedDocument.IsDeleted)
			{
				AddRequiredDocument(addedDocument);
				result = addedDocument;
			}
			else
			{
				result = null;
			}

			return result;
		}

		StorageMain GetStorageMainForAddingEDocs() => !IsUserControlVisible || IsReadOnly || dragDropFileData is DocManagerDataObject ? TopLevelParentMain : CurrentParentMainOnGrid;

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (topLevelParentMain != null)
				{
					if (topLevelParentMain.IsFilesLoaded)
					{
						topLevelParentMain.Files.DisposeAll();
					}
					topLevelParentMain.HandlingUniqueIndexFailure -= new EventHandler<StorageMainUniqueIndexViolationEventArgs>(TopLevelParentMain_HandlingUniqueIndexFailure);
					topLevelParentMain = null;
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Plugin Overrides

		public override string Name
		{
			get { return "eDocs"; }
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		public BusinessObject HostBusinessObject
		{
			get { return (BusinessObject)HostBusinessEntity; }
		}

		DocManagerInfo DocManagerInfo
		{
			get
			{
				IDocManagerSupport hostDocManagerSupport = HostBusinessObject as IDocManagerSupport;
				if (fDocManagerInfo == null && hostDocManagerSupport != null)
				{
					fDocManagerInfo = hostDocManagerSupport.DocManagerInfo;
					if (fDocManagerInfo != null)
					{
						fDocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
					}
				}
				return fDocManagerInfo;
			}
		}

		DocManagerInfo fDocManagerInfo;

		protected override Control GetNewUserControl()
		{
			eDocsUserControl control = new eDocsUserControl(this);
			control.ReadOnly = eDocsControlReadOnly;
			return control;
		}

		bool eDocsControlReadOnly => DocManagerInfo?.ReadOnly ?? HostBusinessObject?.ReadOnly ?? false;

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.DocManager; }
		}

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			TopLevelParentMain.SetReadOnlyIncludingChildren(eDocsControlReadOnly);
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		void RenewUserControl(StorageMain newTopLevel, StorageMain oldStorage)
		{
			if (newTopLevel != null)
			{
				try
				{
					if (topLevelParentMain != null)
					{
						topLevelParentMain.HandlingUniqueIndexFailure -= new EventHandler<StorageMainUniqueIndexViolationEventArgs>(TopLevelParentMain_HandlingUniqueIndexFailure);
					}

					topLevelParentMain = newTopLevel;
					topLevelParentMain.IsTopLevelParent = true;
					topLevelParentMain.HasChanges = false;

					if (!topLevelParentMain.HandlingUniqueIndexFailureIsBinded)
					{
						topLevelParentMain.HandlingUniqueIndexFailure += new EventHandler<StorageMainUniqueIndexViolationEventArgs>(TopLevelParentMain_HandlingUniqueIndexFailure);
					}

					CurrentParentMainOnGrid = newTopLevel;
					SelectTabPage();
					TabPage.ClearNotificationImage();
					ResetBusinessEntityToNull();

					Enabled = false;
					Enabled = true;
					SelectTabPage();
					oldStorage.Delete();
					oldStorage.DocManagerInfo.ResetStorageMainIfNeeded();
					SynchroniseIfTabPageVisible();
					DiscardCurrentUserControl();
					storageHookedUp = false;
					UserControl.Refresh();
					SynchroniseIfTabPageVisible();
				}
				finally
				{
					HostBusinessEntity.HasChanges = true;
				}
			}
		}

		public override void OnGUIShown()
		{
			base.OnGUIShown();
			if (!storageHookedUp)
			{
				if (!TabPage.Controls.Contains(UserControl))
				{
					TabPage.AddPlugInUserControl();
				}
				((eDocsUserControl)UserControl).SetDataBinding(topLevelParentMain, "");
				storageHookedUp = true;
			}
		}

		bool storageHookedUp = true;

		protected override ZTabPagePlugIn GetTabPage()
		{
			return new ZAutoSizedTabPagePlugIn(this);
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return TopLevelParentMain;
		}

		protected override bool RegisterPlugInBusinessEntityAsEditable
		{
			get { return true; }
		}

		protected override ZBool AllowPlugInDisplayWithNoLicence
		{
			get { return true; }
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			bool result = false;

			if (TopLevelParentMain != null)
			{
				if (!TopLevelParentMain.LoadFailureReason.IsEmpty)
				{
					var dbHelper = new DocManagerDBHelper();
					NotDisplayedMessage = Res.GetString("{A20D8F68-9386-48ba-A94F-95F5F9390299}",
						@"The eDocs tab is not currently available due to an error retrieving data.
Please inform your System Administrator and have them ensure the eDoc Databases have been correctly restored.

Most likely the main database {0} has been renamed or copied from elsewhere and has existing document data in it.
To fix the error, rename or copy all other existing databases with the prefix {0}_SDXX.

Error Message: {1}{2}", dbHelper.GetDatabaseName(0), TopLevelParentMain.LoadFailureReason, Env.Instance.IsProductionSystem ? "" : "\r\n\r\n" + Res.GetString("82ce2858-4826-43b6-a622-cde993bdb9b7", "Please make sure that the eDoc databases have been restored correctly."));
				}
				else if (TopLevelParentMain.SM_DB != 0)
				{
					result = true;
				}
			}

			return result;
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return NotDisplayedMessage; }
		}

		protected string NotDisplayedMessage = Res.GetString("eDocsPlugIn|NotDisplayedMessage", "The eDocs tab is not available.");

		#endregion

		#region Related Business Objects

		#region MasterFactory

		public DocumentFactory MasterFactory
		{
			get
			{
				if (masterFactory == null)
				{
					if (DocManagerInfo != null)
					{
						masterFactory = (DocumentFactory)DocManagerInfo.MasterFactory;
					}

					if (masterFactory == null)
					{
						masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
					}
				}
				return masterFactory;
			}
		}

		protected DocumentFactory masterFactory;

#endregion

		#region CurrentParentMainOnGrid

		public StorageMain CurrentParentMainOnGrid
		{
			get { return currentParentMainOnGrid ?? TopLevelParentMain; }
			set { currentParentMainOnGrid = value; }
		}

		StorageMain currentParentMainOnGrid;

		#endregion

		#region TopLevelParentMain

		protected StorageMain TopLevelParentMain
		{
			get
			{
				if ((topLevelParentMain == null || topLevelParentMain.IsDeleted) && !hadExceptionGettingTopLevelParentMain)
				{
					if (DocManagerInfo != null && !DocManagerInfo.DocManagerCode.IsEmpty && AssemblyDataLookup.IsDocManagerCodeValid(DocManagerInfo.DocManagerCode))
					{
						try
						{
							topLevelParentMain = MasterFactory.RetrieveExistingOrCreateStorageMainForPK(HostBusinessObject.PK, HostBusinessObject, DocManagerInfo.DocManagerCode);
							topLevelParentMain.IsTopLevelParent = true;
							topLevelParentMain.HasChanges = false; // avoid enabling Save & Close buttons until there is a change to child collections.

							if (!topLevelParentMain.HandlingUniqueIndexFailureIsBinded)
							{
								topLevelParentMain.HandlingUniqueIndexFailure += new EventHandler<StorageMainUniqueIndexViolationEventArgs>(TopLevelParentMain_HandlingUniqueIndexFailure);
							}

							if (topLevelParentMain.SM_Type != DocManagerInfo.DocManagerCode)
							{
								topLevelParentMain.SM_TypeOverride = DocManagerInfo.DocManagerCode;
							}
						}
						catch (EDocsOffLineException ex)
						{
							NotDisplayedMessage = Res.GetString("{CD44DA6E-4800-41f7-A118-6E3858061971}", "There is an error with the Database Server. Please inform your System Administrator of this problem.\r\n\r\nError : {0}", ex.Message);
							hadExceptionGettingTopLevelParentMain = true;
						}
						catch (Exception ex) when (ex is CanNotCreateSDDatabaseException || ex is CanNotAcquireLockForCreatingSDDatabaseException)
						{
							NotDisplayedMessage = Res.GetString("{9E843BD4-0642-4E89-9168-6CDE544440EC}", @"The eDocs tab is not currently available, see below message:
{0}", ex.Message);
							Globals.Message.ShowError(ex.Message);
						}
					}
				}
				return topLevelParentMain;
			}
		}

		protected StorageMain topLevelParentMain;

		protected bool hadExceptionGettingTopLevelParentMain;

		void TopLevelParentMain_HandlingUniqueIndexFailure(object sender, StorageMainUniqueIndexViolationEventArgs e)
		{
			RenewUserControl(e.StorageMainInDB, e.StorageMainInMemory);
		}

		bool IsTopLevelParentMainLoaded
		{
			get { return topLevelParentMain != null; }
		}

		public bool IsTopLevelParentSelected
		{
			get { return CurrentParentMainOnGrid == TopLevelParentMain; }
		}

		public void Reload()
		{
			if (HostBusinessObject.IsInDatabase)
			{
				TopLevelParentMain.RunPreSaveValidation();
				if (!TopLevelParentMain.HasErrors)
				{
					TopLevelParentMain.MasterFactory.Save();
				}
				else
				{
					Globals.Message.Show(Res.GetString("1F94E5E7-90D1-4174-B422-754446D58EFC", "Documents could not be saved because there are validation errors on eDocs form. Please fix them before saving again."),
						Res.GetString("3F5EC6F5-BC29-4E16-826E-CBE4F9FF172A", "Reload EDocs"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				ReloadWithoutSave();
			}
		}

		public void ReloadWithoutSave()
		{
			TopLevelParentMain.MasterFactory.ClearQueryCache();
			TopLevelParentMain.eDocs.Factory.ClearQueryCache();
			TopLevelParentMain.RequireReload();

			TopLevelParentMain.RelatedParentMains.RemoveAll();
			TopLevelParentMain.RelatedParentMains.AddRange(TopLevelParentMain.LoadRelatedParentMains(true));
		}

		#endregion

		#endregion

		#region Save Dialogs

		protected override bool ShowPreSaveDialogsWhenInactive
		{
			get { return true; }
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes)
			{
				result = EnsureDocumentsNotOpen();
				if (result == ContinueWithSave.Yes)
				{
					result = EnsureRelatedDocumentsRead();
				}
			}
			return result;
		}

		ContinueWithSave EnsureDocumentsNotOpen()
		{
			ContinueWithSave result = ContinueWithSave.Yes;

			if (!IsTopLevelParentMainLoaded || TopLevelParentMain == null)
			{
				return result;
			}

			StringBuilder humanReadableList = (new StringBuilder()).AppendLine();
			int initialLength = humanReadableList.Length;
			foreach (StorageMain relatedMain in topLevelParentMain.RelatedParentMains)
			{
				if (relatedMain.Files == null)
				{
					continue;
				}

				string[] filesUnableToSave = relatedMain.Files.FilesOpen;

				if (filesUnableToSave.Length > 0)
				{
					humanReadableList.Append(GetHumanReadableList(filesUnableToSave, true));
				}
			}
			if (humanReadableList.Length > initialLength)
			{
				bool response = Globals.Message.Show(
					Res.GetString("02c9899d-9266-482a-a3bf-b869fe69820f", "The following files are still open. If they have unsaved changes, they will be lost. Continue with save anyway?\r\n{0}", humanReadableList),
					Res.GetString("72482f28-a480-40a5-b860-7fe7b9d41168", "Cannot save form"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK;
				result = response ? ContinueWithSave.Yes : ContinueWithSave.No;

				if (response && topLevelParentMain != null)
				{
					//unset HasChanges on files that are still open
					foreach (var relatedMain in topLevelParentMain.RelatedParentMains.OfType<StorageMain>().Where(x => x.Files != null))
					{
						foreach (var file in relatedMain.Files.OfType<StorageFile>().Where(x => x.IsTempFileOpen).ToList())
						{
							if (file.HasChanges)
							{
								file.HasChanges = false;
								file.ReloadSafe();
							}
						}
					}
				}
			}
			return result;
		}

		ContinueWithSave EnsureRelatedDocumentsRead()
		{
			ContinueWithSave result = ContinueWithSave.Yes;
			if (DocManagerInfo != null && DocManagerInfo.HasUnreadRelatedDocuments)
			{
				if (ShowUnreadDocumentsDialog())
				{
					SelectTabPage();
					result = ContinueWithSave.No;
				}
				else
				{
					HostBusinessObject.GetLogs().AddNew(Events.RelatedEDocsNotRead);
					result = ContinueWithSave.Yes;
				}
			}
			return result;
		}

		bool ShowUnreadDocumentsDialog()
		{
			string message = Res.GetString("1dc10bd5-ebec-4a90-a1bc-609accd5e497", "Unread eDocs exist for this {0}. Read these documents before saving?",
				HostBusinessObject.HumanReadableName);

			using (ZMessageBox msgBox = new ZMessageBox(message, Res.GetString("a6c17cea-c670-44b8-ad5a-ea90fa28c75c", "Important!"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
			{
				msgBox.SetCustomIcon(Icons.GetIcon(IconTypes.eDocsToReadLarge));
				return ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox) == DialogResult.Yes;
			}
		}

		#endregion

		#region IDocumentManipulationSupport Members

		void IDocumentManipulationSupport.DeleteDocumentsQuietly(ICollection<BusinessObject> documents)
		{
			foreach (StorageDocsBase document in documents)
			{
				var reference = DocumentLogReferenceHelper.GetReference(document, Events.DocumentDeleted);
				var isInDatabase = document.IsInDatabase;
				var parentMain = document.ParentMain;
				document.DeleteQuietly();
				AddLogForDocument(reference, isInDatabase, parentMain, Events.DocumentDeleted);
			}
		}

		void IDocumentManipulationSupport.RestoreDocuments(ICollection<BusinessObject> documents)
		{
			foreach (StorageDocsBase document in documents)
			{
				var reference = DocumentLogReferenceHelper.GetReference(document, Events.DocumentRestored);
				document.Restore();
				AddLogForDocument(reference, document.IsInDatabase, document.ParentMain, Events.DocumentRestored);
			}
		}

		IEnumerable<BusinessObject> IDocumentManipulationSupport.AllocateDocuments(IEnumerable<BusinessObject> list)
		{
			// Do not allow allocation of eDocs in eDocsPlugin.
			throw new NotImplementedException();
		}

		ICollection<BusinessObject> IDocumentManipulationSupport.DeleteDocumentsPermanently(ICollection<BusinessObject> documents)
		{
			foreach (StorageDocsBase document in documents)
			{
				var reference = DocumentLogReferenceHelper.GetReference(document, Events.DocumentDeletedPermanently);
				var isInDatabase = document.IsInDatabase;
				var parentMain = document.ParentMain;
				document.Delete();
				AddLogForDocument(reference, isInDatabase, parentMain, Events.DocumentDeletedPermanently);
			}

			return Array.Empty<BusinessObject>();
		}

		IEnumerable<BusinessObject> IDocumentManipulationSupport.UnallocateDocuments(IEnumerable<BusinessObject> documents)
		{
			// Do not allow unallocation of eDocs in eDocsPlugin.
			throw new NotImplementedException();
		}

		#endregion

		#region IDragDropSupport Members

		void IDragDropSupport.Add(SerializableEDocCollection collection)
		{
			if (collection != null)
			{
				foreach (SerializableEDoc docFromClipboard in collection)
				{
					if (docFromClipboard == null)
					{
						continue;
					}

					var newDocument = docFromClipboard.ToBusinessObject(GetStorageMainForAddingEDocs(), true, true);

					if (newDocument.SC_DocTypeInfo.HasErrors()) // drag-drop from a different BizO that doesn't have this doctype registered
					{
						ShowEditForm(newDocument);
					}
					else
					{
						AddLogForNewDocument(newDocument);
					}
				}
			}
		}

		void ShowEditForm(StorageDocsBase newDocument)
		{
#if DEBUG
			if (!Globals.IsTest || shouldShowEditFormForTest)
			{
#endif
				ZChildForm form = null;

				var main = GetStorageMainForAddingEDocs();
				form = new EditPropertiesFileForm(newDocument);

				if (IsMonitoringDocumentAttachment)
				{
					((IEditResponse)form).CloseOfForm += (object sender, DocumentEventArgs e)
						=>
						{
							var dialog = (IEditResponse)sender;
							if (dialog.ApplyToAll && dialog.DialogResult != DialogResult.Cancel)
							{
								DocumentConfigApplyingToAll = newDocument;
							}
						};
				}

				form.Owner = Form;

				// this is part of a rare case when canceling the dialog, sometimes main.eDocs does not contain the eDoc that wat just added
				// if the eDoc wasn't added before the dialog shows, report the error
				if (!main.eDocs.Contains(newDocument))
				{
					ErrorReporter.ReportOnce("Failed adding file to main.eDocs", "Element (" + newDocument.GetType().FullName + ", " + newDocument.PK + ")is not in collection main.eDocs" + GetType().FullName);
				}

				DialogResult result = ZFormModaliser.ShowDialogAndDispose(form);
				if (result == (DialogResult.Cancel))
				{
					if (main.eDocs.Contains(newDocument))
					{
						main.eDocs.RemoveAndDelete(newDocument);
					}
				}
				else
				{
					AddLogForNewDocument(newDocument);
				}

#if DEBUG
			}
			else
			{
				if (testStorageDoc != null)
				{
					newDocument.SC_DocType = testStorageDoc.SC_DocType;
					newDocument.SC_Desc = testStorageDoc.SC_Desc;
				}
				else
				{
					newDocument.SC_DocType = Core.Constants.RefDocTypes.PackingDeclaration;
				}
			}
#endif
		}

		void AddLogForNewDocument(StorageDocsBase document)
		{
			document.EnableAddEventLogsForNewDocument();
		}

		void AddLogForDocument(string reference, bool isInDatabase, StorageMain parentMain, Event logEvent)
		{
			if (isInDatabase)
			{
				parentMain.Logs.AddNew(logEvent, reference);

				var parent = parentMain.DocumentOwner as EnterpriseBusinessObject;
				if (parent != null)
				{
					parent.Logs.AddNew(logEvent, reference);
				}

				if (parentMain != TopLevelParentMain)
				{
					parentMain.EnableAddEditedARecordEventLogForDocumentOwner();
				}
			}
		}

#if DEBUG
		public bool shouldShowEditFormForTest;
		public StorageDocsBase testStorageDoc;
		public bool IsMonitoringDocumentAttachmentForDebug { get; set; }
#endif

		string[] IDragDropSupportBase.Add(string[] files)
		{
			if (!IsMonitoringDocumentAttachment)
			{
				Add(files);
			}
			return Array.Empty<string>();
		}

		#endregion

		#region Drag, Drop and Paste from Parent Form
		public void DisableInsert()
		{
			isDisableInsert = true;
			if (UserControl is eDocsUserControl eDocsUserControl)
			{
				eDocsUserControl.DisableInsert();
			}
		}
		bool isDisableInsert;

		#region IsInsertAllowed

		internal bool IsInsertAllowed => IsSecurityGrantedEdit && IsSecurityGranted && !eDocsControlReadOnly && TopLevelParentMain != null && TopLevelParentMain.SM_DB != 0 && !isDisableInsert;

		internal bool IsEditable => IsTopLevelParentSelected ? !eDocsControlReadOnly
			: CurrentParentMainOnGrid.DocumentOwner is DocumentEngineCore.DocumentSupport.IDocumentSupportable docOwner && docOwner.DocumentSupporter.StorageDocsAreEditableIfInRelated;
		internal bool IsReadOnly => !(IsEditable && IsSecurityGrantedEdit);

		internal void RegisterEditableForRelatedEDocsIfNeeded()
		{
			if (!IsTopLevelParentSelected && !IsReadOnly)
			{
				TopLevelParentMain.DocumentOwner.RegisterEditableChildObject(CurrentParentMainOnGrid);
			}
		}

		#endregion

		protected override void OnParentFormDragOver(ZForm form, DragEventArgs e)
		{
			base.OnParentFormDragOver(form, e);
			if (IsDropDataSupported(e))
			{
				parentFormOfPlugin = form;
				e.Effect = GetDragDropEffect(e);
			}
		}

		protected override void OnParentFormDragDrop(ZForm form, DragEventArgs e)
		{
			if (!IsInsertAllowed)
			{
				return;
			}
			using (InitializeDragDropFileData(e.Data))
			{
				base.OnParentFormDragDrop(form, e);
				parentFormOfPlugin = form;

				if (IsDropDataSupported(e))
				{
					TopLevelParentMain.SetReadOnlyIncludingChildren(false);
					InsertDataFromParentFormDrag(e);
				}
			}
		}

		IDataObject dragDropFileData;
		IDisposable InitializeDragDropFileData(IDataObject data)
		{
			dragDropFileData = data;
			return new DisposableAction(() => dragDropFileData = null);
		}

		protected bool IsDropDataSupported(DragEventArgs e)
		{
			using (ZDataObject o = ZDataObject.FromData(e.Data))
			{
				return o.SupportsEDocs;
			}
		}

		protected override void OnParentFormDataObjectPasted(ZForm form, DataObjectPastedEventArgs e)
		{
			base.OnParentFormDataObjectPasted(form, e);
			parentFormOfPlugin = form;

			if (IsInsertAllowed)
			{
				Dictionary<string, StorageDocsBase> insertedEDocs;
				if (e.DataToPaste is ZOutlookEmailSenderDataObject)
				{
					insertedEDocs = AddAndRenameTempFilesToProperCaptions((string[])e.DataToPaste.GetData(DataFormats.FileDrop), ((ZDataObject)e.DataToPaste).Caption);
				}
				else if (e.DataToPaste is IEmbeddedRtfImageSource)
				{
					insertedEDocs = ExtractEmbeddedRtfImagesAndAddToEDocs((IEmbeddedRtfImageSource)e.DataToPaste);
				}
				else
				{
					insertedEDocs = InsertFromData(e.DataToPaste);
				}

				if (insertedEDocs != null)
				{
					foreach (var insertedEDoc in insertedEDocs)
					{
						if (insertedEDoc.Value != null)
						{
							e.AddPastedFile(insertedEDoc.Key, insertedEDoc.Value.SC_FileNameWithExtension, insertedEDoc.Value.SC_DescMultilingual, insertedEDoc.Value.ParentMain.SM_ParentFK.ToGuid(), insertedEDoc.Value.PK.ToGuid(), true);
						}
						else
						{
							e.AddPastedFile(insertedEDoc.Key, "", "", Guid.Empty, Guid.Empty, false);
						}
					}
				}
			}
		}

		#region IMonitorEDocAttachment Members

		public bool IsMonitoringDocumentAttachment { get; set; }

		public void StartMonitorEDocAttachment()
		{
			DocumentConfigApplyingToAll = null;
		}

		public void EndMonitorEDocAttachment()
		{
			DocumentConfigApplyingToAll = null;
		}

		StorageDocsBase DocumentConfigApplyingToAll { get; set; }

		#endregion

		protected Dictionary<string, StorageDocsBase> ExtractEmbeddedRtfImagesAndAddToEDocs(IEmbeddedRtfImageSource embeddedRtfImageSource)
		{
			Dictionary<string, StorageDocsBase> result = new Dictionary<string, StorageDocsBase>();

			if (embeddedRtfImageSource.Valid)
			{
				if (embeddedRtfImageSource.ImageFiles.Length > 0 && !IsMonitoringDocumentAttachment)
				{
					using (EDocsPluginAttachMonitor.StartMonitorEDocAttachment(this))
					{
						foreach (EmbeddedRtfImageFileInfo imageFile in embeddedRtfImageSource.ImageFiles)
						{
							using (ZDataObject imageData = GetSingleImageDropDataObject(imageFile.filePath))
							{
								Dictionary<string, StorageDocsBase> insertedEDocs = InsertFromData(imageData);
								foreach (KeyValuePair<string, StorageDocsBase> insertedEDoc in insertedEDocs)
								{
									if (insertedEDoc.Value != null)
									{
										imageFile.fileCaption = insertedEDoc.Value.SC_DescMultilingual;
									}
									result.Add(insertedEDoc.Key, insertedEDoc.Value);

									// We're always dropping a single file so it is impossible to have more than one element in the dictionary
									break;
								}
							}
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(
					Res.GetString("4b3bec58-3be2-4bb1-aa11-1d6af66520ff", @"The clipboard data you are trying to copy is too large and cannot be pasted here.
Please try again and perform the copy in several smaller chucks"));
			}

			return result;
		}

		protected virtual ZDataObject GetSingleImageDropDataObject(string imageFile)
		{
			return ZDataObject.FromData(DataFormats.FileDrop, new string[] { imageFile });
		}

		protected ZForm parentFormOfPlugin;

		IEDocsUnattendedConfigProvider UnattendedConfigProvider => parentFormOfPlugin as IEDocsUnattendedConfigProvider;

		void InsertDataFromParentFormDrag(DragEventArgs e)
		{
			if (e.Data is ZOutlookEmailSenderDataObject)
			{
				AddAndRenameTempFilesToProperCaptions((string[])e.Data.GetData(DataFormats.FileDrop), ((ZDataObject)e.Data).Caption);
			}
			else
			{
				InsertFromData(e.Data);
			}
		}

		protected Dictionary<string, StorageDocsBase> InsertFromData(IDataObject data)
		{
			Dictionary<string, StorageDocsBase> result;

			if (data is ZDataObject dataObject)
			{
				result = InsertFromZData(dataObject);
			}
			else if (data.GetDataPresent(DataFormats.FileDrop) && data.GetData(DataFormats.FileDrop) is string[] files)
			{
				result = Add(files);
			}
			else
			{
				try
				{
					using (var insertableData = GetZDataObjectFromData(data))
					{
						result = InsertFromZData(insertableData);
					}
				}
				catch (NotSupportedException ex)
				{
					result = new Dictionary<string, StorageDocsBase>();
					Globals.Message.ShowError(ex.Message);
				}
			}

			return result;
		}

		protected virtual ZDataObject GetZDataObjectFromData(IDataObject dataToInsert) => ZDataObject.FromData(dataToInsert);

		Dictionary<string, StorageDocsBase> InsertFromZData(ZDataObject data)
		{
			return data.FileDropCount > 0
				? Add((string[])data.GetData(DataFormats.FileDrop))
				: new Dictionary<string, StorageDocsBase>(0);
		}

		Dictionary<string, StorageDocsBase> AddAndRenameTempFilesToProperCaptions(string[] filenames, string caption)
		{
			List<string> humanReadableFilenames = new List<string>();

			foreach (string filename in filenames)
			{
				UniqueFilenameGenerator generator = new UniqueFilenameGenerator();
				string uniqueFilename = generator.GetNewUniqueFilePath(Temp.TempPath, caption + Path.GetExtension(filename));
				File.Copy(filename, uniqueFilename);
				humanReadableFilenames.Add(uniqueFilename);
			}

			return Add(humanReadableFilenames.ToArray());
		}

		protected DragDropEffects GetDragDropEffect(DragEventArgs e)
		{
			DragDropEffects effect = DragDropEffects.None;

			if (IsInsertAllowed)
			{
				if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
				{
					effect = DragDropEffects.Copy;
				}
				else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
				{
					effect = DragDropEffects.Move;
				}
			}

			return effect;
		}

		#endregion

		#region Required Documents

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected void AddRequiredDocument(StorageDocsBase storedDocument)
		{
			if (SupportsDocTracking(out var iHaveReqDoc))
			{
				var docCategoryToAdd = "";
				if (iHaveReqDoc.UltimateDocumentParent is IDocManagerSupport docManager && docManager.DocManagerInfo != null)
				{
					docCategoryToAdd = DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(docManager.DocManagerInfo.DocManagerCode);
				}

				var addAsSCLRefType = false;

				var isPowerOfAttorney = false;
				if (storedDocument.SC_DocType == Core.Constants.RefDocTypes.PowerOfAttorney
					|| storedDocument.SC_DocType == Core.Constants.RefDocTypes.PowerOfAttorneyCustoms
					|| storedDocument.SC_DocType == Core.Constants.RefDocTypes.PowerOfAttorneyForwarding)
				{
					isPowerOfAttorney = true;
				}

				ZQuery query;
				if (iHaveReqDoc.AdditionalRefTypes != null && iHaveReqDoc.AdditionalRefTypes.Count > 0 && iHaveReqDoc.AdditionalRefTypes[0] == Core.Constants.ReferenceTypes.SupplyChainLogistics)
				{
					// only support for one additional refType
					query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, iHaveReqDoc.AdditionalRefTypes[0]);
					query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_DocType, storedDocument.SC_DocType);
					RefDocType foundRefType = HostBusinessObject.Factory.LoadTop1<RefDocType>(query);

					if (foundRefType != null && !isPowerOfAttorney)
					{
						string prompt = Res.GetString("fe215401-550a-4698-b295-e62c2ea198a1",
	@"You have stored a CSR(Client/Supplier/Relationship) document that matches the code of a SCL(Supply Chains Logistics) document. 
The Document Type is {0} - Description: {1}.
			
If your intention was to log this document as a periodic or continuing permission document – you should add a line to the lower grid showing the expiry date of the document.
This will cause the system to complete this required supply chain document as ‘on file’ for any supply chain activity that uses the required documents architecture.
Would you like to add this line to the grid now – please answer Yes if so and add the expiry date – otherwise answer No.", storedDocument.SC_DocType, storedDocument.SC_DescMultilingual);

						addAsSCLRefType = Globals.Message.Show(prompt, Res.GetString("0dfb292c-86c6-495a-8769-73a7846d6fd1", "Message"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
						if (addAsSCLRefType)
						{
							docCategoryToAdd = Core.Constants.ReferenceTypes.SupplyChainLogistics;
						}
					}
				}

				bool addNewReqDocument = true;

				var reqDocumentsQuery = new ZQuery(iHaveReqDoc.RequiredDocuments.CompleteFilter);
				var masterType = iHaveReqDoc.RequiredDocuments.Master.GetType();
				var proposedDocPeriod = (addAsSCLRefType || isPowerOfAttorney) ?
					Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic : Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;

				query = new DocTypeCategoryQuery(Factory, docCategoryToAdd);
				query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_DocType, SQLComparisonOperator.Equal, storedDocument.SC_DocType);
				var docType = Factory.LoadTop1<RefDocType>(query);

				if (docType != null && docType.RT_AllowMultiplePeriodicDocs)
				{
					var findDocumentQuery = new ZQuery(iHaveReqDoc.RequiredDocuments.CompleteFilter);
					findDocumentQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, docType.RT_DocType);
					findDocumentQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocCategory, docCategoryToAdd);
					findDocumentQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocPeriod, proposedDocPeriod);
					if (proposedDocPeriod == Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic)
					{
						findDocumentQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DateReceived, ZDateTimeOffset.Empty);
					}
					var requiredDocument = Factory.Load<JobRequiredDocument>(findDocumentQuery).FirstOrDefault();
					if (requiredDocument != null)
					{
						requiredDocument.ParentType = masterType;
						requiredDocument.EQ_DateReceived = ZDateTimeOffset.Now;
						addNewReqDocument = false;
					}
				}
				else
				{
					if (FindAndUpdateRequiredDocument(Factory, storedDocument, docCategoryToAdd, reqDocumentsQuery, masterType) != null)
					{
						addNewReqDocument = false;
					}

					if (addNewReqDocument)
					{
						//check again in a new factory in case there is new record saved to database by another factory
						var factoryForCheck = new BusinessObjectFactory { NameForDebugging = "FactoryForCheckingDupDocType" };
						if (FindAndUpdateRequiredDocument(factoryForCheck, storedDocument, docCategoryToAdd, reqDocumentsQuery, masterType) != null)
						{
							addNewReqDocument = false;
						}
					}
				}

				//It is a validation error to track a PUB or PRV document, so do not automatically create such rows.
				if (addNewReqDocument
					&& storedDocument.SC_DocType != Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument
					&& storedDocument.SC_DocType != Core.Constants.RefDocTypes.InternallyCreatedPublicDocument)
				{
					JobRequiredDocument newRequiredDocument = iHaveReqDoc.RequiredDocuments.AddNew();
					newRequiredDocument.EQ_DateReceived = ZDateTimeOffset.UtcNow;
					newRequiredDocument.EQ_DocCategory = docCategoryToAdd;
					newRequiredDocument.EQ_DocUsage = addAsSCLRefType ? JobRequiredDocument.DocUsage.Both : GetDefaultDocUsageCode(newRequiredDocument);
					newRequiredDocument.EQ_DocPeriod = proposedDocPeriod;

					newRequiredDocument.EQ_DocType = storedDocument.SC_DocType;
					if (storedDocument.SC_DocType == Core.Constants.RefDocTypes.MiscellaneousDocument && newRequiredDocument.EQ_DocDescriptionMultilingual.IsEmpty)
					{
						AssignRequiredDocDescriptionFromStoredDoc(newRequiredDocument, storedDocument);
					}
				}
			}

			string GetDefaultDocUsageCode(JobRequiredDocument newRequiredDocument) => newRequiredDocument.Lookups.DocUsage_List.Count > 0 ? newRequiredDocument.Lookups.DocUsage_List[0].Code : string.Empty;

			bool SupportsDocTracking(out IHaveRequiredDocuments result)
			{
				if (TopLevelParentMain.DocumentOwner is Freight.Business.IShipmentWithDocsAndCartage shipment)
				{
					result = shipment.RequiredDocumentsProvider;
				}
				else
				{
					result = TopLevelParentMain.DocumentOwner as IHaveRequiredDocuments;
				}

				//Specifically additional type is SCL or primary type is HRE. (Specifically covers all the business objects we want it to be turned on for, for now:
				//HRJobApplicant, HRJobApplication, forwarding shipment, etc)
				if (result != null && result.AdditionalRefTypes != null && result.AdditionalRefTypes.Count > 0 &&
					(result.AdditionalRefTypes[0] == Core.Constants.ReferenceTypes.SupplyChainLogistics))
				{
					return true;
				}
				IDocManagerSupport docManager = result?.UltimateDocumentParent as IDocManagerSupport;
				if (docManager != null)
				{
					var docManagerReferenceType = DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(docManager.DocManagerInfo.DocManagerCode);
					return docManagerReferenceType == Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
				}
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Assigning EQDocDescription from SCDesc")]
		void AssignRequiredDocDescriptionFromStoredDoc(JobRequiredDocument newRequiredDocument, StorageDocsBase storedDocument)
		{
			newRequiredDocument.EQ_DocDescription = storedDocument.SC_Desc.Left(JobRequiredDocument.Schema.EQ_DocDescriptionMaxLength);
		}

		JobRequiredDocument GetRequiredDocumentsInFactory(BusinessObjectFactory factory, ZQuery query, ZString docType, Type masterType, ZString docCategory)
		{
			var requiredDocument = factory.Load<JobRequiredDocument>(query).FirstOrDefault(d => d.EQ_DocType == docType && d.EQ_DocCategory == docCategory);
			if (requiredDocument != null)
			{
				requiredDocument.ParentType = masterType;
			}
			return requiredDocument;
		}

		JobRequiredDocument FindAndUpdateRequiredDocument(BusinessObjectFactory factory, StorageDocsBase storedDocument, ZString docCategoryToAdd, ZQuery reqDocumentsQuery, Type masterType)
		{
			var requiredDocument = GetRequiredDocumentsInFactory(factory, reqDocumentsQuery, storedDocument.SC_DocType, masterType, docCategoryToAdd);
			if (requiredDocument != null)
			{
				requiredDocument.EQ_DateReceived = ZDateTimeOffset.Now;
			}
			return requiredDocument;
		}

		#endregion

		#region ThreadSentry

		readonly IThreadSentry threadSentry;

		void EnsureCurrentThreadIsOwnerOfPlugin()
		{
			threadSentry.EnsureCurrentThreadIsOwner();
		}

		#endregion
	}
}
