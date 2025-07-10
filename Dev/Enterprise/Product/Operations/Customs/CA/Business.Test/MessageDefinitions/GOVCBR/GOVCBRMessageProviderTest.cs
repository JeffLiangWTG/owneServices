using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class GOVCBRMessageProviderTest : TestCaseWithFactory
	{
		public void TestGetMessageWrapper()
		{
			var message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+312+8036081-14073001+11
DTM+9:201407300108:203
RFF+AGO:CLS-C10001000
RCS+11
GEI+5+1
UNS+D
HYN+3
UNS+S
UNT+10+1".Replace("'\r\n", "'").Replace("\r\n", "'");

			var wrapper = GOVCBRMessageProvider.GetMessageWrapper(message);
			AssertEquals("type is D11BGOVCBRMessageWrapper", typeof(D11BGOVCBRMessageWrapper), wrapper.GetType());

			var message2 = Factory.New<UniversalEventMessage>();
			var rawMessage = @"UNH+1+GOVCBR:D:13A:UN
BGM+23:::RA0-1000+10207003502172:1:1+11
DTM+9:202206211118:203
RFF+AGO:HBL-803636474747
GOR++5
STS++2:::0001
UNS+D
HYN+3
UNS+S
UNT+10+1
UNE+1+733
UNZ+1+6588".Replace("'\r\n", "'").Replace("\r\n", "'");

			message2.EM_MessageText = ZString.Format(@"<UniversalEvent>
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

			wrapper = GOVCBRMessageProvider.GetMessageWrapper(message2);
			AssertEquals("type is D13AGOVCBRMessageWrapper", typeof(D13AGOVCBRMessageWrapper), wrapper.GetType());
		}
	}
}
