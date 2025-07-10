using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMROceanBillSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniser()
		{
			var consol = CreateFCLConsol();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBillSynchroniser = new CMROceanBillSynchroniser(oceanBill, consol, seaCargoSynchroniser);
			oceanBillSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			var newShipment1 = consol.Shipments.AddNew();
			AssertEquals("OceanBill HouseBill Count", 1, oceanBill.HouseBills.Count);
			AssertEquals("Default values from shipment", newShipment1.PK, oceanBill.HouseBills[0].CA_JS);
			var newShipment2 = consol.Shipments.AddNew();
			AssertEquals("OceanBill HouseBill Count", 2, oceanBill.HouseBills.Count);
			AssertEquals("Default values from shipment", newShipment2.PK, oceanBill.HouseBills[1].CA_JS);
			consol.Shipments.Remove(newShipment1);
			AssertEquals("OceanBill HouseBill Count", 1, oceanBill.HouseBills.Count);
		}

		public void TestSynchroniseShippingLine()
		{
			var consol = CreateFCLConsol();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBillSynchroniser = new CMROceanBillSynchroniser(oceanBill, consol, seaCargoSynchroniser);
			oceanBillSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			var shippingLine1 = CreateShippingLine("A Shipping Line");
			var shippingLine2 = CreateShippingLine("Another Line");
			consol.SetDefaultShippingLineAddress(shippingLine1);
			AssertEquals("Shipping Line", shippingLine1.PK, oceanBill.CB_OH_ShippingLine);
			consol.SetDefaultShippingLineAddress(shippingLine2);
			AssertEquals("Shipping Line", shippingLine2.PK, oceanBill.CB_OH_ShippingLine);
		}

		#region Implementation

		OrgHeader CreateShippingLine(ZString lineName)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = lineName;
			result.OH_IsShippingLine = true;
			result.MainAddress.OA_Address1 = lineName + "Address 1";
			result.OH_RL_NKClosestPort = "AUSYD";
			return result;
		}

		#endregion
	}
}
