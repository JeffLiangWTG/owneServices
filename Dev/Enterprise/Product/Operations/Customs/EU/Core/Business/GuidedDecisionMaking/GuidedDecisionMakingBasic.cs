using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Meursing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingBasic
		: AutoGuidedDecisionMakingBasic
		, IMeursingTarget
		, ITariffFormatProvider
		, ITariffViewFilterDataProvider, IGuidedDecisionMakingBasic
	{
		public GuidedDecisionMakingBasic(IGuidedDecisionMakingSource source, BusinessObjectFactory factory) : base(factory)
		{
			ParentBusinessObject = source.ParentBusinessObject;
			EffectiveDate = source.EffectiveDate;
			DataGrouping = source.DataGrouping;
			ParentDataGrouping = source.ParentDataGrouping;
			UserLanguage = source.UserLanguage;
			DutyRateTypeCode = source.DutyRateTypeCode;
			CountryCode = source.CountryCode;
			TariffCode = source.TariffCode;
			CountryOfOrigin = source.CountryOfOrigin;
			CountryOfDestination = source.EffectiveCountryOfDestination;
			Preference = source.Preference;
			QuotaOrderNumber = source.QuotaOrderNumber;
			CustomsFirstQuantity = source.CustomsFirstQuantity;
			CustomsSecondQuantity = source.CustomsSecondQuantity;
			CustomsSecondUnitQty = source.CustomsSecondUnitQty;
			CustomsThirdQuantity = source.CustomsThirdQuantity;
			CustomsThirdUnitQty = source.CustomsThirdUnitQty;
			TariffType = source.TariffType;
			IsImport = source.IsImport;
			IsExport = source.IsExport;
			CapturedDocumentConditions = source.SupportingAndAdditionalDocuments.ToArray();
			CapturedSupplementaryCodes = source.SupplementaryCodes;
			CapturedVATCode = source.VATCode;
			IsCustomsFirstQuantityReadOnly = source.IsCustomsFirstQuantityReadOnly;
			IsCustomsSecondQuantityReadOnly = source.IsCustomsSecondQuantityReadOnly;
			IsCustomsThirdQuantityReadOnly = source.IsCustomsThirdQuantityReadOnly;
		}

		[ReadOnly(false)]
		[ResourceStringData("GuidedDecisionMakingBasic|EffectiveDate", Caption = "Effective Date")]
		public ZDate EffectiveDate
		{
			get => effectiveDate;
			set
			{
				SetNonPersistentPropertyValue(EffectiveDateInfo, ref effectiveDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEffectiveDate();
				}
			}
		}

		ZDate effectiveDate;

		public IBusiness ParentBusinessObject;

		public ZPropertyInfo EffectiveDateInfo => this.GetZPropertyInfo(nameof(EffectiveDate));

		[ReadOnly(false)]
		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingBasicLookups.CountryOfOriginList))]
		[ResourceStringData("GuidedDecisionMakingBasic|CountryOfOrigin", Caption = "Country Of Origin")]
		public override ZString CountryOfOrigin
		{
			get => base.CountryOfOrigin;
			set
			{
				base.CountryOfOrigin = value;
				CustomsSecondQuantityInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsSecondQuantity();
				}
			}
		}

		[ReadOnly(false)]
		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingBasicLookups.CountryOfOriginList))]
		[ResourceStringData("GuidedDecisionMakingBasic|CountryOfDestination", Caption = "Country Of Destination")]
		public override ZString CountryOfDestination
		{
			get => base.CountryOfDestination;
			set
			{
				base.CountryOfDestination = value;
			}
		}

		public virtual ZString EffectiveTradeGroupCountry => IsExport ? CountryOfDestination : CountryOfOrigin;

		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingBasicLookups.Tariffs))]
		public override ZString TariffCode
		{
			get => base.TariffCode;
			set
			{
				base.TariffCode = TariffFormatter.Format(value).Left(TariffCodeInfo.MaxLength);
				CustomsSecondQuantityInfo.RefreshBinding();
			}
		}

		[ReadOnly(false)]
		[ResourceStringData("GuidedDecisionMakingBasic|TariffCode", Caption = "Tariff")]
		public ZString FormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(TariffCode); }
			set { TariffCode = value; }
		}
		public virtual ZPropertyInfo FormattedTariffInfo => GetZPropertyInfo(Schema.TariffCode);

		TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = TariffFormatter.New(CountryCode)); }
		}
		TariffFormatter tariffFormatter;

		public override ZString TariffType
		{
			get => base.TariffType;
			set
			{
				base.TariffType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTariffCode();
				}
			}
		}

		[ReadOnlyMember(nameof(PreferenceReadOnly))]
		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingBasicLookups.PreferenceList))]
		[ResourceStringData("GuidedDecisionMakingBasic|Preference", Caption = "Preference")]
		public override ZString Preference
		{
			get => base.Preference;
			set { base.Preference = value; }
		}
		protected bool PreferenceReadOnly => Lookups.PreferenceList.Count == 0 || TariffCodeInfo.HasErrors();

		[ReadOnlyMember(nameof(QuotaOrderNumberReadOnly))]
		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingBasicLookups.QuotaOrderNumberList))]
		[ResourceStringData("GuidedDecisionMakingBasic|QuotaOrderNumberList", Caption = "Quota/Order Number")]
		public override ZString QuotaOrderNumber
		{
			get => base.QuotaOrderNumber;
			set { base.QuotaOrderNumber = value; }
		}
		protected bool QuotaOrderNumberReadOnly => Lookups.QuotaOrderNumberList.Count == 0 || TariffCodeInfo.HasErrors();

		[ReadOnlyMember(nameof(IsCustomsFirstQuantityReadOnly))]
		[ResourceStringData("GuidedDecisionMakingBasic|CustomsFirstQuantity", Caption = "Net Weight in KGM")]
		public override ZDecimal CustomsFirstQuantity
		{
			get => base.CustomsFirstQuantity;
			set { base.CustomsFirstQuantity = value; }
		}

		[ReadOnlyMember(nameof(CustomsSecondQuantityRequireNoCU2))]
		[ResourceStringData("GuidedDecisionMakingBasic|CustomsSecondQuantity", Caption = "Supplementary Qty.")]
		public override ZDecimal CustomsSecondQuantity
		{
			get => base.CustomsSecondQuantity;
			set { base.CustomsSecondQuantity = value; }
		}

		public bool CustomsSecondQuantityRequireNoCU2 =>
			IsCustomsSecondQuantityReadOnly ||
			(!Tariff?.UnitsOfMeasure?.Any(a => a.ZZ8_Type == UOMTypeList.Codes.CU2) ?? true);

		public bool CustomsThirdQuantityRequireNoCU3 =>
			!Tariff?.UnitsOfMeasure?.Any(a => a.ZZ8_Type == UOMTypeList.Codes.CU3) ?? true;

		[ReadOnlyMember(nameof(IsCustomsThirdQuantityReadOnly))]
		[ResourceStringData("GuidedDecisionMakingBasic|CustomsThirdQuantity", Caption = "Third Quantity")]
		public override ZDecimal CustomsThirdQuantity
		{
			get => base.CustomsThirdQuantity;
			set { base.CustomsThirdQuantity = value; }
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingBasicLookups.CustomsSecondUQList))]
		public override ZString CustomsSecondUnitQty
		{
			get => base.CustomsSecondUnitQty;
			set => base.CustomsSecondUnitQty = value;
		}

		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingBasicLookups.CustomsUQList))]
		public override ZString CustomsThirdUnitQty
		{
			get => base.CustomsThirdUnitQty;
			set
			{
				base.CustomsThirdUnitQty = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsThirdQuantity();
				}
			}
		}

		[ResourceStringData("06586861-d681-45c4-ab79-de487be78d85", Caption = "Meursing Result")]
		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingBasicLookups.MeursingResultList))]
		public override ZString MeursingResult
		{
			get => base.MeursingResult;
			set => base.MeursingResult = value;
		}

		public virtual bool IsMeursingApplicable => Factory.GetValue(ref isMeursingApplicable, () =>
		{
			if (!DataGrouping.IsEmpty && !EffectiveTradeGroupCountry.IsEmpty && Tariff is TariffView tariff)
			{
				if (tariff.GetApplicableRates(RateSelectionCriteria).Any(x => EURateFormulaHelper.ContainsMursingPattern(x.ZZ2_RateFormula)))
				{
					return true;
				}
			}
			return false;
		});
		CachedProperty<bool> isMeursingApplicable;

		public virtual bool IsVATApplicable => Factory.GetValue(ref isVATApplicable, () =>
		{
			return IsImport && VATApplicabilities.Any();
		});
		CachedProperty<bool> isVATApplicable;

		public IZZRateSelectionCriteria RateSelectionCriteriaWithoutAdditionalCodes => Factory.GetValue(ref rateSelectionCriteriaWithoutAdditionalCodes, () => GetNewRateSelectionCriteriaWithoutAdditionalCodes());
		CachedProperty<IZZRateSelectionCriteria> rateSelectionCriteriaWithoutAdditionalCodes;

		protected virtual IZZRateSelectionCriteria GetNewRateSelectionCriteriaWithoutAdditionalCodes() => new SpecificRateSelectionCriteria(EffectiveTradeGroupCountry, DataGrouping, Preference, QuotaOrderNumber, new HashSet<ZString>(), EffectiveDate, ZString.Empty, ZString.Empty);

		public IZZRateSelectionCriteria RateSelectionCriteria => Factory.GetValue(ref rateSelectionCriteria, () => GetNewRateSelectionCriteria());
		CachedProperty<IZZRateSelectionCriteria> rateSelectionCriteria;

		protected virtual IZZRateSelectionCriteria GetNewRateSelectionCriteria() => new SpecificRateSelectionCriteria(EffectiveTradeGroupCountry, DataGrouping, Preference, QuotaOrderNumber, SelectedSupplementaryCodes.ToHashSet(), EffectiveDate, ZString.Empty, ZString.Empty);

		public List<IGrouping<ZString, GuidedDecisionMakingAdditionalCode>> GroupedAdditionalCodes
		{
			get
			{
				return AdditionalCodes.Cast<GuidedDecisionMakingAdditionalCode>()
					.OrderBy(x => x is GuidedDecisionMakingAdditionalCodeFromCondition)
					.ThenBy(x => x.ApplicableToType)
					.ThenBy(x => x.AdditionalCode)
					.GroupBy(x => x.ApplicableToDescription).ToList();
			}
		}

		public GuidedDecisionMakingAdditionalCodeCollection AdditionalCodes
		{
			get
			{
				if (additionalCodes == null)
				{
					additionalCodes = new GuidedDecisionMakingAdditionalCodeCollection(this);
					RegisterEditableChildObject(additionalCodes);
				}
				return additionalCodes;
			}
		}
		GuidedDecisionMakingAdditionalCodeCollection additionalCodes;

		public void ClearAdditionalCodesCache()
		{
			additionalCodes = null;
			documentConditions = null;
		}

		public ITariffAdditionalCodeSelectionCriteria TariffAdditionalCodeSelectionCriteria => Factory.GetValue(ref tariffAdditionalCodeSelectionCriteria, () => GetNewTariffAdditionalCodeSelectionCriteria());
		CachedProperty<ITariffAdditionalCodeSelectionCriteria> tariffAdditionalCodeSelectionCriteria;

		protected virtual ITariffAdditionalCodeSelectionCriteria GetNewTariffAdditionalCodeSelectionCriteria() => null;

		public IZZConditionSelectionCriteria ConditionSelectionCriteriaWithoutAdditionalCodes => Factory.GetValue(ref conditionSelectionCriteriaWithoutAdditionalCodes, () => GetNewConditionSelectionCriteriaWithoutAdditionalCodes());
		CachedProperty<IZZConditionSelectionCriteria> conditionSelectionCriteriaWithoutAdditionalCodes;

		protected virtual IZZConditionSelectionCriteria GetNewConditionSelectionCriteriaWithoutAdditionalCodes()
		{
			var direction = IsImport ? ConditionChecker.ConditionDirection.Import : IsExport ? ConditionChecker.ConditionDirection.Export : ConditionChecker.ConditionDirection.Either;
			return new ZZConditionSelectionCriteria(EffectiveDate, EffectiveTradeGroupCountry, Preference, new HashSet<ZString>(), QuotaOrderNumber, DataGrouping, direction, ZString.Empty, ZString.Empty);
		}

		public IZZConditionSelectionCriteria ConditionSelectionCriteria => Factory.GetValue(ref conditionSelectionCriteria, () => GetNewConditionSelectionCriteria());
		CachedProperty<IZZConditionSelectionCriteria> conditionSelectionCriteria;

		protected virtual IZZConditionSelectionCriteria GetNewConditionSelectionCriteria()
		{
			var direction = IsImport ? ConditionChecker.ConditionDirection.Import : IsExport ? ConditionChecker.ConditionDirection.Export : ConditionChecker.ConditionDirection.Either;
			return new ZZConditionSelectionCriteria(EffectiveDate, EffectiveTradeGroupCountry, Preference, SelectedSupplementaryCodes.ToHashSet(), QuotaOrderNumber, DataGrouping, direction, ZString.Empty, ZString.Empty);
		}

		public GuidedDecisionMakingConditionCollection DocumentConditions => documentConditions ?? (documentConditions = new GuidedDecisionMakingConditionCollection(this));
		GuidedDecisionMakingConditionCollection documentConditions;

		public void ClearDocumentConditionsCache()
		{
			documentConditions = null;
		}

		public GuidedDecisionMakingVATCollection VATApplicabilities
		{
			get
			{
				if (vATApplicabilities is null)
				{
					vATApplicabilities = GetNewGuidedDecisionMakingVATCollection();
					RegisterEditableChildObject(vATApplicabilities);
				}
				return vATApplicabilities;
			}
		}
		GuidedDecisionMakingVATCollection vATApplicabilities;

		GuidedDecisionMakingVATCollection GetNewGuidedDecisionMakingVATCollection() => GetNewGuidedDecisionMakingVATCollectionCore();

		protected virtual GuidedDecisionMakingVATCollection GetNewGuidedDecisionMakingVATCollectionCore() => new GuidedDecisionMakingVATCollection(this);

		ZString SelectedVATAdditionalCode => SelectedVAT?.AdditionalCode ?? ZString.Empty;

		public void ClearVATApplicabilitiesCache()
		{
			vATApplicabilities = null;
		}

		public GuidedDecisionMakingBasicLookups Lookups => lookups ?? (lookups = GetNewLookups());
		GuidedDecisionMakingBasicLookups lookups;

		public virtual GuidedDecisionMakingBasicLookups GetNewLookups() => new GuidedDecisionMakingBasicLookups(this);

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = GetNewValueSetStrategy());

		protected virtual IValueSetStrategy GetNewValueSetStrategy() => new GuidedDecisionMakingBasicValueSetStrategy(this);

		IValueSetStrategy valueSetStrategy;

		public IReadOnlyList<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> CapturedDocumentConditions { get; private set; }

		public IEnumerable<ZString> CapturedSupplementaryCodes { get; private set; }

		public ZString CapturedVATCode { get; private set; }

		public TariffView Tariff
		{
			get
			{
				var tariffLoader = new TariffView.Loader(Factory);
				return tariffLoader.LoadMostRecentCachedTariff(DataGrouping, TariffCode, EffectiveDate);
			}
		}

		public void SetMeursingResult(ZString meursingResult)
		{
			MeursingResult = meursingResult;
		}

		public IMeursingTableManager MeursingManager => meursingManager ?? (meursingManager = new MeursingTableManager(this));

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		IMeursingTableManager meursingManager;

		public List<ZString> SelectedSupplementaryCodes
		{
			get
			{
				return Factory.GetCached(ref selectedSupplementaryCodesCache, () =>
				{
					var codes = AdditionalCodes.Cast<GuidedDecisionMakingAdditionalCode>().Where(x => x.IsTicked).Select(x => x.AdditionalCode).ToList();
					if (!MeursingResult.IsEmpty)
					{
						codes.Add(MeursingResult);
					}
					if (SelectedVATAdditionalCode is ZString selectedVATAdditionalCode)
					{
						if (!selectedVATAdditionalCode.IsEmpty)
						{
							codes.Add(selectedVATAdditionalCode);
						}
					}
					return codes.Distinct().ToList();
				});
			}
		}
		CachedProperty<List<ZString>> selectedSupplementaryCodesCache;

		public List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> SelectedDocuments
		{
			get
			{
				return Factory.GetCached(ref selectedDocumentsCache, () =>
				{
					return DocumentConditions.Cast<GuidedDecisionMakingCondition>()
						.SelectMany(x => x.ConditionDetails).Cast<GuidedDecisionMakingConditionDetail>()
						.Where(x => x.IsTicked)
						.Select(x => (x.Code, x.Reference, x.DateOfIssue)).ToList();
				});
			}
		}

		public ITariffViewFilterData TariffViewFilterData => new TariffViewFilterData(EffectiveTradeGroupCountry, EffectiveDate);

		CachedProperty<List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>> selectedDocumentsCache;

		GuidedDecisionMakingVAT SelectedVAT => Factory.GetCached(ref selectedVATCache, () =>
		{
			return VATApplicabilities.Cast<GuidedDecisionMakingVAT>().FirstOrDefault(v => v.IsTicked);
		});

		CachedProperty<GuidedDecisionMakingVAT> selectedVATCache;

		public void TransferToTarget(IGuidedDecisionMakingTarget target) => TransferToTargetCore(target);

		protected virtual void TransferToTargetCore(IGuidedDecisionMakingTarget target)
		{
			if (IsVATApplicable && SelectedVAT is GuidedDecisionMakingVAT selectedVAT)
			{
				target.SetVATCode(selectedVAT.VATCode, selectedVAT.AdditionalCode);
			}
			target.SetAdditionalCodes(SelectedSupplementaryCodes);
			target.SetSupportingAndAdditionalDocuments(SelectedDocuments);
			target.CountryOfOrigin = CountryOfOrigin;
			target.Preference = Preference;
			target.QuotaOrderNumber = QuotaOrderNumber;
			target.TariffCode = TariffCode;
			target.CustomsSecondQuantity = CustomsSecondQuantity;
			target.CustomsSecondUnitQty = CustomsSecondUnitQty;
			target.CustomsThirdQuantity = CustomsThirdQuantity;
			target.CustomsThirdUnitQty = CustomsThirdUnitQty;
			target.CustomsFirstUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			target.CustomsFirstQuantity = CustomsFirstQuantity;
			target.CountryOfDestination = CountryOfDestination;
		}

		internal protected virtual IGuidedDecisionMakingBasicValidationConfiguration GetNewExportValidationConfiguration() => IsExport ? new ExportGuidedDecisionMakingBasicValidationConfiguration() : null;

		internal protected virtual IGuidedDecisionMakingBasicValidationConfiguration GetNewImportValidationConfiguration() => IsImport ? new ImportGuidedDecisionMakingBasicValidationConfiguration() : null;

		public virtual ZString PreferredLanguage
		{
			get
			{
				return GlbStaff.CurrentUser?.Language ?? Core.SharedConstants.Languages.English;
			}
		}

		public bool IsUpdatingSiblingsAdditionalCodes;

		public void UpdateSiblingsAdditionalCodes(string additionalCode, string applicableToType, bool value)
		{
			AdditionalCodes.Cast<GuidedDecisionMakingAdditionalCode>().Where(x => x.AdditionalCode == additionalCode && x.ApplicableToType == applicableToType && x.IsTicked != value).ForEach(ac => ac.IsTicked = value);
		}

		public GdmConfiguration Configuration => GdmConfiguration.GetConfiguration(Factory, CountryCode);

		public ZString SelectAllWaiversCaption => SelectAllWaiversCaptionCore;

		protected virtual ZString SelectAllWaiversCaptionCore => Res.GetString("AB82D13E-BD5C-40B9-BABB-316F46A4FD1F", "Select all Y - series waivers");

		public bool IsCustomsFirstQuantityReadOnly { get; set; }

		public bool IsCustomsSecondQuantityReadOnly { get; set; }

		public bool IsCustomsThirdQuantityReadOnly { get; set; }
	}
}
