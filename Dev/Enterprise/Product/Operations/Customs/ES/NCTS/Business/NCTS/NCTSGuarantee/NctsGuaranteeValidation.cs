namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsGuaranteeValidation : EU.NCTS.Business.NctsGuaranteeValidation
	{
		public NctsGuaranteeValidation(NctsGuarantee nctsGuarantee) : base(nctsGuarantee)
		{
		}

		protected override bool ApplyRuleC086 => false;
	}
}
