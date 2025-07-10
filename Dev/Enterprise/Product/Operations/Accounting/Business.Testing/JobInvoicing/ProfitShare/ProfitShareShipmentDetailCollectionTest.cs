using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	[TestedType(typeof(ProfitShareShipmentDetailCollection))]
	public class ProfitShareShipmentDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProfitShareShipmentDetailCollection>
	{
		public void TestContainsShipmentAndProfitShareCharges_ProfitShareCreateChargesPerCurrencyRegistryOff()
		{
			AssertContainsShipmentAndProfitShareCharges(false);
		}

		public void TestContainsShipmentAndProfitShareCharges_ProfitShareCreateChargesPerCurrencyRegistryOn()
		{
			AssertContainsShipmentAndProfitShareCharges(true);
		}

		void AssertContainsShipmentAndProfitShareCharges(bool isRegistryOn)
		{
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isRegistryOn);

			var creator = new TestObjectCreator(Factory);

			var shipment1 = creator.CreateShipment("S0001", true);
			var shipment2 = creator.CreateShipment("S0002", true);

			var job1 = creator.CreateJob(shipment1);
			var job2 = creator.CreateJob(shipment2);

			var job1Charge = job1.Charges.AddNew();
			job1Charge.JR_AC = creator.CC1.PK;
			job1Charge.JR_OSCostAmt = 10m;
			job1Charge.JR_OSSellAmt = 10m;
			job1Charge.JR_IsIncludedInProfitShare = false;

			var job2Charge = job2.Charges.AddNew();
			job2Charge.JR_AC = creator.CC1.PK;
			job2Charge.JR_OSCostAmt = 10m;
			job2Charge.JR_OSSellAmt = 10m;
			job2Charge.JR_IsIncludedInProfitShare = false;

			Factory.Save();

			ProfitShareShipmentDetailCollection shipmentDetails = new ProfitShareShipmentDetailCollection(Factory);

			AssertEquals("Precondition: Does not contain any shipments", false, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment1.PK));
			AssertEquals("Precondition: Does not contain any shipments", false, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment2.PK));

			var overseasAgent = creator.CreateOrgHeader("MAOEWR", true, true);

			var agentRelationship = creator.CreateAgentRelationship(overseasAgent, GlbCompany.CurrentCompany.OrgProxy);
			var profitShareAgreement = creator.CreateProfitShare(agentRelationship, 50m, 50, "USLAX", "AUSYD", "ALL");
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(30);

			var profitShareShipmentDetail1 = new ProfitShareShipmentDetail(shipment1, null,
				OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory)
			{
				ProfitShareAgreement = profitShareAgreement
			};
			shipmentDetails.Add(profitShareShipmentDetail1);

			AssertEquals(false, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment1.PK));
			AssertEquals(false, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment2.PK));

			job1Charge.JR_IsIncludedInProfitShare = true;
			Factory.Save();

			AssertEquals(true, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment1.PK));
			AssertEquals(false, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment2.PK));

			var profitShareShipmentDetail2 = new ProfitShareShipmentDetail(shipment2, null,
				OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory)
			{
				ProfitShareAgreement = profitShareAgreement
			};
			shipmentDetails.Add(profitShareShipmentDetail2);

			AssertEquals(true, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment1.PK));
			AssertEquals(false, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment2.PK));

			job2Charge.JR_IsIncludedInProfitShare = true;
			Factory.Save();

			AssertEquals(true, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment1.PK));
			AssertEquals(true, shipmentDetails.ContainsShipmentAndProfitShareCharges(shipment2.PK));
		}

		#region Implementation

		protected override ProfitShareShipmentDetailCollection GetCollectionToTest()
		{
			return new ProfitShareShipmentDetailCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProfitShareShipmentDetail(Factory.New<ForwardingShipment>(), Factory.New<OrgHeader>(), OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
		}

		#endregion
	}
}
