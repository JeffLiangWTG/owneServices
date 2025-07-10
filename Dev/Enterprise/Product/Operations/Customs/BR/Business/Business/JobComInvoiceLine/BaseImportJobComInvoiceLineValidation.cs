using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public abstract class BaseImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public BaseImportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsSecondUnitQtyInfo);
		}

		protected override void CheckJI_CustomsThirdUnitQty()
		{
			base.CheckJI_CustomsThirdUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsThirdUnitQtyInfo);
		}

		protected override void CheckMercosulForeignDeclarationType()
		{
			base.CheckMercosulForeignDeclarationType();
			ListValidation.MessageErrorIfInvalidCode(Parent.MercosulForeignDeclarationTypeInfo);
			if (Parent.MercosulForeignDeclarations.Any())
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.MercosulForeignDeclarationTypeInfo);
			}
		}

		protected override void CheckICMSFCPRateValue()
		{
			ValidationHelper.CheckValidPercentage(Parent.ICMSFCPRateValueInfo);
		}

		protected override void CheckComplementaryDescription()
		{
			base.CheckComplementaryDescription();
			if (Parent.ComplementaryDescription.IsEmpty && (Parent.InvoiceHeader?.JZ_IncoTerm ?? ZString.Empty) == BRIncoTermList.Codes.OCV)
			{
				Parent.ComplementaryDescriptionInfo.AddMessageError(Res.GetString("125ED12D-953A-4040-962F-D0DCAEA3DECA", "The Incoterm chosen is OCV - Other Condition of Sale, but you have not entered a Complement."));
			}
		}

		protected override void CheckJI_ManufacturerIndicator()
		{
			base.CheckJI_ManufacturerIndicator();
			if (!Parent.JI_ManufacturerIndicatorReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_ManufacturerIndicatorInfo);
			}
		}

		protected override void CheckJI_ICMSRate()
		{
			base.CheckJI_ICMSRate();
			if (Parent.IsAttachedToPersistentDeclaration)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JI_ICMSRateInfo);
			}
		}

		protected override void CheckJI_ICMSBaseValueReductionPercentage()
		{
			base.CheckJI_ICMSBaseValueReductionPercentage();
			if (!Parent.ICMSBaseValueReductionPercentageReadOnly)
			{
				CheckICMSReductionPercentage(Parent.JI_ICMSBaseValueReductionPercentageInfo);
				ValidationHelper.CheckValidPercentage(Parent.JI_ICMSBaseValueReductionPercentageInfo);
			}
		}

		protected override void CheckJI_ICMSTotalAmountReductionPercentage()
		{
			base.CheckJI_ICMSTotalAmountReductionPercentage();
			if (!Parent.ICMSTotalAmountReductionPercentageReadOnly)
			{
				CheckICMSReductionPercentage(Parent.JI_ICMSTotalAmountReductionPercentageInfo);
				ValidationHelper.CheckValidPercentage(Parent.JI_ICMSTotalAmountReductionPercentageInfo);
			}
		}

		protected override void CheckJI_ICMSFormula()
		{
			base.CheckJI_ICMSFormula();
			if (!Parent.JI_ICMSFormula_ReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_ICMSFormulaInfo);
			}
		}

		protected abstract bool IsGoodsApplicationMandatory { get; }

		protected override void CheckJI_GoodsApplication()
		{
			base.CheckJI_GoodsApplication();
			if (IsGoodsApplicationMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_GoodsApplicationInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_GoodsApplicationInfo);
		}

		protected override void CheckJI_GoodsCondition()
		{
			base.CheckJI_GoodsCondition();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_GoodsConditionInfo);
		}

		void CheckICMSReductionPercentage(ZPropertyInfo targetInfo)
		{
			if (Parent?.JI_ICMSBaseValueReductionPercentage == 0 && Parent?.JI_ICMSTotalAmountReductionPercentage == 0)
			{
				targetInfo.AddMessageError(Res.GetString("6982B966-90CE-407D-9F69-10B92BF86391", "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation."));
			}
		}
	}
}
