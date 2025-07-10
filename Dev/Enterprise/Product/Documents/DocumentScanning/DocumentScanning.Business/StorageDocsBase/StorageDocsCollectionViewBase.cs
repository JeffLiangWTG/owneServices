using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsCollectionViewBase : BusinessObjectCollectionView<StorageDocsBase>, IStorageDocsBaseCollection, IHaveAbstractElementType
	{
		public StorageDocsCollectionViewBase(BusinessObjectCollection collection)
			: base(collection)
		{
		}

		public StorageDocsCollectionViewBase(StorageMain master, BusinessObjectCollection collection)
			: base(collection)
		{
			this.Master = master;
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var bizO = element as StorageDocsBase;
			return bizO != null &&
				(IncludeDeletedDocuments || !bizO.SC_IsDeleted) &&
				(!ExcludeUnpublishedDocuments || bizO.SC_IsPublished) &&
				bizO.IsAvailableForCurrentEnvContext(showDocumentsForAllCompanies, showDocumentsForAllBranches, showDocumentsForAllDepartments) &&
				bizO.MatchesFilter(ExtraFilter);
		}

		public ZQuery ExtraFilter
		{
			get
			{
				return extraFilter;
			}
			set
			{
				extraFilter = value;
			}
		}
		ZQuery extraFilter = new ZQuery();

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		internal StorageDocsBase CreateNewDoc(byte[] contents, string filenameWithoutExtension, string extension)
		{
			var newFile = Factory.New<StorageFile>();
			newFile = CreateNewDoc(newFile, filenameWithoutExtension, extension) as StorageFile;
			newFile.SC_ImageData = contents;
			Add(newFile);
			return newFile;
		}

		internal StorageDocsBase CreateNewDoc(SubStreamableStream contents, string filenameWithoutExtension, string extension)
		{
			var newFile = AddNew();
			newFile = CreateNewDoc(newFile, filenameWithoutExtension, extension);
			((IeDoc)newFile).SetImageDataStream(contents);
			return newFile;
		}

		internal void MaybeCallOnSaving(StorageDocsBase newFile)
		{
			//HACK: have to do this because our time to naturally have OnSaving be called has already passed by this point
			if (((NumberedBusinessObjectFactory)newFile.Factory).MasterFactory.FactoryForEverythingExceptEDocs.IsInSaveTransaction)
			{
				newFile.OnSaving();
			}
		}
#if DEBUG
		public
#else
		internal
#endif
		StorageDocsBase FindDocByName(ZString filenameWithoutExtension, ZString extension, bool includeAllDocuments = true)
		{
			var fileName = TruncateAndTrimDot(filenameWithoutExtension, StorageDocsSchema.SC_FileName.MaxLength);
			var fileNamePattern = $@"^{Regex.Escape(fileName)}$";

			return FindDocsByPattern(fileNamePattern, extension, includeAllDocuments).FirstOrDefault();
		}

		internal List<StorageDocsBase> FindDocsByNameInAllVersions(ZString filenameWithoutExtension, ZString extension, bool includeAllDocuments = true)
		{
			var fileName = TruncateAndTrimDot(filenameWithoutExtension, StorageDocsSchema.SC_FileName.MaxLength);
			var fileNamePattern = $@"^{Regex.Escape(fileName)}(\[\d+\])?$";

			return FindDocsByPattern(fileNamePattern, extension, includeAllDocuments);
		}

		List<StorageDocsBase> FindDocsByPattern(ZString fileNamePattern, ZString extension, bool includeAllDocuments = true)
		{
			var result = new List<StorageDocsBase>();
			var dataType = TruncateAndTrimDot(extension, StorageDocsSchema.SC_DataType.MaxLength);
			var collection = includeAllDocuments ? CollectionToFilter : this;
			var fileNameRegex = new Regex(fileNamePattern, RegexOptions.IgnoreCase);

			foreach (StorageDocsBase file in collection)
			{
				if (!file.SC_FileName.IsEmpty &&
					fileNameRegex.Match(file.SC_FileName.Trim()).Success &&
					file.SC_DataType.Trim().EqualsIgnoringCase(dataType) &&
					!file.IsDeleted)
				{
					result.Add(file);
				}
			}

			return result;
		}

		/// <summary>
		/// Given a filename, return a name that is unique within the existing docs on this form
		/// by appending [2], [3], etc (e.g. 'apple[2].doc')
		/// </summary>
		internal ZString GetUniqueFilename(ZString filenameWithoutExtension, ZString extension)
		{
			var filenameGenerator = new UniqueFilenameGenerator();
			return filenameGenerator.GetNewUniqueFilenameInCollection(this, filenameWithoutExtension, extension);
		}

		internal ZString TruncateAndTrimDotRemoveExtension(ZString value, int maxLength)
		{
			return TruncateAndTrimDot(Path.GetFileNameWithoutExtension(value), maxLength);
		}

		internal ZString TruncateAndTrimDot(ZString value, int maxLength)
		{
			return value.Trim('.', ' ').SubstringSafe(0, maxLength);
		}

		public ZBool ExcludeUnpublishedDocuments
		{
			get { return fExcludeUnpublishedDocuments; }
			set
			{
				fExcludeUnpublishedDocuments = value;
				Rebuild();
			}
		}
		ZBool fExcludeUnpublishedDocuments;

		public ZBool IncludeDeletedDocuments
		{
			get { return fIncludeDeletedDocuments; }
			set
			{
				fIncludeDeletedDocuments = value;
				Rebuild();
			}
		}
		ZBool fIncludeDeletedDocuments;

		public DocumentFactory MasterFactory
		{
			get { return ((NumberedBusinessObjectFactory)CollectionToFilter.Factory).MasterFactory; }
		}

		protected readonly StorageMain Master;

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == nameof(StorageDocsBase.HumanReadableAttachmentSize))
			{
				return new PropertyComparer(typeof(StorageDocsBase), nameof(StorageDocsBase.FileSizeInBytes), direction);
			}
			return base.GetComparerForSort(property, direction);
		}

		#region Company/Branch/Department Specific

		public ZBool ShowDocumentsForAllCompanies
		{
			get { return showDocumentsForAllCompanies; }
			set
			{
				showDocumentsForAllCompanies = value;
				Rebuild();
			}
		}
		ZBool showDocumentsForAllCompanies;

		public ZBool ShowDocumentsForAllBranches
		{
			get { return showDocumentsForAllBranches; }
			set
			{
				showDocumentsForAllBranches = value;
				Rebuild();
			}
		}
		ZBool showDocumentsForAllBranches;

		public ZBool ShowDocumentsForAllDepartments
		{
			get { return showDocumentsForAllDepartments; }
			set
			{
				showDocumentsForAllDepartments = value;
				Rebuild();
			}
		}
		ZBool showDocumentsForAllDepartments;

		public ZBool ShowDocumentsForAll
		{
			set
			{
				showDocumentsForAllCompanies = value;
				showDocumentsForAllBranches = value;
				showDocumentsForAllDepartments = value;

				Rebuild();
			}
		}

			#endregion

		#region Required Documents

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = StorageDocsSchema.Constants.SC_Desc + " for error reporting")]
		string reportStorageDocInformation(StorageDocsBase storageDoc)
		{
			return (NoResString)"StorageDoc PK: " + storageDoc.PK + (NoResString)" File Name: " + storageDoc.SC_FileName + (NoResString)" Desc: " + storageDoc.SC_Desc + (NoResString)" DocType: " + storageDoc.SC_DocType + (NoResString)" ParentID: " + storageDoc.SC_ParentID;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Copying SC_Desc to EQ_DocDescription")]
#if DEBUG
		public
#else
		internal
#endif
		void AddRelatedRequiredDocuments() 
		{
			var factoryForCheck = new BusinessObjectFactory { NameForDebugging = "FactoryForCheckingDupDocType" };

			foreach (StorageDocsBase storageDoc in Elements)
			{
				if (!storageDoc.IsObsolete
					&& storageDoc.SC_DocType != Core.Constants.RefDocTypes.MiscellaneousDocument
					&& storageDoc.SC_DocType != Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument
					&& storageDoc.SC_DocType != Core.Constants.RefDocTypes.InternallyCreatedPublicDocument)
				{
					var activeDocTypeQuery = new ZQuery(RefDocTypeSchema.RT_DocType, storageDoc.SC_DocType);
					activeDocTypeQuery.AddToFilter(RefDocTypeSchema.RT_IsActive, true);

					if (!storageDoc.MasterFactory.Exists(typeof(RefDocType), activeDocTypeQuery))
					{
						continue;
					}

					if (!(Master.DocumentOwner is IHaveRequiredDocuments requiredDocumentsParent))
					{
						var docsAndCartageParent = (IDocsAndCartageParent)Master.DocumentOwner;
						requiredDocumentsParent = docsAndCartageParent.RequiredDocumentsProvider;
					}

					if (requiredDocumentsParent.RequiredDocuments.Master == null)
					{
						ErrorReporter.ReportOnce("RequiredDocumentsMasterNullInAddRelatedRequiredDocuments", "requiredDocumentsParent.RequiredDocuments.Master is null in AddRelatedRequiredDocuments. " +
							reportStorageDocInformation(storageDoc) + " Master PK: " + Master.PK + " Master.DocumentOwner Type: " + Master.DocumentOwner.GetType().ToString() + " RequiredDocumentsParent PK: " + requiredDocumentsParent.PK);
					}

					var query = new ZQuery(requiredDocumentsParent.RequiredDocuments.CompleteFilter);
					var masterType = requiredDocumentsParent.RequiredDocuments.Master?.GetType();
					var requiredDocumentType = requiredDocumentsParent.RequiredDocuments.TypeOfElements;
					SetExistingRequiredDocumentIfPossible(requiredDocumentsParent.Factory, query, storageDoc, masterType, requiredDocumentType, out var shouldAddNewDocument);

					if (shouldAddNewDocument)
					{
						var shouldCheckDocTypeDuplication = JobRequiredDocument.ShouldCheckDocTypeDuplication(storageDoc.SC_DocType);
						var canAddNewDocument = !shouldCheckDocTypeDuplication || AcquireDBLockForAddingRequiredDocument(requiredDocumentsParent, storageDoc.SC_DocType);

						//check again in a new factory in case there is new record saved to database by another factory
						SetExistingRequiredDocumentIfPossible(factoryForCheck, query, storageDoc, masterType, requiredDocumentType, out shouldAddNewDocument);

						//if canLock = false, means there's another guy is doing the same thing, we can ignore below.
						if (shouldAddNewDocument && canAddNewDocument)
						{
							//at this point we KNOW there is no new record in database, AND no one can add one while we have the lock.
							//So we can safely add our own.
							var requiredDocumentToUpdate = requiredDocumentsParent.RequiredDocuments.AddNew();
							requiredDocumentToUpdate.Origin = JobRequiredDocument.JRDOrigin.AddedEDoc;

							SetDocCategory(requiredDocumentToUpdate);
							requiredDocumentToUpdate.EQ_DocType = storageDoc.SC_DocType;

							if (requiredDocumentToUpdate.EQ_DocDescription.IsEmpty)
							{
								var description = storageDoc.SC_Desc.ToString();

								if (description.Length > AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength)
								{
									description = description.Substring(0, AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength);
								}

								requiredDocumentToUpdate.EQ_DocDescription = description;
							}

							SetDocUsage(requiredDocumentToUpdate);
							SetDocPeriod(requiredDocumentToUpdate);

							requiredDocumentToUpdate.EQ_DateReceived = storageDoc.SC_Date.UtcToDateTimeOffset();
						}
					}
				}
			}

			factoryForCheck.Save();

			void SetExistingRequiredDocumentIfPossible(BusinessObjectFactory factory, ZQuery query, StorageDocsBase storageDoc, Type masterType, Type requiredDocumentType, out bool shouldAddNewDocument)
			{
				shouldAddNewDocument = true;
				query.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, storageDoc.SC_DocType);
				query.IsNoLock = true;
				var shouldSetOriginWhenDateReceivedUpdated = SystemDataRegistry.Instance.RestrictAEDAndAIDEvents.Value;
				var requiredDocument = (JobRequiredDocument)factory.LoadTop1(requiredDocumentType, query);
				if (requiredDocument != null)
				{
					if (masterType != null)
					{
						requiredDocument.ParentType = masterType;
					}
					if (requiredDocument.EQ_DateReceived.IsEmpty)
					{
						requiredDocument.EQ_DateReceived = storageDoc.SC_Date.UtcToDateTimeOffset();
						if (shouldSetOriginWhenDateReceivedUpdated && requiredDocument.Origin == JobRequiredDocument.JRDOrigin.Unknown)
						{
							requiredDocument.Origin = JobRequiredDocument.JRDOrigin.FromRequirements;
						}
					}

					if (!shouldSetOriginWhenDateReceivedUpdated)
					{
						requiredDocument.Origin = JobRequiredDocument.JRDOrigin.FromRequirements;
					}

					shouldAddNewDocument = false;
				}
			}

			void SetDocCategory(JobRequiredDocument reqDoc)
			{
				// The original logic to set category CSR for OrgHeader and SCL for others...
				var categoryToAdd = typeof(OrgHeader).IsAssignableFrom(Master.DocumentOwner.GetType()) ? Core.Constants.ReferenceTypes.ClientSupplierRelationship : Core.Constants.ReferenceTypes.SupplyChainLogistics;
				if (reqDoc.Lookups.CategoryType_List.ContainsCode(categoryToAdd))
				{
					// we only set to this type if the reqDoc's category list has it
					reqDoc.EQ_DocCategory = categoryToAdd;
				}
				else if (reqDoc.EQ_DocCategory.IsEmpty)
				{
					// otherwise we set to the default type only when it hasn't been set
					reqDoc.EQ_DocCategory = reqDoc.Lookups.FormReferenceType;
				}
			}
		}

		void SetDocUsage(JobRequiredDocument reqDoc)
		{
			var docUsage = typeof(OrgHeader).IsAssignableFrom(Master.DocumentOwner.GetType()) ? JobRequiredDocument.DocUsage.Broker :
				(reqDoc.EQ_DocType == Core.Constants.RefDocTypes.LandedCosting ? JobRequiredDocument.DocUsage.Import : JobRequiredDocument.DocUsage.Both);

			if (!reqDoc.Lookups.DocUsage_List.ContainsCode(docUsage))
			{
				docUsage = reqDoc.Lookups.DocUsage_List.DefaultCode;
				if (string.IsNullOrEmpty(docUsage))
				{
					docUsage = reqDoc.Lookups.DocUsage_List.GetAllCodes().FirstOrDefault();
				}
			}

			reqDoc.EQ_DocUsage = docUsage;
		}

		StorageDocsBase CreateNewDoc(StorageDocsBase newFile, string filenameWithoutExtension, string extension)
		{
			var uniqueName = GetUniqueFilename(filenameWithoutExtension, extension);
			newFile.SC_FileName = TruncateAndTrimDotRemoveExtension(uniqueName, StorageDocsSchema.SC_FileName.MaxLength);
			newFile.SC_DataType = TruncateAndTrimDot(extension, StorageDocsSchema.SC_DataType.MaxLength).ToUpper();

			MaybeCallOnSaving(newFile);
			return newFile;
		}

		void SetDocPeriod(JobRequiredDocument reqDoc)
		{
			var docPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;

			if (!reqDoc.Lookups.DocPeriod_List.ContainsCode(docPeriod))
			{
				docPeriod = reqDoc.Lookups.DocPeriod_List.DefaultCode;
				if (string.IsNullOrEmpty(docPeriod))
				{
					docPeriod = reqDoc.Lookups.DocPeriod_List.GetAllCodes().FirstOrDefault();
				}
			}

			reqDoc.EQ_DocPeriod = docPeriod;
		}

		bool AcquireDBLockForAddingRequiredDocument(IHaveRequiredDocuments documentOwner, string docType)
		{
			var retryTimes = 0;
			var sqlLockKey = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", documentOwner.PK.ToString(), docType);
			var currentConnection = ((IDbConnected)documentOwner.RequiredDocuments.Factory).Connection;

			while (retryTimes < 3)
			{
				currentConnection.TryGetLock(sqlLockKey, out var lockResult);
				if (lockResult != null)
				{
					documentOwner.RequiredDocuments.Factory.AddSqlLockToTransaction(lockResult);
					return true;
				}

				Thread.Sleep(100);
				retryTimes++;
			}

			return false;
		}

