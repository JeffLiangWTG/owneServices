using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocumentsAllowedForSigning : RegistryBusinessObjectTemplate
	{
		public DocumentsAllowedForSigningItemCollection DocumentsAllowedForSigningItems
		{
			get
			{
				if (documentsAllowedForSigningItems == null)
				{
					documentsAllowedForSigningItems = GetDefaultDocumentsAllowedForSigningItems();
					RegisterEditableChildObject(documentsAllowedForSigningItems);
				}
				return documentsAllowedForSigningItems;
			}
		}
		DocumentsAllowedForSigningItemCollection documentsAllowedForSigningItems;

		DocumentsAllowedForSigningItemCollection GetDefaultDocumentsAllowedForSigningItems()
		{
			var result = new DocumentsAllowedForSigningItemCollection();
			result.Add(new DocumentsAllowedForSigningItem(DocumentSigningRegistryConstants.DocumentAllowedForSigningName.DocBuilderInvoice, DocumentSigningRegistryConstants.DocumentAllowedForSigningType.Yes));
			result.Add(new DocumentsAllowedForSigningItem(DocumentSigningRegistryConstants.DocumentAllowedForSigningName.PeriodicInvoice, DocumentSigningRegistryConstants.DocumentAllowedForSigningType.Yes));
			result.Add(new DocumentsAllowedForSigningItem(DocumentSigningRegistryConstants.DocumentAllowedForSigningName.PeriodicInvoiceAndInvoiceDetail, DocumentSigningRegistryConstants.DocumentAllowedForSigningType.Yes));
			return result;
		}

		public bool IsDocumentsAllowedForSigning(string documentName, bool isSystemDefined)
		{
			var documentType = isSystemDefined ? DocumentSigningRegistryConstants.DocumentAllowedForSigningType.Yes : DocumentSigningRegistryConstants.DocumentAllowedForSigningType.No;
			return DocumentsAllowedForSigningItems.OfType<DocumentsAllowedForSigningItem>().Any(x => x.DocumentName == documentName && (x.DocumentType == DocumentSigningRegistryConstants.DocumentAllowedForSigningType.All || x.DocumentType == documentType));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentsAllowedForSigning();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var documentsClone = clone as DocumentsAllowedForSigning;
			if (documentsAllowedForSigningItems != null)
			{
				documentsClone.documentsAllowedForSigningItems = documentsAllowedForSigningItems.Clone(null, null) as DocumentsAllowedForSigningItemCollection;
			}
			documentsClone.RegisterDocumentsAllowedForSigningItems();
		}

		void RegisterDocumentsAllowedForSigningItems()
		{
			if (documentsAllowedForSigningItems != null)
			{
				RegisterEditableChildObject(documentsAllowedForSigningItems);
			}
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			documentsAllowedForSigningItems = (DocumentsAllowedForSigningItemCollection)ZXmlSerializer.New(typeof(DocumentsAllowedForSigningItemCollection)).Deserialize(reader);
			RegisterDocumentsAllowedForSigningItems();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			ZXmlSerializer.New(typeof(DocumentsAllowedForSigningItemCollection)).Serialize(writer, DocumentsAllowedForSigningItems);
		}
	}
}
