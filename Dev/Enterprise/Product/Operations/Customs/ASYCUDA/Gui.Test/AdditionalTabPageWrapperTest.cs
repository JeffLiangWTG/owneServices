using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AdditionalTabPageWrapperTest : TestCaseWithFactory
	{
		public void TestHasHeaderAdditionalTabPageType()
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
					var headerAdditionalTabPageUserControl = (ZUserControl)control.Controls.Find("mainTabControl_UserControl_VisitedPortsForManifestHeaderUserControl", true).FirstOrDefault();
					AssertNotNull(headerAdditionalTabPageUserControl);

					var headerAdditionalTabPage = (ZTabPage)control.Controls.Find("mainTabControl_TabPage_VisitedPortsForManifestHeaderUserControl", true).FirstOrDefault();
					AssertNotNull(headerAdditionalTabPage);
				}
			}
		}
		public void TestNoHeaderAdditionalTabPageType()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "CIKONC", ApplicationCodeTypeList.Codes.Consolidator);
			header.AMA_Nature = "EXP";
			header.AMA_TransportMode = "SEA";

			using (var form = new ZForm(header))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var headerAdditionalTabPageUserControl = (ZUserControl)control.Controls.Find("mainTabControl_UserControl_VisitedPortsForManifestHeaderUserControl", true).FirstOrDefault();
					AssertNull(headerAdditionalTabPageUserControl);

					var headerAdditionalTabPage = (ZTabPage)control.Controls.Find("mainTabControl_TabPage_VisitedPortsForManifestHeaderUserControl", true).FirstOrDefault();
					AssertNull(headerAdditionalTabPage);
				}
			}
		}
	}
}
