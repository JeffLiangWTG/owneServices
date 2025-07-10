using CargoWise.Integration;
using Enterprise.Integration.Web;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Web.GUI.Res;
using ResString = Enterprise.ZArchitecture.Web.GUI.ResString;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class WebEDocsDownloadModulesList : IWebEDocsDownloadModulesList
	{
		public ICodeDescription AllModules
		{
			get
			{
				return allModules ?? (allModules = new CodeDescriptionPair("AllModules", Res.GetString("7DEDCCF8-4F24-422A-9AC7-51C4E16B4595", "All Modules")));
			}
		}
		ICodeDescription allModules;

		public ICodeDescriptionPairList ModulesCodeDescriptionPairList
		{
			get
			{
				if (modulesCodeDescriptionPairList == null)
				{
					GetModulesCodeDescriptionPairList();
				}

				return modulesCodeDescriptionPairList;
			}
		}
		CodeDescriptionPairList modulesCodeDescriptionPairList;

		void GetModulesCodeDescriptionPairList()
		{
			modulesCodeDescriptionPairList = new CodeDescriptionPairList();
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingOrderLines.Name, Descriptions.TrackingOrderLines);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingBookings.Name, Descriptions.TrackingBookings);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingDeclarations.Name, Descriptions.TrackingDeclarations);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingCartage.Name, Descriptions.TrackingCartage);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingImporterSecurityFiling.Name, Descriptions.TrackingImporterSecurityFiling);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingOrders.Name, Descriptions.TrackingOrders);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingOrdersTimeline.Name, Descriptions.TrackingOrdersTimeline);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingShipments.Name, Descriptions.TrackingShipments);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingAccounts.Name, Descriptions.TrackingAccounts);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingWarehouse.Name, Descriptions.TrackingWarehouse);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingMAWB.Name, Descriptions.TrackingMAWB);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingWarehouseOrders.Name, Descriptions.TrackingWarehouseOrders);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingQuotations.Name, Descriptions.TrackingQuotations);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingContainers.Name, Descriptions.TrackingContainers);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingWarehouseReceive.Name, Descriptions.TrackingWarehouseReceive);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingSailingSchedules.Name, Descriptions.TrackingSailingSchedules);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingFlightSchedules.Name, Descriptions.TrackingFlightSchedules);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingRoadSchedules.Name, Descriptions.TrackingRoadSchedules);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingRailSchedules.Name, Descriptions.TrackingRailSchedules);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.CMRSeaCargo.Name, Descriptions.CMRSeaCargo);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.CMRAirCargo.Name, Descriptions.CMRAirCargo);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.TrackingHAWB.Name, Descriptions.TrackingHAWB);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.CargoWiseEDIClassroms.Name, Descriptions.CargoWiseEDIClassroms);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.CargoWiseEDIIncidents.Name, Descriptions.CargoWiseEDIIncidents);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.CFSContainerAvailability.Name, Descriptions.CFSContainerAvailability);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.CFSFumigation.Name, Descriptions.CFSFumigation);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.CFSSailings.Name, Descriptions.CFSSailings);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.LinerAndAgencyBookings.Name, Descriptions.LinerAndAgencyBookings);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.LinerAndAgencyBillsOfLading.Name, Descriptions.LinerAndAgencyBillsOfLading);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.LinerAndAgencyContainers.Name, Descriptions.LinerAndAgencyContainers);
			modulesCodeDescriptionPairList.AddPair(WebModuleIDs.DangerousGoods.Name, Descriptions.DangerousGoods);
		}

		public bool ContainsCode(string moduleName)
		{
			return ModulesCodeDescriptionPairList.ContainsCode(moduleName);
		}

		public static class Descriptions
		{
			public static MultilingualString TrackingOrderLines { get { return ResString.GetMultilingualString("97D00C4D-28BD-43C7-8B44-ED097C9F60A0", "Order Lines"); } }
			public static MultilingualString TrackingBookings { get { return ResString.GetMultilingualString("9088DCEE-EBB9-4AFF-82C4-4C354E7DCD2F", "Bookings"); } }
			public static MultilingualString TrackingDeclarations { get { return ResString.GetMultilingualString("69415572-3FBC-44EC-B3F1-76669582B132", "Declarations"); } }
			public static MultilingualString TrackingCartage { get { return ResString.GetMultilingualString("A099AEE3-C5DC-4844-961E-F76C309D5F2F", "Cartage"); } }
			public static MultilingualString TrackingImporterSecurityFiling { get { return ResString.GetMultilingualString("7CC5EA2A-769B-456E-B174-B5AC8E20EBB1", "Importer Security Filing"); } }
			public static MultilingualString TrackingOrders { get { return ResString.GetMultilingualString("FCA9539C-4861-4CD6-81BB-CD839CB07384", "Orders"); } }
			public static MultilingualString TrackingOrdersTimeline { get { return ResString.GetMultilingualString("DE504654-658D-419C-A441-E2BECBF8E22F", "Orders Timeline"); } }
			public static MultilingualString TrackingShipments { get { return ResString.GetMultilingualString("0F5EB822-8409-46C7-8CE3-837DAE5BB9F2", "Shipments"); } }
			public static MultilingualString TrackingAccounts { get { return ResString.GetMultilingualString("93982CA2-C731-4FAE-90C7-76AAB433FF3D", "Accounts"); } }
			public static MultilingualString TrackingWarehouse { get { return ResString.GetMultilingualString("8AA5ABBF-5C56-4790-A094-3D28AF53706D", "Warehouse"); } }
			public static MultilingualString TrackingMAWB { get { return ResString.GetMultilingualString("F44ED406-CC6D-4271-8886-F22954D09A80", "MAWB"); } }
			public static MultilingualString TrackingWarehouseOrders { get { return ResString.GetMultilingualString("89EC1011-ADAD-465E-91D9-89323578E3DA", "Warehouse Orders"); } }
			public static MultilingualString TrackingQuotations { get { return ResString.GetMultilingualString("F12D3EA7-C55A-4A31-853B-F17AC33340FA", "Quotations"); } }
			public static MultilingualString TrackingContainers { get { return ResString.GetMultilingualString("8C657710-8B45-416B-A69F-F094F2213D8B", "Containers"); } }
			public static MultilingualString TrackingWarehouseReceive { get { return ResString.GetMultilingualString("2850177C-1EC4-453E-B0A4-2FD2DE2BBB54", "Warehouse Receive"); } }
			public static MultilingualString TrackingSailingSchedules { get { return ResString.GetMultilingualString("26017912-4438-4919-99C8-481ADC5F4167", "Sailing Schedules"); } }
			public static MultilingualString TrackingFlightSchedules { get { return ResString.GetMultilingualString("44A3C507-3735-4054-A144-1070EBB24931", "Flight Schedules"); } }
			public static MultilingualString TrackingRoadSchedules { get { return ResString.GetMultilingualString("BE33F086-8FE4-451D-8787-899769183454", "Road Schedules"); } }
			public static MultilingualString TrackingRailSchedules { get { return ResString.GetMultilingualString("F790BD7A-2A57-4704-B730-F0AEB454A48A", "Rail Schedules"); } }
			public static MultilingualString CMRSeaCargo { get { return ResString.GetMultilingualString("0AE1FA81-2566-4992-B930-CBAD96840722", "CMR Sea Cargo"); } }
			public static MultilingualString CMRAirCargo { get { return ResString.GetMultilingualString("CE48D276-452F-4D04-9F76-4CC58C3378F8", "CMR Air Cargo"); } }
			public static MultilingualString TrackingHAWB { get { return ResString.GetMultilingualString("877E6C69-7AB3-414A-B604-1DAD57613577", "HAWB"); } }
			public static MultilingualString CargoWiseEDIClassroms { get { return ResString.GetMultilingualString("A1C6E011-E94B-4AF1-B944-B73C9ED4E7AE", "Classroom Sessions"); } }
			public static MultilingualString CargoWiseEDIIncidents { get { return ResString.GetMultilingualString("61305756-B5E5-482C-8FCB-0E5B5D8D8666", "Customer Service Incidents"); } }
			public static MultilingualString CFSContainerAvailability { get { return ResString.GetMultilingualString("7CEA6114-3FAC-4C4B-895B-2357A6F2368A", "Container Availability"); } }
			public static MultilingualString CFSFumigation { get { return ResString.GetMultilingualString("2D14B377-C473-47D3-9897-CF53874E2094", "Fumigation"); } }
			public static MultilingualString CFSSailings { get { return ResString.GetMultilingualString("50A6D6AA-9DB6-45A9-AA33-BDF2812E3B7E", "Sailings"); } }
			public static MultilingualString LinerAndAgencyBookings { get { return ResString.GetMultilingualString("426F444E-534A-4008-ABAE-28B66D348276", "Liner And Agency Bookings"); } }
			public static MultilingualString LinerAndAgencyBillsOfLading { get { return ResString.GetMultilingualString("8748229A-EE0B-472C-9326-4CE2D955A00E", "Liner And Agency Bills Of Lading"); } }
			public static MultilingualString LinerAndAgencyContainers { get { return ResString.GetMultilingualString("BA295CE7-9FA6-40AF-B2A0-FBDEEAF18A1A", "Liner And Agency Containers"); } }
			public static MultilingualString DangerousGoods { get { return ResString.GetMultilingualString("E46302D4-D09F-402A-BFCB-6094F0A56E3D", "Dangerous Goods"); } }
		}
	}
}
