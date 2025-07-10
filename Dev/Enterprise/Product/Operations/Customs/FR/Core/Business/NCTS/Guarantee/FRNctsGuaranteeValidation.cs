namespace Enterprise.Customs.FR.Business.NCTS
{
	public class FRNctsGuaranteeValidation : EU.NCTS.Business.NctsGuaranteeValidation
	{
		public FRNctsGuaranteeValidation(FRNctsGuarantee cusBondDetail)
			: base(cusBondDetail)
		{
		}

		protected override bool ApplyRuleTR0301 => Parent.CusGuarantee == null;
	}
}
