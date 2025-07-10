using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC013C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC015C;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class OutboundEDIMessageBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetDataProvider_Empty()
		{
			var testMessage = Factory.New<AISOutboundEDIMessage>();
			Cc015CType result = default;
			AssertNoExceptionThrown("Should not throw exception with empty xml.", () => result = testMessage.GetDataProvider<Cc015CType>());
			AssertNull("Should get an empty result.", result);
		}

		public void TestResetToQueuedStatus()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.Sent;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			interchange.Logs.AddNew(Events.EditedARecord, $"Status changed to {EDIInterchange.Status.eHubQueued} by eHub");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var testMessage = Factory.New<AISOutboundEDIMessage>();
			testMessage.EM_EI = interchange.PK;
			testMessage.EM_Status = EDIMessage.Status.Sent;

			CombineAssertions("ResetToQueuedStatus should not effect EM_Status and EI_Status.", () =>
			{
				AssertSame("Make sure Message is attached to Interchange.", interchange, testMessage.Interchange);
				testMessage.ResetToQueuedStatus();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, testMessage.EM_Status);
				AssertEquals("EI_Status", EDIInterchange.Status.Sent, interchange.EI_Status);
			});
		}

		public void TestGetDataProvider_Invalid()
		{
			var testMessage = Factory.New<AISOutboundEDIMessage>();
			Cc015CType result = default;
			testMessage.EM_MessageText = "<Invalid></xml>";
			AssertNoExceptionThrown("Should not throw exception trying convert invalid text to message provider", () => result = testMessage.GetDataProvider<Cc015CType>());
			AssertNull("Should get an empty result.", result);
		}

		public void TestGetDataProvider_Unrecognized()
		{
			var testMessage = Factory.New<AISOutboundEDIMessage>();
			testMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8"" ?><Item><Code>CODE1</Code></Item>";
			Cc015CType result = default;
			AssertNoExceptionThrown("Should not throw exception trying convert unrecognized text to message provider", () => result = testMessage.GetDataProvider<Cc015CType>());
			AssertNull("Should get an empty result.", result);
		}

		public void TestGetDataProvider_InconsistentType()
		{
			var testMessage = Factory.New<AISOutboundEDIMessage>();
			testMessage.EM_MessageText =
@"<q1:CC015C xmlns:q1=""http://ncts.dgtaxud.ec"">
	<messageSender>_NCTSMessageSenderHolder_</messageSender>
	<messageRecipient>IE</messageRecipient>
	<preparationDateAndTime>2023-10-12T07:26:12</preparationDateAndTime>
	<messageIdentification>_NCTSMessageRecipientHolder_</messageIdentification>
	<messageType>CC015C</messageType>
	<TransitOperation>
	<LRN>NCTS__LRN_Holder</LRN>
	<declarationType>TIR</declarationType>
	<additionalDeclarationType>D</additionalDeclarationType>
	<security>1</security>
	<reducedDatasetIndicator>0</reducedDatasetIndicator>
	<communicationLanguageAtDeparture>IE</communicationLanguageAtDeparture>
	<bindingItinerary>0</bindingItinerary>
	</TransitOperation>
	<CustomsOfficeOfDeparture>
	<referenceNumber>AD000001</referenceNumber>
	</CustomsOfficeOfDeparture>
	<CustomsOfficeOfTransitDeclared>
	<sequenceNumber>1</sequenceNumber>
	<referenceNumber />
	<arrivalDateAndTimeEstimated>0001-01-01T00:00:00</arrivalDateAndTimeEstimated>
	</CustomsOfficeOfTransitDeclared>
	<Consignment>
	<containerIndicator>0</containerIndicator>
	<grossMass>1.000000</grossMass>
	<LocationOfGoods>
		<typeOfLocation />
		<qualifierOfIdentification />
	</LocationOfGoods>
	<ActiveBorderTransportMeans>
		<sequenceNumber>1</sequenceNumber>
	</ActiveBorderTransportMeans>
	<PlaceOfUnloading>
		<UNLocode>DE222</UNLocode>
	</PlaceOfUnloading>
	<HouseConsignment>
		<sequenceNumber>1</sequenceNumber>
		<grossMass>1.000000</grossMass>
		<ConsignmentItem>
		<goodsItemNumber>1</goodsItemNumber>
		<declarationGoodsItemNumber>1</declarationGoodsItemNumber>
		<Commodity>
			<CommodityCode>
			<harmonizedSystemSubHeadingCode />
			</CommodityCode>
			<GoodsMeasure>
			<grossMass>1.000000</grossMass>
			</GoodsMeasure>
		</Commodity>
		<PreviousDocument>
			<sequenceNumber>1</sequenceNumber>
			<type />
			<referenceNumber>1</referenceNumber>
			<numberOfPackages>1</numberOfPackages>
		</PreviousDocument>
		<PreviousDocument>
			<sequenceNumber>2</sequenceNumber>
			<type />
			<referenceNumber />
			<numberOfPackages>1</numberOfPackages>
		</PreviousDocument>
		</ConsignmentItem>
	</HouseConsignment>
	</Consignment>
</q1:CC015C>";
			Cc013CType result = default;
			AssertNoExceptionThrown("Should not throw exception trying convert CC015C text to CC013CType provider", () => result = testMessage.GetDataProvider<Cc013CType>());
			AssertNull("Should get an empty result.", result);
		}

		public void TestGetDataProvider_Correct()
		{
			var testMessage = Factory.New<AISOutboundEDIMessage>();
			testMessage.EM_MessageText =
@"<q1:CC015C xmlns:q1=""http://ncts.dgtaxud.ec"">
	<messageSender>_NCTSMessageSenderHolder_</messageSender>
	<messageRecipient>IE</messageRecipient>
	<preparationDateAndTime>2023-10-12T07:26:12</preparationDateAndTime>
	<messageIdentification>_NCTSMessageRecipientHolder_</messageIdentification>
	<messageType>CC015C</messageType>
	<TransitOperation>
	<LRN>NCTS__LRN_Holder</LRN>
	<declarationType>TIR</declarationType>
	<additionalDeclarationType>D</additionalDeclarationType>
	<security>1</security>
	<reducedDatasetIndicator>0</reducedDatasetIndicator>
	<communicationLanguageAtDeparture>IE</communicationLanguageAtDeparture>
	<bindingItinerary>0</bindingItinerary>
	</TransitOperation>
	<CustomsOfficeOfDeparture>
	<referenceNumber>AD000001</referenceNumber>
	</CustomsOfficeOfDeparture>
	<CustomsOfficeOfTransitDeclared>
	<sequenceNumber>1</sequenceNumber>
	<referenceNumber />
	<arrivalDateAndTimeEstimated>0001-01-01T00:00:00</arrivalDateAndTimeEstimated>
	</CustomsOfficeOfTransitDeclared>
	<Consignment>
		<containerIndicator>0</containerIndicator>
		<grossMass>1.000000</grossMass>
		<LocationOfGoods>
			<typeOfLocation />
			<qualifierOfIdentification />
		</LocationOfGoods>
		<ActiveBorderTransportMeans>
			<sequenceNumber>1</sequenceNumber>
		</ActiveBorderTransportMeans>
		<PlaceOfUnloading>
			<UNLocode>DE222</UNLocode>
		</PlaceOfUnloading>
		<HouseConsignment>
			<sequenceNumber>1</sequenceNumber>
			<grossMass>1.000000</grossMass>
			<ConsignmentItem>
			<goodsItemNumber>1</goodsItemNumber>
			<declarationGoodsItemNumber>1</declarationGoodsItemNumber>
			<Commodity>
				<CommodityCode>
				<harmonizedSystemSubHeadingCode />
				</CommodityCode>
				<GoodsMeasure>
				<grossMass>1.000000</grossMass>
				</GoodsMeasure>
			</Commodity>
			<PreviousDocument>
				<sequenceNumber>1</sequenceNumber>
				<type />
				<referenceNumber>1</referenceNumber>
				<numberOfPackages>1</numberOfPackages>
			</PreviousDocument>
			<PreviousDocument>
				<sequenceNumber>2</sequenceNumber>
				<type />
				<referenceNumber />
				<numberOfPackages>1</numberOfPackages>
			</PreviousDocument>
			</ConsignmentItem>
		</HouseConsignment>
	</Consignment>
</q1:CC015C>";
			Cc015CType result = default;
			AssertNotNull("Should get a valid empty result.", result = testMessage.GetDataProvider<Cc015CType>());
			AssertEquals("Consignment.GrossMass", 1m, result.Consignment.GrossMass);
			AssertEquals("Consignment.PlaceOfUnloading.UNLocode", "DE222", result.Consignment.PlaceOfUnloading.UnLocode);
		}
	}
}
