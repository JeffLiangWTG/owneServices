namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDAddInfoDeclarationValidation : AUAddInfoValidation
	{
		public EXDAddInfoDeclarationValidation(AUAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckZA_GoodsOwnerPartyIDHidden()
		{
			base.CheckZA_GoodsOwnerPartyIDHidden();
			JobDeclaration declaration = JobDeclaration;
			if (declaration != null)
			{
				declaration.Validation.ValidateJE_OH_Supplier();
			}
		}

		protected override void CheckZA_ConsigneeNameHidden()
		{
			base.CheckZA_ConsigneeNameHidden();
			JobDeclaration declaration = JobDeclaration;
			if (declaration != null)
			{
				declaration.Validation.ValidateJE_OH_Importer();
			}
		}

		protected override void CheckZA_ConsigneeCityHidden()
		{
			base.CheckZA_ConsigneeCityHidden();
			JobDeclaration declaration = JobDeclaration;
			if (declaration != null)
			{
				declaration.Validation.ValidateJE_OH_Importer();
			}
		}

		protected override void CheckZA_ImporterToOrder_Hidden()
		{
			base.CheckZA_ImporterToOrder_Hidden();
			ValidateZA_ImporterToOrderComment_Hidden();
		}

		protected override void CheckZA_ImporterToOrderComment_Hidden()
		{
			base.CheckZA_ImporterToOrderComment_Hidden();
			var declaration = JobDeclaration;
			if (declaration != null && declaration.IsQuarantine && Parent.ZA_ImporterToOrder_Hidden && Parent.ZA_ImporterToOrderComment_Hidden.IsEmpty)
			{
				Parent.ZA_ImporterToOrderComment_HiddenInfo.AddMessageError("City is required when 'To Order' box is checked.");
			}
		}
	}
}
