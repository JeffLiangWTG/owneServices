using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EXDOCInterchange))]
	public class EXDOCInterchangeTest : EDIInterchangeTest
	{
		public void TestFormattedInterchangeForHumansToReadIt()
		{
			EXDOCInterchange testInterchange = Factory.New<EXDOCInterchange>();
			testInterchange.EI_ApplicationCode = "EXD";
			testInterchange.EI_InterchangeType = "LDG";
			testInterchange.EI_ReceiveTransmit = "TRX";
			testInterchange.EI_HeaderText = "UNA.  UNBUNOB287061215095243EXDOCEDI1";
			testInterchange.EI_BodyText = "UNH4300SANCRTD97BUNRF0801BGMMAQ913LOC9SYDLOC12USTPALOC8NEW ORLEANSLOC36USLOC30AULOC91CBRRFFABEMEAT NO CONSIGNEE DRGISMAQPHCGISNAQSCGISNAQSMGISNAQSPGISNAQSSTGISNAQQIGISNAQACSPNAEX99999TDT122501ACT 10DTM13620061219102PRCINPPAQDTM31820061211102PNAFO77LIN1MEAAAASQKGM10000.000MEAAAIAALLBR26000PIA5FA  BP  CCPIA52450BPFTXABL1234567PAC5553CTAQPCIMIP/48/PHPRCSLPPAQDTM19420061209102DTM20620061209102PNAMP77PRCPKPPAQDTM19420061210102DTM20620061210102PNAMP77UNT394300";
			testInterchange.EI_FooterText = "UNZ143";
			AssertMultilineASCIIEquals("The output is formatted correctly", TestMessageFormatted, testInterchange.FormattedInterchangeForHumansToReadIt);
		}

		const string TestMessageFormatted = @"UNA:+.  '
UNB+UNOB:2+8+7+061215:0952+43++EXDOC+++EDI+1'
UNH+4300+SANCRT:D:97B:UN:RF0801'
BGM+M::AQ:9++13'
LOC+9+SYD'
LOC+12+USTPA'
LOC+8+NEW ORLEANS'
LOC+36+US'
LOC+30+AU'
LOC+91+CBR'
RFF+ABE:MEAT NO CONSIGNEE DR'
GIS+M::AQ:PHC'
GIS+N::AQ:SC'
GIS+N::AQ:SM'
GIS+N::AQ:SP'
GIS+N::AQ:SST'
GIS+N::AQ:QI'
GIS+N::AQ:ACS'
PNA+EX+99999'
TDT+12+250+1+++++:::ACT 10'
DTM+136:20061219:102'
PRC+IN:PP:AQ'
DTM+318:20061211:102'
PNA+FO+77'
LIN+1'
MEA+AAA+SQ+KGM:10000.000'
MEA+AAI+AAL+LBR:26000'
PIA+5+FA  BP  :CC'
PIA+5+2450:BP'
FTX+ABL+++1234567'
PAC+555+3+CT::AQ'
PCI++MIP/48/PH'
PRC+SL:PP:AQ'
DTM+194:20061209:102'
DTM+206:20061209:102'
PNA+MP+77'
PRC+PK:PP:AQ'
DTM+194:20061210:102'
DTM+206:20061210:102'
PNA+MP+77'
UNT+39+4300'
UNZ+1+43'";
	}
}
