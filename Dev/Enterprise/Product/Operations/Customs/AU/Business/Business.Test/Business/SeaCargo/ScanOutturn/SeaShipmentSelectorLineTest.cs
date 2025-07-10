using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaShipmentSelectorLine))]
	sealed class SeaShipmentSelectorLineTest : ShipmentSelectorLineTest
	{
		public void TestConstructor()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_MasterHouseBill = "MH123";
			var line = new SeaShipmentSelectorLine(oceanBill);
			AssertEquals("MH123", line.Shipment);
			AssertEquals(oceanBill, line.OceanBill);
			oceanBill.CB_ParentId = Factory.New<ForwardingConsol>().PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			line = new SeaShipmentSelectorLine(oceanBill);
			AssertEquals("All Standards", line.Shipment);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new SeaShipmentSelectorLine(Factory.New<CusSCAOceanBill>());
		}
	}
}
