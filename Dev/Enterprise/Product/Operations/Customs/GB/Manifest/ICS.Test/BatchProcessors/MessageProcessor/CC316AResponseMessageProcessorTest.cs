using CargoWise.Customs.GB.MessageDefinitions.ICS.CC316A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS.Testing
{
	sealed class CC316AResponseMessageProcessorTest : IcsSsGreatBritainResponseMessageProcessorBaseTest<CC316AResponseMessageProcessor, Cc316AType>
	{
		protected override ZString MessageTypeToProcess => IcsSsGreatBritainEDIMessageTypeList.Codes.CC316A;

		protected override ZString MessageTextToProcess =>
@"<CC316A>
  <MesSenMES3>Token1</MesSenMES3>
  <MesRecMES6>Token1</MesRecMES6>
  <DatOfPreMES9>Token1</DatOfPreMES9>
  <TimOfPreMES10>Tok1</TimOfPreMES10>
  <PriMES15>1</PriMES15>
  <TesIndMES18>0</TesIndMES18>
  <MesIdeMES19>Token1</MesIdeMES19>
  <MesTypMES20>CC304A</MesTypMES20>
  <CorIdeMES25>Token1</CorIdeMES25>
  <HEAHEA>
    <RefNumHEA4>Token1</RefNumHEA4>
    <DecRejReaHEA252>Token1</DecRejReaHEA252>
    <DecRejReaHEA252LNG>T1</DecRejReaHEA252LNG>
    <DecRejDatTimHEA116>Token______1</DecRejDatTimHEA116>
  </HEAHEA>
  <FUNERRER1>
    <ErrTypER11>12</ErrTypER11>
    <ErrPoiER12>Token1ptr</ErrPoiER12>
    <ErrReaER13>Token1reason</ErrReaER13>
    <OriAttValER14>Token1orig</OriAttValER14>
  </FUNERRER1>
  <FUNERRER1>
    <ErrTypER11>13</ErrTypER11>
    <ErrPoiER12>Token2ptr</ErrPoiER12>
    <ErrReaER13>Token2reason</ErrReaER13>
    <OriAttValER14>Token2original</OriAttValER14>
  </FUNERRER1>
  <FUNERRER1>
    <ErrTypER11>14</ErrTypER11>
    <ErrPoiER12>Token3pointer</ErrPoiER12>
    <ErrReaER13>Token3rsn</ErrReaER13>
    <OriAttValER14>Token3org</OriAttValER14>
  </FUNERRER1>
</CC316A>";

		protected override CC316AResponseMessageProcessor Processor => new CC316AResponseMessageProcessor(new LoggingInformation());

		protected override void AssertProcessedData(AsycudaManifestHeaderSS manifestHeader, IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage outgoingMessage, LoggingInformation logger)
		{
			base.AssertProcessedData(manifestHeader, incomingMessage, outgoingMessage, logger);
			AssertEquals("Outgoing message status", EDIMessageStatusList.Codes.Rejected, outgoingMessage.EM_Status);
			AssertEquals("Incoming message status", EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
			AssertEquals("Manifest registration status not changed", "XXX", manifestHeader.RegistrationStatus);
			AssertInterpretationWithTable(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC316A, "Rejection Reason: Token1", "Functional Errors",
				["Error Type", "Error Pointer", "Error Reason", "Original Value"],
				[
					["12", "Token1ptr", "Token1reason", "Token1orig"],
					["13", "Token2ptr", "Token2reason", "Token2original"],
					["14", "Token3pointer", "Token3rsn", "Token3org"],
				],
				incomingMessage.EM_MessageInterpretation);
		}

		public void TestMessageInterpretation()
		{
			var processor = new CC316AResponseMessageProcessorForTest();
			var cc316 = new Cc316AType { Heahea = new HeaheaType { DecRejReaHea252 = "Something & Nothing" } };
			AssertSimpleInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC316A, "Rejection Reason: Something &amp; Nothing", processor.MessageInterpretation(cc316));
		}

		class CC316AResponseMessageProcessorForTest : CC316AResponseMessageProcessor
		{
			public CC316AResponseMessageProcessorForTest() : base(new LoggingInformation()) { }
			public new string MessageInterpretation(Cc316AType messageObject) => base.MessageInterpretation(messageObject);
		}
	}
}
