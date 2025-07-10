using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
{
	public AsycudaContainerValidation(AsycudaContainer parent) : base(parent)
	{
	}

	new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

	protected override void CheckACN_ContainerNumber()
	{
		base.CheckACN_ContainerNumber();

		var parent = Parent;
		var containerNumber = parent.ACN_ContainerNumber;
		var containers = parent.Header.Containers;

		if (containers.Where(x => x.ACN_ContainerNumber == containerNumber).Take(2).Count() > 1)
		{
			parent.ACN_ContainerNumberInfo.AddError(Res.GetString("9F618BAC-6469-4C21-A916-4E6841D415C3", "Duplicate Container Number – Please remove duplicate container."));
		}
	}
	protected override void CheckACN_GoodsWeight()
	{
		base.CheckACN_GoodsWeight();

		var parent = Parent;
		MandatoryValidation.MessageErrorIfNotEntered(parent.ACN_GoodsWeightInfo);
	}

	protected override void CheckACN_NumberOfPackages()
	{
		base.CheckACN_NumberOfPackages();

		var parent = Parent;
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(parent.ACN_NumberOfPackagesInfo, parent.ACN_EmptyFullIndicatorInfo,
			new IZType[] { (ZString)ASYCUDA.Business.EmptyFullIndicatorList.Codes.FullContainerLoad, (ZString)ASYCUDA.Business.EmptyFullIndicatorList.Codes.LessThanFullContainerLoad },
			Res.GetString("67C1D953-B35D-4E31-BA0A-C9E30E266A1C", "Packages cannot be zero for FCL or LCL Container loads"));
	}
	protected override void CheckACN_SetPointTemperatureUnit()
	{
		base.CheckACN_SetPointTemperatureUnit();
		var parent = Parent;
		if (!parent.ACN_SetPointTemperatureUnit.IsEmpty)
		{
			ListValidation.ErrorIfInvalidCode(parent.ACN_SetPointTemperatureUnitInfo, parent.Lookups.AEContainerTemperatureUnitCodes);
		}
	}

	protected override void CheckACN_Seal1()
	{
		base.CheckACN_Seal1();
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.ACN_Seal1Info, Parent.ACN_ContainerNumberInfo);
	}
}
