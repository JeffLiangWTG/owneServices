using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.CH.Business.SwissCustomsConstants;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.CH.Business;

public class JobComInvoiceLine
	: AutoCHJobComInvoiceLine
	, Integration.Customs.CH.IJobComInvoiceLine
	, ICusSupportingInfoTypeSupporter
	, ISpecialMentions
	, ICusSupportingInfoParent
	, ISupportingDocumentParent
	, ICusCodeDataTypeSupporter
	, IRestrictionParent
{
	public new class Schema : AutoJobComInvoiceLine.Schema
	{
		public const string SpecialMentions = nameof(JobComInvoiceLine.SpecialMentions);
		public const string NetDuty = nameof(JobComInvoiceLine.NetDuty);
		public const string CalculatedGrossMass = nameof(JobComInvoiceLine.CalculatedGrossMass);
		public const string CalculatedGrossMassUQ = nameof(JobComInvoiceLine.CalculatedGrossMassUQ);
		public const string DutyRateFormulaNumber = nameof(JobComInvoiceLine.DutyRateFormulaNumber);
		public const string DutyRateConfirmation = nameof(JobComInvoiceLine.DutyRateConfirmation);
		public const string DutyRateAdditionalCode = nameof(JobComInvoiceLine.DutyRateAdditionalCode);
		public const string DutyRateDescription = nameof(JobComInvoiceLine.DutyRateDescription);
		public const int JI_FormattedTariffMaxLength = 17;
		public const string EntryLineNumber = nameof(JobComInvoiceLine.EntryLineNumber);
		public const int DutyRateAdditionalCodeMaxLength = 15;
		public const int JI_WeightIncludingInnerPackageUQMaxLength = 2;
		public const string UNDGCodes = nameof(JobComInvoiceLine.UNDGCodes);
	}

	public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobComInvoiceLine Clone() => (JobComInvoiceLine)base.Clone();

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

	public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	[ChildEditable(true)]
	public new JobComInvChargeCollection<InvoiceLineCharge> Charges => (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges;

	[ChildEditable(true)]
	public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

	#region Properties

	public override ZGuid JI_JZ
	{
		get => base.JI_JZ;
		set
		{
			var oldValue = JI_JZ;
			base.JI_JZ = value;
			if (!IsCopying && oldValue != JI_JZ)
			{
				Permits.MarkAsNeedingValidation();
				Vehicles.MarkAsNeedingValidation();
				NotifyCustomsOffices.MarkAsNeedingValidation();
				AdditionalFees.MarkAsNeedingValidation();
				AdditionalTaxes.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_TareSupplementPercentage", Caption = "Tare supplement")]
	[ReadOnlyMember(nameof(JI_TareSupplementPercentage_ReadOnly))]
	public override ZDecimal JI_TareSupplementPercentage
	{
		get => base.JI_TareSupplementPercentage;
		set
		{
			base.JI_TareSupplementPercentage = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Procedure();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_TareSupplementConfirmation", Caption = "Override")]
	[ReadOnlyMember(nameof(NetDutyControls_ReadOnly))]
	public override ZBool JI_TareSupplementConfirmation { get => base.JI_TareSupplementConfirmation; set => base.JI_TareSupplementConfirmation = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_GoodsReturned", Caption = "Returned Goods")]
	public override ZBool JI_GoodsReturned { get => base.JI_GoodsReturned; set => base.JI_GoodsReturned = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_PrimaryPreference", Caption = "Preference Code", MediumCaption = "Pref. Code")]
	public override ZString JI_PrimaryPreference
	{
		get => base.JI_PrimaryPreference;
		set
		{
			var oldValue = JI_PrimaryPreference;
			base.JI_PrimaryPreference = value;
			if (!IsCopying && oldValue != JI_PrimaryPreference)
			{
				DefaultDutyRateAdditionalCodeIfApplicable();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateNetDuty();
				Validation.ValidateDutyRateAdditionalCode();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_Procedure", Caption = "Procedure", ShortCaption = "CPC")]
	[ReadOnlyMember(nameof(IsExportOrExportDeclarationActivation))]
	public override ZString JI_Procedure
	{
		get => base.JI_Procedure;
		set
		{
			base.JI_Procedure = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_CustomsValue();
				Validation.ValidateJI_CountryOfOrigin();
				Validation.ValidateDutyRateAdditionalCode();
				Validation.ValidateJI_Tariff();
				Validation.ValidateJI_NonTradingGoods();
				Validation.ValidateJI_RefundType();
				InAndOutwardProcessing.Validation.ValidateCSI_SubType();
				InAndOutwardProcessing.Validation.ValidateCSI_Code();
				InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
				InAndOutwardProcessing.Validation.ValidateCSI_IssuerType();
				InAndOutwardProcessingDirectionInfo.RefreshBinding();
				InAndOutwardProcessingRefinementTypeInfo.RefreshBinding();
				InAndOutwardProcessingProcessTypeInfo.RefreshBinding();
				InAndOutwardProcessingBillingTypeInfo.RefreshBinding();
			}
		}
	}

	[MaxLength(2)]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_ZZF_NKTaxType", Caption = "VAT Code")]
	public override ZString JI_ZZF_NKTaxType
	{
		get => base.JI_ZZF_NKTaxType;
		set
		{
			var oldValue = JI_ZZF_NKTaxType;
			base.JI_ZZF_NKTaxType = value;
			if (!IsCopying && oldValue != JI_ZZF_NKTaxType)
			{
				JobDeclaration?.MarkAsNeedingValidation();
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Procedure();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|NetDuty", Caption = "Net Duty")]
	public ZBool NetDuty
	{
		get => netDuty;
		set
		{
			var oldValue = netDuty;
			netDuty = value;
			if (IsImport && oldValue != value)
			{
				UpdateNetDutyDependencies();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateNetDuty();
			}
			NetDutyInfo.RefreshBinding();
		}
	}
	ZBool netDuty;

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_RefundReferenceNumber", Caption = "GDRN")]
	public override ZString JI_RefundReferenceNumber { get => base.JI_RefundReferenceNumber; set => base.JI_RefundReferenceNumber = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_RefundReason", Caption = "Refund Reason")]
	public override ZString JI_RefundReason { get => base.JI_RefundReason; set => base.JI_RefundReason = value; }

	public ZPropertyInfo NetDutyInfo => GetZPropertyInfo(Schema.NetDuty);

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|CalculatedGrossMass", Caption = "Calculated Gross mass", ShortCaption = "Calc. Gross mass")]
	public ZDecimal CalculatedGrossMass
	{
		get
		{
			var weight = JI_WeightIncludingInnerPackage + (JI_WeightIncludingInnerPackage * JI_TareSupplementPercentage / 100);
			return Math.Ceiling(new ZWeight(weight, JI_WeightIncludingInnerPackageUQ).InKilogramsSafe * 10) / 10;
		}
	}

	public ZPropertyInfo CalculatedGrossMassInfo => GetZPropertyInfo(Schema.CalculatedGrossMass);

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.WeightUQList))]
	[MaxLength(JobComInvoiceLine.Schema.JI_WeightIncludingInnerPackageUQMaxLength)]
	public ZString CalculatedGrossMassUQ => Core.Constants.Weight.Kilograms;

	public ZPropertyInfo CalculatedGrossMassUQInfo => GetZPropertyInfo(Schema.CalculatedGrossMassUQ);

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_Calc_StatisticalValue", Caption = "Statistical Value")]
	public ZDecimal JI_Calc_StatisticalValue
	{
		get
		{
			if (jI_Calc_StatisticalValueCached == null)
			{
				jI_Calc_StatisticalValueCached = new CachedProperty<ZDecimal>(Factory, () =>
				{
					decimal result = 0m;
					if (InvoiceHeader != null)
					{
						result = ItemPriceInLocalCurrency + ValuationCalculator.GetAmountToAddToITOTForStatistical(LocalCurrency);
					}
					return result;
				});
			}
			return jI_Calc_StatisticalValueCached.Value;
		}
	}
	CachedProperty<ZDecimal> jI_Calc_StatisticalValueCached;

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_CustomsValue", Caption = "Customs Value")]
	public override ZDecimal JI_CustomsValue => base.JI_CustomsValue;

	public ZBool NetDutyControls_ReadOnly => !NetDuty;

	public ZBool JI_TareSupplementPercentage_ReadOnly => NetDutyControls_ReadOnly || !JI_TareSupplementConfirmation;

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|EntryLineNumber", Caption = "Entry Line #")]
	public ZShort EntryLineNumber => CusEntryLine?.CL_LineNumber ?? ZShort.Zero;

	[MaxLength(5)]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_RefundGoodsItemNumber", Caption = "Goods Item Number", ShortCaption = "Goods Item No.")]
	public override ZInt JI_RefundGoodsItemNumber { get => base.JI_RefundGoodsItemNumber; set => base.JI_RefundGoodsItemNumber = value; }

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RefundTypeList))]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_RefundType", Caption = "Refund Type")]
	public override ZString JI_RefundType
	{
		get => base.JI_RefundType;
		set
		{
			var oldValue = JI_RefundType;
			base.JI_RefundType = value;
			if (!IsCopying && oldValue != JI_RefundType)
			{
				if (!IsReturnedGoodsWithRefundRequest)
				{
					SetRefundInfoPropertiesToEmpty();
				}

				MarkAsNeedingValidation();
			}
		}
	}

	void SetRefundInfoPropertiesToEmpty()
	{
		JI_RefundReferenceNumber = ZString.Empty;
		JI_RefundGoodsItemNumber = 0;
		JI_RefundReason = ZString.Empty;
	}

	[DecimalPrecision(9)]
	[DecimalPlaces(3)]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_WeightIncludingInnerPackage", Caption = "Customs Net Weight")]
	[ReadOnlyMember(nameof(NetDutyControls_ReadOnly))]
	public override ZDecimal JI_WeightIncludingInnerPackage
	{
		get => base.JI_WeightIncludingInnerPackage;
		set
		{
			base.JI_WeightIncludingInnerPackage = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Procedure();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.WeightUQList))]
	[ReadOnlyMember(nameof(NetDutyControls_ReadOnly))]
	public override ZString JI_WeightIncludingInnerPackageUQ { get => base.JI_WeightIncludingInnerPackageUQ; set => base.JI_WeightIncludingInnerPackageUQ = value; }

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PermitObligationCodeList))]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_PermitObligation", Caption = "Permit Obligation Code", MediumCaption = "Permit Obligation")]
	public override ZString JI_PermitObligation { get => base.JI_PermitObligation; set => base.JI_PermitObligation = value; }

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NonCustomsLawObligationCodeList))]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_NonCustomsLawObligation", Caption = "NCL Obligation Code", MediumCaption = "NCL Obligation")]
	public override ZString JI_NonCustomsLawObligation { get => base.JI_NonCustomsLawObligation; set => base.JI_NonCustomsLawObligation = value; }

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.StorageTypeCodeList))]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_StorageType", Caption = "Storage Code")]
	public override ZString JI_StorageType
	{
		get => base.JI_StorageType;
		set
		{
			base.JI_StorageType = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Procedure();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_VATCodeConfirmation", Caption = "Confirmation")]
	public override ZBool JI_VATCodeConfirmation
	{
		get => base.JI_VATCodeConfirmation;
		set
		{
			var oldValue = JI_VATCodeConfirmation;
			base.JI_VATCodeConfirmation = value;
			if (!IsCopying && oldValue != JI_VATCodeConfirmation)
			{
				MarkAsNeedingValidation();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_ZZF_NKTaxType();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_VATValueConfirmation", Caption = "VAT Value")]
	public override ZBool JI_VATValueConfirmation
	{
		get => base.JI_VATValueConfirmation;
		set
		{
			var oldValue = JI_VATValueConfirmation;
			base.JI_VATValueConfirmation = value;
			if (!IsCopying && oldValue != JI_VATValueConfirmation)
			{
				MarkAsNeedingValidation();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_CustomsValue();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_NetMassConfirmation", Caption = "Net Mass")]
	public override ZBool JI_NetMassConfirmation
	{
		get => base.JI_NetMassConfirmation;
		set
		{
			var oldValue = JI_NetMassConfirmation;
			base.JI_NetMassConfirmation = value;
			if (!IsCopying && oldValue != JI_NetMassConfirmation)
			{
				MarkAsNeedingValidation();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Tariff();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_GrossMassConfirmation", Caption = "Gross Mass")]

	public override ZBool JI_GrossMassConfirmation
	{
		get => base.JI_GrossMassConfirmation;
		set
		{
			var oldValue = JI_GrossMassConfirmation;
			base.JI_GrossMassConfirmation = value;
			if (!IsCopying && oldValue != JI_VATValueConfirmation)
			{
				MarkAsNeedingValidation();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Weight();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_AdditionalUnitConfirmation", Caption = "Additional Quantity")]
	public override ZBool JI_AdditionalUnitConfirmation
	{
		get => base.JI_AdditionalUnitConfirmation;
		set
		{
			var oldValue = JI_AdditionalUnitConfirmation;
			base.JI_AdditionalUnitConfirmation = value;
			if (!IsCopying && oldValue != JI_AdditionalUnitConfirmation)
			{
				MarkAsNeedingValidation();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Tariff();
				Validation.ValidateJI_CustomsThirdQuantity();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_StatisticalValueConfirmation", Caption = "Statistical Value")]
	public override ZBool JI_StatisticalValueConfirmation
	{
		get => base.JI_StatisticalValueConfirmation;
		set
		{
			var oldValue = JI_StatisticalValueConfirmation;
			base.JI_StatisticalValueConfirmation = value;
			if (!IsCopying && oldValue != JI_StatisticalValueConfirmation)
			{
				MarkAsNeedingValidation();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CusCodeList))]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_CusNumber", Caption = "CUS-Code")]
	public override ZString JI_CusNumber { get => base.JI_CusNumber; set => base.JI_CusNumber = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_NonTradingGoods", Caption = "Non Commercial Goods", ShortCaption = "Non Comm. Goods", FullDescription = "Indicate if a certain goods is to be considered commercial or not when imported.")]
	public override ZBool JI_NonTradingGoods
	{
		get => base.JI_NonTradingGoods;
		set
		{
			base.JI_NonTradingGoods = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_GoodsReturned();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_RateOverride", Caption = "Rate Override", ShortCaption = "Rate Over.")]
	public override ZBool JI_RateOverride
	{
		get => base.JI_RateOverride;

		set
		{
			var oldValue = JI_RateOverride;
			base.JI_RateOverride = value;

			if (!IsCopying && oldValue != value && !value)
			{
				JI_OverriddenRate = ZDecimal.Zero;
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_RateOverride();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_OverriddenRate", Caption = "Overridden Rate", ShortCaption = "Over. Rate")]
	public override ZDecimal JI_OverriddenRate
	{
		get => base.JI_OverriddenRate;

		set
		{
			base.JI_OverriddenRate = value;

			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_OverriddenRate();
				Validation.ValidateJI_RateOverride();
			}
		}
	}

	public ZDecimal OverriddenRateXML => Factory.GetCached(ref overriddenRateXML, () =>
	{
		if (JI_RateOverride)
		{
			const string kgm = $"[{MeasurementUnits.NetWeightUOM}]";
			const string kgmg = $"[{MeasurementUnits.GrossWeightUOM}]";

			return (UniversalDutyRate != null && (UniversalDutyRate.ZZ2_RateFormula.Contains(kgm) || UniversalDutyRate.ZZ2_RateFormula.Contains(kgmg))) ? JI_OverriddenRate * 100m : JI_OverriddenRate;
		}
		else
		{
			return ZDecimal.Zero;
		}
	});
	CachedProperty<ZDecimal> overriddenRateXML;

	#endregion

	public override ZDateTime EffectiveAssessmentDate => (IsImport && (!EntryInstruction?.CEI_DateForDuty.IsEmpty ?? false)) ? EntryInstruction.CEI_DateForDuty : base.EffectiveAssessmentDate;

	protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Switzerland;

	protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

	protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

	protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation() => new JobComInvoiceLineValidation(this);

	protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

	protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new JobComInvChargeCollection<InvoiceLineCharge>(this);

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new Strategy(this);

	protected override Customs.Business.InvoiceLinePackagePivotCollection GetNewPackagesPivotCore() => new InvoiceLinePackagePivotCollection(this);

	protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage) => new InvoiceLinePackageValidation(linkPackage, this);

	public new RefCurrencyCurrencyConverter CurrencyConverter => (RefCurrencyCurrencyConverter)base.CurrencyConverter;

	protected override CurrencyConverter GetCurrencyConverter()
	{
		return new RefCurrencyCurrencyConverter(Factory)
		{
			DateForRate = EffectiveAssessmentDate,
			RateType = ExchangeRateType.Customs
		};
	}

	[ChildEditable(true)]
	public PermitCollection Permits
	{
		get
		{
			if (permits == null)
			{
				permits = new PermitCollection(this);
				permits.Load();
				RegisterEditableChildObject(permits);
			}
			return permits;
		}
	}
	PermitCollection permits;

	[ChildEditable(true)]
	public AdditionalInformationCollection AdditionalInformations
	{
		get
		{
			if (additionalInformations == null)
			{
				additionalInformations = new AdditionalInformationCollection(this);
				additionalInformations.Load();
				RegisterEditableChildObject(additionalInformations);
			}
			return additionalInformations;
		}
	}
	AdditionalInformationCollection additionalInformations;

	public override ZGuid JI_CEI
	{
		get => base.JI_CEI;
		set
		{
			var oldValue = JI_CEI;
			var oldInvoiceHeader = InvoiceHeader;
			var oldInstruction = EntryInstruction;
			base.JI_CEI = value;
			if (!IsCopying && oldValue != JI_CEI)
			{
				oldInvoiceHeader?.MarkAsNeedingValidation();
				InvoiceHeader?.MarkAsNeedingValidation();
				NotifyCustomsOffices?.MarkAsNeedingValidation();
				oldInstruction?.Validation.ValidateCEI_DeclarationReason();
				EntryInstruction?.Validation.ValidateCEI_DeclarationReason();
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_PrimaryPreference();
			}
		}
	}

	#region ICusCodeDataTypeSupporter Members
	IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
	{
		return new Dictionary<ZString, Type>()
		{
			{ Common.CH.CusSupportingInfoTypeList.Codes.Permit, typeof(Permit) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.NonCustomsLaw, typeof(NonCustomsLaw) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.Tobacco, typeof(Tobacco) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.AdditionalInformation, typeof(AdditionalInformation) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.InAndOutwardProcessing, typeof(InAndOutwardProcessing) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.Restriction, typeof(Restriction) },
		};
	}

	IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
	{
		return new Dictionary<ZString, Type>()
		{
			{ CusCodeDataTypeList.Codes.AdditionalCode, typeof(AdditionalCodeData) },
			{ CusCodeDataTypeList.Codes.NotifyCustomsOffice, typeof(NotifyCustomsOffice) }
		};
	}

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
	}
	#endregion

	#region ICusSupportingInfoParent Members
	public JobDeclaration JobDeclaration => (JobDeclaration)base.Declaration;

	public ZDateTime DateOfValuation => JobDeclaration?.DateOfValuation ?? ZDate.Today;

	public HugeSequenceNumberGenerator AdditionalInformationLineNumberGenerator => additionalInformationLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => new TypedEnumerable<IHugeSequenceNumberLine>(AdditionalInformations));
	HugeSequenceNumberGenerator additionalInformationLineNumberGenerator;

	public void ValidateNonTradingGoods() => Validation.ValidateJI_NonTradingGoods();
	#endregion

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|SpecialMentions", Caption = "Special Mentions", ShortCaption = "Goods Item No.")]
	[BusinessObjectMaxLengthTestExclude]
	public ZString SpecialMentions
	{
		get => SpecialMentionsNote.Text;
		set
		{
			SpecialMentionsNote.SetNoteText(this, SpecialMentionsInfo, SpecialMentionsHelper.FormatText(value));
			if (!IsValidationSuspended)
			{
				Validation.ValidateSpecialMentions();
			}
		}
	}

	public ZPropertyInfo SpecialMentionsInfo => GetZPropertyInfo(Schema.SpecialMentions);

	HiddenTextNote SpecialMentionsNote => specialMentionsNote ?? (specialMentionsNote = new HiddenTextNote(this, SpecialMentionsHelper.NoteType.Description));
	HiddenTextNote specialMentionsNote;

	public int CountOfSpecialMentionsLines => Factory.GetCached(ref fCountOfSpecialMentionsLines, () => SpecialMentionsHelper.CountLines(SpecialMentions));
	CachedProperty<int> fCountOfSpecialMentionsLines;

	[MaxLength(14)]
	public override ZString JI_Tariff
	{
		get => base.JI_Tariff;
		set
		{
			var oldValue = base.JI_Tariff;
			base.JI_Tariff = value;
			if (!IsCopying && oldValue != JI_Tariff)
			{
				if (IsImport)
				{
					UpdateNetDutyDependencies();
					UpdateVatCode();
					AdditionalTaxes.RebuildAdditionalTaxes();
				}
				DefaultDutyRateAdditionalCodeIfApplicable();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Weight();
				Validation.ValidateNetDuty();
				Validation.ValidateDutyRateAdditionalCode();
				Validation.ValidateJI_Procedure();
				Validation.ValidateJI_StorageType();
				Validation.ValidateJI_PermitObligation();
				Validation.ValidateJI_NonCustomsLawObligation();
				Validation.ValidateJI_NonTradingGoods();
			}
		}
	}

	public ZString CustomsFavourCode => IsImport && !JI_Tariff.IsEmpty ? JI_Tariff.SubstringSafe(8, 3).TrimStart('0') : ZString.Empty;

	public ZString StatisticalCode => IsImport && !JI_Tariff.IsEmpty ? JI_Tariff.SubstringSafe(11, 3).TrimStart('0') : ZString.Empty;

	public ZBool IsStatisticalCodeOther => StatisticalCode == TariffStatisticalCodes.StatisticalCodeOther;

	public ZString TariffWithoutCustomsFavourCode => IsImport && !JI_Tariff.IsEmpty ? JI_Tariff.SubstringSafe(0, 8) + JI_Tariff.SubstringSafe(11, 3) : string.Empty;

	public ZString TariffCodeGroup => IsImport && !JI_Tariff.IsEmpty ? JI_Tariff.SubstringSafe(0, 4) : ZString.Empty;

	public ZString TariffNumber => !JI_Tariff.IsEmpty ? JI_Tariff.SubstringSafe(0, 8) : ZString.Empty;

	public ZDecimal TariffTobaccoTaxQuantity => Factory.GetCached(ref tariffTobaccoTaxQuantity, () => AdditionalTaxes.Where(x => AdditionalTaxesTariffs.IsTobaccoTariffCode450(x.BZ_Tariff)).Sum(x => x.BZ_Qty1));
	CachedProperty<ZDecimal> tariffTobaccoTaxQuantity;

	public ZDecimal TariffTobaccoPreventionFundQuantity => Factory.GetCached(ref tariffTobaccoPreventionFundQuantity, () => AdditionalTaxes.Where(x => AdditionalTaxesTariffs.IsTobaccoTariffCode470(x.BZ_Tariff)).Sum(x => x.BZ_Qty1));
	CachedProperty<ZDecimal> tariffTobaccoPreventionFundQuantity;

	public override ZString UniversalTariffType
	{
		get
		{
			var result = base.UniversalTariffType;
			if (IsImport)
			{
				result = UniversalReferenceConstants.TariffTypes.ImportTariff;
			}
			else if (IsExportOrExportDeclarationActivation)
			{
				result = UniversalReferenceConstants.TariffTypes.ExportTariff;
			}
			return result;
		}
	}

	protected override bool UseUniversalConditionCheck => true;

	public override IUniversalRateCalcData CalcDataForConditionFormula => new CHConditionCalcDataForInvoiceLine(this);

	protected override ZBool ShouldReCalculateCustomsQtyOnLineQuantityChange => false;

	protected override IZZConditionSelectionCriteria[] GetConditionSelectionCriterias()
	{
		if (IsImport || IsExportOrExportDeclarationActivation)
		{
			return GetConditionSelectionCriteriaForExportOrImport().ToArray();
		}

		return base.GetConditionSelectionCriterias();
	}

	IEnumerable<IZZConditionSelectionCriteria> GetConditionSelectionCriteriaForExportOrImport()
	{
		if (!JI_NetMassConfirmation)
		{
			yield return new CHConditionSelectionCriteria(this, ZString.Empty, UniversalReferenceConstants.CusConditionType.WeightCheck1);
		}

		if (!JI_AdditionalUnitConfirmation)
		{
			yield return new CHConditionSelectionCriteria(this, ZString.Empty, UniversalReferenceConstants.CusConditionType.WeightCheck2);
		}

		if (!JI_StatisticalValueConfirmation)
		{
			yield return new CHConditionSelectionCriteria(this, ZString.Empty, UniversalReferenceConstants.CusConditionType.MeanValueCheck);
		}

		yield return ControlConditionSelectionCriteria;
	}

	public IZZConditionSelectionCriteria ControlConditionSelectionCriteria => Factory.GetValue(ref controlConditionSelectionCriteriaCached, GetControlConditionSelectionCriteria);
	CachedProperty<IZZConditionSelectionCriteria> controlConditionSelectionCriteriaCached;

	protected IZZConditionSelectionCriteria GetControlConditionSelectionCriteria() => new CHConditionSelectionCriteria(this, RefCusConditionTypes.ConditionClass.Control, ZString.Empty);

	public override ConditionChecker.EvaluateConditionValue EvaluateConditionValue => (conditionType, valueType, inputValue) =>
	{
		switch (valueType)
		{
			case UniversalReferenceConstants.CusConditionValueType.Permit:
				return Permits.Cast<Permit>().Any(x => x.CSI_IssuerType == inputValue);
			case UniversalReferenceConstants.CusConditionValueType.NonCustomsLaw:
				return NonCustomsLaws.Cast<NonCustomsLaw>().Any(x => x.CSI_Code == inputValue);
			case UniversalReferenceConstants.CusConditionValueType.Restriction:
				return Restrictions.Cast<Restriction>().Any(x => x.CSI_Code == inputValue);
			default:
				return false;
		}
	};

	public bool IsIndustrialTariff => JI_Tariff.CompareTo(UniversalReferenceConstants.Tariffs.BeginOfIndustrialTariffs) >= 0;

	public bool OriginIsDirectTransportationCountry => RefCusCodeListLoader.IsDirectTransportationCountry(Factory, JI_CountryOfOrigin, EffectiveAssessmentDate);

	public IZZRateSelectionCriteria NormalTariffDutyRateSelectionCriteria => (normalTariffDutyRateSelectionCriteria ?? (normalTariffDutyRateSelectionCriteria = new CachedProperty<IZZRateSelectionCriteria>(Factory, GetNormalTariffDutyRateSelectionCriteria))).Value;
	CachedProperty<IZZRateSelectionCriteria> normalTariffDutyRateSelectionCriteria;

	IZZRateSelectionCriteria GetNormalTariffDutyRateSelectionCriteria() => new NormalTariffRateSelectionCriteria(this, Universal.Constants.RateTypes.Duty, ZString.Empty);

	internal class NormalTariffRateSelectionCriteria : RateSelectionCriteria<JobComInvoiceLine>
	{
		public NormalTariffRateSelectionCriteria(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode) : base(invoiceLine, rateType, rateCode)
		{
			PrimaryPreference = PrimaryPreferenceCodes.NormalTariff;
		}
	}

	public ZString RateFormulaWithFallbackToNormalTariff => Factory.GetValue(ref rateFormulaWithFallbackToNormalTariff, GetRateFormulaWithFallbackToNormalTariff);
	CachedProperty<ZString> rateFormulaWithFallbackToNormalTariff;

	ZString GetRateFormulaWithFallbackToNormalTariff()
	{
		var rateFormula = ZString.Empty;

		if (!JI_CountryOfOrigin.IsEmpty && UniversalTariff != null)
		{
			if (!JI_PrimaryPreference.IsEmpty)
			{
				rateFormula = UniversalDutyRate?.ZZ2_RateFormula ?? ZString.Empty;
			}
			if ((rateFormula == ZString.Empty || rateFormula == UniversalReferenceConstants.FormulaPlaceholder.FreeRateFormula) && JI_PrimaryPreference != PrimaryPreferenceCodes.NormalTariff)
			{
				rateFormula = UniversalTariff?.GetApplicableRate(NormalTariffDutyRateSelectionCriteria)?.ZZ2_RateFormula ?? rateFormula;
			}
		}
		return rateFormula;
	}

	#region PreviousDocuments

	[ChildEditable(true)]
	public PreviousDocumentCollection PreviousDocuments => previousDocuments ?? (previousDocuments = GetPreviousDocuments());
	PreviousDocumentCollection previousDocuments;

	PreviousDocumentCollection GetPreviousDocuments()
	{
		var previousDocuments = CreateNewPreviousDocumentCollection();
		previousDocuments.Load();
		RegisterEditableChildObject(previousDocuments);

		return previousDocuments;
	}

	PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	#endregion

	#region SupportingDocuments

	[ChildEditable]
	public SupportingDocumentCollection SupportingDocuments => supportingDocuments ?? (supportingDocuments = GetSupportingDocuments());
	SupportingDocumentCollection supportingDocuments;

	SupportingDocumentCollection GetSupportingDocuments()
	{
		var result = CreateNewSupportingDocumentCollection();
		result.Load();
		RegisterEditableChildObject(result);

		return result;
	}

	SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	bool ISupportingDocumentParent.IsGSPCertificateRequired => JI_PrimaryPreference == PrimaryPreferenceCodes.PreferentialTariff && RefCusTradeGroupLoader.IsCountryPartOfDevelopingCountries(Factory, CountryOfOrigin, EffectiveAssessmentDate);

	public IEnumerable<SupportingDocument> SupportingDocumentsIncludingInherited => SupportingDocuments.Union(InvoiceHeader?.SupportingDocuments ?? Enumerable.Empty<SupportingDocument>()).Cast<SupportingDocument>();

	#endregion

	protected override TariffFormatter TariffFormatter => new TariffFormatterCH();

	[MaxLength(Schema.JI_FormattedTariffMaxLength)]
	public override ZString JI_FormattedTariff
	{
		get => base.JI_FormattedTariff;
		set
		{
			if (!IsCopying)
			{
				ExtendTariffDefault(ref value);
			}
			base.JI_FormattedTariff = value;
		}
	}

	void ExtendTariffDefault(ref ZString tariff)
	{
		if (UniversalTariffType == TariffTypes.ImportTariff)
		{
			var numericTariff = tariff.KeepNumericCharacters();
			if (numericTariff.Length == 8)
			{
				var defaultTariff = numericTariff + "000000";
				if (new TariffView.Loader(base.Factory).LoadMostRecentCachedTariff(GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff), TariffTypes.ImportTariff, defaultTariff, UniversalTariffValuationDate) != null)
				{
					tariff = defaultTariff;
				}
			}
		}
	}

	protected override ZString GetTariffDescription(ZString tariffCode) => UniversalTariff?.FullTariffDescription(EffectiveAssessmentDate, includeSectionHeadings: false, includeChapterHeading: false, useTariffPreferredLanguage: true, countryPreferedLanguage: LanguageForTariffDescription) ?? ZString.Empty;

	string LanguageForTariffDescription
	{
		get
		{
			var allowedLanguages = new[] { SwissCustomsLanguageList.Codes.German, SwissCustomsLanguageList.Codes.French, SwissCustomsLanguageList.Codes.Italian };
			
			var language = Declaration.JE_DeclarationLanguage.IsEmpty ? GlbStaff.CurrentUser.GS_WorkingLanguage : (ZString)$"{Declaration.JE_DeclarationLanguage}-CH";
			var twoLetterLanguageCode = language.SubstringSafe(0, 2).ToUpperInvariant();
			if (!allowedLanguages.Any(lang => lang == twoLetterLanguageCode))
			{
				language = Core.SharedConstants.Languages.EnglishAmerican;
			}
			return language;
		}
	}

	public override ZDecimal JI_Weight
	{
		get => base.JI_Weight;
		set
		{
			var oldValue = JI_Weight;
			base.JI_Weight = value;
			if (!IsCopying && oldValue != JI_Weight)
			{
				CalcualteFromWeightToCustomsQty();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Procedure();
			}
		}
	}

	public override ZString JI_WeightUQ
	{
		get => base.JI_WeightUQ;
		set
		{
			var oldValue = JI_WeightUQ;
			base.JI_WeightUQ = value;
			if (!IsCopying && oldValue != JI_WeightUQ)
			{
				CalcualteFromWeightToCustomsQty();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Weight();
			}
		}
	}

	public override ZDecimal JI_NetWeight
	{
		get => base.JI_NetWeight;
		set
		{
			base.JI_NetWeight = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Weight();
				Validation.ValidateJI_Tariff();
			}
		}
	}

	public override ZString JI_NetWeightUQ
	{
		get => base.JI_NetWeightUQ;
		set
		{
			base.JI_NetWeightUQ = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Weight();
				Validation.ValidateJI_Tariff();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_CustomsQuantity", Caption = "Gross Weight")]
	public override ZDecimal JI_CustomsQuantity
	{
		get => base.JI_CustomsQuantity;
		set
		{
			base.JI_CustomsQuantity = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_WeightIncludingInnerPackage();
			}
		}
	}

	protected override bool GetJI_CustomsQuantityReadOnly() => true;

	[MaxLength(4)]
	public override ZString JI_CustomsUnitQty { get => base.JI_CustomsUnitQty; set => base.JI_CustomsUnitQty = value; }

	protected override bool GetJI_CustomsUnitQtyInfoReadOnly() => true;

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_CustomsSecondQuantity", Caption = "Net Weight")]
	public override ZDecimal JI_CustomsSecondQuantity
	{
		get => base.JI_CustomsSecondQuantity;
		set
		{
			base.JI_CustomsSecondQuantity = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_WeightIncludingInnerPackage();
			}
		}
	}

	[ReadOnly(true)]
	public override ZString JI_CustomsSecondUnitQty { get => base.JI_CustomsSecondUnitQty; set => base.JI_CustomsSecondUnitQty = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_CustomsThirdQuantity", Caption = "Additional Quantity")]
	public override ZDecimal JI_CustomsThirdQuantity
	{
		get => base.JI_CustomsThirdQuantity;
		set
		{
			base.JI_CustomsThirdQuantity = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Tariff();
				Validation.ValidateJI_Weight();

				foreach (var additionalTax in AdditionalTaxes)
				{
					additionalTax.SetAdditionalTaxQuantity();
				}
			}
		}
	}

	[ReadOnly(true)]
	public override ZString JI_CustomsThirdUnitQty
	{
		get => base.JI_CustomsThirdUnitQty;
		set
		{
			base.JI_CustomsThirdUnitQty = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_CustomsThirdQuantity();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|JI_CustomsFourthQuantity", Caption = "Sensible Goods Qty")]
	[ReadOnlyMember(nameof(JI_CustomsFourthQuantity_ReadOnly))]
	public override ZDecimal JI_CustomsFourthQuantity { get => base.JI_CustomsFourthQuantity; set => base.JI_CustomsFourthQuantity = value; }

	public ZBool JI_CustomsFourthQuantity_ReadOnly => JI_CustomsFourthUnitQty.IsEmpty;

	[ReadOnly(true)]
	public override ZString JI_CustomsFourthUnitQty
	{
		get => base.JI_CustomsFourthUnitQty;
		set
		{
			var oldValue = JI_CustomsFourthUnitQty;
			base.JI_CustomsFourthUnitQty = value;
			if (!IsCopying && (JI_CustomsFourthUnitQty.IsEmpty || oldValue != JI_CustomsFourthUnitQty))
			{
				JI_CustomsFourthQuantity = ZDecimal.Zero;
			}
		}
	}

	public ZBool IsReturnedGoodsWithRefundRequest => IsExportOrExportDeclarationActivation && JI_RefundType == UniversalReferenceConstants.RefundType.ReturnedGoodsWithRefundRequest;

	protected override ZString JI_RX_NKLinePriceCurrCore => InvoiceHeader?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|RateFormulaDescription", Caption = "Rate")]
	public ZString RateFormulaDescription => Factory.GetCached(ref rateFormulaDescription, () => UniversalDutyRate?.RateFormulaDescription ?? ZString.Empty);
	CachedProperty<ZString> rateFormulaDescription;

	public void CalcualteFromWeightToCustomsQty()
	{
		JI_CustomsQuantity = new ZWeight(JI_Weight, JI_WeightUQ).InKilogramsSafe;
	}

	public override void CalculateFromNetWeightToCustomsQty()
	{
		JI_CustomsSecondQuantity = new ZWeight(JI_NetWeight, JI_NetWeightUQ).InKilogramsSafe;
	}

	public override bool ShouldConverCustomsQuantity2 => false;

	protected override bool ClearCustomsQuantityWhenUQSet => false;

	protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new UniversalTariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>();

	public new ICusVehicleCollection<CusVehicle, JobComInvoiceLine> Vehicles => (ICusVehicleCollection<CusVehicle, JobComInvoiceLine>)base.Vehicles;

	protected override ICusVehicleCollection<Customs.Business.CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection(this);

	public override VehicleRelationshipType VehicleRelationship => VehicleRelationshipType.Many;

	[ChildEditable(true)]
	public TobaccoCollection Tobaccos
	{
		get
		{
			if (tobaccos == null)
			{
				tobaccos = new TobaccoCollection(this);
				tobaccos.Load();
				RegisterEditableChildObject(tobaccos);
			}
			return tobaccos;
		}
	}

	TobaccoCollection tobaccos;

	#region InAndOutwardProcessing

	public InAndOutwardProcessingLookups InAndOutwardLookups => InAndOutwardProcessing.Lookups;

	[ChildEditable(true)]
	public InAndOutwardProcessingCollection InAndOutwardProcessings
	{
		get
		{
			if (inAndOutwardProcessings == null)
			{
				inAndOutwardProcessings = new InAndOutwardProcessingCollection(this);
				inAndOutwardProcessings.Load();
				RegisterEditableChildObject(inAndOutwardProcessings);
			}
			return inAndOutwardProcessings;
		}
	}
	InAndOutwardProcessingCollection inAndOutwardProcessings;

	public InAndOutwardProcessing InAndOutwardProcessing => inAndOutwardProcessing != null && !inAndOutwardProcessing.IsDeleted ? inAndOutwardProcessing : (inAndOutwardProcessing = InAndOutwardProcessings.FindOrCreate());
	InAndOutwardProcessing inAndOutwardProcessing;

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingDirection", Caption = "Direction")]
	[List(nameof(InAndOutwardLookups) + "." + nameof(InAndOutwardProcessingLookups.DirectionList))]
	public ZString InAndOutwardProcessingDirection
	{
		get => InAndOutwardProcessing.CSI_SubType;
		set
		{
			InAndOutwardProcessing.CSI_SubType = value;
			InAndOutwardProcessingDirectionInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo InAndOutwardProcessingDirectionInfo => GetWrappedZPropertyInfo(nameof(InAndOutwardProcessingDirection), _ => InAndOutwardProcessing.CSI_SubTypeInfo);

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingRefinementType", Caption = "Refinement Type")]
	[List(nameof(InAndOutwardLookups) + "." + nameof(InAndOutwardProcessingLookups.RefinementTypeList))]
	public ZString InAndOutwardProcessingRefinementType
	{
		get => InAndOutwardProcessing.CSI_Code;
		set
		{
			InAndOutwardProcessing.CSI_Code = value;
			InAndOutwardProcessingRefinementTypeInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo InAndOutwardProcessingRefinementTypeInfo => GetWrappedZPropertyInfo(nameof(InAndOutwardProcessingRefinementType), _ => InAndOutwardProcessing.CSI_CodeInfo);

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingProcessType", Caption = "Process Type")]
	[List(nameof(InAndOutwardLookups) + "." + nameof(InAndOutwardProcessingLookups.ProcessTypeList))]
	public ZString InAndOutwardProcessingProcessType
	{
		get => InAndOutwardProcessing.CSI_Procedure;
		set
		{
			InAndOutwardProcessing.CSI_Procedure = value;
			InAndOutwardProcessingProcessTypeInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo InAndOutwardProcessingProcessTypeInfo => GetWrappedZPropertyInfo(nameof(InAndOutwardProcessingProcessType), _ => InAndOutwardProcessing.CSI_ProcedureInfo);

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingBillingType", Caption = "Billing Type")]
	[List(nameof(InAndOutwardLookups) + "." + nameof(InAndOutwardProcessingLookups.BillingTypeList))]
	public ZString InAndOutwardProcessingBillingType
	{
		get => InAndOutwardProcessing.CSI_IssuerType;
		set
		{
			InAndOutwardProcessing.CSI_IssuerType = value;
			InAndOutwardProcessingBillingTypeInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo InAndOutwardProcessingBillingTypeInfo => GetWrappedZPropertyInfo(nameof(InAndOutwardProcessingBillingType), _ => InAndOutwardProcessing.CSI_IssuerTypeInfo);

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingRepair", Caption = "Repair")]
	public ZBool InAndOutwardProcessingRepair
	{
		get => InAndOutwardProcessing.Repair;
		set
		{
			InAndOutwardProcessing.Repair = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Tariff();
				Validation.ValidateJI_NonTradingGoods();
			}
			InAndOutwardProcessingRepairInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo InAndOutwardProcessingRepairInfo => GetWrappedZPropertyInfo(nameof(InAndOutwardProcessingRepair), _ => InAndOutwardProcessing.RepairInfo);

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingRepairReason", Caption = "Repair Reason")]
	public ZString InAndOutwardProcessingRepairReason
	{
		get => InAndOutwardProcessing.CSI_Description;
		set
		{
			InAndOutwardProcessing.CSI_Description = value;
			InAndOutwardProcessingRepairReasonInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo InAndOutwardProcessingRepairReasonInfo => GetWrappedZPropertyInfo(nameof(InAndOutwardProcessingRepairReason), _ => InAndOutwardProcessing.CSI_DescriptionInfo);

	[ChildEditable(true)]
	[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
	public NotifyCustomsOfficeCollection NotifyCustomsOffices
	{
		get
		{
			if (notifyCustomsOffices == null)
			{
				notifyCustomsOffices = new NotifyCustomsOfficeCollection(this);
				RegisterEditableChildObject(notifyCustomsOffices);
				notifyCustomsOffices.Load();
			}

			return notifyCustomsOffices;
		}
	}
	NotifyCustomsOfficeCollection notifyCustomsOffices;
	#endregion

	public override ZString JI_CountryOfOrigin
	{
		get => base.JI_CountryOfOrigin;
		set
		{
			var oldValue = JI_CountryOfOrigin;
			base.JI_CountryOfOrigin = value;
			if (!IsCopying && oldValue != JI_CountryOfOrigin)
			{
				DefaultDutyRateAdditionalCodeIfApplicable();
				EntryInstruction?.MarkAsNeedingValidation();
				if (IsImport)
				{
					AdditionalTaxes.RebuildAdditionalTaxes();
				}
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateNetDuty();
				Validation.ValidateDutyRateAdditionalCode();
				Validation.ValidateJI_PrimaryPreference();
				Validation.ValidateJI_NonCustomsLawObligation();
			}
		}
	}

	public override ZDecimal JI_LinePrice
	{
		get => base.JI_LinePrice;
		set
		{
			base.JI_LinePrice = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJI_Procedure();
			}
		}
	}

	#region Additional Code

	public ZString DutyRateFormulaNumber => Factory.GetCached(ref fDutyRateFormulaNumber, GetDutyRateFormulaNumber);
	CachedProperty<ZString> fDutyRateFormulaNumber;

	ZString GetDutyRateFormulaNumber()
	{
		var rate = UniversalDutyRate;
		if (rate != null && rate.RateFormulaNumber.HasValue)
		{
			var number = rate.RateFormulaNumber.Value;
			if ((rate.ZZ2_RateFormula.Contains(UniversalReferenceConstants.FormulaPlaceholder.NetWeightUOMPlaceHolder) || rate.ZZ2_RateFormula.Contains(UniversalReferenceConstants.FormulaPlaceholder.GrossWeightUOMPlaceHolder)))
			{
				number *= 100m;
			}
			return FormattableString.Invariant($"{number.Normalize()}");
		}
		else
		{
			return ZString.Empty;
		}
	}

	public ZPropertyInfo DutyRateFormulaNumberInfo => GetZPropertyInfo(Schema.DutyRateFormulaNumber);

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|DutyRateConfirmation", Caption = "Confirmation")]
	public ZBool DutyRateConfirmation
	{
		get
		{
			return AdditionalCodeData.CY_IsOverridden;
		}
		set
		{
			AdditionalCodeData.CY_IsOverridden = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateDutyRateAdditionalCode();
				Validation.ValidateDutyRateConfirmation();
			}
			DutyRateConfirmationInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo DutyRateConfirmationInfo => GetZPropertyInfo(Schema.DutyRateConfirmation);

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|DutyRateAdditionalCode", Caption = "Duty Rate")]
	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalCodesList))]
	[MaxLength(Schema.DutyRateAdditionalCodeMaxLength)]
	[ReadOnlyMember(nameof(DutyRateAdditionalCode_ReadOnly))]
	public ZString DutyRateAdditionalCode
	{
		get
		{
			return AdditionalCodeData.CY_Code;
		}
		set
		{
			CheckMaximumLength(DutyRateAdditionalCodeInfo, value);
			AdditionalCodeData.CY_Code = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateDutyRateAdditionalCode();
				Validation.ValidateDutyRateConfirmation();
				Validation.ValidateJI_Procedure();
			}
			DutyRateAdditionalCodeInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo DutyRateAdditionalCodeInfo => GetZPropertyInfo(Schema.DutyRateAdditionalCode);

	ZBool DutyRateAdditionalCode_ReadOnly => IsFixedRate;

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|DutyRateDescription", Caption = "Duty Rate")]
	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalCodesList))]
	[ReadOnlyMember(nameof(DutyRateAdditionalCode_ReadOnly))]
	public ZString DutyRateDescription
	{
		get
		{
			return Lookups.AdditionalCodesList.GetDescriptionFromCode(DutyRateAdditionalCode);
		}
		set
		{
			DutyRateAdditionalCode = Lookups.AdditionalCodesList.GetCodeFromDescription(value);
		}
	}

	public ZPropertyInfo DutyRateDescriptionInfo => GetWrappedZPropertyInfo(Schema.DutyRateDescription, x => DutyRateAdditionalCodeInfo);

	AdditionalCodeDataCollection AdditionalCodes
	{
		get
		{
			if (fAdditionalCodes == null)
			{
				fAdditionalCodes = new AdditionalCodeDataCollection(this);
				fAdditionalCodes.Load();
				RegisterEditableChildObject(fAdditionalCodes);
			}
			return fAdditionalCodes;
		}
	}
	AdditionalCodeDataCollection fAdditionalCodes;

	AdditionalCodeData AdditionalCodeData => additionalCodeData != null && !additionalCodeData.IsDeleted ? additionalCodeData : (additionalCodeData = AdditionalCodes.FindOrCreate());
	AdditionalCodeData additionalCodeData;

	protected override ZString AdditionalCode => DutyRateAdditionalCode;

	#endregion

	[ChildEditable(true)]
	public NonCustomsLawCollection NonCustomsLaws
	{
		get
		{
			if (nonCustomsLaws == null)
			{
				nonCustomsLaws = new NonCustomsLawCollection(this);
				nonCustomsLaws.Load();
				RegisterEditableChildObject(nonCustomsLaws);
			}
			return nonCustomsLaws;
		}
	}
	NonCustomsLawCollection nonCustomsLaws;

	[ChildEditable(true)]
	public RestrictionCollection Restrictions
	{
		get
		{
			if (restrictions == null)
			{
				restrictions = new RestrictionCollection(this);
				restrictions.Load();
				RegisterEditableChildObject(restrictions);
			}
			return restrictions;
		}
	}
	RestrictionCollection restrictions;

	HugeSequenceNumberGenerator IRestrictionParent.RestrictionsLineNumberGenerator => restrictionsLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => new TypedEnumerable<IHugeSequenceNumberLine>(Restrictions));
	HugeSequenceNumberGenerator restrictionsLineNumberGenerator;

	#region CusLineTariffDetail

	protected override bool SupportsAdditionalTariffs => true;

	[ChildEditable(true)]
	public CusLineTariffDetailCollection AdditionalTaxes
	{
		get
		{
			if (fAdditionalTaxes == null)
			{
				fAdditionalTaxes = new CusLineTariffDetailCollection(this, UniversalReferenceConstants.RateTypes.AdditionalTaxes);
				fAdditionalTaxes.Load();
				RegisterEditableChildObject(fAdditionalTaxes);
			}
			return fAdditionalTaxes;
		}
	}
	CusLineTariffDetailCollection fAdditionalTaxes;

	[ChildEditable(true)]
	public CusLineTariffDetailCollection AdditionalFees
	{
		get
		{
			if (fAdditionalFees == null)
			{
				fAdditionalFees = new CusLineTariffDetailCollection(this, UniversalReferenceConstants.RateTypes.AdditionalFees);
				fAdditionalFees.Load();
				RegisterEditableChildObject(fAdditionalFees);
			}
			return fAdditionalFees;
		}
	}
	CusLineTariffDetailCollection fAdditionalFees;

	#endregion

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|UNDGCodes", Caption = "UN Dangerous Codes", MediumCaption = "UNDG Codes", ShortCaption = "UNDG")]
	public ZString UNDGCodes => UNDGs.AsString;

	public ZPropertyInfo UNDGCodesInfo => GetZPropertyInfo(Schema.UNDGCodes);

	protected override void InitialiseUNDGs()
	{
		base.InitialiseUNDGs();
		((IBindingList)UNDGs).ListChanged += (sender, args) =>
		{
			Validation.ValidateUNDGCodes();
			UNDGCodesInfo.RefreshBinding();
		};
		UNDGs.EnableMaxCountValidation(99, false, NotificationType.MessageError, PassarValidationMessages.MessageCH0006, null);
	}

	public bool IsReturnedGoods => UniversalReferenceConstants.ProcedureCodesEdec.IsReturnedGoods(JI_Procedure);

	public bool IsWithoutDuty => UniversalReferenceConstants.ProcedureCodesEdec.IsWithoutDuty(JI_Procedure);

	public bool IsCustomsRelief => UniversalReferenceConstants.ProcedureCodesEdec.IsCustomsRelief(JI_Procedure);

	public bool IsTobaccoRefundType => JI_RefundType == UniversalReferenceConstants.RefundType.TobaccoTaxRefund || JI_RefundType == UniversalReferenceConstants.RefundType.TobaccoProductsExTaxWarehouse;

	public bool IsFixedRate => Lookups.AdditionalCodesList.Count == 0;

	public bool IsOrdinaryProcess => InAndOutwardProcessingProcessType == InAndOutwardProcessingProcessTypesEdec.DueProcedure;

	public bool IsOverriddenRateZero => JI_RateOverride && JI_OverriddenRate.IsEmpty;

	public bool TariffHasMultipleRates => Lookups.AdditionalCodesList.Count > 1;

	public bool IsSamnaunFreeZoneTraffic => Factory.GetCached(ref fIsSamnaunFreeZoneTraffic,
		() => AdditionalInformations.Cast<AdditionalInformation>().Any(a => a.IsSamnaunFreeZoneTraffic));
	CachedProperty<bool> fIsSamnaunFreeZoneTraffic;

	public bool HasOriginDocument => Factory.GetCached(ref fHasOriginDocument,
		() => SupportingDocumentsIncludingInherited.Any(d => d.IsValidOriginDocument));
	CachedProperty<bool> fHasOriginDocument;

	public bool HasFederalTaxAdministrationCommitmentPermit => Factory.GetCached(ref fHasFederalTaxAdministrationCommitmentPermit,
		() => Permits.Cast<Permit>().Any(x => x.CSI_Code == UniversalReferenceConstants.PermitCodes.Commitment && x.CSI_IssuerType == UniversalReferenceConstants.PermitAuthorityCodes.FTA));
	CachedProperty<bool> fHasFederalTaxAdministrationCommitmentPermit;

	public bool HasReversPermit => Factory.GetCached(ref fHasReversPermit,
		() => Permits.Cast<Permit>().Any(x => x.CSI_Code == PermitCodes.ReversTobacco && x.CSI_IssuerType == UniversalReferenceConstants.PermitAuthorityCodes.STB));
	CachedProperty<bool> fHasReversPermit;

	public bool HasAdditionalInformationA1301 => Factory.GetCached(ref hasAdditionalInformationA1301,
		() => AdditionalInformations.Cast<AdditionalInformation>().Any(x => x.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.VocQuantityInKilograms));
	CachedProperty<bool> hasAdditionalInformationA1301;

	public string TariffSensibleGoodsCode => UniversalTariff?.GetAttribute(UniversalReferenceConstants.TariffAttributes.SensibleGoodsCode)?.ZZ3_Value;

	public bool HasAdditionalInformationA1102 => Factory.GetCached(ref hasAdditionalInformationA1102,
		() => AdditionalInformations.Cast<AdditionalInformation>().Any(x => x.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.AlcoholOnBeerRefundLiters));
	CachedProperty<bool> hasAdditionalInformationA1102;

	public bool HasAnySOTAAdditionalTax => Factory.GetCached(ref hasAnySOTAAdditionalTax,
		() => AdditionalTaxes.Cast<CusLineTariffDetail>().Any(y => y.IsSOTAAdditionalTax));
	CachedProperty<bool> hasAnySOTAAdditionalTax;

	public bool HasAnyPreventionAdditionalTax => Factory.GetCached(ref hasAnyPreventionAdditionalTax,
		() => AdditionalTaxes.Cast<CusLineTariffDetail>().Any(y => y.IsPreventionAdditionalTax));
	CachedProperty<bool> hasAnyPreventionAdditionalTax;

	public bool HasAnyTobaccoSubGroup02or03 => Factory.GetCached(ref hasAnyTobaccoSubGroup02or03,
		() => Tobaccos.Cast<Tobacco>().Any(y => y.CSI_Code == UniversalReferenceConstants.TobaccoMainGroupCodes.CutTobacco &&
											(y.CSI_SubType == UniversalReferenceConstants.TobaccoSubGroupCodes._02
											|| y.CSI_SubType == UniversalReferenceConstants.TobaccoSubGroupCodes._03)));
	CachedProperty<bool> hasAnyTobaccoSubGroup02or03;

	void UpdateNetDutyDependencies()
	{
		if (NetDuty)
		{
			if (JI_WeightIncludingInnerPackageUQ.IsEmpty)
			{
				JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Kilograms;
			}

			var tareSupplements = UniversalTariff?.GetAttributes(UniversalReferenceConstants.TariffAttributes.TareSupplement)
				.Select(attribute => ZDecimal.ParseSafe(attribute.ZZ3_Value, ZDecimal.Zero)).Where(x => !x.IsEmpty).OrderByDescending(x => x);

			JI_TareSupplementConfirmation = tareSupplements == null || tareSupplements.Count() != 1;
			JI_TareSupplementPercentage = tareSupplements?.Count() > 0 ? tareSupplements.First() : (ZDecimal)10m;
		}
		else
		{
			ResetNetDutyDependencies();
		}
	}

	void ResetNetDutyDependencies()
	{
		JI_WeightIncludingInnerPackage = 0m;
		JI_WeightIncludingInnerPackageUQ = ZString.Empty;
		JI_TareSupplementPercentage = 0m;
		JI_TareSupplementConfirmation = false;
	}

	void UpdateVatCode()
	{
		var effectiveVatApplicabilities = EffectiveVATApplicabilities;
		JI_ZZF_NKTaxType = effectiveVatApplicabilities.Count() == 1 ? effectiveVatApplicabilities.First().ZX5_ZZF_NKTaxOrFeeCode : ZString.Empty;
	}

	void DefaultDutyRateAdditionalCodeIfApplicable()
	{
		if (IsImport)
		{
			var additionalCodesList = Lookups.AdditionalCodesList;
			DutyRateAdditionalCode = additionalCodesList.Count == 1 ? additionalCodesList[0].Code : string.Empty;
		}
	}

	public bool IsExportOrExportDeclarationActivation => JobDeclaration?.IsExportOrExportDeclarationActivation ?? false;

	public IEnumerable<VATApplicabilityView> EffectiveVATApplicabilities => UniversalTariff != null ? UniversalTariff.GetEffectiveVATApplicabilities(EffectiveAssessmentDate) : new List<VATApplicabilityView>();

	protected override ZBool IsSupportEmptyPackType(BasePackage package) => true;

	public override void OnLoaded()
	{
		base.OnLoaded();

		netDuty = IsImport && JI_WeightIncludingInnerPackage > 0;
	}

	protected override void OnFactorySaving()
	{
		if (IsImport)
		{
			Restrictions.RemoveAndDeleteAll();
			PreviousDocuments.RemoveAndDeleteAll();
			SetPermitObligation();
			SetNonCustomsLawObligation();
		}
		else
		{
			ResetNetDutyDependencies();
			AdditionalFees.RemoveAndDeleteAll();
			AdditionalTaxes.RemoveAndDeleteAll();
			Permits.RemoveAndDeleteAll();
			NonCustomsLaws.RemoveAndDeleteAll();
			NotifyCustomsOffices.RemoveAndDeleteAll();
		}

		base.OnFactorySaving();

		inAndOutwardProcessing = null;
	}

	void SetPermitObligation()
	{
		if (Permits.Any())
		{
			JI_PermitObligation = PermitObligationCodes.PermitNeeded;
		}
		else if (JI_PermitObligation.IsEmpty)
		{
			if (UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.HasOptionalPermit, UniversalReferenceConstants.TariffAttributes.Values._1) ?? false)
			{
				JI_PermitObligation = PermitObligationCodes.NoPermitNeeded;
			}
			else
			{
				JI_PermitObligation = PermitObligationCodes.NoPermit;
			}
		} 
	}

	void SetNonCustomsLawObligation()
	{
		if (NonCustomsLaws.Any())
		{
			JI_NonCustomsLawObligation = NonCustomsLawObligationCodes.Needed;
		}
		else if (JI_NonCustomsLawObligation.IsEmpty)
		{
			if (UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.HasOptionalNCL, UniversalReferenceConstants.TariffAttributes.Values._1) ?? false)
			{
				var conditions = ConditionChecker.GetApplicableConditions(Factory, UniversalTariff, ControlConditionSelectionCriteria);
				var hasNclConditions = conditions.Any(c => c.CusConditionType.ZX2_ConditionType.StartsWith(CusConditionType.NonCustomsLawPrefix));

				if (hasNclConditions)
				{
					JI_NonCustomsLawObligation = NonCustomsLawObligationCodes.NotNeededAccordingDeclarant;
				}
				else
				{
					JI_NonCustomsLawObligation = NonCustomsLawObligationCodes.NotPossible;
				}
			}
			else
			{
				JI_NonCustomsLawObligation = NonCustomsLawObligationCodes.NotPossible;
			}
		}
	}

	public override void Delete()
	{
		EntryInstruction?.MarkAsNeedingValidation();

		using (AdditionalInformationLineNumberGenerator.GetLineNumberSuspender())
		using (((IRestrictionParent)this).RestrictionsLineNumberGenerator.GetLineNumberSuspender())
		{
			base.Delete();
		}
	}

	class Strategy : JobComInvoiceLineFetchStrategy
	{
		public Strategy(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, BusinessObject.PK);
		}
	}

	public class CHConditionSelectionCriteria : ZZConditionSelectionCriteria<JobComInvoiceLine>
	{
		public CHConditionSelectionCriteria(JobComInvoiceLine invoiceLine, ZString conditionClass, ZString conditionType)
			: base(invoiceLine, conditionClass, conditionType)
		{
		}

		protected override ZString GetTradeGroupCountry(JobComInvoiceLine invoiceLine) => invoiceLine.IsImport ? invoiceLine.EffectiveCountryOfOrigin : (invoiceLine.JobDeclaration?.JE_GoodsDestination ?? ZString.Empty);
	}
}
