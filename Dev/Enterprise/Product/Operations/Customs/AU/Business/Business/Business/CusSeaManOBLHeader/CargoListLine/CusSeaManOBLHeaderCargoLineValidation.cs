using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderCargoLineValidation : BaseCusSeaManOBLHeaderValidation
	{
		public CusSeaManOBLHeaderCargoLineValidation(CusSeaManOBLHeaderCargoLine parent)
			: base(parent)
		{
		}

		protected override void CheckBO_HeaderCargoType()
		{
			base.CheckBO_HeaderCargoType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BO_HeaderCargoTypeInfo);
		}

		protected new CusSeaManOBLHeaderCargoLine Parent
		{
			get { return (CusSeaManOBLHeaderCargoLine)base.Parent; }
		}
	}
}
