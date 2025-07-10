using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS410;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IETS410ProcessorTest : PNTSBaseProcessorTest<Iets410, IETS410Processor>
	{
		protected override void AddPropertiesForTemporaryStorageHeader(TemporaryStorageHeader header)
		{
			header.CRN = "CRN21BETS00000000QFU2";
		}

		protected override CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader header) => CusEntryNumber.Load(header, "CRN", Core.Constants.CountryCodes.France);

		protected override ZString GetExpectedErrorTextIfEntryNumberNotFound() => new ZString("Couldn't locate Job using provided CRN#");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>TSD Invalidated<br><strong>CRN: </strong>CRN21BETS00000000QFU2<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM<br><strong>Invalidation Initiated By Customs: </strong>False</p>");

		protected override ZString GetExpectedCustomsStatus() => Enterprise.Customs.EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated;

		protected override ZDateTime GetExpectedCustomsStatusDate() => new ZDateTime(2021, 5, 1, 12, 34, 56);

		protected override ZString GetExpectedNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZString GetExpectedCRN() => "CRN21BETS00000000QFU2";

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetExpectedFRN() => ZString.Empty;

		protected override ZString GetMessageSubType() => "410";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS410ResponseMessage.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
