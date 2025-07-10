using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IN.Business;

public class SWControlLookups : CusSupportingInfoLookups
{
	public SWControlLookups(AutoCusSupportingInfo parent) : base(parent)
	{
	}

	public ZZRefCusCodeListCombinedCollection ControlResultCodeList =>
		ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.India,Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SingleWindowControl, Parent?.DateOfValuation ?? ZDateTime.Today);

	public new SWControl Parent => (SWControl)base.Parent;
}