#endregion

		#region IHaveAbstractElementType Members

		Type IHaveAbstractElementType.NonAbstractTypeOfElements
		{
			get
			{
				return typeof(StorageFile);
			}
		}

		#endregion

		#region IStorageDocsBaseCollection Members

		void IStorageDocsBaseCollection.Add(IeDoc elementToAdd)
		{
			Add((BusinessObject)elementToAdd);
		}

		bool IStorageDocsBaseCollection.Contains(IeDoc element)
		{
			return Contains((BusinessObject)element);
		}

		bool IStorageDocsBaseCollection.ContainsDocType(ZString docType)
		{
			return StorageDocsCollectionHelper.ContainsEDocType(docType, this);
		}

		IeDoc IStorageDocsBaseCollection.GetMostRecentEDoc(string docType)
		{
			return StorageDocsCollectionHelper.GetMostRecentEDoc(docType, this);
		}

		IeDoc IStorageDocsBaseCollection.GetFromUniqueKey(Guid uniqueKey)
		{
			return StorageDocsCollectionHelper.GetFromUniqueKey(uniqueKey, this);
		}

		void IStorageDocsBaseCollection.Remove(IeDoc elementToRemove)
		{
			Remove((BusinessObject)elementToRemove);
		}

		IeDoc IStorageDocsBaseCollection.this[int index]
		{
			get { return this[index]; }
		}

		#endregion
	}
}
