using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusGuaranteeHeaderValidation : Customs.Business.CusGuaranteeHeaderValidation
	{
		public CusGuaranteeHeaderValidation(CusGuaranteeHeader parent)
			: base(parent)
		{
		}

		public new CusGuaranteeHeader Parent => (CusGuaranteeHeader)base.Parent;

		protected override void CheckCPH_UnitOfMeasure()
		{
			base.CheckCPH_UnitOfMeasure();

			if (!Parent.CPH_UnitOfMeasure_ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.CPH_UnitOfMeasureInfo);
				ListValidation.ErrorIfInvalidCode(Parent.CPH_UnitOfMeasureInfo);
			}
		}

		protected override void CheckCPH_Number()
		{
			base.CheckCPH_Number();

			if (ApplyRuleTR0301)
			{
				var parent = Parent;
				ValidationExtendMethods.RuleTR301(parent.CPH_SubType, parent.CPH_Number, parent.CPH_NumberInfo);
			}
		}

		protected virtual bool ApplyRuleTR0301 => Parent.CPH_Type == PermitTransactionTypeList.Codes.TRA;
	}
}
