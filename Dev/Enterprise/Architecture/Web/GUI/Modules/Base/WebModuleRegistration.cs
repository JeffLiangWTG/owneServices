using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.ZArchitecture.Modules;

#if DEBUG
using CargoWise.Common.Testing;
using WTG.StaticAnalysis.Annotation;
#endif

namespace Enterprise.ZArchitecture.Web.Modules
{
	#region class WebModuleIDs

	public static class WebModuleIDs
	{
		public static IEnumerable<WebModuleID> All
		{
			get { return fAll ?? (fAll = (WebModuleID[])ModuleIDLoader.GetModuleIDs(typeof(WebModuleIDs), typeof(WebModuleID))); }
		}

#if DEBUG
		[SuppressThreadStaticFieldMessage]
#endif
		static WebModuleID[] fAll;

		public static readonly WebModuleID NotAssigned = new WebModuleID(WebModuleId.NotAssignedWeb, "NotAssignedWeb", "&NotAssignedWeb"); // Unknown & behaviour in different languages

#if DEBUG
		public static readonly WebModuleID Dummy = new WebModuleID(WebModuleId.DummyWeb, "DummyWeb", "Dummy&Web");
		public static readonly WebModuleID DummyDate = new WebModuleID(WebModuleId.DummyDate, "DummyDate", "Dummy&Date");
		public static readonly WebModuleID DummyZTreeView = new WebModuleID(WebModuleId.DummyTreeView, "DummyTreeView", "Dummy&TreeView");
		public abstract class DummyClass
		{
			public static readonly WebModuleID NestedDummy = new WebModuleID(WebModuleId.NestedDummyWeb, "NestedDummyWeb", "NestedD&ummyWeb");
		}
#endif

