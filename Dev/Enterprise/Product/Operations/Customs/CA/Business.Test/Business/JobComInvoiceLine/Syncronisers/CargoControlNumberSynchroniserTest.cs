using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CargoControlNumberSynchroniserTest : TestCaseWithFactory
	{
		public void TestSynchroniser()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_HouseBill = "HS12345678";
			var ccn = shipment.Numbers.AddNew();
			ccn.CE_EntryNum = "CCN12345678";
			ccn.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			var shipment1 = shipment.CoLoadShipments.AddNew();
			shipment1.JS_HouseBill = "HS12345678A";
			var ccn1 = shipment1.Numbers.AddNew();
			ccn1.CE_EntryNum = "CCN12345678A";
			ccn1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			var releaseStatuses = declaration.ReleaseStatuses.Cast<ReleaseStatus>();
			AssertEquals(2, declaration.ReleaseStatuses.Count);
			AssertEquals("HS12345678", Factory.Load<Bill>(releaseStatuses.FirstOrDefault(x => x.RL_CargoControlNumber == "CCN12345678").RL_Bill)?.CU_BillNum);
			AssertEquals("HS12345678A", Factory.Load<Bill>(releaseStatuses.FirstOrDefault(x => x.RL_CargoControlNumber == "CCN12345678A").RL_Bill)?.CU_BillNum);

			var cargoControlNumbers = declaration.CargoControlNumbers;
			AssertEquals(2, cargoControlNumbers.Count);
			Assert(cargoControlNumbers.FirstOrDefault(x => x.CY_CargoControlNumber == "CCN12345678").CA_IsFromNumbersTab);
			Assert(!cargoControlNumbers.FirstOrDefault(x => x.CY_CargoControlNumber == "CCN12345678A").CA_IsFromNumbersTab);

			shipment.JS_HouseBill = "HS12345678C";
			AssertEquals("HS12345678C", Factory.Load<Bill>(releaseStatuses.FirstOrDefault(x => x.RL_CargoControlNumber == "CCN12345678").RL_Bill)?.CU_BillNum);

			ccn.CE_EntryNum = "CCN12345678C";
			AssertEquals("HS12345678C", Factory.Load<Bill>(releaseStatuses.FirstOrDefault(x => x.RL_CargoControlNumber == "CCN12345678C").RL_Bill)?.CU_BillNum);

			shipment1.JS_HouseBill = "HS12345678D";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("HS12345678D", Factory.Load<Bill>(releaseStatuses.FirstOrDefault(x => x.RL_CargoControlNumber == "CCN12345678A").RL_Bill)?.CU_BillNum);

			ccn1.CE_EntryNum = "CCN12345678D";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("HS12345678D", Factory.Load<Bill>(releaseStatuses.FirstOrDefault(x => x.RL_CargoControlNumber == "CCN12345678D").RL_Bill)?.CU_BillNum);

			ccn1.Delete();
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(1, cargoControlNumbers.Count);
			AssertEquals("HS12345678C", Factory.Load<Bill>(releaseStatuses.FirstOrDefault(x => x.RL_CargoControlNumber == "CCN12345678C").RL_Bill)?.CU_BillNum);
		}
	}
}
