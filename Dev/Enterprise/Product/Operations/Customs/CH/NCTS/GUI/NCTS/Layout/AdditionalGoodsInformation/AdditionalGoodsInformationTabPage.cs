using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

sealed class AdditionalGoodsInformationTabPage : ITabPage
{
	public ResourceStringData Caption => Res.GetData("C793CD86-E528-4960-B4EF-3C4A6E5AB29D", "Additional Goods Information");

	public ZString UserControlBindingMember => ".";

	public ZUserControl CreateUserControl() => new AdditionalGoodsInformationUserControl();
}
