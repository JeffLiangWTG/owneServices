using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing
{
	sealed class JobDeclarationSynchroniserTest : TestCaseWithFactory
	{
		public void TestOnSynchronisedTriggersUpdateAddressesDependingOnDeclarantType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentConsignor = Factory.New<OrgHeader>();
			shipment.JS_HouseBill = "HB12312";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipmentConsignor.MainAddress.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Shipment's Consignor => Declaration's Supplier => Declaration's Declarant", shipmentConsignor.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		}
	}
}
