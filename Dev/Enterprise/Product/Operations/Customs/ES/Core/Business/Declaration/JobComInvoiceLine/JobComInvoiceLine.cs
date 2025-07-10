using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Declaration;

public partial class JobComInvoiceLine : AutoJobComInvoiceLine, Integration.Customs.ES.IJobComInvoiceLine
{
	public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.JobComInvoiceLine.Schema
	{
		public new const int JI_StateOrRegionOfOriginMaxLength = 2;
	}

	#region override Properties

	[ResourceStringData("4769B4DE-31E3-4E1A-BB31-B13663F48715", Caption = "[33] Tariff", MediumCaption = "[33] Tariff", ShortCaption = "[33] Tariff")]
	public override ZString JI_Tariff { get => base.JI_Tariff; set => base.JI_Tariff = value; }

	[ResourceStringData("CC1FF0F9-42ED-4012-8675-B0C1BFDF4DF1", Caption = "Fourth Quantity", MediumCaption = "Fourth Qty", ShortCaption = "Fourth Qty")]
	public override ZDecimal JI_CustomsFourthQuantity { get => base.JI_CustomsFourthQuantity; set => base.JI_CustomsFourthQuantity = value; }

	#region JI_FormattedTariff

	[ResourceStringData("91B36400-58A0-4F36-9CC9-B4C3F8D6CFA7", Caption = "[33] Tariff", MediumCaption = "[33] Tariff", ShortCaption = "[33] Tariff")]
	public override ZString JI_FormattedTariff
	{
		get => base.JI_FormattedTariff;
		set
		{
			var oldValue = JI_FormattedTariff;
			base.JI_FormattedTariff = value;
			if (!IsCopying && oldValue != JI_FormattedTariff && IsImport)
			{
				DefaultExciseCodeIfNeeded();
				DefaultAIEMCodeIfNeeded();
				SetDefaultTaxOrFeeCode();
			}
		}
	}

	void DefaultExciseCodeIfNeeded()
	{
		var list = AddInfoLookups.ExciseCodeList;
		if (list.Count == 1)
		{
			ZG_ExciseCode = list[0].Code;
		}
		else if (!list.ContainsCode(ZG_ExciseCode))
		{
			ZG_ExciseCode = ZString.Empty;
		}
	}

	void DefaultAIEMCodeIfNeeded()
	{
		var list = AddInfoLookups.AIEMTypeCodeList;
		if (list.Count == 1)
		{
			ZG_AIEMType = list[0].Code;
		}
		else if (!list.ContainsCode(ZG_AIEMType))
		{
			ZG_AIEMType = ZString.Empty;
		}
	}

	protected override void SetDefaultTaxOrFeeCode()
	{
		if (IsImport)
		{
			var list = Lookups.TaxOrFeeCodeList;
			list.RemoveCode("EX");
			if (list.Count == 1)
			{
				JI_ZZF_NKTaxType = list[0].Code;
			}
			else if (!list.ContainsCode(JI_ZZF_NKTaxType))
			{
				JI_ZZF_NKTaxType = ZString.Empty;
			}
		}
	}

	#endregion

	#region ZG Properties

	[ResourceStringData("Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine|ZG_MethodOfPayment2", Caption = "Method Of Payment Can", ShortCaption = "MP Can")]
	public override ZString ZG_MethodOfPayment2 { get => base.ZG_MethodOfPayment2; set => base.ZG_MethodOfPayment2 = value; }

	[ResourceStringData("Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine|ZG_T2LItemNumber", Caption = "T2L Item Number")]
	public override ZInt ZG_T2LItemNumber { get => base.ZG_T2LItemNumber; set => base.ZG_T2LItemNumber = value; }

	[ResourceStringData("FDA5CBE1-9D75-4BD3-B29D-820ED775AB34", Caption = "Country of Dispatch", MediumCaption = "Ctry. Dispatch", ShortCaption = "Ctry. Disp.", FullDescription = "Country of Dispatch of the goods")]
	public override ZString ZG_CountryOfDispatch { get => base.ZG_CountryOfDispatch; set => base.ZG_CountryOfDispatch = value; }

	[ResourceStringData("Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine|ZG_CountryOfDestination", Caption = "Country of Destination")]
	public override ZString ZG_CountryOfDestination { get => base.ZG_CountryOfDestination; set => base.ZG_CountryOfDestination = value; }

