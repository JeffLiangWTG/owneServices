using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.ExitControl.GUI
{
	public class ConsignmentAdditionalInformationTabPage : ITabPage
	{
		public ResourceStringData Caption => Res.GetData("9FA9EC92-FCC2-4B57-B0C0-8C924FD88BD5", "Additional Information");

		public ZUserControl CreateUserControl() => new ConsignmentAdditionalInformationTabUserControl();

		public ZString UserControlBindingMember => nameof(CusExitConsignment.AdditionalInfos);
	}
}
