using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CA.Module.Testing
{
	class ColumnsAndFiltersProviderTest : ZFilterStripControlTest
	{
		public void TestRNSFilterAndColumnsOnlyVisibleForCanadianCompanies()
		{
			foreach (var country in new[] { Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Australia })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var shipments = new ShipmentCollection(Factory);
					var filterBO = new JobShipmentFilterBusinessObject();
					using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
					{
						var rnsStatus = filterControl.FilterBusinessObject.ModuleFilters["RNS Release Status"];
						if (country == Core.Constants.CountryCodes.Canada)
						{
							AssertNotNull("RNS Release Status", rnsStatus);
						}
						else
						{
							AssertNull("RNS Relase Status", rnsStatus);
						}
					}
				}
			}
		}

		internal static void AssertStatusQuery(ModuleTextFilter filter, SQLComparisonOperator comparisonOperator, string property, params ForwardingShipment[] expectedShipments)
		{
			filter.SqlComparisonOperator = comparisonOperator;
			filter.Property = property;
			AssertProperShipmentsLoaded(filter, expectedShipments);
		}

		internal static void AssertProperShipmentsLoaded(ModuleFilter filter, ForwardingShipment[] expectedShipments)
		{
			var shipments = expectedShipments[0].Factory.Load<ForwardingShipment>(filter.Query);
			AssertEquals("Proper quantity of shipments loaded", expectedShipments.Length, shipments.Length);
			foreach (var shipment in expectedShipments)
			{
				var shipmentCached = shipment;
				Assert("Shipment should be loaded", shipments.Any(s => s.PK == shipmentCached.PK));
			}
		}
	}
}
