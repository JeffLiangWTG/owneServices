using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class UPEGlbGroupsRegistryObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UPEGlbGroupsRegistryObjectCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UPEGlbGroupsRegistryObject();
		}

		public new UPEGlbGroupsRegistryObject AddNew()
		{
			return (UPEGlbGroupsRegistryObject)base.AddNew();
		}

		public new UPEGlbGroupsRegistryObject this[int i]
		{
			get { return (UPEGlbGroupsRegistryObject)base[i]; }
		}
	}
}
