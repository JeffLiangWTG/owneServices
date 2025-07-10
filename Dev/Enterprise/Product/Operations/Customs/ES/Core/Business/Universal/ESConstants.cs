using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
public static class ESConstants
{
	public static class SupportingDocumentTypes
	{
		public static readonly ImmutableArray<ZString> InvoiceDocumentTypesForImport = new ZString[] { "N380", "N325", "N935", "D005", "D008", "1001", "1003" }.ToImmutableArray();
		public static readonly ImmutableArray<ZString> InvoiceTransportDocumentTypesForImport = new ZString[] { "N705", "N710", "N720", "N730", "N740", "N750", "N760", "N785", "N271", "1002", "1833", "C618", "C619", "C625" }.ToImmutableArray();
	}

	public static class DocumentTypes
	{
		public static readonly ImmutableDictionary<ZString, ZString> DocumentCodeByTransportMode = new Dictionary<ZString, ZString>()
		{
			{ TransportModes.Sea, "N705" },
			{ TransportModes.Rail, "N720" },
			{ TransportModes.Road, "N730" },
			{ TransportModes.Air, "N740" }
		}.ToImmutableDictionary();
	}

	public static class AcceptedDocumentExtensions
	{
		public static List<ZString> DocumentExtensions => new List<ZString> { "DOC", "DOCX", "GIF", "JPEG", "JPG", "PDF", "RTF", "TIF", "TIFF", "TXT", "XLS", "XLSX", "ZIP", "7Z" };
	}

	public static class TransportModeTypes
	{
		public static readonly ImmutableArray<ZString> TransportModesForImportTransportIDAndNationality = new ZString[] { TransportModes.Air, TransportModes.InlandWaterwayTransport, TransportModes.OwnPropulsion, TransportModes.Road, TransportModes.Sea }.ToImmutableArray();
		public static readonly ImmutableArray<ZString> TransportModesForTransportDocuments = new ZString[] { TransportModes.Sea, TransportModes.Rail, TransportModes.Road, TransportModes.Air }.ToImmutableArray();
	}

	public static class DocumentWrapperConstants
	{
		public const string DocumentWrappersAssembly = "Enterprise.Customs.ES.DocumentWrappers";
		public const string SADHExportDocumentWrapperType = "Enterprise.Customs.ES.DocumentWrappers.SADH.ESDocSADHExport";
		public const string SADHImportDocumentWrapperType = "Enterprise.Customs.ES.DocumentWrappers.SADH.ESDocSADHImport";
		public const string NctsHeaderDocumentWrapperType = "Enterprise.Customs.ES.DocumentWrappers.NCTS.NctsHeaderDocumentWrapper";
		public const string TemporaryStorageHeaderDocumentWrapperType = "Enterprise.Customs.ES.DocumentWrappers.TemporaryStorage.TemporaryStorageHeaderWrapper";
	}

	public static class DocumentCaptureRequestFileNameSuffixes
	{
		public const string ExportClearanceDoc = "_E_AEAT_CLR";
		public const string ExportAccompanyingDoc = "_E_AEAT_ead";
		public const string ExportT2LFDoc = "_E_AEAT_t2lf";
		public const string ExportAESCertificateEffectiveDepartureDoc = "_E_AEAT_CLR_EXT";

		public const string ImportClearanceDoc = "_I_AEAT_CLR";
		public const string ImportClearanceComplementatyDoc = "_I_AEAT_CLR_C";
		public const string ImportCertificateDoc = "_I_AEAT_CER";
		public const string ImportCertificateComplementatyDoc = "_I_AEAT_CER_C";
		public const string ImportPaymentLetterAEATDoc = "_I_AEAT_M031";
		public const string ImportPaymentLetterATCDoc = "_I_AEAT_M032";
		public const string ImportProofOfPaymentAEATDoc = "_I_AEAT_J031";
		public const string ImportProofOfPaymentATCDoc = "_I_AEAT_J032";

		public const string DVDClearanceDoc = "_D_AEAT_CLR";

