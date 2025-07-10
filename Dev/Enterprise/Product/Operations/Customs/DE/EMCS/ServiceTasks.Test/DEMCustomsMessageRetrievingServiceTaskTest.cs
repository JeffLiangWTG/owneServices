using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.EMCS.Business;
using Enterprise.Customs.DE.ServiceTasks;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.ServiceTasks.Testing
{
	[TestedType(typeof(DEMCustomsMessageRetrievingServiceTask))]
	public class DEMCustomsMessageRetrievingServiceTaskTest : GMDCustomsMessagingServiceTest<DEMCustomsMessageRetrievingServiceTask>
	{
		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("DEV", "DE EMCS Customs Message Retrieving", "DEC");
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEMCustomsMessageRetrievingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEMMessageRetrieving,
				ServiceTaskApplicationCodeList.Descriptions.DEMMessageRetrieving,
				"DEC",
				typeof(DEMCustomsMessageRetrievingServiceTask),
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
						ServiceTaskApplicationCodeList.Descriptions.DEMMessageRetrieving,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsEmcsSystem,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, GMDCustomsMessagingServiceTestHelperData testData, DEMCustomsMessageRetrievingServiceTask serviceTask)
		{
			var interchange = factory.Load<EDIInterchange>(testData.InterchangePK);
			CombineAssertions(() =>
			{
				AssertEquals("EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);
				var message = interchange.ContainedMessages[0];
				AssertEquals("message.EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsEmcsSystem, message.EM_ApplicationCode);
				AssertEquals("message.EM_ApplicationReference", randomEmcsKey, message.EM_ApplicationReference);
				AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageNum", interchange.EI_InterchangeNum, message.EM_MessageNum);
				AssertEquals("message.EM_MessageType", DE.Messaging.EDIMessageTypeList.Codes.EMCS, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", Messaging.EmcsMessageSubTypeList.Codes.Eme, message.EM_MessageSubType);
				AssertEquals("message.EM_MessageText", interchange.EI_BodyText, message.EM_MessageText);
				AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			});
		}

		protected override DEMCustomsMessageRetrievingServiceTask CreateServiceTask() => new DEMCustomsMessageRetrievingServiceTask();

		protected override GMDCustomsMessagingServiceTestHelperData SetupDataForTesting()
		{
			var interchange = CreateInterchange();
			return new GMDCustomsMessagingServiceTestHelperData()
			{
				InterchangePK = interchange.PK
			};
		}

		EDIInterchange CreateInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.DECustomsEmcsSystem;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "DEMEMCS";
			interchange.EI_To = "KDSER";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_BodyText = $@"
<DECustomsData>
	<LogbookTime>2019-11-20T15:29:00.2347048+02:00</LogbookTime>
	<CustomsData>
		<{randomEmcsKey}>
			<Header>
				<Preparation>
					<Date>2019-02-25</Date>
					<Time>16:00:00</Time>
				</Preparation>
				<InterchangeControlReference>0000375302</InterchangeControlReference>
				<MessageReferenceNumber>1</MessageReferenceNumber>
				<MessageIdentifier>0000375302</MessageIdentifier>
				<MessageGroup>EME</MessageGroup>
			</Header>
		</{randomEmcsKey}>
	</CustomsData>
</DECustomsData>";
			return interchange;
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			randomEmcsKey = EmcsResponseMessageDetails.Instance.ResponseMessages.Keys.First();
		}
		ZString randomEmcsKey;
	}
}
