using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	[XmlRoot("EmailSignatureFields")]
	public class EmailSignatureFieldCollection : EmailFieldCollection
	{
		public EmailSignatureFieldCollection()
		{
		}

		public new EmailSignatureField this[int i]
		{
			get { return (EmailSignatureField)Elements[i]; }
		}

		public new EmailSignatureField AddNew()
		{
			return (EmailSignatureField)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EmailSignatureFieldCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EmailSignatureField();
		}
	}
}
