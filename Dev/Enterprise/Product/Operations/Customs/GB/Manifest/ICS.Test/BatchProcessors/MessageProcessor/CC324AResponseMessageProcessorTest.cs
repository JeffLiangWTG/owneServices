using CargoWise.Customs.GB.MessageDefinitions.ICS.CC324A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS.Testing
{
	sealed class CC324AResponseMessageProcessorTest : IcsSsGreatBritainResponseMessageProcessorBaseTest<CC324AResponseMessageProcessor, Cc324AType>
	{
		protected override ZString MessageTypeToProcess => IcsSsGreatBritainEDIMessageTypeList.Codes.CC324A;

		protected override ZString MessageTextToProcess =>
 @"<CC324A>
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
    <DivRefNumHEA119>Token1</DivRefNumHEA119>
    <RejDatTimHEA126>Token______1</RejDatTimHEA126>
    <RejReaHEA127>Error Message</RejReaHEA127>
    <RejReaHEA128LNG>en</RejReaHEA128LNG>
  </HEAHEA>
  <FUNERRER1>
    <ErrTypER11>12</ErrTypER11>
    <ErrPoiER12>Token1</ErrPoiER12>
    <ErrReaER13>Token1</ErrReaER13>
    <OriAttValER14>Token1</OriAttValER14>
  </FUNERRER1>
  <FUNERRER1>
    <ErrTypER11>13</ErrTypER11>
    <ErrPoiER12>Token2</ErrPoiER12>
    <ErrReaER13>Token2</ErrReaER13>
    <OriAttValER14>Token2</OriAttValER14>
  </FUNERRER1>
  <FUNERRER1>
    <ErrTypER11>14</ErrTypER11>
    <ErrPoiER12>Token3</ErrPoiER12>
    <ErrReaER13>Token3</ErrReaER13>
    <OriAttValER14>Token3</OriAttValER14>
  </FUNERRER1>
</CC324A>";

		protected override CC324AResponseMessageProcessor Processor => new CC324AResponseMessageProcessor(new LoggingInformation());

		protected override void AssertProcessedData(AsycudaManifestHeaderSS manifestHeader, IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage outgoingMessage, LoggingInformation logger)
		{
			base.AssertProcessedData(manifestHeader, incomingMessage, outgoingMessage, logger);
			AssertEquals("Outgoing message status", EDIMessageStatusList.Codes.Sent, outgoingMessage.EM_Status);
			AssertEquals("Incoming message status", EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
			AssertEquals("Manifest registration status", RegistrationStatusList.Codes.DiversionRequestDenied, manifestHeader.RegistrationStatus);
			AssertInterpretationWithTable(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC324A, "Rejection Reason: Error Message", "Functional Errors",
				["Error Type", "Error Pointer", "Error Reason", "Original Value"],
				[
					["12", "Token1", "Token1", "Token1"],
					["13", "Token2", "Token2", "Token2"],
					["14", "Token3", "Token3", "Token3"],
				],
				incomingMessage.EM_MessageInterpretation);
		}

		public void TestMessageInterpretation()
		{
			var processor = new CC324AResponseMessageProcessorForTest();
			var cc324 = new Cc324AType { Heahea = new HeaheaType { RejReaHea127 = "Something & Nothing" } };
			AssertSimpleInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC324A, "Rejection Reason: Something &amp; Nothing", processor.MessageInterpretation(cc324));
		}

		class CC324AResponseMessageProcessorForTest : CC324AResponseMessageProcessor
		{
			public CC324AResponseMessageProcessorForTest() : base(new LoggingInformation()) { }
			public new string MessageInterpretation(Cc324AType messageObject) => base.MessageInterpretation(messageObject);
		}
	}
}
