using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SingleWindowXmlResponseMessageProcessorTest : SingleWindowIncomingMessageProcessorTest<SingleWindowXmlResponseMessageProcessor>
{
	public void TestProcessResponseImport()
	{
		(_, var entryHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(Common.EU.EUJobMessageTypeList.Codes.Import, CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);

		processor.ProcessMessage(receivedMessage);

		AssertProcessingResult(entryHeader, CustomsChannelCA, ITEntryStatusList.Codes.ImportCleared, ReleaseCode, ReleaseDateTimeAsString);
	}

	public void TestProcessResponseImportWithEmptyReleaseData()
	{
		(_, var entryHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(Common.EU.EUJobMessageTypeList.Codes.Import, CustomsChannelCA, ZString.Empty, ZString.Empty);

		processor.ProcessMessage(receivedMessage);

		AssertProcessingResult(entryHeader, CustomsChannelCA, ZString.Empty, ZString.Empty, ZString.Empty);
	}

	public void TestProcessResponseWithEntryHeaderAlreadyCleared()
	{
		(_, var entryHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(Common.EU.EUJobMessageTypeList.Codes.Import, CustomsChannelCA, ZString.Empty, ZString.Empty);

		var sampleDate = "2021-04-01T14:17:57.000+01:00";
		var parsedSampleDate = ParseISO8601Date(sampleDate);
		entryHeader.CH_EntryReleaseDate = parsedSampleDate;
		entryHeader.EntryNumbersProvider.InsertOrUpdateEntryNum(CusEntryNumberConstants.EntryTypes.ClereanceCode, "QQQ", parsedSampleDate);

		processor.ProcessMessage(receivedMessage);
		AssertProcessingResult(entryHeader, CustomsChannelCA, ZString.Empty, "QQQ", sampleDate);
	}

	public void TestProcessResponseExport()
	{
		(_, var entryHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(Common.EU.EUJobMessageTypeList.Codes.Export, CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);

		processor.ProcessMessage(receivedMessage);

		AssertProcessingResult(entryHeader, CustomsChannelCA, ITEntryStatusList.Codes.ExportCleared, ReleaseCode, ReleaseDateTimeAsString);
	}

	public void TestUpdateControlChannelIfAlreadySet()
	{
		(_, var entryHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(Common.EU.EUJobMessageTypeList.Codes.Import, CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);
		entryHeader.CustomsChannel = "XX";

		processor.ProcessMessage(receivedMessage);

		AssertEquals("CustomsChannel updated", CustomsChannelCA, entryHeader.CustomsChannel);
	}

	public void TestProcessResponseInvalidCustomsChannel()
	{
		(_, _, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(Common.EU.EUJobMessageTypeList.Codes.Import, "ABCDEF", ReleaseCode, ReleaseDateTimeAsString);

		processor.ProcessMessage(receivedMessage);

		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Invalid CustomsChannel");
	}

	public void TestUpdateEntryStatusIfAlreadySet()
	{
		(_, var entryHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(Common.EU.EUJobMessageTypeList.Codes.Import, CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);
		entryHeader.CH_EntryStatus = "XXX";

		processor.ProcessMessage(receivedMessage);

		AssertEquals("EntryStatus updated", "ICC", entryHeader.CH_EntryStatus);
	}

	public void TestUpdateReleaseCodeIfAlreadySet()
	{
		(_, var entryHeader, _, var receivedMessage) = PrepareTestDataForXmlMessageProcessor(Common.EU.EUJobMessageTypeList.Codes.Import, CustomsChannelCA, ReleaseCode, ReleaseDateTimeAsString);

		var firstCode = "XXXX";
		var firstDate = "2021-04-02T10:17:57.000+01:00";
		var parsedFirstDate = ParseISO8601Date(firstDate);

		entryHeader.EntryNumbersProvider.InsertOrUpdateReleaseCode(firstCode, parsedFirstDate);
		entryHeader.CH_Status = ITMessageStatusList.Codes.ClearOriginal;

		processor.ProcessMessage(receivedMessage);

		AssertReleaseCodeEntryNumber(entryHeader, firstCode, firstDate);
	}

	public void TestEdgeCases()
	{
		TestThrowsExceptionIfEmptyXml();
		TestThrowsExceptionIfInvalidXml();
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "SWS" };

	protected override SingleWindowXmlResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SingleWindowXmlResponseMessageProcessor(logger);

	(JobDeclaration declaration, CusEntryHeader entryHeader, EDIMessage sentMessage, EDIMessage receivedMessage) PrepareTestDataForXmlMessageProcessor(ZString declarationType, ZString customsChannel, ZString releaseCode, ZString releaseDateAsString)
	{
		var messageText = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
						<esito_bolletta>
							<codice_file>190019563757</codice_file>
							<estremi_dichiarazione>
								<num_reg>64252</num_reg>
								<cod_uff_dog>371101</cod_uff_dog>
								<cod_reg>4 T</cod_reg>
								<anno_reg>2019</anno_reg>
							</estremi_dichiarazione>
							<dichiarazione_registrata>2019-12-09T14:17:56.000+01:00</dichiarazione_registrata>
							<dichiarazione_convalidata>2019-12-09T14:17:56.000+01:00</dichiarazione_convalidata>
							{GetSvincoloXml(releaseCode, releaseDateAsString)}
							<prospetto_svincolo>SI</prospetto_svincolo>
							<sportello_unico>
								<amm_coinvolta>1</amm_coinvolta>
								<data_ultimo_controllo>2019-12-09T12:50:31.000+01:00</data_ultimo_controllo>
								<contributo>Controllo completato positivamente</contributo>
							</sportello_unico>
							<controllo_doganale>
								<flag_ctrl_dog>{customsChannel}</flag_ctrl_dog>
								<esito_ctrl_dog>Eseguito</esito_ctrl_dog>
								<data_esito_ctrl_dog>2019-12-09T14:17:56.000+01:00</data_esito_ctrl_dog>
							</controllo_doganale>
							<controllo_sicurezza>
								<esito_ctrl_sic>Svincolabile</esito_ctrl_sic>
							</controllo_sicurezza>
						</esito_bolletta>";

		return PrepareTestData(declarationType: declarationType, messageText: messageText);
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

	void AssertProcessingResult(CusEntryHeader entryHeader, ZString customsChannel, ZString entryStatus, ZString releaseCode, ZString releaseDateAsString)
	{
		CombineAssertions(() =>
		{
			AssertEquals("CustomsChannel", customsChannel, entryHeader.CustomsChannel);
			AssertStatusUpdatedLog(entryHeader, customsChannel);
			AssertEquals("EntryStatus", entryStatus, entryHeader.CH_EntryStatus);

			ZDateTime parsedReleaseDate = ZDateTime.Empty;
			if (!releaseDateAsString.IsEmpty)
			{
				ZDateTime.TryParseISO8601Date(releaseDateAsString, out parsedReleaseDate);
				AssertReleaseCodeEntryNumber(entryHeader, releaseCode, releaseDateAsString);
			}

			AssertEquals("CH_EntryReleaseDate", parsedReleaseDate, entryHeader.CH_EntryReleaseDate);
		});
	}

	void AssertReleaseCodeEntryNumber(CusEntryHeader entryHeader, ZString releaseCode, ZString releaseDateAsString)
	{
		var releaseCodeEntryNumber = GetCLREntryNumber();
		AssertNotNull("ReleaseCode entry number has been created", releaseCodeEntryNumber);

		AssertEquals("ReleaseCode", releaseCode, releaseCodeEntryNumber.CE_EntryNum);

		ZDateTime.TryParseISO8601Date(releaseDateAsString, out var parsedReleaseDate);
		AssertEquals("ReleaseDate", parsedReleaseDate, releaseCodeEntryNumber.CE_IssueDate);

		CusEntryNumber GetCLREntryNumber()
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
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
	const string ReleaseCode = "VL9NSW";
	const string ReleaseDateTimeAsString = "2019-12-09T14:17:57.000+01:00";
}
