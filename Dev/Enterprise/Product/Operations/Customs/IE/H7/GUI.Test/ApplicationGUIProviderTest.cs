using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.Customs.IE.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.GUI.Testing
{
	[TestedType(typeof(H7ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : H7ApplicationGUIProviderAbstractTest<H7ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(IEH7BillLayouts);

		public void TestBillsGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				Assert("ABL_BolType IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BolType).IsUnavailable);
				Assert("ABL_FreightValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_FreightValue).IsUnavailable);
				Assert("ABL_RX_NKFreightValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency).IsUnavailable);

				Assert("ABL_Procedure IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Procedure).IsVisible);
			}
		}

		public void TestPacksGridMandatoryColumns()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>("billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>("billsAndPacksTabControl_TabPage_EUH7PackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var packUserControl = packsTabPage.FindSingle<EUH7PackUserControl>("EUH7PackUserControl");
				var grid = packUserControl.FindSingle<ZGrid>("PacksGrid");

				Assert(grid.GetColumnStyle(AsycudaPack.Schema.APA_LineNo).IsMandatory);
				Assert(grid.GetColumnStyle(AsycudaPack.Schema.APA_PackQty).IsMandatory);
				Assert(grid.GetColumnStyle(AsycudaPack.Schema.APA_PackUQ).IsMandatory);
				Assert(grid.GetColumnStyle(AsycudaPack.Schema.APA_MarksAndNumbers).IsMandatory);
				Assert(grid.GetColumnStyle(AsycudaPack.Schema.APA_Weight).IsMandatory);
				Assert(grid.GetColumnStyle(AsycudaPack.Schema.APA_WeightUQ).IsMandatory);
			}
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			base.AssertGetBillsGridExtraColumnInfos(columnInfos);

			var additionalDeclarationTypeColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_ShipmentType);
			var additionalProcedureColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_Procedure);

			CombineAssertions(() =>
			{
				AssertNotNull("additionalDeclarationTypeColumnStyleInfo", additionalDeclarationTypeColumnStyleInfo);
				Assert(additionalDeclarationTypeColumnStyleInfo.IsVisible);
				AssertType(typeof(ZDropEditColumnStyleInfo), additionalDeclarationTypeColumnStyleInfo);

				AssertNotNull("additionalProcedureColumnStyleInfo", additionalProcedureColumnStyleInfo);
				Assert(additionalProcedureColumnStyleInfo.IsVisible);
				AssertType(typeof(ZDropEditColumnStyleInfo), additionalProcedureColumnStyleInfo);
			});
		}

		public new void TestGetHeaderAdditionalTabPageUserControls()
		{
			var header = CreateNewManifest();
			var controls = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetHeaderAdditionalTabPageUserControls(header);
			AssertEquals(0, controls.Count());
		}

		public void TestBillAdditionalTabPages_GridColumns()
		{
			var expectedControlGridColumns = new Dictionary<string, string[]>
			{
				{ "Additional Documents", new[] { "CSI_SubType", "CSI_Code", "CSI_ReferenceNumber", "CSI_Description" } },
				{ "Supporting Documents", new[] { "CSI_Code", "DocumentDescription", "CSI_ReferenceNumber" } },
				{ "Previous Documents", new[] { "CSI_Code", "DocumentDescription", "CSI_ReferenceNumber" } }
			};

			var manifest = CreateNewManifest();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				foreach (var controlAndGridColumns in expectedControlGridColumns)
				{
					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.CaptionResourceString.Caption == controlAndGridColumns.Key);
					billsAndPacksTabControl.SelectedTab = itemTabpage;

					var grid = itemTabpage.FindSingle<ZGrid>();

					CombineAssertions(controlAndGridColumns.Key, () =>
					{
						AssertContainsExactElementsInAnyOrder("number of columns is the same", controlAndGridColumns.Value, grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName));
						EUH7GUITestHelper.AssertGridLayout(grid, controlAndGridColumns.Value);
					});
				}
			}
		}
		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = base.CreateNewManifest();
			header.AMA_ApplicationCode = "LV1";
			return header;
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[]
		{
			typeof(EUH7PackUserControl),
			typeof(EUH7ItemUserControl),
			typeof(AdditionalDocumentsUserControl),
			typeof(RelatedDocumentsUserControlWithGrid),
			typeof(RelatedDocumentsUserControlWithGrid),
			typeof(RequestedDocumentsUserControl)
		};

		protected override IReadOnlyList<Type> ExpectedItemAdditionalTabPageUserControls => new[]
		{
			typeof(EUH7ItemPacksUserControl),
			typeof(AdditionalDocumentsUserControl),
			typeof(RelatedDocumentsUserControlWithGrid),
			typeof(RelatedDocumentsUserControlWithGrid),
		};
	}
}
