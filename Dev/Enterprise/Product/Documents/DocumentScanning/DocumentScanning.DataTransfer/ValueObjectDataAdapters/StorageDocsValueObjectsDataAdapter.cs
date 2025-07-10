using System;
using System.Drawing;
using System.IO;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DocumentScanning.DataTransfer
{
	#region base class

	public abstract class StorageDocsBaseValueObjectDataAdapter<T> : ValueObjectDataAdapter<T, Xsd.Document> where T : StorageDocsBase
	{
		protected StorageDocsBaseValueObjectDataAdapter()
			: base()
		{
		}

		protected StorageDocsBaseValueObjectDataAdapter(BusinessObject parentBizObj)
			: base()
		{
			this.ParentBizObj = parentBizObj;
		}

		#region Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description XML element name")]
		public override string RootCollectionElementName
		{
			get { return "Documents"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description XML element name")]
		public override string RootElementName
		{
			get { return "Document"; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleDocumentSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return XmlSchemaDefinitions.Instance.DocumentsSchema; }
		}

		#endregion

		#region Import

		protected override T FindBusinessObject(Xsd.Document value, IValueObjectImportContext context)
		{
			ThrowExceptionIfParentObjectIsNotValid();
			ThrowExceptionIfParentIsNotIDocManagerSupport();

			T storageDoc = null;
			if (ValidToImport(value, context))
			{
				StorageMain storageMain = DocumentFactory.RetrieveExistingOrCreateStorageMainForPK(ParentBizObj.PK, ((IDocManagerSupport)ParentBizObj).DocManagerInfo.DocManagerCode);
				if (storageMain != null)
				{
					ZQuery filter = ZQuery.NoResultQuery;
					var document = value;
					if (!document.Date.IsEmpty)
					{
						filter = new ZQuery(StorageDocsSchema.SC_DocType, document.DocumentType);
						filter.AddToFilter(StorageDocsSchema.SC_Date, document.Date);
						filter.AddToFilter(StorageDocsSchema.SC_Desc, document.Description);
						filter.AddToFilter(StorageDocsSchema.SC_FileName, document.FileName);
						filter.MaximumRows = 1;
					}

					T[] storageDocs = (T[])GetProperDocsCollection(storageMain).Find(filter);
					storageDoc = storageDocs.Length > 0 ? storageDocs[0] : GetProperDocsCollection(storageMain).AddNew() as T;
				}
			}

			return storageDoc;
		}

		protected abstract StorageDocsCollectionViewBase GetProperDocsCollection(StorageMain main);

		protected override T NewBusinessObject(Xsd.Document value, IValueObjectImportContext context)
		{
			StorageMain storageMain = DocumentFactory.RetrieveExistingOrCreateStorageMainForPK(ParentBizObj.PK, ((IDocManagerSupport)ParentBizObj).DocManagerInfo.DocManagerCode);
			return GetProperDocsCollection(storageMain).AddNew() as T;
		}

		protected override void ImportFromValueObjectCore(T bizObj, Xsd.Document document, IValueObjectImportContext context)
		{
			if (document.IsSpecified)
			{
				var storageDoc = bizObj;

				context.SetPropertyInfoValue(storageDoc.SC_DataTypeInfo, document.DataType, document.DataTypeSpecified);
				context.SetPropertyInfoValueIfValueNotEmpty(storageDoc.SC_DocTypeInfo, !document.DocumentType.IsEmpty ? document.DocumentType : (ZString)Core.Constants.RefDocTypes.MiscellaneousDocument);
				context.SetPropertyInfoValue(storageDoc.SC_DescInfo, document.Description, document.DescriptionSpecified);
				context.SetPropertyInfoValue(storageDoc.SC_FileNameInfo, document.FileName, document.FileNameSpecified);

				if (!document.Date.IsEmpty)
				{
					context.SetPropertyInfoValue(storageDoc.SC_DateInfo, document.Date.ToDateTime());
				}

				if (document.IsSystemGeneratedSpecified)
				{
					storageDoc.SC_IsSystemGenerated = document.IsSystemGenerated == Xsd.TrueFalse.@true;
				}

				if (document.IsPublishedSpecified)
				{
					storageDoc.SC_IsPublished = document.IsPublished == Xsd.TrueFalse.@true;
				}

				if (document.SaveVersionsSpecified)
				{
					storageDoc.SC_SaveVersions = document.SaveVersions == Xsd.TrueFalse.@true;
				}

				storageDoc.SC_ImageData = document.Data;
				DataTransferTransactionCoordinator.SaveFactory(DocumentFactory);
			}
		}

		protected override bool AllowDifferentImportContextFactory
		{
			get { return true; }
		}

		#endregion

		#region Export

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		protected override void ExportToValueObjectCore(T storageDoc, Xsd.Document document, IValueObjectExportContext context)
		{
			document.DataType = ExportFormatType.ToString();
			document.DocumentType = storageDoc.SC_DocType;
			document.Description = storageDoc.SC_Desc;
			document.FileName = storageDoc.SC_FileName;
			document.Date = storageDoc.SC_Date;
			document.IsSystemGenerated = storageDoc.SC_IsSystemGenerated ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			document.IsSystemGeneratedSpecified = true;
			document.IsPublished = storageDoc.SC_IsPublished ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			document.IsPublishedSpecified = true;
			document.SaveVersions = storageDoc.SC_SaveVersions ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			document.SaveVersionsSpecified = true;

			if (!storageDoc.SC_ImageData.IsEmpty)
			{
				document.Data = GetData(storageDoc.SC_ImageData);
			}
		}

		#endregion

		#region Implementation

		bool ValidToImport(Xsd.Document document, INotifications notify)
		{
			bool result = true;
			if (document.Data == null || document.Data.Length == 0)
			{
				notify.Notify(new WarningNotification(WarningType.Warning, Res.GetString("1896c7e9-72a8-4840-8986-30a1742de1a4", "No Document Data To Import")));
				result = false;
			}
			else
			{
				result = IsDataValid(document.Data, notify);
			}

			document.IsSpecified = result;

			return result;
		}

		protected abstract bool IsDataValid(byte[] imageData, INotifications notify);

		protected abstract byte[] GetData(byte[] data);

		void ThrowExceptionIfParentObjectIsNotValid()
		{
			if (ParentBizObj == null)
			{
				throw new NotSupportedException("Creating or updating StorageDocs cannot be supported without a parent Business Object, Pass one through the Constructor");
			}
		}

		void ThrowExceptionIfParentIsNotIDocManagerSupport()
		{
			if (!(ParentBizObj is IDocManagerSupport))
			{
				throw new NotSupportedException("The parent Business Object must implement the IDocManagerSupport interface to be able to import documents.");
			}
		}

		#endregion

		#region DocumentFactory

		DocumentFactory DocumentFactory
		{
			get
			{
				if (fDocumentFactory == null)
				{
					fDocumentFactory = new DocumentFactoryProvider().GetFactory(ParentBizObj == null ? null : ParentBizObj.Factory);
				}
				return fDocumentFactory;
			}
		}
		DocumentFactory fDocumentFactory;

		#endregion

		protected readonly BusinessObject ParentBizObj;

		protected abstract OutputFormatType ExportFormatType { get; }
	}

	#endregion

	#region StorageDocsValueObjectDataAdapter

	public class StorageDocsValueObjectDataAdapter : StorageDocsBaseValueObjectDataAdapter<StorageDocs>, IStorageDocsValueObjectDataAdapter
	{
		public StorageDocsValueObjectDataAdapter()
			: base()
		{
		}

		public StorageDocsValueObjectDataAdapter(BusinessObject parentBizObj)
			: base(parentBizObj)
		{
		}

		protected override bool IsDataValid(byte[] imageData, INotifications notify)
		{
			bool result = true;
			using (TempFile tempFile = TempFile.NewWithExtension("TIF"))
			{
				using (FileStream stream = new FileStream(tempFile.Filename, FileMode.Open))
				{
					stream.Write(imageData, 0, imageData.Length);
				}

				try
				{
					using (Image image = Image.FromFile(tempFile.Filename))
					{
					}
				}
				catch (Exception exception) when (!exception.IsCriticalException() || exception is OutOfMemoryException) // GDI+ load error.
				{
					notify.Notify(new WarningNotification(WarningType.Warning, Res.GetString("ee86a03e-f90b-4a36-a865-de66e7f422b4", "Could not import document, File is not an image")));
					result = false;
				}
			}
			return result;
		}

		protected override OutputFormatType ExportFormatType
		{
			get { return OutputFormatType.TIF; }
		}

		protected override StorageDocsCollectionViewBase GetProperDocsCollection(StorageMain main)
		{
			return main.Documents;
		}

		protected override byte[] GetData(byte[] data)
		{
			byte[] result = null;
			switch (ExportFormatType)
			{
				case OutputFormatType.PDF:
					result = DocumentConverter.ConvertTIFToPDF(data);
					break;

				default:
					result = DocumentConverter.CompressTIFImage(data);
					break;
			}
			return result;
		}
	}

	#endregion

	#region StorageFilesValueObjectDataAdapter

	public class StorageFilesValueObjectDataAdapter : StorageDocsBaseValueObjectDataAdapter<StorageFile>, IStorageFilesValueObjectDataAdapter
	{
		public StorageFilesValueObjectDataAdapter()
			: base()
		{
		}

		public StorageFilesValueObjectDataAdapter(BusinessObject parentBizObj)
			: base(parentBizObj)
		{
		}

		protected override StorageDocsCollectionViewBase GetProperDocsCollection(StorageMain main)
		{
			return main.Files;
		}

		protected override bool IsDataValid(byte[] imageData, INotifications notify)
		{
			return true;
		}

		protected override OutputFormatType ExportFormatType
		{
			get { return OutputFormatType.PDF; }
		}

		protected override byte[] GetData(byte[] data)
		{
			return data;
		}
	}

	#endregion
}
