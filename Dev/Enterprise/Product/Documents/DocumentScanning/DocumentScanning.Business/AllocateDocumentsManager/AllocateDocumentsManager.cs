using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public class AllocateDocumentsManager : NonPersistentBusinessObject, IDragDropSupport, IDocumentManipulationSupport, IObsoleteValidation
	{
		public AllocateDocumentsManager(DocumentFactory factory)
			: base(factory)
		{
		}

		#region Schema

		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Schema
		{
			public const string StartDateForFilter = "StartDateForFilter";
			public const string EndDateForFilter = "EndDateForFilter";
			public const string AddingUserForFilter = "AddingUserForFilter";
			public const string AllocationNotesForFilter = "AllocationNotesForFilter";
			public const string AllocationStatusForFilter = "AllocationStatusForFilter";
			public const string ShowUnallocatedDocumentsForAllCompanies = "ShowUnallocatedDocumentsForAllCompanies";
			public const string ShowUnallocatedDocumentsForAllBranches = "ShowUnallocatedDocumentsForAllBranches";
			public const string ShowUnallocatedDocumentsForAllDepartments = "ShowUnallocatedDocumentsForAllDepartments";
			public const string ShowUnallocatedDocumentsForAllDeleted = "ShowUnallocatedDocumentsForAllDeleted";
			public const string ShowAllocatedDocumentsForAllCompanies = "ShowAllocatedDocumentsForAllCompanies";
			public const string ShowAllocatedDocumentsForAllBranches = "ShowAllocatedDocumentsForAllBranches";
			public const string ShowAllocatedDocumentsForAllDepartments = "ShowAllocatedDocumentsForAllDepartments";
		}

		#endregion

		#region Related Business Objects

		#region Allocated Documents
		public StorageDocsCollection AllocatedDocuments
		{
			get
			{
				if (fAllocatedDocuments == null)
				{
					fAllocatedDocuments = new StorageDocsCollection(MasterFactory);
					fAllocatedDocuments.ExcludeDeletedDocuments = true;
					RegisterEditableChildObject(fAllocatedDocuments);
					fAllocatedDocuments.SetReadOnlyIncludingChildren(ReadOnly);
				}
				return fAllocatedDocuments;
			}
		}

		StorageDocsCollection fAllocatedDocuments;
		#endregion

		#region Allocated Documents View
		public StorageDocsCollectionView AllocatedDocumentsView
		{
			get
			{
				if (fAllocatedDocumentsView == null)
				{
					fAllocatedDocumentsView = new StorageDocsCollectionView(AllocatedDocuments);
					fAllocatedDocumentsView.IncludeDeletedDocuments = false;
				}
				return fAllocatedDocumentsView;
			}
		}

		StorageDocsCollectionView fAllocatedDocumentsView;
		#endregion

		#region Unallocated Documents
		public StorageDocsUnallocatedCollection UnallocatedDocuments
		{
			get
			{
				if (fUnallocatedDocuments == null)
				{
					fUnallocatedDocuments = new StorageDocsUnallocatedCollection(MasterFactory);
					fUnallocatedDocuments.ExcludeDeletedDocuments = false;

					for (int j = 0; j < CurrentUnallocatedDocuments.Count; j++)
					{
						fUnallocatedDocuments.Add(CurrentUnallocatedDocuments[j]);
					}

					RegisterEditableChildObject(fUnallocatedDocuments);
					fUnallocatedDocuments.SetReadOnlyIncludingChildren(ReadOnly);
				}
				return fUnallocatedDocuments;
			}
		}

		StorageDocsUnallocatedCollection fUnallocatedDocuments;
		#endregion

		#region Unallocated Documents View
		public StorageDocsUnallocatedCollectionView UnallocatedDocumentsView
		{
			get
			{
				if (fUnallocatedDocumentsView == null)
				{
					fUnallocatedDocumentsView = new StorageDocsUnallocatedCollectionView(UnallocatedDocuments);
					fUnallocatedDocumentsView.IncludeDeletedDocuments = false;
				}
				return fUnallocatedDocumentsView;
			}
		}

		StorageDocsUnallocatedCollectionView fUnallocatedDocumentsView;
		#endregion

		#region Unallocated Documents

		public StorageDocsUnallocatedCollection CurrentUnallocatedDocuments
		{
			get
			{
				if (fDocuments == null)
				{
					fDocuments = new StorageDocsUnallocatedCollection(MasterFactory);
					fDocuments.ExcludeDeletedDocuments = false;
					fDocuments.Load();
				}
				return fDocuments;
			}
		}

		StorageDocsUnallocatedCollection fDocuments;

		#endregion

		#region MasterFactory

		public DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = (DocumentFactory)Factory;
					fMasterFactory.SaveSuccessful += new EventHandler(fMasterFactory_SaveSuccessful);
				}
				return fMasterFactory;
			}
		}
		DocumentFactory fMasterFactory;

		void fMasterFactory_SaveSuccessful(object sender, EventArgs e)
		{
			HasChanges = false;
		}

		#endregion

		#region FileImporterForConfigSettings

		public FileImporter FileImporterForConfigSettings => FileImporterForImport;

		#endregion

		#region FileImporterForImport
		/// <summary>
		/// This property is temporary on purpose, a new instance is created each time so that the correct
		/// registry settings are loaded and used.
		/// </summary>
		public
