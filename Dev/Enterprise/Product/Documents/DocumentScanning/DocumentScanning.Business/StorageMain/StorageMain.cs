using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageMain : AutoStorageMain, IDocumentsView, IStorageMain, ICanBeSavedByDocumentFactory, IForceLogsOnParentForNewWithNoHasChanges
	{
		public StorageMain(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!(factory is DocumentFactory) && !factory.IsConstructingNullBusinessObject)
			{
				throw new ArgumentException("StorageMain can only be loaded in a DocumentFactory", nameof(factory));
			}
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SM_DB), ConcurrencyPolicy.Strict);

			constructionStack = System.Environment.StackTrace;
		}
		readonly string constructionStack;

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			SM_Type = Core.Constants.DocManagerCodes.Unallocated;
		}

		#endregion

		#region Saving

#if DEBUG
		public virtual
#else
			internal
#endif
		DbConnection CurrentDbConnection
		{
			get { return ((IDbConnected)Factory).Connection; }
		}

#if DEBUG
		public
#else
		internal
#endif
		void ReleaseSqlLocks()
		{
			Factory.ReleaseSqlLocks();
			if (Factory is DocumentFactory documentFactory)
			{
				documentFactory.FactoryForEverythingExceptEDocs?.ReleaseSqlLocks();
			}
		}

#if DEBUG
		public event EventHandler OnFactorySavingForTest;

		protected override void ResetHasChangesForTest()
		{
			eDocs.ForEach(e => e.HasChanges = false);
			base.ResetHasChangesForTest();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			SM_DB = 1;
		}
#endif
#if DEBUG
		public
#else
		internal
