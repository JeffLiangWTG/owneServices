using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ASYCUDAManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDAManifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		public void TestEGMColumnVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ZForm(manifest))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var mainTabControl = control.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions(() =>
				{
					AssertSADColumnVisibity("Not show for country other than Bangladesh", true, billsGrid);

					manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Bangladesh;
					AssertSADColumnVisibity("Not show for import manifest", true, billsGrid);

					manifest.AMA_Nature = "EXP";
					AssertSADColumnVisibity("show for export manifest on Bangladesh", false, billsGrid);
				});
			}

			void AssertSADColumnVisibity(string message, bool expectedUnavailable, ZGrid billsGrid)
			{
				AssertEquals(message + ": SADOfficeCode visiblity", !expectedUnavailable, billsGrid.GetColumnStyle(AsycudaBill.Schema.SADOfficeCode).IsVisible);
				AssertEquals(message + ": SADRegistrationSerial visiblity", !expectedUnavailable, billsGrid.GetColumnStyle(AsycudaBill.Schema.SADRegistrationSerial).IsVisible);
				AssertEquals(message + ": SADRegistrationNumber visiblity", !expectedUnavailable, billsGrid.GetColumnStyle(AsycudaBill.Schema.SADRegistrationNumber).IsVisible);
				AssertEquals(message + ": SADRegistrationDate visiblity", !expectedUnavailable, billsGrid.GetColumnStyle(AsycudaBill.Schema.SADRegistrationDate).IsVisible);

				AssertEquals(message + ": SADOfficeCode", expectedUnavailable, billsGrid.GetColumnStyle(AsycudaBill.Schema.SADOfficeCode).IsUnavailable);
				AssertEquals(message + ": SADRegistrationSerial", expectedUnavailable, billsGrid.GetColumnStyle(AsycudaBill.Schema.SADRegistrationSerial).IsUnavailable);
				AssertEquals(message + ": SADRegistrationNumber", expectedUnavailable, billsGrid.GetColumnStyle(AsycudaBill.Schema.SADRegistrationNumber).IsUnavailable);
				AssertEquals(message + ": SADRegistrationDate", expectedUnavailable, billsGrid.GetColumnStyle(AsycudaBill.Schema.SADRegistrationDate).IsUnavailable);
			}
		}

		protected override void AssertGetContainersGridColumnVisibility(IReadOnlyDictionary<bool, string[]> columnVisibility)
		{
			AssertEquals(1, columnVisibility.Count);

			if (columnVisibility.TryGetValue(false, out string[] invisibleColumns))
			{
				AssertContainsExactElementsInAnyOrder(
					new string[]
					{
						ManifestBase.AutoAsycudaContainer.Schema.ACN_Seal1UnloadingState,
						ManifestBase.AutoAsycudaContainer.Schema.ACN_Seal2UnloadingState,
						ManifestBase.AutoAsycudaContainer.Schema.ACN_Seal3UnloadingState,
					},
					invisibleColumns);
			}
		}

		protected override void AssertGetBillsGridColumnVisiblilityOnValueChanged(IReadOnlyDictionary<string, bool> columnsAvailablility, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			AssertEquals(4, columnsAvailablility.Count);
		}

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override Type ExpectedBillLayoutType => typeof(ASYCUDABillLayouts);

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[] {
				AsycudaBill.Schema.SADOfficeCode,
				AsycudaBill.Schema.SADRegistrationSerial,
				AsycudaBill.Schema.SADRegistrationNumber,
				AsycudaBill.Schema.SADRegistrationDate }, columnInfos.Select(s => s.ColumnName));
		}
	}
}
