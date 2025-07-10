using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public enum WebModuleId
	{
		NotAssignedWeb,
#if DEBUG
		Dummy,
		DummyWeb,
		DummyDate,
		DummyTreeView,
		NestedDummyWeb,
#endif
		OrganisationWeb,
		LocationWeb,
		RefCountryWeb,
		RefUNLOCOWeb,
		RefContainerWeb,
		RefCurrencyWeb,
		RefCommodityCodeWeb,
		RefVesselWeb,
		OrganisationWebTracking,
		OrgAgentTracking,
		OrgSeaCarrierTracking,
		OrgAirCarrierTracking,
		OrgRailCarrierTracking,
		OrgRoadCarrierTracking,
		OrgConsigneeTracking,
		OrgConsignorTracking,
		OrgSupplierWebTracking,
		OrgSupplierPartWeb,
		OrgReceivablesTracking,
		OrgAddressReceivablesTracking,
		AHECCWeb,
		NZCCCWeb,
		NZConcessions,
		CMRWebCodeLists,
		CMRInstrumentNumber,
		RefServiceLevelWeb,
		CMRSeaCargo,
		CMRAirCargo,
		TrackingBookings,
		TrackingDeclarations,
		TrackingCartage,
		TrackingImporterSecurityFiling,
		TrackingOrders,
		TrackingOrdersTimeline,
		TrackingShipments,
		TrackingCFSShipments,
		TrackingAccounts,
		TrackingWarehouse,
		TrackingInventory,
		ShoppingCartInventory,
		TrackingInventoryDetails,
		TrackingWarehouseOrders,
		TrackingQuotations,
		TrackingContainers,
		LinerAndAgencyContainers,
		TrackingWarehouseReceive,
		TrackingSailingSchedules,
		TrackingFlightSchedules,
		TrackingRailSchedules,
		TrackingRoadSchedules,
		EDIClassrooms,
		EDIIncidents,
		eDoc,
		CargoWiseEDIClassrooms,
		CargoWiseEDIIncidents,
		CFSContainerAvailability,
		CFSFumigation,
		CFSSailings,
		LinerAndAgencyBookings,
		LinerAndAgencyBillsOfLading,
		DangerousGoods,
		OrgAddressWeb,
		OrgContactWeb,
		TrackingOrderLines,
		TrackingUSCForeignPort,
		TrackingUSCRegionDistrictPort,
		TrackingMAWB,
		TrackingHAWB,
		TrackingDefault,
		TrackingHousebill,
		TrackingFreightLabel,
		CargoWiseEDIWebSecurityContacts,
		CargoWiseEDINotificationRolesContacts,
		OrgCarrierTracking,
		GlbPersonWeb
	}

	public static class WebModuleOptions
	{
		public static string Ref => nameof(Ref); // Non-semantic text
		public static string Number => nameof(Number); // Non-semantic text

		public static class TrackingQuotations
		{
			public static string ClientAccepts => nameof(ClientAccepts); // Non-semantic text
			public static string ClientNotAccepts => nameof(ClientNotAccepts); // Non-semantic text
		}
	}

	[TypeConverter(typeof(WebModuleIDConverter))]
	public class WebModuleID : ModuleIdentifier
	{
		protected internal WebModuleID(Enum iD, string menuName, string menuNameWithAmpersand)
			: base(iD, (NoResString)menuName)
		{
		}
	}

	public class WebModuleIDConverter : ModuleIDConverter
	{
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				WebModuleID moduleID = (WebModuleID)value;
				FieldInfo notAssignedModule = null;

				foreach (FieldInfo module in Modules)
				{
					WebModuleID otherModuleID = (WebModuleID)module.GetValue(null);
					if (moduleID == otherModuleID)
					{
						return new InstanceDescriptor(module, Array.Empty<object>());
					}
					else if (moduleID == WebModuleIDs.NotAssigned)
					{
						notAssignedModule = module;
					}
				}

				return new InstanceDescriptor(notAssignedModule, Array.Empty<object>());
			}
			else
			{
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value.GetType() == typeof(string))
			{
				foreach (FieldInfo module in Modules)
				{
					WebModuleID moduleID = (WebModuleID)module.GetValue(null);
					if (moduleID.ToString() == value.ToString())
					{
						return moduleID;
					}
				}

				return WebModuleIDs.NotAssigned;
			}
			else
			{
				return base.ConvertFrom(context, culture, value);
			}
		}

		FieldInfo[] fModules;
		FieldInfo[] Modules
		{
			get
			{
				if (fModules == null)
				{
					fModules = GetFields(typeof(WebModuleIDs));
				}
				return fModules;
			}
		}

		FieldInfo[] GetFields(Type moduleIDsType)
		{
			ArrayList result = new ArrayList();
			ArrayList typesToCheck = new ArrayList();
			int currentPosition = 0;
			typesToCheck.Add(moduleIDsType);
			while (currentPosition < typesToCheck.Count)
			{
				Type typeToCheck = (Type)typesToCheck[currentPosition];
				result.AddRange(typeToCheck.GetFields(BindingFlags.Static | BindingFlags.Public));
				typesToCheck.AddRange(typeToCheck.GetNestedTypes());
				currentPosition++;
			}

			return (FieldInfo[])result.ToArray(typeof(FieldInfo));
		}
	}
}
