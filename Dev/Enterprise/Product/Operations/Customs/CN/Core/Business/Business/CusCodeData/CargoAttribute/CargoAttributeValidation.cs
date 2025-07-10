using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CargoAttributeValidation : CusCodeDataValidation
	{
		public CargoAttributeValidation(AutoCusCodeData parent) : base(parent) { }

		protected override void CheckCY_Code() { }
	}
}
