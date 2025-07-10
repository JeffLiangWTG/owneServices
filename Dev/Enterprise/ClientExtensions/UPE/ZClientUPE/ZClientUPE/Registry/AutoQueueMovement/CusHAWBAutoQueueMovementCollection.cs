using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class CusHAWBAutoQueueMovementCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CusHAWBAutoQueueMovementCollection()
		{
		}

		public new CusHAWBAutoQueueMovement this[int i]
		{
			get { return (CusHAWBAutoQueueMovement)Elements[i]; }
		}

		public new CusHAWBAutoQueueMovement AddNew()
		{
			return (CusHAWBAutoQueueMovement)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CusHAWBAutoQueueMovement();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CusHAWBAutoQueueMovementCollection();
		}
	}
}
