using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	internal interface IPGADataToLoad
	{
		ZString? PGA_CFIA_Indicator { get; set; }
		ZString? PGA_CFIA_AIRSExtensionCode { get; set; }
		ZString? PGA_CFIA_LPCOs { get; set; }
		ZString? PGA_CFIA_AIRSRegistrations { get; set; }
		ZString? PGA_CFIA_AIRSEndUse { get; set; }
		ZString? PGA_CFIA_AIRSMiscellaneous { get; set; }
		ZString? PGA_CFIA_SourceCountry { get; set; }
		ZString? PGA_CFIA_SourceState { get; set; }

		ZString? PGA_CNSC_Indicator { get; set; }
		ZString? PGA_CNSC_Category { get; set; }
		ZString? PGA_CNSC_NNIECRSchedulePartNo { get; set; }
		ZString? PGA_CNSC_PackMarks { get; set; }
		ZString? PGA_CNSC_LPCOs { get; set; }

		ZString? PGA_GAC_Indicator { get; set; }

		ZString? PGA_DFO_ABIInd { get; set; }
		ZString? PGA_DFO_AISInd { get; set; }
		ZString? PGA_DFO_TTPInd { get; set; }

		ZString? PGA_ECCC_WRMInd { get; set; }

		ZString? PGA_ECCC_ODSInd { get; set; }
		ZString? PGA_ECCC_CASNumber { get; set; }

		ZString? PGA_ECCC_WENInd { get; set; }

		ZString? PGA_ECCC_SourceOfSpecimen { get; set; }
		ZString? PGA_ECCC_LifeStage { get; set; }
		ZInt? PGA_ECCC_Age { get; set; }
		ZString? PGA_ECCC_Sex { get; set; }
		ZString? PGA_ECCC_Regulated { get; set; }
		ZString? PGA_ECCC_ScientificName { get; set; }
		ZString? PGA_ECCC_TSN { get; set; }
		ZString? PGA_ECCC_AphiaID { get; set; }
		ZString? PGA_ECCC_Identities { get; set; }

		ZString? PGA_ECCC_VEEInd { get; set; }

		ZString? PGA_ECCC_ProcessCode { get; set; }
		ZString? PGA_ECCC_NationalMark { get; set; }
		ZString? PGA_ECCC_EPACertified { get; set; }
		ZString? PGA_ECCC_Transition { get; set; }
		ZString? PGA_ECCC_Incomplete { get; set; }
		ZString? PGA_ECCC_CanadaUnique { get; set; }
		ZString? PGA_ECCC_BulkReporting { get; set; }

		ZString? PGA_ECCC_VehicleClass { get; set; }

		ZString? PGA_ECCC_EngineClass { get; set; }
		ZString? PGA_ECCC_EngineMake { get; set; }
		ZString? PGA_ECCC_EngineModel { get; set; }
		ZString? PGA_ECCC_EngineModelYear { get; set; }
		ZString? PGA_ECCC_EngineIDNumber { get; set; }
		ZString? PGA_ECCC_EngineManufacturer { get; set; }
		ZString? PGA_ECCC_EngineFamilyName { get; set; }
		ZString? PGA_ECCC_EngineTestGroup { get; set; }
		ZString? PGA_ECCC_EngineEvaporativeFamily { get; set; }
		ZDecimal? PGA_ECCC_EnginePowerRating { get; set; }
		ZString? PGA_ECCC_EnginePowerRatingUQ { get; set; }

		ZString? PGA_ECCC_MachineMake { get; set; }
		ZString? PGA_ECCC_MachineModel { get; set; }
		ZString? PGA_ECCC_MachineModelYear { get; set; }
		ZString? PGA_ECCC_MachineManufacturer { get; set; }
		ZString? PGA_ECCC_EngineLocation { get; set; }
		ZString? PGA_ECCC_EvidenceOfConfirmityLocation { get; set; }

		ZString? PGA_ECCC_IntendedUseCode { get; set; }
		ZString? PGA_ECCC_LPCOs { get; set; }

		ZString? PGA_HC_APIInd { get; set; }
		ZString? PGA_HC_IntendedUseCodeAPI { get; set; }
		ZString? PGA_HC_CommodityTypeAPI { get; set; }

		ZString? PGA_HC_BBCInd { get; set; }
		ZString? PGA_HC_IntendedUseCodeBBC { get; set; }
		ZString? PGA_HC_CommodityTypeBBC { get; set; }

		ZString? PGA_HC_CTOInd { get; set; }
		ZString? PGA_HC_IntendedUseCodeCTO { get; set; }
		ZString? PGA_HC_CommodityTypeCTO { get; set; }

		ZString? PGA_HC_CPRInd { get; set; }
		ZString PGA_HC_IntendedUseCodeCPR { get; set; }
		ZString PGA_HC_CommodityTypeCPR { get; set; }

		ZString? PGA_HC_DSEInd { get; set; }
		ZString? PGA_HC_IntendedUseCodeDSE { get; set; }
		ZString? PGA_HC_CommodityTypeDSE { get; set; }

		ZString? PGA_HC_HDRInd { get; set; }
		ZString? PGA_HC_IntendedUseCodeHDR { get; set; }
		ZString? PGA_HC_CommodityTypeHDR { get; set; }

		ZString? PGA_HC_OCSInd { get; set; }
		ZString? PGA_HC_IntendedUseCodeOCS { get; set; }
		ZString? PGA_HC_CommodityTypeOCS { get; set; }

		ZString? PGA_HC_MDEInd { get; set; }
		ZString? PGA_HC_IntendedUseCodeMDE { get; set; }
		ZString? PGA_HC_CommodityTypeMDE { get; set; }

		ZString? PGA_HC_NHPInd { get; set; }
		ZString? PGA_HC_IntendedUseCodeNHP { get; set; }
		ZString? PGA_HC_CommodityTypeNHP { get; set; }

		ZString? PGA_HC_PESInd { get; set; }
		ZString? PGA_HC_IntendedUseCodePES { get; set; }
		ZString? PGA_HC_CommodityTypePES { get; set; }

		ZString? PGA_HC_REDInd { get; set; }
		ZString PGA_HC_IntendedUseCodeRED { get; set; }
		ZString PGA_HC_CommodityTypeRED { get; set; }

		ZString? PGA_HC_VETInd { get; set; }
		ZString? PGA_HC_IntendedUseCodeVET { get; set; }
		ZString? PGA_HC_CommodityTypeVET { get; set; }

		ZString? PGA_HC_GTINNumber { get; set; }
		ZString? PGA_HC_BatchLotNumber { get; set; }
		ZString? PGA_HC_LymphoCellOrgan { get; set; }
		ZString? PGA_HC_SemenCertification { get; set; }
		ZString? PGA_HC_MedUniqueDeviceIDNumber { get; set; }
		ZString? PGA_HC_MedDevEstablishLicenceExemption { get; set; }
		ZString? PGA_HC_CASNumber { get; set; }
		ZString? PGA_HC_PMRAScheduledPestControlProducts { get; set; }
		ZString? PGA_HC_PMRAExemptPestControlProducts { get; set; }
		ZString? PGA_HC_FDANumber { get; set; }
		ZString? PGA_HC_LPCOs { get; set; }

		ZString? PGA_NRCan_EEFInd { get; set; }
		ZString? PGA_NRCan_EXPInd { get; set; }
		ZString? PGA_NRCan_RDAInd { get; set; }

		ZString? PGA_PHAC_HAPInd { get; set; }

		ZString? PGA_TC_TPRInd { get; set; }
		ZString? PGA_TC_VPRInd { get; set; }
	}
}
