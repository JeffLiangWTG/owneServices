using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureSingleWindowXmlResponseMessageProcessorTest : NctsDepartureSingleWindowIncomingMessageProcessorTest<SingleWindowXmlResponseMessageProcessor>
{
	public void TestProcessResponse()
	{
		(var nctsHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);

		processor.ProcessMessage(receivedMessage);

		AssertProcessingResult(nctsHeader, CustomsChannelCA, NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, ReleaseCode, ReleaseDateTimeAsString);
	}

	public void TestProcessResponseImportWithEmptyReleaseData()
	{
		(var nctsHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(CustomsChannelCA, ZString.Empty, ZString.Empty);

		processor.ProcessMessage(receivedMessage);

		AssertProcessingResult(nctsHeader, CustomsChannelCA, ZString.Empty, ZString.Empty, ZString.Empty);
	}

	public void TestProcessResponseWithNctsHeaderAlreadyCleared()
	{
		(var nctsHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(CustomsChannelCA, ZString.Empty, ZString.Empty);

		var sampleDate = "2021-07-13T14:17:57.000+01:00";
		var parsedSampleDate = ParseISO8601Date(sampleDate);
		nctsHeader.EntryNumbersProvider.InsertOrUpdateEntryNum(CusEntryNumberConstants.EntryTypes.ClereanceCode, "QQQ", parsedSampleDate);

		processor.ProcessMessage(receivedMessage);
		AssertProcessingResult(nctsHeader, CustomsChannelCA, ZString.Empty, "QQQ", sampleDate);
	}

	public void TestUpdateControlChannelIfAlreadySet()
	{
		(var nctsHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);
		nctsHeader.MovementHeader.BM_ControlChannel = "XX";

		processor.ProcessMessage(receivedMessage);

		AssertEquals("CustomsChannel updated", CustomsChannelCA, nctsHeader.MovementHeader.BM_ControlChannel);
	}

	public void TestProcessResponseInvalidCustomsChannel()
	{
		(_, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor("ABCDEF", ReleaseCode, ReleaseDateTimeAsString);

		processor.ProcessMessage(receivedMessage);

		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Invalid CustomsChannel");
	}

	public void TestUpdateEntryStatusIfAlreadySet()
	{
		(var nctsHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);
		nctsHeader.MovementHeader.BM_CustomsStatus = "XXX";

		processor.ProcessMessage(receivedMessage);

		AssertEquals("EntryStatus updated", "DRL", nctsHeader.MovementHeader.BM_CustomsStatus);
	}

	public void TestUpdateReleaseCodeIfAlreadySet()
	{
		(var nctsHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);

		var firstCode = "XXXX";
		var firstDate = "2021-04-02T10:17:57.000+01:00";
		var parsedFirstDate = ParseISO8601Date(firstDate);

		nctsHeader.EntryNumbersProvider.InsertOrUpdateReleaseCode(firstCode, parsedFirstDate);
		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;

		processor.ProcessMessage(receivedMessage);

		AssertReleaseCodeEntryNumber(nctsHeader, firstCode, firstDate);
	}

	public void TestEdgeCases()
	{
		TestThrowsExceptionIfEmptyXml();
		TestThrowsExceptionIfInvalidXml();
	}

	public void TestTadPrintedWhenReceivingClearance()
	{
		(var nctsHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);

		processor.ProcessMessage(receivedMessage);

		AssertEquals("When Receiving Positive Irisp with Clearance, Should have a TAD document", 1, Factory.GetTadPrintedJobs(nctsHeader).Length);
	}

	public void TestFailingIrispProcessingWhenGenerateDocumentThrowsException()
	{
		var nctsHeader = Factory.New<NctsHeaderForFailingDocumentsGeneration>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		(_, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString, nctsHeader);

		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Generation documents process failed", "When Documents generation process fails");
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "SWS" };

	protected override SingleWindowXmlResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SingleWindowXmlResponseMessageProcessor(logger);

	(NctsHeader nctsHeader, EDIMessage sentMessage, EDIMessage receivedMessage) PrepareTestDataForXmlMessageProcessor(ZString customsChannel, ZString releaseCode, ZString releaseDateAsString, NctsHeader nctsHeader = null)
	{
		var messageText = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
						<esito_bolletta>
							<codice_file>190019563757</codice_file>
							<estremi_dichiarazione>
								<num_reg>4098</num_reg>
								<cod_uff_dog>025100</cod_uff_dog>
								<cod_reg>1 T</cod_reg>
								<anno_reg>2021</anno_reg>
							</estremi_dichiarazione>
							<dichiarazione_registrata>2021-07-12T14:17:56.000+01:00</dichiarazione_registrata>
							<dichiarazione_convalidata>2021-07-12T14:17:56.000+01:00</dichiarazione_convalidata>
							<visto_uscire>
								<esito_visto_uscire>uscita conclusa il</esito_visto_uscire>
								<data_visto_uscire>2021-07-12T00:00:00.000+01:00</data_visto_uscire>
							</visto_uscire>
							{GetSvincoloXml(releaseCode, releaseDateAsString)}
							<prospetto_svincolo>SI</prospetto_svincolo>
							<controllo_doganale>
								<flag_ctrl_dog>{customsChannel}</flag_ctrl_dog>
								<esito_ctrl_dog>Eseguito</esito_ctrl_dog>
								<data_esito_ctrl_dog>2021-07-12T14:17:56.000+01:00</data_esito_ctrl_dog>
							</controllo_doganale>
							<controllo_sicurezza>
								<esito_ctrl_sic>Svincolabile</esito_ctrl_sic>
							</controllo_sicurezza>
						</esito_bolletta>";

		return PrepareTestData(messageText: messageText, nctsHeader: nctsHeader);
	}

	ZString GetSvincoloXml(ZString releaseCode, ZString releaseDateAsString)
	{
		var svincolo = "<svincolo />";

		if (!releaseCode.IsEmpty && !releaseDateAsString.IsEmpty)
		{
			svincolo = $@"<svincolo>
								<cod_svincolo>{releaseCode}</cod_svincolo>
								<data_codice_svincolo>{releaseDateAsString}</data_codice_svincolo>
								<flag_svincolo_forzato>0</flag_svincolo_forzato>
							</svincolo>";
		}

		return svincolo;
	}

	void AssertProcessingResult(NctsHeader nctsHeader, ZString customsChannel, ZString customsStatus, ZString releaseCode, ZString releaseDateAsString)
	{
		CombineAssertions(() =>
		{
			AssertEquals("CustomsChannel", customsChannel, nctsHeader.MovementHeader.BM_ControlChannel);
			AssertStatusUpdatedLog(nctsHeader, customsChannel);
			AssertEquals("CustomsStatus", customsStatus, nctsHeader.MovementHeader.BM_CustomsStatus);

			ZDateTime parsedReleaseDate = ZDateTime.Empty;
			if (!releaseDateAsString.IsEmpty)
			{
				ZDateTime.TryParseISO8601Date(releaseDateAsString, out parsedReleaseDate);
				AssertReleaseCodeEntryNumber(nctsHeader, releaseCode, releaseDateAsString);
			}
		});
	}

	void AssertReleaseCodeEntryNumber(NctsHeader nctsHeader, ZString releaseCode, ZString releaseDateAsString)
	{
		var releaseCodeEntryNumber = GetCLREntryNumber();
		AssertNotNull("ReleaseCode entry number has been created", releaseCodeEntryNumber);

		AssertEquals("ReleaseCode", releaseCode, releaseCodeEntryNumber.CE_EntryNum);

		ZDateTime.TryParseISO8601Date(releaseDateAsString, out var parsedReleaseDate);
		AssertEquals("ReleaseDate", parsedReleaseDate, releaseCodeEntryNumber.CE_IssueDate);

		CusEntryNumber GetCLREntryNumber()
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberConstants.EntryTypes.ClereanceCode);
			return Factory.LoadTop1<CusEntryNumber>(query);
		}
	}

	ZDateTime ParseISO8601Date(ZString dateAsISO861String)
	{
		ZDateTime parsedSampleDate = ZDateTime.Empty;
		ZDateTime.TryParseISO8601Date(dateAsISO861String, out parsedSampleDate);

		return parsedSampleDate;
	}

	const string CustomsChannelCA = CustomsChannelCodeList.Codes.AutomaticControl;
	const string ReleaseCode = "RDAAHG";
	const string ReleaseDateTimeAsString = "2021-07-12T14:17:57.000+01:00";
}
