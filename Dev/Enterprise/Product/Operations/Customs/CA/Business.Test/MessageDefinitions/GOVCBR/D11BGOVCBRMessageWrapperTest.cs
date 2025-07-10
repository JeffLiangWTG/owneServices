using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class D11BGOVCBRMessageWrapperTest : GOVCBRMessageWrapperTest
	{
		public override void TestHasGOVCBRMessage()
		{
			var message = CreateACIForwarderMessage("1");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("HasGOVCBRMessage", true, wrapper.HasGOVCBRMessage);
		}

		public override void TestDocumentName()
		{
			var message = CreateACIForwarderMessage("1");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("DocumentName", "312", wrapper.DocumentName);
		}

		public override void TestDocumentReference()
		{
			var message = CreateACIForwarderMessage("1");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("DocumentReference", "8036081-14073001", wrapper.DocumentReference);
		}

		public override void TestProcessingDate()
		{
			var message = CreateACIForwarderMessage("1");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("ProcessingDate", new ZDateTime(2022, 07, 30, 01, 08, 00), wrapper.ProcessingDate);
		}

		public override void TestIsMessageContentAccepted()
		{
			var message = CreateACIForwarderMessage("1");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("IsMessageContentAccepted", true, wrapper.IsMessageContentAccepted);
		}

		public override void TestIsMessageContentAcceptedWithComments()
		{
			var message = CreateACIForwarderMessage("66");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("IsMessageContentAcceptedWithComments", true, wrapper.IsMessageContentAcceptedWithComments);
		}

		public override void TestIsMessageContentRejectedWithComment()
		{
			var message = CreateACIForwarderMessage("2");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("IsMessageContentRejectedWithComment", true, wrapper.IsMessageContentRejectedWithComment);
		}

		public override void TestIsMessageReceived()
		{
			var message = CreateACIForwarderMessage("17");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("IsMessageReceived", true, wrapper.IsMessageReceived);
		}

		public override void TestIsErrorMessage()
		{
			var message = CreateACIForwarderMessage("14");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("IsErrorMessage", true, wrapper.IsErrorMessage);
		}

		public override void TestOriginalMessageReference()
		{
			var message = CreateACIForwarderMessage("1");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("OriginalMessageReference", "CLS-C10001000", wrapper.OriginalMessageReference);
		}

		ACIForwarderMessage CreateACIForwarderMessage(ZString geiValue)
		{
			var message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = ZString.Format(@"UNH+1+GOVCBR:D:11B:UN
BGM+312+8036081-14073001+11
DTM+9:202207300108:203
RFF+AGO:CLS-C10001000
RCS+11
GEI+5+{0}
UNS+D
HYN+3
UNS+S
UNT+10+1".Replace("'\r\n", "'").Replace("\r\n", "'"), geiValue);

			return message;
		}

		public override void TestErrorComments()
		{
			var message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+961+80363647474C+11
DTM+9:201308060133:203
RFF+AGO:HBL-803636474747
RCS+11
FTX+AAO+++UMRN(377)SEGMENTPACLINE21ELEM7064(3.4)MAND ELEM MISSING
FTX+AAO+++UMRN(377)SEGMENTCNTLINE27ELEM6411(1.3)MAND ELEM MISSING
GEI+5+14
UNS+D
HYN+3
UNS+S
UNT+12+1".Replace("'\r\n", "'").Replace("\r\n", "'");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertContainsExactElementsInAnyOrder("ErrorComments", new string[] { "UMRN(377)SEGMENTPACLINE21ELEM7064(3.4)MAND ELEM MISSING", "UMRN(377)SEGMENTCNTLINE27ELEM6411(1.3)MAND ELEM MISSING" }, wrapper.ErrorComments);
		}

		public override void TestNotifications()
		{
			var message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+313+8036X555+11
DTM+9:201308072136:203
RFF+AGO:HBL-CAH0000004
RCS+11
FTX+AAO+++29
GEI+5+14
ERC+H11
ERP+2:420:29
UNS+D
HYN+3
UNS+S
UNT+13+1".Replace("'\r\n", "'").Replace("\r\n", "'");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertContainsExactElementsInAnyOrder("Notifications", new[] { "H11" }, wrapper.Notifications.Select(x => x.code));
		}

		public override void TestNoticeStatusCode()
		{
			var message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN'
BGM+23+10207000007531'
DTM+9:201411250601:203'
RFF+AGO:857477707RM0001'
STS++2:::0001'
UNS+D'
HYN+3'
UNS+S'
UNT+9+1'
".Replace("'\r\n", "'").Replace("\r\n", "'");
			var wrapper = new D11BGOVCBRMessageWrapper(message);
			AssertEquals("NoticeStatusCode", "0001", wrapper.NoticeStatusCode);
		}
	}
}
