using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class AlternativeEvidenceTabPage : ITabPageWithVisibility<CusExitReport>
	{
		public Func<CusExitReport, bool> IsVisible => (x) => x != null && x.IsAlternativeEvidenceRequired;

		public ResourceStringData Caption => Res.GetData("D9499ECD-A698-4242-86FF-DF139CFFA530", "Alternative Evidence");

		public ZString UserControlBindingMember => nameof(CusExitReport.AlternativeEvidences);

		public ZUserControl CreateUserControl() => new AlternativeEvidenceUserControl();
	}
}
