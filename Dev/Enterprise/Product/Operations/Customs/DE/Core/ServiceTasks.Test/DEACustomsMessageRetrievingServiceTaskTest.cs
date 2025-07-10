using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	[TestedType(typeof(DEACustomsMessageRetrievingServiceTask))]
	class DEACustomsMessageRetrieverServiceTaskTest : GMDCustomsMessagingServiceTest<DEACustomsMessageRetrievingServiceTask>
	{
		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("DET", "DE ATLAS Customs Message Retrieving", "DEC");
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEACustomsMessageRetrievingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEAMessageRetrieving,
				ServiceTaskApplicationCodeList.Descriptions.DEAMessageRetrieving,
				"DEC",
				typeof(DEACustomsMessageRetrievingServiceTask),
				"60Seconds",
				Core.Constants.CountryCodes.Germany,
				true);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.DEAMessageRetrieving,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAtlasSystem,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, GMDCustomsMessagingServiceTestHelperData testData, DEACustomsMessageRetrievingServiceTask serviceTask)
		{
			var interchange = factory.Load<EDIInterchange>(testData.InterchangePK);
			CombineAssertions(() =>
			{
				AssertEquals("EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);
				AssertMessage(interchange.ContainedMessages[0], messageTypeKey, Messaging.EDIMessageTypeList.Codes.TemporaryStorage, interchange.EI_BodyText, interchange.EI_InterchangeNum);
			});
		}

		protected override DEACustomsMessageRetrievingServiceTask CreateServiceTask() => new DEACustomsMessageRetrievingServiceTask();

		protected override GMDCustomsMessagingServiceTestHelperData SetupDataForTesting()
		{
			var interchange = CreateInterchange();
			return new GMDCustomsMessagingServiceTestHelperData()
			{
				InterchangePK = interchange.PK
			};
		}

		void AssertMessage(EDIMessage message, ZString aplicationReference, ZString messageType, ZString bodyText, ZString messageNum)
		{
			AssertEquals("message.EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, message.EM_ApplicationCode);
			AssertEquals("message.EM_ApplicationReference", aplicationReference, message.EM_ApplicationReference);
			AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageNum", messageNum, message.EM_MessageNum);
			AssertEquals("message.EM_MessageType", messageType, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", "SVM", message.EM_MessageSubType);
			AssertEquals("message.EM_MessageText", bodyText, message.EM_MessageText);
			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		}

		EDIInterchange CreateInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.DECustomsAtlasSystem;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "DEATLAS";
			interchange.EI_To = "KDSER";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_BodyText = $@"
<DECustomsData>
	<LogbookTime>2019-11-20T15:29:00.2347048+02:00</LogbookTime>
	<CustomsData>
		<{messageTypeKey}>
			<MetaData>
				<Preparation>
					<Date>2019-02-25</Date>
					<Time>16:00:00</Time>
				</Preparation>
				<InterchangeControlReference>0000000375302</InterchangeControlReference>
				<MessageReferenceNumber>1</MessageReferenceNumber>
				<MessageIdentifier>CUSTST58750000000375302250219160050</MessageIdentifier>
				<MessageGroup>SvM</MessageGroup>
				<MessageType>{messageTypeKey}</MessageType>
			</MetaData>
		</{messageTypeKey}>
	</CustomsData>
</DECustomsData>";
			return interchange;
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			messageTypeKey = nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LECWIF);
		}
		ZString messageTypeKey;
	}
}
