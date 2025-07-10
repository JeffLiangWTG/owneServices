using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business;

public class CusVehicleValidation : EU.Business.CusVehicleValidation
{
	public CusVehicleValidation(AutoCusVehicle parent) : base(parent)
	{
	}

	public new CusVehicle Parent => (CusVehicle)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateMaxVehicleCombinedFieldsLength();
	}

	void ValidateMaxVehicleCombinedFieldsLength()
	{
		const int maxVehicleCombinedFieldsLength = 40;

		var message = MaxVehicleCombinedLengthMessage;
		Parent.ClearRowNotificationsContaining(message);
		var combinedLength = Parent.CVH_VehicleIdentificationNumber.Length + Parent.CVH_BrandName.Length + Parent.CVH_ModelName.Length;
		if (IsTransitionPeriod && combinedLength > maxVehicleCombinedFieldsLength)
		{
			Parent.AddRowMessageError(message);
		}
	}

	bool IsTransitionPeriod => Parent.InvoiceLine?.Declaration?.IsTransitionPeriodAES30 ?? false;

	protected override void CheckCVH_VehicleIdentificationNumber()
	{
		base.CheckCVH_VehicleIdentificationNumber();
		var tarif = Parent.InvoiceLine.JI_Tariff;
		if (!tarif.IsEmpty && !tarif.StartsWith("87", StringComparison.CurrentCultureIgnoreCase) && !Parent.CVH_VehicleIdentificationNumber.IsEmpty)
		{
			Parent.CVH_VehicleIdentificationNumberInfo.AddWarning(Res.GetString("047CA67C-E69C-4B5A-8A65-BBB316AD7D44", "The VIN may not be required for Tariff: {0}", Parent.InvoiceLine?.JI_Tariff));
		}
	}

	protected override void CheckCVH_BrandName()
	{
		base.CheckCVH_BrandName();
		if (!Parent.CVH_VehicleIdentificationNumber.IsEmpty && Parent.CVH_BrandName.IsEmpty)
		{
			Parent.CVH_BrandNameInfo.AddMessageError(Res.GetString("AB507732-3322-40A1-8068-79F910F59DDF", "When declaring VIN, Brand must be declared"));
		}
	}

	protected override void CheckCVH_ModelName()
	{
		base.CheckCVH_ModelName();
		if (!Parent.CVH_VehicleIdentificationNumber.IsEmpty && Parent.CVH_ModelName.IsEmpty)
		{
			Parent.CVH_ModelNameInfo.AddMessageError(Res.GetString("F002D369-FECB-444B-8389-4B6A6DF1A43B", "When declaring VIN, Model must be declared"));
		}
	}

	ZString MaxVehicleCombinedLengthMessage => Res.GetString("559F64F5-4207-4E3A-9D8E-78BE29DA0348", "In provisional period, VIN+Brand+Model length could not be greater than 40.");
}
