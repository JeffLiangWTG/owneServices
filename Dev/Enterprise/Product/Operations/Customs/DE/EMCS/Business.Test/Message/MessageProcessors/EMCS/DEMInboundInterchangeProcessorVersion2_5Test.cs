using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class DEMInboundInterchangeProcessorVersion2_5Test : TestCaseWithFactory
	{
		public void TestInvalidInterchange()
		{
			var invalidInterchange = CreateInterchange("$#$#$", LogBookTime);
			Factory.Save();
			processor.ExecuteBatch();
			invalidInterchange = newFactory.Load<EDIInterchange>(invalidInterchange.PK);
			CombineAssertions(() =>
			{
				AssertNull(invalidInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
				AssertEquals("invalidInterchange.EI_Status", EDIInterchange.Status.Failed, invalidInterchange.EI_Status);
			});
		}

		public void TestNoDECustomsData()
		{
			var noDECustomsDataInterchange = CreateInterchange("", LogBookTime);
			noDECustomsDataInterchange.EI_BodyText = ZString.Empty;
			Factory.Save();
			processor.ExecuteBatch();
			noDECustomsDataInterchange = newFactory.Load<EDIInterchange>(noDECustomsDataInterchange.PK);
			CombineAssertions(() =>
			{
				AssertEquals("NO DE CUSTOMS DATA", noDECustomsDataInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
				AssertEquals("noDECustomsDataInterchange.EI_Status", EDIInterchange.Status.Error, noDECustomsDataInterchange.EI_Status);
			});
		}

		public void TestInvalidDECustomsData()
		{
			var interchange = CreateInterchange(messageName, "2019-11-45 15:29:12");
			Factory.Save();
			processor.ExecuteBatch();
			interchange.Reload();
			CombineAssertions(() =>
			{
				AssertContains("Log", "The 'LogbookTime' element is invalid - The value '2019-11-45 15:29:12' is invalid according to its datatype 'String' - The Pattern constraint failed.", processor.Logger.UserLogStrings[0]);
				AssertEquals("EI_Status", EDIInterchange.Status.Failed, interchange.EI_Status);
			});
		}

		[TestDate(2020, 1, 14, 12, 45, 0)]
		public void TestCreateMessage()
		{
			var interchanges = new List<(EDIInterchange interchange, ZString messageType)>();
			var messageNames = EmcsResponseMessageDetails.Instance.Version2_5ResponseMessages.Select(x => x.Key);

			foreach (var name in messageNames)
			{
				var interchange = CreateInterchange(name, LogBookTime);
				interchanges.Add((interchange, name));
			}
			Factory.Save();
			CombineAssertions(() =>
			{
				processor.ExecuteBatch();

				foreach (var data in interchanges)
				{
					var interchangeInDiffFactory = newFactory.Load<EDIInterchange>(data.interchange.PK);
					AssertEquals(data.messageType + " Error Logs", null, interchangeInDiffFactory.Logs.MostRecentLogByEventTime(Events.ErrorReport));
					AssertEquals(data.messageType + " interchangeInDiffFactory.EI_Status", EDIInterchange.Status.Received, interchangeInDiffFactory.EI_Status);
					AssertEquals(data.messageType + " interchangeInDiffFactory.ContainedMessages.Count", 1, interchangeInDiffFactory.ContainedMessages.Count);
					AssertMessage(interchangeInDiffFactory.ContainedMessages[0], data.messageType, data.interchange.EI_BodyText, interchangeInDiffFactory.EI_InterchangeNum);
					AssertEquals(data.messageType + " interchangeInDiffFactory.EI_SystemCreateTimeUTC", new ZDateTime(2020, 1, 14, 12, 45, 0), interchangeInDiffFactory.EI_SystemCreateTimeUtc);
				}
			});
		}

		public void TestLogbookTime()
		{
			var interchange = CreateInterchange(messageName, LogBookTime);
			Factory.Save();
			processor.ExecuteBatch();
			var interchangeInDiffFactory = newFactory.Load<EDIInterchange>(interchange.PK);
			AssertEquals("interchangeInDiffFactory.EI_DeliveredTime", "2023-08-28T15:51:10.5208000+02:00", interchangeInDiffFactory.EI_DeliveredTime.ToISO8601String());
		}

		public void TestInvalidLogbookTime()
		{
			var interchange = CreateInterchange(messageName, "2019-19-34T15:29:12.2347000+02:00");
			Factory.Save();
			processor.ExecuteBatch();
			AssertEquals("LogbookTime Invalid. Interchange Number: 1, LogBookTime: 2019-19-34T15:29:12.2347000+02:00", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new DEMInboundInterchangeProcessor();
			newFactory = new BusinessObjectFactory();
			messageName = EmcsResponseMessageDetails.Instance.ResponseMessages.First().Key;
		}
		DEMInboundInterchangeProcessor processor;
		BusinessObjectFactory newFactory;
		ZString messageName;

		void AssertMessage(EDIMessage message, ZString applicationReference, ZString bodyText, ZString messageNum)
		{
			AssertEquals(applicationReference + " message.EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsEmcsSystem, message.EM_ApplicationCode);
			AssertEquals(applicationReference + " message.EM_ApplicationReference", applicationReference, message.EM_ApplicationReference);
			AssertEquals(applicationReference + " message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(applicationReference + " message.EM_MessageNum", messageNum, message.EM_MessageNum);
			AssertEquals(applicationReference + " message.EM_MessageType", DE.Messaging.EDIMessageTypeList.Codes.EMCS, message.EM_MessageType);
			AssertEquals(applicationReference + " message.EM_MessageSubType", Messaging.EmcsMessageSubTypeList.Codes.Eme, message.EM_MessageSubType);
			AssertEquals(applicationReference + " message.EM_MessageText", bodyText, message.EM_MessageText);
			AssertEquals(applicationReference + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		}

		EDIInterchange CreateInterchange(ZString messageName, ZString logbookTime)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.DECustomsEmcsSystem;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "DEEAES";
			interchange.EI_To = "KDSER";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_BodyText = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DECustomsData>
	<LogbookTime>{logbookTime}</LogbookTime>
	<CustomsData>
		<{messageName}>
			<Header>
				<MessageGroup>EME</MessageGroup>
			</Header>
		</{messageName}>
	</CustomsData>
</DECustomsData>
";
			return interchange;
		}

		const string LogBookTime = "2023-08-28T15:51:10.520793+02:00";
	}
}
