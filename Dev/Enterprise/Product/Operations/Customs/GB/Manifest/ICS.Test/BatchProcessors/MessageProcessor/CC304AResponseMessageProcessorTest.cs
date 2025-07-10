using CargoWise.Customs.GB.MessageDefinitions.ICS.CC304A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS.Testing
{
	sealed class CC304AResponseMessageProcessorTest : IcsSsGreatBritainResponseMessageProcessorBaseTest<CC304AResponseMessageProcessor, Cc304AType>
	{
		protected override ZString MessageTypeToProcess => IcsSsGreatBritainEDIMessageTypeList.Codes.CC304A;

		protected override ZString MessageTextToProcess =>
@"<CC304A>
  <MesSenMES3>GB142936702000/0000000000</MesSenMES3>
  <MesRecMES6>GB142936702000/0000000000</MesRecMES6>
  <DatOfPreMES9>250325</DatOfPreMES9>
  <TimOfPreMES10>0947</TimOfPreMES10>
  <MesIdeMES19>9515</MesIdeMES19>
  <MesTypMES20>CC304A</MesTypMES20>
  <CorIdeMES25>pBufgMy0EywLJx</CorIdeMES25>
  <HEAHEA>
    <DocNumHEA5>25GB03I3B5L9D43003</DocNumHEA5>
    <TraModAtBorHEA76>3</TraModAtBorHEA76>
    <IdeOfMeaOfTraCroHEA85>72BZD7</IdeOfMeaOfTraCroHEA85>
    <ComRefNumHEA>MAN0010304</ComRefNumHEA>
    <ConRefNumHEA>BDO1517545</ConRefNumHEA>
    <AmeAccDatTimHEA111>202503250948</AmeAccDatTimHEA111>
    <DatTimAmeHEA113>202503250947</DatTimAmeHEA113>
  </HEAHEA>
  <GOOITEGDS>
    <IteNumGDS7>1</IteNumGDS7>
    <ComRefNumGIM1>675386 / 675385</ComRefNumGIM1>
  </GOOITEGDS>
  <PERLODSUMDEC>
    <TINPLD1>GB142936702000</TINPLD1>
  </PERLODSUMDEC>
  <CUSOFFFENT730>
    <RefNumCUSOFFFENT731>GB000060</RefNumCUSOFFFENT731>
    <ExpDatOfArrFIRENT733>202503261212</ExpDatOfArrFIRENT733>
  </CUSOFFFENT730>
  <TRACARENT601>
    <NamTRACARENT604>W HUSMANN GMBH</NamTRACARENT604>
    <StrNumTRACARENT607>SCHACHENWEID</StrNumTRACARENT607>
    <PstCodTRACARENT606>6105</PstCodTRACARENT606>
    <CtyTRACARENT603>SCHACHEN</CtyTRACARENT603>
    <CouCodTRACARENT605>CH</CouCodTRACARENT605>
  </TRACARENT601>
</CC304A>";

		protected override CC304AResponseMessageProcessor Processor => new CC304AResponseMessageProcessor(new LoggingInformation());

		protected override void AssertProcessedData(AsycudaManifestHeaderSS manifestHeader, IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage outgoingMessage, LoggingInformation logger)
		{
			base.AssertProcessedData(manifestHeader, incomingMessage, outgoingMessage, logger);
			AssertEquals("Outgoing message status", EDIMessageStatusList.Codes.Acknowledged, outgoingMessage.EM_Status);
			AssertEquals("Manifest registration status", RegistrationStatusList.Codes.Acknowledged, manifestHeader.RegistrationStatus);
		}

		public void TestMessageInterpretation()
		{
			var processor = new CC304AResponseMessageProcessorForTest();
			var cc304 = new Cc304AType { Heahea = new HeaheaType { AmeAccDatTimHea111 = "202512312359" } };
			AssertSimpleInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC304A, "Acceptance Date: 31/12/2025 23:59", processor.MessageInterpretation(cc304));
		}

		class CC304AResponseMessageProcessorForTest : CC304AResponseMessageProcessor
		{
			public CC304AResponseMessageProcessorForTest() : base(new LoggingInformation()) { }
			public new string MessageInterpretation(Cc304AType messageObject) => base.MessageInterpretation(messageObject);
		}
	}
}
