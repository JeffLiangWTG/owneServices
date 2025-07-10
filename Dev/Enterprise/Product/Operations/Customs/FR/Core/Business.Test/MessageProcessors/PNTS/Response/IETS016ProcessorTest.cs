using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS016;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IETS016ProcessorTest : PNTSBaseProcessorTest<Iets016, IETS016Processor>
	{
		protected override void AddPropertiesForTemporaryStorageHeader(TemporaryStorageHeader header)
		{
			header.LRN = "IETS115INVALIDMESSAGE";
		}

		protected override CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader header) => CusEntryNumber.Load(header, "LRN", Core.Constants.CountryCodes.France);

		protected override ZString GetExpectedErrorTextIfEntryNumberNotFound() => new ZString("Couldn't locate Job using provided LRN #");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Functional rejection<br><strong>LRN: </strong>IETS115INVALIDMESSAGE<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM<br><strong>Business Validation: </strong>115</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""100px"">Sequence Number</td><td width=""100px"">Error Pointer</td><td width=""100px"">Error Code</td><td width=""100px"">Error Reason</td><td width=""135px"">Remarks</td></tr><tr><td>1</td><td>dateAndTimeOfPresentationOfTheGoods</td><td>99</td><td>BER0069</td><td>The Date and time of presentation of the goods is not valid.</td></tr><tr><td>2</td><td>declarationDate</td><td>99</td><td>BER0071</td><td>The Declaration date is not valid.</td></tr></table></p>");

		protected override ZString GetExpectedCustomsStatus() => ZString.Empty;

		protected override ZDateTime GetExpectedCustomsStatusDate() => ZDateTime.BrettsBirthday;

		protected override ZString GetExpectedNewMessageStatus() => PNTSMessageStatusList.Codes.FunctionalRejection;

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetExpectedFRN() => ZString.Empty;

		protected override ZString GetMessageSubType() => "016";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS016ResponseMessage.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
