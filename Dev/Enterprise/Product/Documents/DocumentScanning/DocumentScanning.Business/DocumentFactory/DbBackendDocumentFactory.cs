using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.PrintProcessing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public class DbBackendDocumentFactory : DocumentFactory
	{
		/// <summary>
		/// DocumentFactory
		/// </summary>
		/// <param name="factoryForEverythingExceptEDocs">Document Factory will only create/load edoc objects in itself. Other objects will be created/loaded in this factory. If null is passed in, new BusinessObjectFactory will be created on demand.</param>
		public DbBackendDocumentFactory(BusinessObjectFactory factoryForEverythingExceptEDocs)
			: base(factoryForEverythingExceptEDocs)
		{
			if (factoryForEverythingExceptEDocs is NumberedBusinessObjectFactory)
			{
				throw new ArgumentException("factoryForEverythingExceptEDocs can only be common BusinessObjectFactory, not another DocumentFactory", nameof(factoryForEverythingExceptEDocs));
			}
			NameForDebugging = FormattableString.Invariant($"eDocs Factory for DB:0, backed by '{factoryForEverythingExceptEDocs?.NameForDebugging ?? (NoResString)"Unnamed Factory"}'"); // Debugging information shouldn't be translated
		}

		public DbBackendDocumentFactory(BusinessObjectFactory factoryForEverythingExceptEDocs, DbConnection connection, DocumentFactory masterFactory, bool canBeSavedWithoutMasterFactory = false)
			: base(connection, masterFactory, factoryForEverythingExceptEDocs, canBeSavedWithoutMasterFactory)
		{
		}

		#region IDocumentFactory Members

		/// <summary>
		/// Creates an appropriate document from the details provided. Called by the print batch processor to log 
		/// a document after it has been printed, faxed or emailed.
		/// </summary>
		/// <param name="itemFK">PK for the BusinessObject that this document should be allocated to</param>
		/// <param name="formCategory">3-letter Reference type (DocManagerCode) from BusinessObject's IDocManagerSupport interface</param>
		/// <param name="pathToFile">A path to the file that should be stored</param>
		/// <param name="documentType">3-letter RefDocType code for type of document (e.g. MSC)</param>
		/// <param name="documentDescription">A description of the document (e.g. Invoice)</param>
		/// <param name="forceAllocate">Allocate document even if its type is not allowed to be autosaved to eDocs</param>
		/// <param name="fileNameWithoutExtension">File name of the document to be autosaved to eDocs</param>
		public override BusinessObject CreateAndAllocateDocument(ZGuid itemFK, ZString formCategory, ZString pathToFile, ZString documentType, ZString documentDescription, bool forceAllocate, string fileNameWithoutExtension = "", string language = "", string dataType = "", string printJobPKAsString = "")
		{
			try
			{
				ZBlob imageData = DocumentUtilities.GetFileAsBytes(pathToFile);

				if (imageData == null || imageData.IsEmpty)
				{
					OnLog(TraceEventType.Warning, Res.GetString("748c7bf0-4ec1-4437-8eb0-664f991ddf92",
						"Cannot allocate document '{0}' of type '{1}': Document is empty.", documentDescription, documentType, formCategory));
					return null;
				}

				if (documentType.IsEmpty)
				{
					documentType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				}

				ZString referenceType = AssemblyDataLookup.GetReferenceTypeFromDocManagerCode(formCategory);

				if (forceAllocate || LogDocumentType(referenceType, documentType))
				{
					if (AssemblyDataLookup.IsDocManagerCodeValid(formCategory))
					{
						var extension = Path.GetExtension(pathToFile);

						var storageFileName = string.IsNullOrEmpty(fileNameWithoutExtension) ? documentDescription : (ZString)fileNameWithoutExtension;
						storageFileName += extension;

						var document = GetDocumentToAllocate(itemFK, formCategory, documentType, documentDescription, language, storageFileName);
						document.SC_ImageData = imageData;
						document.ParentMain.SM_Type = formCategory;
						document.SC_DocType = documentType;
						document.SC_Date = ZDateTime.UtcNow;
						var description = documentDescription.Left(document.SC_DescInfo.MaxLength);
						document.SC_Desc = description;

						if (extension.IndexOf(Core.Constants.FileFormats.PDF, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							document.SC_DataType = Core.Constants.FileFormats.PDF;
						}
						else if (!string.IsNullOrEmpty(dataType) && dataType != document.SC_DataType)
						{
							document.SC_DataType = dataType;
						}

						var docType = document.DocType;

						if (docType != null)
						{
							document.SC_IsPublished = docType.RT_IsPublished;
						}

						var result = document.IsAllocated ? document : Allocate(document, itemFK);

						var parentStorage = !document.IsDeleted
							? document.ParentMain
							: result != null && !result.IsDeleted
								? result.ParentMain
								: null;

						if (parentStorage != null && !parentStorage.IsDeleted)
						{
							var parentBizO = parentStorage.DocumentOwner as EnterpriseBusinessObject;

							if (parentBizO != null && result != null)
							{
								var builder = EventLogReferenceBuilder.New()
									.AddMandatory(documentType);

								var pkAsString = result.PK.ToString();
								if (!result.IsDeleted)
								{
									builder.AddShortenable(result.SC_DescMultilingual);
								}

								builder.AddMandatory(pkAsString);
								builder.AddMandatory(printJobPKAsString);

								//TODO: Add a way to specify this? It's happening from the context of a service task, isn't it?
								if (SystemDataRegistry.Instance.DDIDocumentSource.Value && !result.SC_RDS_NKDocSource.IsEmpty)
								{
									builder.AddMandatory("NAM", documentType);
									builder.AddMandatory("SRC", result.SC_RDS_NKDocSource);
								}
								parentBizO.Logs.AddNew(Events.DocumentAllocated, builder.Build());
							}

							parentStorage.SupersedeOlderVersionDocs(result, storageFileName);
						}

						if (result != null)
						{
							if (result.ParentMain == null)
							{
								OnLog(TraceEventType.Warning, Res.GetString("3689853a-7153-4de8-a1b2-b115e1bb093e",
									"Document '{0}' of type '{1}' was created, but not allocated to a storage.", documentDescription, documentType));
							}
							else if (result.ParentMain.DocumentOwner == null)
							{
								OnLog(TraceEventType.Warning, Res.GetString("4d97b7e5-86c6-4fc5-a2c5-b8f2bf19a5db",
									"Document '{0}' of type '{1}' was allocated, but not registered with business object.", documentDescription, documentType));
							}
						}

						return result;
					}
					else
					{
						OnLog(TraceEventType.Warning, Res.GetString("e82ce24d-efbb-4269-a5e9-cd8c36e11b7b",
							"Cannot allocate document '{0}' of type '{1}': DocManager Code '{2}' is not valid", documentDescription, documentType, formCategory));
					}
				}
				else
				{
					OnLog(TraceEventType.Warning, Res.GetString("8b79e248-dfa9-496d-a57e-bc0e73bb18dd",
						"Cannot allocate document '{0}': Document Type '{1}' with Category '{2}' is not allowed to save copies on eDocs tab", documentDescription, documentType, referenceType));
				}

				return null;
			}
			catch (Exception ex)
			{
				ConvertToEdocsOffLine(ex);
				throw;
			}
		}

#if DEBUG
		public
#endif
		bool LogDocumentType(ZString refType, ZString docType)
		{
			ZQuery query = new ZQuery(RefDocTypeSchema.RT_DocType, docType);
			query.AddToFilter(new DocTypeCategoryQuery(MasterFactory, refType), JoinCondition.And);

			RefDocType documentType = MasterFactory.LoadTop1<RefDocType>(query);

			if (documentType == null)
			{
				OnLog(
					TraceEventType.Warning,
					refType.IsEmpty
						? Res.GetString("b4c48918-4b2f-45ad-9552-57a83f21bef3", "Document Category for Document Type '{0}' is not identified", docType)
						: Res.GetString("5532622e-a021-48ce-b6d0-89b770b843c2", "Document Type '{0}' with Category '{1}' is not registered in system", docType, refType));
			}

			return (documentType != null) && (bool)documentType.RT_LogSystemCreatedDocsToEDocs;
		}

		public override BusinessObject AddFileOrDocument(
			ZGuid parentBizOPK,
			string docManagerCode,
			SubStreamableStream contents,
			string filenameOnly,
			string documentType,
			string documentSource,
			bool overwriteExistingFileIfNotAnImage)
		{
			try
			{
				var parent = RetrieveExistingOrCreateStorageMainForPK(parentBizOPK, docManagerCode);
				var defaultFileAction = overwriteExistingFileIfNotAnImage ? FileAction.Overwrite : FileAction.CreateNew;
				var result = parent.AddFileOrDocument(contents, new AddFileOrDocumentDto
				{
					FileName = filenameOnly,
					DocumentType = documentType,
					FileAction = defaultFileAction,
					Source = documentSource,
				});
				if (!parent.LoadFailureReason.IsEmpty)
				{
					throw new EDocsOffLineException(parent.LoadFailureReason);
				}
				return result;
			}
			catch (SqlException ex)
			{
				ConvertToEdocsOffLine(ex);
				throw;
			}
		}

		public override BusinessObject AddFileOrDocument(
			ZGuid parentBizOPK,
			string docManagerCode,
			byte[] contents,
			string filenameOnly,
			string documentType,
			string documentSource,
			bool overwriteExistingFileIfNotAnImage)
		{
			try
			{
				var parent = RetrieveExistingOrCreateStorageMainForPK(parentBizOPK, docManagerCode);
				var defaultFileAction = overwriteExistingFileIfNotAnImage ? FileAction.Overwrite : FileAction.CreateNew;
				var result = parent.AddFileOrDocument(contents, new AddFileOrDocumentDto
				{
					FileName = filenameOnly,
					DocumentType = documentType,
					FileAction = defaultFileAction,
					Source = documentSource,
				});
				if (!parent.LoadFailureReason.IsEmpty)
				{
					throw new EDocsOffLineException(parent.LoadFailureReason);
				}
				return result;
			}
			catch (SqlException ex)
			{
				ConvertToEdocsOffLine(ex);
				throw;
			}
		}

		public override StorageMain RetrieveExistingOrCreateStorageMainForPK(ZGuid parentPK, BusinessObject parent, string refType)
		{
			var returnValue = GetStorageMainForPK(parentPK) ?? CreateStorageMain(parentPK, parent, refType);
			return returnValue;
		}

		protected override StorageMain CreateStorageMain(ZGuid parentBusinessObjectPK, BusinessObject parentBusinessObject, string refType)
		{
			var newParent = New<StorageMain>();
			if (parentBusinessObject != null && !parentBusinessObject.IsInDatabase)
			{
				newParent.UnsavedParent = parentBusinessObject;
			}
			newParent.SM_Type = refType;
			newParent.SM_ParentFK = parentBusinessObjectPK;
			AllocateToDB(newParent);
			return newParent;
		}

		public override void UpdatePublishedFlagForAllEDocs(ZString referenceType, ZString documentType, ZBool isPublished)
		{
			int[] databaseNumbers = DBQueryHelper.GetStorageDocDbNumbersIncludingMainDb().ToArray();

			var query = GetWhereQueryForUpdatingEDocs(documentType, referenceType);
			foreach (int dbNumber in databaseNumbers)
			{
				string updateCommand = string.Format((NoResString)"UPDATE {0} SET {1} = '{2}' WHERE {3}",
					DBQueryHelper.GetTableNameWithDatabasePrefix(dbNumber, StorageDocsSchema.Constants.TableName),
					StorageDocsSchema.Constants.SC_IsPublished,
					isPublished,
					query.LiteralTextADO);

				try
				{
					Db.Connection.ExecuteNonQuery(updateCommand); // Z can't handle doing subqueries in different databases.
				}
				catch (SqlException ex)
				{
					var exMsg = Res.GetString("5054333E-74B2-4093-AE87-0AB6FAE8389C", "SQL Error: '{0}' is lost. Please check your database.\r\nDetail: '{1}'", DBQueryHelper.GetTableNameWithDatabasePrefix(dbNumber, StorageDocsSchema.Constants.TableName), ex.Message);
					throw new ZCannotSaveException(exMsg, "Save failed");
				}
			}
		}

		public override void UpdateDocTypeForAllEDocs(ZPropertyInfo referenceTypeInfo, ZPropertyInfo docTypeInfo, ZPropertyInfo descInfo)
		{
			var setStatements = new List<string>();

			if (docTypeInfo.HasChanges)
			{
				setStatements.Add($"{StorageDocsSchema.Constants.SC_DocType} = '{docTypeInfo.Value}'");
			}

			if (descInfo.HasChanges)
			{
				setStatements.Add($"{StorageDocsSchema.Constants.SC_Desc} = '{descInfo.Value}'");
			}

			if (setStatements.Count > 0)
			{
				var where = GetWhereQueryForUpdatingEDocs((ZString)docTypeInfo.OriginalValue, (ZString)referenceTypeInfo.Value);
				int[] databaseNumbers = DBQueryHelper.GetStorageDocDbNumbersIncludingMainDb().ToArray();

				foreach (int dbNumber in databaseNumbers)
				{
					var tableName = DBQueryHelper.GetTableNameWithDatabasePrefix(dbNumber, StorageDocsSchema.Constants.TableName);
					var updateCommand = $"UPDATE {tableName} SET {setStatements.Aggregate((m, n) => $"{m}, {n}")} WHERE {where.LiteralTextADO}";

					try
					{
						Db.Connection.ExecuteNonQuery(updateCommand); // Z can't handle doing subqueries in different databases.
					}
					catch (SqlException ex)
					{
						var exMsg = Res.GetString("C08625A0-C116-48E0-B710-AF8B33D43FED", "SQL Error: '{0}' is lost. Please check your database.\r\nDetail: '{1}'", tableName, ex.Message);
						throw new ZCannotSaveException(exMsg, "Save failed");
					}
				}
			}
		}

		public override void UpdateDocTypeForJobRequiredDocument(ZPropertyInfo referenceTypeInfo, ZPropertyInfo docTypeInfo, ZPropertyInfo descInfo)
		{
			var setStatements = new List<string>();

			if (docTypeInfo.HasChanges)
			{
				setStatements.Add($"{JobRequiredDocumentSchema.Constants.EQ_DocType} = '{docTypeInfo.Value}'");
			}

			if (descInfo.HasChanges)
			{
				var descToCopy = (ZString)descInfo.Value;
				if (descToCopy.Length > JobRequiredDocumentSchema.EQ_DocDescription.MaxLength)
				{
					descToCopy = descToCopy.Substring(0, JobRequiredDocumentSchema.EQ_DocDescription.MaxLength);
				}
				setStatements.Add($"{JobRequiredDocumentSchema.Constants.EQ_DocDescription} = '{descToCopy}'");
				setStatements.Add($"{JobRequiredDocumentSchema.Constants.EQ_SystemLastEditTimeUtc} = GETUTCDATE()");
				setStatements.Add($"{JobRequiredDocumentSchema.Constants.EQ_SystemLastEditUser} = '{GlbStaff.CurrentUser.GS_Code.ToString()}'");
			}
			if (setStatements.Count > 0)
			{
				var where = GetQueryForUpdatingJobRequiredDocument((ZString)docTypeInfo.OriginalValue, (ZString)referenceTypeInfo.Value);
				var updateCommand = @$"
UPDATE {JobRequiredDocumentSchema.Constants.SqlSchemaName}.{JobRequiredDocumentSchema.Constants.TableName}
SET
	{setStatements.Aggregate((m, n) => $"{m}, {n}")}
WHERE
	{where.LiteralTextADO}";

				try
				{
					Db.Connection.ExecuteNonQuery(updateCommand); // Z can't handle doing subqueries in different databases.
				}
				catch (SqlException ex)
				{
					var exMsg = Res.GetString("3915AE03-F392-47E1-B567-AFC0F8E21EF9", "SQL Error: '{0}' is lost. Please check your database.\r\nDetail: '{1}'", JobRequiredDocumentSchema.Constants.TableName, ex.Message);
					throw new ZCannotSaveException(exMsg, "Save failed");
				}
			}
		}
#if DEBUG
		public
#else
		internal
#endif
		ZQuery GetWhereQueryForUpdatingEDocs(string documentType, string referenceType)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(StorageDocs));
			mainQuery.AddToFilter(StorageDocsSchema.SC_DocType, documentType);

			var docManagerCodes = new List<string>();

			foreach (var pair in AssemblyDataLookup.AllAssemblyData)
			{
				if (pair.Value.ReferenceType == referenceType)
				{
					docManagerCodes.Add(pair.Key);
				}
			}

			if (referenceType != Core.Constants.ReferenceTypes.All)
			{
				var storageMainWhereClause = GetMatchingStorageMainTypesAsWhereQuery(docManagerCodes);

				var sQLFilterQuery = string.Format("{0} IN (SELECT {1} FROM {2} {3})",
					StorageDocsSchema.SC_SM.Name,
					StorageMainSchema.PK.Name,
					DBQueryHelper.GetTableNameWithDatabasePrefix(0, StorageMainSchema.Constants.TableName),
					storageMainWhereClause);

				mainQuery.AddFilterAndZSQLParameterCollection(sQLFilterQuery, new ZSqlParameterCollection());
			}

			return mainQuery;
		}
