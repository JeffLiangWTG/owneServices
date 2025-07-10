using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class JobComInvoiceLineValidation : AutoCHJobComInvoiceLineValidation
{
	public JobComInvoiceLineValidation(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateSpecialMentions();
		ValidateNetDuty();
		ValidateJI_CustomsValue();
		ValidateDutyRateAdditionalCode();
		ValidateDutyRateConfirmation();
		ValidateUNDGCodes();
		PlausiValidation.CheckCH0003(Parent);
	}

	protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New(Parent.JobDeclaration));
	PlausiValidation plausiValidation;

	protected override bool IsTariffMandatory => !Parent.EntryInstruction?.IsSimplified ?? true;

	protected override void CheckJI_Tariff()
	{
		base.CheckJI_Tariff();
		PlausiValidation.CheckR177(Parent.JI_TariffInfo, Parent);

		if (Parent.JobDeclaration?.IsExportOrExportDeclarationActivation ?? false)
		{
			if (Parent.JI_Tariff.Length != 0 && Parent.JI_Tariff.Length != 11)
			{
				Parent.JI_TariffInfo.AddMessageError(ValidationMessages.JobComInvoiceLine.InvalidTariff);
			}
		}

		PlausiValidation.CheckR162R201(Parent.JI_TariffInfo, Parent);
		PlausiValidation.CheckR175R179(Parent.JI_TariffInfo, Parent);
		PlausiValidation.CheckR193(Parent.JI_TariffInfo, Parent);
		PlausiValidation.CheckR356(Parent.JI_TariffInfo, Parent);
		PlausiValidation.CheckR182(Parent.JI_TariffInfo, Parent);
		PlausiValidation.CheckR256(Parent.JI_TariffInfo, Parent);
		PlausiValidation.CheckR338acd(Parent.JI_TariffInfo, Parent);
		PlausiValidation.CheckNS30003_Tariff(Parent.JI_TariffInfo, Parent.EntryInstruction);
		PlausiValidation.CheckR336(Parent.JI_TariffInfo, Parent);
	}

	protected override void CheckJI_Weight()
	{
		base.CheckJI_Weight();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_WeightInfo);
		PlausiValidation.CheckR127(Parent.JI_WeightInfo, Parent);
		PlausiValidation.CheckR267(Parent.JI_WeightInfo, Parent);
		PlausiValidation.CheckR268b(Parent.JI_WeightInfo, Parent);
		PlausiValidation.CheckNP70001(Parent.JI_WeightInfo, Parent);
		PlausiValidation.CheckNP70009_Weight(Parent.JI_WeightInfo, Parent);
		PlausiValidation.CheckNP70026(Parent.JI_WeightInfo, Parent);
		PlausiValidation.CheckR249b(Parent.JI_WeightInfo, Parent);
	}

	protected override void CheckJI_WeightUQ()
	{
		base.CheckJI_WeightUQ();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_WeightUQInfo);
	}

	protected override void CheckJI_NetWeight()
	{
		base.CheckJI_NetWeight();
		PlausiValidation.CheckR128(Parent.JI_NetWeightInfo, Parent);
		PlausiValidation.CheckNS30092(Parent.JI_NetWeightInfo, Parent);
		PlausiValidation.CheckNS30003_NetWeight(Parent.JI_NetWeightInfo, Parent.EntryInstruction);
	}

	protected override void CheckJI_NetWeightUQ()
	{
		base.CheckJI_NetWeightUQ();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_NetWeightUQInfo);
	}

	protected override void CheckJI_CustomsThirdQuantity()
	{
		base.CheckJI_CustomsThirdQuantity();
		PlausiValidation.CheckR130(Parent.JI_CustomsThirdQuantityInfo, Parent);
		PlausiValidation.CheckNP70097(Parent.JI_CustomsThirdQuantityInfo, Parent);
		PlausiValidation.CheckNS30003_GreaterThanZero(Parent.JI_CustomsThirdQuantityInfo, Parent.EntryInstruction);
	}

	public void ValidateSpecialMentions()
	{
		ValidateCalculatedProperty(Parent.SpecialMentionsInfo);
	}

	protected virtual void CheckSpecialMentions()
	{
		if (Parent.IsImport)
		{
			SpecialMentionsHelper.Validate(Parent.SpecialMentionsInfo, Parent.CountOfSpecialMentionsLines);
		}
	}

	protected override void CheckJI_PrimaryPreference()
	{
		base.CheckJI_PrimaryPreference();
		if (Parent.IsImport)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_PrimaryPreferenceInfo);
		}
		PlausiValidation.CheckR158(Parent.JI_PrimaryPreferenceInfo, Parent);
		PlausiValidation.CheckR166c(Parent.JI_PrimaryPreferenceInfo, Parent);
		PlausiValidation.CheckR290(Parent.JI_PrimaryPreferenceInfo, Parent);
	}

	protected override void CheckJI_Procedure()
	{
		base.CheckJI_Procedure();
		if (Parent.IsExportOrExportDeclarationActivation)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_ProcedureInfo);
		}
		PlausiValidation.CheckR165(Parent.JI_ProcedureInfo, Parent);
		PlausiValidation.CheckR176(Parent.JI_ProcedureInfo, Parent);
		PlausiValidation.CheckR183a(Parent.JI_ProcedureInfo, Parent);
		PlausiValidation.CheckR183b(Parent.JI_ProcedureInfo, Parent);
		PlausiValidation.CheckR353(Parent.JI_ProcedureInfo, Parent);
		PlausiValidation.CheckR359(Parent.JI_ProcedureInfo, Parent);
		PlausiValidation.CheckR249e(Parent.JI_ProcedureInfo, Parent);
	}

	protected override void CheckJI_ZZF_NKTaxType()
	{
		base.CheckJI_ZZF_NKTaxType();
		PlausiValidation.CheckR276(Parent.JI_ZZF_NKTaxTypeInfo, Parent);
		PlausiValidation.CheckR124(Parent.JI_ZZF_NKTaxTypeInfo, Parent);
		PlausiValidation.CheckR277(Parent.JI_ZZF_NKTaxTypeInfo, Parent);
		PlausiValidation.CheckR219(Parent.JI_ZZF_NKTaxTypeInfo, Parent);
	}

	protected override void CheckJI_LinePrice()
	{
		base.CheckJI_LinePrice();
		PlausiValidation.CheckR268a(Parent.JI_LinePriceInfo, Parent);
		PlausiValidation.CheckNP70009_Price(Parent.JI_LinePriceInfo, Parent);
	}

	protected override void CheckJI_CountryOfOrigin()
	{
		base.CheckJI_CountryOfOrigin();
		PlausiValidation.CheckR275(Parent.JI_CountryOfOriginInfo, Parent);
		PlausiValidation.CheckR173(Parent.JI_CountryOfOriginInfo, Parent);
	}

	public virtual void ValidateNetDuty()
	{
		ValidateCalculatedProperty(Parent.NetDutyInfo);
	}

	protected void CheckNetDuty()
	{
		PlausiValidation.CheckR224(Parent.NetDutyInfo, Parent);
	}

	public void ValidateJI_CustomsValue()
	{
		ValidateCalculatedProperty(Parent.JI_CustomsValueInfo);
	}

	protected virtual void CheckJI_CustomsValue()
	{
		PlausiValidation.CheckR123(Parent.JI_CustomsValueInfo, Parent);
		PlausiValidation.CheckR249a(Parent.JI_CustomsValueInfo, Parent);
	}

	public virtual void ValidateDutyRateAdditionalCode()
	{
		ValidateCalculatedProperty(Parent.DutyRateAdditionalCodeInfo);
	}

	protected virtual void CheckDutyRateAdditionalCode()
	{
		PlausiValidation.CheckR133b(Parent.DutyRateAdditionalCodeInfo, Parent);
	}

	public virtual void ValidateDutyRateConfirmation()
	{
		ValidateCalculatedProperty(Parent.DutyRateConfirmationInfo);
	}

	protected virtual void CheckDutyRateConfirmation()
	{
		PlausiValidation.CheckR133a(Parent.DutyRateConfirmationInfo, Parent);
	}

	public void ValidateUNDGCodes()
	{
		ValidateCalculatedProperty(Parent.UNDGCodesInfo);
	}

	protected void CheckUNDGCodes()
	{
		PlausiValidation.CheckNS30003_NotAllowed(Parent.UNDGCodesInfo, Parent.EntryInstruction, isEmpty: () => Parent.UNDGs.Count == 0);
	}

	protected override void CheckJI_Description()
	{
		base.CheckJI_Description();

		PlausiValidation.CheckCH0004(Parent.JI_DescriptionInfo);
	}

	protected override void CheckJI_RefundType()
	{
		base.CheckJI_RefundType();
		PlausiValidation.CheckNS30003_NotEmpty(Parent.JI_RefundTypeInfo, Parent.EntryInstruction);
		PlausiValidation.CheckNP70168(Parent.JI_RefundTypeInfo, Parent);
		PlausiValidation.CheckNP70195(Parent.JI_RefundTypeInfo, Parent);
		PlausiValidation.CheckNP70197(Parent.JI_RefundTypeInfo, Parent);
		PlausiValidation.CheckNP70175(Parent.JI_RefundTypeInfo, Parent);

		if (Parent?.IsExportOrExportDeclarationActivation ?? false)
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_RefundTypeInfo);
		}
	}

	protected override void CheckJI_RefundReferenceNumber()
	{
		base.CheckJI_RefundReferenceNumber();

		if (Parent?.IsReturnedGoodsWithRefundRequest ?? false)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_RefundReferenceNumberInfo);
		}
	}

	protected override void CheckJI_RefundGoodsItemNumber()
	{
		base.CheckJI_RefundGoodsItemNumber();

		if (Parent?.IsReturnedGoodsWithRefundRequest ?? false)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_RefundGoodsItemNumberInfo);
		}
	}

	protected override void CheckJI_RefundReason()
	{
		base.CheckJI_RefundReason();

		if (Parent?.IsReturnedGoodsWithRefundRequest ?? false)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_RefundReasonInfo);
		}
	}

	protected override void CheckJI_WeightIncludingInnerPackage()
	{
		base.CheckJI_WeightIncludingInnerPackage();
		MandatoryValidation.CheckNotNegative(Parent.JI_WeightIncludingInnerPackageInfo);

		PlausiValidation.CheckR159(Parent.JI_WeightIncludingInnerPackageInfo, Parent);
		PlausiValidation.CheckR160(Parent.JI_WeightIncludingInnerPackageInfo, Parent);
	}

	protected override void CheckJI_WeightIncludingInnerPackageUQ()
	{
		base.CheckJI_WeightIncludingInnerPackageUQ();

		if (!Parent.JI_WeightIncludingInnerPackage.IsEmpty)
		{
			MandatoryValidation.CheckEntered(Parent.JI_WeightIncludingInnerPackageUQInfo);

			if (Parent != null && Parent.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_WeightIncludingInnerPackageUQInfo);
			}
		}
	}

	protected override void CheckJI_TareSupplementPercentage()
	{
		base.CheckJI_TareSupplementPercentage();
		MandatoryValidation.CheckNotNegative(Parent.JI_TareSupplementPercentageInfo);
	}

	protected override void CheckJI_PermitObligation()
	{
		base.CheckJI_PermitObligation();

		if (Declaration?.IsImport ?? false)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_PermitObligationInfo);
		}

		PlausiValidation.CheckR134abc(Parent.JI_PermitObligationInfo, Parent);
		PlausiValidation.CheckR135(Parent.JI_PermitObligationInfo, Parent);
	}

	protected override void CheckJI_NonCustomsLawObligation()
	{
		base.CheckJI_NonCustomsLawObligation();

		if (Declaration?.IsImport ?? false)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_NonCustomsLawObligationInfo);
		}

		PlausiValidation.CheckR144abc(Parent.JI_NonCustomsLawObligationInfo, Parent);
		PlausiValidation.CheckR170ab(Parent.JI_NonCustomsLawObligationInfo, Parent);
	}

	protected override void CheckJI_StorageType()
	{
		base.CheckJI_StorageType();
		PlausiValidation.CheckR156(Parent.JI_StorageTypeInfo, Parent);
		PlausiValidation.CheckR141(Parent.JI_StorageTypeInfo, Parent);
		PlausiValidation.CheckR142(Parent.JI_StorageTypeInfo, Parent);
		PlausiValidation.CheckR145a(Parent.JI_StorageTypeInfo, Parent);
		PlausiValidation.CheckR145b(Parent.JI_StorageTypeInfo, Parent);
		if (Declaration?.IsImport ?? false)
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_StorageTypeInfo);
		}
	}

	protected override void CheckJI_NonTradingGoods()
	{
		base.CheckJI_NonTradingGoods();
		PlausiValidation.CheckR325(Parent.JI_NonTradingGoodsInfo, Parent);
		PlausiValidation.CheckR285(Parent.JI_NonTradingGoodsInfo, Parent);
		PlausiValidation.CheckR198AndNP70167(Parent.JI_NonTradingGoodsInfo, Parent);
		PlausiValidation.CheckR261NonTradingGoods(Parent.JI_NonTradingGoodsInfo, Parent);
		PlausiValidation.CheckNP70000(Parent.JI_NonTradingGoodsInfo, Parent);
		PlausiValidation.CheckNP70169(Parent.JI_NonTradingGoodsInfo, Parent);
		PlausiValidation.CheckNS30003_Ticked(Parent.JI_NonTradingGoodsInfo, Parent.EntryInstruction, () => DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_NonTradingGoods)).Caption);
	}

	protected override void CheckJI_GoodsReturned()
	{
		base.CheckJI_GoodsReturned();
		PlausiValidation.CheckNP70066(Parent.JI_GoodsReturnedInfo, Parent);
		PlausiValidation.CheckNS30003_Ticked(Parent.JI_GoodsReturnedInfo, Parent.EntryInstruction);
	}

	protected override void CheckJI_CusNumber()
	{
		base.CheckJI_CusNumber();
		PlausiValidation.CheckNS30003_NotEmpty(Parent.JI_CusNumberInfo, Parent.EntryInstruction);
	}

	protected override void CheckJI_RateOverride()
	{
		base.CheckJI_RateOverride();
		PlausiValidation.CheckR174(Parent.JI_RateOverrideInfo, Parent);
		PlausiValidation.CheckR181(Parent.JI_RateOverrideInfo, Parent);
		PlausiValidation.CheckR191(Parent.JI_RateOverrideInfo, Parent);
		PlausiValidation.CheckR247(Parent.JI_RateOverrideInfo, Parent);
		PlausiValidation.CheckR249d(Parent.JI_RateOverrideInfo, Parent);
	}
}
