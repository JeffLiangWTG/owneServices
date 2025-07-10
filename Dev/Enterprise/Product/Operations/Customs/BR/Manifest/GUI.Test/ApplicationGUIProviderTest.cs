using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.BR.Manifest.Business;
using Enterprise.Customs.BR.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override Type ExpectedBillLayoutType => typeof(BRBillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(BRBillPartiesLayouts);

		public void TestGetBillLayoutControlBag()
		{
			var header = CreateNewManifest();
			var billLayout = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillLayout().Layout;
			Assert(nameof(BRBillControlBag) + " used", billLayout.ControlBags.OfType<BRBillControlBag>().Any());
		}

		protected override void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals("BR additional Container columns", 1, columnInfos.Length);
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

		protected override void AssertGetTaxesGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals("BR additional Taxes columns", 1, columnInfos.Length);
			AssertEquals("Additional Column name", AsycudaTax.Schema.AET_TypeDescription, columnInfos[0].ColumnName);
		}

		public void TestShowTaxesUserControlCore()
		{
			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				AssertEquals("ShowTaxesUserControl is false", false, ApplicationGUIProvider.GetApplicationGuiProvider(header).ShowTaxesUserControl(header));

				header.AMA_ManifestType = BRManifestTypes.Codes.MER;
				AssertEquals("ShowTaxesUserControl is true", true, ApplicationGUIProvider.GetApplicationGuiProvider(header).ShowTaxesUserControl(header));
			}
		}

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var applicationGUIProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var packs = applicationGUIProvider.GetPacksGridColumnsOrder();
			AssertEquals(11, packs.Length);

			var columns = new string[]
				{
					AsycudaPack.Schema.APA_PackQty,
					AsycudaPack.Schema.APA_PackUQ,
					AsycudaPack.Schema.BulkType,
					AsycudaPack.Schema.APA_Weight,
					AsycudaPack.Schema.APA_WeightUQ,
					AsycudaPack.Schema.APA_Volume,
					AsycudaPack.Schema.APA_VolumeUQ,
					AsycudaPack.Schema.APA_GoodsDescription,
					AsycudaPack.Schema.ContainerPK,
					AsycudaPack.Schema.APA_CommodityCode,
					AsycudaPack.Schema.APA_MarksAndNumbers,
				};

			AssertContainsExactElementsInExactOrder("PacksGridColumnsOrder", columns, packs);
		}

		protected override void AssertGetPacksGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			Assert("BulkType should be visible", columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaPack.Schema.BulkType).IsVisible);
		}

		protected override string ExpectedDutiesTabPageCaption => "Charges";
	}
}