#if DEBUG
		virtual
#endif
		FileImporter FileImporterForImport
		{
			get
			{
				if (fFileImporterForImport == null)
				{
					fFileImporterForImport = new FileImporter(MasterFactory, true);
					fFileImporterForImport.ImportSuccessful += new ScanningFinishedEventHandler(FileImporter_ImportSuccessful);
				}

				return fFileImporterForImport;
			}
		}

		FileImporter fFileImporterForImport;

		void FileImporter_ImportSuccessful(object sender, ScanningFinishedEventArgs ea)
		{
			AddNewDocumentAfterScanning(ea, ((FileImporter)sender).IsAutoAllocate);
		}

		#endregion

		#endregion

		#region Business scanning functions

#if DEBUG
		public bool ForceThrowOutOfMemoryException;
		public bool IsDocumentWithParentDeleted;
		public bool IsNewDocumentDeleted;
#endif

		public void AddNewDocumentAfterScanning(ScanningFinishedEventArgs ea, bool isAutoAllocate)
		{
			if (ea.FileDetailsCount > 0)
			{
				foreach (DocumentResult scannedDocument in ea)
				{
					var unallocatedDoc = UnallocatedDocuments.AddNew();
					unallocatedDoc.SC_DataType = Core.Constants.DocManagerCodes.Unallocated;
					unallocatedDoc.SC_FileName = Path.GetFileNameWithoutExtension(scannedDocument.SourceFileName);
					unallocatedDoc.SC_ImageData = DocumentUtilities.GetFileAsBytes(scannedDocument.FilePath);
					unallocatedDoc.ScannedBarcodeValue = scannedDocument.ScannedBarcodeValue;
					File.Delete(scannedDocument.FilePath);

					var itemFK = ZGuid.Empty;

					if (scannedDocument is BaseBarcode)
					{
						var barcodeInfo = (BaseBarcode)scannedDocument;

						if (!barcodeInfo.DocManagerCode.IsEmpty)
						{
							unallocatedDoc.SM_Type = barcodeInfo.DocManagerCode;
							itemFK = barcodeInfo.RefPK;
						}

						unallocatedDoc.SC_DocType = barcodeInfo.DocType;
					}

					unallocatedDoc.SetAllocateStatus(scannedDocument);
					if (!itemFK.IsEmpty)
					{
						if (isAutoAllocate && unallocatedDoc.IsAutoAllocatable)
						{
							var documentWithParent = MasterFactory.New<StorageDocs>();
							documentWithParent.CopyPersistentValuesFrom(unallocatedDoc);
							documentWithParent.SC_DataType = unallocatedDoc.EDocFormat;
							var parent = MasterFactory.CreateParentFor(documentWithParent);
							parent.SM_ParentFK = unallocatedDoc.SC_ParentID;
							parent.SM_Type = unallocatedDoc.SM_Type;
#if DEBUG
							documentWithParent.ForceThrowOutOfMemoryException = ForceThrowOutOfMemoryException;
#endif
							StorageDocs newDocument = null;
							try
							{
								newDocument = AllocateDocument(documentWithParent, unallocatedDoc, itemFK) as StorageDocs;
							}
							catch (Exception ex)
							{
								if (!documentWithParent.IsDeleted)
								{
									documentWithParent.Delete();
#if DEBUG
									IsDocumentWithParentDeleted = documentWithParent.IsDeleted;
#endif
								}

								if (ex.IsExceptionPresentIncludingInner<OutOfMemoryException>() && Globals.IsUserInteractive)
								{
									var errorMessage = Res.GetString("CFD41A57-B8E5-4ADD-BF2C-1F7D6A3B1157", "Some eDocs were not allocated because this program is running low on memory. Please restart {0} and try again.", Core.Constants.ProductName);
									Globals.Message.ShowError(errorMessage); // only showing this info when it is user interactive.
								}
								throw;
							}
						}
						else
						{
							unallocatedDoc.SC_ParentID = itemFK;
						}
					}

					if (File.Exists(scannedDocument.FilePath))
					{
						File.Delete(scannedDocument.FilePath);
					}
				}
			}
		}
		#endregion

		#region Document allocation and management
		public void AllocateDocuments()
		{
			RemoveAlreadyProcessedDocuments();
			AllocateDocuments(UnallocatedDocuments.ToArray());
		}

		void RemoveAlreadyProcessedDocuments()
		{
			var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			newFactory.NameForDebugging = "FactoryForRemoveAlreadyProcessedDocuments()";
			var lastestDocumentsInDatabase = new StorageDocsUnallocatedCollection(newFactory);
			lastestDocumentsInDatabase.Load();

			var processedDocuments = UnallocatedDocuments
				.Cast<StorageDocsUnallocated>()
				.Where(document => document.IsInDatabase && lastestDocumentsInDatabase.FindByPK(document.PK) == null)
				.ToList();

			if (processedDocuments.Count > 0)
			{
				using (UnallocatedDocumentsView.SuspendListChanged())
				{
					UnallocatedDocuments.RemoveRange(processedDocuments);
				}

				if (Globals.CanShowDialogs)
				{
					string errorMessage = Res.GetString("b52cba6a-d6ac-47ed-8663-7920ca42ee62", "The following document(s) have been allocated or deleted by another user before this allocation operation was done, the allocation for these file(s) was canceled.");
					foreach (var document in processedDocuments)
					{
						errorMessage += System.Environment.NewLine + Res.GetString(
							"9C4246E1-5F8C-4409-BEF0-AEDD90217766", "Parent Type: {0} Doc Type: {1} Scan Date: {2}", document.SM_Type, document.SC_DocType, document.SC_Date);
					}

					Globals.Message.Show(errorMessage);// only showing this info when it is user interactive.
				}
			}
		}
