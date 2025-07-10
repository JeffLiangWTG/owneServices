using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Business
{
	public class CusLineTariffDetailValidation : AutoJPCusLineTariffDetailValidation
	{
		public CusLineTariffDetailValidation(CusLineTariffDetail parent) : base(parent)
		{
		}

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

		protected override void CheckBZ_Type()
		{
			base.CheckBZ_Type();
			ListValidation.MessageErrorIfInvalidCode(Parent.BZ_TypeInfo);
		}

		protected override void CheckBZ_Tariff()
		{
			base.CheckBZ_Tariff();
			ListValidation.MessageErrorIfInvalidCode(Parent.BZ_TariffInfo);
		}

		protected override void CheckBZ_ExemptionReductionCode()
		{
			base.CheckBZ_ExemptionReductionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BZ_ExemptionReductionCodeInfo);
		}
	}
}
