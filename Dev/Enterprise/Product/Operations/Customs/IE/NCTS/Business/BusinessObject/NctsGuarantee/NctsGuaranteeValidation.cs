using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class NctsGuaranteeValidation : EU.NCTS.Business.NctsGuaranteeValidation
	{
		public NctsGuaranteeValidation(NctsGuarantee parent) : base(parent)
		{
		}

		public new NctsGuarantee Parent => (NctsGuarantee)base.Parent;

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_BondTypeInfo);
		}
	}
}
