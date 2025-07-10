using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.MX.Business
{
	public class CusVehicleValidation : Customs.Business.CusVehicleValidation
	{
		public CusVehicleValidation(AutoCusVehicle parent) : base(parent)
		{
		}

		protected override void CheckCVH_SerialNumber()
		{
			base.CheckCVH_SerialNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_SerialNumberInfo);
		}

		protected override void CheckCVH_Mileage()
		{
			base.CheckCVH_Mileage();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_MileageInfo);
		}
	}
}