#if DEBUG
		public
#else
		internal
#endif
		ZQuery GetQueryForUpdatingJobRequiredDocument(string documentType, string referenceType)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(JobRequiredDocument));
			mainQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, documentType);

			if (referenceType != Core.Constants.ReferenceTypes.All)
			{
				mainQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocCategory, referenceType);
			}

			return mainQuery;
		}

		string GetMatchingStorageMainTypesAsWhereQuery(List<string> docManagerCodes)
		{
			var query = new ZQuery();
			query.AddToFilter(JoinCondition.Or, StorageMainSchema.SM_Type, docManagerCodes);

			return query.GetAsWhereClause(true);
		}

#if DEBUG
		public override BusinessObject CreateDocument(ZGuid parentPK)
		{
			var main = Load<StorageMain>(parentPK);
			if (main != null)
			{
				return main.Documents.AddNew();
			}
			else
			{
				return NewWithParent(typeof(StorageDocs));
			}
		}

		public override BusinessObject[] Load(Type bizOType, ZQuery sQLFilter)
		{
			actionBeforeLoadForTest?.Invoke(sQLFilter);
			return base.Load(bizOType, sQLFilter);
		}

		public static IDisposable SetActionBeforeLoadForTest(Action<ZQuery> action)
		{
			actionBeforeLoadForTest = action;
			return new DisposableAction(() => { actionBeforeLoadForTest = null; });
		}

		[ThreadStatic]
		static Action<ZQuery> actionBeforeLoadForTest;
