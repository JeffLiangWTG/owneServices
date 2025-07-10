using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class WebEDocsDownloadModulesListTest : TestCase
	{
		public void TestList()
		{
			var list = (new WebEDocsDownloadModulesList().ModulesCodeDescriptionPairList) as CodeDescriptionPairList;
			AssertEquals(31, list.Count);

			AssertNotNull("The list must contain the code for the Order Lines module.", list[WebModuleIDs.TrackingOrderLines.Name]);
			AssertNotNull("The list must contain the code for the Bookings module.", list[WebModuleIDs.TrackingBookings.Name]);
			AssertNotNull("The list must contain the code for the Declarations module.", list[WebModuleIDs.TrackingDeclarations.Name]);
			AssertNotNull("The list must contain the code for the Cartage module.", list[WebModuleIDs.TrackingCartage.Name]);
			AssertNotNull("The list must contain the code for the Importer Security Filing module.", list[WebModuleIDs.TrackingImporterSecurityFiling.Name]);
			AssertNotNull("The list must contain the code for the Orders module.", list[WebModuleIDs.TrackingOrders.Name]);
			AssertNotNull("The list must contain the code for the Orders Timeline module.", list[WebModuleIDs.TrackingOrdersTimeline.Name]);
			AssertNotNull("The list must contain the code for the Shipments module.", list[WebModuleIDs.TrackingShipments.Name]);
			AssertNotNull("The list must contain the code for the Accounts module.", list[WebModuleIDs.TrackingAccounts.Name]);
			AssertNotNull("The list must contain the code for the Warehouse module.", list[WebModuleIDs.TrackingWarehouse.Name]);
			AssertNotNull("The list must contain the code for the MAWB module.", list[WebModuleIDs.TrackingMAWB.Name]);
			AssertNotNull("The list must contain the code for the Warehouse Orders module.", list[WebModuleIDs.TrackingWarehouseOrders.Name]);
			AssertNotNull("The list must contain the code for the Quotations module.", list[WebModuleIDs.TrackingQuotations.Name]);
			AssertNotNull("The list must contain the code for the Containers module.", list[WebModuleIDs.TrackingContainers.Name]);
			AssertNotNull("The list must contain the code for the Warehouse Receive module.", list[WebModuleIDs.TrackingWarehouseReceive.Name]);
			AssertNotNull("The list must contain the code for the Sailing Schedules module.", list[WebModuleIDs.TrackingSailingSchedules.Name]);
			AssertNotNull("The list must contain the code for the Flight Schedules module.", list[WebModuleIDs.TrackingFlightSchedules.Name]);
			AssertNotNull("The list must contain the code for the Road Schedules module.", list[WebModuleIDs.TrackingRoadSchedules.Name]);
			AssertNotNull("The list must contain the code for the Rail Schedules module.", list[WebModuleIDs.TrackingRailSchedules.Name]);
			AssertNotNull("The list must contain the code for the CMR Sea Cargo module.", list[WebModuleIDs.CMRSeaCargo.Name]);
			AssertNotNull("The list must contain the code for the CMR Air Cargo module.", list[WebModuleIDs.CMRAirCargo.Name]);
			AssertNotNull("The list must contain the code for the HAWB module.", list[WebModuleIDs.TrackingHAWB.Name]);
			AssertNotNull("The list must contain the code for the Classroom Sessions module.", list[WebModuleIDs.CargoWiseEDIClassroms.Name]);
			AssertNotNull("The list must contain the code for the Customer Service Incidents module.", list[WebModuleIDs.CargoWiseEDIIncidents.Name]);
			AssertNotNull("The list must contain the code for the Container Availability module.", list[WebModuleIDs.CFSContainerAvailability.Name]);
			AssertNotNull("The list must contain the code for the Fumigation module.", list[WebModuleIDs.CFSFumigation.Name]);
			AssertNotNull("The list must contain the code for the Sailings module.", list[WebModuleIDs.CFSSailings.Name]);
			AssertNotNull("The list must contain the code for the Liner And Agency Bookings module.", list[WebModuleIDs.LinerAndAgencyBookings.Name]);
			AssertNotNull("The list must contain the code for the Liner And Agency Bills Of Lading module.", list[WebModuleIDs.LinerAndAgencyBillsOfLading.Name]);
			AssertNotNull("The list must contain the code for the Liner And Agency Containers module.", list[WebModuleIDs.LinerAndAgencyContainers.Name]);
			AssertNotNull("The list must contain the code for the Dangerous Goods module.", list[WebModuleIDs.DangerousGoods.Name]);

			AssertEquals("The description must be 'Order Lines'", "Order Lines", list[WebModuleIDs.TrackingOrderLines.Name].Description);
			AssertEquals("The description must be 'Bookings'", "Bookings", list[WebModuleIDs.TrackingBookings.Name].Description);
			AssertEquals("The description must be 'Declarations'", "Declarations", list[WebModuleIDs.TrackingDeclarations.Name].Description);
			AssertEquals("The description must be 'Cartage'", "Cartage", list[WebModuleIDs.TrackingCartage.Name].Description);
			AssertEquals("The description must be 'Importer Security Filing'", "Importer Security Filing", list[WebModuleIDs.TrackingImporterSecurityFiling.Name].Description);
			AssertEquals("The description must be 'Orders'", "Orders", list[WebModuleIDs.TrackingOrders.Name].Description);
			AssertEquals("The description must be 'Orders Timeline'", "Orders Timeline", list[WebModuleIDs.TrackingOrdersTimeline.Name].Description);
			AssertEquals("The description must be 'Shipments'", "Shipments", list[WebModuleIDs.TrackingShipments.Name].Description);
			AssertEquals("The description must be 'Accounts'", "Accounts", list[WebModuleIDs.TrackingAccounts.Name].Description);
			AssertEquals("The description must be 'Warehouse'", "Warehouse", list[WebModuleIDs.TrackingWarehouse.Name].Description);
			AssertEquals("The description must be 'MAWB'", "MAWB", list[WebModuleIDs.TrackingMAWB.Name].Description);
			AssertEquals("The description must be 'Warehouse Orders'", "Warehouse Orders", list[WebModuleIDs.TrackingWarehouseOrders.Name].Description);
			AssertEquals("The description must be 'Quotations'", "Quotations", list[WebModuleIDs.TrackingQuotations.Name].Description);
			AssertEquals("The description must be 'Containers'", "Containers", list[WebModuleIDs.TrackingContainers.Name].Description);
			AssertEquals("The description must be 'Warehouse Receive'", "Warehouse Receive", list[WebModuleIDs.TrackingWarehouseReceive.Name].Description);
			AssertEquals("The description must be 'Sailing Schedules'", "Sailing Schedules", list[WebModuleIDs.TrackingSailingSchedules.Name].Description);
			AssertEquals("The description must be 'Flight Schedules'", "Flight Schedules", list[WebModuleIDs.TrackingFlightSchedules.Name].Description);
			AssertEquals("The description must be 'Road Schedules'", "Road Schedules", list[WebModuleIDs.TrackingRoadSchedules.Name].Description);
			AssertEquals("The description must be 'Rail Schedules'", "Rail Schedules", list[WebModuleIDs.TrackingRailSchedules.Name].Description);
			AssertEquals("The description must be 'CMR Sea Cargo'", "CMR Sea Cargo", list[WebModuleIDs.CMRSeaCargo.Name].Description);
			AssertEquals("The description must be 'CMR Air Cargo'", "CMR Air Cargo", list[WebModuleIDs.CMRAirCargo.Name].Description);
			AssertEquals("The description must be 'HAWB'", "HAWB", list[WebModuleIDs.TrackingHAWB.Name].Description);
			AssertEquals("The description must be 'Classroom Sessions'", "Classroom Sessions", list[WebModuleIDs.CargoWiseEDIClassroms.Name].Description);
			AssertEquals("The description must be 'Customer Service Incidents'", "Customer Service Incidents", list[WebModuleIDs.CargoWiseEDIIncidents.Name].Description);
			AssertEquals("The description must be 'Container Availability'", "Container Availability", list[WebModuleIDs.CFSContainerAvailability.Name].Description);
			AssertEquals("The description must be 'Fumigation'", "Fumigation", list[WebModuleIDs.CFSFumigation.Name].Description);
			AssertEquals("The description must be 'Sailings'", "Sailings", list[WebModuleIDs.CFSSailings.Name].Description);
			AssertEquals("The description must be 'Liner And Agency Bookings'", "Liner And Agency Bookings", list[WebModuleIDs.LinerAndAgencyBookings.Name].Description);
			AssertEquals("The description must be 'Liner And Agency Bills Of Lading'", "Liner And Agency Bills Of Lading", list[WebModuleIDs.LinerAndAgencyBillsOfLading.Name].Description);
			AssertEquals("The description must be 'Liner And Agency Containers'", "Liner And Agency Containers", list[WebModuleIDs.LinerAndAgencyContainers.Name].Description);
			AssertEquals("The description must be 'Dangerous Goods'", "Dangerous Goods", list[WebModuleIDs.DangerousGoods.Name].Description);

			foreach (ICodeDescription codePair1 in list)
			{
				foreach (ICodeDescription codePair2 in list)
				{
					if (codePair1.Description == codePair2.Description && codePair1.Code != codePair2.Code)
					{
						Fail("All of the descriptions in the list must be unique.");
					}
				}
			}
		}
	}
}
