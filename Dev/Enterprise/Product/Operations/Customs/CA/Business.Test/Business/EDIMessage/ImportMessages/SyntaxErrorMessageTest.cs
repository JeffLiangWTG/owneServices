using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(SyntaxErrorMessage))]
	sealed class SyntaxErrorMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFormattedMessageTextForB3SyntaxError()
		{
			#region Expected Interpretation Text

			const string expectedInterpretationText = @"Source Message Interpretation:

0 - UNH+157+CUSDEC:S:99B:UN'
1 - BGM+:::AB+080+9'
2 - LOC+41+706'
3 - RFF+TN:000000250'
4 - RFF+ARA:123456789RM1234'
5 - TDT+11++9'
6 - DOC+785+9463TKWB1438742C'

{0} 7 - MOA+43:200.22'
{0} Error Message: ELEM TOO LONG,  Component Value: '200.22', Segment/Position: MOA / L: 7, P: 1,2.
{0} Error Message: INVALID CHARS,  Component Value: '200.22', Segment/Position: MOA / L: 7, P: 1,2.

8 - UNS+D'
9 - DMS+1'
10 - NAD+SE++ABC CANADA'
11 - DOC+935'
12 - DTM+129:20100930:102'
13 - LOC+27+CA+CA'
14 - PAT+1+CONSIGN:::02'
15 - MOA+6::CAD'

{0} 16 - CST+1+POS+1+8525800010+13'
{0} Error Message: MAND SEG MISSING,  Component Value: '', Segment/Position: LOC / L: 16, I: 12.

17 - MOA+40:20000'
18 - MOA+43:20000'
19 - MOA+125:20000'
20 - RFF+MF::1'

{0} 21 - GIN+PN+TRANSMISSION APPARATUS FOR RADIO-BR:OADC'
{0} Error Message: ELEM TOO LONG,  Component Value: 'TRANSMISSION APPARATUS FOR RADIO-BR', Segment/Position: GIN / L: 21, P: 2,1.

22 - RFF+LI:1:1'
23 - MOA+38:20000'
24 - TAX+7+VAT++5.0'
25 - MOA+1:1000'
26 - GIR+1+1'
27 - MEA+AAR++NMB:20000'
28 - TAX+5+++0.0'
29 - MOA+155:000'
30 - UNS+S'
31 - TAX+7+:::K90'
32 - MOA+1:1000'
33 - TAX+4+:::K90'
34 - MOA+176:1000'
35 - UNT+36+157'

{0} Error Message: BLA BLA,  Component Value: '', Segment/Position: .

Message Text:

UNH+1+CUSRES:S:99B:UN+12345
BGM+:::000000250+080+11
DTM+137:201007071025:203
GIS+14
ERP+2:35:29
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:ELEM TOO LONG
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:INVALID CHARS
FTX+AAO+++SEGMENTGINLINE21ELE POS2,1:ELEM TOO LONG
FTX+AAO+++SEGMENTLOC- BYTE OFFSET256:MAND SEG MISSING
FTX+AAO+++BLA BLA
UNT+11+1
";

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "25";

			var systemCreateTime = new ZDateTime(2010, 10, 1);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Messages.Add(GetSentEDIMessage<EDIReleaseMessage>(Factory, BlaText, systemCreateTime));
			entryHeader.Messages.Add(GetRecivedEDIMessage<B3Message>(Factory, BlaText, systemCreateTime));
			entryHeader.Messages.Add(GetSentEDIMessage<B3Message>(Factory, B3MessageText, systemCreateTime, "35"));
			entryHeader.Messages.Add(GetSentEDIMessage<B3Message>(Factory, BlaText, new ZDateTime(2010, 10, 3)));
			entryHeader.Messages.Add(GetSyntaxErrorMessage(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.B3CUSDEC, B3SyntaxErrorMessageText));

			AssertMultilineASCIIEquals("EM_MessageInterpretation", string.Format(expectedInterpretationText, "##"), message.EM_FormattedMessageText.Replace("\t", ""));
		}

		public void TestFormattedMessageTextForQuerySyntaxError()
		{
			#region Expected Interpretation Text

			const string expectedInterpretationText = @"Source Message Interpretation:

0 - UNH+158+CUSDEC:S:99B:UN'
1 - BGM+:::QA+081+9'

{0} 2 - RFF+ABD:85258000101'
{0} Error Message: ELEM TOO LONG,  Component Value: '85258000101', Segment/Position: RFF / L: 2, P: 1,2.

3 - DTM+7:20100930:102'

{0} 4 - RFF+AFG:990'
{0} Error Message: ELEM TOO SHORT,  Component Value: '990', Segment/Position: RFF / L: 4, P: 1,2.

5 - DTM+204:20100930:102'
6 - UNS+D'
7 - UNS+S'
8 - UNT+9+158'

Message Text:

UNH+1+CUSRES:S:99B:UN+12345
BGM+:::+081+11
DTM+137:201007071025:203
GIS+14
ERP+2:35:29
FTX+AAO+++SEGMENTRFFLINE2ELE POS1,2:ELEM TOO LONG
FTX+AAO+++SEGMENTRFFLINE4ELE POS1,2:ELEM TOO SHORT
UNT+11+1

";

			#endregion

			var systemCreateTime = new ZDateTime(2010, 10, 1);
			GetSentEDIMessage<QueryMessage>(Factory, QueryMessageText, systemCreateTime, "35");
			GetRecivedEDIMessage<QueryMessage>(Factory, BlaText, systemCreateTime);
			GetSentEDIMessage<QueryMessage>(Factory, BlaText, new ZDateTime(2010, 10, 3));
			GetSyntaxErrorMessage(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.Query, QuerySyntaxErrorMessageText);

			AssertMultilineASCIIEquals("EM_MessageInterpretation", string.Format(expectedInterpretationText, "##"), message.EM_FormattedMessageText.Replace("\t", ""));
		}

		public void TestFormattedMessageTextForSupplementaryCargoReportSyntaxError()
		{
			#region Expected Interpretation Text

			const string expectedInterpretationText = @"Source Message Interpretation:

{0} 0 - UNH+55+GSMCAR:D:00A:UN:SUPRPT'
{0} Error Message: SEG USE EXCEEDED,  Component Value: '', Segment/Position: CST / L: 0, I: 0.

1 - BGM+85+S00043369+4'
2 - CST++687::96'
3 - TDT+20++1++8036'
4 - CNI+1'
5 - DOC+704+801027363636'
6 - RFF+ABE:8010S00001244D'
7 - LOC+8+CA:::VANCOUVER'

{0} 8 - GEI+6+:::22'
{0} Error Message: INVALID CODE,  Component Value: '22', Segment/Position: GEI / L: 8, P: 2,4.

9 - TDT+12'
10 - RFF+AIJ:OBL1234566'
11 - NAD+CN+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA'
12 - NAD+CZ+++ABC IMPORTS PTY LTD+123 MAIN RD:DO NOT CHANGE ANY DETAILS. USED FOR+ALEXANDRIA+NSW+2015+AU'
13 - CTA+CO'
14 - COM+61266699999:TE'
15 - GID+1'
16 - PAC+1++PLT'
17 - FTX+AAA+++BOOKS'
18 - MEA+WT+AAE+KGM:1000'
19 - MEA+VOL+:::X+WSD:1.000'
20 - PCI++MARKS 1-1 MADE IN AU'
21 - CST++11+12+13+14+15'
22 - UNT+23+55'

Message Text:

UNH+1+CUSRES:D:00A:UN
BGM+:::687+8010S00001244D+11
DTM+9:201107200202:203
GIS+14
ERP+2:55:28
ERC+ZZZ
FTX+AAO+++SEGMENTGEILINE8ELEM7364[2.4]INVALID CODE
FTX+AAO+++SEGMENTCSTBYTE OFFSETSEG USE EXCEEDED
UNT+8+1";

			#endregion

			var systemCreateTime = new ZDateTime(2010, 10, 1);
			GetSentEDIMessage<SUPRPTMessage>(Factory, SupplementaryCargoReportMessageText, systemCreateTime, "55");
			GetRecivedEDIMessage<SUPRPTMessage>(Factory, BlaText, systemCreateTime);
			GetSentEDIMessage<SUPRPTMessage>(Factory, BlaText, new ZDateTime(2010, 10, 3));
			GetSyntaxErrorMessage(EDIMessage.ApplicationCodes.CAACI, MessageTypeList.Codes.SupplementaryCargoReport, SupplementaryCargoReportSyntaxErrorMessageText);

			AssertMultilineASCIIEquals("EM_MessageInterpretation", string.Format(expectedInterpretationText, "##"), message.EM_FormattedMessageText.Replace("\t", ""));
		}

		public void TestFormattedMessageTextForEManifestHouseBillSyntaxError()
		{
			#region Expected Interpretation Text

			const string expectedInterpretationText = @"Source Message Interpretation:

0 - UNH+377+GOVCBR:D:11B:UN:ACIHG'
1 - BGM+714+80363647474C+4'
2 - RFF+ABO:HBL-803636474747'
3 - DOC+23+:24'
4 - DOC+85+081-11111111'
5 - TDT+11++4'
6 - UNS+D'
7 - HYN+3'
8 - CNI+1'
9 - STS++0'
10 - MEA+AAX++MTQ:1'
11 - NAD+CN+++ABC CANADA+111 HURONTARIO STREET+TORONTO+ON+M5P 1A2+CA'
12 - CTA+AH'
13 - COM+ 1 (905) 555-1247:TE'
14 - NAD+CZ+++TREETOYS PTY LTD+105 WOMBAT DRIVE+KATOOMBA+NSW+2780+AU'
15 - CTA+IC+:FRED NERCK'
16 - CTA+AH'
17 - COM+ 61290251100:TE'
18 - LOC+8+0821+3380'
19 - TDT+1'
20 - SEQ+4'

{0} 21 - PAC+5'
{0} Error Message: MAND ELEM MISSING,  Component Value: '', Segment/Position: PAC / L: 21, P: 3,4.

22 - SEQ+4'
23 - PCI++SOME MARKS'
24 - GID+1'
25 - FTX+AAA+++GOODS DESCRIPTION'
26 - UNS+S'

{0} 27 - CNT+7:121'
{0} Error Message: MAND ELEM MISSING,  Component Value: '', Segment/Position: CNT / L: 27, P: 1,3.

28 - UNT+29+377'

Message Text:

UNH+1+GOVCBR:D:11B:UN
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
UNT+12+1";

			#endregion

			var systemCreateTime = new ZDateTime(2010, 10, 1);
			GetSentEDIMessage<ACIHouseBillMessage>(Factory, EManifestHouseBillMessageText, systemCreateTime, "377");
			GetRecivedEDIMessage<ACIHouseBillMessage>(Factory, BlaText, systemCreateTime);
			GetSentEDIMessage<ACIHouseBillMessage>(Factory, BlaText, new ZDateTime(2010, 10, 3));
			GetSyntaxErrorMessage(ACIHouseBillMessage.ApplicationCodes.CAACI, MessageTypeList.Codes.ACIHouseBill, EManifestHouseBillSyntaxErrorMessageText);

			AssertMultilineASCIIEquals("EM_MessageInterpretation", string.Format(expectedInterpretationText, "##"), message.EM_FormattedMessageText.Replace("\t", ""));
		}

		public void TestMessageSubTypeDescription()
		{
			message.EM_MessageSubType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals("EM_MessageSubTypeDescription", MessageTypeList.Descriptions.B3CUSDEC, message.EM_MessageSubTypeDescription);
		}

		public void TestDefaultValues()
		{
			AssertEquals(EDIMessage.ApplicationCodes.CAIMP, message.EM_ApplicationCode);
			AssertEquals(MessageTypeList.Codes.SyntaxError, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestMessageNumberFilledIn()
		{
			var number = Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIMessage.ApplicationCodes.CAIMP).PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("MessageNumberFilledIn", "Message Number = " + number, message.EM_MessageText);
		}

		#region Implementation

		EDIMessage GetSyntaxErrorMessage(string appCode, string subType, string messageText)
		{
			message.EM_ApplicationCode = appCode;
			message.EM_MessageSubType = subType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2010, 10, 2);
			message.EM_MessageText = messageText.Replace("\r\n", "'");
			return message;
		}

		internal static T GetSentEDIMessage<T>(BusinessObjectFactory factory, string messageText, ZDateTime systemCreateTime, string messageNo = "") where T : EDIMessage
		{
			var ediMessage = GetEDIMessage<T>(factory, messageText, systemCreateTime);
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ediMessage.EM_Status = EDIMessage.Status.Sent;
			if (!(new ZString(messageNo)).IsEmpty)
			{
				ediMessage.EM_MessageNum = messageNo;
			}
			return ediMessage;
		}

		internal static T GetRecivedEDIMessage<T>(BusinessObjectFactory factory, string messageText, ZDateTime systemCreateTime) where T : EDIMessage
		{
			var ediMessage = GetEDIMessage<T>(factory, messageText, systemCreateTime);
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Received;
			return ediMessage;
		}

		static T GetEDIMessage<T>(BusinessObjectFactory factory, string messageText, ZDateTime systemCreateTime) where T : EDIMessage
		{
			var ediMessage = factory.New<T>();
			ediMessage.EM_MessageText = messageText.Replace("\r\n", "'");
			ediMessage.EM_SystemCreateTimeUtc = systemCreateTime;
			return ediMessage;
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = (EDIMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (SyntaxErrorMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<SyntaxErrorMessage>();
		}

		EDIMessage message;
		const string BlaText = "BLA";

		#region B3 Message Text

		internal const string B3MessageText = @"UNH+157+CUSDEC:S:99B:UN
BGM+:::AB+080+9
LOC+41+706
RFF+TN:000000250
RFF+ARA:123456789RM1234
TDT+11++9
DOC+785+9463TKWB1438742C
MOA+43:200.22
UNS+D
DMS+1
NAD+SE++ABC CANADA
DOC+935
DTM+129:20100930:102
LOC+27+CA+CA
PAT+1+CONSIGN:::02
MOA+6::CAD
CST+1+POS+1+8525800010+13
MOA+40:20000
MOA+43:20000
MOA+125:20000
RFF+MF::1
GIN+PN+TRANSMISSION APPARATUS FOR RADIO-BR:OADC
RFF+LI:1:1
MOA+38:20000
TAX+7+VAT++5.0
MOA+1:1000
GIR+1+1
MEA+AAR++NMB:20000
TAX+5+++0.0
MOA+155:000
UNS+S
TAX+7+:::K90
MOA+1:1000
TAX+4+:::K90
MOA+176:1000
UNT+36+157";

		internal const string B3SyntaxErrorMessageText = @"UNH+1+CUSRES:S:99B:UN+12345
BGM+:::000000250+080+11
DTM+137:201007071025:203
GIS+14
ERP+2:35:29
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:ELEM TOO LONG
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:INVALID CHARS
FTX+AAO+++SEGMENTGINLINE21ELE POS2,1:ELEM TOO LONG
FTX+AAO+++SEGMENTLOC- BYTE OFFSET256:MAND SEG MISSING
FTX+AAO+++BLA BLA
UNT+11+1";

		#endregion

		#region B3X Message Text

		internal const string B3XSyntaxErrorMessageText = @"UNH+1+CUSRES:S:99B:UN+12345
BGM+:::500000148++11
DTM+137:201007071025:203
GIS+14
ERP+2:35:29
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:ELEM TOO LONG
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:INVALID CHARS
FTX+AAO+++SEGMENTGINLINE21ELE POS2,1:ELEM TOO LONG
FTX+AAO+++SEGMENTLOC- BYTE OFFSET256:MAND SEG MISSING
FTX+AAO+++BLA BLA
UNT+11+1";

		#endregion

		#region Query Message Text

		internal const string QueryMessageText = @"UNH+158+CUSDEC:S:99B:UN
BGM+:::QA+081+9
RFF+ABD:85258000101
DTM+7:20100930:102
RFF+AFG:990
DTM+204:20100930:102
UNS+D
UNS+S
UNT+9+158";

		internal const string QuerySyntaxErrorMessageText = @"UNH+1+CUSRES:S:99B:UN+12345
BGM+:::+081+11
DTM+137:201007071025:203
GIS+14
ERP+2:35:29
FTX+AAO+++SEGMENTRFFLINE2ELE POS1,2:ELEM TOO LONG
FTX+AAO+++SEGMENTRFFLINE4ELE POS1,2:ELEM TOO SHORT
UNT+11+1";

		#endregion

		#region Supplementary Cargo Report Message Text

		internal const string SupplementaryCargoReportMessageText = @"UNH+55+GSMCAR:D:00A:UN:SUPRPT
BGM+85+S00043369+4
CST++687::96
TDT+20++1++8036
CNI+1
DOC+704+801027363636
RFF+ABE:8010S00001244D
LOC+8+CA:::VANCOUVER
GEI+6+:::22
TDT+12
RFF+AIJ:OBL1234566
NAD+CN+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA
NAD+CZ+++ABC IMPORTS PTY LTD+123 MAIN RD:DO NOT CHANGE ANY DETAILS. USED FOR+ALEXANDRIA+NSW+2015+AU
CTA+CO
COM+61266699999:TE
GID+1
PAC+1++PLT
FTX+AAA+++BOOKS
MEA+WT+AAE+KGM:1000
MEA+VOL+:::X+WSD:1.000
PCI++MARKS 1-1 MADE IN AU
CST++11+12+13+14+15
UNT+23+55";

		internal const string SupplementaryCargoReportSyntaxErrorMessageText = @"UNH+1+CUSRES:D:00A:UN
BGM+:::687+8010S00001244D+11
DTM+9:201107200202:203
GIS+14
ERP+2:55:28
ERC+ZZZ
FTX+AAO+++SEGMENTGEILINE8ELEM7364[2.4]INVALID CODE
FTX+AAO+++SEGMENTCSTBYTE OFFSETSEG USE EXCEEDED
UNT+8+1";

		#endregion

		#region eManifest House Bill Message Text

		internal const string EManifestHouseBillMessageText = @"UNH+377+GOVCBR:D:11B:UN:ACIHG
BGM+714+80363647474C+4
RFF+ABO:HBL-803636474747
DOC+23+:24
DOC+85+081-11111111
TDT+11++4
UNS+D
HYN+3
CNI+1
STS++0
MEA+AAX++MTQ:1
NAD+CN+++ABC CANADA+111 HURONTARIO STREET+TORONTO+ON+M5P 1A2+CA
CTA+AH
COM+ 1 (905) 555-1247:TE
NAD+CZ+++TREETOYS PTY LTD+105 WOMBAT DRIVE+KATOOMBA+NSW+2780+AU
CTA+IC+:FRED NERCK
CTA+AH
COM+ 61290251100:TE
LOC+8+0821+3380
TDT+1
SEQ+4
PAC+5
SEQ+4
PCI++SOME MARKS
GID+1
FTX+AAA+++GOODS DESCRIPTION
UNS+S
CNT+7:121
UNT+29+377
";

		internal const string EManifestHouseBillSyntaxErrorMessageText = @"UNH+1+GOVCBR:D:11B:UN
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
UNT+12+1";

		#endregion

		#endregion
	}
}
