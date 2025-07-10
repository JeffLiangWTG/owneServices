using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ReceiveMessageInterpreter))]
class ReceiveMessageInterpreterTest : MessageInterpreterAbstractTest<ReceiveMessageInterpreter>
{
	protected override ZString ExpectMessageInterpretation => @"<style>
	body, p, td 
	{
		font-family: Microsoft Sans Serif; 
		font-size: 8.25pt;
		margin:4pt;
	}
</style>Filling status - control no. 0000024, filing date 20240909, Receiver ID INBLR4, Message ID CMCHI01
<hr><br><br>Dear Sir/Madam,<br><br>There was an error in processing your file with control no. 1200024, filing date 20240905, Receiver ID INBLR4, Message ID CMCHI01.<br>Please try again.<br><br>Regards,<br>ICEGATE Support Team<br><br>
<hr><br>Attachment<br><br>HREC&harr;ZZ&harr;INBOM4&harr;ZZ&harr;LIPLINDIA&harr;ICES1_5&harr;P&harr;&harr;CHCMI02&harr;3447163&harr;20240108&harr;1106<br>&lt;consoligm&gt;<br>&lt;consack&gt;  <br>INBOM4&harr;2414523&harr;07012024&harr;MH194&harr;07012024&harr;23257531084&harr;07012024&harr;&harr;&harr;&harr;&harr;000<br>INBOM4&harr;2414523&harr;07012024&harr;MH194&harr;07012024&harr;23257531084&harr;07012024&harr;&harr;07012024&harr;&harr;&harr;000<br>&lt;END-consack&gt;<br>TREC&harr;3447163<br><br><br><hr><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><th>MAWB</th><th>HAWB</th><th>Response</th></tr><tr><td>23257531084</td><td>&nbsp;</td><td>000 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>000 - Response code description not found</td></tr></table>";

	ZString ExpectNotValidAttachmentMessageInterpretation => @"<style>
	body, p, td 
	{
		font-family: Microsoft Sans Serif; 
		font-size: 8.25pt;
		margin:4pt;
	}
</style>Filling status - control no. 0000024, filing date 20240909, Receiver ID INBLR4, Message ID CMCHI01
<hr><br><br>Dear Sir/Madam,<br><br>There was an error in processing your file with control no. 1200024, filing date 20240905, Receiver ID INBLR4, Message ID CMCHI01.<br>Please try again.<br><br>Regards,<br>ICEGATE Support Team<br><br>
<hr><br>Attachment<br><br>HREC&harr;ZZ&harr;INBOM4&harr;ZZ&harr;LIPLINDIA&harr;ICES1_5&harr;P&harr;&harr;CHCMI21A&harr;3447163&harr;20240108&harr;1106<br>&lt;consoligm&gt;<br>&lt;consack&gt;  <br>INBOM4&harr;2414523&harr;07012024&harr;MH194&harr;07012024&harr;23257531084&harr;07012024&harr;&harr;&harr;&harr;&harr;000<br>&lt;END-consack&gt;<br>TREC&harr;3447163<hr><br>";

	ZString ExpectMultipleErrorCodesMessageInterpretation => @"<style>
	body, p, td 
	{
		font-family: Microsoft Sans Serif; 
		font-size: 8.25pt;
		margin:4pt;
	}
</style>Filling status - control no. 0000024, filing date 20240909, Receiver ID INBLR4, Message ID CMCHI01
<hr><br><br>Dear Sir/Madam,<br><br>There was an error in processing your file with control no. 1200024, filing date 20240905, Receiver ID INBLR4, Message ID CMCHI01.<br>Please try again.<br><br>Regards,<br>ICEGATE Support Team<br><br>
<hr><br>Attachment<br><br>HREC&harr;ZZ&harr;INBOM4&harr;ZZ&harr;LIPLINDIA&harr;ICES1_5&harr;P&harr;&harr;CHCMI02&harr;3447163&harr;20240108&harr;1106<br>&lt;consoligm&gt;<br>&lt;consack&gt;  <br>INBOM4&harr;2414523&harr;07012024&harr;MH194&harr;07012024&harr;23257531084&harr;07012024&harr;&harr;&harr;&harr;&harr;001,002,036<br>INBOM4&harr;2414523&harr;07012024&harr;MH194&harr;07012024&harr;23257531084&harr;07012024&harr;&harr;07012024&harr;&harr;&harr;001 002 036<br>INBOM4&harr;2414523&harr;07012024&harr;MH194&harr;07012024&harr;23257531084&harr;07012024&harr;&harr;&harr;&harr;&harr;001;002;036<br>INBOM4&harr;2414523&harr;07012024&harr;MH194&harr;07012024&harr;23257531084&harr;07012024&harr;&harr;07012024&harr;&harr;&harr;001|002|036<br>&lt;END-consack&gt;<br>TREC&harr;3447163<hr><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><th>MAWB</th><th>HAWB</th><th>Response</th></tr><tr><td>23257531084</td><td>&nbsp;</td><td>001 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>002 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>036 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>001 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>002 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>036 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>001 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>002 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>036 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>001 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>002 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>036 - Response code description not found</td></tr></table><hr><br>Attachment<br><br>HREC&harr;ZZ&harr;INBOM4&harr;ZZ&harr;LIPLINDIA&harr;ICES1_5&harr;P&harr;&harr;CHCMI02&harr;3447163&harr;20240108&harr;1106<br>&lt;consoligm&gt;<br>&lt;consack&gt;  <br>&lt;END-consack&gt;<br>TREC&harr;3447163<hr><br>";

	protected override ReceiveMessageInterpreter CreateMessageInterpreterForTest() => CreateMessageInterpreter("AirCGMSample.eml");

	public void TestGetMessageInterpretationForNotValidAttachment()
	{
		AssertEquals(ExpectNotValidAttachmentMessageInterpretation.ToString(), ((IMessageInterpreter)CreateMessageInterpreter("SeaCGMSample.eml")).GetMessageInterpretation().ToString());
	}

	public void TestGetMessageInterpretationForMultipleErrorCodes()
	{
		AssertEquals(ExpectMultipleErrorCodesMessageInterpretation.ToString(), ((IMessageInterpreter)CreateMessageInterpreter("AirCGMSampleWithMultipleErrorCode.eml")).GetMessageInterpretation().ToString());
	}

	ReceiveMessageInterpreter CreateMessageInterpreter(string emailFileName)
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ReceiveTransmit = Messaging.Business.EDIInterchange.Direction.Receive;
		message.EM_MessageData = TestFileHelper.GetBytesFromEmbeddedResource(emailFileName);
		return new ReceiveMessageInterpreter(message);
	}
}
