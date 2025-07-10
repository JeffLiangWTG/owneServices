using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class DEEInboundInterchangeProcessorVersion3_0Test : TestCaseWithFactory
	{
		public void TestInvalidInterchange()
		{
			var invalidInterchange = CreateInterchange("$#$#$");
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
			var noDECustomsDataInterchange = CreateInterchange("");
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

		[TestDate(2021, 7, 12, 10, 25, 0)]
		public void TestCreateMessage()
		{
			var interchanges = new List<(EDIInterchange interchange, ZString messageType)>();
			var messageNames = AesResponseMessageDetails.Instance.AesVersion3_0ResponseMessages.Select(x => x.Key).ToArray();

			foreach (var name in messageNames)
			{
				var interchange = CreateInterchange(name);
				interchanges.Add((interchange, name));
			}
			Factory.Save();
			processor.ExecuteBatch();
			CombineAssertions(() =>
			{
				foreach (var (interchange, messageType) in interchanges)
				{
					interchange.Reload();
					AssertNull(interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
					AssertEquals(messageType + " interchangeInDiffFactory.EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
					AssertEquals(messageType + " interchangeInDiffFactory.ContainedMessages.Count", 1, interchange.ContainedMessages.Count);
					var message = interchange.ContainedMessages[0];
					AssertEquals(messageType + " message.EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAesSystem, message.EM_ApplicationCode);
					AssertEquals(messageType + " message.EM_ApplicationReference", messageType, message.EM_ApplicationReference);
					AssertEquals(messageType + " message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
					AssertEquals(messageType + " message.EM_MessageNum", interchange.EI_InterchangeNum, message.EM_MessageNum);
					AssertEquals(messageType + " message.EM_MessageType", Messaging.EDIMessageTypeList.Codes.AES, message.EM_MessageType);
					AssertEquals(messageType + " message.EM_MessageSubType", "EXP", message.EM_MessageSubType);
					AssertEquals(messageType + " message.EM_MessageText", interchange.EI_BodyText, message.EM_MessageText);
					AssertEquals(messageType + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
					AssertEquals(messageType + " LogbookEORIBranchSuffix", "0001", message.GetLogbookEORIBranchSuffix());
					AssertEquals(messageType + " LogbookLocalReferenceNumber", "LOCALREFERENCENUMBER", message.GetLogbookLocalReferenceNumber());
					AssertEquals(messageType + " interchangeInDiffFactory.EI_SystemCreateTimeUTC", ZDateTime.Now, interchange.EI_SystemCreateTimeUtc);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new DEEInboundInterchangeProcessor();
		}
		DEEInboundInterchangeProcessor processor;

		EDIInterchange CreateInterchange(ZString messageName)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.DECustomsAesSystem;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "DEEAES";
			interchange.EI_To = "KDSER";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_BodyText = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DECustomsData>
	<LogbookTime>2019-11-20T15:29:00.2347048+02:00</LogbookTime>
	<CustomsData>
		<{messageName}>
			<preparationDateAndTime>2021-07-06T15:48:00</preparationDateAndTime>
			<messageIdentification>3000000012</messageIdentification>
			<messageGroup>EXP</messageGroup>
			<messageType>{messageName}</messageType>
			<messageVersion>F.1.2</messageVersion>
			<correlationIdentifier>HYEZNTCMT00000000000672</correlationIdentifier>
			<MessageSender>
				<referenceNumber>DE005866</referenceNumber>
			</MessageSender>
			<MessageRecipient>
				<identificationNumber>DE9000348</identificationNumber>
				<subsidiaryNumber>0001</subsidiaryNumber>
			</MessageRecipient>
			<ExportOperation>
				<LRN>LOCALREFERENCENUMBER</LRN>
				<declarationType>AA</declarationType>
				<additionalDeclarationType>A</additionalDeclarationType>
				<exportDeclarationType>20000000</exportDeclarationType>
				<partyConstellation>1000</partyConstellation>
				<declarationRecordationDateAndTime>2000-01-01T00:00:00</declarationRecordationDateAndTime>
				<declarationAcceptanceDateAndTime>2000-01-01T00:00:00</declarationAcceptanceDateAndTime>
				<releaseDateAndTime>2000-01-01T00:00:00</releaseDateAndTime>
				<decisiveDate>2000-01-01</decisiveDate>
				<exitDate>2000-01-01</exitDate>
				<security>0</security>
				<specificCircumstanceIndicator>A00</specificCircumstanceIndicator>
				<totalAmountInvoiced>0</totalAmountInvoiced>
				<invoiceCurrency>AAA</invoiceCurrency>
			</ExportOperation>
		</{messageName}>
	</CustomsData>
</DECustomsData>";
			return interchange;
		}
	}
}
