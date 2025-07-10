using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	public class CusTempStorageDecUserControlTest : TestCaseWithFactory
	{
		public void TestMessageStatusTextBoxCaption()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeFRC;
			header.CreateRelatedCusTempStorageDec();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<CINTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;

				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<CusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var statusTextBox = cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "StatusTextBox");
				AssertNotNull(statusTextBox);
				AssertEquals("Message Status", statusTextBox.CaptionResourceString.Caption);
			}
		}
	}
}
