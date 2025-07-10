using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using TariffFormatter = Enterprise.Customs.Business.TariffFormatter;

namespace Enterprise.Customs.EU.NCTS.Business
{
	/// <summary>
	/// Goods line item
	/// </summary>
	[IDontMindLoadingASubclassInstead]
	public abstract class NctsCommonCargoDesc : CusInBondCargoDesc
		, Integration.Customs.EU.NCTS.ICommonCargoDesc
		, EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport
		, IShortSequenceNumberLine
		, ISequenceNumberHeader
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, ICusInvPackTypeSupporter
		, ISupplementaryCodeSupporter
		, Integration.Customs.ICusCodeDataTypeSupporter
		, ISupportMultipleResourceStringData
		, ICusInBondFeeTypeSupporter
		, ICanSupportPhase5
		, INctsAdditionalInfoSequenceHeader
		, Integration.Customs.Shared.ICountryCodeProvider
		, INctsCusInBondCargoDescMaster
	{
		protected NctsCommonCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusInBondCargoDesc.Schema
		{
			public new const int BY_HarmonisedTariffMaxLength = 22;
			public new const int BY_FormattedHarmonisedTariffMaxLength = 22;
			public const string BY_Supplements = nameof(NctsDepartureCargoDesc.BY_Supplements);
		}

		public static class HarmonisedTariffLength
		{
			public const int WCOHsnCode = 6;
			public const int EUExportCode = 8;
		}

		public new NctsHeader Header => HeaderCore;

		protected virtual NctsHeader HeaderCore => MoveHeaderOrBillParent?.Header;

		public bool IsPhase5 => Header?.IsPhase5 ?? false;

		public bool IsPhase5Arrival => Header?.IsPhase5Arrival ?? false;

		public bool IsPhase5Departure => Header?.IsPhase5Departure ?? false;

		public bool IsInPhase5TransitionPeriod => Header?.IsInPhase5TransitionPeriod ?? false;

		public new NctsCommonCargoDescLookups Lookups => (NctsCommonCargoDescLookups)base.Lookups;

		public new NctsCommonCargoDescValidation Validation => (NctsCommonCargoDescValidation)base.Validation;

		public new static readonly NctsCommonCargoDescTypeDecider TypeDecider = new NctsCommonCargoDescTypeDecider();

		public ZString DataGroupingCode => Header?.DefaultDataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public ZString TariffType => TariffTypeCore;

		protected virtual ZString TariffTypeCore => Constants.TariffTypes.Import;

		public ZString UniversalTariffDescription => UniversalTariff?.ZZ1_Description ?? ZString.Empty;

		public virtual TariffView UniversalTariff
		{
			get
			{
				if (EffectiveTariffForCalculateFields.IsEmpty)
				{
					return null;
				}
				if (IsPhase5)
				{
					switch (EffectiveTariffForCalculateFields.Length)
					{
						case HarmonisedTariffLength.WCOHsnCode:
							return new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, Constants.TariffTypes.HarmonizedSystem, EffectiveTariffForCalculateFields, ValuationDate);
						case HarmonisedTariffLength.EUExportCode:
							return new TariffView.Loader(Factory).LoadMostRecentCachedTariff(DataGroupingCode, Constants.TariffTypes.Export, EffectiveTariffForCalculateFields, ValuationDate);
					}
				}
				return new TariffView.Loader(Factory).LoadMostRecentCachedTariff(DataGroupingCode, TariffType, EffectiveTariffForCalculateFields, ValuationDate);
			}
		}

		public NctsCommonMovementHeader MoveHeader => Factory.Load<NctsCommonMovementHeader>(BY_ParentID);

		public NctsBill Bill => Factory.Load<NctsBill>(BY_ParentID);

		public virtual ZDateTime ValuationDate => ZDateTime.Empty;

		public Guid RegistryCompanyPK => Header?.RegistryCompanyPK ?? GlbCompany.CurrentCompany.PK.ToGuid();

		public Guid RegistryBranchPK => Header?.RegistryBranchPK ?? GlbBranch.CurrentBranch.PK.ToGuid();

		public INctsCusInBondCargoDescMaster MoveHeaderOrBillParent
		{
			get
			{
				switch (BY_ParentTableCode)
				{
					case CusInBondMoveHeaderSchema.Constants.Prefix:
						return MoveHeader;
					case CusInBondBillSchema.Constants.Prefix:
						return Bill;
					default:
						return null;
				}
			}
		}

		internal INctsCargoDescValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsCargoDescValidationDecider> validationDeciderCached;

		INctsCargoDescValidationDecider GetValidationDecider() => Header?.Configuration.GoodsItemsConfiguration.GetValidationDecider(this);

		protected override ZString HumanReadableNameCore => Res.GetString("5F7E34D2-3B77-42E8-9CA9-584B5D2E3013", "Goods Item");

		public ZString CountryCode => Header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		[RelatedBusinessObject(nameof(MoveHeaderOrBillParent))]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject Parent (INctsCusInBondCargoDescMaster) is an interface")]
		public override ZGuid BY_ParentID
		{
			get => base.BY_ParentID;
			set
			{
				var oldValue = BY_ParentID;
				base.BY_ParentID = value;
				if (!IsCopying && oldValue != BY_ParentID)
				{
					if (BY_ParentTableCode == CusInBondMoveHeaderSchema.Constants.Prefix && IsPhase5)
					{
						throw new DeveloperNotificationException("Trying to add GoodsItem on MovementHeader in Phase 5. For Phase 5 GoodsItems should be added to NctsBill");
					}
				}
			}
		}