#endif
		bool IsArchiving { get; set; }

		protected override void OnFactorySaving()
		{
			if (!IsArchiving && (DocumentOwner is IHaveRequiredDocuments || DocumentOwner is IDocsAndCartageParent))
			{
				if (IsFilesLoaded)
				{
					var allFilesCollectionView = new StorageFileCollectionView(this, eDocs)
					{
						ShowDocumentsForAll = true
					};

					allFilesCollectionView.AddRelatedRequiredDocuments();
				}

				if (IsDocumentsLoaded)
				{
					var allDocumentsCollectionView = new StorageDocsCollectionView(this, eDocs)
					{
						ShowDocumentsForAll = true
					};

					allDocumentsCollectionView.AddRelatedRequiredDocuments();
				}
			}

			// DEBUG info - remove when it is clear where the error comes from

			if (SM_Type == "IAT")
			{
				if (DocumentOwner != null && !DocumentOwner.PKSchemaColumn.Name.Equals(EDIInterchangeSchema.Constants.PK, StringComparison.OrdinalIgnoreCase))
				{
					//var productKey = ObjectFactory.Get<IProductRegistration>().Key;

					//if (productKey.EnterpriseCode.Equals("MFI", StringComparison.InvariantCultureIgnoreCase))
					//{
					ErrorReporter.ReportOnce("WI00101247 / CS00378488 - Inconsistent SM_Type and SM_ParentFK - Notify A.K", string.Format(CultureInfo.InvariantCulture, "DocumentOwner.PKSchemaColumn.Name = {0}, SM_PK = {1}, SM_Type = {2}, SM_ParentFK = {3}", DocumentOwner.PKSchemaColumn.Name, PK, SM_Type, SM_ParentFK));
					//}
				}
				// this else clause breaks several unit tests where DocumentOwner is null, but SM_Type and SM_ParentFK are correct. 
				// the reason why DocumentOwner is null is that newly created parent (EDIInterchange) is in another factory and factory in DocumentOwner getter can't load it. 
				// if the above added error reporting does not return any results - we might try to uncomment below else clause
				//else 
				//{
				//	ErrorReporter.ReportOnce("WI00101247 / CS00378488 - Inconsistent SM_Type and SM_ParentFK - Notify A.K", string.Format("DocumentOwner is null, SM_PK = {0}, SM_Type = {1}, SM_ParentFK = {2}", PK.ToString(), SM_Type, SM_ParentFK.ToString()));
				//}
			}

			using (AddEditedARecordEventLogForDocumentOwnerIfNeeded())
			{
				savingStack = $"RowState: {((INeedRow)this).Row.RowState}; IsInDatabase: {IsInDatabase}\r\n{System.Environment.StackTrace}";
				base.OnFactorySaving();
			}

#if DEBUG
			if (OnFactorySavingForTest != null)
			{
				OnFactorySavingForTest(null, null);
			}
#endif
		}
		string savingStack;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			try
			{
				base.OnFactorySaved(saveSucceeded);
				savedStack = $"RowState: {((INeedRow)this).Row.RowState}; IsInDatabase: {IsInDatabase}\r\n{System.Environment.StackTrace}";
			}
			finally
			{
				ReleaseSqlLocks();
			}
		}
		string savedStack;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			CopyEdocsToWriteableDatabaseIfApplicable();
			base.OnFactorySavingBeforeTransactionCore();

			//move here so message boxes are raised before transaction begins?
			SetImageDataForAllIfRequired();
		}

		protected override void RunPreSaveValidationCore()
		{
			SetImageDataForAllIfRequired();

			base.RunPreSaveValidationCore();
		}

		internal void SetImageDataForAllIfRequired()
		{
			if (IsEDocsLoaded && ShouldSetImageDataForAll)
			{
				eDocs.SetImageDataForAllIfRequired();
				ShouldSetImageDataForAll = false;
			}
		}

		void CopyEdocsToWriteableDatabaseIfApplicable()
		{
			if (HasChanges &&
				feDocs != null &&
				eDocs.HasChanges &&
				MasterFactory.GetDbWriteableState(SM_DB) == DbWriteableState.ReadOnly)
			{
				var oldDbName = MasterFactory.GetDatabaseName(SM_DB);
				var newDbNumber = MasterFactory.LastWriteableDatabaseWithFreeSpace();
				var newDbName = MasterFactory.GetDatabaseName(newDbNumber);

				var connection = ((IDbConnected)MasterFactory).Connection;

				try
				{
					using (var transactionManager = connection.BeginTransactionWithManager())
					{
						var sql = string.Format(@"
							DELETE [{0}]..[{4}]
								WHERE SC_PK in (SELECT SC_PK FROM [{3}]..[{4}] WHERE SC_SM = '{1}');
							INSERT [{0}]..[{4}] ({2})
								SELECT {2} FROM [{3}]..[{4}]
								WHERE SC_SM = '{1}';",
							newDbName, //0
							this.PK.ToString(), //1
							StorageDocColumnList, //2
							oldDbName, //3
							StorageDocsSchema.Constants.TableName); //4
						connection.ExecuteNonQuery(sql);

						transactionManager.CommitTransaction();
					}
				}
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce(ex.Message, ex);
					throw;
				}

				var numberedFactory = eDocs.Factory as NumberedBusinessObjectFactory;
				numberedFactory.ChangeDbNumberToAWriteableOne(newDbNumber);
				SM_DB = newDbNumber;
			}
		}

		string StorageDocColumnList
		{
			get { return storageDocColumnList ?? (storageDocColumnList = StorageDocsSchema.CsvColumnList(StorageDocsSchema.Instance)); }
		}
		string storageDocColumnList;

		#endregion

		#region EditedARecord logs

		bool enableAddEditedARecordEventLog;
		public bool EnableAddEditedARecordEventLogForDocumentOwner() => enableAddEditedARecordEventLog = true;

		IDisposable AddEditedARecordEventLogForDocumentOwnerIfNeeded()
		{
			if ((enableAddEditedARecordEventLog || eDocs.OfType<StorageDocsBase>().Any(d => d.ShouldAddEventLogsForNewDocument))
				&& documentOwner is ZArchitecture.EnterpriseBusinessObject parent && !parent.HasChanges
				&& documentOwner.IsInDatabase)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				parent.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			return new DisposableAction(() => enableAddEditedARecordEventLog = false);
		}

		#endregion

		#region Related Business Objects

		#region Documents

		/// <summary>
		/// A collection of eDocs that are TIF files and ACTIVE (SC_IsDeleted is false)
		/// </summary>
		public StorageDocsCollectionView Documents
		{
			get
			{
				if (documents == null)
				{
					documents = new StorageDocsCollectionView(this, eDocs);
				}
				return documents;
			}
		}
		StorageDocsCollectionView documents;

		public bool IsDocumentsLoaded
		{
			get { return documents != null; }
		}

		void ClearDocumentsView()
		{
			this.documents = null;
		}

		public ZString LoadFailureReason
		{
			get
			{
				object dummy = eDocs; // poke edocs to set load failure reason the first time
				return loadFailureReason;
			}
		}
		ZString loadFailureReason;

		#endregion

		#region Files

		/// <summary>
		/// A collection of eDocs that are any file type except TIF (e.g. pdf, xls, doc, txt) and are ACTIVE (SC_IsDeleted is false)
		/// </summary>
		public StorageFileCollectionView Files
		{
			get
			{
				MasterFactory.ThreadSentry.EnsureCurrentThreadIsOwner();

				if (files == null)
				{
					files = new StorageFileCollectionView(this, eDocs);
				}
				files.SwapCollectionToFilter(eDocs);
				return files;
			}
		}
		StorageFileCollectionView files;

		public bool IsFilesLoaded
		{
			get { return files != null; }
		}

		#endregion

		#region eDocs

		public override ZInt SM_DB
		{
			get { return base.SM_DB; }
			set
			{
				ZInt originalValue = SM_DB;
				base.SM_DB = value;
				if (SM_DB != originalValue)
				{
					ClearDocumentsView();
					RefreshBindingIncludingChildren();
				}
			}
		}

		/// <summary>
		///  Require that eDocs get reloaded before they are used again
		/// </summary>
		public void RequireReload()
		{
			if (feDocs != null)
			{
				feDocs.Cast<StorageDocsBase>().ForEach(doc =>
				{
					doc.DeleteTempFile();
					doc.ActiveShipamaxMessage?.Reload();
				});
				feDocs = null;
				documents = null;
				files = null;
			}

			feDocsView = null;
			feDocsNeedReload = true;
		}

		/// <summary>
		/// A collection of ALL documents of any file type that belong to this parent, including deleted docs (where SC_IsDeleted is true)
		/// </summary>
		[ChildEditable(false)]
		public StorageDocsDependentCollectionBase eDocs
		{
			get
			{
				MasterFactory.ThreadSentry.EnsureCurrentThreadIsOwner();

				bool createCollection;

				if (feDocs == null)
				{
					createCollection = true;
				}
				else
				{
					createCollection = (!IsDeleted && SM_DB != MasterFactory.GetFactoryNumberInUse(feDocs));

					if (createCollection)
					{
						UnRegisterEditableChildObject(feDocs);
					}
				}

				if (createCollection)
				{
					feDocs = LoadStorageDocsCollection(MasterFactory.GetFactory(SM_DB));

					RegisterEditableChildObject(feDocs);
				}

				return feDocs;
			}
		}

		StorageDocsDependentCollectionBase LoadStorageDocsCollection(NumberedBusinessObjectFactory writeableEdocsFactory)
		{
			var docCollection = new StorageDocsDependentCollectionBase(this, writeableEdocsFactory);

			try
			{
				if (feDocsNeedReload)
				{
					docCollection.Reload(true, true);
					feDocsNeedReload = false;
				}
				else
				{
					docCollection.Load();
				}
				loadFailureReason = "";
			}
			catch (SqlException ex)
			{
				if (SqlFailureChecker.IsAcceptableFailure(ex))
				{
					loadFailureReason = ex.Message;
				}
				else
				{
					throw;
				}
			}

			return docCollection;
		}

		StorageDocsDependentCollectionBase feDocs;
		bool feDocsNeedReload;

		#endregion

		#region eDocsView

		/// <summary>
		/// A collection of all documents of any file type, excluding deleted docs (where SC_IsDeleted is false)
		/// </summary>
		public StorageDocsCollectionViewBase eDocsView
		{
			get
			{
				if (feDocsView == null)
				{
					feDocsView = new StorageDocsCollectionViewBase(eDocs);
				}
				return feDocsView;
			}
		}

		StorageDocsCollectionViewBase feDocsView;

		bool IsEDocsLoaded
		{
			get { return feDocs != null; }
		}

		internal bool ShouldSetImageDataForAll { get; set; }

		#endregion

		#region PublishedEDocsAndFiles

		/// <summary>
		/// A collection of eDocs that are published and active
		/// </summary>
		public StorageDocsCollectionViewBase PublishedEDocsAndFiles
		{
			get
			{
				if (fPublishedEDocsAndFiles == null)
				{
					fPublishedEDocsAndFiles = new StorageDocsCollectionViewBase(eDocs)
					{
						ExcludeUnpublishedDocuments = true
					};
				}
				return fPublishedEDocsAndFiles;
			}
		}

		StorageDocsCollectionViewBase fPublishedEDocsAndFiles;

		#endregion

		#region DocManagerInfo

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null && DocumentOwner is BusinessObject owner)
				{
					docManagerInfo = ((IDocManagerSupport)owner).DocManagerInfo;
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region RelatedStorageMains

		public StorageMainCollection RelatedParentMains => relatedParentMains ?? (relatedParentMains = LoadRelatedParentMains(false));

		StorageMainCollection relatedParentMains;

		public StorageMainCollection LoadRelatedParentMains(bool requireReload)
		{
			var storageMainCollection = new StorageMainCollection(MasterFactory) { this };

			if (DocManagerInfo is DocManagerInfo managerInfo)
			{
				var dbQueryHelper = new DocManagerDBHelper();
				foreach (var bizO in managerInfo.RelatedObjects)
				{
					var parent = MasterFactory.GetStorageMainForPK(bizO.PK);

					if (parent != null && requireReload)
					{
						parent.RequireReload();
					}
					if (parent != null && (managerInfo.RelatedObjectTypesAlwaysShow(bizO) || StorageMainHasStorageDocs(parent, dbQueryHelper)))
					{
						if (bizO is IOverrideStorageMainDocManagerCode codeManager)
						{
							var newCode = codeManager.GetOverridenCodeIfNecessary(parent.SM_Type);
							if (!newCode.IsEmpty)
							{
								parent.SM_TypeOverride = newCode;
							}
						}
						storageMainCollection.Add(parent);

						if (parent.DocumentOwner is IDocumentSupportable docOwner && docOwner.DocumentSupporter != null && docOwner.DocumentSupporter.StorageDocsAreEditableIfInRelated)
						{
							RegisterEditableChildObject(parent);
						}
					}
				}
			}

			return storageMainCollection;
		}

		bool StorageMainHasStorageDocs(StorageMain storageMain, DocManagerDBHelper dbQueryHelper)
		{
			string tableNameWithDBPrefix = dbQueryHelper.GetTableNameWithDatabasePrefix(storageMain.SM_DB, StorageDocsSchema.Constants.TableName);
			string sql = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(1) FROM {0} WHERE {1} = @storageMainPK",
				tableNameWithDBPrefix,
				StorageDocsSchema.Constants.SC_SM);

			var result = -1;

			try
			{
				using (var command = Db.Connection.Command(sql)) // use direct sql to avoid loading documents into memory
				{
					command.AddParameter("storageMainPK", SqlDbType.UniqueIdentifier, storageMain.PK.ToGuid());
					result = (int)command.ExecuteScalar();
				}
			}
			catch (SqlException ex)
			{
				if (SqlFailureChecker.IsAcceptableFailure(ex))
				{
					if (Globals.CanShowDialogs)
					{
						Globals.Message.ShowError(Res.GetString("aad68a30-483b-4988-958b-b3b9bc9378ae", "Unable to access the eDocs, you may need to restore the eDocs database(s).\r\nError message: {0}", ex.Message));  // only showing this info when it is user interactive.
					}
				}
				else
				{
					throw;
				}
			}

			return result > 0;
		}

		public bool IsRelatedParentMainsLoaded => relatedParentMains != null;

		#endregion

		#region OwnerAssemblyData

		public IAssemblyData OwnerAssemblyData
		{
			get
			{
				if (ownerAssemblyData == null || (ownerAssemblyData.DocManagerCode != SM_TypeForDocumentOwner && AssemblyDataLookup.IsDocManagerCodeValid(SM_TypeForDocumentOwner)))
				{
					ownerAssemblyData = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(SM_TypeForDocumentOwner);
				}

				return ownerAssemblyData;
			}
		}
		IAssemblyData ownerAssemblyData;

		#endregion

		#region DocumentOwner

		/// <summary>
		/// Should be used to have Document Owner type different to SM_Type. This could be required when there are several types of objects around single row. Example - Load List Consol and Forwarding Consol.
		/// </summary>
		public ZString SM_TypeOverride { get; set; }
		ZString SM_TypeForDocumentOwner
		{
			get
			{
				return SM_TypeOverride.IsEmpty ? SM_Type : SM_TypeOverride;
			}
		}

		public BusinessObject DocumentOwner
		{
			get
			{
				if ((documentOwner == null) || (documentOwner.PK != SM_ParentFK))
				{
					if (!SM_ParentFK.IsEmpty && !SM_TypeForDocumentOwner.IsEmpty && (OwnerAssemblyData != null))
					{
						documentOwner = Factory.Load(OwnerAssemblyData.BusinessObjectType, SM_ParentFK);
						if (documentOwner != null)
						{
							GetRequiredDocuments();
							if (LastRequiredDocuments != null && !IsRegisteredEditableChildObject(LastRequiredDocuments))
							{
								RegisterEditableChildObject(LastRequiredDocuments);
							}
						}
					}
					else
					{
						documentOwner = null;
					}
				}
				return documentOwner;
			}
		}

		BusinessObject documentOwner;

		#endregion

		#region Parent

		internal ZString ParentHumanReadableName
		{
			get
			{
				if (parentHumanReadableName.IsEmpty && !SM_ParentFK.IsEmpty && OwnerAssemblyData != null)
				{
					var parent = Factory.Load(OwnerAssemblyData.BusinessObjectType, SM_ParentFK);
					if (parent != null)
					{
						parentHumanReadableName = parent.HumanReadableName;
					}
				}
				return parentHumanReadableName;
			}
		}
		ZString parentHumanReadableName;

		#endregion

		public BusinessObject UnsavedParent { get; set; }

		#region Required Documents

		public JobRequiredDocumentDependentCollection GetRequiredDocuments()
		{
			var documentOwner = DocumentOwner ?? UnsavedParent;

			IHaveRequiredDocuments haveRequiredDocuments = documentOwner as IHaveRequiredDocuments;
			JobRequiredDocumentDependentCollection result = null;

			if (haveRequiredDocuments != null)
			{
				result = haveRequiredDocuments.RequiredDocuments;
			}
			else
			{
				IDocsAndCartageParent docsAndCartageParent = (documentOwner as IDocsAndCartageParent);
				if (docsAndCartageParent != null)
				{
					result = docsAndCartageParent.RequiredDocumentsProvider.RequiredDocuments;
				}
			}

			LastRequiredDocuments = result;

			return result;
		}

		JobRequiredDocumentDependentCollection LastRequiredDocuments;

		#endregion

		#endregion

		#region Add

		public StorageDocsBase AddFileOrDocument(SubStreamableStream contents, AddFileOrDocumentDto dto)
		{
			return AddFileOrDocumentCore(contents, dto);
		}

		public StorageDocsBase AddFileOrDocument(byte[] contents, AddFileOrDocumentDto dto)
		{
			return AddFileOrDocumentCore(contents, dto);
		}

		StorageDocsBase AddFileOrDocumentCore(object contents, AddFileOrDocumentDto dto)
		{
			IsArchiving = dto.IsArchiving;
			var contentsToInsertBytes = contents as byte[];
			var contentsToInsertStream = contents as SubStreamableStream;

			if ((contentsToInsertBytes?.Length ?? 0) == 0 && (contentsToInsertStream?.Length ?? 0) == 0)
			{
				throw new EmptyContentEDocsException();
			}

			var correctedFileName = RemoveIllegalCharactersInFileName(dto.FileName);
			var dataType = Path.GetExtension(correctedFileName);

			StorageDocsBase addedDocument;
			if (correctedFileName.IsEmpty || FileImporter.IsSupportedImageFile(dataType))
			{
				if (contentsToInsertBytes == null)
				{
					var buffer = new byte[contentsToInsertStream.Length];
					contentsToInsertStream.Read(buffer, 0, buffer.Length);
					contentsToInsertBytes = buffer;
				}
				addedDocument = Documents.Factory.New<StorageDocs>();

				if (!string.IsNullOrEmpty(dataType))
				{
					addedDocument.SC_DataType = Path.GetExtension(correctedFileName).Substring(1).ToUpper(CultureInfo.InvariantCulture);
				}

				// We allow having mutiple files with empty name, please see StorageDocsBase.IeDocBase.FileName and DocumentPackTest.TestAddEDocsToAttach(),
				// so here we only get unique file name when file name is not empty.
				if (!correctedFileName.IsEmpty)
				{
					var uniqueName = Documents.GetUniqueFilename(Documents.TruncateAndTrimDotRemoveExtension(correctedFileName, StorageDocsSchema.SC_FileName.MaxLength), addedDocument.SC_DataType);
					addedDocument.SC_FileName = Documents.TruncateAndTrimDotRemoveExtension(uniqueName, StorageDocsSchema.SC_FileName.MaxLength);
				}

				addedDocument.SC_ImageData = contentsToInsertBytes;

				Documents.Add(addedDocument);
			}
			else
			{
				addedDocument = contentsToInsertBytes != null ?
					Files.AddOrUpdateFromFilename(contentsToInsertBytes, correctedFileName, dto.FileAction) :
					Files.AddOrUpdateFromFilename(contentsToInsertStream, correctedFileName, dto.FileAction);
			}

			if (addedDocument == null)
			{
				return null;
			}

			using (addedDocument.SuspendCheckSecurityRightsForDocTypes())
			{
				if (!dto.DocumentType.IsEmpty)
				{
					addedDocument.SC_DocType = dto.DocumentType;
					if ((!addedDocument.SC_Desc_ReadOnly || dto.IsArchiving) && !string.IsNullOrEmpty(dto.Description))
					{
						addedDocument.SC_Desc = dto.Description;
					}
				}
				if (!string.IsNullOrEmpty(dto.Source))
				{
					addedDocument.SC_RDS_NKDocSource = dto.Source;
				}

				if (addedDocument.SC_ImageData.IsEmpty)
				{
					if (contentsToInsertBytes != null)
					{
						addedDocument.SC_ImageData = contentsToInsertBytes;
					}
					else
					{
						((IeDoc)addedDocument).SetImageDataStream(contentsToInsertStream);
					}
				}

				addedDocument.SC_GC_Company = dto.VisibleCompanyPK;
				addedDocument.SC_GB_Branch = dto.VisibleBranchPK;
				addedDocument.SC_GE_Department = dto.VisibleDepartmentPK;

				if (dto.ShouldSupersedeOlderVersion)
				{
					SupersedeOlderVersionDocs(addedDocument, correctedFileName);
				}
			}

			return addedDocument;
		}

		public void SupersedeOlderVersionDocs(StorageDocsBase doc, string fileName)
		{
			if (SystemDataRegistry.Instance.UnpublishOlderVersionDocument.Value && doc != null && !string.IsNullOrEmpty(fileName))
			{
				var files = eDocsView.FindDocsByNameInAllVersions(Path.GetFileNameWithoutExtension(fileName), Path.GetExtension(fileName));
				files.ForEach(x =>
				{
					if (x != null && x.PK != doc.PK)
					{
						x.IsSupersededByNewVersion = true;
						x.SC_IsPublished = false;
					}
				});
			}
		}

		public void AddLogsForNewDocument(BusinessObject logOwner, IeDoc newDoc)
		{
			var document = newDoc as StorageDocsBase;
			if (logOwner != null && document != null)
			{
				AddLogsForNewDocument(logOwner, document);
			}
		}

		public void AddLogsForNewDocument(BusinessObject logOwner, StorageDocsBase document)
		{
			if (logOwner is ZArchitecture.EnterpriseBusinessObject parent)
			{
				parent.Logs.AddNew(AutoEvents.DocumentImported, document.CreateReference());
			}
		}

		#endregion

		#region Delete

		public bool DeleteFileOrDocument(string docType)
		{
			foreach (StorageDocsBase storageDoc in this.eDocs)
			{
				if (!storageDoc.SC_IsDeleted && storageDoc.SC_IsPublished && storageDoc.SC_DocType == docType)
				{
					storageDoc.DeleteQuietly();
					return true;
				}
			}

			return false;
		}

		public override void Delete()
		{
			DeleteStorageReferences();

			deletedStack = $"RowState: {((INeedRow)this).Row.RowState}; IsInDatabase: {IsInDatabase}\r\n{System.Environment.StackTrace}";
			DeleteStorageDocs();
			base.Delete();
		}
		string deletedStack;

		public StorageReference[] StorageReferences
		{
			get
			{
				if (storageReferences == null)
				{
					storageReferences = Factory.Load<StorageReference>(new ZQuery(StorageReferenceSchema.SR_SM, PK));
				}
				return storageReferences;
			}
		}
		StorageReference[] storageReferences;

		void DeleteStorageReferences()
		{
			StorageReferences.DeleteAll();
		}

		void DeleteStorageDocs()
		{
			if (!SM_ParentFK.IsEmpty)
			{
				if (IsFilesLoaded)
				{
					foreach (StorageFile file in Files)
					{
						if (file.SC_SM == PK)
						{
							file.Dispose();
						}
					}
				}

				foreach (StorageDocsBase eDoc in eDocs.ToArray())
				{
					if (eDoc.SC_SM == PK)
					{
						eDoc.Delete();
					}
				}
			}
		}

