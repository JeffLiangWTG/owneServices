using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC531C;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC531CProcessor))]
	class CC531CProcessorTest : EntryHeaderMessageProcessorTest<CC531CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC531Provider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Sent, entry.CH_Status);
			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"An Expiry Of Timer message has been received for supplementary declaration from Customs for Job {jobNumber} through the IE531 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Lodgement of Supplementary Declaration start date</td><td>18-Sep-71</td></tr><tr><td>Lodgement of Supplementary Declaration expiry date</td><td>19-Sep-71</td></tr><tr><td>Timer Expiry Information</td><td>Test Information 123</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
			var cusEntry = CusEntryNumber.LoadOrCreate(entry, Core.Constants.CountryCodes.Ireland);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), cusEntry.CE_ExpiryDate);
		}

		protected override ZString MessageFriendlyName => "CC531C: EXPIRY OF TIMER FOR SUPPLEMENTARY DECLARATION NOTIFICATION";

		protected override CC531CProcessor Processor => new CC531CProcessor(logger, typeof(Cc531C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE531;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardAESCC531CText("LRN123456789", "21IEDUB11A782454R2");
	}
}
