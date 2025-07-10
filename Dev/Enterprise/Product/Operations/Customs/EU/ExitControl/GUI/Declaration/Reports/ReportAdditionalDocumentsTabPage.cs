using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class ReportAdditionalDocumentsTabPage : ITabPageWithVisibility<CusExitReport>
	{
		public ResourceStringData Caption => Res.GetData("6E4A1720-363F-496C-8D94-30AD0B541652", "Additional Documents");

		public ZUserControl CreateUserControl() => new ReportAdditionalDocumentsTabUserControl();

		public ZString UserControlBindingMember => nameof(CusExitReport.AdditionalInfos);

		public Func<CusExitReport, bool> IsVisible => (x) => x != null && x.IsAdditionalInfosRequiredForReportAndItems;
	}
}