		public override ZString BY_ParentTableCode
		{
			get => base.BY_ParentTableCode;
			set
			{
				var oldValue = BY_ParentTableCode;
				base.BY_ParentTableCode = value;
				if (!IsCopying && oldValue != BY_ParentTableCode)
				{
					if (value == CusInBondMoveHeaderSchema.Constants.Prefix && IsPhase5)
					{
						throw new DeveloperNotificationException("Trying to add GoodsItem on MovementHeader in Phase 5. For Phase 5 GoodsItems should be added to NctsBill");
					}
					if (AutomaticSequenceNumberEnabled)
					{
						MoveHeaderOrBillParent?.LineNumberGenerator.RecalculateWhenAdded(this);
					}
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|BY_LineNo", Caption = "Item Number", MediumCaption = "Item No.", ShortCaption = "Item#")]
		public override ZShort BY_LineNo
		{
			get => base.BY_LineNo;
			set => base.BY_LineNo = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|BY_DeclarationGoodsItemNumber", Caption = "Decl. Goods Item No.", ShortCaption = "Decl. Item No.", FullDescription = "Declaration Goods Item Number")]
		public override ZInt BY_DeclarationGoodsItemNumber { get => base.BY_DeclarationGoodsItemNumber; set => base.BY_DeclarationGoodsItemNumber = value; }

		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.Tariffs))]
		[MaxLength(Schema.BY_HarmonisedTariffMaxLength)]
		[ResourceStringData("D000D29A-ED5D-45DE-8ECA-D00CE8DB6573", Caption = "[33] Commodity Code", MediumCaption = "Commodity", ShortCaption = "Cmdty.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("734662FD-20FB-4FA0-A06C-F662FD4037CC", Caption = "Commodity Code", MediumCaption = "Commodity", ShortCaption = "Cmdty.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BY_HarmonisedTariff
		{
			get => base.BY_HarmonisedTariff;
			set => base.BY_HarmonisedTariff = value;
		}

		[MaxLength(Schema.BY_FormattedHarmonisedTariffMaxLength)]
		[ResourceStringData("5EC197CF-0007-4CEF-86BB-15E478AC4B2B", Caption = "[33] Commodity Code", MediumCaption = "Commodity", ShortCaption = "Cmdty.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("835CEBF1-FC08-4699-AE65-F7C4BA511001", Caption = "Commodity Code", MediumCaption = "Commodity", ShortCaption = "Cmdty.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BY_FormattedHarmonisedTariff
		{
			get => base.BY_FormattedHarmonisedTariff;
			set => base.BY_FormattedHarmonisedTariff = value;
		}

		protected override TariffFormatter GetNewTariffFormatter() => EU.Business.TariffFormatter.New(DataGroupingCode);

		protected virtual ZString EffectiveTariffForCalculateFields => BY_HarmonisedTariff;

		[MaxLength(nameof(BY_Description_MaxLength))]
		[ResourceStringData("04B9EE7F-21C0-4953-BF3C-E9BCDBC69373", Caption = "[31] Description of Goods", MediumCaption = "[31] Description", ShortCaption = "Desc.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("20706EB0-1F16-495E-BB77-E61C2B981AEF", Caption = "Goods Description", MediumCaption = "Description", ShortCaption = "Desc.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BY_Description
		{
			get => base.BY_Description;
			set => base.BY_Description = value;
		}
		public int BY_Description_MaxLength => IsPhase5 ? 512 : 280;

		[ReadOnly(true)]
		[ResourceStringData("2371B508-6DF2-42BF-9B43-C8F735A6AD85", Caption = "[35] Gross Weight (kg)", MediumCaption = "Gross Wgt. (kg)", ShortCaption = "Gross (kg)", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("A5B0B5A9-355D-4FEF-90B9-CFA556063167", Caption = "Gross Weight (kg)", MediumCaption = "Gross Wgt. (kg)", ShortCaption = "Gross (kg)", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public ZDecimal GrossMassInKilograms => new ZWeight(BY_GrossWeight, BY_GrossWeightUnit).InKilogramsSafe;

		[ReadOnly(true)]
		[ResourceStringData("F46FBCBD-C739-4BE6-B167-0C32037CA228", Caption = "[38] Net Weight", MediumCaption = "Net Wgt. (kg)", ShortCaption = "Net (kg)", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("E9665FEF-C35E-456E-8BC2-E2D63C0DCF22", Caption = "Net Weight (kg)", MediumCaption = "Net Wgt. (kg)", ShortCaption = "Net (kg)", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public ZDecimal NetMassInKilograms => new ZWeight(BY_NetWeight, BY_NetWeightUnit).InKilogramsSafe;

		[ResourceStringData("72389583-0065-4F39-BAB1-8DE961C004E3", Caption = "[35] Gross Weight", ShortCaption = "Gross Wgt.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("C193E563-C933-4931-ABE4-0AE23F2635B4", Caption = "Gross Weight", MediumCaption = "Gross Wgt.", ShortCaption = "Gross", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[MeasureUnit(nameof(BY_GrossWeightUnit), MeasureUnitType.Weight)]
		public override ZDecimal BY_GrossWeight
		{
			get => base.BY_GrossWeight;
			set
			{
				var oldValue = BY_GrossWeight;
				base.BY_GrossWeight = value;
				if (!IsCopying && oldValue != BY_GrossWeight)
				{
					Bill?.Header.Bills.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|BY_GrossWeightUnit", Caption = "Gross Weight Units", ShortCaption = "Units")]
		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.WeightUnitList))]
		public override ZString BY_GrossWeightUnit
		{
			get => base.BY_GrossWeightUnit;
			set
			{
				base.BY_GrossWeightUnit = value.ToUpper();
				if (!IsValidationSuspended)
				{
					Validation.ValidateBY_GrossWeight();
				}
			}
		}

		[ResourceStringData("41452FCA-40C6-4B07-B986-4CDF0DE0DEB0", Caption = "[38] Net Weight", ShortCaption = "Net Wgt.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("878C5443-8106-4E60-BD8D-3EB1CC69E09D", Caption = "Net Weight", MediumCaption = "Net Wgt.", ShortCaption = "Net", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[MeasureUnit(nameof(BY_NetWeightUnit), MeasureUnitType.Weight)]
		public override ZDecimal BY_NetWeight
		{
			get => base.BY_NetWeight;
			set => base.BY_NetWeight = value;
		}

		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|BY_NetWeightUnit", Caption = "Net Weight Units", ShortCaption = "Units")]
		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.WeightUnitList))]
		public override ZString BY_NetWeightUnit
		{
			get => base.BY_NetWeightUnit;
			set
			{
				base.BY_NetWeightUnit = value.ToUpper();
				if (!IsValidationSuspended)
				{
					Validation.ValidateBY_NetWeight();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.CountryOfOriginList))]
		[ResourceStringData("DB24D669-0C9D-46A4-BC7B-859A0BF087AA", Caption = "Origin Country/Region", ShortCaption = "Origin.", MediumCaption = "Origin. Ctry./Rgn.", FullDescription = "Country/Region of Origin", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("34F060CB-01B7-44AA-823A-4E5DE080B1A2", Caption = "Origin Country/Region", ShortCaption = "Origin", MediumCaption = "Origin Ctry./Rgn.", FullDescription = "Country/Region of Origin", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BY_RN_NKCountryOfOrigin
		{
			get => base.BY_RN_NKCountryOfOrigin;
			set
			{
				var oldValue = base.BY_RN_NKCountryOfOrigin;
				base.BY_RN_NKCountryOfOrigin = value;
				if (!IsCopying && oldValue != BY_RN_NKCountryOfOrigin)
				{
					SetDefaultTariffUnitOfMeasuresFromRatesView();
				}
			}
		}

		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|BY_CustomsSecondQuantity", ShortCaption = "Sup. Qty", MediumCaption = "Supplementary Qty", Caption = "Supplementary Quantity")]
		[MeasureUnit(nameof(BY_CustomsSecondUnitQty), MeasureUnitType.Unknown)]
		public override ZDecimal BY_CustomsSecondQuantity
		{
			get => base.BY_CustomsSecondQuantity;
			set => base.BY_CustomsSecondQuantity = value;
		}

		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.CustomsUnitOfQuantityList))]
		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|BY_CustomsSecondUnitQty", ShortCaption = "Sup. Unit Qty", MediumCaption = "Supplementary Unit Qty", Caption = "Supplementary Unit Quantity")]
		public override ZString BY_CustomsSecondUnitQty
		{
			get => base.BY_CustomsSecondUnitQty;
			set => base.BY_CustomsSecondUnitQty = value.ToUpper();
		}

		[MeasureUnit(nameof(BY_RX_NKCurrency), MeasureUnitType.Unknown)]
		public override ZDecimal BY_MonetaryValue
		{
			get => base.BY_MonetaryValue;
			set => base.BY_MonetaryValue = value;
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.Currencies))]
		public override ZString BY_RX_NKCurrency
		{
			get => base.BY_RX_NKCurrency;
			set => base.BY_RX_NKCurrency = value.ToUpper();
		}

		#region BY_CustomsThirdQuantity

		[ResourceStringData("AC6FA309-1F62-4E30-B841-60F97A0DA877", Caption = "[31] Third Qty", MediumCaption = "Third Qty", ShortCaption = "Third Qty", FullDescription = "Third Additional Quantity for Liability Amount Calculation")]
		[MeasureUnit(nameof(BY_CustomsThirdUnitQty), MeasureUnitType.Unknown)]
		public override ZDecimal BY_CustomsThirdQuantity { get => base.BY_CustomsThirdQuantity; set => base.BY_CustomsThirdQuantity = value; }

		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.CustomsUnitOfQuantityList))]
		public override ZString BY_CustomsThirdUnitQty { get => base.BY_CustomsThirdUnitQty; set => base.BY_CustomsThirdUnitQty = value; }

		#endregion

		#region BY_CustomsFourthQuantity

		[ResourceStringData("C0B99121-3B27-4AB4-83E4-0DB80AF05F0E", Caption = "Fourth Quantity", MediumCaption = "Fourth Qty", ShortCaption = "Fourth Qty", FullDescription = "Fourth Additional Quantity for Liability Amount Calculation")]
		[MeasureUnit(nameof(BY_CustomsFourthUnitQty), MeasureUnitType.Unknown)]
		public override ZDecimal BY_CustomsFourthQuantity { get => base.BY_CustomsFourthQuantity; set => base.BY_CustomsFourthQuantity = value; }

		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.CustomsUnitOfQuantityList))]
		public override ZString BY_CustomsFourthUnitQty { get => base.BY_CustomsFourthUnitQty; set => base.BY_CustomsFourthUnitQty = value; }

		#endregion

		#region Supplementary codes

		[ResourceStringData("83F7B44D-BE78-4B12-80DC-B8C822CD8565", Caption = "Supplementary Codes", MediumCaption = "Sup. Codes", ShortCaption = "Sup. Codes")]
		public ZString BY_Supplements => AdditionalSupplementaryCodes.AsString;

		public ZPropertyInfo BY_SupplementsInfo => GetZPropertyInfo(nameof(BY_Supplements));

		[ChildEditable(true)]
		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.AdditionalCodeList))]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public virtual SupplementaryCodeCollection AdditionalSupplementaryCodes
		{
			get
			{
				if (additionalSupplementaryCodes == null)
				{
					additionalSupplementaryCodes = SupplementaryCodeCollection.New(BY_SupplementsInfo);
					RegisterEditableChildObject(additionalSupplementaryCodes);
					additionalSupplementaryCodes.SubscribeToChildrenChanges(UpdateAllFeesFromTariffRates, new[] { CusCodeDataSchema.Constants.CY_Code });
				}

				return additionalSupplementaryCodes;
			}
		}
		SupplementaryCodeCollection additionalSupplementaryCodes;

		ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => AdditionalSupplementaryCodes;

		public IEnumerable<SupplementaryCode> SupplementaryCodes => AdditionalSupplementaryCodes.OfType<SupplementaryCode>();

		IEnumerable<BaseSupplementaryCode> ISupplementaryCodeSupporter.SupplementaryCodes => SupplementaryCodes;

		TariffView ICusCodeDataWithOrderSupporter.Tariff => UniversalTariff;

		IZZRateSelectionCriteria ICusCodeDataWithOrderSupporter.RateSelectionCriteria => AllApplicableRatesSelectionCriteria;

		CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => Lookups.CachedListOfAdditionalCodeDescriptions;

		ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => nameof(FieldType.TextDropEdit);
		ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => null;

		void ICusCodeDataWithOrderSupporter.OnCodesChanged()
		{
		}

		public ZString GetCountryCodeForCodeProvider() => Header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public IZZRateSelectionCriteria AllApplicableRatesSelectionCriteria => Factory.GetValue(ref allApplicableRatesSelectionCriteria, GetAllApplicableRatesSelectionCriteriaCore);
		CachedProperty<IZZRateSelectionCriteria> allApplicableRatesSelectionCriteria;

		protected virtual IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCore() => new SpecificRateSelectionCriteria(EffectiveCountryOfOrigin, DataGroupingCode, "", "", new HashSet<ZString>() { }, ValuationDate, "", "");

		public ZString GetCountryCodeFromAdditionalCode(ZString additionalCode) => ZString.Empty;

		#endregion

		[ChildEditable(true)]
		public ICusInBondFeeCollection<NctsCargoDescFee> Fees
		{
			get
			{
				if (fees == null)
				{
					fees = GetNctsCargoDescFeeCollection();
					RegisterEditableChildObject(fees);
				}
				return fees;
			}
		}
		ICusInBondFeeCollection<NctsCargoDescFee> fees;

		protected virtual ICusInBondFeeCollection<NctsCargoDescFee> GetNctsCargoDescFeeCollection() => new CusInBondFeeCollection<NctsCargoDescFee>(this);

		protected virtual GoodsItemDutyCalculator CreateGoodsItemDutyCalculator() => new(this);

		protected readonly struct FeeData
		{
			public FeeData(ZDecimal baseValue, ZString methodOfCalculation, ZDecimal amount, ZDecimal rate)
			{
				BaseValue = baseValue;
				MethodOfCalculation = methodOfCalculation;
				Amount = amount.Round(2);
				Rate = rate * (methodOfCalculation == "%" ? 100 : 1);
			}

			public ZDecimal BaseValue { get; }
			public ZString MethodOfCalculation { get; }
			public ZDecimal Amount { get; }
			public ZDecimal Rate { get; }
		}

		protected void UpdateFeesForType(ZString feeType, RateView rateView)
		{
			var newFees = new List<FeeData>();
			AddFeesForType(newFees, rateView);
			UpdateFeesForType(feeType, newFees);
		}

		protected void AddFeesForType(List<FeeData> newFees, RateView rateView)
		{
			if (rateView != null)
			{
				var calculationResult = CreateGoodsItemDutyCalculator().Calculate(rateView);
				foreach (var intermediateResult in calculationResult.IntermediateResults)
				{
					newFees.Add(new FeeData(intermediateResult.BaseValue, intermediateResult.MethodOfCalculation, intermediateResult.Amount, intermediateResult.Rate));
				}
			}
		}

		protected void UpdateFeesForType(ZString feeType, List<FeeData> newFees)
		{
			var existingFees = Fees.Where(x => x.BFE_ChargeType == feeType).ToList();
			var newFeeCount = newFees.Count;
			var existingFeeCount = existingFees.Count;
			var maxCount = Math.Max(newFeeCount, existingFeeCount);
			for (var i = 0; i < maxCount; ++i)
			{
				if (i >= existingFeeCount)
				{
					existingFees.Add(Fees.AddNew());
				}
				if (i < newFeeCount)
				{
					existingFees[i].BFE_ChargeType = feeType;
					existingFees[i].BFE_BaseValue = newFees[i].BaseValue;
					existingFees[i].BFE_MethodOfCalculation = newFees[i].MethodOfCalculation.Left(CusInBondFee.Schema.BFE_MethodOfCalculationMaxLength);
					existingFees[i].BFE_ChargeAmount = newFees[i].Amount;
					existingFees[i].BFE_Rate = newFees[i].Rate;
				}
				else
				{
					existingFees[i].Delete();
				}
			}
		}

		public virtual void UpdateAllFeesFromTariffRates()
		{
			UpdateFeesFromTariffRates(updateDuty: true, updateADD: true, updateCVD: true);
		}

		internal protected void UpdateFeesFromTariffRates(bool updateDuty = false, bool updateADD = false, bool updateCVD = false)
		{
			if (updateDuty)
			{
				UpdateFeesForType(ChargeType.Duty, HighestDuty);
			}
			if (updateADD)
			{
				UpdateFeesForType(ChargeType.AntiDumpingDuty, HighestAntiDumpingDuty);
			}
			if (updateCVD)
			{
				UpdateFeesForType(ChargeType.CountervailingDuty, HighestCountervailingDuty);
			}

			GetZPropertyInfo(nameof(DutyAmount)).RefreshBinding();
			GetZPropertyInfo(nameof(AntiDumpingDutyAmount)).RefreshBinding();
			GetZPropertyInfo(nameof(CountervailingDutyAmount)).RefreshBinding();
		}

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ??= new NctsCommonCargoDescValueSetStrategy(this);
		IValueSetStrategy valueSetStrategy;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new NctsCommonCargoDescFetchStrategy(this);

		#region ICusCodeDataTypeSupporter Implementation

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ EU.Business.CusCodeDataTypeList.Codes.SupplementaryCode, typeof(SupplementaryCode) }
			};
		}

