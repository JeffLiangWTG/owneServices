using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(EDIMessage))]
sealed class EDIMessageTest : Messaging.Testing.EDIMessageTest
{
	public void TestLookups()
	{
		AssertType<EDIMessageLookups>(TestMessage.Lookups);
	}

	public void TestMessageDefaults()
	{
		CombineAssertions(() =>
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var testMessage = TestMessage;
			AssertEquals("Default EM_ApplicationCode", ApplicationCodeList.Codes.INCustoms, testMessage.EM_ApplicationCode);
			AssertEquals("Default EM_IsTestMessage when PROD env", expected: false, testMessage.EM_IsTestMessage);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
			testMessage = CreateNewMessage();
			AssertEquals("Default EM_IsTestMessage when TEST env", expected: true, testMessage.EM_IsTestMessage);
		});
	}

	public void TestGetMessageReferenceNumberUsingEM_MessageNum()
	{
		CombineAssertions(() =>
		{
			var message1 = CreateNewMessageWithTestData();
			AssertNullOrEmpty(message1.EM_MessageNum);
			message1.AssignMessageNumber();
			AssertEquals("First message", "0000001", message1.EM_MessageNum);

			var message2 = CreateNewMessageWithTestData();
			message2.AssignMessageNumber();
			AssertEquals("Second message", "0000002", message2.EM_MessageNum);
		});
	}

	public void TestMessageNumberPlaceHolderOverrideUsingEM_MessageText()
	{
		var message = CreateNewMessageWithTestData();
		message.EM_MessageText = $"Test message - {Constants.Messaging.INMessageNumPlaceHolder}";
		message.AssignMessageNumber();
		AssertEquals("Test message - 0000001", message.EM_MessageText);
	}

	public void TestShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride()
	{
		var message = CreateNewMessageWithTestData();
		AssertEquals(ZBool.True, message.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverrideExposed());
	}

	public void TestMessageStreamFormatter()
	{
		var message = CreateNewMessageWithTestData();
		AssertNull(message.MessageStreamFormatterExposed);
	}

	public void TestUpdateMessageAttacheeMessageStatusOnSaving()
	{
		CombineAssertions("CH_Status check", () =>
		{
			var message = CreateNewMessageWithTestData();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			message.EM_ReceiveTransmit = Messaging.Business.EDIMessage.Direction.Transmit;
			message.EM_LinkedObject = entryHeader;
			message.EM_Status = MessageStatusList.Codes.MessageQueued;
			Factory.Save();
			AssertEquals("When message queued", ZString.Empty, entryHeader.CH_Status);

			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			Factory.Save();
			AssertEquals("When message processed successfull", MessageStatusList.Codes.MessageSent,  entryHeader.CH_Status);

			message.EM_Status = EDIMessageStatusList.Codes.Failed;
			Factory.Save();
			AssertEquals("When message processed fail", MessageStatusList.Codes.MessageDeliveryFailed, entryHeader.CH_Status);

			message = CreateNewMessageWithTestData();
			message.EM_ReceiveTransmit = Messaging.Business.EDIMessage.Direction.Receive;
			message.EM_LinkedObject = entryHeader;
			message.EM_Status = MessageStatusList.Codes.MessageQueued;
			Factory.Save();

			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			Factory.Save();
			AssertEquals("CH_Status not update on process receive message", MessageStatusList.Codes.MessageDeliveryFailed, entryHeader.CH_Status);
		});
	}

	public void TestEM_MessageInterpretationTRX()
	{
		AssertEM_MessageInterpretation(Messaging.Business.EDIInterchange.Direction.Transmit, TestFileHelper.GetBytesFromEmbeddedResource("AirCGMSample.cgm"), @"<style>
	body, p, td 
	{
		font-family: Microsoft Sans Serif; 
		font-size: 8.25pt;
		margin:4pt;
	}
</style>HREC&harr;ZZ&harr;USERSICEGATEID&harr;ZZ&harr;INNSA1&harr;ICES1_5&harr;P&harr;&harr;CMCHI01&harr;2035&harr;20220122&harr;1315<br>&lt;consoligm&gt;<br>&lt;consmaster&gt;<br>F&harr;AGSYE7618HCNDEL4&harr;INDEL4&harr;BD1232A&harr;16032022&harr;AI123&harr;20012023&harr;05517785456&harr;28012023&harr;BOM&harr;DEL&harr;T&harr;800&harr;1380.25&harr;Food<br>&lt;END-consmaster&gt;<br>&lt;conshouse&gt;<br>F&harr;AGSYE7618HCNDEL4&harr;INDEL4&harr;BD1232A&harr;16032022&harr;AI123&harr;20012023&harr;05517785456&harr;28012023&harr;HAWB1&harr;28012023&harr;BOM&harr;DEL&harr;T&harr;200&harr;380.25&harr;Coke<br>F&harr;AGSYE7618HCNDEL4&harr;INDEL4&harr;BD1232A&harr;16032022&harr;AI123&harr;20012023&harr;05517785456&harr;28012023&harr;HAWB2&harr;28012023&harr;BOM&harr;DEL&harr;T&harr;600&harr;985.66&harr;Cake<br>&lt;END-conshouse&gt;<br>&lt;END-consoligm&gt;<br>TREC&harr;2035");
	}

	public void TestEM_MessageInterpretationRCV()
	{
		AssertEM_MessageInterpretation(Messaging.Business.EDIInterchange.Direction.Receive, TestFileHelper.GetBytesFromEmbeddedResource("AirCGMSample.eml"), @"<style>
	body, p, td 
	{
		font-family: Microsoft Sans Serif; 
		font-size: 8.25pt;
		margin:4pt;
	}
</style>Filling status - control no. 0000024, filing date 20240909, Receiver ID INBLR4, Message ID CMCHI01
<hr><br><br>Dear Sir/Madam,<br><br>There was an error in processing your file with control no. 1200024, filing date 20240905, Receiver ID INBLR4, Message ID CMCHI01.<br>Please try again.<br><br>Regards,<br>ICEGATE Support Team<br><br>
<hr><br>Attachment<br><br>HREC&harr;ZZ&harr;INBOM4&harr;ZZ&harr;LIPLINDIA&harr;ICES1_5&harr;P&harr;&harr;CHCMI02&harr;3447163&harr;20240108&harr;1106<br>&lt;consoligm&gt;<br>&lt;consack&gt;  <br>INBOM4&harr;2414523&harr;07012024&harr;MH194&harr;07012024&harr;23257531084&harr;07012024&harr;&harr;&harr;&harr;&harr;000<br>INBOM4&harr;2414523&harr;07012024&harr;MH194&harr;07012024&harr;23257531084&harr;07012024&harr;&harr;07012024&harr;&harr;&harr;000<br>&lt;END-consack&gt;<br>TREC&harr;3447163<br><br><br><hr><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><th>MAWB</th><th>HAWB</th><th>Response</th></tr><tr><td>23257531084</td><td>&nbsp;</td><td>000 - Response code description not found</td></tr><tr><td>23257531084</td><td>&nbsp;</td><td>000 - Response code description not found</td></tr></table>");
	}

	void AssertEM_MessageInterpretation(ZString receiveTransmit, byte[] messageData, ZString expectMessageInterpretation)
	{
		var message = CreateNewMessageWithTestData();
		message.EM_ReceiveTransmit = receiveTransmit;
		message.EM_MessageData = messageData;
		AssertEquals(expectMessageInterpretation, message.EM_MessageInterpretation);
	}

	EDIMessageForTest CreateNewMessageWithTestData()
	{
		var message = CreateNewMessage();
		message.EM_MessageType = EDIMessageTypeList.Codes.ShippingBill;
		message.EM_MessageSubType = EDIMessageTypeList.Codes.ShippingBill + DeclarationMessageTypeList.Codes.Fresh;
		message.EM_MessageOwner = "SampleMessageOwner";
		return message;
	}

	EDIMessageForTest TestMessage => testMessage ??= CreateNewMessage();
	EDIMessageForTest testMessage;

	EDIMessageForTest CreateNewMessage() => Factory.New<EDIMessageForTest>();

	class EDIMessageForTest : EDIMessage
	{
		public EDIMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverrideExposed()
			=> base.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride();

		public Messaging.Business.IStreamFormatter MessageStreamFormatterExposed => base.MessageStreamFormatter;
	}
}
