using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.DocumentWrappers.Ccsuk;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq.Protected;

namespace Enterprise.Customs.GB.DocumentWrappers.Test.CCSUK
{
	internal class CcsukWrapperForP5Test : TestCaseWithFactory
	{
		public void TestAllProperties()
		{
			PrepareMessage(
					@"UNH+ISR1FVBG3DKYO0+CUKFSA:1:912:BT'
BGM+:::P5+00746340000+7:9308101452:201'
DOC+740+00746340000+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::LHS:129:ZZZ'
TDT+12'
NAD+CB+DEU'
GDS+2'
QTY+118:10'
QTY+48:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
DOC+740+00746340000+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::BAR:129:ZZZ'
TDT+12'
NAD+CB+XYZ'
GDS+2'
QTY+118:10'
QTY+48:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
UNT+27+ISR1FVBG3DKYO0'");

			var wrapper = new CcsukWrapperForP5(receivedMessage, Factory);

			AssertEquals(string.Empty, wrapper.TempStorageDate);
			AssertEquals("LHRLHS-00746340000", wrapper.OldAWB);
			AssertEquals("LHRBAR-00746340000", wrapper.NewAWB);
			AssertEquals("DEU", wrapper.OldAgent);
			AssertEquals("XYZ", wrapper.NewAgent);
			AssertEquals("Shed", wrapper.ShedOrAirport);
		}

		public void TestAllPropertiesForInterAirport()
		{
			PrepareMessage(
					@"UNH+ISR1FVBG3DKYO0+CUKFSA:1:912:BT'
BGM+:::P5+00746340000+7:9308101452:201'
DOC+740+00746340000+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::LHS:129:ZZZ'
TDT+12'
NAD+CB+DEU'
GDS+2'
QTY+118:10'
QTY+48:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
DOC+740+00746340000+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:MIA:145:3::BAR:129:ZZZ'
TDT+12'
NAD+CB+XYZ'
GDS+2'
QTY+118:10'
QTY+48:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
UNT+27+ISR1FVBG3DKYO0'");

			var wrapper = new CcsukWrapperForP5(receivedMessage, Factory);

			AssertEquals(string.Empty, wrapper.TempStorageDate);
			AssertEquals("LHRLHS-00746340000", wrapper.OldAWB);
			AssertEquals("MIABAR-00746340000", wrapper.NewAWB);
			AssertEquals("DEU", wrapper.OldAgent);
			AssertEquals("XYZ", wrapper.NewAgent);
			AssertEquals("Airport", wrapper.ShedOrAirport);
		}

		void PrepareMessage(string input)
		{
			var mockEdiMessage = Factory.NewMoq<EDIMessage>();
			mockEdiMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("716");
			receivedMessage = mockEdiMessage.Object;
			receivedMessage.EM_MessageText = input.Replace(System.Environment.NewLine, "");
			receivedMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var receivedInterchange = Factory.New<EDIInterchange>();
			receivedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			receivedInterchange.EI_InterchangeNum = DateTime.Now.Ticks.ToString();
			receivedInterchange.EI_From = "Her Maj";
			receivedInterchange.EI_BodyText = "whatever";
			receivedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			receivedMessage.EM_EI = receivedInterchange.PK;
		}

		EDIMessage receivedMessage;
	}
}