#if DEBUG
		public
#else
		protected
#endif
		StorageDocsBase AllocateDocument(StorageDocsBase document, ZGuid itemPK)
		{
			if (UnallocatedDocuments.Contains(document))
			{
				UnallocatedDocuments.Remove(document);
			}
			return AllocateDocument(document, null, itemPK);
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "SCDesc in logs")]
		protected StorageDocsBase AllocateDocument(StorageDocsBase document, StorageDocsBase documentToDelete, ZGuid itemPK)
		{
			if (documentToDelete != null)
			{
				UnallocatedDocuments.Remove(documentToDelete);
				documentToDelete.Delete();
			}
			var allocatedDoc = MasterFactory.Allocate(document, itemPK);
			if (allocatedDoc != null)
			{
				AllocatedDocuments.Add(allocatedDoc);
				ZString reference = ZString.Format((NoResString)"eDoc '{0}' allocated", allocatedDoc.SC_Desc);

				if (!allocatedDoc.ParentMain.IsArchiving)
				{
					allocatedDoc.ParentMain.Logs.AddNew(Events.DocumentAllocated, reference);
					EnterpriseBusinessObject parent = allocatedDoc.ParentMain.DocumentOwner as EnterpriseBusinessObject;
					if (parent != null)
					{
						parent.Logs.AddNew(Events.DocumentAllocated, allocatedDoc.CreateReference());

						if (allocatedDoc.DocType != null && !allocatedDoc.DocType.RT_SE_NKDocumentReceivedEvent.IsEmpty
							&& Events.All.Contains(allocatedDoc.DocType.RT_SE_NKDocumentReceivedEvent))
						{
							Event customEventToAdd = Events.All[allocatedDoc.DocType.RT_SE_NKDocumentReceivedEvent];
							parent.Logs.AddNew(customEventToAdd, reference);
						}
					}
				}
			}

			return allocatedDoc;
		}

		protected StorageDocsUnallocated UnallocateDocument(StorageDocsBase document)
		{
			AllocatedDocuments.Remove(document);
			document.ParentMain.Logs.AddNew(Events.DocumentUnallocated, string.Format("eDoc '{0}' unallocated", document.SC_Desc));
			var sM_ParentFK = document.ParentMain.SM_ParentFK;
			var sM_Type = document.ParentMain.SM_Type;
			var unallocatedDocument = MasterFactory.Unallocate(document);
			unallocatedDocument.SC_DataType = sM_Type;
			unallocatedDocument.SC_ParentID = sM_ParentFK;

			document.Delete();
			UnallocatedDocuments.Add(unallocatedDocument);
			unallocatedDocument.Validation.ValidateSC_DocType();
			return unallocatedDocument;
		}

		public void UpdateDocumentImage(ZGuid pK, string filename, DocumentChangedEventArgs changedArgs)
		{
			StorageDocs requiredDoc = FindDocumentByPK(pK);

			if (requiredDoc != null)
			{
				requiredDoc.SC_ImageData = DocumentUtilities.GetFileAsBytes(filename);
				//Fixme - when Geoff enables access to the edit log created
				//RequiredDoc.Logs.AddNew(ChangedArgs.ChangeType, ChangedArgs.Description);
			}
		}
