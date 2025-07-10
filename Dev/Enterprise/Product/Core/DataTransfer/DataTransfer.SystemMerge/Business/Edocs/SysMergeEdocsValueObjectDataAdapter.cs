using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	public class SysMergeEdocsValueObjectDataAdapter : ValueObjectDataAdapter<StorageDocsForDataTransfer, Xsd.SystemMergeStorageDoc>
	{
		public override string RootCollectionElementName
		{
			get { return "SystemMergeStorageDocs"; }
		}

		public override string RootElementName
		{
			get { return "SystemMergeStorageDoc"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		#region ExportFromValueObject

		protected override void ExportToValueObjectCore(StorageDocsForDataTransfer bizObj, Xsd.SystemMergeStorageDoc constructedValueObject, IValueObjectExportContext context)
		{
			if (bizObj.SC_IsDeleted || bizObj.SC_ImageData.IsEmpty)
			{
				constructedValueObject.IsSpecified = false;
			}
			else
			{
				if (bizObj.ParentOrgPk.IsEmpty)
				{
					throw new InvalidOperationException("Document must have a parent Organisation PK for this to data export");
				}

				ExportStorageDocs(bizObj, constructedValueObject);
			}
		}

		void ExportStorageDocs(StorageDocsForDataTransfer doc, Xsd.SystemMergeStorageDoc xsdDoc)
		{
			xsdDoc.OrgHeaderPk = doc.ParentOrgPk.ToString();
			xsdDoc.PK = doc.PK.ToString();
			xsdDoc.ImageData = doc.SC_ImageData;

			xsdDoc.DataType = doc.SC_DataType;
			xsdDoc.Desc = doc.SC_Desc;
			xsdDoc.DocType = doc.SC_DocType;
			xsdDoc.FileName = doc.SC_FileName;
			xsdDoc.IsPublished = doc.SC_IsPublished;
			xsdDoc.IsSystemGenerated = doc.SC_IsSystemGenerated;
			xsdDoc.SaveVersions = doc.SC_SaveVersions;

			if (!doc.SC_Date.IsEmpty)
			{
				xsdDoc.Date = doc.SC_Date.ToDateTime();
				xsdDoc.DateSpecified = true;
			}

			// Set Specified flag for boolean/numeric fields so they get serialised to XML
			// Boolean
			xsdDoc.IsPublishedSpecified = true;
			xsdDoc.IsSystemGeneratedSpecified = true;
			xsdDoc.SaveVersionsSpecified = true;
		}

		#endregion

		#region ImportFromValueObject

		protected override bool AllowDifferentImportContextFactory
		{
			get { return true; }
		}

		protected override StorageDocsForDataTransfer FindBusinessObject(Xsd.SystemMergeStorageDoc value, IValueObjectImportContext context)
		{
			return FindStorageDocByPk(value, context);
		}

		StorageDocsForDataTransfer FindStorageDocByPk(Xsd.SystemMergeStorageDoc xsdDoc, IValueObjectImportContext context)
		{
			StorageDocsForDataTransfer result = null;

			if (!xsdDoc.OrgHeaderPk.IsEmpty && !xsdDoc.PK.IsEmpty)
			{
				ZGuid orgPk = new ZGuid(xsdDoc.OrgHeaderPk);
				OrgHeaderForDataTransfer org = context.Factory.Load<OrgHeaderForDataTransfer>(orgPk);

				if (org != null)
				{
					DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(context.Factory);
					StorageMain storageMain = docFactory.GetStorageMainForPK(org.PK);

					if (storageMain != null)
					{
						ZGuid pkTofind = new ZGuid(xsdDoc.PK);
						result = docFactory.GetFactory(storageMain.SM_DB).Load<StorageDocsForDataTransfer>(pkTofind);
					}
				}
			}

			return result;
		}

		protected override StorageDocsForDataTransfer NewBusinessObject(Xsd.SystemMergeStorageDoc value, IValueObjectImportContext context)
		{
			StorageDocsForDataTransfer result = null;

			ZGuid orgPk = new ZGuid(value.OrgHeaderPk);
			OrgHeaderForDataTransfer org = context.Factory.Load<OrgHeaderForDataTransfer>(orgPk);

			if (org == null)
			{
				string message = Res.GetString("c944784c-eff3-49cc-a98c-be91c8a93d8f", "[Document: {0} - {1}]\r\nCould not find Organization with PK = [{2}].\r\nPlease this document parent Organization then retry the import operation.",
					value.Desc,
					value.FileName,
					orgPk.ToString()) + "\r\n";

				throw new Exception(message);
			}
			else
			{
				DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(context.Factory);
				StorageMain storageMain = docFactory.RetrieveExistingOrCreateStorageMainForPK(org.PK, Enterprise.Core.Constants.DocManagerCodes.Organisation);
				result = docFactory.GetFactory(storageMain.SM_DB).NewWithPrimaryKey<StorageDocsForDataTransfer>(new Guid(value.PK));
				result.SC_SM = storageMain.PK;
			}

			return result;
		}

		protected override bool ConfirmUpdateOfExistingBusinessObject(StorageDocsForDataTransfer obj, INotifications notifications)
		{
			return false;
		}

		protected override void OnUserDeclinedImport(StorageDocsForDataTransfer bizObj, Xsd.SystemMergeStorageDoc value, IValueObjectImportContext context)
		{
			string message = Res.GetString("11a742dc-2b4e-4ca5-8808-88bf76b515fb", "Import of document [({0}) - {1} - {2}] skipped. Reason: Document already exists.", bizObj.PK.ToString(), bizObj.SC_DataType, bizObj.SC_FileName) + "\r\n";
			context.Notify(new InfoNotification(message));
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			// Don't notify here. It will be notified if save succeeds.
		}

		protected override void ImportFromValueObjectCore(StorageDocsForDataTransfer bizObj, Xsd.SystemMergeStorageDoc value, IValueObjectImportContext context)
		{
			ImportStorageDocs(bizObj, value);
		}

		void ImportStorageDocs(StorageDocsForDataTransfer doc, Xsd.SystemMergeStorageDoc xsdDoc)
		{
			doc.SC_ImageData = xsdDoc.ImageData;

			doc.SC_DataType = xsdDoc.DataType;
			doc.SC_Desc = xsdDoc.Desc;
			doc.SC_DocType = xsdDoc.DocType;
			doc.SC_FileName = xsdDoc.FileName;
			doc.SC_IsPublished = xsdDoc.IsPublished;
			doc.SC_IsSystemGenerated = xsdDoc.IsSystemGenerated;
			doc.SC_SaveVersions = xsdDoc.SaveVersions;
			doc.SC_Date = xsdDoc.Date;
		}

		#endregion
	}
}
