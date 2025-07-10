using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Business;

public class CusVehicleLookups : Customs.Business.CusVehicleLookups
{
	public CusVehicleLookups(CusVehicle parent)
		: base(parent)
	{
	}

	protected new CusVehicle Parent => (CusVehicle)base.Parent;

	public ZZRefCusCodeListCombinedCollection VehicleBrandList => GetVehicleBrandList();

	ZZRefCusCodeListCombinedCollection GetVehicleBrandList()
	{
		return Parent.Declaration == null ? new ZZRefCusCodeListCombinedCollection(Factory) : ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Parent.Declaration.GetDefaultDataGroupingCode(), AEConstants.RefCusCodeList.CodeTypes.VehicleBrand, Parent.Declaration.DateOfValuation);
	}

	public ZZRefCusCodeListCombinedCollection CarTypeList => GetCarTypeList();

	ZZRefCusCodeListCombinedCollection GetCarTypeList()
	{
		return Parent.Declaration == null ? new ZZRefCusCodeListCombinedCollection(Factory) : ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Parent.Declaration.GetDefaultDataGroupingCode(), AEConstants.RefCusCodeList.CodeTypes.VehicleType, Parent.Declaration.DateOfValuation);
	}

	public ZZRefCusCodeListCombinedCollection PayloadUQList => GetPayloadUQList();

	ZZRefCusCodeListCombinedCollection GetPayloadUQList()
	{
		var date = Parent.Declaration?.DateOfValuation ?? ZDateTime.Today;
		return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, date);
	}

	public CodeDescriptionPairList DriveSideList => Factory.GetCachedValue<DriveSideList>();

	public CodeDescriptionPairList SpecificationStandardList => Factory.GetCachedValue<SpecificationStandardList>();
}