		public const string NCTSDepartureTADDoc = "_NCTS_AEAT_TAD";
		public const string NCTS5DepartureDATDoc = "_NCTS_AEAT_DAT";

		public const string T2LExpeditionClearanceDoc = "_E_AEAT_T2L_CLR";
		public const string T2LClearanceDoc = "_I_AEAT_T2L_CLR";
		public const string T2LReceptionClearanceDoc = "_I_AEAT_T2LR_CLR";

		public const string ArrivalAtExitDoc = "_E_AEAT_EAL_CLR";

		public const string EXSClearanceDoc = "_E_AEAT_EXS_CLR";

		public const string H7ClearanceDoc = "_H7_AEAT_CLR";
	}

	public static class CustomsWebsiteUrlCodes
	{
		public const string MRNinRegistryUrl = "%mrn%";
		public const string RecintoInRegistryUrl = "%recinto%";
		public const string AnioInRegistryUrl = "%anio%";
		public const string NumeroInRegistryUrl = "%numero%";
		public const string AnyoInRegistryUrl = "%anyo%";
		public const string PaisInRegistryUrl = "%pais%";
	}

	public static class CustomsWebsiteUrls
	{
		public const string ImportPdiStatus = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.advu.jdit.web.cons.DetalleVUAInt?operacion=3000&CABECERA_MRN=%mrn%";
		public const string ImportStatus = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImporInternet?CABECERA_MRN=%mrn%";
		public const string ImportStatusCanaryIslands = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImpInternetVexcan?CABECERA_MRN=%mrn%";
		public const string ImportH1Status = "https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleSH1?anyo=%anyo%&pais=%pais%&recinto=%recinto%&numero=%numero%";
		public const string ImportH1StatusCanaryIslands = "https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleXSH1?anyo=%anyo%&pais=%pais%&recinto=%recinto%&numero=%numero%";
		public const string ImportH1DJPStatus = "https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleUltH1?mrn=%mrn%";
		public const string T2lNonUCCReceptionStatus = "https://www1.agenciatributaria.gob.es/L/inwinvoc/es.aeat.dit.adu.adtl.gestion.cnt.DetExpedicionT2L?retorno=es.aeat.dit.adu.adtl.gestion.qry.QDocsT2LInt&operacion=1001&clave=%mrn%";
		public const string T2lExpeditionAndReceptionStatus = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/T2LDetalle?mrn=%mrn%";
		public const string T2cClearanceStatus = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/JECDetalle?mrn=%mrn%";
		public const string NctsTransitStatus = "https://www1.agenciatributaria.gob.es/wlpl/ADTR-JDIT/Ncts5Detalle?CLAVE=%mrn%";
		public const string ExportStatus = "https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalle?CLAVE=%mrn%";
		public const string ImportDjpStatus = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.pendencia.cnt.CDespUltInt?operacion=3010&CABECERA_MRN=%mrn%";
		public const string ExsStatus = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adrx.inter.CtrInternet?operacion=1032&clave=%mrn%";
		public const string DvdStatus = "https://www1.agenciatributaria.gob.es/wlpl/ADDV-H2UC/CntAEATInternet?operacion=2250&CABECERA_MRN=%mrn%";
		public const string DvdStatusCanaryIslands = "https://www1.agenciatributaria.gob.es/wlpl/ADDV-H2UC/CntATCInternet?operacion=2250&CABECERA_MRN=%mrn%";
		public const string ExitControlStatus = "https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalleDecLlegada?wMrn=%mrn%";
		public const string G5V1Status = "https://www1.agenciatributaria.gob.es/wlpl/ADDS-JDIT/CtrlG5Sede?op=detCab&mrn=%mrn%";
		public const string SummaryDeclarationStatus = "https://www1.agenciatributaria.gob.es/wlpl/ADDS-JDIT/QIntDecSum?VEZ=BUSCAR&fRecinto4=%recinto%&fAnio1=%anio%&fNumero6=%numero%&fMrn18=%mrn%";
	}

