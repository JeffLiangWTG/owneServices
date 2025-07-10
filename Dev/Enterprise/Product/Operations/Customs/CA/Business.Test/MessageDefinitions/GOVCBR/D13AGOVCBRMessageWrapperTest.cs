using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class D13AGOVCBRMessageWrapperTest : GOVCBRMessageWrapperTest
	{
		public override void TestHasGOVCBRMessage()
		{
			var rawMessage = CreateRawMessage("1");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			Assert("HasGOVCBRMessage is true", wrapper.HasGOVCBRMessage);
		}

		public override void TestDocumentName()
		{
			var rawMessage = CreateRawMessage("1");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			AssertEquals("DocumentName", "23", wrapper.DocumentName);
		}

		public override void TestDocumentReference()
		{
			var rawMessage = CreateRawMessage("1");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			AssertEquals("DocumentReference", "10207003502172", wrapper.DocumentReference);
		}

		public override void TestProcessingDate()
		{
			var rawMessage = CreateRawMessage("1");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			AssertEquals("ProcessingDate", new ZDateTime(2022, 06, 21, 11, 18, 00), wrapper.ProcessingDate);
		}

		public override void TestIsMessageContentAccepted()
		{
			var rawMessage = CreateRawMessage("1");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			Assert("IsMessageContentAccepted", wrapper.IsMessageContentAccepted);
		}

		public override void TestIsMessageContentAcceptedWithComments()
		{
			var rawMessage = CreateRawMessage("66");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			Assert("IsMessageContentAcceptedWithComments", wrapper.IsMessageContentAcceptedWithComments);
		}

		public override void TestIsMessageContentRejectedWithComment()
		{
			var rawMessage = CreateRawMessage("2");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			Assert("IsMessageContentRejectedWithComment", wrapper.IsMessageContentRejectedWithComment);
		}

		public override void TestIsMessageReceived()
		{
			var rawMessage = CreateRawMessage("17");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			Assert("IsMessageReceived", wrapper.IsMessageReceived);
		}

		public override void TestIsErrorMessage()
		{
			var rawMessage = CreateRawMessage("14");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			Assert("IsErrorMessage", wrapper.IsErrorMessage);
		}

		public override void TestOriginalMessageReference()
		{
			var rawMessage = CreateRawMessage("1");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			AssertEquals("OriginalMessageReference", "HBL-803636474747", wrapper.OriginalMessageReference);
		}

		public override void TestErrorComments()
		{
			var rawMessage = CreateRawMessage("1");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			AssertContainsExactElementsInAnyOrder("ErrorComments", Array.Empty<string>(), wrapper.ErrorComments);
		}

		public override void TestNotifications()
		{
			var rawMessage = CreateRawMessage("1");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			AssertContainsExactElementsInAnyOrder("Notifications", new[] { "H11" }, wrapper.Notifications.Select(x => x.code));
		}

		public override void TestNoticeStatusCode()
		{
			var rawMessage = CreateRawMessage("1");
			var message = CreateUniversalEventMessage(rawMessage);
			var wrapper = new D13AGOVCBRMessageWrapper(message);
			AssertEquals("NoticeStatusCode", "0001", wrapper.NoticeStatusCode);
		}

		ZString CreateRawMessage(ZString geiValue)
		{
			return ZString.Format(@"UNH+1+GOVCBR:D:13A:UN
BGM+23:::RA0-1000+10207003502172:1:1+11
DTM+9:202206211118:203
RFF+AGO:HBL-803636474747
RFF+ACE:17897003022057::RD0-1000
GOR++5
LOC+22+0497+4407
STS++2:::0001
RCS+15+1::5
FTX+SIN+++LEI TO ARRANGE
GEI+5+{0}
ERC+H11
TDT+3
EQD+CN+UASU1052332
SEQ+4
UNS+D
HYN+3
UNS+S
UNT+17+7
UNE+1+733
UNZ+1+6588".Replace("'\r\n", "'").Replace("\r\n", "'"), geiValue);
		}
		UniversalEventMessage CreateUniversalEventMessage(ZString rawMessage)
		{
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "12345";

			var message = Factory.New<UniversalEventMessage>();
			message.EM_LinkedObject = master;
			message.EM_MessageText = ZString.Format(@"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type></Type>
					<Key>123</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime></EventTime>
		<EventType></EventType>
		<ContextCollection>
			<Context>
				<Type>MessageNumber</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>RawMessage</Type>
				<Value>{0}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>", rawMessage);

			return message;
		}
	}
}
