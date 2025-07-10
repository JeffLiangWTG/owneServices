using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class UPEBranchIDsRegistryObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public UPEBranchIDsRegistryObjectCollection()
		{
		}

		public new UPEBranchIDsRegistryObject AddNew()
		{
			return (UPEBranchIDsRegistryObject)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UPEBranchIDsRegistryObjectCollection();
		}

		public new UPEBranchIDsRegistryObject this[int i]
		{
			get { return (UPEBranchIDsRegistryObject)base[i]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UPEBranchIDsRegistryObject();
		}

		public UPEBranchIDsRegistryObject FindByPort(ZGuid portPK)
		{
			foreach (UPEBranchIDsRegistryObject obj in this)
			{
				if (obj.FirstArrivalPort == portPK)
				{
					return obj;
				}
			}

			return null;
		}
	}
}
