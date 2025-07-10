using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS928;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IETS928ProcessorTest : PNTSBaseProcessorTest<Iets928, IETS928Processor>
	{
		protected override void AddPropertiesForTemporaryStorageHeader(TemporaryStorageHeader header)
		{
			header.CorrelationID = "142857";
			header.MRN = "MRN";
			header.CustomsStatus = "CS";
			header.CRN = "CRN";
			header.FRN = "FRN";
		}

		protected override CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader header) => CusEntryNumber.Load(header, "CID", Core.Constants.CountryCodes.France);

		protected override ZString GetExpectedErrorTextIfEntryNumberNotFound() => new ZString("Couldn't locate Job using provided CorrelationId#");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Message was successfully received in customs<br><strong>Correlation ID: </strong>142857</p>");

		protected override ZString GetExpectedCustomsStatus() => "CS";

		protected override ZDateTime GetExpectedCustomsStatusDate() => ZDateTime.BrettsBirthday;

		protected override ZString GetExpectedNewMessageStatus() => MessageStatusCodeList.Codes.ACK;

		protected override ZString GetExpectedCRN() => "CRN";

		protected override ZString GetExpectedMRN() => "MRN";

		protected override ZString GetExpectedFRN() => "FRN";

		protected override ZString GetMessageSubType() => "928";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS928ResponseMessage.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
