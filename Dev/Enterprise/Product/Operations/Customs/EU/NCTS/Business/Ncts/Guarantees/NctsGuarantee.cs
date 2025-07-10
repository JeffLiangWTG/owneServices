using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NctsGuaranteeCodes = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsGuarantee : CommonGuarantee
	{
		public NctsGuarantee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public NctsHeader NctsHeader => nctsHeader ??= GetNctsHeader();
		NctsHeader nctsHeader;

		NctsHeader GetNctsHeader()
		{
			NctsHeader result;
			switch (PW_ParentTableCode.ToUpper())
			{
				case CusInBondHeaderSchema.Constants.Prefix:
					result = Factory.Load<NctsHeader>(PW_ParentID);
					break;
				case CusInBondMoveHeaderSchema.Constants.Prefix:
					result = Factory.Load<NctsCommonMovementHeader>(PW_ParentID)?.Header;
					break;
				default:
					result = null;
					break;
			}
			return result;
		}

		public INctsGuaranteeValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsGuaranteeValidationDecider> validationDeciderCached;

		INctsGuaranteeValidationDecider GetValidationDecider() => NctsHeader?.Configuration.GuaranteeConfiguration.GetValidationDecider(NctsHeader);

		public override ZGuid PW_ParentID
		{
			get => base.PW_ParentID;
			set
			{
				var oldValue = PW_ParentID;
				base.PW_ParentID = value;
				if (!IsCopying && oldValue != PW_ParentID)
				{
					if (PW_ParentTableCode == CusInBondHeaderSchema.Constants.Prefix && IsPhase5Departure)
					{
						throw new DeveloperNotificationException("Trying to add NctsGuarantee on NctsHeader in Phase 5 departure. For Phase 5 Departure NctsGuarantee should be added to MovementHeader");
					}
				}
			}
		}

		public override ZString PW_ParentTableCode
		{
			get => base.PW_ParentTableCode;
			set
			{
				var oldValue = PW_ParentTableCode;
				base.PW_ParentTableCode = value;
				if (!IsCopying && oldValue != PW_ParentTableCode)
				{
					if (value == CusInBondHeaderSchema.Constants.Prefix && IsPhase5Departure)
					{
						throw new DeveloperNotificationException("Trying to add NctsGuarantee on NctsHeader in Phase 5 departure. For Phase 5 Departure NctsGuarantee should be added to MovementHeader");
					}
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PW_ApplicationCode = ApplicationCodeList.Codes.EuNcts;
		}

		public void SetDefaultsAfterParentIsSet()
		{
			var liabilityAmount = Phase5DefaultLiabilityAmount;
			if (!liabilityAmount.IsEmpty && IsPhase5)
			{
				SetLiabilityAmount(liabilityAmount);
			}
		}

		public void SetLiabilityAmount(ZDecimal amount)
		{
			if (!PW_Override)
			{
				if (IsPhase5Arrival)
				{
					PW_BondAmount = LiabilityAmount;
				}
				else if (amount.IsEmpty && IsPhase5)
				{
					PW_BondAmount = Phase5DefaultLiabilityAmount;
				}
				else
				{
					var amountFactor = liabilityAmountFactors.GetValueOrDefault(PW_SuretyCode, 1);
					PW_BondAmount = ((ZDecimal)(amount * amountFactor)).Round(2);
				}
			}
		}

		protected virtual ZDecimal Phase5DefaultLiabilityAmount => ZDecimal.Zero;

		protected override TypeLoaderCollection ParentLoaders
		{
			get
			{
				var result = base.ParentLoaders;
				result.Add(typeof(CusInBondHeader));
				return result;
			}
		}

		public GuaranteeApportionmentType ApportionmentType => GetApportionmentTypeCore();

		protected virtual GuaranteeApportionmentType GetApportionmentTypeCore() => GuaranteeApportionmentType.None;

		protected sealed override MasterFiles.Business.CusBondDetailValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual NctsGuaranteePhase5Validation GetNewPhase5Validation() => new NctsGuaranteePhase5Validation(this);

		protected virtual NctsGuaranteeValidation GetNewPhase4Validation() => new NctsGuaranteeValidation(this);

		protected override MasterFiles.Business.CusBondDetailLookups GetNewLookups() => new NctsGuaranteeLookups(this);
		public new NctsGuaranteeLookups Lookups => (NctsGuaranteeLookups)base.Lookups;

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = new NctsGuaranteeValueSetStrategy(this));
		IValueSetStrategy valueSetStrategy;

		protected override ZString HumanReadableNameCore => Res.GetString("6B655BA7-A581-4991-AE5D-8A2C841AF3F2", "Guarantee");

		public override CusGuaranteeHeader CusGuarantee
		{
			get
			{
				CusGuaranteeHeader cusGuarantee = null;
				if (NctsHeader != null)
				{
					var possibleCusGuarantees = Factory.Load<CusGuaranteeHeader>(CusGuaranteeFilter);
					cusGuarantee = possibleCusGuarantees.FirstOrDefault(CusGuaranteeTypeFilter);
				}
				return cusGuarantee;
			}
		}

		protected virtual Func<CusGuaranteeHeader, bool> CusGuaranteeTypeFilter => cusGuarantee => NctsHeader.IsPhase5Arrival
																										? cusGuarantee.CPH_Type == EUGuaranteeTypeList.Codes.TST
																										: cusGuarantee.IsPermitGuaranteeType;

		public CusGuaranteeHeader CusGuaranteeWithoutPermitHolder => cusGuaranteeWithoutPermitHolder ?? (cusGuaranteeWithoutPermitHolder = GetGuaranteeHeader());
		CusGuaranteeHeader cusGuaranteeWithoutPermitHolder;

		CusGuaranteeHeader GetGuaranteeHeader()
		{
			return Factory
				.Load<CusGuaranteeHeader>(LatestPermitHeaderMatchingBondNumberAndC00009CountryFilter)
				.FirstOrDefault(CusGuaranteeTypeFilter);
		}

		public CusGuaranteeHeader CusGuaranteeWithSubTypeFilter
		{
			get
			{
				CusGuaranteeHeader result = null;
				if (NctsHeader != null)
				{
					CusGuaranteeFilter.AddToFilter(CusPermitHeaderSchema.CPH_SubType, PW_BondType);
					var possibleguarantees = Factory.Load<CusGuaranteeHeader>(CusGuaranteeFilter);
					if (possibleguarantees != null && possibleguarantees.Length == 1)
					{
						result = possibleguarantees.FirstOrDefault(CusGuaranteeTypeFilter);
					}
				}
				return result;
			}
		}

		public ZQuery CusGuaranteeFilter
		{
			get
			{
				return NctsHeader?.IsPhase5Arrival ?? false
				? LatestPermitHeaderMatchingBondNumberAndC00009CountryFilter
				: LatestPermitHeaderMatchingBondNumberAndC00009CountryFilter.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, NctsHeader?.Principal?.Address?.OA_OH ?? ZGuid.Empty);
			}
		}

		public EU.Business.CusGuaranteeRule LapPermitRule => lapPermitRule ?? (lapPermitRule = GetPermitRule());
		EU.Business.CusGuaranteeRule lapPermitRule;

		EU.Business.CusGuaranteeRule GetPermitRule()
		{
			return CusGuaranteeWithoutPermitHolder?.CusGuaranteeRules
				.Cast<EU.Business.CusGuaranteeRule>()
				.FirstOrDefault(x => x.CPR_RuleCode == EU.Business.PermitRuleCodeList.Codes.LAP);
		}

		protected virtual ZBool PW_BondNumber2ReadOnly => IsPhase5 && PW_BondType != EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee && PW_BondType != EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies;

		protected virtual ZBool PW_OverrideReadOnly => (NctsHeader?.Configuration.GuaranteeConfiguration.ApplySecurityToPW_Override(NctsHeader) ?? false)
			&& !Environment.Env.Security.NctsDepartureAllowBondAmountOverride.IsAllowed;

		internal ZQuery LatestPermitHeaderMatchingBondNumberAndC00009CountryFilter
		{
			get
			{
				var header = NctsHeader;
				var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Number, PW_BondNumber);
				var countryCodes = header?.GetC0009CountryCodes() ?? Array.Empty<ZString>();
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCodes);
				query.OrderBy = CusPermitHeaderSchema.Constants.CPH_StartDate + OrderByClause.Descending + "," + CusPermitHeaderSchema.Constants.CPH_SystemCreateTimeUtc + OrderByClause.Descending;
				return query;
			}
		}

		public void SetLiabilityAmountWithConsumeAllIfPositive(ZDecimal apportionedAmount)
		{
			var liabilityAmount = CusGuarantee?.CPH_Calc_OpeningBalance ?? apportionedAmount;
			if (!PW_Override && liabilityAmount > 0)
			{
				PW_BondAmount = liabilityAmount;
			}
		}

		public void DefaultSuretyCodeIfHasMatchLapPermitRule()
		{
			if (HasMatchingLapPermitRule && !PW_Override)
			{
				PW_SuretyCode = GetDefaultValueFromLapPermitRule();
			}

			ZString GetDefaultValueFromLapPermitRule() => LapPermitRule.CPR_ValueFrom.Left(3);
		}

		public ZBool HasMatchingLapPermitRule => LapPermitRule != null;

		#region Properties

		[List(nameof(Lookups) + "." + nameof(NctsGuaranteeLookups.LiabilityApplicablePercentageCodeList))]
		[ResourceStringData("2E8C7A32-1978-4810-A344-FE12483FBE34", Caption = "Liability Fraction", ShortCaption = "Liability Fraction", FullDescription = "Fraction for the Liability calculation")]
		[ReadOnlyMember(nameof(PW_SuretyCodeReadOnly))]
		public override ZString PW_SuretyCode
		{
			get => base.PW_SuretyCode;
			set => base.PW_SuretyCode = value;
		}

		[ReadOnlyMember(nameof(PW_BondNumberReadOnly))]
		public override ZString PW_BondNumber
		{
			get => base.PW_BondNumber;
			set
			{
				var oldValue = PW_BondNumber;
				base.PW_BondNumber = value;
				if (!IsCopying && oldValue != value)
				{
					ResetGuaranteeCacheFields();
					UpdateLiabilityFractionFromGuarantee();
				}
			}
		}

		[ReadOnlyMember(nameof(IsGuaranteeReferenceFieldsReadOnlyDueToRuleC0085_2))]
		public override ZString PW_Password
		{
			get => base.PW_Password;
			set => base.PW_Password = value;
		}

		public override ZString PW_BondType
		{
			get => base.PW_BondType;
			set
			{
				var oldValue = base.PW_BondType;
				base.PW_BondType = value;
				if (!IsCopying && oldValue != value)
				{
					EmptyDisabledFieldsIfRuleC0085_2ReadOnly();
					if (PW_BondNumber2ReadOnly)
					{
						PW_BondNumber2 = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("d6aa406b-5d45-4ffc-8857-d32ad886b242", Caption = "Office", FullDescription = "Customs Office of Guarantee", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(IsGuaranteeReferenceFieldsReadOnlyDueToRuleC0085_2))]
		public override ZString PW_BondFiledPort
		{
			get => base.PW_BondFiledPort;
			set => base.PW_BondFiledPort = value;
		}

		[ResourceStringData("B5BEEB8B-4AC9-44BE-A16F-A91D6428C6A9", Caption = "Liability Amount", ShortCaption = "Liability Amt.")]
		[ReadOnlyMember(nameof(PW_BondAmountReadOnly))]
		public override ZDecimal PW_BondAmount
		{
			get => base.PW_BondAmount;
			set
			{
				var oldValue = PW_BondAmount;
				base.PW_BondAmount = value;
				if (oldValue != PW_BondAmount && !IsCopying)
				{
					ResetDirtyStatus();
				}
			}
		}

		public ZDecimal LiabilityAmount => Factory.GetValue(ref liabilityAmountCache, GetLiabilityAmount);
		CachedProperty<ZDecimal> liabilityAmountCache;

		ZDecimal GetLiabilityAmount() => NctsHeader.GetGoodsItems().Sum(x => x.LiabilityAmount);

		public ZDecimal GetTotalAmountWithRevertedFactor()
		{
			var suretyCode = PW_SuretyCode;
			if (suretyCode != LiabilityApplicablePercentageCodeList.Codes.ZER)
			{
				var amountFactor = liabilityAmountFactors.GetValueOrDefault(PW_SuretyCode, 1);

				return ((ZDecimal)(PW_BondAmount / amountFactor)).Round(2);
			}
			return 0;
		}

		[ReadOnlyMember(nameof(PW_BondNumber2ReadOnly))]
		public override ZString PW_BondNumber2 { get => base.PW_BondNumber2; set => base.PW_BondNumber2 = value; }

		[ReadOnlyMember(nameof(PW_OverrideReadOnly))]
		[ResourceStringData("7B21739F-7E17-45D3-A738-6D78961D77EA", Caption = "Override", ShortCaption = "Liability Amount Override", FullDescription = "Tick to override calculated Liability Amount and Fraction")]
		public override ZBool PW_Override
		{
			get => base.PW_Override;
			set
			{
				var oldValue = PW_Override;
				base.PW_Override = value;
				if (!IsCopying && oldValue != value)
				{
					UpdateGuaranteeFieldsWithOverrideChange(value);
				}
			}
		}

		[ResourceStringData("4012601C-ADAB-4523-866F-259E8BC9277D", Caption = "Currency", ShortCaption = "Curr.", FullDescription = "[99 03 012 000] Currency")]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(PW_RX_NKCurrencyReadOnly))]
		public override ZString PW_RX_NKCurrency
		{
			get => base.PW_RX_NKCurrency;
			set
			{
				var oldValue = PW_RX_NKCurrency;
				base.PW_RX_NKCurrency = value;
				if (!IsCopying && oldValue != value)
				{
					ClearCurrencyRelatedPropertiesIfOverridden();
				}
			}
		}

		protected virtual ZBool PW_SuretyCodeReadOnly => GuaranteeSupportsOverride && (PW_Override || HasMatchingLapPermitRule);

		protected virtual ZBool PW_BondAmountReadOnly => (GuaranteeSupportsOverride && !PW_Override && DoesGuaranteeExist) || IsGuaranteeReferenceFieldsReadOnlyDueToRuleC0085_2;

		protected virtual ZBool PW_BondNumberReadOnly => IsGuaranteeReferenceFieldsReadOnlyDueToRuleC0085_2;

		protected virtual ZBool PW_RX_NKCurrencyReadOnly => Lookups.ReferenceNumbers.Any(g => g.CPH_Number == PW_BondNumber && g.CPH_IsActive && (g.CPH_EndDate >= ZDateTime.Today || g.CPH_EndDate.IsEmpty));

		#endregion

		protected override void SyncroniseWithGuaranteeHeader(BaseCusGuaranteeHeader guaranteeHeader)
		{
			base.SyncroniseWithGuaranteeHeader(guaranteeHeader);
			PW_RX_NKCurrency = guaranteeHeader.CPH_UnitOfMeasure.Left(3);
			if (IsPhase5 && CopyCustomsOfficeFromGuaranteeHeader)
			{
				UpdateCustomsOfficeFromGuaranteeHeader(guaranteeHeader);
			}
		}
		protected virtual bool CopyCustomsOfficeFromGuaranteeHeader => true;

		void UpdateCustomsOfficeFromGuaranteeHeader(BaseCusGuaranteeHeader guaranteeHeader)
		{
			var customOfficeRules = guaranteeHeader.CusGuaranteeRules.Where(r => r.CPR_RuleCode.EqualsIgnoringCase(EU.Business.PermitRuleCodeList.Codes.CUS));

			if (customOfficeRules.Count() == 1)
			{
				PW_BondFiledPort = customOfficeRules.First().CPR_ValueFrom.Left(PW_BondFiledPortInfo.MaxLength);
			}
		}

		void UpdateGuaranteeFieldsWithOverrideChange(ZBool newOverride)
		{
			if (newOverride)
			{
				ClearSuretyCode();
				return;
			}

			ReloadSuretyCodeIfHasMatchLapRule();
			RecalculateBondAmount();
		}

		void ClearSuretyCode() => PW_SuretyCode = ZString.Empty;

		void ReloadSuretyCodeIfHasMatchLapRule() => DefaultSuretyCodeIfHasMatchLapPermitRule();

		void RecalculateBondAmount()
		{
			if (IsPhase5Arrival && !PW_Override)
			{
				SetLiabilityAmount(LiabilityAmount);
			}
			else
			{
				NctsHeader?.ApportionedAmountToGuaranteesLiabilityAmount();
			}
		}

		ZBool DoesGuaranteeExist => CusGuaranteeWithoutPermitHolder != null;

		protected bool IsPhase5 => NctsHeader?.IsPhase5 ?? false;

		protected bool IsPhase5Arrival => NctsHeader?.IsPhase5Arrival ?? false;

		protected bool IsPhase5Departure => NctsHeader?.IsPhase5Departure ?? false;

		public bool IsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor => PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee || PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor;

		public bool IsGuaranteeTypeWithAmountNR0067
		{
			get
			{
				switch (PW_BondType)
				{
					case EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver:
					case EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee:
					case EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor:
					case EUNctsGuaranteeTypeList.Codes.FlatRateVoucher:
					case EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage:
						return true;
					default:
						return false;
				}
			}
		}

		ZBool GuaranteeSupportsOverride => NctsHeader?.Configuration?.GuaranteeConfiguration?.OverrideSupport(NctsHeader) ?? ZBool.False;

		void ResetGuaranteeCacheFields()
		{
			lapPermitRule = null;
			cusGuaranteeWithoutPermitHolder = null;
		}

		void UpdateLiabilityFractionFromGuarantee()
		{
			if (IsPhase5)
			{
				var rule = CusGuarantee?.CusGuaranteeRules?.FirstOrDefault(r => r.CPR_RuleCode.EqualsIgnoringCase(EU.Business.PermitRuleCodeList.Codes.LAP));

				if (rule != null)
				{
					PW_SuretyCode = rule?.CPR_ValueFrom ?? ZString.Empty;
				}
			}
		}

		void ClearCurrencyRelatedPropertiesIfOverridden()
		{
			if (PW_Override)
			{
				PW_Override = false;
				PW_BondAmount = ZDecimal.Zero;
			}
		}

		#region Rule C0085_2 Implementation

		public ZBool IsGuaranteeReferenceFieldsReadOnlyDueToRuleC0085_2 => IsRuleC0085_2Applicable();

		bool IsRuleC0085_2Applicable()
		{
			return (ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider validationDecider)
				&& validationDecider.IsRuleC0085_2Active
				&& NctsHeader?.IsPhase5Departure == true
				&& !PW_BondType.IsEmpty
				&& !IsGuaranteeTypeWithReferenceC0085_2(PW_BondType);
		}

		void EmptyDisabledFieldsIfRuleC0085_2ReadOnly()
		{
			if (IsGuaranteeReferenceFieldsReadOnlyDueToRuleC0085_2)
			{
				PW_BondNumber = ZString.Empty;
				PW_Password = ZString.Empty;
				PW_BondAmount = ZDecimal.Zero;
				PW_BondFiledPort = ZString.Empty;
			}
		}

		bool IsGuaranteeTypeWithReferenceC0085_2(ZString guaranteeType)
		{
			switch (guaranteeType)
			{
				case NctsGuaranteeCodes.GuaranteeWaiver:
				case NctsGuaranteeCodes.ComprehensiveGuarantee:
				case NctsGuaranteeCodes.IndividualGuaranteeByGuarantor:
				case NctsGuaranteeCodes.CashDepositGuarantee:
				case NctsGuaranteeCodes.FlatRateVoucher:
				case NctsGuaranteeCodes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur:
				case NctsGuaranteeCodes.IndividualGuaranteeWithMultipleUsage:
					return true;
				default:
					return false;
			}
		}

		#endregion

		readonly ImmutableDictionary<string, ZDecimal> liabilityAmountFactors = new Dictionary<string, ZDecimal>()
		{
			{ LiabilityApplicablePercentageCodeList.Codes.FUL, 1.0m },
			{ LiabilityApplicablePercentageCodeList.Codes.HAL, 0.5m },
			{ LiabilityApplicablePercentageCodeList.Codes.THI, 30m / 100m },
			{ LiabilityApplicablePercentageCodeList.Codes.ZER, 0.0m }
		}.ToImmutableDictionary();

		void ResetDirtyStatus()
		{
			if (IsPhase5 && PW_Status == DirtyStatus)
			{
				PW_Status = string.Empty;
			}
		}

		public const string DirtyStatus = "CHG";

		internal bool IsInPhase5TransitionPeriod => NctsHeader?.IsInPhase5TransitionPeriod ?? false;
	}
}
