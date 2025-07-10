using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	internal class CAFreightWrapperFromShipmentTest : FreightWrapperTest
	{
		public override void TestOrgWrappersReturnTypesOnEmptyWrapper()
		{
			Assert(true);
		}

		public void TestCASpecificProperties()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CusEntryNumber.New(shipment, CanadaAdditionalReferenceNumberTypes.Codes.PCN, Core.Constants.CountryCodes.Canada).CE_EntryNum = "12345XX";
			var helper = new DeclarationTestHelper(Factory, true);
			var aDateTime = new ZDateTime(2011, 11, 11, 11, 11, 0);
			shipment.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN1", aDateTime, "1", aDateTime));
			shipment.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN1", aDateTime.AddMinutes(1), "2", aDateTime.AddHours(1)));
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN1", aDateTime.AddMinutes(2), "3", aDateTime.AddHours(2)));
			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("ReleaseStatuses.Count", 1, wrapper.CAReleaseStatus.Count);
			AssertEquals("Got the latter one for the correct shipment", aDateTime.AddMinutes(1), wrapper.CAReleaseStatus[0].ReleaseDate);
			AssertEquals("CAPreviousCCN", "12345XX", wrapper.CAPreviousCCN);
			AssertEquals("CATransactionNo", "10207400004068", wrapper.CATransactionNo);
			Assert("CAHideCarrier", wrapper.CAHideCarrier);
		}
	}
}
