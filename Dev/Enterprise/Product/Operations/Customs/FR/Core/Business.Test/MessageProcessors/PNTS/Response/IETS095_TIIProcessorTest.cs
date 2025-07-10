using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS095;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IETS095_TIIProcessorTest : PNTSBaseProcessorTest<Iets095, IETS095Processor>
	{
		protected override void AddPropertiesForTemporaryStorageHeader(TemporaryStorageHeader header)
		{
			header.MRN = "21BEPT00000000QGU7";
		}

		protected override CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader header) => CusEntryNumber.Load(header, "MRN", Core.Constants.CountryCodes.France);

		protected override ZString GetExpectedErrorTextIfEntryNumberNotFound() => new ZString("Couldn't locate Job using provided MRN# or LRN#");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Irregularity Under Investigation<br><strong>MRN: </strong>21BEPT00000000QGU7<br><strong>CRN: </strong>CRN21BETS00000000QFU2<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM<br><strong>Timer For Temporary Storage: </strong>30/07/2021 11:59:59 PM<br><strong>Remarks: </strong>The remarks</p>");

		protected override ZString GetExpectedCustomsStatus() => Enterprise.Customs.EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.IrregularityUnderInvestigation;

		protected override ZDateTime GetExpectedCustomsStatusDate() => new ZDateTime(2021, 5, 1, 12, 34, 56);

		protected override ZString GetExpectedNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZString GetExpectedCRN() => "CRN21BETS00000000QFU2";

		protected override ZString GetExpectedMRN() => "21BEPT00000000QGU7";

		protected override ZString GetExpectedFRN() => ZString.Empty;

		protected override ZString GetMessageSubType() => "095";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS095_TIIResponseMessage.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
