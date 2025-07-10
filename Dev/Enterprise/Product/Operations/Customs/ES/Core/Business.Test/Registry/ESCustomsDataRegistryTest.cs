using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Registry.Testing;

[TestedType(typeof(ESCustomsDataRegistry))]
class ESCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<ESCustomsDataRegistry>
{
	public void TestEnableESMessagingThroughDirectxTInterface()
	{
		TestRegistryItem(
			ItemSet.EnableESMessagingThroughDirectxTInterface,
			"EnableESMessagingThroughDirectxTInterface",
			ESCustomsDataRegistry.Categories.Customs_Spain,
			"Enable ES Messaging Through Direct xT Interface",
			"Enable ES Messaging Through Direct xT Interface?",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
			true
		);
	}

	public void TestEnableESInboxMessagesThroughDirectxTInterface()
	{
		TestRegistryItem(
			ItemSet.EnableESInboxMessagesThroughDirectxTInterface,
			"EnableESInboxMessagesThroughDirectxTInterface",
			ESCustomsDataRegistry.Categories.Customs_Spain,
			"Enable ES Inbox Messages Through Direct xT Interface",
			"Enable ES Inbox Messages Through Direct xT Interface?",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
			true
		);
	}

	public void TestAllowEditEDIMessageBody()
	{
		TestRegistryItem(
			ItemSet.AllowEditEDIMessageBody,
			"AllowEditEDIMessageBody",
			ESCustomsDataRegistry.Categories.Customs_Spain,
			"Allow Edit EDI Message Body",
			@"Show ""Edit Message"" checkbox that allows users with CWSupport password to edit the declaration payload before sending it. This is a development/support feature, please keep it disabled in production.",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForSupport,
			false
		);
	}

	public void TestGenerateAutomaticallyEntryIntoTemporaryStorage()
	{
		TestRegistryItem(
			ItemSet.GenerateAutomaticallyEntryIntoTemporaryStorage,
			"GenerateAutomaticallyEntryIntoTemporaryStorage",
			ESCustomsDataRegistry.Categories.Customs_Spain,
			"Generate automatically entry into Temporary Storage",
			"Enable this option to automatically generate the entry into the Temporary Storage (in those managed locations) when a response with clearance is received from ES Customs.",
			RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			RegistryOptions.Default,
			false
		);
	}

	public void TestCustomsClearanceEmailFrom()
	{
		TestGenericRegistryItem(
			ItemSet.CustomsClearanceEmailFrom,
			"CustomsClearanceEmailFrom",
			ESCustomsDataRegistry.Categories.Customs_Spain,
			"Customs Clearance Email From",
			"Customs Clearance Email From",
			RegistryStorageFlags.System,
			RegistryOptions.IsValueMandatory,
			"AgenciaTributaria@correo.aeat.es"
		);
	}

	public void TestCustomsClearanceEmailRecipient()
	{
		TestGenericRegistryItem(
			ItemSet.CustomsClearanceEmailRecipient,
			"CustomsClearanceEmailRecipient",
			ESCustomsDataRegistry.Categories.Customs_Spain,
			"Customs Clearance Email Recipient",
			"Customs Clearance Email Recipient",
			RegistryStorageFlags.Branch,
			RegistryOptions.Default,
			ZString.Empty
		);
	}

	public void TestInboxXTExpirationPeriodDays()
	{
		TestRegistryItem(
			ItemSet.InboxXTExpirationPeriodDays,
			"InboxXTExpirationPeriodDays",
			ESCustomsDataRegistry.Categories.Customs_Spain_Inbox_XT,
			"Expiration Period (days)",
			"This value represents the maximum number of days the system will poll Spanish Customs for a specific Inbox message.",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
			7
		);
	}

	#region Query URLs

