using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class WarehouseTabPage : ITabPage
	{
		public ResourceStringData Caption => Res.GetData("010E8B57-716A-4929-88B0-FFC5B1A44720", "Warehouse");

		public ZString UserControlBindingMember => ".";

		public ZUserControl CreateUserControl() => new Phase5GoodsItemWarehouseTabUserControl();
	}
}
