using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

sealed class RestrictionsTabPage : ITabPage
{
	public ResourceStringData Caption => Res.GetData("C3C353D3-EE9B-404F-B6AB-3D64361BE837", "Restrictions");

	public ZString UserControlBindingMember => (NoResString)"Restrictions";

	public ZUserControl CreateUserControl()
	{
		var control = new RestrictionsUserControl();
		control.PermitOwnerDocAddressControl.BindToOrganisations = $"{nameof(NctsHeader.Bills)}.{nameof(NctsBill.GoodsItems)}.{nameof(NctsDepartureCargoDesc.Restrictions)}.{nameof(Restriction.Lookups)}.{nameof(RestrictionLookups.PermitOwnerList)}";
		return control;
	}
}
