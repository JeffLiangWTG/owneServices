using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ReportAdditionalDocumentsTabPageTest : TestCaseWithFactory
	{
		public void TestCaption()
		{
			AssertEquals("Additional Documents", reportTransportDocumentsTabPage.Caption.Caption);
		}

		public void TestUserControlType()
		{
			using (var userControl = reportTransportDocumentsTabPage.CreateUserControl())
			{
				AssertType<ReportAdditionalDocumentsTabUserControl>(userControl);
			}
		}

		public void TestUserControlBindingMember()
		{
			AssertEquals(nameof(CusExitReport.AdditionalInfos), reportTransportDocumentsTabPage.UserControlBindingMember);
		}

		public void TestIsVisible()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Null", false, reportTransportDocumentsTabPage.IsVisible(null));

				var exitReport = Factory.New<CusExitReport>();
				exitReport.CER_Type = ExitReportTypeList.Codes.Presentation;
				AssertEquals("CER_Type = 'PRE'", true, reportTransportDocumentsTabPage.IsVisible(exitReport));

				exitReport.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
				AssertEquals("CER_Type = 'ALT'", false, reportTransportDocumentsTabPage.IsVisible(exitReport));

				exitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
				AssertEquals("CER_Type <> 'PRE' or 'ALT'", false, reportTransportDocumentsTabPage.IsVisible(exitReport));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			reportTransportDocumentsTabPage = new ReportAdditionalDocumentsTabPage();
		}

		ReportAdditionalDocumentsTabPage reportTransportDocumentsTabPage;
	}
}
