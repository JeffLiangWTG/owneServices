using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaManifestAdditionalTabPageUserControlTest : TestCaseWithFactory
	{
		public void TestVisibilityOfAdditionalTab()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var manifest = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "CIKONC", ApplicationCodeTypeList.Codes.ShippingLine);
				manifest.AMA_Nature = "EXP";
				manifest.AMA_TransportMode = "SEA";

				using (var form = new ManifestForm(manifest))
				{
					form.Show();
					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var asycudaManifestAdditionalTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabControl_TabPage_VisitedPortsForManifestHeaderUserControl");
					AssertEquals(true, asycudaManifestAdditionalTabPage.TabVisible);
				}
			}
		}
	}
}