#endif

		public override string[] NamesOfMissingDbs
		{
			get { return DBQueryHelper.GetDBNamesMissing(); }
		}

		public override string GetDatabaseName(int databaseNumber)
		{
			return DBQueryHelper.GetDatabaseName(databaseNumber);
		}

		public override int LastWriteableDatabaseWithFreeSpace()
		{
			return DBQueryHelper.LastWritableDatabaseWithFreeSpace();
		}

		public override DbWriteableState GetDbWriteableState(int dbNumber)
		{
			return DBQueryHelper.GetDbWriteableState(dbNumber);
		}

		public override StorageMain RetrieveExistingOrCreateStorageMainForPK(ZGuid bizOPK, string docManagerCode)
		{
			return RetrieveExistingOrCreateStorageMainForPK(bizOPK, null, docManagerCode);
		}

		public override StorageMain RetrieveExistingOrCreateStorageMain(BusinessObject bizO, string docManagerCode)
		{
			return RetrieveExistingOrCreateStorageMainForPK(bizO.PK, bizO, docManagerCode);
		}

		public override StorageMain GetStorageMainForPK(ZGuid bizOPK)
		{
			return MasterFactory.LoadFromUniqueKey<StorageMain>(StorageMainSchema.SM_ParentFK, bizOPK);
		}

		/// <summary>
		/// Returns a count of all documents belonging to a parent, excluding the ones marked as deleted and not published
		/// </summary>
		public override int GetCountOfPublishedDocumentsAndFilesForStorageMain(StorageMain parent)
		{
			if (parent != null)
			{
				var childFactory = GetFactory(parent.SM_DB);
				var filter = new ZQuery(StorageDocsSchema.SC_SM, parent.PK);
				filter.AddToFilter(StorageDocsSchema.SC_IsDeleted, ZBool.False);
				filter.AddToFilter(StorageDocsSchema.SC_IsPublished, ZBool.True);
				return childFactory.GetDatabaseCount(typeof(StorageDocs), filter);
			}
			else
			{
				return 0;
			}
		}

		public override IeDoc FindEDocsFromAllSDDatabasesByPK(ZGuid uniqueKey)
		{
			IeDoc result = null;

			foreach (int databaseNumber in DBQueryHelper.GetStorageDocDbNumbersIncludingMainDb())
			{
				var factory = GetFactory(databaseNumber);
				if (factory != null)
				{
					result = factory.Load<StorageDocsBase>(uniqueKey);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region Allocating documents

#if DEBUG
		public bool IsNewlyAllocatedDocDeleted;
#endif

		/// <summary>
		/// Allocates a document to a new StorageMain parent given the PK of the BizO it 
		/// should be allocated to (e.g. the Shipment PK the document belongs to)
		/// </summary>
		/// <param name="document">The document that needs to be allocated</param>
		/// <param name="itemFK">The PK of the BusinessObject that the document is to be allocated to (e.g. the PK of the Shipment)</param>
		/// <returns>The allocated StorageDocs object</returns>
		public override StorageDocsBase Allocate(StorageDocsBase document, ZGuid itemFK)
		{
			if (!itemFK.IsEmpty)
			{
				if (document.ParentMain.SM_ParentFK != itemFK)
				{
					var newStorageParent = GetNewStorageMainAndCleanupOldReferences(document, itemFK);
					AllocateToDB(newStorageParent);

					StorageDocsBase newlyAllocatedDoc = document is StorageDocs ? newStorageParent.Documents.AddNew() : newStorageParent.Files.AddNew();

					try
					{
						newlyAllocatedDoc.CopyPersistentValuesFrom(document);
					}
					catch (Exception ex)
					{
						if (ex.IsExceptionPresentIncludingInner<OutOfMemoryException>() ||
							ex.IsExceptionPresentIncludingInner<ExternalStorageException>())
						{
							newlyAllocatedDoc.Delete();
						}
#if DEBUG
						IsNewlyAllocatedDocDeleted = newlyAllocatedDoc.IsDeleted;
#endif
						throw;
					}

					document.Delete();
					return newlyAllocatedDoc;
				}
				else
				{
					return document;
				}
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Tries to find an existing document matching the ItemFK, RefType, DocType and Description.
		/// Returns the matching document, or a new document if no existing one is found.
		/// The new document can be in the main database regardless of where it will end up.
		/// </summary>
#if DEBUG
		public
#endif
		StorageDocsBase GetDocumentToAllocate(ZGuid itemFK, ZString refType, ZString documentType, ZString documentDescription, string language = "", string fileName = "")
		{
			StorageDocsBase returnDocument = null;
			var query = new ZQuery();
			query.AddToFilter(JoinCondition.And, StorageMainSchema.SM_ParentFK, SQLComparisonOperator.Equal, itemFK);
			query.AddToFilter(JoinCondition.And, StorageMainSchema.SM_Type, SQLComparisonOperator.Equal, refType);
			var parent = MasterFactory.LoadTop1<StorageMain>(query);

			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			var extension = Path.GetExtension(fileName);
			var isImageFile = FileImporter.IsSupportedImageFile(extension);
			var uniqueFileName = (ZString)fileNameWithoutExtension;

			if (parent != null)
			{
				var documentQuery = GetQueryForMostRecentDoc(documentType);
				parent.Documents.CollectionToFilter.LoadWithMoreFiltering(documentQuery);
				parent.Documents.Sort(StorageDocsSchema.Constants.SC_Date, System.ComponentModel.ListSortDirection.Descending);

				returnDocument = GetExistingDocument(parent, documentDescription, language) ?? GetExistingFile(parent, documentDescription, language);
				uniqueFileName = Path.GetFileNameWithoutExtension(parent.eDocsView.GetUniqueFilename(fileNameWithoutExtension, extension));
				if (returnDocument == null)
				{
					returnDocument = isImageFile ? parent.Documents.AddNew() : parent.Files.AddNew();
				}
			}
			else
			{
				returnDocument = isImageFile ? NewWithParent(typeof(StorageDocs)) : NewWithParent(typeof(StorageFile));
			}

			returnDocument.SC_FileName = uniqueFileName.SubstringSafe(0, StorageDocsSchema.SC_FileName.MaxLength);
			returnDocument.SC_Language = language;
			returnDocument.SuppressDocEventLog = true;
			returnDocument.SC_IsSystemGenerated = true;

			return returnDocument;
		}

		StorageDocsBase GetExistingDocument(StorageMain parent, ZString documentDescription, string language)
		{
			var documentDescriptionWithMaxLength = documentDescription.Left(AutoStorageDocs.Schema.SC_DescMaxLength);
			var document = parent.Documents.Cast<StorageDocs>().FirstOrDefault(
				d => (d.SC_DescMultilingual.GetUnresolvedString() == documentDescriptionWithMaxLength || d.SC_DescMultilingual.ToString() == documentDescriptionWithMaxLength)
				&&
				(string.IsNullOrEmpty(d.SC_Language) || d.SC_Language.EqualsIgnoringCase(language)));
			return (document != null && document.SaveThisVersion) ? null : document;
		}

		StorageDocsBase GetExistingFile(StorageMain parent, ZString documentDescription, string language)
		{
			var documentDescriptionWithMaxLength = documentDescription.Left(AutoStorageDocs.Schema.SC_DescMaxLength);
			var document = parent.Files.Cast<StorageFile>().FirstOrDefault(
				f => (f.SC_DescMultilingual.GetUnresolvedString() == documentDescriptionWithMaxLength || f.SC_DescMultilingual.ToString() == documentDescriptionWithMaxLength)
				&&
				(string.IsNullOrEmpty(f.SC_Language) || f.SC_Language.EqualsIgnoringCase(language)));
			return (document != null && document.SaveThisVersion) ? null : document;
		}

		ZQuery GetQueryForMostRecentDoc(ZString documentType)
		{
			var documentQuery = new ZQuery(StorageDocsSchema.SC_IsSystemGenerated, ZBool.True);
			documentQuery.AddToFilter(JoinCondition.And, StorageDocsSchema.SC_IsDeleted, SQLComparisonOperator.Equal, ZBool.False);
			documentQuery.AddToFilter(JoinCondition.And, StorageDocsSchema.SC_DocType, SQLComparisonOperator.Equal, documentType);
			return documentQuery;
		}

		/// <summary>
		/// Gets a document's new parent record, and removes it from its old collections / parents if they existed.
		/// If the old parent is now an orphan record for an unallocated document, delete it.
		/// </summary>
#if DEBUG
		public
#endif
		StorageMain GetNewStorageMainAndCleanupOldReferences(StorageDocsBase document, ZGuid itemFK)
		{
			StorageMain newParent;
			var existingParent = GetStorageMainForPK(itemFK);

			// ToDo: This try-catch block is added for WI00713090. If the issue reported is fixed, please remove this block.
			try
			{
				var previousParent = document.ParentMain;

				if (existingParent != null)
				{
					if (existingParent.PK != previousParent.PK)
					{
						RemoveDocumentFromOldCollections(document, previousParent);
					}

					newParent = existingParent;
				}
				else
				{
					var previousStorageType = document.SM_Type;

					if (document.IsAllocated)
					{
						RemoveDocumentFromOldCollections(document, previousParent);
						CreateParentFor(document);
					}

					newParent = document.ParentMain;
					newParent.SM_Type = previousStorageType;
					newParent.SM_ParentFK = itemFK;
				}
			}
			catch (NullReferenceException ex)
			{
				var message = $@"existingParent is null: {existingParent == null}
document is null: {document == null}
document is allocated: {document?.IsAllocated}
document.ParentMain is null: {document?.ParentMain == null}
";
				ErrorReporter.ReportOnce("WI00713090_NRE_GetNewStorageMainAndCleanupOldReferences", message, ex);

				// The existing logic does not expect a null StorageMain returned. Hence, this exception will not be swallowed here.
				throw;
			}

			return newParent;
		}

		/// <summary>
		/// Will also delete the old parent if it was the parent for an unallocated document.
		/// </summary>
		void RemoveDocumentFromOldCollections(StorageDocsBase document, StorageMain oldParent)
		{
			if (oldParent != null)
			{
				oldParent.eDocs.Remove(document);
				oldParent.DeleteIfUnallocated();
			}
		}

		public override void AllocateToDB(StorageMain parentRecord)
		{
			if (parentRecord.SM_DB.IsEmpty)
			{
				parentRecord.SM_DB = DBQueryHelper.LastWritableDatabaseWithFreeSpace();
			}
		}

		#endregion

		#region Unallocating documents

		/// <summary>
		/// Unallocates a document - moves it from Odyssey_SD00x to Odyssey database.
		/// previous parent is not deleted because other documents could be using it
		/// in any case it's OK to have a parent without documents if the parent
		/// belongs to a BusinessObject
		/// </summary>
		public override StorageDocsUnallocated Unallocate(StorageDocsBase document)
		{
			StorageMain oldStorageParent = document.ParentMain;
			StorageDocsUnallocated unallocatedDoc = ConvertFromAllocated(document);
			document.SC_ImageData = null;
			if (oldStorageParent != null)
			{
				oldStorageParent.eDocs.RemoveAndDelete(document);
			}
			if (unallocatedDoc.SC_ImageData.Length > 1024 * 1024)
			{
				GCWrapper.ReclaimMemory(ref document);
			}
			return unallocatedDoc;
		}

		#endregion

		#region Factory Management

		public override int GetFactoryNumberInUse(IBusinessObjectCollection bizOCollection)
		{
			NumberedBusinessObjectFactory numberedFactory = bizOCollection.Factory as NumberedBusinessObjectFactory;
			return numberedFactory.DBNumber;
		}

#if DEBUG
		public
#endif
		int GetFactoryNumberInUse(BusinessObject bizO)
		{
			NumberedBusinessObjectFactory numberedFactory = bizO.Factory as NumberedBusinessObjectFactory;
			return numberedFactory.DBNumber;
		}

		public override NumberedBusinessObjectFactory GetFactory(ZInt number)
		{
			NumberedBusinessObjectFactory returnFactory;

			if (number == 0)
			{
				returnFactory = this;
			}
			else
			{
				if (OtherDBFactories[number] == null)
				{
					returnFactory = GetNumberedBusinessObjectFactory(number);
					OtherDBFactories[number] = returnFactory;
					ChildFactories.Add(returnFactory);
				}
				else
				{
					returnFactory = (NumberedBusinessObjectFactory)OtherDBFactories[number];
				}
			}

			return returnFactory;
		}

		protected virtual NumberedBusinessObjectFactory GetNumberedBusinessObjectFactory(int number)
		{
			return new NumberedBusinessObjectFactory(number, this)
			{
				NameForDebugging = "eDocsFactory for DB:" + number
			};
		}

		readonly Hashtable OtherDBFactories = new Hashtable();

		#endregion

		#region Add File

		/// <summary>
		/// Imports a file into enterprise. This method does not convert files to TIF (e.g. jpg to gif).
		/// You must already have converted it to a TIF file.
		/// Pass in ZGuid.Empty as visibleCompanyPK/visibleBranchPK/visibleDepartmentPK if the document is not company/branch/department specific
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public override bool Import(byte[] contents, ZString filenameOnly, ZString userSuppliedRefType, ZGuid userSuppliedRefPK, ZString userSuppliedDocType, ZString userSuppliedDocSource, ZGuid visibleCompanyPK, ZGuid visibleBranchPK, ZGuid visibleDepartmentPK, string fileFullPath, bool checkSecurity, out string finalRestrictedDocType)
		{
			finalRestrictedDocType = string.Empty;
			if (checkSecurity && userSuppliedDocType != ZString.Empty)
			{
				if (((ICodeDescriptionPairList)SystemDataRegistry.Instance.DocumentTypesRestrictedForImport.Value).ContainsCode(userSuppliedDocType))
				{
					finalRestrictedDocType = userSuppliedDocType;
					return false;
				}
			}

			var importer = new FileImporter(MasterFactory, true);
			importer.ImportingUserPK = GlbStaff.CurrentUser.PK;

			if (!userSuppliedRefPK.IsEmpty || !userSuppliedRefType.IsEmpty || !userSuppliedDocType.IsEmpty)
			{
				return ImportByUserInformation(importer, contents, filenameOnly, userSuppliedRefType, userSuppliedRefPK, userSuppliedDocType, userSuppliedDocSource, fileFullPath, visibleCompanyPK, visibleBranchPK, visibleDepartmentPK);
			}
			else if (FileImporter.IsSupported(Path.GetExtension(filenameOnly)))
			{
				return ImportByBarcodes(importer, contents, filenameOnly, checkSecurity, userSuppliedDocSource, out finalRestrictedDocType);
			}

			return false;
		}

		/// <summary>
		/// Imports a file into enterprise. This method does not convert files to TIF (e.g. jpg to gif).
		/// You must already have converted it to a TIF file.
		/// Pass in ZGuid.Empty as visibleCompanyPK/visibleBranchPK/visibleDepartmentPK if the document is not company/branch/department specific
		/// </summary>
		public override bool Import(byte[] contents, ZString filenameOnly, ZString userSuppliedRefType, ZGuid userSuppliedRefPK, ZString userSuppliedDocType, ZString userSuppliedDocSource, ZGuid visibleCompanyPK, ZGuid visibleBranchPK, ZGuid visibleDepartmentPK, string fileFullPath = "", bool checkSecurity = false)
		{
			return Import(contents, filenameOnly, userSuppliedRefType, userSuppliedRefPK, userSuppliedDocType, userSuppliedDocSource, visibleCompanyPK, visibleBranchPK, visibleDepartmentPK, fileFullPath, checkSecurity, out _);
		}

#if DEBUG
		protected
#endif
		bool ImportByUserInformation(FileImporter importer, byte[] contents, ZString filenameOnly, ZString userSuppliedRefType, ZGuid userSuppliedRefPK, ZString userSuppliedDocType, ZString userSuppliedDocSource, string fileFullPath, ZGuid visibleCompanyPK, ZGuid visibleBranchPK, ZGuid visibleDepartmentPK)
		{
			var imported = false;
			if (!userSuppliedRefPK.IsEmpty && !userSuppliedRefType.IsEmpty)
			{
				var parent = RetrieveExistingOrCreateStorageMainForPK(userSuppliedRefPK, userSuppliedRefType);
				if (parent != null)
				{
					var filenameOnlyMinusPrefixInfo = GetFileNameMinusPrefix(filenameOnly);
					var eDocAllocated = parent.AddFileOrDocument(contents, new AddFileOrDocumentDto
					{
						FileName = filenameOnlyMinusPrefixInfo,
						DocumentType = userSuppliedDocType,
						VisibleCompanyPK = visibleCompanyPK,
						VisibleBranchPK = visibleBranchPK,
						VisibleDepartmentPK = visibleDepartmentPK,
						Source = userSuppliedDocSource,
					});

					if (eDocAllocated != null)
					{
						AddImportLogs(eDocAllocated);
						imported = true;
					}
				}
			}
			else if (FileImporter.IsSupported(Path.GetExtension(filenameOnly)))
			{
				var newDocument = New<StorageDocsUnallocated>();
				newDocument.SM_Type = (!userSuppliedRefType.IsEmpty) ? userSuppliedRefType : (ZString)Core.Constants.DocManagerCodes.Unallocated;
				newDocument.SC_DocType = userSuppliedDocType;
				newDocument.SC_GC_Company = visibleCompanyPK;
				newDocument.SC_GB_Branch = visibleBranchPK;
				newDocument.SC_GE_Department = visibleDepartmentPK;
				newDocument.SC_RDS_NKDocSource = userSuppliedDocSource;
				newDocument.SC_FileName = Path.GetFileNameWithoutExtension(filenameOnly);
				newDocument.SC_ImageData = contents;

				if (!string.IsNullOrEmpty(fileFullPath))
				{
					newDocument.ScannedBarcodeValue = BarcodeHelper.JoinValidBarcodes(importer.ReadBarcodesFromFile(fileFullPath), StorageDocsBarcode.Schema.SCB_BarcodeMaxLength);
				}

				imported = true;
			}

			return imported;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		bool ImportByBarcodes(FileImporter importer, byte[] contents, ZString filenameOnly, bool checkSecurity, ZString userSuppliedDocSource, out string finalRestrictedDocType)
		{
			finalRestrictedDocType = "";
			using var tempFile = TempFile.NewWithExtension(Path.GetExtension(filenameOnly));
			using (var writer = new FileStream(tempFile.Filename, FileMode.OpenOrCreate))
			{
				writer.Write(contents, 0, contents.Length);
			}

			var denied = 0;
			var ea = importer.ExecuteSort(tempFile.Filename);
			foreach (DocumentResult result in ea)
			{
				try
				{
					var barcodeInfo = result as BaseBarcode;
					if (checkSecurity && barcodeInfo != null)
					{
						if (((ICodeDescriptionPairList)SystemDataRegistry.Instance.DocumentTypesRestrictedForImport.Value).ContainsCode(barcodeInfo.DocType))
						{
							++denied;
							finalRestrictedDocType = barcodeInfo.DocType;
							continue;
						}
					}

					var newDocument = New<StorageDocsUnallocated>();
					newDocument.SM_Type = Core.Constants.DocManagerCodes.Unallocated;
					newDocument.SC_FileName = Path.GetFileNameWithoutExtension(filenameOnly);
					newDocument.SC_ImageData = DocumentUtilities.GetFileAsBytes(result.FilePath);
					newDocument.SC_RDS_NKDocSource = userSuppliedDocSource;
					ZGuid parentFK;

					if (barcodeInfo != null)
					{
						if (!barcodeInfo.DocManagerCode.IsEmpty)
						{
							newDocument.SM_Type = barcodeInfo.DocManagerCode;
						}

						parentFK = barcodeInfo.RefPK;

						newDocument.SC_DocType = barcodeInfo.DocType;
						newDocument.ScannedBarcodeValue = barcodeInfo.ScannedBarcodeValue;
					}
					else
					{
						parentFK = ZGuid.Empty;
					}

					if (!parentFK.IsEmpty && newDocument.IsAutoAllocatable && importer.IsAutoAllocate)
					{
						var filenameOnlyMinusPrefixInfo = GetFileNameMinusPrefix(filenameOnly);
						newDocument.SC_FileName = Path.GetFileNameWithoutExtension(filenameOnlyMinusPrefixInfo);
						var allocatedDoc = Allocate(ConvertFromUnallocated(newDocument), parentFK);
						AddImportLogs(allocatedDoc);
					}
					else
					{
						_ = newDocument.SetAllocateStatus(result);
					}
				}
				finally
				{
					File.Delete(result.FilePath);
				}
			}

			return ea.FileDetailsCount > denied;
		}

		internal string GetFileNameMinusPrefix(ZString filenameOnly)
		{
			if (!filenameOnly.StartsWith("["))
			{
				return filenameOnly;
			}

			var endOfPrefixInfo = filenameOnly.IndexOf(']');
			if (endOfPrefixInfo == -1)
			{
				return filenameOnly;
			}

			var extension = Path.GetExtension(filenameOnly);
			var filenameOnlyMinusPrefixInfo = filenameOnly.SubstringSafe(endOfPrefixInfo + 1).Trim();
			filenameOnlyMinusPrefixInfo = filenameOnlyMinusPrefixInfo.Length > extension.Length && !filenameOnlyMinusPrefixInfo.StartsWith(".") ? filenameOnlyMinusPrefixInfo : (string)filenameOnly;
			return filenameOnlyMinusPrefixInfo;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "SCDesc in logs")]
		void AddImportLogs(StorageDocsBase allocatedDoc)
		{
			var documentOwner = allocatedDoc != null && allocatedDoc.ParentMain != null ? allocatedDoc.ParentMain.DocumentOwner as EnterpriseBusinessObject : null;
			if (documentOwner != null)
			{
				documentOwner.Logs.AddNew(AutoEvents.DocumentImported, allocatedDoc.CreateReference());
			}
		}

		#endregion

		#region Convert from Allocated / Unallocated

		public override StorageDocs ConvertFromUnallocated(StorageDocsUnallocated document)
		{
			var result = (StorageDocs)NewWithParent(typeof(StorageDocs));
			result.CopyPersistentValuesFrom(document);
			result.ParentMain.SM_Type = document.SC_DataType.Left(StorageMain.Schema.SM_TypeMaxLength);
			result.SC_DataType = document.EDocFormat;
			document.Delete();
			return result;
		}

		public override StorageDocsUnallocated ConvertFromAllocated(StorageDocsBase document)
		{
			StorageDocsUnallocated result = New<StorageDocsUnallocated>();
			result.CopyPersistentValuesFrom(document);
			result.SM_Type = document.ParentMain.SM_Type;
			result.SC_DataType = Core.Constants.DocManagerCodes.Unallocated;
			return result;
		}

		public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
		{
			return new DocumentFactoryProvider().GetFactory(FactoryForEverythingExceptEDocs);
		}

		#endregion

		#region Saving

		protected override void CallBusinessObjectsOnFactorySaved(bool saveSucceded)
		{
			base.CallBusinessObjectsOnFactorySaved(saveSucceded);

			if (saveSucceded)
			{
				OnSaveSuccessful();
			}
		}

		#endregion

		#region IStorageMainForPK Members

		public override IDocumentsView GetStorageMain(ZGuid bizObjPK)
		{
			return GetStorageMainForPK(bizObjPK);
		}

		#endregion

		#region Implementation

		DocManagerDBHelper DBQueryHelper
		{
			get
			{
				if (fDBQueryHelper == null)
				{
					fDBQueryHelper = new DocManagerDBHelper();
				}
				return fDBQueryHelper;
			}
		}

		DocManagerDBHelper fDBQueryHelper;

		protected override bool SupportsZQuery()
		{
			return true;
		}

		#endregion
	}
}
