using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class EDIFACTMessageProcessorBaseOnlyTest : Customs.Business.MessageProcessors.Testing.EDIFACTMessageProcessorTest
	{
		public void TestGetMessageProcessor_InvalidFormat()
		{
			var processor = new EDIFACTMessageProcessorForTest(logger);
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = "ACI";
			message.EM_MessageType = "ACI";
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageSubType = "XXX";
			message.EM_IsTestMessage = true;
			message.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+714+802X10037468+9'RFF+AFM:5707:WH'RFF+UCN:'DOC+23+:24'DOC+85+918PSHGV5D596600'RCS+15'FTX+ACB+++'TDT+11++1'UNS+D'HYN+3'CNI+1'STS++0'MEA+AAX++MTQ:3'HAN+:::'NAD+CN+++VEONEER CANADA SAFETY SYSTEMS INC+1 GERMAIN ST SUITE 1500+++SAINT JOHN+NB+E2L 4V1+CA'CTA+IC+:KENNY'CTA+AH'COM+:TE'NAD+CZ+++GETAC PRECISION TECHNOOGY (CHANGSU)CO LTD+67 XIONGYING ROAD+++KUNSHAN CITY+++++CN'CTA+IC+:'CTA+AH'COM+:TE'NAD+DP+++LEAN SUPPLY SOLUTIONS+191 MCNABB ST:ATTN?: BASKEY KANDASAMY++MARKHAM+ON+L3R 8H2+CA'CTA+IC+:'CTA+AH'COM+?+16475235475:TE'NAD+ZZZ++++++++++++'CTA+IC+:'CTA+AH'COM+:TE'NAD+PK+++'CTA+AH'COM+:TE'LOC+8+0497+5707'LOC+11+0809+3037'DOC+714'NAD+CS++++++++++++'CTA+IC+:'CTA+AH'COM+:TE'RCS+15'FTX+AAC+++'EQD+CN+CAIU9919530'SEQ+4'SEL+SML193661'SEQ+4'TDT+1'SEQ+4'PAC+3++:::PKG'SEQ+4'PCI++N/M'GID+1'FTX+AAA+++COVER, MAZDA 7GEN'TCC+++:SRZ'UNS+S'CNT+7:836:KGM'UNT+58+1";

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var messageProcessor = processor.GetMessageProcessor_Exposed(message);
			AssertType<SyntaxEDIFACTMessageHandler>(messageProcessor);
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("Expected no more than 10 values, but was 12 in NADSegment"));
		}
	}

	class EDIFACTMessageProcessorForTest : EDIFACTMessageProcessor
	{
		public EDIFACTMessageProcessorForTest(LoggingInformation logger)
			: base(logger)
		{
		}

		public CustomsMessageProcessor GetMessageProcessor_Exposed(Enterprise.Messaging.Business.EDIMessage ediMessage) => base.GetMessageProcessor(ediMessage);

		protected override string ApplicationCodeCore => string.Empty;

		protected override string MessageFriendlyNameCore => string.Empty;
	}
}
