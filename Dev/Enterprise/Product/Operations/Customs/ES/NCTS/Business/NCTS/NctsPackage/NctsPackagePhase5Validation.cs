using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business;

public sealed class NctsPackagePhase5Validation : EU.NCTS.Business.NctsPackagePhase5Validation
{
	public NctsPackagePhase5Validation(NctsPackage parent) : base(parent)
	{
	}

	public new NctsPackage Parent => (NctsPackage)base.Parent;

	protected override void CheckB5_PackageID()
	{
		base.CheckB5_PackageID();

		if (IsDepartureAndHasVehicles || IsArrivalWithTypeFR)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_PackageIDInfo, Res.GetString("1E088561-3F3E-4C0F-BE40-413C53BEFC9A", "VIN"));
		}
	}
	protected override void CheckB5_Brand()
	{
		base.CheckB5_Brand();

		if (IsDepartureAndHasVehicles || IsArrivalWithTypeFR)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_BrandInfo, Res.GetString("5A76FE3C-07F7-4264-AF2F-09625477C65D", "Brand"));
		}
	}

	protected override void CheckB5_Model()
	{
		base.CheckB5_Model();

		if (IsDepartureAndHasVehicles || IsArrivalWithTypeFR)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_ModelInfo, Res.GetString("779B542D-FEA9-4C9E-B884-BF9E60FC4C63", "Model"));
		}
	}

	protected override void CheckB5_UnitType()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.B5_UnitTypeInfo);
		if (!Parent.IsArrivalMovement && !((NctsDepartureCargoDesc)Parent.Parent).IsVehicles)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_UnitTypeInfo);
		}
	}

	protected override void CheckB5_UnitCount()
	{
		base.CheckB5_UnitCount();
		var currentPackage = Parent;
		var unitCountInfo = currentPackage.B5_UnitCountInfo;
		var unitCount = currentPackage.B5_UnitCount;

		MandatoryValidation.CheckNotNegative(unitCountInfo);
		if (!currentPackage.B5_UnitType.IsEmpty)
		{
			CheckUnitCountRelatedToUnitType(currentPackage, unitCountInfo, unitCount);
		}
	}

	#region Implementation

	void CheckUnitCountRelatedToUnitType(NctsPackage currentPackage, ZPropertyInfo unitCountInfo, ZLong unitCount)
	{
		if (currentPackage.IsBulk)
		{
			CheckUnitCountMustBeZero(currentPackage, unitCountInfo, unitCount);
		}
	}

	void CheckUnitCountMustBeZero(NctsPackage currentPackage, ZPropertyInfo unitCountInfo, ZLong unitCount)
	{
		if (unitCount > 0)
		{
			unitCountInfo.AddMessageError(Res.GetString("BE7F2892-E210-4712-9CD0-3ED80D6B0888", "For the selected unit type, Unit count must be zero."));
		}
	}

	#endregion

	protected override void CheckB5_MarksAndNumbers()
	{
		CheckB5_MarksAndNumbers_Mandatory();
	}

	protected override void CheckB5_TypeOfDifference()
	{
		if (!EU.NCTS.Business.NctsHelper.UnloadedStateInitiallyNew(Parent.B5_TypeOfDifferenceInfo) && Parent.B5_B5_ParentPackage.IsEmpty)
		{
			ListValidation.ErrorIfInvalidCode(Parent.B5_TypeOfDifferenceInfo);
		}
	}

	protected override bool ShouldCheckMandatoryMarksAndNumbers => base.ShouldCheckMandatoryMarksAndNumbers && !IsArrivalWithTypeFR;

	bool IsArrivalWithTypeFR => Parent.IsArrivalMovement && Parent.B5_UnitType == UniversalReferenceConstants.RefCusCodeList.PackageType.Frame;

	bool IsDepartureAndHasVehicles => !Parent.IsArrivalMovement && ((NctsDepartureCargoDesc)Parent.Parent).IsVehicles;
}
