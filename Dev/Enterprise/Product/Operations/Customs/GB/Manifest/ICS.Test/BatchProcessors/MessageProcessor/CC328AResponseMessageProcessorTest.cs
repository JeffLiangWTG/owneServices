using CargoWise.Customs.GB.MessageDefinitions.ICS.CC328A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS.Testing
{
	sealed class CC328AResponseMessageProcessorTest : IcsSsGreatBritainResponseMessageProcessorBaseTest<CC328AResponseMessageProcessor, Cc328AType>
	{
		protected override ZString MessageTypeToProcess => IcsSsGreatBritainEDIMessageTypeList.Codes.CC328A;

		protected override ZString MessageTextToProcess =>
@"<CC328A>
	<MesSenMES3>GB000012340003/1234567890</MesSenMES3>
	<MesRecMES6>GB000012340003/1234567890</MesRecMES6>
	<DatOfPreMES9>190114</DatOfPreMES9>
	<TimOfPreMES10>0945</TimOfPreMES10>
	<MesIdeMES19>MSUI11235227</MesIdeMES19>
	<MesTypMES20>CC328A</MesTypMES20>
	<CorIdeMES25>0JRF7UncK0t004</CorIdeMES25>
	<HEAHEA>
		<RefNumHEA4>Preeti_315A_TC001</RefNumHEA4>
		<DocNumHEA5>10GB08I01234567891</DocNumHEA5>
		<DecRegDatTimHEA115>201901140945</DecRegDatTimHEA115>
	</HEAHEA>
	<CUSOFFLON>
		<RefNumCOL1>ES000055</RefNumCOL1>
	</CUSOFFLON>
	<PERLODSUMDEC>
		<TINPLD1>GB000012340002</TINPLD1>
	</PERLODSUMDEC>
	<CUSOFFFENT730>
		<RefNumCUSOFFFENT731>GB000011</RefNumCUSOFFFENT731>
	</CUSOFFFENT730>
</CC328A>";

		protected override CC328AResponseMessageProcessor Processor => new CC328AResponseMessageProcessor(new LoggingInformation());

		protected override void AssertProcessedData(AsycudaManifestHeaderSS manifestHeader, IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage outgoingMessage, LoggingInformation logger)
		{
			base.AssertProcessedData(manifestHeader, incomingMessage, outgoingMessage, logger);
			AssertEquals("Outgoing message status", EDIMessageStatusList.Codes.Acknowledged, outgoingMessage.EM_Status);
			AssertEquals("Incoming message status", EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
			AssertEquals("Manifest registration status", RegistrationStatusList.Codes.Acknowledged, manifestHeader.RegistrationStatus);
			AssertSimpleInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC328A, "MRN: 10GB08I01234567891", incomingMessage.EM_MessageInterpretation);

			AssertEquals("Manifest registration number", "10GB08I01234567891", manifestHeader.RegistrationNumber);
			AssertEquals("Manifest registration date", new ZDateTime(2019, 01, 14, 09, 45, 0), manifestHeader.RegistrationDate);
		}
	}
}
