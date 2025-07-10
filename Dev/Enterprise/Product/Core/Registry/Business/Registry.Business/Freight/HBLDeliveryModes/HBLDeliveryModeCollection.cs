using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HBLDeliveryModeCollection : RegistryBusinessObjectCollection
	{
		public HBLDeliveryModeCollection()
		{
		}

		public HBLDeliveryModeCollection(ZString shipmentPackingMode)
			: this(null, shipmentPackingMode)
		{
		}

		public HBLDeliveryModeCollection(FallbackLevel currentFallbackLevel)
			: this(currentFallbackLevel, "")
		{
		}

		public HBLDeliveryModeCollection(FallbackLevel currentFallbackLevel, ZString shipmentPackingMode)
			: base(currentFallbackLevel)
		{
			this.ShipmentPackingMode = shipmentPackingMode;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HBLDeliveryMode(CurrentFallbackLevel);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HBLDeliveryModeCollection(fallbackLevel, ShipmentPackingMode);
		}

		public new HBLDeliveryMode this[int i]
		{
			get { return (HBLDeliveryMode)base[i]; }
		}

		public new HBLDeliveryMode AddNew()
		{
			return (HBLDeliveryMode)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public HBLDeliveryModes Parent { get; set; }

		public ZString ShipmentPackingMode { get; set; }

		public HBLDeliveryMode Add(ZString code, MultilingualString description, bool showInList = true)
		{
			var result = AddNew();
			result.Code = code;
			result.Description = description;
			result.ShowInList = showInList;

			return result;
		}
	}
}