	public void TestExportStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ExportStatusQueryUrl,
			"ExportStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"Export Status",
			"Current version of Export Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalle?CLAVE=%mrn%"
		);
	}

	public void TestImportDjpStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ImportDjpStatusQueryUrl,
			"ImportDjpStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"Import DJP Status",
			"Current version of Import DJP Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.pendencia.cnt.CDespUltInt?operacion=3010&CABECERA_MRN=%mrn%"
		);
	}

	public void TestImportPdiStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ImportPdiStatusQueryUrl,
			"ImportPdiStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"Import PDI Status",
			"Current version of Import PDI Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.advu.jdit.web.cons.DetalleVUAInt?operacion=3000&CABECERA_MRN=%mrn%"
		);
	}

	public void TestImportStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ImportStatusQueryUrl,
			"ImportStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"Import Status",
			"Current version of Import Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImporInternet?CABECERA_MRN=%mrn%"
		);
	}

	public void TestImportStatusCanaryIslandsQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ImportStatusCanaryIslandsQueryUrl,
			"ImportStatusCanaryIslandsQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"Import Status (Canary Islands)",
			"Current version of Import Status (Canary Islands) URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImpInternetVexcan?CABECERA_MRN=%mrn%"
		);
	}

	public void TestImportH1StatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ImportH1StatusQueryUrl,
			"ImportH1StatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_ImportH1,
			"Status",
			"Current version of Import H1 Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleSH1?anyo=%anyo%&pais=%pais%&recinto=%recinto%&numero=%numero%"
		);
	}

	public void TestImportH1StatusCanaryIslandsQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ImportH1StatusCanaryIslandsQueryUrl,
			"ImportH1StatusCanaryIslandsQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_ImportH1,
			"Status (Canary Islands)",
			"Current version of Import H1 Status (Canary Islands) URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleXSH1?anyo=%anyo%&pais=%pais%&recinto=%recinto%&numero=%numero%"
		);
	}

	public void TestImportH1DJPStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ImportH1DJPStatusQueryUrl,
			"ImportH1DJPStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_ImportH1,
			"DJP Status",
			"Current version of Import H1 DJP Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleUltH1?mrn=%mrn%"
		);
	}

	public void TestNctsTransitStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.NctsTransitStatusQueryUrl,
			"NctsTransitStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"NCTS Transit Status",
			"Current version of NCTS Transit Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADTR-JDIT/Ncts5Detalle?CLAVE=%mrn%"
		);
	}

	public void TestT2lNonUCCReceptionStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.T2lNonUCCReceptionStatusQueryUrl,
			"T2lNonUCCReceptionStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"T2L Non UCC Reception Status",
			"Current version of T2L Non UCC Reception Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/L/inwinvoc/es.aeat.dit.adu.adtl.gestion.cnt.DetExpedicionT2L?retorno=es.aeat.dit.adu.adtl.gestion.qry.QDocsT2LInt&operacion=1001&clave=%mrn%"
		);
	}

	public void TestT2lExpeditionAndReceptionStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.T2lExpeditionAndReceptionStatusQueryUrl,
			"T2lExpeditionAndReceptionStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"T2L Expedition and Reception Status",
			"Current version of T2L Expedition and Reception Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/T2LDetalle?mrn=%mrn%"
		);
	}

	public void TestT2cClearanceStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.T2cClearanceStatusQueryUrl,
			"T2cClearanceStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"T2C Clearance Status",
			"Current version of T2C Clearance Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/JECDetalle?mrn=%mrn%"
		);
	}

	public void TestExsStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ExsStatusQueryUrl,
			"ExsStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"EXS Status",
			"Current version of EXS Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adrx.inter.CtrInternet?operacion=1032&clave=%mrn%"
		);
	}

	public void TestDvdStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.DvdStatusQueryUrl,
			"DvdStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"DVD Status",
			"Current version of DVD Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADDV-H2UC/CntAEATInternet?operacion=2250&CABECERA_MRN=%mrn%"
		);
	}

	public void TestDvdStatusCanaryIslandsQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.DvdStatusCanaryIslandsQueryUrl,
			"DvdStatusCanaryIslandsQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"DVD Status (Canary Islands)",
			"Current version of DVD Status (Canary Islands) URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADDV-H2UC/CntATCInternet?operacion=2250&CABECERA_MRN=%mrn%"
		);
	}

	public void TestExitControlStatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ExitControlStatusQueryUrl,
			"ExitControlStatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"Exit Control Status",
			"Current version of Exit Control URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalleDecLlegada?wMrn=%mrn%"
		);
	}

	public void TestG5V1StatusQueryUrl()
	{
		TestGenericRegistryItem(
			ItemSet.G5V1StatusQueryUrl,
			"G5V1StatusQueryUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"G5 V1 Status",
			"Current version of G5 V1 URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADDS-JDIT/CtrlG5Sede?op=detCab&mrn=%mrn%"
		);
	}

	public void TestSummaryDeclarationStatusQueryURL()
	{
		TestGenericRegistryItem(
			ItemSet.SummaryDeclarationStatusQueryURL,
			"SummaryDeclarationStatusQueryURL",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs,
			"Summary Declaration Status",
			"Current version of Summary Declaration Status URL for customs query",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/ADDS-JDIT/QIntDecSum?VEZ=BUSCAR&fRecinto4=%recinto%&fAnio1=%anio%&fNumero6=%numero%&fMrn18=%mrn%"
		);
	}

	public void TestPueSendMessageUrl()
	{
		TestGenericRegistryItem(
			ItemSet.PueSendMessageUrl,
			"PueSendMessageUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE,
			"Send Message",
			"Current version of Send Message URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/EnvioMensajePUE"
		);
	}

	public void TestPueAnnexDocumentsUrl()
	{
		TestGenericRegistryItem(
			ItemSet.PueAnnexDocumentsUrl,
			"PueAnnexDocumentsUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE,
			"Annex Documents",
			"Current version of Annex Documents URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/ENVIODOCPUE"
		);
	}

	public void TestRohsRequestUrl()
	{
		TestGenericRegistryItem(
			ItemSet.RohsRequestUrl,
			"RohsRequestUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE_ROHS_RAEE,
			"ROHS Request",
			"Current version of ROHS Request URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/RohsSolicitudForm"
		);
	}

	public void TestRohsAdditionalDataUrl()
	{
		TestGenericRegistryItem(
			ItemSet.RohsAdditionalDataUrl,
			"RohsAdditionalDataUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE_ROHS_RAEE,
			"ROHS Additional Data",
			"Current version of ROHS Additional Data URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/RohsDatosAdiForm"
		);
	}

	public void TestRohsStatusUrl()
	{
		TestGenericRegistryItem(
			ItemSet.RohsStatusUrl,
			"RohsStatusUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE_ROHS_RAEE,
			"ROHS Status",
			"Current version of ROHS Status URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/SvRohsSolQuery"
		);
	}

	public void TestCOMRequestUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ComRequestUrl,
			"ComRequestUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE_COM,
			"COM Request",
			"Current version of COM Request URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/ComSolicitudForm"
		);
	}

	public void TestCOMAdditionalDataUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ComAdditionalDataUrl,
			"ComAdditionalDataUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE_COM,
			"COM Additional Data",
			"Current version of COM Additional Data URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/ComDatosAdiForm"
		);
	}

	public void TestCOMStatusUrl()
	{
		TestGenericRegistryItem(
			ItemSet.ComStatusUrl,
			"ComStatusUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE_COM,
			"COM Status",
			"Current version of COM Status URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/SvComSolQuery"
		);
	}

	public void TestECORequestUrl()
	{
		TestGenericRegistryItem(
			ItemSet.EcoRequestUrl,
			"EcoRequestUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE_ECO,
			"ECO Request",
			"Current version of ECO Request URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/EcoSolicitudForm"
		);
	}

	public void TestECOAdditionalDataUrl()
	{
		TestGenericRegistryItem(
			ItemSet.EcoAdditionalDataUrl,
			"EcoAdditionalDataUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE_ECO,
			"ECO Additional Data",
			"Current version of ECO Additional Data URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/EcoDatosAdiForm"
		);
	}

	public void TestECOStatusUrl()
	{
		TestGenericRegistryItem(
			ItemSet.EcoStatusUrl,
			"EcoStatusUrl",
			ESCustomsDataRegistry.Categories.Customs_Spain_QueryURLs_PUE_ECO,
			"ECO Status",
			"Current version of ECO Status URL",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
			"https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/SvEcoSolQuery"
		);
	}

	#endregion

	#region Message Version

	public void TestESExportMessageVersion()
	{
		TestGenericRegistryItem(
			ItemSet.ESExportMessageVersion,
			"ESExportMessageVersion",
			ESCustomsDataRegistry.Categories.Customs_Spain_MessageVersion,
			"Export",
			"Current export message version configured to submit to Customs.",
			RegistryStorageFlags.Branch,
			EXPORTVersionNumberList.Codes.Aes);

		AssertType(typeof(CodePairRegistryDataType), ESCustomsDataRegistry.Instance.ESExportMessageVersion.DataType);
	}

	public void TestEST2LMessageVersion()
	{
		TestGenericRegistryItem(
			ItemSet.EST2LMessageVersion,
			"EST2LMessageVersion",
			ESCustomsDataRegistry.Categories.Customs_Spain_MessageVersion,
			"T2L",
			"Current T2L message version configured to submit to Customs.",
			RegistryStorageFlags.Branch,
			T2LVersionNumberList.Codes.RequestJecAndReceptionPous);

		AssertType(typeof(CodePairRegistryDataType), ESCustomsDataRegistry.Instance.EST2LMessageVersion.DataType);
	}

	public void TestESImportMessageVersion()
	{
		TestGenericRegistryItem(
			ItemSet.ESImportMessageVersion,
			"ESImportMessageVersion",
			ESCustomsDataRegistry.Categories.Customs_Spain_MessageVersion,
			"Import (Developers Only)",
			"Current import message version configured to submit to Customs.",
			RegistryStorageFlags.Branch,
			RegistryOptions.IsOnlyForDevelopers);

		AssertType(typeof(CodePairRegistryDataType), ESCustomsDataRegistry.Instance.ESImportMessageVersion.DataType);
	}

	#endregion
}