#if DEBUG
		public
#endif
		ZString RemoveIllegalCharactersInFileName(ZString fileName)
		{
			var result = string.Empty;
			if (!fileName.IsEmpty)
			{
				var sb = new System.Text.StringBuilder(fileName);
				var invalidChars = new string(Path.GetInvalidFileNameChars());
				for (int i = sb.Length - 1; i >= 0; i--)
				{
					if (invalidChars.IndexOf(sb[i]) >= 0)
					{
						sb.Remove(i, 1);
					}
				}
				result = sb.ToString();
			}
			return (ZString)result;
		}

		public void DeleteIfUnallocated()
		{
			if (!IsDeleted && SM_ParentFK.IsEmpty)
			{
				Delete();
			}
		}

		#endregion

		#region Properties

		#region DocumentOwnerCode

		[MaxLength(20)]
		public ZString DocumentOwnerCode
		{
			get { return (DocumentOwner != null && !DocumentOwner.IsDeleted) ? CodePropertyAttribute.CodeFromBusinessObject(DocumentOwner) : ZString.Empty; }
		}

		public ZPropertyInfo DocumentOwnerCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentOwnerCode)); }
		}

		#endregion

		#region DocumentOwnerDescription

		[MaxLength(20)]
		public
#if DEBUG
			virtual
