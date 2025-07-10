using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusVehicleValidation : Customs.Business.CusVehicleValidation
	{
		public CusVehicleValidation(CusVehicle parent) : base(parent)
		{
		}

		protected override void CheckCVH_VehicleIdentificationNumber()
		{
			var vin = Parent.CVH_VehicleIdentificationNumber;
			if (!vin.IsEmpty && !VINHelper.IsValidVINWithTheNinthDigitCheck(vin))
			{
				Parent.CVH_VehicleIdentificationNumberInfo.AddWarning(Res.GetString("44FF289D-08C5-40AC-8F83-4CDE747058E9", "The VIN captured is invalid."));
			}
		}
	}
}
