using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class NonPersistentCreateDeclarationBizObjValidation : AutoNonPersistentCreateDeclarationBizObjValidation
	{
		public NonPersistentCreateDeclarationBizObjValidation(AutoNonPersistentCreateDeclarationBizObj parent) : base(parent) { }

		public new CreateDeclarationBizObj Parent => (CreateDeclarationBizObj)base.Parent;

		protected override void CheckCPC()
		{
			base.CheckCPC();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CPCInfo);
		}

		protected override void CheckCustomsOffice()
		{
			base.CheckCustomsOffice();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CustomsOfficeInfo);
		}

		protected override void CheckDeclarantsReference()
		{
			base.CheckDeclarantsReference();
			if (!Parent.CreateFromWarehouseOrder)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DeclarantsReferenceInfo);
			}
		}

		protected override void CheckDeclarationType()
		{
			base.CheckDeclarationType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DeclarationTypeInfo);
		}
	}
}
