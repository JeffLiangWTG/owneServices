using System;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override Type ExpectedBillLayoutType => typeof(ARBillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(ARBillPartiesLayouts);

		protected override void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals("AR additional Container columns", 2, columnInfos.Length);
		}

		public void TestBillsGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			var bill = manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				AssertEquals("ABL_BolType IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BolType).IsUnavailable);
			}
		}
	}
}
