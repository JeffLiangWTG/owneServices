using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(TransmitMessageInterpreter))]
class TransmitMessageInterpreterTest : MessageInterpreterAbstractTest<TransmitMessageInterpreter>
{
	protected override ZString ExpectMessageInterpretation => @"<style>
	body, p, td 
	{
		font-family: Microsoft Sans Serif; 
		font-size: 8.25pt;
		margin:4pt;
	}
</style>HREC&harr;ZZ&harr;USERSICEGATEID&harr;ZZ&harr;INNSA1&harr;ICES1_5&harr;P&harr;&harr;CMCHI01&harr;2035&harr;20220122&harr;1315<br>&lt;consoligm&gt;<br>&lt;consmaster&gt;<br>F&harr;AGSYE7618HCNDEL4&harr;INDEL4&harr;BD1232A&harr;16032022&harr;AI123&harr;20012023&harr;05517785456&harr;28012023&harr;BOM&harr;DEL&harr;T&harr;800&harr;1380.25&harr;Food<br>&lt;END-consmaster&gt;<br>&lt;conshouse&gt;<br>F&harr;AGSYE7618HCNDEL4&harr;INDEL4&harr;BD1232A&harr;16032022&harr;AI123&harr;20012023&harr;05517785456&harr;28012023&harr;HAWB1&harr;28012023&harr;BOM&harr;DEL&harr;T&harr;200&harr;380.25&harr;Coke<br>F&harr;AGSYE7618HCNDEL4&harr;INDEL4&harr;BD1232A&harr;16032022&harr;AI123&harr;20012023&harr;05517785456&harr;28012023&harr;HAWB2&harr;28012023&harr;BOM&harr;DEL&harr;T&harr;600&harr;985.66&harr;Cake<br>&lt;END-conshouse&gt;<br>&lt;END-consoligm&gt;<br>TREC&harr;2035";

	protected override TransmitMessageInterpreter CreateMessageInterpreterForTest()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ReceiveTransmit = Messaging.Business.EDIInterchange.Direction.Transmit;
		message.EM_MessageData = TestFileHelper.GetBytesFromEmbeddedResource("AirCGMSample.cgm");
		return new TransmitMessageInterpreter(message);
	}
}
