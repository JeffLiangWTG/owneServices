using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class SupplementaryCodeValidation : EU.Business.SupplementaryCodeValidation
{
	public SupplementaryCodeValidation(BaseSupplementaryCode parent) : base(parent)
	{
	}

	protected override void CheckCY_Code()
	{
		base.CheckCY_Code();

		var supplementaryCodeSupporter = Parent.SupplementaryCodeSupporter;
		if (supplementaryCodeSupporter != null)
		{
			var invoiceLine = (supplementaryCodeSupporter as JobComInvoiceLine);

			if (Parent.IsQVatAdditionalCode())
			{
				CheckNoMoreThanOneVatQAdditionalCodeIsPresent(supplementaryCodeSupporter);

				if (invoiceLine != null)
				{
					CheckQVatAdditionalCodeIsValidForSelectedTariff(invoiceLine);
				}
			}

			if (invoiceLine != null)
			{
				CheckSupplementaryCodeIsInTheList();
				CheckSupplementaryCodeIsFirstCharacterValid();
			}
		}
	}

	void CheckQVatAdditionalCodeIsValidForSelectedTariff(JobComInvoiceLine invoiceLine)
	{
		var tariffRelatedAdditionalCodes = invoiceLine.GetEffectiveSupplementaryCodesRelatedToVATApplicabilities();
		if (!tariffRelatedAdditionalCodes.Contains(Parent.CY_Code))
		{
			Parent.CY_CodeInfo.AddMessageError(ValidationCaptions.SupplementaryCode.SetVatAdditionalCodeIsNotValidForThisTaricCode);
		}
	}

	void CheckNoMoreThanOneVatQAdditionalCodeIsPresent(ISupplementaryCodeSupporter supplementaryCodeSupporter)
	{
		if (supplementaryCodeSupporter.HasMoreThanOneVatQVatAdditionalCode())
		{
			Parent.CY_CodeInfo.AddMessageError(ValidationCaptions.SupplementaryCode.OnlyOneVatAdditionalCodeCanBeUsedAtTime);
		}
	}

	void CheckSupplementaryCodeIsInTheList()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
	}

	void CheckSupplementaryCodeIsFirstCharacterValid()
	{
		var parent = Parent;
		var code = parent.CY_Code;

		if (!code.IsSupplementaryCodeValid())
		{
			parent.CY_CodeInfo.AddMessageError(ValidationCaptions.SupplementaryCode.FirstCharacterOfTheSelectedCodeIsNotValid);
		}
	}
}
