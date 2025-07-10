using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class JobDeclarationSynchroniserTest : TestCaseWithFactory
	{
		//Some FieldSynchronisers update fields after DE BusinessLogic already set them appropriate
		//E.g. ShipmentSynchroniser: Shipment's Consignee => Importer and OrgProxy => Declarant.
		//In the meantime DE BusinessLogic already updated the Declarant with Importer but it gets overridden again afterwards by a Synchroniser.
		//So OnSynchronised() triggers UpdateAddressesDependingOnDeclarantType() to ensure correct address populattion, in this example it updates the Declarant with the Importer
		public void TestOnSynchronisedTriggersUpdateAddressesDependingOnDeclarantType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentConsignee = Factory.New<OrgHeader>();
			shipment.JS_HouseBill = "HB12312";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = shipmentConsignee.MainAddress.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Shipment's Consignee => Declaration's Importer => Declaration's Declarant", shipmentConsignee.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		}

		public void TestOnSynchronisedTriggersUpdateFinalDestinationPortFromImporter()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentConsignee = Factory.New<OrgHeader>();
			shipmentConsignee.OH_RL_NKClosestPort = "DEBER";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = shipmentConsignee.MainAddress.PK;
			shipment.JS_RL_NKDestination = "DEHAM";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("DEBER", declaration.JE_RL_NKFinalDestination);
		}

		public void TestOnSynchronisedStopDoesNotTriggerUpdateFinalDestinationPortFromImporter()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentConsignee = Factory.New<OrgHeader>();
			shipmentConsignee.OH_RL_NKClosestPort = "DEBER";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = shipmentConsignee.MainAddress.PK;
			shipment.JS_RL_NKDestination = "DEHAM";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "DECGN";

			// this replicates the behaviour of the EDIMessages collection, which calls Synchronise with SynchroniseAction.Stop
			// that disables the synchroniser, but calls OnSynchronised. With the additional check for enabled this is now prevented.
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Stop));

			AssertEquals("DECGN", declaration.JE_RL_NKFinalDestination);
		}

		public void TestOnSynchronisedTriggersUpdateDeclarantAddress()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentConsignor = Factory.New<OrgHeader>();
			shipmentConsignor.OH_IsConsignor = true;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipmentConsignor.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(shipmentConsignor.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		}

		public void TestSetRepresentativeInDifferentBranchContext()
		{
			var branch1 = Factory.NewCompanyAndBranchWith("ABC", "BR1", "DE");
			var branch1OrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch1.GB_OH_OrgProxy = branch1OrgProxy.PK;

			var branch2 = Factory.NewCompanyAndBranchWith("DEF", "BR2", "DE");
			var branch2OrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch2.GB_OH_OrgProxy = branch2OrgProxy.PK;

			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			ForwardingShipment shipment;
			JobDeclaration declaration;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				shipment = Factory.New<ForwardingShipment>();
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			}

			declaration.JE_OA_Representative = otherOrg.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			}

			CombineAssertions(() =>
			{
				AssertEquals(
					"Representative should not defaulted when the JE_OA_Representative value is non-empty",
					declaration.JE_OA_Representative, otherOrg.PK);

				declaration.JE_OA_Representative = ZGuid.Empty;

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				}

				AssertEquals(
					"Representative should be defaulted to the OrgProxy of the declaration's branch regardless of the context in which the synchroniser is called and Only if the representative value is empty",
					branch1.OrgProxy.MainAddress.PK, declaration.JE_OA_Representative);
			});
		}
	}
}
