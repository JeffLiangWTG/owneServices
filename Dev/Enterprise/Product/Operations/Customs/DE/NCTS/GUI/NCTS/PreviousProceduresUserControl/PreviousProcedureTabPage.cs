using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public class PreviousProcedureTabPage : ITabPage
	{
		public ResourceStringData Caption => Res.GetData("F5D281C4-2317-4F13-B59D-5C8CD35347DB", "Previous Procedures");

		public ZString UserControlBindingMember => ".";

		public ZUserControl CreateUserControl() => new NctsPreviousProceduresUserControl();
	}
}
