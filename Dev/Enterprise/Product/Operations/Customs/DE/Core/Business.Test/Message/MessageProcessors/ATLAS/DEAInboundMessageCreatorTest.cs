using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DEAInboundMessageCreatorTest : TestCaseWithFactory
	{
		public void TestInvalidCombination()
		{
			var interchange = CreateInterchange(nameof(ATLASVersion10_1.DEERRF), "xxx");
			creator.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, interchange.ContainedMessages.Count);
				AssertEquals("EM_MessageType", EDIInterchange.ApplicationCodes.Unknown, (interchange.ContainedMessages[0]).EM_MessageType);
			});
		}

		public void TestCollectiveNCTSVersion()
		{
			var interchange = CreateInterchange(nameof(ATLASVersion10_1.DEERRF), NctsMessageSubTypeList.Codes.DepartureMessage);
			creator.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, interchange.ContainedMessages.Count);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.NCTS, (interchange.ContainedMessages[0]).EM_MessageType);
			});
		}

		public void TestSpecificNCTSVersion()
		{
			var interchange = CreateInterchangeForNcts(nameof(ATLASVersion10_1.DETQSC), NctsMessageSubTypeList.Codes.StatusRequestMessage);
			creator.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, interchange.ContainedMessages.Count);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.NCTS, (interchange.ContainedMessages[0]).EM_MessageType);
			});
		}

		public void TestCollectiveTemporaryStorage()
		{
			var interchange = CreateInterchange(nameof(ATLASVersion10_1.GCRECF), TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration);
			creator.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, interchange.ContainedMessages.Count);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.TemporaryStorage, (interchange.ContainedMessages[0]).EM_MessageType);
			});
		}

		public void TestSpecificTemporaryStorage()
		{
			var interchange = CreateInterchange(nameof(ATLASVersion10_1.SCCANE), TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration);
			creator.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, interchange.ContainedMessages.Count);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.TemporaryStorage, (interchange.ContainedMessages[0]).EM_MessageType);
			});
		}

		public void TestSpecificImport()
		{
			var interchange = CreateInterchange(nameof(ATLASVersion10_1.FCREVH), ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration);
			creator.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, interchange.ContainedMessages.Count);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.Import, (interchange.ContainedMessages[0]).EM_MessageType);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			creator = new DEAInboundMessageCreator();
		}

		EDIInterchange CreateInterchangeForNcts(ZString messageTechnicalName, ZString messageSubType)
		{
			return CreateInterchange(() =>
			{
				return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DECustomsData>
	<LogbookTime>2019-11-20T15:29:00.2347048+02:00</LogbookTime>
	<CustomsData>
		<{messageTechnicalName}>
			<preparationDateAndTime>2019-02-25T16:00:00</preparationDateAndTime>
			<messageIdentification>CUSTST58750000000375302250219160050</messageIdentification>
			<messageGroup>{messageSubType}</messageGroup>
			<messageType>{messageTechnicalName}</messageType>
		</{messageTechnicalName}>
	</CustomsData>
</DECustomsData>";
			});
		}

		EDIInterchange CreateInterchange(ZString messageTechnicalName, ZString messageSubType)
		{
			return CreateInterchange(() =>
			{
				return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DECustomsData>
	<LogbookTime>2019-11-20T15:29:00.2347048+02:00</LogbookTime>
	<CustomsData>
		<{messageTechnicalName}>
			<MetaData>
				<Preparation>
					<Date>2019-02-25</Date>
					<Time>16:00:00</Time>
				</Preparation>
				<InterchangeControlReference>0000000375302</InterchangeControlReference>
				<MessageReferenceNumber>1</MessageReferenceNumber>
				<MessageIdentifier>CUSTST58750000000375302250219160050</MessageIdentifier>
				<MessageGroup>{messageSubType}</MessageGroup>
				<MessageType>{messageTechnicalName}</MessageType>
			</MetaData>
		</{messageTechnicalName}>
	</CustomsData>
</DECustomsData>";
			});
		}

		EDIInterchange CreateInterchange(Func<ZString> createInterchangeBodyText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.DECustomsAtlasSystem;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "DEATLAS";
			interchange.EI_To = "KDSER";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_BodyText = createInterchangeBodyText.Invoke();
			return interchange;
		}
		DEAInboundMessageCreator creator;
	}
}
