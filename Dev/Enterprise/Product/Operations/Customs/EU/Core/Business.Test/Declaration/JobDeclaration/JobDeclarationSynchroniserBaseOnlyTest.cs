using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobDeclarationSynchroniserBaseOnlyTest : JobDeclarationSynchroniserTest
	{
		public void TestJE_RL_NKPortOfLoadingSynchronisation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "LVRIX";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports.RemoveAndDeleteAll();

			var transport1 = consol.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "SGSIN";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "LVRIX";

			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "LVRIX";

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgHeader.PK;

			var createDeclarationHelper = new Customs.Business.CreateDeclarationHelper();
			using var mutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK);

			var declaration = createDeclarationHelper.CreateDeclaration(shipment, mutex, () => null);

			AssertEquals("First Load Port from consol when import", "HKHKG", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("IATA Load Port from JE_RL_NKPortOfLoading", "HKG", declaration.JE_IATALoadPort);
		}
	}
}
