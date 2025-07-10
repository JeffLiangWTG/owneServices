using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class CusVehicleLookups : Customs.Business.CusVehicleLookups
{
	public CusVehicleLookups(AutoCusVehicle parent) : base(parent)
	{
	}

	new CusVehicle Parent => (CusVehicle)base.Parent;

	public ZZRefCusCodeListCombinedCollection ModelNameCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.VehiclesMarks, EffectiveAssessmentDate);

	ZDateTime EffectiveAssessmentDate => Parent?.InvoiceLine?.EffectiveAssessmentDate ?? ZDate.Today;
}
