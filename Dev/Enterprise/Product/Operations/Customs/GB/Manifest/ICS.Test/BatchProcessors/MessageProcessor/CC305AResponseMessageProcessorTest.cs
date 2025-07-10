using CargoWise.Customs.GB.MessageDefinitions.ICS.CC305A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS.Testing
{
	sealed class CC305AResponseMessageProcessorTest : IcsSsGreatBritainResponseMessageProcessorBaseTest<CC305AResponseMessageProcessor, Cc305AType>
	{
		protected override ZString MessageTypeToProcess => IcsSsGreatBritainEDIMessageTypeList.Codes.CC305A;

		protected override ZString MessageTextToProcess =>
@"<CC305A>
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
    <DocNumHEA5>Token1</DocNumHEA5>
    <AmeRejMotCodHEA604>1</AmeRejMotCodHEA604>
    <AmeRejMotTexHEA605>Token1</AmeRejMotTexHEA605>
    <AmeRejMotTexHEA605LNG>T1</AmeRejMotTexHEA605LNG>
    <DatTimAmeHEA113>Token______1</DatTimAmeHEA113>
    <AmeRejDatTimHEA112>Token______1</AmeRejDatTimHEA112>
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
  <TRAREP>
    <NamTRE1>Token1</NamTRE1>
    <StrAndNumTRE1>Token1</StrAndNumTRE1>
    <PosCodTRE1>Token1</PosCodTRE1>
    <CitTRE1>Token1</CitTRE1>
    <CouCodTRE1>T1</CouCodTRE1>
    <TRAREPLNG>T1</TRAREPLNG>
    <TINTRE1>TINTRE11</TINTRE1>
  </TRAREP>
  <PERLODSUMDEC>
    <NamPLD1>Token1</NamPLD1>
    <StrAndNumPLD1>Token1</StrAndNumPLD1>
    <PosCodPLD1>Token1</PosCodPLD1>
    <CitPLD1>Token1</CitPLD1>
    <CouCodPLD1>T1</CouCodPLD1>
    <PERLODSUMDECLNG>T1</PERLODSUMDECLNG>
    <TINPLD1>TINPLD11</TINPLD1>
  </PERLODSUMDEC>
  <CUSOFFFENT730>
    <RefNumCUSOFFFENT731>Token__1</RefNumCUSOFFFENT731>
  </CUSOFFFENT730>
</CC305A>";

		protected override CC305AResponseMessageProcessor Processor => new CC305AResponseMessageProcessor(new LoggingInformation());

		protected override void AssertProcessedData(AsycudaManifestHeaderSS manifestHeader, IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage outgoingMessage, LoggingInformation logger)
		{
			base.AssertProcessedData(manifestHeader, incomingMessage, outgoingMessage, logger);
			AssertEquals("Outgoing message status", EDIMessageStatusList.Codes.Sent, outgoingMessage.EM_Status);
			AssertEquals("Incoming message status", EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
			AssertEquals("Manifest registration status not changed", "XXX", manifestHeader.RegistrationStatus);
			AssertInterpretationWithTable(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC305A, "Rejection Reason: Token1", "Functional Errors",
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
			var processor = new CC305AResponseMessageProcessorForTest();
			var cc305 = new Cc305AType { Heahea = new HeaheaType { AmeRejMotTexHea605 = "Something & Nothing" } };
			AssertSimpleInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC305A, "Rejection Reason: Something &amp; Nothing", processor.MessageInterpretation(cc305));
		}

		class CC305AResponseMessageProcessorForTest : CC305AResponseMessageProcessor
		{
			public CC305AResponseMessageProcessorForTest() : base(new LoggingInformation()) { }
			public new string MessageInterpretation(Cc305AType messageObject) => base.MessageInterpretation(messageObject);
		}
	}
}
