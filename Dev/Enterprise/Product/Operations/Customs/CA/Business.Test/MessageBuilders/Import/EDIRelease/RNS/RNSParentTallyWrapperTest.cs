using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	[TestedType(typeof(RNSParentTallyWrapper))]
	sealed class RNSParentTallyWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetRNSRequestCollections()
		{
			var tally = Factory.New<TallyContainer>();
			var shipment1 = tally.PackUnpackShipments.AddNew();
			var entryNumber1 = shipment1.Numbers.AddNew();
			entryNumber1.CE_EntryNum = "111";
			entryNumber1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			var shipment2 = tally.PackUnpackShipments.AddNew();
			var entryNumber2 = shipment2.Numbers.AddNew();
			entryNumber2.CE_EntryNum = "222";
			entryNumber2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			var wrapper = RNSParentTallyWrapper.Load(tally);
			AssertSame("The ParentBusinessObject should be the tally", tally, wrapper.ParentBusinessObject);

			wrapper.IsStatusQuery = true;
			var rnsRequestCollections = wrapper.GetRNSRequestCollections();
			var rnsRequest1 = rnsRequestCollections.First(rnsRequest => rnsRequest.TopLevelBusinessObject == shipment1);
			var rnsRequest2 = rnsRequestCollections.First(rnsRequest => rnsRequest.TopLevelBusinessObject == shipment2);

			AssertNotNull("A RNSMessagingBO should been created for shipment1", (RNSMessagingBO)rnsRequest1);
			AssertEquals("The CargoControlNumber should be the CCN of shipment1", "111", rnsRequest1.CargoControlNumber);
			AssertNotNull("A RNSMessagingBO should been created for shipment2", (RNSMessagingBO)rnsRequest2);
			AssertEquals("The CargoControlNumber should be the CCN of shipment2", "222", rnsRequest2.CargoControlNumber);

			wrapper.IsStatusQuery = false;
			rnsRequestCollections = wrapper.GetRNSRequestCollections();
			rnsRequest1 = rnsRequestCollections.First(rnsRequest => rnsRequest.TopLevelBusinessObject == shipment1);
			rnsRequest2 = rnsRequestCollections.First(rnsRequest => rnsRequest.TopLevelBusinessObject == shipment2);

			AssertNotNull("A RNSRequestBO should been created for shipment1", (RNSRequestBO)rnsRequest1);
			AssertEquals("The CargoControlNumber should be the CCN of shipment1", "111", rnsRequest1.CargoControlNumber);
			AssertNotNull("A RNSRequestBO should been created for shipment2", (RNSRequestBO)rnsRequest2);
			AssertEquals("The CargoControlNumber should be the CCN of shipment2", "222", rnsRequest2.CargoControlNumber);
		}

		public void TestShouldDefaultChooseToSent()
		{
			var tally = Factory.New<TallyContainer>();
			var shipment1 = tally.PackUnpackShipments.AddNew();

			AssertEquals("Precodition: Arrival Certification status is Not Sent", MessageStatusList.Codes.NotSent, shipment1.ArrivalCertificationStatusCode);

			var shipment2 = tally.PackUnpackShipments.AddNew();
			CFSShipmentRNSStatusProviderTest.AddREJMessages(shipment2);

			AssertEquals("Precodition: Arrival Certification status is Rejected", EDIMessage.Status.Rejected, shipment2.ArrivalCertificationStatusCode);

			var shipment3 = tally.PackUnpackShipments.AddNew();
			CFSShipmentRNSStatusProviderTest.AddSNTMessages(shipment3);
			AssertEquals("Precodition: Arrival Certification status is Sent", EDIMessage.Status.Sent, shipment3.ArrivalCertificationStatusCode);

			var packLine31 = shipment3.OuterPackLines.AddNew();
			packLine31.JL_Outturn = 10;

			var shipment4 = tally.PackUnpackShipments.AddNew();
			AssertEquals("Precodition: Arrival Certification status is Not Sent", MessageStatusList.Codes.NotSent, shipment4.ArrivalCertificationStatusCode);

			var packLine41 = shipment4.OuterPackLines.AddNew();
			packLine41.JL_Outturn = 0;

			var shipment5 = tally.PackUnpackShipments.AddNew();
			CFSShipmentRNSStatusProviderTest.AddREJMessages(shipment5);
			AssertEquals("Precodition: Arrival Certification status is Rejected", EDIMessage.Status.Rejected, shipment5.ArrivalCertificationStatusCode);

			var packLine51 = shipment5.OuterPackLines.AddNew();
			packLine51.JL_Outturn = 0;
			var packLine52 = shipment5.OuterPackLines.AddNew();
			packLine52.JL_Outturn = 10;

			var wrapper = RNSParentTallyWrapper.Load(tally);
			wrapper.IsStatusQuery = true;

			foreach (var rnsRequest in wrapper.GetRNSRequestCollections())
			{
				var singleManager = new RNSMessageManager(rnsRequest, wrapper.Notification, wrapper.IsStatusQuery);
				Assert("Send RNS Status Request", !wrapper.ShouldDefaultChooseToSent(singleManager));
			}

			wrapper.IsStatusQuery = false;
			foreach (var rnsRequest in wrapper.GetRNSRequestCollections())
			{
				var singleManager = new RNSMessageManager(rnsRequest, wrapper.Notification, wrapper.IsStatusQuery);
				bool shouldDefaultChooseToSent = wrapper.ShouldDefaultChooseToSent(singleManager);

				if (rnsRequest.TopLevelBusinessObject == shipment1)
				{
					Assert("Shipment1 should not be choosed to sent", !shouldDefaultChooseToSent);
				}
				if (rnsRequest.TopLevelBusinessObject == shipment2)
				{
					Assert("Shipment2 should not be choosed to sent", !shouldDefaultChooseToSent);
				}
				if (rnsRequest.TopLevelBusinessObject == shipment3)
				{
					Assert("Shipment3 should not be choosed to sent", !shouldDefaultChooseToSent);
				}
				if (rnsRequest.TopLevelBusinessObject == shipment4)
				{
					Assert("Shipment4 should not be choosed to sent", !shouldDefaultChooseToSent);
				}
				if (rnsRequest.TopLevelBusinessObject == shipment5)
				{
					Assert("Shipment5 should be choosed to sent", shouldDefaultChooseToSent);
				}
			}
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return RNSParentTallyWrapper.Load(Factory.New<TallyContainer>());
		}

		#endregion
	}
}
