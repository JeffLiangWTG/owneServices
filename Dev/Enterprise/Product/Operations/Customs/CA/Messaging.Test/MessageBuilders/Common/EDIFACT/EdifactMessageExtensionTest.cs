using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CA.Messaging.Testing
{
	sealed class EdifactMessageExtensionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestOriginalMessageNo()
		{
			CombineAssertions("GetOriginalMessageNumber from CUSRES:D:00A", () =>
			{
				var message = Factory.New<EDIMessage>();
				message.EM_MessageText =
	@"UNH+1+CUSRES:D:00A:UN'
BGM+:::687+5555S000053727BLO+11'
DTM+9:202001060250:203'
GIS+14'
ERP+2:2123:28'
ERC+ZZZ'
FTX+AAO+++SEGMENTNADLINE11ELEM3164[6.0]ELEM TOO LONG'
ERP+2:2123:28'
ERC+ZZZ'
FTX+AAO+++SEGMENTGIDBYTE OFFSETLOOP COUNT EXCEEDED'
UNT+11+1'".Replace("\r", "").Replace("\n", "");
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				var cusres = (Edifact.D00A.Messages.CUSRES.CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new Edifact.D00A.EdifactD00AMessageFactory(), characterSet);
				NUnit.Framework.Assert.That(cusres.GetOriginalMessageNo(), NUnit.Framework.Is.EqualTo("2123").Using(CustomComparers.TypeComparison));
			});

			CombineAssertions("GetOriginalMessageNumber from CUSRES:S:99B", () =>
			{
				var message = Factory.New<EDIMessage>();
				message.EM_MessageText =
	@"UNH+3+CUSRES:S:99B:UN+10207'
BGM+:::000001498+463+11'
DTM+137:201902010150:203'
GIS+14'
ERP+2:2298:29'
FTX+AAO+++SEGMENTLOCLINE2ELE POS2,1:MAND ELEM MISSING'
UNT+7+3'".Replace("\r", "").Replace("\n", "");
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				var cusres = (Edifact.D99B.Messages.CUSRES.CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new EdifactS99BMessageFactoryForTest(), characterSet);
				NUnit.Framework.Assert.That(cusres.GetOriginalMessageNo(), NUnit.Framework.Is.EqualTo("2298").Using(CustomComparers.TypeComparison));
			});

			CombineAssertions("GetOriginalMessageNumber from GOVCBR:D:11B", () =>
			{
				var message = Factory.New<EDIMessage>();
				message.EM_MessageText =
	@"UNH+1+GOVCBR:D:11B:UN'
BGM+961+800520521CAB+11'
DTM+9:202104141019:203'
RFF+AGO:HBL-S1IHKG100000532'
RCS+11'
FTX+AAO+++UMRN(2269)SEGMENTDOCLINE4ELEM1004(2.0)COND ELEM MISSING'
GEI+5+14'
UNS+D'
HYN+3'
UNS+S'
UNT+11+1'".Replace("\r", "").Replace("\n", "");
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				var cusres = (Edifact.D11B.Messages.GOVCBR.GOVCBRMessage)message.GetAutoEdifactMessageUsingNamedFactory(new Edifact.D11B.D11BMessageFactory(), characterSet);
				NUnit.Framework.Assert.That(cusres.GetOriginalMessageNo(), NUnit.Framework.Is.EqualTo("2269").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestGetSyntaxErrors()
		{
			CombineAssertions("GetSyntaxErrors from CUSRES:D:00A", () =>
			{
				var message = Factory.New<EDIMessage>();
				message.EM_MessageText =
@"UNH+1+CUSRES:D:00A:UN
BGM+:::687+8010S00001244D+11
DTM+9:201107200202:203
GIS+14
ERP+2:55:28
ERC+ZZZ
FTX+AAO+++SEGMENTGEILINE8ELEM7364[2.4]INVALID CODE
FTX+AAO+++SEGMENTCSTBYTE OFFSETSEG USE EXCEEDED
UNT+8+1".Replace("\r\n", "'");
				var cusres = (Edifact.D00A.Messages.CUSRES.CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new Edifact.D00A.EdifactD00AMessageFactory(), characterSet);
				var errors = cusres.GetSyntaxErrors(ZString.Empty);
				NUnit.Framework.Assert.That(errors.Count(), NUnit.Framework.Is.EqualTo(2));
			});

			CombineAssertions("GetSyntaxErrors from CUSRES:S:99B", () =>
			{
				var message = Factory.New<EDIMessage>();
				message.EM_MessageText =
	@"UNH+1+CUSRES:S:99B:UN+12345
BGM+:::000000250+080+11
DTM+137:201007071025:203
GIS+14
ERP+2:35:29
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:ELEM TOO LONG
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:INVALID CHARS
FTX+AAO+++SEGMENTGINLINE21ELE POS2,1:ELEM TOO LONG
FTX+AAO+++SEGMENTLOC- BYTE OFFSET256:MAND SEG MISSING
FTX+AAO+++BLA BLA
UNT+11+1".Replace("\r\n", "'");
				var cusres = (Edifact.D99B.Messages.CUSRES.CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new EdifactS99BMessageFactoryForTest(), characterSet);
				NUnit.Framework.Assert.That(cusres.GetSyntaxErrors(ZString.Empty).Count(), NUnit.Framework.Is.EqualTo(5));
			});

			CombineAssertions("GetSyntaxErrors from GOVCBR:D:11B", () =>
			{
				var message = Factory.New<EDIMessage>();
				message.EM_MessageText =
@"UNH+1+GOVCBR:D:11B:UN
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
UNT+12+1".Replace("\r\n", "'");
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				var cusres = (Edifact.D11B.Messages.GOVCBR.GOVCBRMessage)message.GetAutoEdifactMessageUsingNamedFactory(new Edifact.D11B.D11BMessageFactory(), characterSet);
				NUnit.Framework.Assert.That(cusres.GetSyntaxErrors(ZString.Empty).Count(), NUnit.Framework.Is.EqualTo(2));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			characterSet = new CACharSet();
		}

		CACharSet characterSet;

		sealed class EdifactS99BMessageFactoryForTest : EdifactD99BMessageFactory
		{
			internal EdifactS99BMessageFactoryForTest()
			{
				AddRegisteredMessage(new MessageRegistration(typeof(Edifact.D99B.Messages.CUSRES.CUSRESMessage), "UN", "S", "99B", "CUSRES"));
			}
		}
	}
}
