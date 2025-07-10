using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.ICS.CC325A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS.Testing
{
	sealed class CC325AResponseMessageProcessorTest : IcsSsGreatBritainResponseMessageProcessorBaseTest<CC325AResponseMessageProcessor, Cc325AType>
	{
		protected override ZString MessageTypeToProcess => IcsSsGreatBritainEDIMessageTypeList.Codes.CC325A;

		protected override ZString MessageTextToProcess =>
@"<CC325A>
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
    <RegDatTimHEA125>201901140945</RegDatTimHEA125>
  </HEAHEA>
  <CUSOFFFENT730>
    <RefNumCUSOFFFENT731>OFFICEREF</RefNumCUSOFFFENT731>
  </CUSOFFFENT730>
  <TRAREQDIV456>
    <NamTRAREQDIV457>Token1</NamTRAREQDIV457>
    <StrAndNumTRAREQDIV458>Token1</StrAndNumTRAREQDIV458>
    <CouTRAREQDIV459>T1</CouTRAREQDIV459>
    <PosCodTRAREQDIV460>Token1</PosCodTRAREQDIV460>
    <CitTRAREQDIV461>Token1</CitTRAREQDIV461>
    <TRAREQDIV456LNG>T1</TRAREQDIV456LNG>
    <TINTRAREQDIV463>TINTRAREQDIV4631</TINTRAREQDIV463>
  </TRAREQDIV456>
</CC325A>";

		protected override CC325AResponseMessageProcessor Processor => new CC325AResponseMessageProcessor(new LoggingInformation());

		protected override void AssertProcessedData(AsycudaManifestHeaderSS manifestHeader, IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage outgoingMessage, LoggingInformation logger)
		{
			base.AssertProcessedData(manifestHeader, incomingMessage, outgoingMessage, logger);
			AssertEquals("Outgoing message status", EDIMessageStatusList.Codes.Acknowledged, outgoingMessage.EM_Status);
			AssertEquals("Incoming message status", EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
			AssertEquals("Manifest registration status", RegistrationStatusList.Codes.DivertedOk, manifestHeader.RegistrationStatus);
			AssertSimpleInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC325A, "Customs Office of First Entry Reference Number: OFFICEREF", incomingMessage.EM_MessageInterpretation);

			var cusCodeData = manifestHeader.ActualEntryDiversions.FirstOrDefault();

			AssertEquals("Manifest office type", OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion, cusCodeData.Code);
			AssertEquals("Manifest office code", "OFFICEREF", cusCodeData.Data);
			AssertEquals("Manifest office date", new ZDateTime(2019, 1, 14, 9, 45, 0), cusCodeData.Date);
		}

		public void TestMessageInterpretation()
		{
			var processor = new CC325AResponseMessageProcessorForTest();
			var cc325 = new Cc325AType { Cusofffent730 = new Cusofffent730Type { RefNumCusofffent731 = "REF>1" } };
			AssertSimpleInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC325A, "Customs Office of First Entry Reference Number: REF&gt;1", processor.MessageInterpretation(cc325));
		}

		class CC325AResponseMessageProcessorForTest : CC325AResponseMessageProcessor
		{
			public CC325AResponseMessageProcessorForTest() : base(new LoggingInformation()) { }
			public new string MessageInterpretation(Cc325AType messageObject) => base.MessageInterpretation(messageObject);
		}
	}
}
