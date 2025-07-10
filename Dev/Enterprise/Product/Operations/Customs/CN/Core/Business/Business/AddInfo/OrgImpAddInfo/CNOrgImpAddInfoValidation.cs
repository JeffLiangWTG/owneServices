using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class CNOrgImpAddInfoValidation : AutoCNOrgImpAddInfoValidation
	{
		public CNOrgImpAddInfoValidation(AutoCNOrgImpAddInfo parent) : base(parent)
		{
		}

		protected override void CheckZO_MessageSubType()
		{
			base.CheckZO_MessageSubType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_MessageSubTypeInfo);
		}

		protected override void CheckZO_IntelligentDeclarationType()
		{
			base.CheckZO_IntelligentDeclarationType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_IntelligentDeclarationTypeInfo);
		}
	}
}
