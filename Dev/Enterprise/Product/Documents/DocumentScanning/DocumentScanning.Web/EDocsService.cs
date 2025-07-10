using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Services
{
	public class EDocsService : IEDocsService
	{
		public EDocUpdateResult AddOrUpdateEDoc(string tablePrefix, Guid businessObjectPk, EDocDetail eDocDetail, byte[] contents, bool includeUnpublished, ZGuid? contactPK = null)
		{
			EDocUpdateResult result = null;
			IeDoc eDoc = null;
			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				if (contactPK != null && !eDocDetail.IsPublished)
				{
					result = new EDocUpdateResult { Status = EDocUpdateStatus.UnpublishedForContact };
					return;
				}

				var bizFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				bizFactory.NameForDebugging = "EDocs Service";
				var documentFactory = new DocumentFactoryProvider().GetFactory(bizFactory);
				documentFactory.NameForDebugging = "EDocs Service";
				var businessObject = documentFactory.Load(tablePrefix, businessObjectPk);
				if (businessObject == null)
				{
					result = new EDocUpdateResult { Status = EDocUpdateStatus.BusinessObjectNotFound };
					return;
				}

				var docManagerSupport = businessObject as IDocManagerSupport;
				if (docManagerSupport == null || docManagerSupport.DocManagerInfo == null)
				{
					result = new EDocUpdateResult { Status = EDocUpdateStatus.BusinessObjectNotSupported };
					return;
				}

				var docManagerInfo = docManagerSupport.DocManagerInfo;
				docManagerInfo.UseBusinessEntityFactoryAsInternal = true;

				var docType = bizFactory.Load<RefDocType>(eDocDetail.DocumentTypePK);
				if (!ValidateDocType(docType, documentFactory, contactPK))
				{
					result = new EDocUpdateResult { Status = EDocUpdateStatus.InvalidDocumentType };
					return;
				}

				if (eDocDetail.Id != Guid.Empty)
				{
					eDoc = docManagerInfo.Documents.GetFromUniqueKey(eDocDetail.Id) ?? docManagerInfo.Files.GetFromUniqueKey(eDocDetail.Id);
					if (!IsAvailableEDoc(eDoc as StorageDocsBase, new eDocsWebSecurity(documentFactory, contactPK), includeUnpublished))
					{
						result = new EDocUpdateResult { Status = EDocUpdateStatus.EDocNotFound };
						return;
					}

					eDoc.DocType = docType.RT_DocType;
					eDoc.Description = eDocDetail.Description;
					eDoc.IsPublished = eDocDetail.IsPublished;
					if (contents != null)
					{
						eDoc.ImageData = contents; // TODO: Ideally we would pass the stream through to the eDoc
					}
				}
				else
				{
					if (contents == null)
					{
						result = new EDocUpdateResult { Status = EDocUpdateStatus.ContentNotSpecified };
						return;
					}

					if (contents.Length == 0)
					{
						result = new EDocUpdateResult { Status = EDocUpdateStatus.InvalidDocument };
						return;
					}

					try
					{
						eDoc = docManagerInfo.AddFileOrDocument(
							contents,
							eDocDetail.FileName,
							docType.RT_DocType,
							overwriteExistingFileIfNotImageFile: false);
					}
					catch (ImageFormatException)
					{
						result = new EDocUpdateResult { Status = EDocUpdateStatus.InvalidDocument };
						return;
					}
					catch (VirusDetectedException)
					{
						result = new EDocUpdateResult { Status = EDocUpdateStatus.VirusDetected };
						return;
					}

					eDoc.Description = eDocDetail.Description;
					eDoc.IsPublished = eDocDetail.IsPublished;

					docManagerInfo.AddLogsForNewDocument(businessObject, eDoc);
				}

				docManagerInfo.MasterFactory.Save();
			}, null);

			return result ?? new EDocUpdateResult
			{
				Status = EDocUpdateStatus.Succeeded,
				EDocDetail = GetEDocDetail((StorageDocsBase)eDoc)
			};
		}

		bool ValidateDocType(RefDocType docType, BusinessObjectFactory factory, ZGuid? contactPK)
		{
			if (docType == null)
			{
				return false;
			}

			if (contactPK == null)
			{
				return true;
			}

			var security = new eDocsWebSecurity(factory, contactPK);
			return security.CanViewDocument(docType) && docType.RT_IsPublished;
		}

		public EDocDetail[] GetEDocDetails(Guid parentPK, bool includeDeleted, bool includeUnpublished, ZGuid? contactPK = null) =>
			GetAvailableEDocs(parentPK, includeDeleted, includeUnpublished, contactPK)
			.Select(d => GetEDocDetail(d))
			.ToArray();

		public int GetEDocCount(Guid entityPK, ZGuid? contactPK = null) =>
			GetAvailableEDocs(entityPK, false, false, contactPK)
			.Count();

		IEnumerable<StorageDocsBase> GetAvailableEDocs(Guid parentPK, bool includeDeleted, bool includeUnpublished, ZGuid? contactPK = null)
		{
			var query = new ZQuery(StorageMainSchema.SM_ParentFK, parentPK);
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			factory.NameForDebugging = "EDocs Service";

			var security = new eDocsWebSecurity(factory, contactPK);
			var storageMain = factory.LoadTop1<StorageMain>(query);
			if (storageMain == null)
			{
				return Enumerable.Empty<StorageDocsBase>();
			}

			if (contactPK == null && storageMain.DocumentOwner is IEDocsSecurity parent && !parent.EdocsSecurityCheckpoint.IsAllowed)
			{
				return Enumerable.Empty<StorageDocsBase>();
			}

			var eDocs = new StorageDocsCollectionViewBase(storageMain.eDocs)
			{
				IncludeDeletedDocuments = includeDeleted,
				ExcludeUnpublishedDocuments = !includeUnpublished
			};

			return eDocs
				.OfType<StorageDocsBase>()
				.Where(d => security.CanViewDocument(d.DocType));
		}

		public EDocImageData GetEDocImageData(Guid eDocPK, int databaseNumber, bool includeUnpublished, ZGuid? contactPK = null)
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			documentFactory.NameForDebugging = "EDocs Service";
			var numberedFactory = documentFactory.GetFactory(databaseNumber);
			var storageDocs = numberedFactory.Load<StorageDocs>(new ZGuid(eDocPK));

			if (IsAvailableEDoc(storageDocs, new eDocsWebSecurity(documentFactory, contactPK), includeUnpublished))
			{
				var filename = storageDocs.SC_FileName + (string.IsNullOrEmpty(storageDocs.SC_DataType) ? "" : "." + storageDocs.SC_DataType);
				return new EDocImageData { FullFileName = filename, Data = storageDocs.SC_ImageData };
			}
			return null;
		}

		public void DeliverEDoc(Guid eDocPK, int databaseNumber, DeliveryInstructionsBase deliveryInstructions, bool includeUnpublished, ZGuid? contactPK = null)
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			documentFactory.NameForDebugging = "EDocs Service";
			var numberedFactory = documentFactory.GetFactory(databaseNumber);
			var storageDocs = numberedFactory.Load<StorageDocs>(new ZGuid(eDocPK));

			if (IsAvailableEDoc(storageDocs, new eDocsWebSecurity(documentFactory, contactPK), includeUnpublished))
			{
				var documentPack = new DocumentPack();
				documentPack.Add(storageDocs);

				var adapter = new DeliveryInstructionsAdapter(deliveryInstructions);
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					printTask.Run(adapter);
				}
			}
		}

		public void DeleteEDoc(Guid eDocPK, int databaseNumber, bool includeUnpublished, ZGuid? contactPK = null)
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			documentFactory.NameForDebugging = "EDocs Service";
			var numberedFactory = documentFactory.GetFactory(databaseNumber);
			var storageDocs = numberedFactory.Load<StorageDocs>(new ZGuid(eDocPK));

			if (IsAvailableEDoc(storageDocs, new eDocsWebSecurity(documentFactory, contactPK), includeUnpublished) && CanDeleteEDoc())
			{
				storageDocs.SC_IsDeleted = ZBool.True;
				documentFactory.Save();
			}

			bool CanDeleteEDoc()
			{
				return contactPK == null || storageDocs.SC_SystemCreateUser == User.WebUserCode;
			}
		}

		bool IsAvailableEDoc(StorageDocsBase storageDocs, eDocsWebSecurity eDocsSecurity, bool includeUnpublished)
		{
			return storageDocs != null &&
				!storageDocs.SC_IsDeleted &&
				(includeUnpublished || storageDocs.SC_IsPublished) &&
				storageDocs.IsAvailableForCurrentEnvContext() &&
				eDocsSecurity.CanViewDocument(storageDocs.DocType);
		}

		public bool IsFileAcceptable(string fileName)
		{
			return !new FileTypeValidation().IsDangerousFile(fileName);
		}

		public bool IsFileAcceptable(Stream fileData, out string apparentFileType)
		{
			var result = !new FileTypeValidation().IsDangerousFile(fileData, out string apparentFileType2);
			apparentFileType = apparentFileType2;
			return result;
		}

		public string GetReferenceType(string entityDocManagerCode)
		{
			return AssemblyDataLookup.AllAssemblyData.GetReferenceTypeFromDocManagerCode(entityDocManagerCode);
		}

		public IEnumerable<RefDocType> GetRefDocTypes(string referenceType, bool includeUnpublished, ZGuid? contactPK = null)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = "EDocs Service GetRefDocTypes" };
			var query = new ZQuery();
			query.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.All);
			query.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, referenceType);
			query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_IsActive, true);
			if (!includeUnpublished)
			{
				query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_IsPublished, true);
			}
			query.OrderBy = RefDocTypeSchema.RT_DocType.Name;

			// This is a workaround to clear the UberFactory cache
			query.ReLoadExistingRows = true;

			var security = new eDocsWebSecurity(factory, contactPK);
			return factory.Load<RefDocType>(query).Where(type => security.CanViewDocument(type));
		}

		static EDocDetail GetEDocDetail(StorageDocsBase eDoc)
		{
			var lastEdited = ((IeDoc)eDoc).LastEdited;
			return new EDocDetail()
			{
				Id = eDoc.PK.ToGuid(),
				DateAdded = DateTime.SpecifyKind(eDoc.SC_Date.ToSmallDateTimeFloor().ToDateTime(), DateTimeKind.Utc),
				DocumentTypePK = eDoc.DocType == null ? Guid.Empty : eDoc.DocType.PK.ToGuid(),
				DocumentTypeCode = eDoc.DocType?.RT_DocType,
				DocumentTypeDescription = eDoc.DocType?.RT_DescMultilingual,
				DataType = eDoc.SC_DataType,
				OwnerReadableName = eDoc.ParentMain?.DocumentOwner?.HumanReadableName ?? string.Empty,
				Description = eDoc.SC_DescMultilingual,
				FileName = eDoc.SC_FileName,
				IsSystemGenerated = eDoc.SC_IsSystemGenerated,
				IsPublished = eDoc.SC_IsPublished,
				IsDeleted = eDoc.SC_IsDeleted,
				CreatingUser = eDoc.SC_AddingUser,
				LastEditUser = eDoc.SC_LastEditingUser,
				LastEditDate = DateTime.SpecifyKind(lastEdited.IsEmpty ? DateTime.MinValue : lastEdited.ToDateTime(), DateTimeKind.Utc),
				DatabaseNumber = eDoc.ParentMain.SM_DB
			};
		}
	}
}
