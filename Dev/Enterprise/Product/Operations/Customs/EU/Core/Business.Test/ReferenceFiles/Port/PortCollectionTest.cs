using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(PortCollection))]
	class PortCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PortCollection>
	{
		public void TestLoad()
		{
			PortTest.CreatePort(Factory, "GB", "BLE", "Agility Logistics Ltd", portType: "SEA");
			PortTest.CreatePort(Factory, "GB", "LLA", "LLANDDULAS", portType: "ICD");
			PortTest.CreatePort(Factory, "GB", "FZO", "FILTON AERODROME", portType: "DES");
			PortTest.CreatePort(Factory, "GB", "LDC", "North West Collector’s Office", portType: "COL");
			PortTest.CreatePort(Factory, "GB", "MLS", "RAF MOLESHILL", portType: "MIL");
			PortTest.CreatePort(Factory, "GB", "FZY", "Port of Tilbury Free Zone", portType: "FZN");
			PortTest.CreatePort(Factory, "GB", "MPD", "MPD Mount Pleasant Depot", portType: "MAI");
			PortTest.CreatePort(Factory, "GB", "GLO", "GLOUCESTER (STAVERTON) AIRPORT	COA", portType: "COA");
			Factory.Save();

			var portCollection = new PortCollection(Factory, "GB", TransportTypeList.Codes.Air);
			AssertEquals("PortCollection.Count", 3, portCollection.Count);
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "FZO"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "MLS"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "GLO"));

			portCollection = new PortCollection(Factory, "GB", TransportTypeList.Codes.Sea);
			AssertEquals("PortCollection.Count", 4, portCollection.Count);
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "BLE"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "LDC"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "FZY"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "LLA"));

			portCollection = new PortCollection(Factory, "GB", TransportTypeList.Codes.Road);
			AssertEquals("PortCollection.Count", 4, portCollection.Count);
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "LDC"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "FZY"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "LLA"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "MPD"));

			portCollection = new PortCollection(Factory, "GB", TransportTypeList.Codes.Rail);
			AssertEquals("PortCollection.Count", 4, portCollection.Count);
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "LDC"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "FZY"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "LLA"));
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "MPD"));

			portCollection = new PortCollection(Factory, "GB", TransportTypeList.Codes.Mail);
			AssertEquals("PortCollection.Count", 1, portCollection.Count);
			AssertNotNull(portCollection.Cast<Port>().FirstOrDefault(x => x.Code == "MPD"));

			portCollection = new PortCollection(Factory, "GB", ZString.Empty);
			AssertEquals("PortCollection.Count", 8, portCollection.Count);

			portCollection = new PortCollection(Factory, "GB", "XXX");
			AssertEquals("PortCollection.Count", 8, portCollection.Count);
		}

		protected override PortCollection GetCollectionToTest()
		{
			return new PortCollection(Factory, "GB", "AIR");
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Port(Factory.New<ZZRefCusCodeListCombined>());
		}
	}
}
