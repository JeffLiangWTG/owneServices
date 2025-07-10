using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Business
{
	public class CusClassPartPivotValidation : AutoJPCusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(CusClassPartPivot parent)
			: base(parent)
		{
		}

		protected override void CheckCI_StorageType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_StorageTypeInfo);
		}

		protected override void CheckCI_DomesticConsumptionTaxExemptionCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_DomesticConsumptionTaxExemptionCodeInfo);
		}

		protected override void CheckCI_TradeControlOrderAppendix()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_TradeControlOrderAppendixInfo);
		}

		protected override void CheckCI_FEFTAArticle48()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_FEFTAArticle48Info);
		}
	}
}
