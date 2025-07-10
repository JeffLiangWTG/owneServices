using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class BillAdditionalTabPageUserControlTest : TestCaseWithFactory
	{
		public void TestVisibilityOfRelatedDeclarationForExportTabPage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				manifest.AMA_ManifestType = "DENIHR";
				using (var form = new ManifestForm(manifest))
				{
					form.Show();
					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(a => a.Name == "billsAndPacksTabControl");
					var billAdditionalTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(a => a.Name == "billsAndPacksTabControl_TabPage_TRBillAdditionalUserControl");
					Assert(billAdditionalTabPage.TabVisible);

					manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
					Assert(!billAdditionalTabPage.TabVisible);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var manifest = Factory.New<AsycudaManifestHeader>();
				manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				manifest.AMA_ManifestType = "ALM";
				using (var form = new ManifestForm(manifest))
				{
					form.Show();
					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(a => a.Name == "billsAndPacksTabControl");
					var billAdditionalTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(a => a.Name == "billAdditionalTabPage");
					AssertEquals(true, billAdditionalTabPage == null);
				}
			}
		}
	}
}
