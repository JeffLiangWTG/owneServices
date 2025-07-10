using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRIMDMessage))]
	sealed class CMRIMDMessageTest : CMRCUSDECMessageTest
	{
		public void TestMaxLineNumber()
		{
			var message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = iMDWithTwoPackingLinesText;

			AssertEquals((ZShort)2, message.MaxLineNumber);
		}

		public void TestIPaymentIncluded()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = iMDWithPaymentText;
			AssertNotNull("IsPaymentIncluded GIS segment", ((IPaymentIncluded)message).GISSegments);
		}

		public void TestBankDetailsInMessage()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = iMDWithPaymentText.Replace("FII+COQ+323232+:::242200::215", "FII+COQ+323232:BANK ACCOUNT NAME TEXT+:::242200::215");
			AssertEquals("Payment Party", PaymentParty.Broker, message.PaymentPartyInMessage);
			AssertEquals("BankAccountNameInMessage", "BANK ACCOUNT NAME TEXT", message.BankAccountNameInMessage);
			AssertEquals("BankAccountNoInMessage", "323232", message.BankAccountNoInMessage);
			AssertEquals("BSBInMessage", "242200", message.BSBInMessage);
			message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = iMDWithPaymentText.Replace("RFF+ANU:B'", "RFF+ANU:I'");
			AssertEquals("Payment Party", PaymentParty.Importer, message.PaymentPartyInMessage);
		}

		public void TestIsPrelodgeMessage()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = "TEST";
			AssertEquals("It is pre-lodge", false, message.IsPreLodgeMessage);

			message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = iMDPreLodgeMessageText;
			AssertEquals("It is pre-lodge", true, message.IsPreLodgeMessage);
		}

		public void TestIsOriginal()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = iMDPreLodgeMessageText;
			AssertEquals("IsOriginalMessage", true, message.IsOriginalMessage);
		}

		public void TestIsAmendment()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = iMDPreLodgeMessageText.Replace(":7+9'", ":7+4'");
			AssertEquals("IsAmendmentMessage", true, message.IsAmendmentMessage);
		}

		#region Implementation

		protected override ZString GetSampleWithdrawMessage()
		{
			return iMDPreLodgeMessageText.Replace(":7+9'", ":7+50'");
		}

		readonly string iMDWithTwoPackingLinesText = @"UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+S3IAKL100001904/1/CMT1:2+4'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+NZAKL::6'LOC+79+AUSYD::6'DTM+178:20230316:102'DTM+260:20230316:102'DTM+252:20230316:102'GIS+Y:153:95'GIS+POR:109:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:1000.00000'FTX+DEL+++AMY TEST'FTX+CHG+++YFUJKH'RFF+ABQ:ADGHFDGJN'RFF+ADU:S3IAKL100001904/1'RFF+APH:FOB'RFF+AMG:0011'RFF+ADP:0008::Y'RFF+ADP:0009::Y'RFF+ABT:AAAGC3H7F'TDT+20+00001+S+++++9175793::11'NAD+AT+41065894724::95'NAD+WP+001::95'NAD+VT+AA33HF::95'NAD+DP++MASCOT++UNIT 23 635 GARDENERS ROAD++:::NSW+2020+AU'MOA+63:2000.00:AUD'MOA+141:2044.91:AUD'MOA+39:2000.00:AUD'MOA+313:20.00:USD'MOA+71:10.00:USD'UNS+D'DMS+1'LIN+1+A'PAC+++LCL:67:95'PAC+1+1'PCI+1'RFF+AAQ:MAEU5481202'PCI+1'RFF+MB:OBOL123'PCI+1'RFF+BH:JGHTFTKJGFH'LIN+2+I'PAC+++LCL:67:95'PAC+2+1'PCI+1'RFF+AAQ:MAEU5481202'PCI+1'RFF+MB:OBOL123'PCI+1'RFF+BH:JFGHJKMHJG'CST+1+A::95+N10::95'FTX+AAA+++MOTORS OF AN OUTPUT NOT EXCEEDING 37.5 W'LOC+27+NZ::5'MEA+AAA++NO:1.00000'NAD+VN+123456789012::95'NAD+SU+AAA3336647M::95'MOA+38:1000.00:AUD'RFF+ABD:85011000'RFF+AED:32'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:MAEU5481202'RFF+AFV:TV'CST+2+A::95+N10::95'FTX+AAA+++MOTORS OF AN OUTPUT NOT EXCEEDING 37.5 W'LOC+27+NZ::5'MEA+AAA++NO:1.00000'NAD+VN+123456789012::95'NAD+SU+AAA3336647M::95'MOA+38:400.00:AUD'RFF+ABD:85011000'RFF+AED:32'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:MAEU5481202'RFF+AFV:TV'CST+3+A::95+N10::95'FTX+AAA+++OF AN OUTPUT NOT EXCEEDING 75 KVA'LOC+27+NZ::5'MEA+AAA++NO:1.00000'NAD+VN+123456789012::95'NAD+SU+AAA3336647M::95'MOA+38:600.00:AUD'RFF+ABD:85021100'RFF+AED:36'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:MAEU5481202'RFF+AFV:TV'UNS+S'UNT+95+1'";

		readonly string iMDWithPaymentText = @"UNH+1+CUSDEC:D:99B:UN'