		#endregion

		public ZString EffectiveCountryOfOrigin
		{
			get { return BY_RN_NKCountryOfOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : BY_RN_NKCountryOfOrigin; }
		}

		public IEnumerable<IStatement> SpecialMentions => new SpecialMentionProvider(this);

		[ChildEditable(true)]
		public INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments //99
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = GetNewNctsSupportingDocumentCollection();
					supportingDocuments.Load();
					if (ShouldSetSupportingDocumentsReadOnly)
					{
						supportingDocuments.SetReadOnlyIncludingChildren(true);
					}
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}
		INctsSupportingDocumentCollection<NctsSupportingDocument> supportingDocuments;

		protected virtual INctsSupportingDocumentCollection<NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected virtual bool ShouldSetSupportingDocumentsReadOnly => false;

		[ChildEditable(true)]
		public INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => additionalInfos ?? (additionalInfos = LoadAdditionalInfoCollection());
		INctsAdditionalInfoCollection<NctsAdditionalInfo> additionalInfos;

		INctsAdditionalInfoCollection<NctsAdditionalInfo> LoadAdditionalInfoCollection()
		{
			var result = GetNctsAdditionalInfoCollection();
			result.Load();
			if (ShouldSetAdditionalInfosReadOnly)
			{
				result.SetReadOnlyIncludingChildren(true);
			}
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual INctsAdditionalInfoCollection<NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);
		protected virtual bool ShouldSetAdditionalInfosReadOnly => false;

		[ChildEditable]
		public INctsPackageCollection<NctsPackage, NctsCommonCargoDesc> Packages
		{
			get
			{
				if (packages == null)
				{
					packages = GetNctsPackageCollection();
					packages.Load();
					RegisterEditableChildObject(packages);
				}
				return packages;
			}
		}
		INctsPackageCollection<NctsPackage, NctsCommonCargoDesc> packages;

		protected virtual INctsPackageCollection<NctsPackage, NctsCommonCargoDesc> GetNctsPackageCollection() => new NctsPackageCollection<NctsPackage, NctsCommonCargoDesc>(this);

		protected virtual bool AutomaticSequenceNumberEnabled => true;

		public IEnumerable<ISgiCode> SgiCodes => new SgiCodeProvider(this); //max=9

		public IBusiness TemplateCopy(ZGuid parentID)
		{
			var result = (NctsCommonCargoDesc)new NctsDeepCloneStrategy(this, parentID).Clone();
			result.HasChanges = false;
			return result;
		}

		public override bool CanDelete
		{
			get
			{
				var moveHeader = MoveHeader;
				var result = true;
				if (moveHeader != null)
				{
					result = (moveHeader.IsUnloadingMovementHeader && BY_LineNo > moveHeader.GoodsItems.Count) ||
						((moveHeader.IsArrivalMovementHeader || moveHeader.IsDepartureMovementHeader) && base.CanDelete);
				}
				return result;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				if (MoveHeader != null)
				{
					result = ResString.GetMultilingualString("E6B992E2-E407-4B4C-99D7-1446C510F4B1", "You cannot delete this item. Mark it as ‘Missing’ instead.");
				}
				else if (IsPhase5Arrival)
				{
					result = ResString.GetMultilingualString("28DC7908-D8F4-4631-B53C-C508ABBA3CCA", "Cannot delete Goods Item from Customs.");
				}
				return result;
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				if (AutomaticSequenceNumberEnabled)
				{
					MoveHeaderOrBillParent?.LineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				Fees.DeleteAll();
			}
			base.Delete();
		}

		#region ICanBeImportOrExport Members

		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.Level => EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		ZBool EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.IsExport => false;

		ZBool EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.IsImport => false;

		void EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.TrueCountryCode => DataGroupingCode;

		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.DataGroupingCode => DataGroupingCode;

		#endregion

		#region ICusSupportingInfoTypeSupporter Remarks

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
			{ CusSupportingInfoTypeList.Codes.PreviousDocument, PreviousDocumentType },
			{ CusSupportingInfoTypeList.Codes.SupportingDocument, SupportingDocumentType },
			{ CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoType },
		};

		protected virtual Type AdditionalInfoType => NctsTypeDecider.GetNctsAdditionalInfoType(Factory, CountryCode);
		protected virtual Type SupportingDocumentType => typeof(NctsSupportingDocument);
		protected virtual Type PreviousDocumentType => typeof(NctsPreviousDocument);

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies() => GetAdditionalBusinessObjectFetchStrategies();

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion ICusSupportingInfoTypeSupporter Remarks

		#region Line number auto calculation

		ZGuid ISequenceNumberLine.FKToHeader => BY_ParentID;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => BY_LineNo;
			set => BY_LineNo = value;
		}

		#endregion

		#region ICusInvPackTypeSupporter

		Type ICusInvPackTypeSupporter.PackType => NctsPackage.TypeDecider.GetTypeForCountryCode(DataGroupingCode);

		TypeDecider ICusInvPackTypeSupporter.PackTypeDecider => NctsPackage.TypeDecider;

		#endregion

		#region ICusInBondFeeTypeSupporter
		protected override Type FeeTypeCore => NctsCargoDescFee.TypeDecider.GetTypeForCountryCode(Header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		#endregion

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newGoodsItem = (NctsCommonCargoDesc)base.CloneInternal(args);

			CloneInternalPackages(args, newGoodsItem);

			CloneInternalAdditionalInfos(args, newGoodsItem);
			CloneInternalSupportingDocuments(args, newGoodsItem);
			return newGoodsItem;
		}

		protected virtual void CloneInternalPackages(BusinessObjectCloneArgs args, NctsCommonCargoDesc newGoodsItem)
		{
			newGoodsItem.Packages.AddCloneFrom(Packages, args);
		}

		protected virtual void CloneInternalAdditionalInfos(BusinessObjectCloneArgs args, NctsCommonCargoDesc newGoodsItem)
		{
			newGoodsItem.AdditionalInfos.AddCloneFrom(AdditionalInfos, args);
		}

		protected virtual void CloneInternalSupportingDocuments(BusinessObjectCloneArgs args, NctsCommonCargoDesc newGoodsItem)
		{
			newGoodsItem.SupportingDocuments.AddCloneFrom(SupportingDocuments, args);
		}

		public void WipeCommercialReferenceNumber()
		{
			BY_CommercialReferenceNumber = string.Empty;
		}

		protected override bool SupportsCloneCore() => true;

		protected virtual NonPersistentDepartureContainerPivotCollection GetContainersPivots() => new NonPersistentDepartureContainerPivotCollection(this);

		public IReadOnlyList<string> MultipleKeysToUse => Header?.MultipleKeysToUse ?? Array.Empty<string>();

		[ChildEditable]
		public NonPersistentDepartureContainerPivotCollection ContainersPivots
		{
			get
			{
				if (containersPivots == null)
				{
					containersPivots = GetContainersPivots();
					RegisterEditableChildObject(containersPivots);
				}
				return containersPivots;
			}
		}

		NonPersistentDepartureContainerPivotCollection containersPivots;

		#region ISequenceNumberHeader

		public ShortSequenceNumberGenerator RefSequenceNumberGenerator
		{
			get
			{
				return refLineNumberGenerator ?? (refLineNumberGenerator = new ShortSequenceNumberGenerator(this, (x) => ((NctsAdditionalInfo)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference));
			}
		}
		ShortSequenceNumberGenerator refLineNumberGenerator;

		public ShortSequenceNumberGenerator InfSequenceNumberGenerator
		{
			get
			{
				return infLineNumberGenerator ?? (infLineNumberGenerator = new ShortSequenceNumberGenerator(this, (x) => ((NctsAdditionalInfo)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation));
			}
		}
		ShortSequenceNumberGenerator infLineNumberGenerator;

		public ShortSequenceNumberGenerator TraSequenceNumberGenerator
		{
			get
			{
				return traLineNumberGenerator ?? (traLineNumberGenerator = new ShortSequenceNumberGenerator(this, (x) => ((NctsAdditionalInfo)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument));
			}
		}
		ShortSequenceNumberGenerator traLineNumberGenerator;

		public IEnumerable<ISequenceNumberLine> Lines => AdditionalInfos;

		public ShortSequenceNumberGenerator LineNumberGenerator => new ShortSequenceNumberGenerator(this);

		#endregion

		#region ZZConditionSelectionCriteria

		public IZZConditionSelectionCriteria ConditionSelectionCriteria => Factory.GetValue(ref conditionSelectionCriteria, GetConditionSelectionCriteriaCore);
		CachedProperty<IZZConditionSelectionCriteria> conditionSelectionCriteria;
		protected virtual IZZConditionSelectionCriteria GetConditionSelectionCriteriaCore() => new ZZConditionSelectionCriteria<NctsCommonCargoDesc>(this);

		#endregion

		#region Calculated Amounts

		[ResourceStringData("EU.NCTS.NctsDepartureCargoDesc|DutyAmount", Caption = "Duty Amount")]
		public ZDecimal DutyAmount => GetFeeTotalAmount(ChargeType.Duty);

		public ZPropertyInfo DutyAmountInfo => GetZPropertyInfo(nameof(DutyAmount));

		[ResourceStringData("EU.NCTS.NctsDepartureCargoDesc|VatAmount", Caption = "Vat Amount")]
		public ZDecimal VatAmount => GetFeeTotalAmount(ChargeType.VAT);

		public ZPropertyInfo VatAmountInfo => GetZPropertyInfo(nameof(VatAmount));

		protected virtual ZDecimal OtherFeesAmount => GetFeeTotalAmount(ChargeType.AntiDumpingDuty) + GetFeeTotalAmount(ChargeType.CountervailingDuty);

		public RateView HighestDuty
		{
			get
			{
				var tradeGroupCountry = BY_RN_NKCountryOfOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : BY_RN_NKCountryOfOrigin;
				var supplementaryCodesForCachedValue = ZString.Join("_", SupplementaryCodes.Select(x => x.CY_Code).ToArray());
				var effectiveDate = ValuationDate;
				var dataGroupingCode = DataGroupingCode;
				return Factory.GetCachedValue(FormattableString.Invariant($"EU.NCTS.NctsDepartureCargoDesc.HighestDuty_{tradeGroupCountry}_{dataGroupingCode}_{EffectiveTariffForCalculateFields}_{effectiveDate}_{supplementaryCodesForCachedValue}_{BY_MonetaryValue}"), () =>
				{
					var primaryPreferenceCode = "100";
					var rateType = Constants.RateTypes.Duty;
					var criteria = new SpecificRateSelectionCriteria(tradeGroupCountry, dataGroupingCode, primaryPreferenceCode, "", new HashSet<ZString>(SupplementaryCodes.Select(x => x.CY_Code)), effectiveDate, rateType, "");

					RateView result = GetApplicableRateFromCriteria(criteria);

					if (result is null)
					{
						var addCode = UniversalTariff?.GetAdditionalCodesForDuty(tradeGroupCountry, effectiveDate, dataGroupingCode, primaryPreferenceCode).OrderByDescending(x => x).FirstOrDefault() ?? ZString.Empty;
						var criteria2 = new SpecificRateSelectionCriteria(tradeGroupCountry, dataGroupingCode, primaryPreferenceCode, "", new HashSet<ZString>() { addCode }, effectiveDate, rateType, "");
						result = GetApplicableRateFromCriteria(criteria2);
					}

					return result;
				});
			}
		}

		[ResourceStringData("EU.NCTS.NctsDepartureCargoDesc|AntiDumpingDutyAmount", Caption = "Antidumping Duty Amount", ShortCaption = "Antidumping Duty")]
		public ZDecimal AntiDumpingDutyAmount => GetFeeTotalAmount(ChargeType.AntiDumpingDuty);

		public ZPropertyInfo AntiDumpingDutyAmountInfo => GetZPropertyInfo(nameof(AntiDumpingDutyAmount));

		public RateView HighestAntiDumpingDuty
		{
			get
			{
				var tradeGroupCountry = BY_RN_NKCountryOfOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : BY_RN_NKCountryOfOrigin;
				var addAdditionalCodesForCachedValue = ZString.Join("_", ADDAdditionalCodes.ToArray());
				var effectiveDate = ValuationDate;
				var rateType = Constants.RateTypes.AntiDumping;
				var primaryPreferenceCode = "100";
				var dataGroupingCode = DataGroupingCode;
				return Factory.GetCachedValue(FormattableString.Invariant($"EU.NCTS.NctsDepartureCargoDesc.HighestAntiDumpingDuty_{tradeGroupCountry}_{dataGroupingCode}_{EffectiveTariffForCalculateFields}_{effectiveDate}_{addAdditionalCodesForCachedValue}"), () =>
				{
					var criteria = new SpecificRateSelectionCriteria(tradeGroupCountry, dataGroupingCode, primaryPreferenceCode, "", new HashSet<ZString>(ADDAdditionalCodes), effectiveDate, rateType, "");
					return GetApplicableRateFromCriteria(criteria);
				});
			}
		}

		[ResourceStringData("EU.NCTS.NctsDepartureCargoDesc|CountervailingDutyAmount", Caption = "Countervailing Duty Amount", ShortCaption = "Countervailing Duty")]
		public ZDecimal CountervailingDutyAmount => GetFeeTotalAmount(ChargeType.CountervailingDuty);

		public ZPropertyInfo CountervailingDutyAmountInfo => GetZPropertyInfo(nameof(CountervailingDutyAmount));

		public RateView HighestCountervailingDuty
		{
			get
			{
				var tradeGroupCountry = BY_RN_NKCountryOfOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : BY_RN_NKCountryOfOrigin;
				var cvdAdditionalCodesForCachedValue = ZString.Join("_", CVDAdditionalCodes.ToArray());
				var effectiveDate = ValuationDate;
				var rateType = Constants.RateTypes.Countervailing;
				var primaryPreferenceCode = "100";
				var dataGroupingCode = DataGroupingCode;
				return Factory.GetCachedValue(FormattableString.Invariant($"EU.NCTS.NctsDepartureCargoDesc.HighestCountervailingDuty_{tradeGroupCountry}_{dataGroupingCode}_{EffectiveTariffForCalculateFields}_{effectiveDate}_{cvdAdditionalCodesForCachedValue}"), () =>
				{
					var criteria = new SpecificRateSelectionCriteria(tradeGroupCountry, dataGroupingCode, primaryPreferenceCode, "", new HashSet<ZString>(CVDAdditionalCodes), effectiveDate, rateType, "");
					return GetApplicableRateFromCriteria(criteria);
				});
			}
		}

		protected ZDecimal GetFeeTotalAmount(ZString feeType) => Fees.Cast<NctsCargoDescFee>().Where(x => x.BFE_ChargeType == feeType).Sum(x => x.BFE_ChargeAmount);

		public virtual ZDecimal LiabilityAmount => Factory.GetValue(ref liabilityAmountCached, GetLiabilityAmount);
		CachedProperty<ZDecimal> liabilityAmountCached;

		public ZPropertyInfo LiabilityAmountInfo => GetZPropertyInfo(nameof(LiabilityAmount));

		ZDecimal GetLiabilityAmount() => Header.IsPhase4 ? DutyAmount + AntiDumpingDutyAmount + CountervailingDutyAmount : DutyLiabilityAmount + VATLiabilityAmount + OtherLiabilityAmount;

		ZDecimal DutyLiabilityAmount => (ZDecimal)(DutyAmount * (Header.GetEffectiveGuarantees().FirstOrDefault()?.CusGuarantee?.RateForDuty ?? 100m)) / 100m;

		ZDecimal VATLiabilityAmount => (ZDecimal)(VatAmount * (Header.GetEffectiveGuarantees().FirstOrDefault()?.CusGuarantee?.RateForVAT ?? 100m)) / 100m;

		ZDecimal OtherLiabilityAmount => (ZDecimal)(OtherFeesAmount * (Header.GetEffectiveGuarantees().FirstOrDefault()?.CusGuarantee?.RateForOtherFees ?? 100m)) / 100m;

		public IEnumerable<ZString> ADDAdditionalCodes
		{
			get
			{
				var additionalCodesForAntiDumping = UniversalTariff?.GetAdditionalCodesForAntiDumping(BY_RN_NKCountryOfOrigin, ValuationDate) ?? Array.Empty<ZString>();
				return GetAdditionalCodes(additionalCodesForAntiDumping);
			}
		}

		public IEnumerable<ZString> CVDAdditionalCodes
		{
			get
			{
				var additionalCodesForCountervailing = UniversalTariff?.GetAdditionalCodesForCountervailing(BY_RN_NKCountryOfOrigin, ValuationDate) ?? Array.Empty<ZString>();
				return GetAdditionalCodes(additionalCodesForCountervailing);
			}
		}

		IEnumerable<ZString> GetAdditionalCodes(IEnumerable<ZString> additionalCodes)
		{
			var supplementaryCodes = SupplementaryCodes.Select(x => x.CY_Code);

			if (additionalCodes.Any(x => supplementaryCodes.Contains(x)))
			{
				return supplementaryCodes;
			}
			else
			{
				var worstCaseAdditionalCodes = EUUniversalLookupsHelper.GetWorstCaseAdditionalCodes(Factory, DataGroupingCode, ValuationDate).GetAllCodesZString();
				if (additionalCodes.Any(x => worstCaseAdditionalCodes.Contains(x)))
				{
					return worstCaseAdditionalCodes;
				}
			}
			return Array.Empty<ZString>();
		}

		protected RateView GetApplicableRateFromCriteria(SpecificRateSelectionCriteria criteria)
		{
			var applicableRates = UniversalTariff?.GetApplicableRates(criteria);

			RateView result = null;

			if (applicableRates != null)
			{
				var applicableRatesCount = applicableRates.Count();
				if (applicableRatesCount == 1)
				{
					result = applicableRates.First();
				}
				else if (applicableRatesCount > 1)
				{
					result = applicableRates.Where(x => x.RateApplicabilities.Any(y => y.ZZT_AdditionalCode != ""))
											.OrderByDescending(x => x.ZZ2_StartDate)
											.ThenBy(x => x.PreferenceCode)
											.FirstOrDefault();
					if (result is null)
					{
						var calculator = new GoodsItemDutyCalculator(this);
						result = applicableRates.MaxBy(p => calculator.Calculate(p).ResultAmount);
					}
				}
			}

			return result;
		}

		#endregion

		#region Default UOMS

		protected void DefaultTariffUnitOfMeasures()
		{
			if (!EffectiveTariffForCalculateFields.IsEmpty)
			{
				BY_CustomsSecondUnitQty = UniversalTariff?.GetSpecificUOM(CustomsSecondUnitQtySpecificUOM) ?? ZString.Empty;
				DefaultUOMsThirdAndFourthUnitQty();
			}
		}

		protected void SetDefaultTariffUnitOfMeasuresFromRatesView()
		{
			if (!EffectiveTariffForCalculateFields.IsEmpty && !BY_RN_NKCountryOfOrigin.IsEmpty)
			{
				UOMDefaulter.DefaultUOMIfApplicable(HighestDuty, this);
				UOMDefaulter.DefaultUOMIfApplicable(HighestAntiDumpingDuty, this);
				UOMDefaulter.DefaultUOMIfApplicable(HighestCountervailingDuty, this);
			}
		}

		protected virtual string CustomsSecondUnitQtySpecificUOM => Constants.UnitOfMeasureTypes.AdditionalUOMType;

		protected void DefaultUOMsThirdAndFourthUnitQty()
		{
			if (UniversalTariff is TariffView tariff)
			{
				var tradeGroupCountry = BY_RN_NKCountryOfOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : BY_RN_NKCountryOfOrigin;
				var tariffUoms = tariff.UnitsOfMeasure
					.Where(uom => uom.ZZ8_Type.In(new ZString[] { Constants.UnitOfMeasureTypes.CustomsUOM3Type, Constants.UnitOfMeasureTypes.CustomsUOM4Type, Constants.UnitOfMeasureTypes.CustomsUOM5Type }))
					.Where(uom => uom.CusTradeGroup?.TradeGroupCountries.Any(country => country.ZZB_RN_NKTradeGroupCountryCode == tradeGroupCountry) ?? true)
					.OrderBy(x => x.ZZ8_Type).ThenBy(x => x.ZZ8_UOM).Select(x => x.ZZ8_UOM).ToArray();

				var tariffUomsAlreadySet = tariff.UnitsOfMeasure
					.Where(uom => uom.ZZ8_Type.In(new ZString[] { Constants.UnitOfMeasureTypes.StatisticalUOMType, Constants.UnitOfMeasureTypes.AdditionalUOMType }))
					.Where(uom => uom.CusTradeGroup?.TradeGroupCountries.Any(country => country.ZZB_RN_NKTradeGroupCountryCode == tradeGroupCountry) ?? true)
					.OrderBy(x => x.ZZ8_Type).ThenBy(x => x.ZZ8_UOM).Select(x => x.ZZ8_UOM).ToArray();

				var rateUoms = HighestDuty?.UnitsOfMeasure.Select(x => x.ZXG_UOM).Where(x => !RateCalcUnitOfMeasureAggregator.IsConvertableFrom(x, tariffUomsAlreadySet.Union(tariffUoms), Header.CountryCode, Factory)).OrderBy(x => x) ?? Enumerable.Empty<ZString>();
				var distinctUOMs = new Queue<ZString>(tariffUoms.Union(rateUoms.Where(x => !tariffUoms.Contains(x))).Distinct());

				var thirdUnit = ZString.Empty;
				var fourthUnit = ZString.Empty;
				if (distinctUOMs.Any())
				{
					thirdUnit = distinctUOMs.First();
					fourthUnit = distinctUOMs.FirstOrDefault(u => u != thirdUnit);
				}
				DefaultUOMsExtraThirdUnitQty(thirdUnit);
				BY_CustomsThirdUnitQtyInfo.Value = thirdUnit.SubstringSafe(0, BY_CustomsThirdUnitQtyInfo.MaxLength);
				BY_CustomsFourthUnitQtyInfo.Value = fourthUnit.SubstringSafe(0, BY_CustomsFourthUnitQtyInfo.MaxLength);
			}
		}

		protected virtual void DefaultUOMsExtraThirdUnitQty(ZString thirdUnit) { }

		public IReadOnlyCollection<HashSet<string>> ConvertibleUnitsOfMeasureSets => convertibleUnitsOfMeasureSets ??= GetUnitOfMeasureConversionSets();
		IReadOnlyCollection<HashSet<string>> convertibleUnitsOfMeasureSets;

		protected virtual List<HashSet<string>> GetUnitOfMeasureConversionSets()
		{
			return new List<HashSet<string>>()
			{
				ConvertibleUnitsOfMeasure.WeightConversionDictionary.Keys.ToHashSet(),
				ConvertibleUnitsOfMeasure.VolumeConversionDictionary.Keys.ToHashSet(),
				ConvertibleUnitsOfMeasure.AlcoholConversionDictionary.Keys.ToHashSet(),
			};
		}

		#endregion

		public static class ChargeType
		{
			public static readonly ZString Duty = "DTY";
			public static readonly ZString AntiDumpingDuty = "ADD";
			public static readonly ZString CountervailingDuty = "CVD";
			public static readonly ZString VAT = "VAT";
		}
	}
}
