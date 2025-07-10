using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAOrgSupplierPartDataLoad : Customs.Business.GlobalOrgSupplierPartDataLoad, Integration.Customs.CA.ICAOrgSupplierPartDataLoad, IExposeMethodsForPGADataLoad
	{
		public static class CAFieldNames
		{
			public const string VFDCode = "VFDCode";
			public const string TariffTreatment = "TariffTreatment";
			public const string Tariff99Code = "Tariff99Code";
			public const string AuthorityNum = "AuthorityNum";
			public const string TRSNum = "TRSNum";
			public const string SITTCertNums = "SITTCertNums";
			public const string ImportReason = "ImportReason";
			public const string ProductModel = "ProductModel";
			public const string ProductNum = "ProductNum";
			public const string ProductBrand = "ProductBrand";
			public const string TireImporterID = "TireImporterID";
			public const string TypeOrSize = "TypeOrSize";
			public const string TireCompliantCompletion = "TireCompliantCompletion";
			public const string TireImportDateCompliant = "TireImportDateCompliant";
			public const string GSTCode = "GSTCode";
			public const string ExciseExemptCode = "ExciseExemptCode";
			public const string ExciseRateCode = "ExciseRateCode";
			public const string SIMACode = "SIMACode";
			public const string SIMADumpingNum = "SIMADumpingNum";
			public const string Manufacturer = "Manufacturer";
		}

		#region Parts Data

		protected override IEnumerable<string> GetFieldNames()
		{
			foreach (var property in base.GetFieldNames())
			{
				yield return property;
			}

			yield return CAFieldNames.VFDCode;
			yield return CAFieldNames.TariffTreatment;
			yield return CAFieldNames.Tariff99Code;
			yield return CAFieldNames.AuthorityNum;
			yield return CAFieldNames.TRSNum;
			yield return CAFieldNames.SITTCertNums;
			yield return CAFieldNames.ImportReason;
			yield return CAFieldNames.ProductModel;
			yield return CAFieldNames.ProductNum;
			yield return CAFieldNames.ProductBrand;
			yield return CAFieldNames.TireImporterID;
			yield return CAFieldNames.TypeOrSize;
			yield return CAFieldNames.TireCompliantCompletion;
			yield return CAFieldNames.TireImportDateCompliant;
			yield return CAFieldNames.GSTCode;
			yield return CAFieldNames.ExciseExemptCode;
			yield return CAFieldNames.ExciseRateCode;
			yield return CAFieldNames.SIMACode;
			yield return CAFieldNames.SIMADumpingNum;
			yield return CAFieldNames.Manufacturer;
			foreach (var property in CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.PGAFieldNames.GetPGAFieldNames())
			{
				yield return property;
			}
		}

		protected override PartsDataToLoad GetPartsDataToLoad()
		{
			return new CAPartsDataToLoad();
		}

		public class CAPartsDataToLoad : Customs.Business.GlobalPartsDataToLoad, IPGADataToLoad
		{
			public ZString VFDCode;
			public ZString TariffTreatment;
			public ZString Tariff99Code;
			public ZString AuthorityNum;
			public ZString TRSNum;
			public ZString SITTCertNums;
			public ZString ImportReason;
			public ZString ProductModel;
			public ZString ProductNum;
			public ZString ProductBrand;
			public ZString TireImporterID;
			public ZString TypeOrSize;
			public ZString TireCompliantCompletion;
			public ZString TireImportDateCompliant;
			public ZString GSTCode;
			public ZString ExciseExemptCode;
			public ZString ExciseRateCode;
			public ZString SIMACode;
			public ZString SIMADumpingNum;
			public ZString? Manufacturer;

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
		}

		#endregion

		#region Create/Update Pivot

		protected override bool UseOldClassificationFields => true;

		protected override bool IsScheduleB(ZString tariff)
		{
			return isExportTariffNum(tariff);
		}

		bool isExportTariffNum(ZString tariffNumber)
		{
			return tariffNumber.KeepNumericCharacters().Length == 8;
		}

		protected override string ScheduleBClassificationType => ClassificationTypeList.Codes.SHB;

		protected override void AddDataToPivot(Customs.Business.BaseCusClassPartPivot basePivot, PartsDataToLoad partsData)
		{
			base.AddDataToPivot(basePivot, partsData);
			var data = (CAPartsDataToLoad)partsData;
			var pivot = (CusClassPartPivot)basePivot;

			var supportDataImporting = pivot as ISupportDataImporting;
			if (supportDataImporting != null)
			{
				supportDataImporting.IsImportingData = true;
			}

			if (data.Manufacturer.HasValue)
			{
				using (pivot.SetterSuspender.ResumeSetting(CusClassPartPivot.Schema.CCA_OA_Manufacturer))
				{
					pivot.CCA_OA_Manufacturer_ZAddress.OrgPK = OrgHeader.LoadFromCode(Factory, data.Manufacturer.Value)?.PK ?? ZGuid.Empty;
				}
			}

			if (pivot.IsImport)
			{
				AddImportDataToPivot(pivot, partsData);
			}
			else if (pivot.IsExport)
			{
				AddExportDataToPivot(pivot, partsData);
			}
		}

		void AddImportDataToPivot(CusClassPartPivot pivot, PartsDataToLoad partsData)
		{
			CAPartsDataToLoad data = (CAPartsDataToLoad)partsData;
			var setterSuspender = pivot.SetterSuspender;
			SetValue(pivot.CCA_RN_NKOriginInfo, data.PartOrigin, setterSuspender: setterSuspender, resumeSuspender: true);
			if (pivot.CCA_RN_NKOrigin == Core.Constants.CountryCodes.UnitedStates)
			{
				SetValue(pivot.CCA_ProvinceOfOriginInfo, data.OriginState, setterSuspender: setterSuspender, resumeSuspender: true);
			}
			SetValue(pivot.CCA_ValueForDutyCodeInfo, data.VFDCode);
			if (!string.IsNullOrEmpty(data.TariffTreatment))
			{
				SetValue(pivot.CCA_TreatmentCodeInfo, data.TariffTreatment.PadLeft(2, '0'));
			}

			SetValue(pivot.CCA_99TariffCodeInfo, data.Tariff99Code);
			SetValue(pivot.CCA_AuthorityNumberInfo, data.AuthorityNum);
			SetValue(pivot.CCA_TRSNumberInfo, data.TRSNum);
			SetValue(pivot.CCA_ImportReasonCodeInfo, data.ImportReason);
			SetValue(pivot.CCA_ModelInfo, data.ProductModel);
			SetValue(pivot.CCA_ModelNumberInfo, data.ProductNum);
			SetValue(pivot.CCA_BrandNameInfo, data.ProductBrand);
			SetValue(pivot.CCA_TypeSizeInfo, data.TypeOrSize);
			SetValue(pivot.CCA_TIINInfo, data.TireImporterID);
			SetValue(pivot.CCA_GSTStatusCodeInfo, data.GSTCode);
			SetValue(pivot.CCA_ETExemptionInfo, data.ExciseExemptCode);
			SetValue(pivot.CCA_ETRateCodeInfo, data.ExciseRateCode);
			if (!string.IsNullOrEmpty(data.TireCompliantCompletion))
			{
				pivot.CCA_CompliantCompletion = CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.IsValidYesReply(data.TireCompliantCompletion);
			}

			if (!string.IsNullOrEmpty(data.TireImportDateCompliant))
			{
				pivot.CCA_CompliantImportDateIndicator = CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.IsValidYesReply(data.TireImportDateCompliant);
			}

			pivot.SITTCertificationNumbers.Add(data.SITTCertNums);
			if (data is IPGADataToLoad pgaDataToLoad)
			{
				var helper = new CAOrgSupplierPartAndClassificationDataLoad_PGAHelper(Factory, this);
				helper.SetPGAIndicator(pivot, pgaDataToLoad);
			}
		}

		protected override IEnumerable<ZString> GetPivotPropertiesToSuspendSetting()
		{
			foreach (var property in base.GetPivotPropertiesToSuspendSetting())
			{
				yield return property;
			}

			foreach (var property in CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.PGAFieldNames.GetPGAPropertiesToSuspendSetting(this))
			{
				yield return property;
			}

			if (HasColumn(CAFieldNames.Manufacturer))
			{
				yield return CusClassPartPivot.Schema.CCA_OA_Manufacturer;
			}
		}

		protected override void LoadCountrySpecificDataForTariffNum(MasterFiles.Business.OrgSupplierPart enterprisePart, ZString tariffNum, ZString classificationType, PartsDataToLoad dataToLoad)
		{
			var isImport = classificationType == Common.ClassificationType.IMP;
			var pivot = (CusClassPartPivot)AddCusClassPartPivot(enterprisePart, classificationType, !isImport && IsScheduleB(tariffNum), dataToLoad);
			pivot.CI_TariffNum = tariffNum.Left(pivot.CI_TariffNumInfo.MaxLength);
			pivot.CI_CC = ZGuid.Empty;

			if (isImport)
			{
				var partsDataToLoad = dataToLoad as CAPartsDataToLoad;
				if (partsDataToLoad != null)
				{
					pivot.CCA_SIMADumpingNumber = partsDataToLoad.SIMADumpingNum.Left(pivot.CCA_SIMADumpingNumInfo.MaxLength);

					var simaTax = pivot.DutiesAndTaxes.Cast<DutyAndTax>().FirstOrDefault(x => DutyAndTaxTypes.IsSIMATaxCode(x.C1_TaxType));
					if (simaTax != null)
					{
						var simaExemptCode = partsDataToLoad.SIMACode.Left(simaTax.C1_ExemptCodeInfo.MaxLength);
						if (!pivot.HasMultipleADDs || simaExemptCode.EndsWith("0", System.StringComparison.OrdinalIgnoreCase))
						{
							simaTax.C1_ExemptCode = simaExemptCode;
						}
						else if (pivot.HasMultipleADDs)
						{
							DisplayLogMessage(Res.GetString("9AC28AB5-7CF1-4917-B6BF-8E00C0FCA338", "Warning – Product code {0} has multiple matching SIMA dumping codes. SIMA rates could not be loaded.", pivot.Part.OP_PartNum));
							pivot.DutiesAndTaxes.DeleteAll();
						}
					}
				}
			}
		}

		void AddExportDataToPivot(CusClassPartPivot pivot, PartsDataToLoad partsData)
		{
			CAPartsDataToLoad data = (CAPartsDataToLoad)partsData;
			pivot.CCA_RN_NKOrigin = data.PartOrigin.Left(pivot.CCA_RN_NKOriginInfo.MaxLength);
			pivot.CCA_ProvinceOfOrigin = data.OriginState.Left(pivot.CCA_ProvinceOfOriginInfo.MaxLength);
		}

		#endregion

		#region Classification

		protected override Customs.Business.BaseCusClassification GetClassification(string partClassCode, string classType)
		{
			var loader = new CusClassification.Loader(Factory);
			CusClassification classification;
			classification = loader.Load(partClassCode, classType);
			if (classType == CusClassification.ClassificationType.EXP && classification == null)
			{
				classification = loader.Load(partClassCode, CusClassification.ClassificationType.IMP);
			}
			return classification;
		}

		#endregion

		#region Set UQ

		protected override string GetUQFromTariff(ZString classificationLookup, ZString classType)
		{
			string tariffUQ = base.GetUQFromTariff(classificationLookup, classType);
			var classification = GetClassification(classificationLookup, classType);
			if (classification != null && !classification.CC_TariffNum.IsEmpty)
			{
				tariffUQ = GetUQFromTariffNum(classification.CC_TariffNum, classification.CC_ClassificationType);
			}
			return tariffUQ;
		}

		ZString GetUQFromTariffNum(ZString tariffNum, ZString classType)
		{
			var result = ZString.Empty;
			ITariffData tariff = null;
			if (classType == CusClassification.ClassificationType.IMP)
			{
				tariff = new TariffWrapper(new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem, tariffNum, ZDateTime.Today));
			}
			else if (classType == CusClassification.ClassificationType.EXP)
			{
				tariff = new CACExportTariff.Loader(Factory).LoadFromCode(tariffNum);
			}
			if (tariff != null)
			{
				result = tariff.TariffUnits;
			}

			return result;
		}

		protected override void SetRecordUQFromTariff(PartsDataToLoad record)
		{
			if (!record.PartClassification.IsEmpty)
			{
				record.PartUQ = GetUQFromTariff(record.PartClassification, CusClassification.ClassificationType.IMP);
			}
			else if (!record.PartExportClassification.IsEmpty)
			{
				record.PartUQ = GetUQFromTariff(record.PartExportClassification, CusClassification.ClassificationType.EXP);
			}
			else if (!record.ImportTariff.IsEmpty)
			{
				record.PartUQ = GetUQFromTariffNum(record.ImportTariff, CusClassification.ClassificationType.IMP);
			}
			else if (!record.ExportTariff.IsEmpty)
			{
				record.PartUQ = GetUQFromTariffNum(record.ExportTariff, isExportTariffNum(record.ExportTariff) ?
					CusClassification.ClassificationType.EXP : CusClassification.ClassificationType.IMP);
			}
		}

		#endregion

		#region IAddToDisposableList

		void IExposeMethodsForPGADataLoad.AddToDisposableList(IDisposable disposable)
		{
			base.AddToDisposableList(disposable);
		}

		bool IExposeMethodsForPGADataLoad.HasColumn(string columnName)
		{
			return base.HasColumn(columnName);
		}

		void IExposeMethodsForPGADataLoad.DisplayLogMessage(string message)
		{
			base.DisplayLogMessage(message);
		}

		#endregion
	}
}
