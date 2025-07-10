using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ConsignmentAuthorisationsTabPage : ITabPage
	{
		public ResourceStringData Caption => Res.GetData("098C7A05-CDC6-4E5F-B710-9C876DE2F333", "Authorizations");

		public ZUserControl CreateUserControl() => new ConsignmentAuthorisationsTabUserControl();

		public ZString UserControlBindingMember => nameof(CusExitConsignment.CusAuthorizationUsages);
	}
}
