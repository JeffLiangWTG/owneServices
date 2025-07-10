using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class CompletionCustomsOfficeValidation : Customs.Business.CusCodeDataValidation
	{
		public CompletionCustomsOfficeValidation(CompletionCustomsOffice parent) : base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_DataInfo);
		}

		protected new CompletionCustomsOffice Parent => (CompletionCustomsOffice)base.Parent;
	}
}
