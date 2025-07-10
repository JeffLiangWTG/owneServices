using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public class NonStandardExchangeRateValidation : CusSupportingInfoValidation
{
	public NonStandardExchangeRateValidation(NonStandardExchangeRate parent) : base(parent)
	{
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		if (IsImport)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
		}
	}

	protected override void CheckCSI_EffectiveDate()
	{
		base.CheckCSI_EffectiveDate();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_EffectiveDateInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		if (IsImport)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}
	}

	protected override void CheckCSI_DateOfIssue()
	{
		base.CheckCSI_DateOfIssue();
		if (IsImport)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DateOfIssueInfo);
		}
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo);
	}

	protected override void CheckCSI_RX_NKCurrency()
	{
		base.CheckCSI_RX_NKCurrency();
		if (Parent.Parent is JobDeclaration declaration
			&& declaration.NonStandardExchangeRates.Cast<NonStandardExchangeRate>().Any(x => x.CSI_RX_NKCurrency == Parent.CSI_RX_NKCurrency && x.PK != Parent.PK))
		{
			Parent.CSI_RX_NKCurrencyInfo.AddError(Res.GetString("14517032-D7D5-4102-8B14-D9906C222F44", "Currency {0} is already present", Parent.CSI_RX_NKCurrency));
		}
	}

	bool IsImport => Parent?.Parent?.IsImport ?? false;

	protected new NonStandardExchangeRate Parent => (NonStandardExchangeRate)base.Parent;
}
