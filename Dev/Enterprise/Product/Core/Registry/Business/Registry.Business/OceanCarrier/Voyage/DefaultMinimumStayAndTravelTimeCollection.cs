using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DefaultMinimumStayAndTravelTimeCollection : RegistryBusinessObjectCollectionTemplate<DefaultMinimumStayAndTravelTime>
	{
		public DefaultMinimumStayAndTravelTimeCollection() : base(null, null)
		{
		}

		public DefaultMinimumStayAndTravelTimeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowSort => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefaultMinimumStayAndTravelTime();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultMinimumStayAndTravelTimeCollection();
		}

		public static DefaultMinimumStayAndTravelTimeCollection DefaultValue =>
			new()
			{
				new DefaultMinimumStayAndTravelTime
				{
					TransportMode = Core.Constants.TransportModes.Sea,
					StayTime = 120,
					TravelTime = 60
				},
				new DefaultMinimumStayAndTravelTime
				{
					TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport,
					StayTime = 60,
					TravelTime = 30
				},
				new DefaultMinimumStayAndTravelTime
				{
					TransportMode = Core.Constants.TransportModes.Air,
					StayTime = 60,
					TravelTime = 30
				},
				new DefaultMinimumStayAndTravelTime
				{
					TransportMode = Core.Constants.TransportModes.Rail,
					StayTime = 30,
					TravelTime = 15
				},
			};
	}
}