	public static class CustomsWebsiteUrlsPue
	{
		public const string AnnexDocumentsUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/ENVIODOCPUE";
		public const string SendMessageUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/EnvioMensajePUE";
		public static class RohsRaee
		{
			public const string RequestUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/RohsSolicitudForm";
			public const string AdditionalDataUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/RohsDatosAdiForm";
			public const string StatusUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/SvRohsSolQuery";
		}
		public static class Com
		{
			public const string RequestUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/ComSolicitudForm";
			public const string AdditionalDataUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/ComDatosAdiForm";
			public const string StatusUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/SvComSolQuery";
		}
		public static class Eco
		{
			public const string RequestUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/EcoSolicitudForm";
			public const string AdditionalDataUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/EcoDatosAdiForm";
			public const string StatusUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/SvEcoSolQuery";
		}
	}

	public static class UCC6VersionCodes
	{
		public const int NoUCC6 = 0;
		public const int UCC6 = 1;
	}

	public static class POUSVersionCodes
	{
		public const int NoPOUS = 0;
		public const int POUS = 1;
		public const int POUS2 = 2;
	}

	public static class InboxNotificationResponseTypes
	{
		public const string Import = "https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/PresentaMercanciasV1.wsdl";
		public const string Export = "https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/dit/adu/adex/ws/predua/NotifPreDUAV1.wsdl";
		public const string AESInvalidation = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/ComunicaInvalidacionV1.wsdl";
		public const string AESClearance = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/ComunicaLevanteExporV1.wsdl";
		public const string AESNonConformity = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/ComunicaDisconformeExporV1.wsdl";
		public const string AESCceControl = "https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/ComunicaControlesCCEV1.wsdl";
		public const string AESExitResult = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/ComunicaResulSalidaV1.wsdl";
		public const string AESExitClearance = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/ComunicaLevanteSalidaV1.wsdl";
		public const string AESExitNonConformity = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/ComunicaDisconformeSalidaV1.wsdl";
		public const string DVD = "https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/ActivaPDCVinculacionV1.wsdl";
		public const string NCTSClearance = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/ComunicaLevanteParV1.wsdl";
		public const string NCTSNonConformity = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/ComunicaDisconformeParV1.wsdl";
		public const string NCTSCceControl = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/ComunicaControlesParV1.wsdl";
		public const string NCTSInvalidation = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/ComunicaInvaliTranV1.wsdl";
	}

	public static class CustomMsgAttributes
	{
		public const string MRNToRequest = "custom.ES.MRNToRequest";
	}

	public static class AESWarehouseCodes
	{
		public const string WarehouseTypeR = "R";
		public const string WarehouseTypeS = "S";
		public const string WarehouseTypeU = "U";
		public const string WarehouseTypeV = "V";
		public const string WarehouseTypeY = "Y";
	}

	public static class AESAuthorizationCodes
	{
		public const string C019 = "C019";
		public const string C501 = "C501";
		public const string C502 = "C502";
		public const string C503 = "C503";
		public const string C504 = "C504";
		public const string C505 = "C505";
		public const string C506 = "C506";
		public const string C507 = "C507";
		public const string C508 = "C508";
		public const string C509 = "C509";
		public const string C510 = "C510";
		public const string C511 = "C511";
		public const string C512 = "C512";
		public const string C513 = "C513";
		public const string C514 = "C514";
		public const string C515 = "C515";
		public const string C516 = "C516";
		public const string C517 = "C517";
		public const string C518 = "C518";
		public const string C519 = "C519";
		public const string C520 = "C520";
		public const string C521 = "C521";
		public const string C522 = "C522";
		public const string C523 = "C523";
		public const string C524 = "C524";
		public const string C525 = "C525";
		public const string C526 = "C526";
		public const string C600 = "C600";
		public const string C601 = "C601";
		public const string C626 = "C626";
		public const string C627 = "C627";
		public const string C990 = "C990";
		public const string D019 = "D019";
		public const string N990 = "N990";
	}

	public static class NCTS5AuthorizationCodes
	{
		public const string C521 = "C521";
		public const string C523 = "C523";
		public const string C524 = "C524";
	}

	public static class UOM
	{
		public const string PK = "PK";
		public const string GF = "GF";
		public const string KN = "KN";
	}
}
