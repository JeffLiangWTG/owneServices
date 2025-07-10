using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class AWSPrivateCACollection : RegistryBusinessObjectCollectionTemplate
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AWSPrivateCA();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AWSPrivateCACollection();
		}

		public new AWSPrivateCA this[int i]
		{
			get { return (AWSPrivateCA)Elements[i]; }
		}

		public new AWSPrivateCA AddNew()
		{
			return (AWSPrivateCA)base.AddNew();
		}
	}
}
