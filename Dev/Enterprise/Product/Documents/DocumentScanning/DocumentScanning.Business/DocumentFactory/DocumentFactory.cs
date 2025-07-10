using System;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentScanning.Business
{
	public abstract class DocumentFactory : NumberedBusinessObjectFactory, IDocumentFactory, IStorageMainForPK, ILogSource
	{
		protected DocumentFactory(BusinessObjectFactory factoryForEverythingExceptEDocs)
		{
			FactoryForEverythingExceptEDocs = factoryForEverythingExceptEDocs;
			if (FactoryForEverythingExceptEDocs == null)
			{
				FactoryForEverythingExceptEDocs = new BusinessObjectFactory();
				ErrorReporter.ReportOnce("Null FactoryForEverythingExceptEDocs passed in to DocumentFactory"); // Replace with Argument.NotNull after 6 months (Oct 2019)
			}

			ChildFactories.Add(FactoryForEverythingExceptEDocs);
		}

		protected DocumentFactory(DbConnection connection, DocumentFactory masterFactory, BusinessObjectFactory factoryForEverythingExceptEDocs = null, bool canBeSavedWithoutMasterFactory = false)
			: base(connection, masterFactory, canBeSavedWithoutMasterFactory)
		{
			FactoryForEverythingExceptEDocs = factoryForEverythingExceptEDocs ?? masterFactory.FactoryForEverythingExceptEDocs;
			if (FactoryForEverythingExceptEDocs == null)
			{
				FactoryForEverythingExceptEDocs = new BusinessObjectFactory();
				ErrorReporter.ReportOnce("Null FactoryForEverythingExceptEDocs passed in to DocumentFactory"); // Replace with Argument.NotNull after 6 months (Oct 2019)
			}

			ChildFactories.Add(FactoryForEverythingExceptEDocs);
		}

		#region ParentFactory

		public BusinessObjectFactory FactoryForEverythingExceptEDocs { get; }

		#endregion

		#region Convert from Allocated / Unallocated

		public abstract StorageDocs ConvertFromUnallocated(StorageDocsUnallocated document);

		public abstract StorageDocsUnallocated ConvertFromAllocated(StorageDocsBase document);

		public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
		{
			return new DocumentFactoryProvider().GetFactory(FactoryForEverythingExceptEDocs);
		}

		#endregion

		#region Save/Load

		public override BusinessObject[] Load(Type bizOType, ZQuery sQLFilter)
		{
			if (!SupportsZQuery())
			{
				ErrorReporter.ReportOnce(String.Format("Factory of type {0} doesn't support loading using ZQuery", this.GetType().FullName));
			}
			return base.Load(bizOType, sQLFilter);
		}

		public void SaveFactoriesExceptFactoryForEverythingExceptEDocs()
		{
			using (TemporarilyRemoveFactoryForEverythingExceptEDocs())
			{
				Save();
			}
		}

		IDisposable TemporarilyRemoveFactoryForEverythingExceptEDocs()
		{
			ChildFactories.Remove(FactoryForEverythingExceptEDocs);
			return new DisposableAction(() => { ChildFactories.Add(FactoryForEverythingExceptEDocs); });
		}

		#endregion

		#region Abstract members

		public abstract StorageMain GetStorageMainForPK(ZGuid bizOPK);

		public abstract DbWriteableState GetDbWriteableState(int dbNumber);

		public abstract int GetCountOfPublishedDocumentsAndFilesForStorageMain(StorageMain parent);

		public abstract bool Import(byte[] contents, ZString filenameOnly, ZString userSuppliedRefType, ZGuid userSuppliedRefPK, ZString userSuppliedDocType, ZString userSuppliedDocSource, ZGuid visibleCompanyPK, ZGuid visibleBranchPK, ZGuid visibleDepartmentPK, string fileFullPath = "", bool checkSecurity = false);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public abstract bool Import(byte[] contents, ZString filenameOnly, ZString userSuppliedRefType, ZGuid userSuppliedRefPK, ZString userSuppliedDocType, ZString userSuppliedDocSource, ZGuid visibleCompanyPK, ZGuid visibleBranchPK, ZGuid visibleDepartmentPK, string fileFullPath, bool checkSecurity, out string finalDocType);

		public abstract StorageDocsBase Allocate(StorageDocsBase document, ZGuid itemFK);

		public abstract StorageDocsUnallocated Unallocate(StorageDocsBase document);

		public abstract int LastWriteableDatabaseWithFreeSpace();

		public abstract void AllocateToDB(StorageMain parentRecord);

		protected abstract StorageMain CreateStorageMain(ZGuid parentBusinessObjectPK, BusinessObject parentBusinessObject, string refType);

		public abstract int GetFactoryNumberInUse(IBusinessObjectCollection bizOCollection);

		public abstract NumberedBusinessObjectFactory GetFactory(ZInt databaseNumber);

		public abstract StorageMain RetrieveExistingOrCreateStorageMainForPK(ZGuid bizOPK, string docManagerCode);

		public abstract StorageMain RetrieveExistingOrCreateStorageMainForPK(ZGuid parentPK, BusinessObject parent, string refType);

		public abstract StorageMain RetrieveExistingOrCreateStorageMain(BusinessObject bizO, string docManagerCode);

		protected abstract bool SupportsZQuery();

		#endregion

		#region Event handlers

		public event EventHandler SaveSuccessful;

		protected virtual void OnSaveSuccessful()
		{
			if (SaveSuccessful != null)
			{
				SaveSuccessful(this, EventArgs.Empty);
			}
		}

#if DEBUG
		public
#else
		protected internal
#endif
		virtual void OnLog(TraceEventType eventType, string message)
		{
			Log?.Invoke(new LogEventArgs(eventType, message));
		}

#endregion

		#region Implementation

		public abstract BusinessObject CreateAndAllocateDocument(ZGuid itemFK, ZString relatedBusinessContext, ZString pathToFile, ZString documentType, ZString documentDescription, bool forceAllocate = false, string fileName = "", string document = "", string dataType = "", string printJobPKAsString = "");

		public abstract BusinessObject AddFileOrDocument(ZGuid parentBizOPK, string docManagerCode, SubStreamableStream contents, string filenameOnly, string documentType, string documentSource, bool overwriteExistingFileIfNotAnImage);

		public abstract BusinessObject AddFileOrDocument(ZGuid parentBizOPK, string docManagerCode, byte[] contents, string filenameOnly, string documentType, string documentSource, bool overwriteExistingFileIfNotAnImage);

		public abstract void UpdatePublishedFlagForAllEDocs(ZString referenceType, ZString documentType, ZBool isPublished);

		public abstract void UpdateDocTypeForAllEDocs(ZPropertyInfo referenceTypeInfo, ZPropertyInfo docTypeInfo, ZPropertyInfo descInfo);

		public abstract void UpdateDocTypeForJobRequiredDocument(ZPropertyInfo referenceTypeInfo, ZPropertyInfo docTypeInfo, ZPropertyInfo descInfo);

		public abstract string[] NamesOfMissingDbs { get; }

		public abstract string GetDatabaseName(int databaseNumber);

		BusinessObjectFactory IDocumentFactory.GetFactory(ZInt databaseNumber)
		{
			return GetFactory(databaseNumber);
		}

		IStorageMain IDocumentFactory.GetStorageMainForPK(ZGuid bizOPK)
		{
			return GetStorageMainForPK(bizOPK);
		}

		IStorageMain IDocumentFactory.RetrieveExistingOrCreateStorageMainForPK(ZGuid bizOPK, string docManagerCode)
		{
			return RetrieveExistingOrCreateStorageMainForPK(bizOPK, docManagerCode);
		}

		IStorageMain IDocumentFactory.RetrieveExistingOrCreateStorageMain(BusinessObject bizO, string docManagerCode)
		{
			return RetrieveExistingOrCreateStorageMain(bizO, docManagerCode);
		}

#if DEBUG
		public abstract BusinessObject CreateDocument(ZGuid storageMainPK);
#endif

		public event Action<LogEventArgs> Log;

		public abstract IDocumentsView GetStorageMain(ZGuid bizOPK);

		public abstract IeDoc FindEDocsFromAllSDDatabasesByPK(ZGuid uniqueKey);

		#endregion
	}
}

