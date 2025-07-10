using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class DocumentsRequestedTabPage : ITabPage
	{
		public ResourceStringData Caption => Res.GetData("5426659C-D1A5-412E-8AB9-19B53BDE5FCD", "Documents Requested");

		public ZString UserControlBindingMember => ".";

		public ZUserControl CreateUserControl() => new RequestedDocumentsUserControl();
	}
}
