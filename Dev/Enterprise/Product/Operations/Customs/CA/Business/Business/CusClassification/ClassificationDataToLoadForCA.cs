using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	internal class ClassificationDataToLoadForCA : ClassificationDataToLoad, IPGADataToLoad
	{
		public ClassificationDataToLoadForCA() : base() { }

		public ZString VFDCode { get; set; }
		public ZString TariffTreatment { get; set; }
		public ZString Tariff99Code { get; set; }
		public ZString AuthorityNum { get; set; }
		public ZString TRSNum { get; set; }
		public ZString? Manufacturer { get; set; }
		public ZString GSTCode { get; set; }
		public ZString ExciseExemptCode { get; set; }
		public ZString ExciseRateCode { get; set; }
		public ZString SIMACode { get; set; }
		public ZString SIMADumpingNumber { get; set; }
		public ZString OriginCountry { get; set; }

		#region IPGADataToLoad

		public ZString? PGA_CFIA_Indicator { get; set; }
		public ZString? PGA_CFIA_AIRSExtensionCode { get; set; }
		public ZString? PGA_CFIA_LPCOs { get; set; }
		public ZString? PGA_CFIA_AIRSRegistrations { get; set; }
		public ZString? PGA_CFIA_AIRSEndUse { get; set; }
		public ZString? PGA_CFIA_AIRSMiscellaneous { get; set; }
		public ZString? PGA_CFIA_SourceCountry { get; set; }
		public ZString? PGA_CFIA_SourceState { get; set; }

		public ZString? PGA_CNSC_Indicator { get; set; }
		public ZString? PGA_CNSC_Category { get; set; }
		public ZString? PGA_CNSC_NNIECRSchedulePartNo { get; set; }
		public ZString? PGA_CNSC_PackMarks { get; set; }
		public ZString? PGA_CNSC_LPCOs { get; set; }

		public ZString? PGA_GAC_Indicator { get; set; }

		public ZString? PGA_DFO_ABIInd { get; set; }
		public ZString? PGA_DFO_AISInd { get; set; }
		public ZString? PGA_DFO_TTPInd { get; set; }

		public ZString? PGA_ECCC_WRMInd { get; set; }

		public ZString? PGA_ECCC_ODSInd { get; set; }
		public ZString? PGA_ECCC_CASNumber { get; set; }

		public ZString? PGA_ECCC_WENInd { get; set; }

		public ZString? PGA_ECCC_SourceOfSpecimen { get; set; }
		public ZString? PGA_ECCC_LifeStage { get; set; }
		public ZInt? PGA_ECCC_Age { get; set; }
		public ZString? PGA_ECCC_Sex { get; set; }
		public ZString? PGA_ECCC_Regulated { get; set; }
		public ZString? PGA_ECCC_ScientificName { get; set; }
		public ZString? PGA_ECCC_TSN { get; set; }
		public ZString? PGA_ECCC_AphiaID { get; set; }
		public ZString? PGA_ECCC_Identities { get; set; }

		public ZString? PGA_ECCC_VEEInd { get; set; }

		public ZString? PGA_ECCC_ProcessCode { get; set; }
		public ZString? PGA_ECCC_NationalMark { get; set; }
		public ZString? PGA_ECCC_EPACertified { get; set; }
		public ZString? PGA_ECCC_Transition { get; set; }
		public ZString? PGA_ECCC_Incomplete { get; set; }
		public ZString? PGA_ECCC_CanadaUnique { get; set; }
		public ZString? PGA_ECCC_BulkReporting { get; set; }

		public ZString? PGA_ECCC_VehicleClass { get; set; }

		public ZString? PGA_ECCC_EngineClass { get; set; }
		public ZString? PGA_ECCC_EngineMake { get; set; }
		public ZString? PGA_ECCC_EngineModel { get; set; }
		public ZString? PGA_ECCC_EngineModelYear { get; set; }
		public ZString? PGA_ECCC_EngineIDNumber { get; set; }
		public ZString? PGA_ECCC_EngineManufacturer { get; set; }
		public ZString? PGA_ECCC_EngineFamilyName { get; set; }
		public ZString? PGA_ECCC_EngineTestGroup { get; set; }
		public ZString? PGA_ECCC_EngineEvaporativeFamily { get; set; }
		public ZDecimal? PGA_ECCC_EnginePowerRating { get; set; }
		public ZString? PGA_ECCC_EnginePowerRatingUQ { get; set; }

		public ZString? PGA_ECCC_MachineMake { get; set; }
		public ZString? PGA_ECCC_MachineModel { get; set; }
		public ZString? PGA_ECCC_MachineModelYear { get; set; }
		public ZString? PGA_ECCC_MachineManufacturer { get; set; }
		public ZString? PGA_ECCC_EngineLocation { get; set; }
		public ZString? PGA_ECCC_EvidenceOfConfirmityLocation { get; set; }

		public ZString? PGA_ECCC_IntendedUseCode { get; set; }
		public ZString? PGA_ECCC_LPCOs { get; set; }

		public ZString? PGA_HC_APIInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodeAPI { get; set; }
		public ZString? PGA_HC_CommodityTypeAPI { get; set; }

		public ZString? PGA_HC_BBCInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodeBBC { get; set; }
		public ZString? PGA_HC_CommodityTypeBBC { get; set; }

		public ZString? PGA_HC_CTOInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodeCTO { get; set; }
		public ZString? PGA_HC_CommodityTypeCTO { get; set; }

		public ZString? PGA_HC_CPRInd { get; set; }
		public ZString PGA_HC_IntendedUseCodeCPR { get; set; }
		public ZString PGA_HC_CommodityTypeCPR { get; set; }

		public ZString? PGA_HC_DSEInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodeDSE { get; set; }
		public ZString? PGA_HC_CommodityTypeDSE { get; set; }

		public ZString? PGA_HC_HDRInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodeHDR { get; set; }
		public ZString? PGA_HC_CommodityTypeHDR { get; set; }

		public ZString? PGA_HC_OCSInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodeOCS { get; set; }
		public ZString? PGA_HC_CommodityTypeOCS { get; set; }

		public ZString? PGA_HC_MDEInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodeMDE { get; set; }
		public ZString? PGA_HC_CommodityTypeMDE { get; set; }

		public ZString? PGA_HC_NHPInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodeNHP { get; set; }
		public ZString? PGA_HC_CommodityTypeNHP { get; set; }

		public ZString? PGA_HC_PESInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodePES { get; set; }
		public ZString? PGA_HC_CommodityTypePES { get; set; }

		public ZString? PGA_HC_REDInd { get; set; }
		public ZString PGA_HC_IntendedUseCodeRED { get; set; }
		public ZString PGA_HC_CommodityTypeRED { get; set; }

		public ZString? PGA_HC_VETInd { get; set; }
		public ZString? PGA_HC_IntendedUseCodeVET { get; set; }
		public ZString? PGA_HC_CommodityTypeVET { get; set; }

		public ZString? PGA_HC_GTINNumber { get; set; }
		public ZString? PGA_HC_BatchLotNumber { get; set; }
		public ZString? PGA_HC_LymphoCellOrgan { get; set; }
		public ZString? PGA_HC_SemenCertification { get; set; }
		public ZString? PGA_HC_MedUniqueDeviceIDNumber { get; set; }
		public ZString? PGA_HC_MedDevEstablishLicenceExemption { get; set; }
		public ZString? PGA_HC_CASNumber { get; set; }
		public ZString? PGA_HC_PMRAScheduledPestControlProducts { get; set; }
		public ZString? PGA_HC_PMRAExemptPestControlProducts { get; set; }
		public ZString? PGA_HC_FDANumber { get; set; }
		public ZString? PGA_HC_LPCOs { get; set; }

		public ZString? PGA_NRCan_EEFInd { get; set; }
		public ZString? PGA_NRCan_EXPInd { get; set; }
		public ZString? PGA_NRCan_RDAInd { get; set; }

		public ZString? PGA_PHAC_HAPInd { get; set; }

		public ZString? PGA_TC_TPRInd { get; set; }
		public ZString? PGA_TC_VPRInd { get; set; }

		#endregion
	}
}
