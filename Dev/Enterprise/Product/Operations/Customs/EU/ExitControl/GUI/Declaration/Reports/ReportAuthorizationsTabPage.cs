using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public sealed class ReportAuthorizationsTabPage : ITabPageWithVisibility<CusExitReport>, ITabPage
{
	public ResourceStringData Caption => Res.GetData("9DE90F61-C211-4C88-A222-81A9C11B302B", "Authorizations");

	public ZUserControl CreateUserControl() => new ReportAuthorizationsTabUserControl();

	public ZString UserControlBindingMember => nameof(CusExitReport.CusAuthorizationUsages);

	public Func<CusExitReport, bool> IsVisible => (x) => x is not null;
}
