using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS906;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IETS906ProcessorTest : PNTSBaseProcessorTest<Iets906, IETS906Processor>
	{
		protected override void AddPropertiesForTemporaryStorageHeader(TemporaryStorageHeader header)
		{
			header.CorrelationID = "12345";
		}

		protected override CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader header) => CusEntryNumber.Load(header, "CID", Core.Constants.CountryCodes.France);

		protected override ZString GetExpectedErrorTextIfEntryNumberNotFound() => new ZString("Couldn't locate Job using provided CorrelationId#");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Message was technically rejected by customs<br><strong>Correlation ID: </strong>12345</p><p><strong>Technical Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""100px"">Sequence Number</td><td width=""100px"">Error Pointer</td><td width=""100px"">Error Code</td><td width=""100px"">Error Reason</td><td width=""135px"">Original Attribute Value</td></tr><tr><td>1</td><td>tech99</td><td>99</td><td>BEC0001</td><td>BEC0001000</td></tr></table></p>");

		protected override ZString GetExpectedCustomsStatus() => ZString.Empty;

		protected override ZDateTime GetExpectedCustomsStatusDate() => ZDateTime.BrettsBirthday;

		protected override ZString GetExpectedNewMessageStatus() => MessageStatusCodeList.Codes.Error;

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetExpectedFRN() => ZString.Empty;

		protected override ZString GetMessageSubType() => "906";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS906ResponseMessage.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
