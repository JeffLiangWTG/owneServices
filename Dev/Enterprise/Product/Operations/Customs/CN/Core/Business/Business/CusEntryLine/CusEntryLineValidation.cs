namespace Enterprise.Customs.CN.Business
{
	public class CusEntryLineValidation : Customs.Business.CusEntryLineValidation
	{
		public CusEntryLineValidation(CusEntryLine parent)
			: base(parent)
		{
		}
		public CusEntryLine EntryLine => Parent;

		protected new CusEntryLine Parent => (CusEntryLine)base.Parent;

		internal IValidationModeProvider ValidationModeProvider => Parent.Declaration as JobDeclaration;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGoodsSpecModel();
		}

		public void ValidateGoodsSpecModel()
		{
			ValidateCalculatedProperty(Parent.GoodsSpecModelInfo);
		}

		protected void CheckGoodsSpecModel()
		{
			ValidationHelper.CheckMaxLength(Parent.GoodsSpecModelInfo, AdditionalInformationHelper.GoodsSpecModelMaxLength, ValidationModeProvider);
		}
	}
}