#endif
			ZString DocumentOwnerDescription
		{
			get
			{
				if (OwnerAssemblyData != null)
				{
					if (IsTopLevelParent)
					{
						return Res.GetString("b8145adf-aadc-4e2b-85d6-5aaf4147accc", "This {0}", OwnerAssemblyData.HumanReadableName);
					}

					else
					{
						return OwnerAssemblyData.GetFriendlyName(DocumentOwner);
					}
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo DocumentOwnerDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentOwnerDescription)); }
		}
		#endregion

		#region SM_LastActivity

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods")]
		public ZDateTime SM_LastActivity
		{
			get
			{
				string tableNameWithDBPrefix = new DocManagerDBHelper().GetTableNameWithDatabasePrefix(SM_DB, StorageDocsSchema.Constants.TableName);
				string sql = string.Format(CultureInfo.InvariantCulture, "SELECT MAX(SC_SystemLastEditTimeUTC) FROM {0} WHERE {1} = @storageMainPK",
					tableNameWithDBPrefix,
					StorageDocsSchema.Constants.SC_SM);
				using var command = Db.Connection.Command(sql); // use direct sql to avoid loading documents into memory
				command.AddParameter("storageMainPK", SqlDbType.UniqueIdentifier, PK.ToGuid());
				var lastActivity = command.ExecuteScalar();
				return lastActivity == DBNull.Value ? ZDateTime.Empty : new ZDateTime(lastActivity, DateTimeKind.Local);
			}
		}

		public ZPropertyInfo SM_LastActivityInfo
		{
			get { return GetZPropertyInfo(nameof(SM_LastActivity)); }
		}

		#endregion

		#region SM_ParentFK

		public override ZGuid SM_ParentFK
		{
			get
			{
				try
				{
					return base.SM_ParentFK;
				}
				catch (RowNotInTableException ex)
				{
					throw new RowNotInTableException($"{ex.Message}\r\nConstruction StackTrace:\r\n{constructionStack}\r\nSaving StackTrace:\r\n{savingStack}\r\nSaved StackTrace:\r\n{savedStack}\r\nDelete StackTrace:\r\n{deletedStack}", ex);
				}
			}
			set
			{
				base.SM_ParentFK = value;
			}
		}

		public bool SM_ParentFK_ReadOnly => true;

		#endregion

		#region ViewIncludesDeletedDocuments

		public ZBool ViewIncludesDeletedDocuments
		{
			get
			{
				return eDocsView.IncludeDeletedDocuments;
			}
			set
			{
				if (eDocsView.IncludeDeletedDocuments != value)
				{
					eDocsView.IncludeDeletedDocuments = value;
					ViewIncludesDeletedDocumentsInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ViewIncludesDeletedDocumentsInfo
		{
			get { return GetZPropertyInfo(nameof(ViewIncludesDeletedDocuments)); }
		}

		#endregion

		#region Company/Branch/Department Specific

		public ZBool ShowDocumentsForAllCompanies
		{
			get
			{
				return eDocsView.ShowDocumentsForAllCompanies;
			}
			set
			{
				if (eDocsView.ShowDocumentsForAllCompanies != value)
				{
					eDocsView.ShowDocumentsForAllCompanies = value;
					ShowDocumentsForAllCompaniesInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ShowDocumentsForAllCompaniesInfo
		{
			get { return GetZPropertyInfo(nameof(ViewIncludesDeletedDocuments)); }
		}

		public bool ShowDocumentsForAllCompanies_ReadOnly
		{
			get { return !Env.Security.ViewAllCompanySpecificDocuments.IsAllowed; }
		}

		public ZBool ShowDocumentsForAllBranches
		{
			get
			{
				return eDocsView.ShowDocumentsForAllBranches;
			}
			set
			{
				if (eDocsView.ShowDocumentsForAllBranches != value)
				{
					eDocsView.ShowDocumentsForAllBranches = value;
					ShowDocumentsForAllBranchesInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ShowDocumentsForAllBranchesInfo
		{
			get { return GetZPropertyInfo(nameof(ShowDocumentsForAllBranches)); }
		}

		public bool ShowDocumentsForAllBranches_ReadOnly
		{
			get { return !Env.Security.ViewAllBranchSpecificDocuments.IsAllowed; }
		}

		public ZBool ShowDocumentsForAllDepartments
		{
			get
			{
				return eDocsView.ShowDocumentsForAllDepartments;
			}
			set
			{
				if (eDocsView.ShowDocumentsForAllDepartments != value)
				{
					eDocsView.ShowDocumentsForAllDepartments = value;
					ShowDocumentsForAllDepartmentsInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ShowDocumentsForAllDepartmentsInfo
		{
			get { return GetZPropertyInfo(nameof(ShowDocumentsForAllDepartments)); }
		}

		public bool ShowDocumentsForAllDepartments_ReadOnly
		{
			get { return !Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed; }
		}

		public ZBool ShowPreview
		{
			get
			{
				if (fShowPreview == null)
				{
					fShowPreview = Globals.CanShowDialogs &&
						DocManagerRegistry.Instance.EDocsPreviewEnabled.
						GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentUser?.PK ?? Guid.Empty, Guid.Empty, Guid.Empty);
				}

				return fShowPreview.Value;
			}
			set
			{
				if (fShowPreview != value)
				{
					fShowPreview = value;
					ShowPreviewInfo.RefreshBinding();
				}
			}
		}

		ZBool? fShowPreview;

		public ZPropertyInfo ShowPreviewInfo
		{
			get { return GetZPropertyInfo(nameof(ShowPreview)); }
		}

		public bool ShowPreview_ReadOnly
		{
			get { return false; }
		}

		#endregion

		#region ViewExcludesUnpublishedDocuments

		public ZBool ViewExcludesUnpublishedDocuments
		{
			get
			{
				return eDocsView.ExcludeUnpublishedDocuments;
			}
			set
			{
				if (eDocsView.ExcludeUnpublishedDocuments != value)
				{
					eDocsView.ExcludeUnpublishedDocuments = value;
					ViewExcludesUnpublishedDocumentsInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ViewExcludesUnpublishedDocumentsInfo
		{
			get { return GetZPropertyInfo(nameof(ViewExcludesUnpublishedDocuments)); }
		}

		#endregion

		#region IsTopLevelParent

		/// <summary>
		/// Flag to indicate whether this storagemain object is the top level parent 
		/// when viewed on the eDocs tab
		/// </summary>
		public bool IsTopLevelParent { get; set; }

		#endregion

		public override string QuickViewCard => DocumentOwnerDescription;

		#endregion

		#region Lookups

		public IBusinessObjectCollection SM_ParentFK_List
		{
			get
			{
				IBusinessObjectCollection result = null;
				if (OwnerAssemblyData != null)
				{
					result = OwnerAssemblyData.GetBusinessObjectCollection(MasterFactory);
				}
				if (result == null)
				{
					result = new NonPersistentUnallocatedObjectCollection(MasterFactory);
				}
				return result;
			}
		}

		#endregion

		#region Read Related eDocs Event

		internal void NotifyDocumentReadByUser(StorageDocsBase document)
		{
			if (RequiresReadRelatedDocumentLog)
			{
				SilentlySaveReadRelatedDocumentsLog();
			}
		}

		StorageDocsBase[] StorageDocsThatRequireReadingInThisSession
		{
			get
			{
				List<StorageDocsBase> result = new List<StorageDocsBase>();
				foreach (StorageDocsBase document in eDocs)
				{
					if (document.DocType != null && document.DocType.RT_ForceUserToRead && WasDocumentUnread(document))
					{
						result.Add(document);
					}
				}
				return result.ToArray();
			}
		}

		ZBool RequiresReadRelatedDocumentLog
		{
			get
			{
				bool result = false;
				try
				{
					StorageDocsBase[] storageDocsThatRequireReadingInThisSession = this.StorageDocsThatRequireReadingInThisSession;
					bool hadDocumentsThatRequiredReading = storageDocsThatRequireReadingInThisSession.Length > 0;

					result = hadDocumentsThatRequiredReading;
					if (hadDocumentsThatRequiredReading)
					{
						foreach (StorageDocsBase document in storageDocsThatRequireReadingInThisSession)
						{
							if (!document.IsReadByUserInThisSession)
							{
								result = false;
								break;
							}
						}
					}
				}
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce("StorageMain.RequiresReadRelatedDocumentLog", ex.Message, ex);
				}
				return result;
			}
		}

		ZBool IsDocumentAddedSinceLastRead(StorageDocsBase document)
		{
			StmALog lastRelatedDocumentsReadLog = DocumentOwner?.GetLogs().MostRecentLogByEventTime(Events.RelatedEDocsRead, new ZQuery(StmALogSchema.SL_GS_NKUser, GlbStaff.CurrentUser.GS_Code));
			return lastRelatedDocumentsReadLog == null || lastRelatedDocumentsReadLog.SL_PostedTimeUtc < document.SC_Date;
		}

		ZBool WasDocumentUnread(StorageDocsBase document)
		{
			return document.IsInDatabase && document.SC_AddingUser != GlbStaff.CurrentUser.GS_Code && IsDocumentAddedSinceLastRead(document);
		}

		void SilentlySaveReadRelatedDocumentsLog()
		{
			try
			{
				BusinessObjectFactory factoryToSave = new BusinessObjectFactory();
				BusinessObject documentOwnerToLog = factoryToSave.Load(DocumentOwner.GetType(), DocumentOwner.PK);
				if (documentOwnerToLog == null)
				{
					return;
				}

				Logs ownerLogs = documentOwnerToLog.GetLogs();
				if (ownerLogs != null)
				{
					ZQuery loadOldLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, "RER");
					loadOldLogQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, GlbStaff.CurrentUser.GS_Code);
					loadOldLogQuery.AddToFilter(StmALogSchema.SL_IsCancelled, "N");
					var oldLogs = ownerLogs.Find(loadOldLogQuery);
					if (oldLogs != null)
					{
						foreach (var oldLog in oldLogs)
						{
							oldLog.Cancel();
						}
					}
				}

				documentOwnerToLog.GetLogs().AddNew(Events.RelatedEDocsRead);
				factoryToSave.Save();
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("StorageMain.SilentlySaveReadRelatedDocumentsLog", ex.Message, ex);
			}
		}

		#endregion

		#region Document Factory

		public DocumentFactory MasterFactory
		{
			get { return Factory as DocumentFactory; }
		}

		public override BusinessObjectFactory CreateNewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		#endregion

		#region UniqueIndexFailureHandler

		public void AttemptToResolveUniqueIndexFailure()
		{
			(UniqueIndexFailureHandlers.Single() as StorageMainNumberFountainUniqueIndexFailureHandler).AttemptToResolveCore();
		}

		IUniqueIndexFailureHandler fUniqueIndexFailureHandler;
		public event EventHandler<StorageMainUniqueIndexViolationEventArgs> HandlingUniqueIndexFailure;
		public bool HandlingUniqueIndexFailureIsBinded => HandlingUniqueIndexFailure != null;

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return fUniqueIndexFailureHandler ?? (fUniqueIndexFailureHandler = new StorageMainNumberFountainUniqueIndexFailureHandler(this)); }
		}

		void OnHandlingUniqueIndexFailure(StorageMainUniqueIndexViolationEventArgs e)
		{
			EventHandler<StorageMainUniqueIndexViolationEventArgs> handlingUniqueIndexFailure = HandlingUniqueIndexFailure;
			if (handlingUniqueIndexFailure != null)
			{
				handlingUniqueIndexFailure(this, e);
			}
			else
			{
				e.StorageMainInMemory.Delete();
			}
		}

		class StorageMainNumberFountainUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			readonly StorageMain storageMainRecord;

			public StorageMainNumberFountainUniqueIndexFailureHandler(StorageMain storageMainRecord)
			{
				this.storageMainRecord = storageMainRecord;
			}

			#region IUniqueIndexFailureHandler Members

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				if (Globals.IsUserInteractive)
				{
					notifier.ReportInformation(Res.GetString("0cd2bd65-fb0e-46b9-a649-5322161e605c", "While you were working, the eDocs for this record were modified. The system will now need to merge this information. Press OK to have this information loaded and then try saving again."), Res.GetString("9c8c5c0f-3040-4be1-9d23-1ce7e67f9dc7", "eDocs Reload Required"));
				}
				else
				{
					notifier.ReportInformation(Res.GetString("2e2fa704-cec1-45cc-b905-0d8bb8c240c0", "While you were working, the eDocs for this record were modified. The system will now need to merge this information."), Res.GetString("9c8c5c0f-3040-4be1-9d23-1ce7e67f9dc7", "eDocs Reload Required"));
				}
				AttemptToResolveCore();
			}

			public void AttemptToResolveCore()
			{
				var query = new ZQuery();
				query.AddToFilter(StorageMainSchema.SM_ParentFK, storageMainRecord.SM_ParentFK);
				query.AddToFilter(StorageMainSchema.PK, SQLComparisonOperator.NotEqual, storageMainRecord.PK);
				query.ReLoadExistingRows = true;
				var storageMainInDB = storageMainRecord.Factory.LoadTop1<StorageMain>(query);

				if (storageMainInDB != null)
				{
					var eDocsInRecord = storageMainRecord.eDocs.OfType<StorageDocsBase>().ToArray();

					storageMainRecord.eDocs.RemoveAll();
					storageMainInDB.eDocs.AddRange(eDocsInRecord);

					if (!storageMainRecord.RelatedParentMains.Contains(storageMainInDB))
					{
						storageMainRecord.RelatedParentMains.Add(storageMainInDB);
					}

					if (storageMainRecord.RelatedParentMains.Contains(storageMainRecord))
					{
						var e = new StorageMainUniqueIndexViolationEventArgs(storageMainInDB, storageMainRecord);
						storageMainRecord.OnHandlingUniqueIndexFailure(e);

						var requiredDocuments = storageMainInDB.GetRequiredDocuments();
						if (requiredDocuments != null)
						{
							requiredDocuments.Factory.ClearQueryCache();
							requiredDocuments.Load();
						}
					}
				}
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get
				{
					yield return StorageMainSchema.Constants.Indexes.NR_UC__SM_ParentFK;
				}
			}

			#endregion
		}

		#endregion

		#region IStorageMain Members

		public bool EDocsHasChanges => feDocs?.HasChanges ?? false;

		IeDoc IStorageMain.AddFileOrDocument(byte[] contents, string filenameOnlyWithExtension, string documentType)
		{
			var docType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, documentType));
			var action = (docType?.RT_OverrideVersions ?? false) ? FileAction.Overwrite : FileAction.CreateNew;
			return AddFileOrDocumentCore(contents, new AddFileOrDocumentDto
			{
				FileName = filenameOnlyWithExtension,
				DocumentType = documentType,
				FileAction = action,
			});
		}

		public IeDoc AddFileOrDocument(
			byte[] contents,
			string filenameOnlyWithExtension,
			string documentType,
			bool overwriteExistingFileIfNotImageFile,
			Guid visibleCompanyPK,
			Guid visibleBranchPK,
			Guid visibleDepartmentPK,
			string description,
			string source)
		{
			var action = overwriteExistingFileIfNotImageFile ? FileAction.Overwrite : FileAction.CreateNew;
			return AddFileOrDocumentCore(contents, new AddFileOrDocumentDto
			{
				FileName = filenameOnlyWithExtension,
				DocumentType = documentType,
				FileAction = action,
				VisibleCompanyPK = visibleCompanyPK,
				VisibleBranchPK = visibleBranchPK,
				VisibleDepartmentPK = visibleDepartmentPK,
				Description = description,
				Source = source,
			});
		}

		public IeDoc AddFileOrDocument(
			SubStreamableStream contents,
			string filenameOnlyWithExtension,
			string documentType,
			bool overwriteExistingFileIfNotImageFile,
			Guid visibleCompanyPK,
			Guid visibleBranchPK,
			Guid visibleDepartmentPK,
			string description,
			string source)
		{
			var action = overwriteExistingFileIfNotImageFile ? FileAction.Overwrite : FileAction.CreateNew;
			return AddFileOrDocumentCore(contents, new AddFileOrDocumentDto
			{
				FileName = filenameOnlyWithExtension,
				DocumentType = documentType,
				FileAction = action,
				VisibleCompanyPK = visibleCompanyPK,
				VisibleBranchPK = visibleBranchPK,
				VisibleDepartmentPK = visibleDepartmentPK,
				Description = description,
				Source = source,
			});
		}

		IStorageDocsBaseCollection IStorageMain.AllEDocs
		{
			get { return eDocs; }
		}

		IStorageDocsBaseCollection IStorageMain.EDocsView
		{
			get { return eDocsView; }
		}

		IStorageDocsBaseCollection IStorageMain.Documents
		{
			get { return Documents; }
		}

		IStorageDocsBaseCollection IStorageMain.Files
		{
			get { return Files; }
		}

		ZGuid IStorageMain.ParentFK
		{
			get { return SM_ParentFK; }
		}

		Enum IStorageMain.DocumentOwnerModuleID
		{
			get
			{
				if (OwnerAssemblyData != null && OwnerAssemblyData.ModuleID != null)
				{
					return OwnerAssemblyData.ModuleID.ID;
				}
				else
				{
					return null;
				}
			}
		}

		ZString IStorageMain.PhysicalLocation
		{
			get { return PhysicalLocation; }
		}

		#endregion

		#region IDocumentsView Members

		StorageDocsCollectionView documentCollectionView;
		/// <summary>
		/// A collection that contains Published Documents only.
		/// </summary>
		CargoWise.Integration.IBusinessObjectCollectionView IDocumentsView.DocumentCollectionView
		{
			get
			{
				if (documentCollectionView == null && eDocs != null)
				{
					documentCollectionView = new StorageDocsCollectionView(this, eDocs)
					{
						ExcludeUnpublishedDocuments = true
					};
				}
				return documentCollectionView;
			}
		}

		StorageFileCollectionView pdfFilesCollectionView;
		/// <summary>
		/// A collection that contains Published PDF Documents only.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "file extension")]
		CargoWise.Integration.IBusinessObjectCollectionView IDocumentsView.PDFFilesCollectionView
		{
			get
			{
				if (pdfFilesCollectionView == null && eDocs != null)
				{
					pdfFilesCollectionView = new StorageFileCollectionView(this, eDocs)
					{
						ExcludeUnpublishedDocuments = true,
						SC_DataTypeFilter = "pdf"
					};
					pdfFilesCollectionView.Rebuild();
				}
				return pdfFilesCollectionView;
			}
		}

		#endregion

		#region Physical Location

		public ZString PhysicalLocation
		{
			get
			{
				return SM_PhysicalLocation;
			}
		}

		#endregion
	}

	#region Unique Index Violation Event

	public sealed class StorageMainUniqueIndexViolationEventArgs : EventArgs
	{
		readonly StorageMain storageMainInDB;
		readonly StorageMain storageMainInMemory;

		public StorageMainUniqueIndexViolationEventArgs(StorageMain storageMainInDB, StorageMain storageMainInMemory)
		{
			this.storageMainInDB = storageMainInDB;
			this.storageMainInMemory = storageMainInMemory;
		}

		public StorageMain StorageMainInDB
		{
			get { return storageMainInDB; }
		}

		public StorageMain StorageMainInMemory
		{
			get { return storageMainInMemory; }
		}
	}

	#endregion
}