	[ResourceStringData("6EF38B38-AE23-45C6-81C3-743C80D79BC9", Caption = "Region of Destination", MediumCaption = "Reg. Destination", ShortCaption = "Reg. Dest.", FullDescription = "State or Region of Destination")]
	public override ZString ZG_RegionOfDestination { get => base.ZG_RegionOfDestination; set => base.ZG_RegionOfDestination = value; }

	public bool IsCountryOfDestinationESOrXCOrXLOrEmpty => Declaration.IsDestinationESOrXCOrXLOrEmpty(ZG_CountryOfDestination);

	public bool DestinationStateIsCanaryIsland => Declaration?.DestinationStateIsCanaryIsland ?? false;

	#endregion

	[ResourceStringData("40EB1502-B253-4061-BEC3-DFFD1C1064C8", Caption = "Valuation Method", MediumCaption = "Val. Method", ShortCaption = "Val. Method", FullDescription = "Valuation Method Code")]
	public override ZString JI_ValuationCode { get => base.JI_ValuationCode; set => base.JI_ValuationCode = value; }

	#region JI_PartAttrib

	public override ZString JI_PartAttrib1
	{
		get => base.JI_PartAttrib1;
		set
		{
			SettingPartAttrib(ref settingJI_PartAttrib1InProgres, () =>
			{
				var oldValue = JI_PartAttrib1;
				base.JI_PartAttrib1 = value;
				var newValue = JI_PartAttrib1;
				if (!IsCopying && oldValue != newValue)
				{
					PartSyncManager.Refresh();
					PopulateVehicleVinFromVINPartAttribute(1, newValue);
				}
			});
		}
	}

	public override ZString JI_PartAttrib2
	{
		get => base.JI_PartAttrib2;
		set
		{
			SettingPartAttrib(ref settingJI_PartAttrib2InProgres, () =>
			{
				var oldValue = JI_PartAttrib2;
				base.JI_PartAttrib2 = value;
				var newValue = JI_PartAttrib2;
				if (!IsCopying && oldValue != newValue)
				{
					PartSyncManager.Refresh();
					PopulateVehicleVinFromVINPartAttribute(2, newValue);
				}
			});
		}
	}

	public override ZString JI_PartAttrib3
	{
		get => base.JI_PartAttrib3;
		set
		{
			SettingPartAttrib(ref settingJI_PartAttrib3InProgres, () =>
			{
				var oldValue = JI_PartAttrib3;
				base.JI_PartAttrib3 = value;
				var newValue = JI_PartAttrib3;
				if (!IsCopying && oldValue != newValue)
				{
					PartSyncManager.Refresh();
					PopulateVehicleVinFromVINPartAttribute(3, newValue);
				}
			});
		}
	}

	bool settingJI_PartAttrib1InProgres;
	bool settingJI_PartAttrib2InProgres;
	bool settingJI_PartAttrib3InProgres;
	void SettingPartAttrib(ref bool settingPartAttribInProgress, Action setPartAttrib)
	{
		if (!settingPartAttribInProgress)
		{
			try
			{
				settingPartAttribInProgress = true;
				setPartAttrib();
			}
			finally
			{
				settingPartAttribInProgress = false;
			}
		}
	}

	#endregion

	public new ICusVehicleCollection<CusVehicle, JobComInvoiceLine> Vehicles => (ICusVehicleCollection<CusVehicle, JobComInvoiceLine>)base.Vehicles;

	protected override ICusVehicleCollection<Customs.Business.CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection(this);

	public override VehicleRelationshipType VehicleRelationship => VehicleRelationshipType.Many;

	#region JI_PartNo

	public override ZString JI_PartNo
	{
		get => base.JI_PartNo;
		set
		{
			var oldValue = JI_PartNo;
			base.JI_PartNo = value;
			var vehicle = Vehicles?.Cast<CusVehicle>().OrderBy(vehicle => vehicle.CVH_SystemCreateTimeUtc).FirstOrDefault();
			if (!IsCopying && vehicle != null && !vehicle.CVH_VehicleIdentificationNumber.IsEmpty && oldValue != JI_PartNo)
			{
				PopulateVINPartAttributeFromUZ_VIN();
			}
		}
	}
	#endregion

