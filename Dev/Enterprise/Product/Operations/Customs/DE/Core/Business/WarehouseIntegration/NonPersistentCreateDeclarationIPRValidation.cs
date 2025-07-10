using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public sealed class NonPersistentCreateDeclarationIPRValidation : NonPersistentCreateDeclarationBizObjValidation
	{
		public NonPersistentCreateDeclarationIPRValidation(CreateDeclarationIPR parent) : base(parent) { }

		public new CreateDeclarationIPR Parent => (CreateDeclarationIPR)base.Parent;

		protected override void CheckCustomsOffice()
		{
			if (Parent.DeclarationType.Equals(ImportDeclarationTypeList.Codes.AVABR))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CustomsOfficeInfo);
			}
			else
			{
				base.CheckCustomsOffice();
			}
		}

		protected override void CheckDeclarantsReference()
		{
			if (!Parent.DeclarationType.Equals(ImportDeclarationTypeList.Codes.AVABR))
			{
				base.CheckDeclarantsReference();
			}
		}

		protected override void CheckCustomsDeadline()
		{
			if (Parent.DeclarationType.Equals(ImportDeclarationTypeList.Codes.AVABR))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CustomsDeadlineInfo);
			}
		}
	}
}
