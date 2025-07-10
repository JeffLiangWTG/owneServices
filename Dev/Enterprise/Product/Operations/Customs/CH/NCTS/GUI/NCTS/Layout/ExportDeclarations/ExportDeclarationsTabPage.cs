using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

sealed class ExportDeclarationsTabPage : ITabPage
{
	public ResourceStringData Caption => Res.GetData("618AD003-6B20-454C-B932-C8841247DEB1", "Export Declarations");

	public ZString UserControlBindingMember => ".";

	public ZUserControl CreateUserControl() => new ExportDeclarationsUserControl();
}