	#region JI_StateOrRegionOfOrigin

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.StateIslandCodesList))]
	[MaxLength(Schema.JI_StateOrRegionOfOriginMaxLength)]
	[ResourceStringData("Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine|JI_StateOrRegionOfOrigin", Caption = "[34b] Orig. State/Island", MediumCaption = "Origin State/Island", ShortCaption = "Orig. State")]
	public override ZString JI_StateOrRegionOfOrigin { get => base.JI_StateOrRegionOfOrigin; set => base.JI_StateOrRegionOfOrigin = value; }

	#endregion

	#region JI_CustomsThirdQuantity

	[ResourceStringData("356F2B26-E6ED-45CD-AB63-E58C8C0829DE", Caption = "[31] Third Qty", MediumCaption = "[31] Third Qty", ShortCaption = "[31] Third Qty")]
	public override ZDecimal JI_CustomsThirdQuantity { get => base.JI_CustomsThirdQuantity; set => base.JI_CustomsThirdQuantity = value; }

	#endregion

	#region JI_Procedure

	public override ZString JI_Procedure
	{
		get => base.JI_Procedure;
		set
		{
			var oldValue = JI_Procedure;
			var newValue = base.JI_Procedure = value;
			if (newValue != oldValue)
			{
				if (newValue.IsEmpty || newValue.SubstringSafe(0, 4) != oldValue.SubstringSafe(0, 4))
				{
					AdditionalProcedureCodes.RemoveAndDeleteAll();
				}

				AddInfoValidation?.ValidateZG_HasNonRecycledPlastics();
			}
		}
	}

	#endregion

	#region JI_FormattedProcedure

	public override ZString JI_FormattedProcedure { get => JI_Procedure; set => base.JI_FormattedProcedure = value; }

	#endregion

	public override ZGuid JI_CL
	{
		get => base.JI_CL;
		set
		{
			var oldValue = JI_CL;
			base.JI_CL = value;
			if (!IsCopying && oldValue != JI_CL)
			{
				InvoiceHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid JI_JZ
	{
		get => base.JI_JZ;
		set
		{
			var oldValue = JI_JZ;
			base.JI_JZ = value;
			if (!IsCopying && oldValue != JI_JZ)
			{
				InvoiceHeader?.MarkAsNeedingValidation();
			}
		}
	}

	#endregion

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CurrencyList))]
	public ZString TotalRetailPriceCurrency => Core.Constants.CurrencyCodes.EuropeanUnion;

	public bool IsPCAApplicable
	{
		get
		{
			if (isPCAApplicableCached == null)
			{
				isPCAApplicableCached = new CachedProperty<bool>(Factory, () => IsImport && (CurrentExciseRate?.ZZ2_RateFormula.Contains(UniversalReferenceConstants.ReservedRateFormulaValue.GlobalWarmingPotential) ?? false));
			}
			return isPCAApplicableCached.Value;
		}
	}
	CachedProperty<bool> isPCAApplicableCached;

	public bool IsPVPApplicable
	{
		get
		{
			if (isPVPApplicableCached == null)
			{
				isPVPApplicableCached = new CachedProperty<bool>(Factory, () => IsImport && (CurrentExciseRate?.ZZ2_RateFormula.Contains(UniversalReferenceConstants.ReservedRateFormulaValue.RetailPriceCode) ?? false));
			}
			return isPVPApplicableCached.Value;
		}
	}
	CachedProperty<bool> isPVPApplicableCached;

	public RateView CurrentExciseRate
	{
		get
		{
			if (currentExciseRateCached == null)
			{
				currentExciseRateCached = new CachedProperty<RateView>(Factory, () => UniversalTariff?.ChildTariffs.Where(x => x.ZZH_RelatedTariffCode == ZG_ExciseCode).Select(x => x.RelatedTariffFrom).FirstOrDefault(x => x.ZZ1_ZZI_TariffTypeCode == ExciseTariffTypeCodeByDestinationState)?.Rates.FirstOrDefault());
			}
			return currentExciseRateCached.Value;
		}
	}
	CachedProperty<RateView> currentExciseRateCached;

	string ExciseTariffTypeCodeByDestinationState => Declaration.DestinationStateIsCanaryIsland ?
											UniversalReferenceConstants.RefCusTariffType.CANEX :
											UniversalReferenceConstants.RefCusTariffType.ESEXC;

	public RateView CurrentSpecialExciseRate
	{
		get
		{
			return Factory.GetCachedValue("ESJobComInvoiceLine.CurrentSpecialExciseRate_" + ZG_ExciseCode, () =>
			{
				var result = UniversalTariff?.ChildTariffs.Where(x => x.ZZH_RelatedTariffCode == SpecialExciseCode).Select(x => x.RelatedTariffFrom).FirstOrDefault()?.Rates.FirstOrDefault();
				return result;
			});
		}
	}

	public IEnumerable<TariffRelationshipView> GetChildTariffsGivenTariffTypeCode(ZString tariffType) => UniversalTariff.ChildTariffs.Where(x => x.RelatedTariffType.ZZI_TariffType == tariffType);

	public ZString SpecialExciseCode
	{
		get
		{
			var specialExciseCode = ZString.Empty;

			if (ZG_ExciseCode.StartsWith("0") || ZG_ExciseCode.StartsWith("5"))
			{
				specialExciseCode = ZG_ExciseCode.StartsWith("0") ? "5" + ZG_ExciseCode.SubstringSafe(1) : "0" + ZG_ExciseCode.SubstringSafe(1);
			}

			return specialExciseCode;
		}
	}

	internal int VINPartAttributeIndex
	{
		get
		{
			var result = 0;
			var importer = Importer;
			var product = importer == null ? null : Part;
			if (product != null && importer.PartAttributeManager.HasVINForProduct(product))
			{
				result = importer.PartAttributeManager.VINPartAttribute.Index;
			}
			return result;
		}
	}

	void SynchroniseVIN(Action sync)
	{
		if (!isSynchronisingVINInProgress)
		{
			try
			{
				isSynchronisingVINInProgress = true;
				sync?.Invoke();
			}
			finally
			{
				isSynchronisingVINInProgress = false;
			}
		}
	}
	bool isSynchronisingVINInProgress;

	void PopulateVehicleVinFromVINPartAttribute(int partAttributeIndex, ZString value)
	{
		SynchroniseVIN(() =>
		{
			var vehicle = Vehicles.Cast<CusVehicle>().OrderBy(vehicle => vehicle.CVH_SystemCreateTimeUtc).FirstOrDefault();
			if (VINPartAttributeIndex == partAttributeIndex && !vehicle.IsNull && vehicle.CVH_VehicleIdentificationNumber != value)
			{
				vehicle.CVH_VehicleIdentificationNumber = value;
			}
		});
	}

	internal ZPropertyInfo VINPartAttributeInfo
	{
		get
		{
			switch (VINPartAttributeIndex)
			{
				case 1:
					return JI_PartAttrib1Info;
				case 2:
					return JI_PartAttrib2Info;
				case 3:
					return JI_PartAttrib3Info;
				default:
					return null;
			}
		}
	}

	public PreviousDocumentCollection GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly()
	{
		PreviousDocumentCollection result = null;
		if (PreviousDocuments.Count > 0)
		{
			result = PreviousDocuments;
		}
		else if (InvoiceHeader?.PreviousDocuments.Count > 0)
		{
			result = InvoiceHeader.PreviousDocuments;
		}
		else if (Declaration?.PreviousDocuments.Count > 0)
		{
			result = Declaration.PreviousDocuments;
		}
		return result;
	}

	#region SupportingDocuments

	public override int MaxSupportingDocuments => EntryInstruction?.IsEXS ?? false ? 10 : 99;
	public override ZString SupportingDocumentsValidationMessage => Res.GetString("41187B8F-92D5-46D1-9B85-D86089D7AE21", "Customs will not accept a declaration with more than {0} documents per line", MaxSupportingDocuments);
	public override Func<int> GetSupportingDocumentsMaxCountReduction => () => (InvoiceHeader?.SupportingDocuments?.Count ?? 0) + (Declaration?.SupportingDocuments?.Count ?? 0);

	#endregion

	public ZDecimal JI_PosAdj
	{
		get
		{
			var result = ZDecimal.Zero;
			if (InvoiceHeader?.IsImport ?? false)
			{
				result = ValuationCalculator.GetAdjustement(true) + ValuationCalculator.GetEGVChargeAmount();
			}
			return result;
		}
	}

	public ZDecimal JI_NegAdj
	{
		get
		{
			var result = ZDecimal.Zero;
			if (InvoiceHeader?.IsImport ?? false)
			{
				result -= ValuationCalculator.GetAdjustement(false) - ValuationCalculator.GetEGPChargeAmount();
			}
			return result;
		}
	}

	public override ZInt MaxNumberOfAdditionalProcedureCode => 2;

	public ZDecimal JI_VAT_Additions
	{
		get
		{
			decimal result = 0m;
			if (InvoiceHeader?.IsImport ?? false)
			{
				result = ValuationCalculator.GetVATAdditions(LocalCurrency);
			}
			return result;
		}
	}

	public ZPropertyInfo JI_VAT_AdditionsInfo => GetZPropertyInfo(nameof(JI_VAT_Additions));

	/// <summary>
	/// VAT Value = Customs Value + VAT Additions
	/// </summary>
	public override ZDecimal JI_Calc_ValueForVat
	{
		get
		{
			decimal result = 0m;
			if (InvoiceHeader != null)
			{
				result = JI_CustomsValue + JI_VAT_Additions;
			}
			return result;
		}
	}

	public ZDecimal JI_Calc_ESCustomsValue
	{
		get
		{
			var result = JI_CustomsValue;

			if (InvoiceHeader?.IsImport ?? false)
			{
				result += ValuationCalculator.GetEGVChargeAmount() + ValuationCalculator.GetEGPChargeAmount();
			}
			return result;
		}
	}

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		AdjustImportSupportingDocuments();
	}

	public void AddNewSupportingDocument(ZString code, ZString reference)
	{
		var document = SupportingDocuments.AddNew();
		document.CSI_Code = code;
		document.CSI_ReferenceNumber = reference;
		document.CSI_ParentID = PK;
	}

	void AdjustImportSupportingDocuments()
	{
		if (IsImport)
		{
			var documentsToRemove = SupportingDocuments.Find(sd => sd.CSI_Code == SupportingDocumentType.TransformedRPP).ToList();

			foreach (var docu in documentsToRemove)
			{
				SupportingDocuments.RemoveAndDelete(docu);
			}

			var egvAndegpChargeAmount = ValuationCalculator.GetEGVChargeAmount() + ValuationCalculator.GetEGPChargeAmount();

			if (egvAndegpChargeAmount > 0)
			{
				AddNewSupportingDocument(SupportingDocumentType.TransformedRPP, Utilities.FormatNumberNational(egvAndegpChargeAmount, 2));
			}
		}
	}

	public void PopulateVINPartAttributeFromUZ_VIN()
	{
		SynchroniseVIN(() =>
		{
			var vehicle = Vehicles.Cast<CusVehicle>().OrderBy(vehicle => vehicle.CVH_SystemCreateTimeUtc).FirstOrDefault();
			var value = vehicle?.CVH_VehicleIdentificationNumber ?? ZString.Empty;
			var vinPartAttributeInfo = VINPartAttributeInfo;
			if (vinPartAttributeInfo != null && (ZString)vinPartAttributeInfo.Value != value)
			{
				vinPartAttributeInfo.Value = value;
			}
		});
	}

	protected override ZBool IsSupportEmptyPackType(BasePackage package)
	{
		var entryLine = CusEntryLine;
		bool result = true;

		if (entryLine == null)
		{
			result = base.IsSupportEmptyPackType(package);
		}
		else if (entryLine.CL_LineNumber == 1)
		{
			result = base.IsSupportEmptyPackType(package) || entryLine.HasNonEmptyPackage;
		}

		return result;
	}

	protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Spain;

	protected override Type TypeOfPartUsedCore => typeof(MasterFiles.OrgSupplierPart);

	protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage) => new InvoiceLinePackageValidation(linkPackage, this);

	protected override BaseCusLinkPackageCollection PackagesForInvoiceLinesCore() => new InvoiceLineCusLinkPackageCollection(this);

	public IZZRateSelectionCriteria REARateSelectionCriteria => (reaRateSelectionCriteria ?? (reaRateSelectionCriteria = new CachedProperty<IZZRateSelectionCriteria>(Factory, GetREARateSelectionCriteriaCore))).Value;
	CachedProperty<IZZRateSelectionCriteria> reaRateSelectionCriteria;

	IZZRateSelectionCriteria GetREARateSelectionCriteriaCore() => new RateSelectionCriteria<JobComInvoiceLine>(this, UniversalReferenceConstants.RateTypeList.REA, REARateCode);

	protected override ZBool ShouldCheckMissingPreviousDocumentsCore => (IsExport && !(EntryInstruction?.IsT2L ?? false)) || (EntryInstruction?.IsT2C ?? false);

	public ZString REARateCode => ZG_IsREADirectConsumption ? UniversalReferenceConstants.RateCodeCodeList.AYD : UniversalReferenceConstants.RateCodeCodeList.AYT;

	public ZBool IsVehicleDeclared => Vehicles?.Cast<CusVehicle>().Any() ?? false;

	public ZBool IsEXS => (ZBool)(EntryInstruction?.IsEXS ?? false);

	public bool EntryHeaderValidationModeIsImportNoneOrPDS
	{
		get
		{
			var entryHeader = CusEntryLine?.Header;
			var validationMode = entryHeader?.ValidationMode ?? ValidationModes.None;
			return validationMode == ValidationModes.PDS || validationMode == ValidationModes.None;
		}
	}

	public bool EntryHeaderValidationModeIsImportNone
	{
		get
		{
			var entryHeader = CusEntryLine?.Header;
			var validationMode = entryHeader?.ValidationMode ?? ValidationModes.None;
			return validationMode == ValidationModes.None;
		}
	}

	public ResourceStringData VATIGICTypeCaption => Declaration?.DestinationStateIsCanaryIsland ?? false
									? Res.GetData("43CC4183-BA4F-43DB-8D2C-BD13D7D8930D", "IGIC Type")
									: Res.GetData("6908F60E-2E0B-4ACB-91A6-A59C150866B2", "VAT Type");

	protected override EU.Business.IGuidedDecisionMakingSource GetGuidedDecisionMakingSingleInvoiceLineSourceCore() => new GuidedDecisionMakingSingleInvoiceLineSource(this);
	protected override EU.Business.IGuidedDecisionMakingTarget GetGuidedDecisionMakingSingleInvoiceLineTargetCore() => new GuidedDecisionMakingSingleInvoiceLineTarget(this);

	protected override EU.Business.IGuidedDecisionMakingSource GetGuidedDecisionMakingMultiInvoiceLinesSourceCore() => new GuidedDecisionMakingMultiInvoiceLinesSource(this);
	protected override EU.Business.IGuidedDecisionMakingTarget GetGuidedDecisionMakingMultiInvoiceLinesTargetCore(List<EU.Business.Declaration.JobComInvoiceLine> invoiceLines) => new GuidedDecisionMakingMultiInvoiceLinesTarget(invoiceLines);

	protected override EU.Business.GuidedDecisionMakingBasic GetGuidedDecisionMakingBasicCore(EU.Business.IGuidedDecisionMakingSource source)
		=> new GuidedDecisionMakingBasic((IESGuidedDecisionMakingSource)source, Factory);

	public IReadOnlyCollection<SelectionStyle> GetTariffNomenclatureSelectionModes()
	{
		var entryExtensionSubStyle = EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
		if (entryExtensionSubStyle.IsEmpty || entryExtensionSubStyle != ExsEntrySubStyleList.Codes.EXS)
		{
			return new[] { SelectionStyle.Tariff };
		}

		return new[] { SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff };
	}

	public ZString[] GetAdditionalProcedureCodesList()
	{
		var addCodesList = new List<ZString>();

		var concessionFromProcedure = JI_FormattedProcedure.SubstringSafe(4);
		if (!concessionFromProcedure.IsEmpty)
		{
			addCodesList.Add(concessionFromProcedure);
		}

		addCodesList.AddRange(AdditionalProcedureCodes.Cast<EU.Business.AdditionalProcedureCode>()
														.Where(additionalCode => !additionalCode.CY_Code.IsEmpty)
														.Select(additionalCode => additionalCode.CY_Code.Right(3)).ToList());
		return addCodesList.ToArray();
	}

	public ZString[] GetAdditionalProcedureCodesListForExportUccMessage()
	{
		var numberOfCodesWanted = 2;
		return GetAdditionalProcedureCodesList().OrderByDescending(x => x).Take(numberOfCodesWanted).ToArray();
	}

	[ResourceStringData("1F089A6E-9E18-4833-8341-E2505835285F", Caption = "Contains non-recycled plastics?")]
	public override ZBool ZG_HasNonRecycledPlastics { get => base.ZG_HasNonRecycledPlastics; set => base.ZG_HasNonRecycledPlastics = value; }

	[ResourceStringData("5FB27908-58EA-41BE-8A4A-CF90EEEEC5CA", Caption = "PCA(GWP)")]
	public override ZDecimal ZG_GlobalWarmingPotential { get => base.ZG_GlobalWarmingPotential; set => base.ZG_GlobalWarmingPotential = value; }

	protected override EU.Business.ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.JobComInvoiceLine> GetCusAuthorizationUsages()
		=> new EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>(this, Factory);
}
