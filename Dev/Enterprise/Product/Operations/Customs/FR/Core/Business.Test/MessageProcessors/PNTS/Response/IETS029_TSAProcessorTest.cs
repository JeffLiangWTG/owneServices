using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS029;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IETS029_TSAProcessorTest : PNTSBaseProcessorTest<Iets029, IETS029Processor>
	{
		protected override void AddPropertiesForTemporaryStorageHeader(TemporaryStorageHeader header)
		{
			header.CRN = "CRN21BETS00000000QFU2";
		}

		protected override CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader header) => CusEntryNumber.Load(header, "MRN", Core.Constants.CountryCodes.France);

		protected override ZString GetExpectedErrorTextIfEntryNumberNotFound() => new ZString("Couldn’t locate Job using provided FRN# or CRN#");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Temporary Storage Activated<br><strong>MRN: </strong>21BEPT00000000QGU7<br><strong>CRN: </strong>CRN21BETS00000000QFU2<br><strong>FRN: </strong>FRN21BEPN000000C3FLU1<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM<br><strong>Date and time of presentation of goods: </strong>1/05/2021 12:34:56 PM</p><p><strong>Activation Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""100px"">Error reason</td><td width=""200px"">Remarks</td></tr><tr><td>Error Reason 1</td><td>Remark 1</td></tr><tr><td>Error Reason 2</td><td>Remark 2</td></tr></table></p>");

		protected override ZString GetExpectedCustomsStatus() => Enterprise.Customs.EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;

		protected override ZDateTime GetExpectedCustomsStatusDate() => new ZDateTime(2021, 5, 1, 12, 34, 56);

		protected override ZString GetExpectedNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZString GetExpectedCRN() => "CRN21BETS00000000QFU2";

		protected override ZString GetExpectedMRN() => "21BEPT00000000QGU7";

		protected override ZString GetExpectedFRN() => "FRN21BEPN000000C3FLU1";

		protected override ZString GetMessageSubType() => "029";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS029_TSAResponseMessage.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
