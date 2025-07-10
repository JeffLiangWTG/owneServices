using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class CusVehicleValidation : Customs.Business.CusVehicleValidation
{
	public CusVehicleValidation(AutoCusVehicle parent) : base(parent)
	{
	}

	public new CusVehicle Parent => (CusVehicle)base.Parent;

	protected override void CheckCVH_VehicleIdentificationNumber()
	{
		base.CheckCVH_VehicleIdentificationNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_VehicleIdentificationNumberInfo);
	}

	protected override void CheckCVH_BrandName()
	{
		base.CheckCVH_BrandName();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CVH_BrandNameInfo, Parent.Lookups.VehicleBrandList);
	}

	protected override void CheckCVH_CarType()
	{
		base.CheckCVH_CarType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CVH_CarTypeInfo, Parent.Lookups.CarTypeList);
	}

	protected override void CheckCVH_Color()
	{
		base.CheckCVH_Color();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_ColorInfo);
	}

	protected override void CheckCVH_DriveSide()
	{
		base.CheckCVH_DriveSide();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CVH_DriveSideInfo, Parent.Lookups.DriveSideList);
	}

	protected override void CheckCVH_ModelName()
	{
		base.CheckCVH_ModelName();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_ModelNameInfo);
	}

	protected override void CheckCVH_ModelYear()
	{
		base.CheckCVH_ModelYear();
		var targetInfo = Parent.CVH_ModelYearInfo;
		MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

		if (!Regex.IsMatch(Parent.CVH_ModelYear, @"^[12][0-9]{3}$", RegexOptions.IgnoreCase))
		{
			targetInfo.AddMessageError(Res.GetString("a8fc2bf8-10c1-4bfb-9d98-0b510df8c24b", "Please enter a valid Model Year."));
		}
	}

	protected override void CheckCVH_Payload()
	{
		base.CheckCVH_Payload();
		MandatoryValidation.MessageErrorIfIsNegative(Parent.CVH_PayloadInfo);
	}

	protected override void CheckCVH_PayloadUQ()
	{
		base.CheckCVH_PayloadUQ();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CVH_PayloadUQInfo, Parent.Lookups.PayloadUQList);
	}

	protected override void CheckCVH_SpecificationStandard()
	{
		base.CheckCVH_SpecificationStandard();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CVH_SpecificationStandardInfo, Parent.Lookups.SpecificationStandardList);
	}
}