BGM+929:::IMD+B00148337/1/SYD2:2+9'
CST++N10::95'
LOC+8+AUSYD::6'
LOC+12+AUSYD::6'
LOC+9+USLAX::6'
LOC+79+AUSYD::6'
DTM+178:20050831:102'
DTM+260:20050801:102'
DTM+252:20050831:102'
GIS+EPA:109:95'
GIS+Y:153:95'
GIS+LLB:109:95'
GIS+TLB:109:95'
FII+COQ+323232+:::242200::215'
MEA+AAE+G+KG:200.00000'
FTX+DEL+++TREETOYS PTY LTD'
RFF+ABQ:SC DEPOT TEST'
RFF+ADU:sc depot test'
RFF+ANU:B'
RFF+APH:FOB'
RFF+ADP:0009::Y'
RFF+AMG:0001'
RFF+ADP:0008::Y'
TDT+20+S1234+S+++++9044748::11'
NAD+AT+81001682024::95'
NAD+VT+AA33HF::95'
NAD+DP++KATOOMBA++105 WOMBAT DRIVE++:::NSW+2780+AU'
NAD+CB+54321::95'
MOA+63:500.00:AUD'
MOA+141:1000.00:AUD'
MOA+39:500.00:AUD'
MOA+313:200.00:AUD'
MOA+71:300.00:AUD'
UNS+D'
DMS+1'
LIN+1+I'
PAC+++LCL:67:95'
PAC+200+1'
PAC+1+3'
PCI+1'
RFF+AAQ:OCLU2324286'
PCI+1'
RFF+MB:UBMTESTB'
CST+1+I::95+N10::95'
FTX+AAA+++OF OTHER PLASTICS'
LOC+27+US::5'
NAD+SU+AAA3336647M::95'
PAC++1'
PCI+1'
FTX+RAH+++00150:00165:N'
PCI+1'
FTX+RAH+++00160:00175:N'
PCI+1'
FTX+RAH+++00151:00166:N'
MOA+38:500.00:AUD'
MOA+68:500.00:AUD'
MOA+5:30.00:AUD'
RFF+ABD:39269090'
RFF+AED:88'
RFF+AFV:TV'
UNS+S'
UNT+63+1'".Replace("\r\n", "");

		readonly string iMDPreLodgeMessageText = @"UNH+1+CUSDEC:D:99B:UN'
BGM+929:::IMD+B00148229/1/SYD7:7+9'
CST++N20::95'
LOC+8+AUSYD::6'
LOC+12+AUSYD::6'
LOC+9+SGSIN::6'
LOC+79+AUSYD::6'
DTM+178:20050610:102'
DTM+252:20050610:102'
DTM+260:20050610:102'
GIS+N:153:95'
GIS+PRE:109:95'
MEA+AAE+G+KG:150.00000'
FTX+DEL+++DELIVERY NAME'
RFF+ABQ:OWNERS REF'
RFF+ADU:B00148229'
RFF+APH:FOB'
TDT+20++A++QF::3'
NAD+AT+51006765546::95'
NAD+VT+AA33HF::95'
NAD+DP++CANBERRA++DELIVERY ADDRESS LINE 1::DELIVERY ADDRESS LINE 2++:::ACT+2601+AU'
NAD+CB+54321::95'
MOA+63:15000.00:AUD'
MOA+141:15000.00:AUD'
MOA+39:15000.00:AUD'
UNS+D'
DMS+1'
LIN+1+I'
PAC+120+1'
PCI+1'
RFF+MWB:08155555555'
PCI+1'
RFF+HWB:HOUSE BILL'
CST+1+I::95+N20::95'
FTX+AAA+++FLOOR TILES'
LOC+27+AU::5'
MEA+AAA++SM:150.00'
NAD+SU+66015286036::95'
MOA+38:15000.00:AUD'
RFF+ABD:39181000'
RFF+AED:23'
RFF+AFV:TV'
UNS+S'
UNT+44+1'".Replace("\r\n", "");

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CMRIMDMessage result = (CMRIMDMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CMRIMDMessage>();
		}

		#endregion

	}
}
