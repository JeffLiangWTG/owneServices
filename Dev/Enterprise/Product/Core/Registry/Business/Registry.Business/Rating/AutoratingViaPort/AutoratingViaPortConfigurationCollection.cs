using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.AutoratingViaPortHelper;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot(ElementName = "Configurations")]
	public class AutoratingViaPortConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public AutoratingViaPortConfigurationCollection()
			: this(null, null)
		{
		}

		public AutoratingViaPortConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new AutoratingViaPortConfiguration(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new AutoratingViaPortConfigurationCollection(fallbackLevel, factory);

		public new AutoratingViaPortConfiguration this[int i]
			=> (AutoratingViaPortConfiguration)Elements[i];

		public new AutoratingViaPortConfiguration AddNew()
			=> (AutoratingViaPortConfiguration)base.AddNew();

		public static AutoratingViaPortConfigurationCollection Default
		{
			get
			{
				var defaultItems = new AutoratingViaPortConfigurationCollection(null, null);

				foreach (var settings in Defaults.GroupBy(x => (x.jobType, x.transportMode)))
				{
					var configuration = defaultItems.AddNew();
					configuration.JobType = settings.Key.jobType;
					configuration.TransportMode = settings.Key.transportMode;

					foreach (var item in settings)
					{
						var setting = configuration.Settings.AddNew();
						setting.Direction = item.direction;
						setting.OriginSourceOption = item.origin;
						setting.DestinationSourceOption = item.destination;
						setting.ViaSourceOption = item.via;
					}
				}

				return defaultItems;
			}
		}

		public static IList<(string jobType, string transportMode, string direction, string origin, string destination, string via)> Defaults
			=> new List<(string, string, string, string, string, string)>
			{
				// Quoted Booking
				(
					JobType.QuotedBooking.Code,
					TransportModes.All,
					DirectionOption.All.Code,
					LocationSourceOption.VoyageLoad.Code,
					LocationSourceOption.NotVoyageDischarge.Code,
					LocationSourceOption.VoyageDischarge.Code
				),
				(
					JobType.QuotedBooking.Code,
					TransportModes.All,
					DirectionOption.All.Code,
					LocationSourceOption.NotVoyageLoad.Code,
					LocationSourceOption.VoyageDischarge.Code,
					LocationSourceOption.VoyageLoad.Code
				),
				(
					JobType.QuotedBooking.Code,
					TransportModes.All,
					DirectionOption.Export.Code,
					LocationSourceOption.NotVoyageLoad.Code,
					LocationSourceOption.NotVoyageDischarge.Code,
					LocationSourceOption.VoyageLoad.Code
				),
				(
					JobType.QuotedBooking.Code,
					TransportModes.All,
					DirectionOption.Import.Code,
					LocationSourceOption.NotVoyageLoad.Code,
					LocationSourceOption.NotVoyageDischarge.Code,
					LocationSourceOption.VoyageDischarge.Code
				),

				// Shipment
				(
					JobType.Shipment.Code,
					TransportModes.All,
					DirectionOption.All.Code,
					LocationSourceOption.FirstLoad.Code,
					LocationSourceOption.NotLastDischarge.Code,
					LocationSourceOption.LastDischarge.Code
				),
				(
					JobType.Shipment.Code,
					TransportModes.All,
					DirectionOption.All.Code,
					LocationSourceOption.NotFirstLoad.Code,
					LocationSourceOption.LastDischarge.Code,
					LocationSourceOption.FirstLoad.Code
				),
				(
					JobType.Shipment.Code,
					TransportModes.All,
					DirectionOption.Export.Code,
					LocationSourceOption.NotFirstLoad.Code,
					LocationSourceOption.NotLastDischarge.Code,
					LocationSourceOption.FirstLoad.Code
				),
				(
					JobType.Shipment.Code,
					TransportModes.All,
					DirectionOption.Import.Code,
					LocationSourceOption.NotFirstLoad.Code,
					LocationSourceOption.NotLastDischarge.Code,
					LocationSourceOption.LastDischarge.Code
				),

				// Forwarding Consol
				(
					JobType.ForwardingConsol.Code,
					TransportModes.All,
					DirectionOption.All.Code,
					LocationSourceOption.VoyageLoad.Code,
					LocationSourceOption.NotVoyageDischarge.Code,
					LocationSourceOption.VoyageDischarge.Code
				),
				(
					JobType.ForwardingConsol.Code,
					TransportModes.All,
					DirectionOption.All.Code,
					LocationSourceOption.NotVoyageLoad.Code,
					LocationSourceOption.VoyageDischarge.Code,
					LocationSourceOption.VoyageLoad.Code
				),
				(
					JobType.ForwardingConsol.Code,
					TransportModes.All,
					DirectionOption.Export.Code,
					LocationSourceOption.NotVoyageLoad.Code,
					LocationSourceOption.NotVoyageDischarge.Code,
					LocationSourceOption.VoyageLoad.Code
				),
				(
					JobType.ForwardingConsol.Code,
					TransportModes.All,
					DirectionOption.Import.Code,
					LocationSourceOption.NotVoyageLoad.Code,
					LocationSourceOption.NotVoyageDischarge.Code,
					LocationSourceOption.VoyageDischarge.Code
				),
			};
	}
}