		/// <summary>
		/// Findboxes
		/// </summary>
		public static readonly WebModuleID Organisation = new WebModuleID(WebModuleId.OrganisationWeb, "OrganisationWeb", "&OrganisationWeb"); // Unknown & behaviour in different languages
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID Location = new WebModuleID(WebModuleId.LocationWeb, "Location", "&Location");
		public static readonly WebModuleID RefCountry = new WebModuleID(WebModuleId.RefCountryWeb, "RefCountry", "&RefCountry"); // Unknown & behaviour in different languages
		public static readonly WebModuleID RefUNLOCO = new WebModuleID(WebModuleId.RefUNLOCOWeb, "RefUNLOCO", "&RefUNLOCO"); // Unknown & behaviour in different languages
		public static readonly WebModuleID RefContainer = new WebModuleID(WebModuleId.RefContainerWeb, "RefContainerWeb", "&RefContainerWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID RefCurrency = new WebModuleID(WebModuleId.RefCurrencyWeb, "RefCurrency", "&RefCurrency"); // Unknown & behaviour in different languages
		public static readonly WebModuleID RefCommodityCode = new WebModuleID(WebModuleId.RefCommodityCodeWeb, "RefCommodityCode", "&RefCommodityCode"); // Unknown & behaviour in different languages
		public static readonly WebModuleID RefVessel = new WebModuleID(WebModuleId.RefVesselWeb, "RefVesselModule", "&RefVesselModule"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrganisationTracking = new WebModuleID(WebModuleId.OrganisationWebTracking, "OrganisationWeb", "&OrganisationWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgCarrierTracking = new WebModuleID(WebModuleId.OrgCarrierTracking, "OrganisationWeb", "&OrganisationWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgAgentTracking = new WebModuleID(WebModuleId.OrgAgentTracking, "OrganisationWeb", "&OrganisationWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgSeaCarrierTracking = new WebModuleID(WebModuleId.OrgSeaCarrierTracking, "OrganisationWeb", "&OrganisationWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgAirCarrierTracking = new WebModuleID(WebModuleId.OrgAirCarrierTracking, "OrganisationWeb", "&OrganisationWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgRailCarrierTracking = new WebModuleID(WebModuleId.OrgRailCarrierTracking, "OrganisationWeb", "&OrganisationWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgRoadCarrierTracking = new WebModuleID(WebModuleId.OrgRoadCarrierTracking, "OrganisationWeb", "&OrganisationWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgConsigneeTracking = new WebModuleID(WebModuleId.OrgConsigneeTracking, "OrgConsigneeWeb", "&OrgConsigneeWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgConsignorTracking = new WebModuleID(WebModuleId.OrgConsignorTracking, "OrgConsignorWeb", "&OrgConsignorWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgSupplierTracking = new WebModuleID(WebModuleId.OrgSupplierWebTracking, "OrganisationWeb", "&OrgConsigneeWeb"); // Unknown & behaviour in different languages
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Unknown & behaviour in different languages")]
		public static readonly WebModuleID OrgSupplierPartTracking = new WebModuleID(WebModuleId.OrgSupplierPartWeb, "OrgSupplierPart", "OrgSupplier&Part");
		public static readonly WebModuleID OrgReceivablesTracking = new WebModuleID(WebModuleId.OrgReceivablesTracking, "OrganisationWeb", "&OrganisationWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgAddressReceivablesTracking = new WebModuleID(WebModuleId.OrgAddressReceivablesTracking, "OrgAddress", "&OrgAddress"); // Unknown & behaviour in different languages
		public static readonly WebModuleID AHECC = new WebModuleID(WebModuleId.AHECCWeb, "AHECCWeb", "&AHECCWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID NZCCC = new WebModuleID(WebModuleId.NZCCCWeb, "NZCCCWeb", "N&ZCCCWeb"); // Unknown & behaviour in different languages
		public static readonly WebModuleID NZConcessions = new WebModuleID(WebModuleId.NZConcessions, "NZConcessionModule", "&NZConcessionModule"); // Unknown & behaviour in different languages
		public static readonly WebModuleID CMRWebCodeLists = new WebModuleID(WebModuleId.CMRWebCodeLists, "CMRCodeListsModule", "&CMRCodeListsModule"); // Unknown & behaviour in different languages
		public static readonly WebModuleID CMRInstrumentNumber = new WebModuleID(WebModuleId.CMRInstrumentNumber, "CMRInstrumentNumberModule", "&CMRInstrumentNumberModule"); // Unknown & behaviour in different languages
		public static readonly WebModuleID RefServiceLevel = new WebModuleID(WebModuleId.RefServiceLevelWeb, "RefServiceLevel", "&RefServiceLevel"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgAddress = new WebModuleID(WebModuleId.OrgAddressWeb, "OrgAddress", "&OrgAddress"); // Unknown & behaviour in different languages
		public static readonly WebModuleID OrgContact = new WebModuleID(WebModuleId.OrgContactWeb, "OrgContact", "&OrgContact"); // Unknown & behaviour in different languages
		public static readonly WebModuleID GlbPerson = new WebModuleID(WebModuleId.GlbPersonWeb, "GlbPersonWeb", "&GlbPersonWeb"); // Unknown & behaviour in different languages

		/// <summary>
		/// Search screens
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingOrderLines = new WebModuleID(WebModuleId.TrackingOrderLines, "Order Lines", "&Order Lines");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID CMRSeaCargo = new WebModuleID(WebModuleId.CMRSeaCargo, "CMR Sea Cargo", "CMR &Sea Cargo");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID CMRAirCargo = new WebModuleID(WebModuleId.CMRAirCargo, "CMR Air Cargo", "CMR &Air Cargo");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingBookings = new WebModuleID(WebModuleId.TrackingBookings, "Bookings", "&Bookings");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingDeclarations = new WebModuleID(WebModuleId.TrackingDeclarations, "Declarations", "&Declarations");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingCartage = new WebModuleID(WebModuleId.TrackingCartage, "Cartage", "&Cartage");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingImporterSecurityFiling = new WebModuleID(WebModuleId.TrackingImporterSecurityFiling, "Importer Security Filing", "&Importer Security Filing");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingOrders = new WebModuleID(WebModuleId.TrackingOrders, "Orders", "&Orders");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingOrdersTimeline = new WebModuleID(WebModuleId.TrackingOrdersTimeline, "Orders", "&Orders");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingShipments = new WebModuleID(WebModuleId.TrackingShipments, "Shipments", "&Shipments");
		public static readonly WebModuleID TrackingCFSShipments = new WebModuleID(WebModuleId.TrackingCFSShipments, "CFSShipments", "&CFSShipments"); // May be an identifier or GUID.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingAccounts = new WebModuleID(WebModuleId.TrackingAccounts, "Accounts", "&Accounts");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingWarehouse = new WebModuleID(WebModuleId.TrackingWarehouse, "Warehouse", "&Warehouse");
		public static readonly WebModuleID TrackingMAWB = new WebModuleID(WebModuleId.TrackingMAWB, "MAWB", "&MAWB"); // May be an identifier or GUID.
		public static readonly WebModuleID TrackingHAWB = new WebModuleID(WebModuleId.TrackingHAWB, "HAWB", "&HAWB"); // May be an identifier or GUID.

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingInventory = new WebModuleID(WebModuleId.TrackingInventory, "Inventory", "&Inventory");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID ShoppingCartInventory = new WebModuleID(WebModuleId.ShoppingCartInventory, "Inventory", "&Inventory");

		public static readonly WebModuleID TrackingInventoryDetails = new WebModuleID(WebModuleId.TrackingInventoryDetails, "InventoryDetails", "&InventoryDetails"); // Unknown & behaviour in different languages
		public static readonly WebModuleID TrackingWarehouseOrders = new WebModuleID(WebModuleId.TrackingWarehouseOrders, "WarehouseOrders", "&WarehouseOrders"); // Unknown & behaviour in different languages
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingQuotations = new WebModuleID(WebModuleId.TrackingQuotations, "Quotations", "&Quotations");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingContainers = new WebModuleID(WebModuleId.TrackingContainers, "Containers", "&Containers");
		public static readonly WebModuleID TrackingWarehouseReceive = new WebModuleID(WebModuleId.TrackingWarehouseReceive, "WarehouseReceive", "&WarehouseReceive"); // Unknown & behaviour in different languages

		public static readonly WebModuleID TrackingSailingSchedules = new WebModuleID(WebModuleId.TrackingSailingSchedules, "SailingSchedules", "&SailingSchedules"); // Unknown & behaviour in different languages
		public static readonly WebModuleID TrackingFlightSchedules = new WebModuleID(WebModuleId.TrackingFlightSchedules, "FlightSchedules", "&FlightSchedules"); // Unknown & behaviour in different languages
		public static readonly WebModuleID TrackingRoadSchedules = new WebModuleID(WebModuleId.TrackingRoadSchedules, "RoadSchedules", "&RoadSchedules"); // Unknown & behaviour in different languages
		public static readonly WebModuleID TrackingRailSchedules = new WebModuleID(WebModuleId.TrackingRailSchedules, "RailSchedules", "&RailSchedules"); // Unknown & behaviour in different languages

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID EDIClassrooms = new WebModuleID(WebModuleId.EDIClassrooms, "Classroom Sessions", "&Classroom Sessions");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID EDIIncidents = new WebModuleID(WebModuleId.EDIIncidents, "Customer Service Incidents", "&Customer Service Incidents");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID CargoWiseEDIClassroms = new WebModuleID(WebModuleId.CargoWiseEDIClassrooms, "Classroom Sessions", "&Classroom Sessions");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID CargoWiseEDIIncidents = new WebModuleID(WebModuleId.CargoWiseEDIIncidents, "Customer Service Incidents", "&Customer Service Incidents");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID CargoWiseEDIWebSecurityContacts = new WebModuleID(WebModuleId.CargoWiseEDIWebSecurityContacts, "Web Security Contacts", "&Web Security Contacts");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID CargoWiseEDINotificationRolesContacts = new WebModuleID(WebModuleId.CargoWiseEDINotificationRolesContacts, "Notification Roles Contacts", "&Notification Roles Contacts");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID CFSContainerAvailability = new WebModuleID(WebModuleId.CFSContainerAvailability, "Container Availability", "&Container Availability");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID CFSFumigation = new WebModuleID(WebModuleId.CFSFumigation, "Fumigation", "&Fumigation");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID CFSSailings = new WebModuleID(WebModuleId.CFSSailings, "Sailings", "&Sailings");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID LinerAndAgencyBookings = new WebModuleID(WebModuleId.LinerAndAgencyBookings, "Liner And Agency Bookings", "&Liner And Agency Bookings");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID LinerAndAgencyBillsOfLading = new WebModuleID(WebModuleId.LinerAndAgencyBillsOfLading, "Liner And Agency Bills Of Lading", "&Liner And Agency Bills Of Lading");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID LinerAndAgencyContainers = new WebModuleID(WebModuleId.LinerAndAgencyContainers, "Liner And Agency Containers", "&Liner And Agency Containers");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID DangerousGoods = new WebModuleID(WebModuleId.DangerousGoods, "Dangerous Goods", "&Dangerous Goods");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingUSCForeignPort = new WebModuleID(WebModuleId.TrackingUSCForeignPort, "Schedule K Port Codes", "Schedule K Port Codes");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly WebModuleID TrackingUSCRegionDistrictPort = new WebModuleID(WebModuleId.TrackingUSCRegionDistrictPort, "Schedule D Port Codes", "Schedule D Port Codes");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter description string used for comparison")]
		public static WebModuleID GetWebModuleIDFromModuleID(ModuleIdentifier module, string description = null)
		{
			WebModuleID result = NotAssigned;
			if (module != null)
			{
				var moduleID = (ModuleId)module.ID;
				switch (moduleID)
				{
					case ModuleId.QuotedBookings:
						result = TrackingBookings;
						break;
					case ModuleId.JobShipment:
						result = TrackingShipments;
						break;
					case ModuleId.ShipmentReceival:
						result = TrackingCFSShipments;
						break;
					case ModuleId.CusDec:
						result = TrackingDeclarations;
						break;
					case ModuleId.WhsOrder:
						result = TrackingWarehouseOrders;
						break;
					case ModuleId.USCForeignPort:
						result = TrackingUSCForeignPort;
						break;
					case ModuleId.USCRegionDistrictPort:
						result = TrackingUSCRegionDistrictPort;
						break;
					case ModuleId.Organisation:
						if (description?.Equals("Carrier", StringComparison.OrdinalIgnoreCase) ?? false)
						{
							result = OrgCarrierTracking;
						}
						else
						{
							result = OrganisationTracking;
						}
						break;
					case ModuleId.Location:
						result = Location;
						break;
					case ModuleId.RefCountry:
						result = RefCountry;
						break;
					case ModuleId.RefUNLOCO:
						result = RefUNLOCO;
						break;
					case ModuleId.RefVessel:
						result = RefVessel;
						break;
					case ModuleId.RefCommodityCode:
						result = RefCommodityCode;
						break;
					case ModuleId.WhsConfigProduct:
						result = OrgSupplierPartTracking;
						break;
					case ModuleId.WhsConfigWarehouse:
						result = TrackingWarehouse;
						break;
					case ModuleId.ServiceLevel:
						result = RefServiceLevel;
						break;
					case ModuleId.SupplierPart:
						result = OrgSupplierPartTracking;
						break;
					case ModuleId.GlbPerson:
						result = GlbPerson;
						break;
				}
			}
			return result;
		}

		public static bool IsFilterStripModule(ModuleIdentifier module)
		{
			return module == WebModuleIDs.TrackingWarehouse ||
				module == WebModuleIDs.OrgSupplierPartTracking ||
				module == WebModuleIDs.TrackingInventoryDetails ||
				module == WebModuleIDs.TrackingSailingSchedules ||
				module == WebModuleIDs.TrackingFlightSchedules ||
				module == WebModuleIDs.TrackingRoadSchedules ||
				module == WebModuleIDs.TrackingRailSchedules ||
				module == WebModuleIDs.DangerousGoods ||
				module == WebModuleIDs.TrackingOrders ||
				module == WebModuleIDs.TrackingUSCForeignPort ||
				module == WebModuleIDs.TrackingUSCRegionDistrictPort ||
				module == WebModuleIDs.TrackingOrderLines ||
				module == WebModuleIDs.GlbPerson;
		}
	}

	#endregion

	#region class WebModuleList

	public class WebModuleList : ModuleList
	{
		public WebModuleList()
		{
#if DEBUG
			Add(new ModuleInfo(WebModuleIDs.Dummy, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.Testing.WebDummyModule"));
			Add(new ModuleInfo(WebModuleIDs.DummyDate, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.Testing.DateDummyModule"));
			Add(new ModuleInfo(WebModuleIDs.DummyZTreeView, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.Testing.DummyZTreeViewModule"));
#endif
			//Findboxes
			Add(new ModuleInfo(WebModuleIDs.Organisation, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.OrganisationModule"));
			Add(new ModuleInfo(WebModuleIDs.Location, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.LocationModule"));
			Add(new ModuleInfo(WebModuleIDs.RefCountry, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.RefCountryModule"));
			Add(new ModuleInfo(WebModuleIDs.RefUNLOCO, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.RefUNLOCOModule"));
			Add(new ModuleInfo(WebModuleIDs.RefContainer, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.RefContainerModule"));
			Add(new ModuleInfo(WebModuleIDs.RefCurrency, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.RefCurrencyModule"));
			Add(new ModuleInfo(WebModuleIDs.RefCommodityCode, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.RefCommodityCodeModule"));
			Add(new ModuleInfo(WebModuleIDs.RefVessel, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.RefVesselModule"));
			Add(new ModuleInfo(WebModuleIDs.OrganisationTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrganisationModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgCarrierTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgCarrierModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgAgentTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgAgentModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgSeaCarrierTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgSeaCarrierModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgAirCarrierTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgAirCarrierModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgRailCarrierTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgRailCarrierModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgRoadCarrierTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgRoadCarrierModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgConsigneeTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgConsigneeModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgConsignorTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgConsignorModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgSupplierTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgSupplierModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgSupplierPartTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgSupplierPartModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgReceivablesTracking, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.OrgReceivablesModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgAddressReceivablesTracking, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.OrgAddressReceivablesModule"));
			Add(new ModuleInfo(WebModuleIDs.RefServiceLevel, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.RefServiceLevelModule"));
			Add(new ModuleInfo(WebModuleIDs.GlbPerson, "ZClientWebEDI", "Enterprise.Client.EDI.Web.Module.GlbPersonModule"));

			Add(new ModuleInfo(WebModuleIDs.OrgAddress, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.OrgAddressModule"));
			Add(new ModuleInfo(WebModuleIDs.OrgContact, "Enterprise.ZArchitecture.Web.GUI", "Enterprise.ZArchitecture.Web.Modules.OrgContactModule"));

			///Search screens
			Add(new ModuleInfo(WebModuleIDs.CMRSeaCargo, "Enterprise.Customs.AU.CMRWeb.Module", "Enterprise.Customs.AU.CMRWeb.Module.CMRSeaCargoModule"));
			Add(new ModuleInfo(WebModuleIDs.CMRAirCargo, "Enterprise.Customs.AU.CMRWeb.Module", "Enterprise.Customs.AU.CMRWeb.Module.CMRAirCargoModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingBookings, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingBookingsModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingDeclarations, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingDeclarationsModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingCartage, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingCartageModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingImporterSecurityFiling, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingImporterSecurityFilingModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingOrders, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingOrdersModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingOrderLines, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingOrderLinesModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingOrdersTimeline, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingOrdersTimelineModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingShipments, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingShipmentsModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingCFSShipments, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingCFSShipmentsModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingAccounts, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingAccountsModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingQuotations, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingQuotationsModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingContainers, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingContainersModule"));

			Add(new ModuleInfo(WebModuleIDs.TrackingFlightSchedules, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingFlightSchedulesModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingSailingSchedules, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingSailingSchedulesModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingRailSchedules, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingRailSchedulesModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingRoadSchedules, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingRoadSchedulesModule"));

			Add(new ModuleInfo(WebModuleIDs.CFSContainerAvailability, "Enterprise.WebCFS.Module", "Enterprise.WebCFS.Module.CFSContainerAvailabilityModule"));
			Add(new ModuleInfo(WebModuleIDs.CFSFumigation, "Enterprise.WebCFS.Module", "Enterprise.WebCFS.Module.CFSFumigationModule"));
			Add(new ModuleInfo(WebModuleIDs.CFSSailings, "Enterprise.WebCFS.Module", "Enterprise.WebCFS.Module.CFSSailingsModule"));
			Add(new ModuleInfo(WebModuleIDs.CargoWiseEDIClassroms, "ZClientWebEDI", "Enterprise.ZClientWebCargoWiseEDI.Module.EDIClassroomsModule"));
			Add(new ModuleInfo(WebModuleIDs.CargoWiseEDIIncidents, "ZClientWebEDI", "Enterprise.ZClientWebCargoWiseEDI.Module.EDIIncidentsModule"));
			Add(new ModuleInfo(WebModuleIDs.CargoWiseEDIWebSecurityContacts, "ZClientWebEDI", "Enterprise.ZClientWebCargoWiseEDI.WebSecurityContactsModule"));
			Add(new ModuleInfo(WebModuleIDs.CargoWiseEDINotificationRolesContacts, "ZClientWebEDI", "Enterprise.ZClientWebCargoWiseEDI.NotificationRolesContactsModule"));
			//Add(new ModuleInfo(WebModuleIDs.EDIClassrooms, "ZClientWebEDI", "Enterprise.ZClientWebEDI.Module.EDIClassroomsModule"));
			//Add(new ModuleInfo(WebModuleIDs.EDIIncidents, "ZClientWebEDI", "Enterprise.ZClientWebEDI.Module.EDIIncidentsModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingWarehouse, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingWarehouseModule"));

			Add(new ModuleInfo(WebModuleIDs.TrackingInventory, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingInventoryModule"));
			Add(new ModuleInfo(WebModuleIDs.ShoppingCartInventory, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.ShoppingCartInventoryModule"));

			Add(new ModuleInfo(WebModuleIDs.TrackingInventoryDetails, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingInventoryDetailsModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingWarehouseOrders, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingWhsOrderModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingWarehouseReceive, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingWhsReceiveModule"));

			Add(new ModuleInfo(WebModuleIDs.LinerAndAgencyBookings, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.LinerAndAgencyBookingsModule"));
			Add(new ModuleInfo(WebModuleIDs.LinerAndAgencyBillsOfLading, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.LinerAndAgencyBillOfLadingModule"));
			Add(new ModuleInfo(WebModuleIDs.LinerAndAgencyContainers, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.LinerAndAgencyContainersModule"));

			Add(new ModuleInfo(WebModuleIDs.DangerousGoods, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.DangerousGoodsModule"));

			Add(new ModuleInfo(WebModuleIDs.TrackingUSCForeignPort, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingUSCForeignPortModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingUSCRegionDistrictPort, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingUSCRegionDistrictPortModule"));

			Add(new ModuleInfo(WebModuleIDs.TrackingMAWB, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingMAWBModule"));
			Add(new ModuleInfo(WebModuleIDs.TrackingHAWB, "Enterprise.Tracking.Module", "Enterprise.Tracking.Module.TrackingHAWBModule"));
		}

#if DEBUG
		[TypeFactoryAnnotationMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method called via Reflection from Enterprise.ReflectionTestDeadCodeTest.TestNoDeadCode()")]
		static IEnumerable<string> TypeFactoryAnnotation()
		{
			using (Db.DisposableActionForDbConnection())
			{
				return new WebModuleList().All.Where(m => m != null && !string.IsNullOrEmpty(m.TypePath)).Select(m => m.TypePath);
			}
		}
#endif
	}

	#endregion
}
