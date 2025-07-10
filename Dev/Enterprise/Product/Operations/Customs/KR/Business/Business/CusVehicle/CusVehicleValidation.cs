using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class CusVehicleValidation : Customs.Business.CusVehicleValidation
	{
		public CusVehicleValidation(AutoCusVehicle parent) : base(parent)
		{
		}

		protected override void CheckCVH_RN_NKCountryOfManufacture()
		{
			base.CheckCVH_RN_NKCountryOfManufacture();
			ListValidation.MessageErrorIfInvalidCode(Parent.CVH_RN_NKCountryOfManufactureInfo);
		}

		public new CusVehicle Parent => (CusVehicle)base.Parent;
	}
}
