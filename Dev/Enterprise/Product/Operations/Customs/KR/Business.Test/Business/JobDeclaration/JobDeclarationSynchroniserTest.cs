using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobDeclarationSynchroniserTest : BusinessObjectLookupsTestCase
	{
		public void TestGetCustomsUnitForThisPackType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var synchroniser = new JobDeclarationSynchroniser(declaration);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			shipment.JS_F3_NKPackType = "BAG";
			AssertEquals("If it exists in the CW1 code, the KR code is saved.", "BG", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "@@@";
			AssertEquals("If it not exists in the CW1 code, the KR code is saved.", "", declaration.JE_TotalNoOfPacksPackType);
		}
	}
}

