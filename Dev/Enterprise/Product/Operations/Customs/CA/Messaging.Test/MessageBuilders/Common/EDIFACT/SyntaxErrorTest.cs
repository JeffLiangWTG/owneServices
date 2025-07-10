using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CA.Messaging.Testing
{
	sealed class SyntaxErrorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestProperties()
		{
			CombineAssertions("Properties from CUSRES:D:00A", () =>
			{
				var expectedInterpretationText = "Error Message: ELEM TOO LONG,  Component Value: '', Segment/Position: NAD / L: 11, P: 6,0.";
				var error = new SyntaxError(D00AMessageText.Replace("\r\n", "'"), "SEGMENTNADLINE11ELEM3164[6.0]ELEM TOO LONG");
				NUnit.Framework.Assert.That(error.LineNumber, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison), "LineNumber");
				NUnit.Framework.Assert.That(error.ErrorMessage, NUnit.Framework.Is.EqualTo("ELEM TOO LONG").Using(CustomComparers.TypeComparison), "ErrorMessage");
				NUnit.Framework.Assert.That(error.ComponentValue, NUnit.Framework.Is.EqualTo(ZString.Empty), "ComponentValue");
				NUnit.Framework.Assert.That(error.SourceLine, NUnit.Framework.Is.EqualTo("NAD+CN+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA").Using(CustomComparers.TypeComparison), "SourceLine");
				NUnit.Framework.Assert.That(error.SegmentPosition, NUnit.Framework.Is.EqualTo("NAD / L: 11, P: 6,0").Using(CustomComparers.TypeComparison), "SegmentPosition");
				NUnit.Framework.Assert.That(error.ToString().Replace("\t", ""), CustomConstraints.MultilineASCIIEquals(string.Format(expectedInterpretationText, "##")), "Syntax error text");
			});

			CombineAssertions("Properties from CUSRES:S:99B", () =>
			{
				var expectedInterpretationText = "Error Message: :MAND ELEM MISSING,  Component Value: '706', Segment/Position: LOC / L: 2, P: 2,1.";
				var error = new SyntaxError(S99BMessageText.Replace("\r\n", "'"), "SEGMENTLOCLINE2ELE POS2,1:MAND ELEM MISSING");
				NUnit.Framework.Assert.That(error.LineNumber, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "LineNumber");
				NUnit.Framework.Assert.That(error.ErrorMessage, NUnit.Framework.Is.EqualTo(":MAND ELEM MISSING").Using(CustomComparers.TypeComparison), "ErrorMessage");
				NUnit.Framework.Assert.That(error.ComponentValue, NUnit.Framework.Is.EqualTo("706").Using(CustomComparers.TypeComparison), "ComponentValue");
				NUnit.Framework.Assert.That(error.SourceLine, NUnit.Framework.Is.EqualTo("LOC+41+706").Using(CustomComparers.TypeComparison), "SourceLine");
				NUnit.Framework.Assert.That(error.SegmentPosition, NUnit.Framework.Is.EqualTo("LOC / L: 2, P: 2,1").Using(CustomComparers.TypeComparison), "SegmentPosition");
				NUnit.Framework.Assert.That(error.ToString().Replace("\t", ""), CustomConstraints.MultilineASCIIEquals(string.Format(expectedInterpretationText, "##")), "Syntax error text");
			});

			CombineAssertions("Properties from GOVCBR:D:11B", () =>
			{
				var expectedInterpretationText = "Error Message: COND ELEM MISSING,  Component Value: '', Segment/Position: DOC / L: 4, P: 2,0.";
				var error = new SyntaxError(D11BMessageText.Replace("\r\n", "'"), "UMRN(2269)SEGMENTDOCLINE4ELEM1004(2.0)COND ELEM MISSING");
				NUnit.Framework.Assert.That(error.LineNumber, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison), "LineNumber");
				NUnit.Framework.Assert.That(error.ErrorMessage, NUnit.Framework.Is.EqualTo("COND ELEM MISSING").Using(CustomComparers.TypeComparison), "ErrorMessage");
				NUnit.Framework.Assert.That(error.ComponentValue, NUnit.Framework.Is.EqualTo(ZString.Empty), "ComponentValue");
				NUnit.Framework.Assert.That(error.SourceLine, NUnit.Framework.Is.EqualTo("DOC+85+081-11111111").Using(CustomComparers.TypeComparison), "SourceLine");
				NUnit.Framework.Assert.That(error.SegmentPosition, NUnit.Framework.Is.EqualTo("DOC / L: 4, P: 2,0").Using(CustomComparers.TypeComparison), "SegmentPosition");
				NUnit.Framework.Assert.That(error.ToString().Replace("\t", ""), CustomConstraints.MultilineASCIIEquals(string.Format(expectedInterpretationText, "##")), "Syntax error text");
			});
		}

		const string D11BMessageText = @"UNH+377+GOVCBR:D:11B:UN:ACIHG
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

		const string D00AMessageText = @"UNH+55+GSMCAR:D:00A:UN:SUPRPT
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

		const string S99BMessageText = @"UNH+157+CUSDEC:S:99B:UN
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
	}
}