#endregion

		#region Document Cut/Paste and Drag/Drop

		public StorageDocsUnallocated CreateNewDocument(SerializableEDoc eDocToAdd)
		{
			var newParent = (StorageMain)MasterFactory.New(typeof(StorageMain));

			var newDocument = eDocToAdd.ToBusinessObject(newParent);
			newDocument.SC_Date = ZDateTime.UtcNow;

			var document = MasterFactory.New<StorageDocsUnallocated>();
			document.CopyPersistentValuesFrom(newDocument);
			document.SM_Type = newDocument.SM_Type;
			document.SC_ParentID = newParent.SM_ParentFK;

			newParent.Delete();
			newDocument.Delete();

			UnallocatedDocuments.Add(document);
			return document;
		}

		#endregion

		public void DeleteDocument(ZGuid pK)
		{
			StorageDocs docToDelete = UnallocatedDocuments.FindByPK(pK) as StorageDocs;
			if (docToDelete != null)
			{
				docToDelete.DeleteParentIfUnallocated();
				UnallocatedDocuments.RemoveAndDelete(docToDelete);
			}
			else
			{
				docToDelete = AllocatedDocuments.FindByPK(pK) as StorageDocs;
				if (docToDelete != null)
				{
					var reference = DocumentLogReferenceHelper.GetReference(docToDelete, Events.DocumentDeletedPermanently);
					docToDelete.ParentMain.Logs.AddNew(Events.DocumentDeletedPermanently, reference);
					AllocatedDocuments.RemoveAndDelete(docToDelete);
				}
			}
		}

		#region ScanningManager

		// Will refactor the init/deinit stuff... soon...

		#endregion

		#region Events

		public event EventHandler NotifyNoAllocationsDone;
		public event EventHandler NotifyNonPreviewableFilesNotAdded;
		public event EventHandler NotifyPermanentlyDeletingDocumentsInProgress;
		public event EventHandler NotifyPermanentlyDeletingDocumentsComplete;
		public event EventHandler<FilePermanentlyDeletedEventArgs> DocumentPermanentlyDeleted;

		#endregion

		#region Filter

		public ZDateTime StartDateForFilter
		{
			get
			{
				return startDateForFilter;
			}
			set
			{
				startDateForFilter = value;
				StartDateForFilterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StartDateForFilterInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.StartDateForFilter);
			}
		}

		ZDateTime startDateForFilter;

		public ZDateTime EndDateForFilter
		{
			get
			{
				return endDateForFilter;
			}
			set
			{
				endDateForFilter = value;
				EndDateForFilterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EndDateForFilterInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.EndDateForFilter);
			}
		}

		ZDateTime endDateForFilter;

		[List("AllStaff")]
		public ZString AddingUserForFilter
		{
			get
			{
				return addingUserForFilter;
			}
			set
			{
				addingUserForFilter = value;
				AddingUserForFilterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AddingUserForFilterInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.AddingUserForFilter);
			}
		}

		public GlbStaffCollection AllStaff
		{
			get
			{
				return new GlbStaffCollection(Factory);
			}
		}

		ZString addingUserForFilter;

		public ZString AllocationNotesForFilter
		{
			get
			{
				return allocationNotesForFilter;
			}
			set
			{
				allocationNotesForFilter = value;
				AllocationNotesForFilterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AllocationNotesForFilterInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.AllocationNotesForFilter);
			}
		}

		ZString allocationNotesForFilter;

		public ZString AllocationStatusForFilter
		{
			get
			{
				return allocationStatusForFilter;
			}
			set
			{
				allocationStatusForFilter = value;
				AllocationStatusForFilterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AllocationStatusForFilterInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.AllocationStatusForFilter);
			}
		}

		ZString allocationStatusForFilter;

		ZQuery DateFilter
		{
			get
			{
				var result = new ZQuery();
				if (StartDateForFilter.IsValid)
				{
					result.AddToFilter(StorageDocsSchema.SC_Date, SQLComparisonOperator.GreaterThanOrEqualTo, StartDateForFilter);
				}
				if (EndDateForFilter.IsValid)
				{
					result.AddToFilter(StorageDocsSchema.SC_Date, SQLComparisonOperator.LessThanOrEqualTo, EndDateForFilter);
				}
				return result;
			}
		}

		ZDBOnlyQuery AddingUserFilter
		{
			get
			{
				var result = new ZDBOnlyQuery(typeof(StorageDocsUnallocated));

				if (!string.IsNullOrWhiteSpace(AddingUserForFilter))
				{
					result.AddToFilter(StorageDocsSchema.SC_SystemCreateUser, AddingUserForFilter);
				}
				return result;
			}
		}

		ZDBOnlyQuery AllocationNotesFilter
		{
			get
			{
				var result = new ZDBOnlyQuery(typeof(StorageDocsUnallocated));

				if (!string.IsNullOrWhiteSpace(AllocationNotesForFilter))
				{
					var allocationNotesSub = new ZDBOnlySubQuery(typeof(StmNote), StmNoteSchema.ST_ParentID);
					allocationNotesSub.AddToFilter(StmNoteSchema.ST_NoteText, SQLComparisonOperator.Contains, AllocationNotesForFilter);
					allocationNotesSub.AddToFilter(StmNoteSchema.ST_Description, "Allocation Notes");
					result.AddSubQuery(allocationNotesSub, JoinCondition.And);
				}
				return result;
			}
		}

		ZDBOnlyQuery AllocationStatusFilter
		{
			get
			{
				var result = new ZDBOnlyQuery(typeof(StorageDocsUnallocated));

				if (!string.IsNullOrWhiteSpace(AllocationStatusForFilter))
				{
					var allocationStatusSub = new ZDBOnlySubQuery(typeof(StmNote), StmNoteSchema.ST_ParentID);
					allocationStatusSub.AddToFilter(StmNoteSchema.ST_NoteText, SQLComparisonOperator.Contains, AllocationStatusForFilter);
					allocationStatusSub.AddToFilter(StmNoteSchema.ST_Description, "Allocation Status");
					result.AddSubQuery(allocationStatusSub, JoinCondition.And);
				}
				return result;
			}
		}

		ZQuery CompleteFilter
		{
			get
			{
				var result = new ZQuery();
				result.AddToFilter(DateFilter, JoinCondition.And);
				result.AddToFilter(AddingUserFilter, JoinCondition.And);
				result.AddToFilter(AllocationNotesFilter, JoinCondition.And);
				result.AddToFilter(AllocationStatusFilter, JoinCondition.And);
				return result;
			}
		}

		public void LoadUnallocatedDocumentsWithFiltering()
		{
			UnallocatedDocumentsView.ExtraFilter = CompleteFilter;
			UnallocatedDocumentsView.Rebuild();
		}

		public void ClearUnallocatedDocumentsFilterValues()
		{
			StartDateForFilter = ZDateTime.Empty;
			EndDateForFilter = ZDateTime.Empty;
			addingUserForFilter = ZString.Empty;
			AllocationNotesForFilter = ZString.Empty;
			AllocationStatusForFilter = ZString.Empty;

			LoadUnallocatedDocumentsWithFiltering();
		}

		#endregion

		public ZBool ShowUnallocatedDocumentsForAllCompanies
		{
			get => UnallocatedDocumentsView.ShowDocumentsForAllCompanies;
			set
			{
				UnallocatedDocumentsView.ShowDocumentsForAllCompanies = value;
				ShowUnallocatedDocumentsForAllCompaniesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowUnallocatedDocumentsForAllCompaniesInfo => GetZPropertyInfo(Schema.ShowUnallocatedDocumentsForAllCompanies);

		public bool ShowUnallocatedDocumentsForAllCompanies_ReadOnly => !Env.Security.ViewAllCompanySpecificDocuments.IsAllowed;

		public ZBool ShowUnallocatedDocumentsForAllBranches
		{
			get => UnallocatedDocumentsView.ShowDocumentsForAllBranches;
			set
			{
				UnallocatedDocumentsView.ShowDocumentsForAllBranches = value;
				ShowUnallocatedDocumentsForAllBranchesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowUnallocatedDocumentsForAllBranchesInfo => GetZPropertyInfo(Schema.ShowUnallocatedDocumentsForAllBranches);

		public bool ShowUnallocatedDocumentsForAllBranches_ReadOnly => !Env.Security.ViewAllBranchSpecificDocuments.IsAllowed;

		public ZBool ShowUnallocatedDocumentsForAllDepartments
		{
			get => UnallocatedDocumentsView.ShowDocumentsForAllDepartments;
			set
			{
				UnallocatedDocumentsView.ShowDocumentsForAllDepartments = value;
				ShowUnallocatedDocumentsForAllDepartmentsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowUnallocatedDocumentsForAllDepartmentsInfo => GetZPropertyInfo(Schema.ShowUnallocatedDocumentsForAllDepartments);

		public bool ShowUnallocatedDocumentsForAllDepartments_ReadOnly => !Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed;

		public ZBool ShowUnallocatedDocumentsForAllDeleted
		{
			get => UnallocatedDocumentsView.IncludeDeletedDocuments;
			set
			{
				UnallocatedDocumentsView.IncludeDeletedDocuments = value;
				ShowUnallocatedDocumentsForAllDeletedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowUnallocatedDocumentsForAllDeletedInfo => GetZPropertyInfo(Schema.ShowUnallocatedDocumentsForAllDeleted);

		public ZBool ShowAllocatedDocumentsForAllCompanies
		{
			get => AllocatedDocumentsView.ShowDocumentsForAllCompanies;
			set
			{
				AllocatedDocumentsView.ShowDocumentsForAllCompanies = value;
				ShowAllocatedDocumentsForAllCompaniesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowAllocatedDocumentsForAllCompaniesInfo => GetZPropertyInfo(Schema.ShowAllocatedDocumentsForAllCompanies);

		public bool ShowAllocatedDocumentsForAllCompanies_ReadOnly => !Env.Security.ViewAllCompanySpecificDocuments.IsAllowed;

		public ZBool ShowAllocatedDocumentsForAllBranches
		{
			get => AllocatedDocumentsView.ShowDocumentsForAllBranches;
			set
			{
				AllocatedDocumentsView.ShowDocumentsForAllBranches = value;
				ShowAllocatedDocumentsForAllBranchesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowAllocatedDocumentsForAllBranchesInfo => GetZPropertyInfo(Schema.ShowAllocatedDocumentsForAllBranches);

		public bool ShowAllocatedDocumentsForAllBranches_ReadOnly => !Env.Security.ViewAllBranchSpecificDocuments.IsAllowed;

		public ZBool ShowAllocatedDocumentsForAllDepartments
		{
			get => AllocatedDocumentsView.ShowDocumentsForAllDepartments;
			set
			{
				AllocatedDocumentsView.ShowDocumentsForAllDepartments = value;
				ShowAllocatedDocumentsForAllDepartmentsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowAllocatedDocumentsForAllDepartmentsInfo => GetZPropertyInfo(Schema.ShowAllocatedDocumentsForAllDepartments);

		public bool ShowAllocatedDocumentsForAllDepartments_ReadOnly => !Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed;

		#region IDragDropSupport Members

		void IDragDropSupport.Add(SerializableEDocCollection collection)
		{
			var nonPreviewableFilesIncluded = false;

			foreach (SerializableEDoc docFromClipboard in collection)
			{
				if (PreviewableDocumentHelper.IsSupported(docFromClipboard.DataType))
				{
					var newParent = (StorageMain)MasterFactory.New(typeof(StorageMain));
					var newDocument = docFromClipboard.ToBusinessObject(newParent);
					var document = MasterFactory.New<StorageDocsUnallocated>();

					document.CopyPersistentValuesFrom(newDocument);
					document.SM_Type = Core.Constants.DocManagerCodes.Unallocated;

					newParent.Delete();
					newDocument.Delete();
					UnallocatedDocuments.Add(document);
				}
				else
				{
					nonPreviewableFilesIncluded = true;
				}
			}

			if (nonPreviewableFilesIncluded)
			{
				NotifyNonPreviewableFilesNotAdded?.Invoke(this, EventArgs.Empty);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		string[] IDragDropSupportBase.Add(string[] filenames)
		{
			var rejectedFilenames = new List<string>();
			foreach (var filename in filenames)
			{
				string fileToSave;
				try
				{
					var convertFile = !(IsJpegFile(filename) || IsGifFile(filename) || PreviewableDocumentHelper.IsPdf(filename));
					fileToSave = convertFile ? DocumentUtilities.ConvertFileToTiff(filename) : filename;
				}
				catch
				{
					rejectedFilenames.Add(filename);
					continue;
				}

				if (File.Exists(fileToSave))
				{
					var document = UnallocatedDocuments.AddNew();
					document.SM_Type = Core.Constants.DocManagerCodes.Unallocated;
					document.HasChanges = false;
					document.SC_FileName = Path.GetFileNameWithoutExtension(filename);
					document.SC_ImageData = DocumentUtilities.GetFileAsBytes(fileToSave);
					document.ScannedBarcodeValue = BarcodeHelper.JoinValidBarcodes(FileImporterForImport.ReadBarcodesFromFile(fileToSave), StorageDocsBarcode.Schema.SCB_BarcodeMaxLength);

					if (fileToSave != filename)
					{
						File.Delete(fileToSave);
					}
				}
			}

			return rejectedFilenames.ToArray();
		}

		public static Image AsImage(Stream data)
		{
			try
			{
				return Image.FromStream(data);
			}
			catch (Exception ex) when (ex is ArgumentException || (ex is ExternalException && ex.Message.Contains("GDI+")))
			{
				return null;
			}
		}

		public static bool IsJpegFile(string filename)
		{
			var extension = Path.GetExtension(filename);
			if (extension != null)
			{
				var extensionUpcaseWithoutDot = extension.Replace(".", "").ToUpperInvariant();
				return extensionUpcaseWithoutDot == Core.Constants.FileFormats.JPG ||
					   extensionUpcaseWithoutDot == Core.Constants.FileFormats.JPEG;
			}
			return false;
		}

		public static bool IsJpegImage(Image image)
		{
			return image.RawFormat.Guid == ImageFormat.Jpeg.Guid;
		}

		public static bool IsGifFile(string filename)
		{
			var extension = Path.GetExtension(filename);
			if (extension != null)
			{
				var extensionUpcaseWithoutDot = extension.Replace(".", "").ToUpperInvariant();
				return extensionUpcaseWithoutDot == Core.Constants.FileFormats.GIF;
			}
			return false;
		}

		public static bool IsGifImage(Image image)
		{
			return image.RawFormat.Guid == ImageFormat.Gif.Guid;
		}

		#endregion

		#region IDocumentManipulationSupport Members

		public void DeleteDocumentsQuietly(ICollection<BusinessObject> documents)
		{
			foreach (StorageDocsBase document in documents)
			{
				document.DeleteQuietly();
			}
		}

		public ICollection<BusinessObject> DeleteDocumentsPermanently(ICollection<BusinessObject> documents)
		{
			ICollection<BusinessObject> nonDeletedDocuments = Array.Empty<BusinessObject>();
			try
			{
				NotifyPermanentlyDeletingDocumentsInProgress?.Invoke(this, EventArgs.Empty);

				nonDeletedDocuments = DeleteDocumentsPermanentlyWithoutMessage(documents);
			}
			finally
			{
				NotifyPermanentlyDeletingDocumentsComplete?.Invoke(this, EventArgs.Empty);
			}
			return nonDeletedDocuments;
		}

		ICollection<BusinessObject> DeleteDocumentsPermanentlyWithoutMessage(ICollection<BusinessObject> documents)
		{
			var totalDocuments = documents.Count;
			var documentsPermanentlyDeleted = 0;

			foreach (StorageDocsBase document in documents)
			{
				if (document.IsInDatabase)
				{
					var logEvent = Events.DocumentDeletedPermanently;
					var reference = DocumentLogReferenceHelper.GetReference(document, logEvent);
					document.Logs.AddNew(logEvent, reference);
				}
				document.Delete();

				++documentsPermanentlyDeleted;
				DocumentPermanentlyDeleted?.Invoke(this, new FilePermanentlyDeletedEventArgs(documentsPermanentlyDeleted, totalDocuments));
			}
			return Array.Empty<BusinessObject>();
		}

		public void RestoreDocuments(ICollection<BusinessObject> documents)
		{
			foreach (StorageDocsBase document in documents)
			{
				document.Restore();
			}
		}

		public IEnumerable<BusinessObject> AllocateDocuments(IEnumerable<BusinessObject> documents)
		{
			var leftovers = new List<BusinessObject>();
			var toBeAllocated = new Dictionary<StorageDocs, StorageDocsUnallocated>();
			var docsHaveBeenDeletedOrAllocated = new List<StorageDocsUnallocated>();

			var errorMessage = CopyDocumentsForAllocation(leftovers, toBeAllocated, docsHaveBeenDeletedOrAllocated);

			UnallocatedDocuments.RemoveRange(docsHaveBeenDeletedOrAllocated);

			if (!string.IsNullOrEmpty(errorMessage) && Globals.CanShowDialogs)
			{
				Globals.Message.Show(errorMessage); // only showing this info when it is user interactive.
			}

			foreach (var entry in toBeAllocated)
			{
				//must be run in 2nd loop to prevent bug when more than 1 doc is alloated to same job and the storagemain search returns the storagemain from an existing unallocated doc.
				AllocateDocument(entry.Key, entry.Value, entry.Value.SC_ParentID);
			}

			if (leftovers.Count == documents.Count())
			{
				if (NotifyNoAllocationsDone != null)
				{
					NotifyNoAllocationsDone(this, EventArgs.Empty);
				}
			}

			return leftovers;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Here we just want to break the 'for' and report the right message to client if anything goes wrong.")]
		string CopyDocumentsForAllocation(List<BusinessObject> leftovers, Dictionary<StorageDocs, StorageDocsUnallocated> toBeAllocated, List<StorageDocsUnallocated> docsHaveBeenDeletedOrAllocated)
		{
			var errorMessage = string.Empty;

			foreach (StorageDocsUnallocated currentDoc in UnallocatedDocuments.Reverse())
			{
				var isDocumentAlreadyProcessed = false;

				if (!currentDoc.HasErrors && currentDoc.IsAutoAllocatable && !currentDoc.SC_ParentID.IsEmpty)
				{
					StorageDocs document = null;

					try
					{
						document = CreateNewDocumentFromUnallocatedDocument(currentDoc, ref isDocumentAlreadyProcessed);
					}
					catch (Exception ex)
					{
						leftovers.Add(currentDoc);

						if (ex.IsExceptionPresentIncludingInner<OutOfMemoryException>())
						{
							errorMessage = Res.GetString("CFD41A57-B8E5-4ADD-BF2C-1F7D6A3B1157", "Some eDocs were not allocated because this program is running low on memory. Please restart {0} and try again.", Core.Constants.ProductName);
						}
						else
						{
							errorMessage = Res.GetString("CFA4FCEC-56A0-4915-8F26-BCA32A11CC69", "Some eDocs were not allocated. Please restart {0} and try again.", Core.Constants.ProductName);
							ErrorReporter.ReportOnce("AllocateDocumentsManager.CopyDocumentsForAllocation.", ex);
						}

						break;
					}

					if (document != null)
					{
						toBeAllocated.Add(document, currentDoc);
					}
					else if (isDocumentAlreadyProcessed)
					{
						if (Globals.CanShowDialogs)
						{
							if (string.IsNullOrEmpty(errorMessage))
							{
								errorMessage = Res.GetString("8e8b27ad-8eb7-414e-be20-2429f775ae94", "The following document(s) have been allocated or deleted by another user before this allocation operation was done, the allocation for these file(s) was canceled.");
							}

							errorMessage += System.Environment.NewLine + Res.GetString("5280f2ef-a2b9-40d6-8867-5f65858eaf19", "Parent Type: {0} Doc Type: {1} Scan Date: {2}", currentDoc.SM_Type, currentDoc.SC_DocType, currentDoc.SC_Date);
						}

						docsHaveBeenDeletedOrAllocated.Add(currentDoc);
					}
				}
				else
				{
					leftovers.Add(currentDoc);
				}
			}

			return errorMessage;
		}

		StorageDocs CreateNewDocumentFromUnallocatedDocument(StorageDocsUnallocated currentDoc, ref bool isDocumentAlreadyProcessed)
		{
			var document = MasterFactory.New<StorageDocs>();

			try
			{
				document.CopyPersistentValuesFrom(currentDoc);
			}
			catch (SqlStreamReaderRowNotFoundException)
			{
				// The document has already been allocated or deleted. When document has been allocated it could have been moved from Odyssey database to document database like Odyssey_SD001. 
				// It will throw SqlStreamReaderRowNotFoundException because it cannot be found in the Odyssey database anymore.
				isDocumentAlreadyProcessed = true;
				document.Delete();
				return null;
			}
			catch
			{
				document.Delete();
				throw;
			}

			var parent = MasterFactory.CreateParentFor(document);
			parent.SM_Type = document.SC_DataType.Left(StorageMain.Schema.SM_TypeMaxLength);
			document.SC_DataType = document.EDocFormat;
			document.Validation.ValidateSC_DocType();
			return document;
		}

		public IEnumerable<BusinessObject> UnallocateDocuments(IEnumerable<BusinessObject> list)
		{
			foreach (StorageDocsBase document in list)
			{
				UnallocateDocument(document);
			}
			return Array.Empty<BusinessObject>();
		}
		#endregion

		#region Implementation

		StorageDocs FindDocumentByPK(ZGuid pK)
		{
			// try the allocated docs
			var requiredDoc = UnallocatedDocuments.FindByPK(pK) as StorageDocs ?? AllocatedDocuments.FindByPK(pK) as StorageDocs;

			return requiredDoc;
		}

		#endregion
	}
}
