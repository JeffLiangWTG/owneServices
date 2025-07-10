using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class AlternativeEvidenceTabPageTest : TestCaseWithFactory
	{
		public void TestCaption()
		{
			AssertEquals("Alternative Evidence", tabPage.Caption.Caption);
		}

		public void TestUserControlBindingMember()
		{
			AssertEquals(nameof(CusExitReport.AlternativeEvidences), tabPage.UserControlBindingMember);
		}

		public void TestCreateUserControl()
		{
			using (var userControl = tabPage.CreateUserControl())
			{
				AssertType<AlternativeEvidenceUserControl>(userControl);
			}
		}

		public void TestIsVisible()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Null", false, tabPage.IsVisible(null));

				var exitReport = Factory.New<CusExitReportForTest>();
				exitReport.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
				AssertEquals("CER_Type = 'ALT'", true, tabPage.IsVisible(exitReport));

				exitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
				AssertEquals("CER_Type <> 'ALT'", false, tabPage.IsVisible(exitReport));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			tabPage = new AlternativeEvidenceTabPage();
		}
		AlternativeEvidenceTabPage tabPage;

		sealed class CusExitReportForTest : CusExitReport
		{
			public CusExitReportForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool IsAlternativeEvidenceRequiredCore => true;
		}
	}
}
