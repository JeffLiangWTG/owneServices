using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class DocumentImageTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DocumentImageTypeCollection()
		{
		}

		public new DocumentImageType this[int i]
		{
			get { return (DocumentImageType)Elements[i]; }
		}

		public new DocumentImageType AddNew()
		{
			return (DocumentImageType)base.AddNew();
		}

		public DocumentImageType AddNew(ZString uPSCode, ZString docTypeCode, ZString description, bool moveJobToClassOnImport, bool notifyOnImport)
		{
			DocumentImageType result = (DocumentImageType)base.AddNew();
			result.UPSCode = uPSCode;
			result.DocTypeCode = docTypeCode;
			result.Description = description;
			result.MoveJobToClassOnImport = moveJobToClassOnImport;
			result.NotifyOnImport = notifyOnImport;
			return result;
		}

		public DocumentImageType FindByUPSCode(ZString uPSCode)
		{
			foreach (DocumentImageType imageType in this)
			{
				if (imageType.UPSCode == uPSCode)
				{
					return imageType;
				}
			}
			return (string.IsNullOrEmpty(uPSCode)) ? null : FindByUPSCode(string.Empty);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocumentImageType();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentImageTypeCollection();
		}
	}
}
