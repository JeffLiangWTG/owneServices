using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CusVehicleValidation : Customs.Business.CusVehicleValidation
{
	public CusVehicleValidation(AutoCusVehicle parent) : base(parent)
	{
	}

	public new CusVehicle Parent => (CusVehicle)base.Parent;

	protected override void CheckCVH_ModelName()
	{
		base.CheckCVH_ModelName();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CVH_ModelNameInfo);
	}

	protected override void CheckCVH_VehicleIdentificationNumber()
	{
		var vin = Parent.CVH_VehicleIdentificationNumber;
		if (!vin.IsEmpty && vin.Length != 17)
		{
			Parent.CVH_VehicleIdentificationNumberInfo.AddMessageError(Res.GetString("4733C760-29D5-43ED-8324-9E1903305AAD", "VIN must be 17 characters."));
		}
		else
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_VehicleIdentificationNumberInfo);
		}
	}

	protected override void CheckCVH_RegistrationNumber()
	{
		var registrationNumber = Parent.CVH_RegistrationNumber;
		if (!registrationNumber.IsEmpty)
		{
			if (registrationNumber.Length == 9 && registrationNumber.IsNumbersOnlyOrEmpty)
			{
				if (!IsValidRegistrationNumber(registrationNumber))
				{
					Parent.CVH_RegistrationNumberInfo.AddMessageError(Res.GetString("50E77985-8517-435B-91E1-95A789A28CA4", "Registration number is invalid."));
				}
			}
			else
			{
				Parent.CVH_RegistrationNumberInfo.AddMessageError(Res.GetString("DA1EDBF5-565F-4142-A06B-6568C4EB49E6", "Registration number must be 9 digits."));
			}
		}
	}

	bool IsValidRegistrationNumber(string registrationNumber)
	{
		var factor = new int[] { 3, 2, 7, 6, 5, 4, 3, 2 };
		var registrationNumberArray = Array.ConvertAll(registrationNumber.ToCharArray(), c => (int)Char.GetNumericValue(c));

		var sum = 0;
		for (var i = 0; i < factor.Length; i++)
		{
			sum += registrationNumberArray[i] * factor[i];
		}

		return sum % 11 == registrationNumberArray.Last();
	}
}
