using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DeliveryOrderCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DeliveryOrderCollection()
		{
		}

		public DeliveryOrderCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new DeliveryOrder this[int index]
		{
			get { return (DeliveryOrder)Elements[index]; }
		}

		public new DeliveryOrder AddNew()
		{
			return (DeliveryOrder)base.AddNew();
		}

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DeliveryOrder(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DeliveryOrderCollection(fallbackLevel, factory);
		}

		#endregion
	}
}
