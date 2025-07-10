using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CalculateDeliveryDueDateTransportModeCollection : RegistryBusinessObjectCollection
	{
		public CalculateDeliveryDueDateTransportModeCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CalculateDeliveryDueDateTransportMode(CurrentFallbackLevel);
		}

		public CalculateDeliveryDueDateTransportModeCollection(FallbackLevel currentFallbackLevel)
			: base(currentFallbackLevel)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CalculateDeliveryDueDateTransportModeCollection(fallbackLevel);
		}

		public new CalculateDeliveryDueDateTransportMode this[int i]
		{
			get { return (CalculateDeliveryDueDateTransportMode)base[i]; }
		}

		public new CalculateDeliveryDueDateTransportMode AddNew()
		{
			return (CalculateDeliveryDueDateTransportMode)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public CalculateDeliveryDueDateOptions Parent { get; set; }

		public CalculateDeliveryDueDateTransportMode Add(ZString code, MultilingualString description, bool enable = false)
		{
			var result = AddNew();
			result.Code = code;
			result.Description = description;
			result.Enabled = enable;

			return result;
		}
	}
}
