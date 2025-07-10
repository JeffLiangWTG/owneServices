using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CommonGuaranteeValidation : CusBondDetailValidation
	{
		public CommonGuaranteeValidation(CommonGuarantee parent)
		: base(parent)
		{
		}
		protected new CommonGuarantee Parent => (CommonGuarantee)base.Parent;

		protected override void CheckPW_BondFiledPort()
		{
			base.CheckPW_BondFiledPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.PW_BondFiledPortInfo);
		}

		protected override void CheckPW_ValidityLimitation()
		{
			base.CheckPW_ValidityLimitation();
			ListValidation.MessageErrorIfInvalidCode(Parent.PW_ValidityLimitationInfo);
		}

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();
			ListValidation.MessageErrorIfInvalidCode(Parent.PW_BondTypeInfo);
		}
	}
}

