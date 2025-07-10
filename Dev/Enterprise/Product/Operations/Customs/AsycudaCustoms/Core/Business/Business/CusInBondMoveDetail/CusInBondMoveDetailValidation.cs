namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondMoveDetailValidation : Customs.Business.CusInBondMoveDetailValidation
	{
		public CusInBondMoveDetailValidation(CusInBondMoveDetail parent)
			: base(parent)
		{
		}

		protected override bool IsBillNumberMandatory => false;
	}
}
