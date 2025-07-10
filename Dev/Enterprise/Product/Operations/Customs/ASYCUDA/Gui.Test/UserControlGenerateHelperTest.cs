using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class UserControlGenerateHelperTest : TestCaseWithFactory
	{
		public void TestHeaderAdditionalTabPageForItenaryType()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "CIKONC", ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_Nature = "EXP";
			header.AMA_TransportMode = "SEA";

			using (var form = new ZForm(header))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var headerAdditionalTabPageUserControl = (ZUserControl)control.Controls.Find("mainTabControl_UserControl_VisitedPortsForManifestHeaderUserControl", true).First();
					AssertNotNull(headerAdditionalTabPageUserControl);
					AssertType(typeof(AsycudaManifestHeaderAdditionalTabPageUserControl), headerAdditionalTabPageUserControl);
					AssertEquals(1, headerAdditionalTabPageUserControl.Controls.Count);
					AssertEquals(System.Windows.Forms.DockStyle.Fill, headerAdditionalTabPageUserControl.Dock);
					AssertEquals(0, headerAdditionalTabPageUserControl.TabIndex);
					AssertEquals(true, headerAdditionalTabPageUserControl.AutoScroll);

					var headerAdditionalTabPage = (ZTabPage)control.Controls.Find("mainTabControl_TabPage_VisitedPortsForManifestHeaderUserControl", true).First();
					AssertNotNull(headerAdditionalTabPage);
					AssertEquals(true, headerAdditionalTabPage.TabVisible);
					AssertType(typeof(ZTabPage), headerAdditionalTabPage);
					AssertEquals(1, headerAdditionalTabPage.Controls.Count);
					AssertEquals(System.Windows.Forms.DockStyle.None, headerAdditionalTabPage.Dock);
					AssertEquals("Itinerary", headerAdditionalTabPage.CaptionResourceString.Caption);
				}
			}
		}
	}
}
