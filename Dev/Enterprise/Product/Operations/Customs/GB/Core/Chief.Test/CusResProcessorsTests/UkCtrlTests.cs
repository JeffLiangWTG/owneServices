using Enterprise.Customs.GB.Chief.EdiFact.UKCTRL;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.Testing
{
	class UkCtrlTests : TestCase
	{
		public void TestParseSampleFromEdcsDocumentation()
		{
			string edifact = "UNH+98061514413578+UKCTRL:1:912:UK:109100'" +
								"UCM+SDI002:SDI001+CUSDEC:2:912:UN:109210'" +
								"UCX+4'" +
								"UCR+24'" +
								"UCD+5:1+6'" +
								"FTX+AAO+++E565 Commodity does not exist'" +
								"UNT+7+98061514413578'";

			UkctrlRegexParser parser = new UkctrlRegexParser();
			UkctrlMessage msg = parser.Parse(edifact);

			AssertEquals("98061514413578", msg.Header.UNH_SYS_MRN);
			AssertEquals("109210", msg.Header.UCM_MSG_ASG_CODE);
			AssertEquals("UN", msg.Header.UCM_MSG_CNTR_AGNCY);
			AssertEquals("SDI002", msg.Header.UCM_MSG_MRN);
			AssertEquals("912", msg.Header.UCM_MSG_REL_NO);
			AssertEquals("SDI001", msg.Header.UCM_MSG_SYS_CAR);
			AssertEquals("CUSDEC", msg.Header.UCM_MSG_TYPE);
			AssertEquals("2", msg.Header.UCM_MSG_VSN);
			AssertEquals("4", msg.Header.UCX_ACTION_CODE);
			AssertEquals("", msg.Header.UCX_MSG_ERROR_CODE);

			AssertEquals(1, msg.GroupTwos.Count);
			GroupTwo g2 = msg.GroupTwos[0];
			AssertEquals("24", g2.UCR_SEG_NO);
			AssertEquals("", g2.UCR_SEG_ERROR_CODE);
			AssertEquals("5", g2.UCD_ELEMENT_NO);
			AssertEquals("1", g2.UCD_COMPONENT_NO);
			AssertEquals("6", g2.UCD_DATA_ERROR_CODE);
			AssertEquals("E565 Commodity does not exist", g2.FTX_DATA_ERROR_TEXT1);
			AssertEquals("", g2.FTX_DATA_ERROR_TEXT2);
			AssertEquals("", g2.FTX_DATA_ERROR_TEXT3);
			AssertEquals("", g2.FTX_DATA_ERROR_TEXT4);
			AssertEquals("", g2.FTX_DATA_ERROR_TEXT5);
		}

		public void TestParseNakFromEacRequestNoteHowTheUcxActionCodeIsFour()
		{
			// Two group twos:
			string edifact = "UNH+09411285047242+UKCTRL:1:912:UK:109100'" +
						"UCM+292420:2S30ZNGYW+UKCINV:D:00A:UN:109001'" +
						"UCX+4+54'" +
						"UCR+0+54'" +
						"UCD+0+54'" +
						"FTX+AAO+++E10003 Errors on Document'" +
						"UCR+3'UCD+2:2+6'" +
						"FTX+AAI+++E1231 UCR / Part  has no associated Masters:Text two:It?'s 3?:00pm now:C?+?+ is great:Hi'" +  // Note FTX type is AAI, not the usual AAO
						"UNT+10+09411285047242'";

			UkctrlRegexParser parser = new UkctrlRegexParser();
			UkctrlMessage msg = parser.Parse(edifact);
			msg = parser.Parse(edifact);

			AssertEquals("09411285047242", msg.Header.UNH_SYS_MRN);
			AssertEquals("109001", msg.Header.UCM_MSG_ASG_CODE);
			AssertEquals("UN", msg.Header.UCM_MSG_CNTR_AGNCY);
			AssertEquals("292420", msg.Header.UCM_MSG_MRN);
			AssertEquals("00A", msg.Header.UCM_MSG_REL_NO);
			AssertEquals("2S30ZNGYW", msg.Header.UCM_MSG_SYS_CAR);
			AssertEquals("UKCINV", msg.Header.UCM_MSG_TYPE);
			AssertEquals("D", msg.Header.UCM_MSG_VSN);
			AssertEquals("4", msg.Header.UCX_ACTION_CODE);  // Note well
			AssertEquals("54", msg.Header.UCX_MSG_ERROR_CODE);

			AssertEquals(2, msg.GroupTwos.Count);
			GroupTwo firstGroup2 = msg.GroupTwos[0];
			AssertEquals("0", firstGroup2.UCR_SEG_NO);
			AssertEquals("54", firstGroup2.UCR_SEG_ERROR_CODE);
			AssertEquals("0", firstGroup2.UCD_ELEMENT_NO);
			AssertEquals("", firstGroup2.UCD_COMPONENT_NO);
			AssertEquals("54", firstGroup2.UCD_DATA_ERROR_CODE);
			AssertEquals("E10003 Errors on Document", firstGroup2.FTX_DATA_ERROR_TEXT1);
			AssertEquals("", firstGroup2.FTX_DATA_ERROR_TEXT2);
			AssertEquals("", firstGroup2.FTX_DATA_ERROR_TEXT3);
			AssertEquals("", firstGroup2.FTX_DATA_ERROR_TEXT4);
			AssertEquals("", firstGroup2.FTX_DATA_ERROR_TEXT5);

			GroupTwo secondGroup2 = msg.GroupTwos[1];
			AssertEquals("3", secondGroup2.UCR_SEG_NO);
			AssertEquals("", secondGroup2.UCR_SEG_ERROR_CODE);
			AssertEquals("2", secondGroup2.UCD_ELEMENT_NO);
			AssertEquals("2", secondGroup2.UCD_COMPONENT_NO);
			AssertEquals("6", secondGroup2.UCD_DATA_ERROR_CODE);
			AssertEquals("E1231 UCR / Part  has no associated Masters", secondGroup2.FTX_DATA_ERROR_TEXT1);
			AssertEquals("Text two", secondGroup2.FTX_DATA_ERROR_TEXT2);
			AssertEquals("It's 3:00pm now", secondGroup2.FTX_DATA_ERROR_TEXT3); // check the special chars!
			AssertEquals("C++ is great", secondGroup2.FTX_DATA_ERROR_TEXT4);  // check the special chars!
			AssertEquals("Hi", secondGroup2.FTX_DATA_ERROR_TEXT5);

			AssertEquals(false, msg.IsOverallSuccess);
		}

		public void TestParsePositiveAckResponseToEacRequestNoteHowTheUcxActionCodeIsOne()
		{
			string edifact = @"UNH+09411288914116+UKCTRL:1:912:UK:109100'" +
							 @"UCM+292423:2S30ZVR64+UKCINV:D:00A:UN:109001'" +
							 @"UCX+1'" +
							 @"UNT+4+09411288914116'";
			UkctrlRegexParser parser = new UkctrlRegexParser();
			UkctrlMessage msg = parser.Parse(edifact);

			AssertEquals("2S30ZVR64", msg.Header.UCM_MSG_SYS_CAR);
			AssertEquals("1", msg.Header.UCX_ACTION_CODE);  // Note well
			AssertEquals(0, msg.GroupTwos.Count);
			AssertEquals(true, msg.IsOverallSuccess);
		}

		public void TestParseMcpOldStyleInventoryErrorStrictlyWeWillNotSeeTheseButNeverthelessItIsStillAValidSampleWithWhichToTest()
		{
			// Now test MCP
			string mcpUkctrl = @"UNH#081014043958543#UKCTRL\1\912\UK\109100{" +
								@"UCM#13049\#CUSDEC\2\912\UN\109203{" +
								@"UCX#4{" +
								@"UCR#0{" +
								@"UCD#0#0{" +
								@"FTX#AAO###FCP0103 60103 - Invalid UVI{" +
								@"UNT#7#081014043958543{";
			UkctrlRegexParser parser = new UkctrlRegexParser();
			UkctrlMessage msg = parser.Parse(mcpUkctrl);

			AssertEquals("13049", msg.Header.UCM_MSG_MRN);
			AssertEquals("FCP0103 60103 - Invalid UVI", msg.GroupTwos[0].FTX_DATA_ERROR_TEXT1);
			AssertEquals("4", msg.Header.UCX_ACTION_CODE);
			AssertEquals("", msg.GroupTwos[0].UCR_SEG_ERROR_CODE);
			AssertEquals("0", msg.GroupTwos[0].UCR_SEG_NO);
			AssertEquals("0", msg.GroupTwos[0].UCD_ELEMENT_NO);
			AssertEquals("0", msg.GroupTwos[0].UCD_DATA_ERROR_CODE);
			AssertEquals("", msg.GroupTwos[0].UCD_COMPONENT_NO);
		}
	}
}
