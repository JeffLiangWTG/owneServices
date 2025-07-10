using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutoratingViaPortRegistryItem))]
	sealed class AutoratingViaPortRegistryItemTest : StronglyTypedRegistryItemTestCase<AutoratingViaPortConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<AutoratingViaPortConfigurationCollection, AutoratingViaPortConfigurationCollection> GetNewRegistryItem()
			=> new AutoratingViaPortRegistryItem("", null, null, null, RegistryStorageFlags.System, AutoratingViaPortConfigurationCollection.Default);

		public void TestGetViaForwardingConsol()
		{
			// Test Loading the correct Via
			AssertGetViaForForwardingConsol
			(
				"Case #1",
				new[] { ("FCN", "ALL", "ALL", "", "", "VL") },
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI",
				"GBLON"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #2",
				new[] { ("FCN", "ALL", "ALL", "", "", "VD") },
				"USCHI",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI",
				"GBLON"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #3",
				new[] { ("FCN", "ALL", "ALL", "", "", "LRD") },
				"GBLON",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI",
				"GBLON"
			);

			// Test Origin Source Options
			AssertGetViaForForwardingConsol
			(
				"Case #4",
				new[] { ("FCN", "ALL", "ALL", "VL", "", "VD") },
				"GBLON",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUSYD", // ==
				"GBLON"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #5",
				new[] { ("FCN", "ALL", "ALL", "VL", "", "VD") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL", // !=
				"GBLON"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #6",
				new[] { ("FCN", "ALL", "ALL", "NVL", "", "VD") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUSYD", // ==
				"GBLON"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #7",
				new[] { ("FCN", "ALL", "ALL", "NVL", "", "VD") },
				"GBLON",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL", // !=
				"GBLON"
			);

			// Test Destination Source Option
			AssertGetViaForForwardingConsol
			(
				"Case #8",
				new[] { ("FCN", "ALL", "ALL", "", "VD", "VL") },
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USLAX" // ==
			);
			AssertGetViaForForwardingConsol
			(
				"Case #9",
				new[] { ("FCN", "ALL", "ALL", "", "VD", "VL") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI" // !=
			);
			AssertGetViaForForwardingConsol
			(
				"Case #10",
				new[] { ("FCN", "ALL", "ALL", "", "NVD", "VL") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USLAX" // ==
			);
			AssertGetViaForForwardingConsol
			(
				"Case #11",
				new[] { ("FCN", "ALL", "ALL", "", "NVD", "VL") },
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI" // !=
			);

			// Test Direction
			AssertGetViaForForwardingConsol
			(
				"Case #12",
				new[]
				{
					("FCN", "ALL", "IMP", "", "", "VL"),
					("FCN", "ALL", "EXP", "", "", "VD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #13",
				new[]
				{
					("FCN", "ALL", "IMP", "", "", "VL"),
					("FCN", "ALL", "EXP", "", "", "VD"),
				},
				"USCHI",
				"SEA",
				"EXP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #14",
				new[]
				{
					("FCN", "ALL", "IMP", "", "", "VL"),
					("FCN", "ALL", "EXP", "", "", "VD"),
				},
				null,
				"SEA",
				"DOM",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #15",
				new[]
				{
					("FCN", "ALL", "ALL", "", "", "VL"),
					("FCN", "ALL", "EXP", "", "", "VD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);

			// Test Transport Mode
			AssertGetViaForForwardingConsol
			(
				"Case #16",
				new[]
				{
					("FCN", "SEA", "ALL", "", "", "VL"),
					("FCN", "AIR", "ALL", "", "", "VD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #17",
				new[]
				{
					("FCN", "SEA", "ALL", "", "", "VL"),
					("FCN", "AIR", "ALL", "", "", "VD"),
				},
				"USCHI",
				"AIR",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #18",
				new[]
				{
					("FCN", "SEA", "ALL", "", "", "VL"),
					("FCN", "AIR", "ALL", "", "", "VD"),
				},
				null,
				"ROA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForForwardingConsol
			(
				"Case #19",
				new[]
				{
					("FCN", "ALL", "ALL", "", "", "VL"),
					("FCN", "AIR", "ALL", "", "", "VD"),
				},
				"AUMEL",
				"ROA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
		}

		public void TestGetViaForShipment()
		{
			// Test Loading the correct Via
			AssertGetViaForShipment
			(
				"Case #1",
				new[] { ("SHP", "ALL", "ALL", "", "", "1L") },
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI",
				"GBLON"
			);
			AssertGetViaForShipment
			(
				"Case #2",
				new[] { ("SHP", "ALL", "ALL", "", "", "LD") },
				"USCHI",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI",
				"GBLON"
			);
			AssertGetViaForShipment
			(
				"Case #3",
				new[] { ("SHP", "ALL", "ALL", "", "", "LRD") },
				"GBLON",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI",
				"GBLON"
			);

			// Test Origin Source Options
			AssertGetViaForShipment
			(
				"Case #4",
				new[] { ("SHP", "ALL", "ALL", "1L", "", "LD") },
				"GBLON",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUSYD", // ==
				"GBLON"
			);
			AssertGetViaForShipment
			(
				"Case #5",
				new[] { ("SHP", "ALL", "ALL", "1L", "", "LD") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL", // !=
				"GBLON"
			);
			AssertGetViaForShipment
			(
				"Case #6",
				new[] { ("SHP", "ALL", "ALL", "N1L", "", "LD") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUSYD", // ==
				"GBLON"
			);
			AssertGetViaForShipment
			(
				"Case #7",
				new[] { ("SHP", "ALL", "ALL", "N1L", "", "LD") },
				"GBLON",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL", // !=
				"GBLON"
			);

			// Test Destination Source Option
			AssertGetViaForShipment
			(
				"Case #8",
				new[] { ("SHP", "ALL", "ALL", "", "LD", "1L") },
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USLAX" // ==
			);
			AssertGetViaForShipment
			(
				"Case #9",
				new[] { ("SHP", "ALL", "ALL", "", "LD", "1L") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI" // !=
			);
			AssertGetViaForShipment
			(
				"Case #10",
				new[] { ("SHP", "ALL", "ALL", "", "NLD", "1L") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USLAX" // ==
			);
			AssertGetViaForShipment
			(
				"Case #11",
				new[] { ("SHP", "ALL", "ALL", "", "NLD", "1L") },
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI" // !=
			);

			// Test Direction
			AssertGetViaForShipment
			(
				"Case #12",
				new[]
				{
					("SHP", "ALL", "IMP", "", "", "1L"),
					("SHP", "ALL", "EXP", "", "", "LD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForShipment
			(
				"Case #13",
				new[]
				{
					("SHP", "ALL", "IMP", "", "", "1L"),
					("SHP", "ALL", "EXP", "", "", "LD"),
				},
				"USCHI",
				"SEA",
				"EXP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForShipment
			(
				"Case #14",
				new[]
				{
					("SHP", "ALL", "IMP", "", "", "1L"),
					("SHP", "ALL", "EXP", "", "", "LD"),
				},
				null,
				"SEA",
				"DOM",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForShipment
			(
				"Case #15",
				new[]
				{
					("SHP", "ALL", "ALL", "", "", "1L"),
					("SHP", "ALL", "EXP", "", "", "LD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);

			// Test Transport Mode
			AssertGetViaForShipment
			(
				"Case #16",
				new[]
				{
					("SHP", "SEA", "ALL", "", "", "1L"),
					("SHP", "AIR", "ALL", "", "", "LD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForShipment
			(
				"Case #17",
				new[]
				{
					("SHP", "SEA", "ALL", "", "", "1L"),
					("SHP", "AIR", "ALL", "", "", "LD"),
				},
				"USCHI",
				"AIR",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForShipment
			(
				"Case #18",
				new[]
				{
					("SHP", "SEA", "ALL", "", "", "1L"),
					("SHP", "AIR", "ALL", "", "", "LD"),
				},
				null,
				"ROA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForShipment
			(
				"Case #19",
				new[]
				{
					("SHP", "ALL", "ALL", "", "", "1L"),
					("SHP", "AIR", "ALL", "", "", "LD"),
				},
				"AUMEL",
				"ROA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForShipment
			(
				"Case #19",
				new[]
				{
					("SHP", "SEA", "IMP", "1L", "LD", "LRD"),
					("SHP", "SEA", "IMP", "N1L", "LD", "LRD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"SGSIN",
				"AUSYD",
				"SGSIN",
				"AUSYD",
				"AUMEL"
			);
		}

		public void TestGetViaQuotedBooking()
		{
			// Test Loading the correct Via
			AssertGetViaForQuotedBooking
			(
				"Case #1",
				new[] { ("QSH", "ALL", "ALL", "", "", "VL") },
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #2",
				new[] { ("QSH", "ALL", "ALL", "", "", "VD") },
				"USCHI",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);

			// Test Origin Source Options
			AssertGetViaForQuotedBooking
			(
				"Case #4",
				new[] { ("QSH", "ALL", "ALL", "VL", "", "VD") },
				"GBLON",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUSYD", // ==
				"GBLON"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #5",
				new[] { ("QSH", "ALL", "ALL", "VL", "", "VD") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL", // !=
				"GBLON"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #6",
				new[] { ("QSH", "ALL", "ALL", "NVL", "", "VD") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUSYD", // ==
				"GBLON"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #7",
				new[] { ("QSH", "ALL", "ALL", "NVL", "", "VD") },
				"GBLON",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL", // !=
				"GBLON"
			);

			// Test Destination Source Option
			AssertGetViaForQuotedBooking
			(
				"Case #8",
				new[] { ("QSH", "ALL", "ALL", "", "VD", "VL") },
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USLAX" // ==
			);
			AssertGetViaForQuotedBooking
			(
				"Case #9",
				new[] { ("QSH", "ALL", "ALL", "", "VD", "VL") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI" // !=
			);
			AssertGetViaForQuotedBooking
			(
				"Case #10",
				new[] { ("QSH", "ALL", "ALL", "", "NVD", "VL") },
				null,
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USLAX" // ==
			);
			AssertGetViaForQuotedBooking
			(
				"Case #11",
				new[] { ("QSH", "ALL", "ALL", "", "NVD", "VL") },
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI" // !=
			);

			// Test Direction
			AssertGetViaForQuotedBooking
			(
				"Case #12",
				new[]
				{
					("QSH", "ALL", "IMP", "", "", "VL"),
					("QSH", "ALL", "EXP", "", "", "VD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #13",
				new[]
				{
					("QSH", "ALL", "IMP", "", "", "VL"),
					("QSH", "ALL", "EXP", "", "", "VD"),
				},
				"USCHI",
				"SEA",
				"EXP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #14",
				new[]
				{
					("QSH", "ALL", "IMP", "", "", "VL"),
					("QSH", "ALL", "EXP", "", "", "VD"),
				},
				null,
				"SEA",
				"DOM",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #15",
				new[]
				{
					("QSH", "ALL", "ALL", "", "", "VL"),
					("QSH", "ALL", "EXP", "", "", "VD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);

			// Test Transport Mode
			AssertGetViaForQuotedBooking
			(
				"Case #16",
				new[]
				{
					("QSH", "SEA", "ALL", "", "", "VL"),
					("QSH", "AIR", "ALL", "", "", "VD"),
				},
				"AUMEL",
				"SEA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #17",
				new[]
				{
					("QSH", "SEA", "ALL", "", "", "VL"),
					("QSH", "AIR", "ALL", "", "", "VD"),
				},
				"USCHI",
				"AIR",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #18",
				new[]
				{
					("QSH", "SEA", "ALL", "", "", "VL"),
					("QSH", "AIR", "ALL", "", "", "VD"),
				},
				null,
				"ROA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
			AssertGetViaForQuotedBooking
			(
				"Case #19",
				new[]
				{
					("QSH", "ALL", "ALL", "", "", "VL"),
					("QSH", "AIR", "ALL", "", "", "VD"),
				},
				"AUMEL",
				"ROA",
				"IMP",
				"AUSYD",
				"USLAX",
				"AUMEL",
				"USCHI"
			);
		}

		public void AssertGetViaForForwardingConsol
			(
				string caseMessage,
				(string jobType, string mode, string direction, string origin, string destination, string via)[] viaConfigurations,
				string expectedVia,
				string transportMode,
				string direction,
				string origin,
				string destination,
				string voyageLoad = null,
				string voyageDischarge = null,
				string lastModeRouteSetDischarge = null
			)
		{
			var registryItem = (AutoratingViaPortRegistryItem)GetNewRegistryItem();

			var collection = new AutoratingViaPortConfigurationCollection(null, null);

			foreach (var settings in viaConfigurations.GroupBy(x => (x.jobType, x.mode)))
			{
				var configuration = collection.AddNew();
				configuration.JobType = settings.Key.jobType;
				configuration.TransportMode = settings.Key.mode;

				foreach (var item in settings)
				{
					var setting = configuration.Settings.AddNew();
					setting.Direction = item.direction;
					setting.OriginSourceOption = item.origin;
					setting.DestinationSourceOption = item.destination;
					setting.ViaSourceOption = item.via;
				}
			}
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var actualVia = registryItem.GetViaForForwardingConsol
					(
						transportMode: transportMode,
						direction: direction,
						origin: origin,
						destination: destination,
						voyageLoad: voyageLoad,
						voyageDischarge: voyageDischarge,
						lastModeRouteSetDischarge: lastModeRouteSetDischarge
					);

				AssertEquals(caseMessage, expectedVia, actualVia);
			}
		}

		public void AssertGetViaForShipment
			(
				string caseMessage,
				(string jobType, string mode, string direction, string origin, string destination, string via)[] viaConfigurations,
				string expectedVia,
				string transportMode,
				string direction,
				string origin,
				string destination,
				string firstLoad = null,
				string lastDischarge = null,
				string lastModeRouteSetDischarge = null
			)
		{
			var registryItem = (AutoratingViaPortRegistryItem)GetNewRegistryItem();

			var collection = new AutoratingViaPortConfigurationCollection(null, null);

			foreach (var settings in viaConfigurations.GroupBy(x => (x.jobType, x.mode)))
			{
				var configuration = collection.AddNew();
				configuration.JobType = settings.Key.jobType;
				configuration.TransportMode = settings.Key.mode;

				foreach (var item in settings)
				{
					var setting = configuration.Settings.AddNew();
					setting.Direction = item.direction;
					setting.OriginSourceOption = item.origin;
					setting.DestinationSourceOption = item.destination;
					setting.ViaSourceOption = item.via;
				}
			}
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var actualVia = registryItem.GetViaForShipment
					(
						transportMode: transportMode,
						direction: direction,
						origin: origin,
						destination: destination,
						firstLoad: firstLoad,
						lastDischarge: lastDischarge,
						lastModeRouteSetDischarge: lastModeRouteSetDischarge
					);

				AssertEquals(caseMessage, expectedVia, actualVia);
			}
		}

		public void AssertGetViaForQuotedBooking
			(
				string caseMessage,
				(string jobType, string mode, string direction, string origin, string destination, string via)[] viaConfigurations,
				string expectedVia,
				string transportMode,
				string direction,
				string origin,
				string destination,
				string voyageLoad = null,
				string voyageDischarge = null
			)
		{
			var registryItem = (AutoratingViaPortRegistryItem)GetNewRegistryItem();

			var collection = new AutoratingViaPortConfigurationCollection(null, null);

			foreach (var settings in viaConfigurations.GroupBy(x => (x.jobType, x.mode)))
			{
				var configuration = collection.AddNew();
				configuration.JobType = settings.Key.jobType;
				configuration.TransportMode = settings.Key.mode;

				foreach (var item in settings)
				{
					var setting = configuration.Settings.AddNew();
					setting.Direction = item.direction;
					setting.OriginSourceOption = item.origin;
					setting.DestinationSourceOption = item.destination;
					setting.ViaSourceOption = item.via;
				}
			}
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var actualVia = registryItem.GetViaForQuotedBooking
					(
						transportMode: transportMode,
						direction: direction,
						origin: origin,
						destination: destination,
						voyageLoad: voyageLoad,
						voyageDischarge: voyageDischarge
					);

				AssertEquals(caseMessage, expectedVia, actualVia);
			}
		}
	}
}
