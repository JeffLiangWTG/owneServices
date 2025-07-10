using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

class NctsPackagePhase4Validation : EU.NCTS.Business.NctsPackagePhase4Validation
{
	internal NctsPackagePhase4Validation(NctsPackage parent) : base(parent)
	{
	}

	new NctsPackage Parent => (NctsPackage)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();

		CheckNumberOfGoodsItemPackageLines();
	}

	protected override void CheckB5_UnitType()
	{
		base.CheckB5_UnitType();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_UnitTypeInfo);
	}

	protected override void CheckB5_UnitCount()
	{
		var currentPackage = Parent;
		var unitCountInfo = currentPackage.B5_UnitCountInfo;
		var unitCount = currentPackage.B5_UnitCount;

		MandatoryValidation.MessageErrorIfIsNegative(unitCountInfo);

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
			CheckUnitCountMustBeZero(unitCountInfo, unitCount);
		}
		else if (unitCount.IsEmpty)
		{
			CheckUnitCountWhenIsEmpty(currentPackage, unitCountInfo);
		}
	}

	void CheckUnitCountWhenIsEmpty(NctsPackage currentPackage, ZPropertyInfo unitCountInfo)
	{
		if (currentPackage.IsUnpacked)
		{
			MandatoryValidation.MessageErrorIfIsZero(unitCountInfo);
		}
	}

	void CheckUnitCountMustBeZero(ZPropertyInfo unitCountInfo, ZLong unitCount)
	{
		if (unitCount > 0)
		{
			unitCountInfo.AddMessageError(ValidationCaptions.NctsPackage.UnitCountMustBeEmpty);
		}
	}

	void CheckNumberOfGoodsItemPackageLines()
	{
		var onlyOnePackageIsAllowedInEtError = ValidationCaptions.NctsPackage.OnlyOnePackageIsAllowedInEt;

		Parent.RemoveRowError(onlyOnePackageIsAllowedInEtError);
		if (Parent.Parent.Packages.Count > 1)
		{
			Parent.AddRowError(onlyOnePackageIsAllowedInEtError);
		}
	}

	#endregion
}
