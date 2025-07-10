using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DEAInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestInvalidInterchange()
		{
			var invalidInterchange = CreateInterchangeForAtlasVersion10_1("$#$#$", "$%$");
			Factory.Save();
			processor.ExecuteBatch();
			invalidInterchange.Reload();
			CombineAssertions(() =>
			{
				AssertNull(invalidInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
				AssertEquals("invalidInterchange.EI_Status", EDIInterchange.Status.Failed, invalidInterchange.EI_Status);
			});
		}

		public void TestNoDECustomsData()
		{
			var noDECustomsDataInterchange = CreateInterchangeForAtlasVersion10_1("", "");
			noDECustomsDataInterchange.EI_BodyText = ZString.Empty;
			Factory.Save();
			processor.ExecuteBatch();
			noDECustomsDataInterchange.Reload();
			CombineAssertions(() =>
			{
				AssertEquals("NO DE CUSTOMS DATA", noDECustomsDataInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
				AssertEquals("noDECustomsDataInterchange.EI_Status", EDIInterchange.Status.Error, noDECustomsDataInterchange.EI_Status);
			});
		}

		[TestDate(2019, 7, 15, 12, 17, 0)]
		public void TestCreateMessage_ATLAS()
		{
			var interchanges = new List<(EDIInterchange interchange, ZString applicationRef, ZString messageType, ZString messageGroup)>();
			//Iterate through all effective ResponseMessages and create an appropriate interchange. We have to keep the order as ResponseMessages are concatenated in Prod code.
			foreach (var messageTechnicalName in ATLASResponseMessageDetails.Instance.ResponseMessages.Keys)
			{
				EDIInterchange interchange = null;
				if (ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages.ContainsKey((messageTechnicalName)))
				{
					interchange = CreateInterchangeForNcts(messageTechnicalName, NctsMessageSubTypeList.Codes.StatusRequestMessage);
					interchanges.Add((interchange, messageTechnicalName, EDIMessageTypeList.Codes.NCTS, NctsMessageSubTypeList.Codes.StatusRequestMessage));
				}
				else if (ATLASResponseMessageDetails.Instance.TemporaryStorageVersion10_1ResponseMessages.ContainsKey(messageTechnicalName))
				{
					interchange = CreateInterchangeForAtlasVersion10_1(messageTechnicalName, TemporaryStorageMessageSubTypeList.Codes.ConfirmationForTemporaryStorage);
					interchanges.Add((interchange, messageTechnicalName, EDIMessageTypeList.Codes.TemporaryStorage, TemporaryStorageMessageSubTypeList.Codes.ConfirmationForTemporaryStorage));
				}
				else if (ATLASResponseMessageDetails.Instance.ImportAndCollectiveVersion10_1ResponseMessages.ContainsKey(messageTechnicalName))
				{
					var lrnXmlElementName = messageTechnicalName.In(new ZString[] { nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DEERRF), nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.GCNOAD) }) ? "LocalReferenceNumber" : "LRN";
					interchange = CreateInterchangeForAtlasVersion10_1(messageTechnicalName, ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration, lrnXmlElementName);
					interchanges.Add((interchange, messageTechnicalName, EDIMessageTypeList.Codes.Import, ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration));
				}
				else
				{
					Fail($"Testing for this type is not implemented. Please implement testing for {messageTechnicalName}");
				}
			}

			Factory.Save();
			processor.ExecuteBatch();
			CombineAssertions(() =>
			{
				var effectiveResponseMessagesCount = ATLASResponseMessageDetails.Instance.ResponseMessages.Where(d => d.Value.EDIMessageType.GetGenericTypeDefinition() == typeof(AtlasInboundEDIMessage<>)).Count();
				AssertEquals("For each effective ResponseMessage an interchange has been created", effectiveResponseMessagesCount, interchanges.Count);
				foreach (var (interchange, applicationRef, messageType, messageGroup) in interchanges)
				{
					interchange.Reload();
					AssertNull(interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
					AssertEquals(applicationRef + " interchangeInDiffFactory.EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
					AssertEquals(applicationRef + " interchangeInDiffFactory.ContainedMessages.Count", 1, interchange.ContainedMessages.Count);
					AssertMessageAndLogbookDetails(interchange.ContainedMessages[0], applicationRef, messageType, interchange.EI_BodyText, interchange.EI_InterchangeNum, messageGroup);
					AssertEquals(applicationRef + " interchangeInDiffFactory.EI_SystemCreateTimeUTC", ZDateTime.Now, interchange.EI_SystemCreateTimeUtc);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new DEAInboundInterchangeProcessor();
		}
		DEAInboundInterchangeProcessor processor;

		void AssertMessageAndLogbookDetails(EDIMessage message, ZString applicationReference, ZString messageType, ZString bodyText, ZString messageNum, string messageGroup)
		{
			AssertEquals(applicationReference + " message.EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, message.EM_ApplicationCode);
			AssertEquals(applicationReference + " message.EM_ApplicationReference", applicationReference, message.EM_ApplicationReference);
			AssertEquals(applicationReference + " message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(applicationReference + " message.EM_MessageNum", messageNum, message.EM_MessageNum);
			AssertEquals(applicationReference + " message.EM_MessageType", messageType, message.EM_MessageType);
			AssertEquals(applicationReference + " message.EM_MessageSubType", messageGroup, message.EM_MessageSubType);
			AssertEquals(applicationReference + " message.EM_MessageText", bodyText, message.EM_MessageText);
			AssertEquals(applicationReference + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(applicationReference + " LogbookEORIBranchSuffix", "0001", message.GetLogbookEORIBranchSuffix());
			AssertEquals(applicationReference + " LogbookLocalReferenceNumber", "LOCALREFERENCENUMBER", message.GetLogbookLocalReferenceNumber());
		}

		EDIInterchange CreateInterchangeForNcts(ZString messageTechnicalName, ZString messageGroup)
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
			<messageGroup>{messageGroup}</messageGroup>
			<messageType>{messageTechnicalName}</messageType>
			<MessageRecipient>
				<subsidiaryNumber>0001</subsidiaryNumber>
			</MessageRecipient>
			<TransitOperation>
				<LRN>LOCALREFERENCENUMBER</LRN>
			</TransitOperation>
		</{messageTechnicalName}>
	</CustomsData>
</DECustomsData>";
			});
		}

		EDIInterchange CreateInterchangeForAtlasVersion10_1(ZString messageTechnicalName, ZString messageGroup, string lrnXmlElementName = "LRN")
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
				<MessageGroup>{messageGroup}</MessageGroup>
				<MessageType>{messageTechnicalName}</MessageType>
				<InterchangeRecipient>
					<Identification>
						<ReferenceNumber>DE9000348</ReferenceNumber>
						<SubsidiaryNumber>0001</SubsidiaryNumber>
					</Identification>
				</InterchangeRecipient>
			</MetaData>
			<Header>
				<{lrnXmlElementName}>LOCALREFERENCENUMBER</{lrnXmlElementName}>
			</Header>
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
	}
}
