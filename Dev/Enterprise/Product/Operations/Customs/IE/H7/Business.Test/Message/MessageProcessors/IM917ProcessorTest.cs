using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM917;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM917Processor))]
	class IM917ProcessorTest : AISH7MessageProcessorTest<IM917Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM917Provider>
	{
		protected override IM917Processor Processor => new IM917Processor(logger, typeof(Im917));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM917;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => "IM917: Syntax Error Notification";

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", LogicalStatusList.Codes.Error, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", string.Empty, messageAttachee.ABL_BillStatus);
			AssertEquals("Message status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

			AssertMessageInterpretation(incomingMessage, @"A Syntax Error Notification (IM917) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Line Number</td><td>1</td></tr><tr><td>Error Reason</td><td>BAD</td></tr><tr><td>Error Column Number</td><td>2</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Line Number</td><td>3</td></tr><tr><td>Error Reason</td><td>THRILLER</td></tr><tr><td>Error Column Number</td><td>4</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
			new Im917
			{
				XmlNegativeAcknowledgement = new System.Collections.ObjectModel.Collection<XmlNegativeAcknowledgement>(new[]
				{
					new XmlNegativeAcknowledgement
					{
						ErrorLineNumber = "1",
						ErrorColumnNumber = "2",
						ErrorReason = "BAD"
					},
					new XmlNegativeAcknowledgement
					{
						ErrorLineNumber = "3",
						ErrorColumnNumber = "4",
						ErrorReason = "THRILLER"
					}
				})
			});
		}
	}
}
