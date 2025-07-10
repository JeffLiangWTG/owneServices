using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.AutoratingViaPortHelper;

namespace Enterprise.Registry.Business
{
	public class AutoratingViaPortRegistryItem : StronglyTypedRegistryItem<AutoratingViaPortConfigurationCollection>
	{
		public AutoratingViaPortRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AutoratingViaPortConfigurationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AutoratingViaPortRegistryDataType(), storage, defaultValue))
		{
		}

		public string GetViaForForwardingConsol(ZString transportMode, ZString direction, string origin, string destination, string voyageLoad, string voyageDischarge, string lastModeRouteSetDischarge) =>
			GetVia
			(
				jobType: JobType.ForwardingConsol.Code,
				transportMode: transportMode,
				direction: direction,
				origin: origin,
				destination: destination,
				voyageLoad: voyageLoad,
				voyageDischarge: voyageDischarge,
				lastModeRouteSetDischarge: lastModeRouteSetDischarge
			);

		public string GetViaForShipment(ZString transportMode, ZString direction, string origin, string destination, string firstLoad, string lastDischarge, string lastModeRouteSetDischarge) =>
			GetVia
			(
				jobType: JobType.Shipment.Code,
				transportMode: transportMode,
				direction: direction,
				origin: origin,
				destination: destination,
				firstLoad: firstLoad,
				lastDischarge: lastDischarge,
				lastModeRouteSetDischarge: lastModeRouteSetDischarge
			);

		public string GetViaForQuotedBooking(ZString transportMode, ZString direction, string origin, string destination, string voyageLoad, string voyageDischarge) =>
			GetVia
			(
				jobType: JobType.QuotedBooking.Code,
				transportMode: transportMode,
				direction: direction,
				origin: origin,
				destination: destination,
				voyageLoad: voyageLoad,
				voyageDischarge: voyageDischarge
			);

		string GetVia
			(
			ZString jobType, // FCN, SHP, QSH
			ZString transportMode, // AIR, SEA
			ZString direction, // EXP, IMP
			string origin,
			string destination,
			string firstLoad = null,
			string voyageLoad = null,
			string lastDischarge = null,
			string voyageDischarge = null,
			string lastModeRouteSetDischarge = null
			)
		{
			var filteredConfigurations = Configurations
				.Where(x => x.JobType == jobType)
				.Where(x => x.TransportMode == Core.Constants.TransportModes.All || x.TransportMode == transportMode);

			var settings = filteredConfigurations.SelectMany(x => x.Settings.Select(s => s as AutoratingViaPortSetting).WhereNotNull());

			var applicableSettings = settings
				.Where(x => x.Direction == DirectionOption.All.Code || x.Direction == direction)
				.Where(x => string.IsNullOrEmpty(x.OriginSourceOption) || Compare(origin, x.OriginSourceOption))
				.Where(x => string.IsNullOrEmpty(x.DestinationSourceOption) || Compare(destination, x.DestinationSourceOption));

			return applicableSettings
				.Select(x => GetVia(x.ViaSourceOption))
				.FirstOrDefault(x => !string.IsNullOrEmpty(x));

			bool Compare(string location, ZString locationSourceOption)
			{
				switch (locationSourceOption)
				{
					case LocationSourceOption.Code.FirstLoad:
						return location == firstLoad;
					case LocationSourceOption.Code.NotFirstLoad:
						return location != firstLoad;
					case LocationSourceOption.Code.VoyageLoad:
						return location == voyageLoad;
					case LocationSourceOption.Code.NotVoyageLoad:
						return location != voyageLoad;
					case LocationSourceOption.Code.LastDischarge:
						return location == lastDischarge;
					case LocationSourceOption.Code.NotLastDischarge:
						return location != lastDischarge;
					case LocationSourceOption.Code.VoyageDischarge:
						return location == voyageDischarge;
					case LocationSourceOption.Code.NotVoyageDischarge:
						return location != voyageDischarge;
				}

				return false;
			}

			string GetVia(ZString viaSourceOption)
			{
				switch (viaSourceOption)
				{
					case LocationSourceOption.Code.FirstLoad:
						return firstLoad;
					case LocationSourceOption.Code.LastDischarge:
						return lastDischarge;
					case LocationSourceOption.Code.VoyageLoad:
						return voyageLoad;
					case LocationSourceOption.Code.VoyageDischarge:
						return voyageDischarge;
					case LocationSourceOption.Code.LastModeRouteSetDischarge:
						return lastModeRouteSetDischarge;
				}

				return null;
			}
		}

		IEnumerable<AutoratingViaPortConfiguration> Configurations
		{
			get
			{
				if (!isConfigurationsSet)
				{
					configurations = Value
						.Select(x => x as AutoratingViaPortConfiguration)
						.WhereNotNull()
						.ToList();

					isConfigurationsSet = true;
				}

				return configurations;
			}
		}

		bool isConfigurationsSet;
		IEnumerable<AutoratingViaPortConfiguration> configurations;

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);

			isConfigurationsSet = false;
			configurations = null;
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.AutoratingViaPortRegistryItemEditor, Enterprise.Registry.GUI")]
	class AutoratingViaPortRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AutoratingViaPortConfigurationCollection>
	{
		public AutoratingViaPortRegistryDataType()
		{
		}
	}
}
