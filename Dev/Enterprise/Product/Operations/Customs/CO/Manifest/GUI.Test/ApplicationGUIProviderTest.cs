using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CO.Manifest.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Dictionary<string, ControlReference[]> GetManifestControlGroups()
		{
			var common = CommonManifestControlBag.Instance;
			var co = COManifestControlBag.Instance;
			var groups = new Dictionary<string, ControlReference[]>();
			groups.Add("Sea Vessel", new[]
			{
				common.VesselCodeFindBox
			});
			return groups;
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override Type ExpectedBillLayoutType => typeof(COBillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(COBillPartiesLayouts);

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(13, columnsOrder.Length);
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					AsycudaPack.Schema.APA_LineNo,
					AsycudaPack.Schema.APA_PackQty,
					AsycudaPack.Schema.APA_PackUQ,
					AsycudaPack.Schema.APA_Weight,
					AsycudaPack.Schema.APA_WeightUQ,
					AsycudaPack.Schema.APA_Volume,
					AsycudaPack.Schema.APA_VolumeUQ,
					AsycudaPack.Schema.APA_GoodsDescription,
					AsycudaPack.Schema.ContainerPK,
					UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
					UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
					AsycudaPack.Schema.IsHazardous,
					AsycudaPack.Schema.ContactPK,
				},
				columnsOrder);
		}

		public void TestCOSpecificFiedsVisibilty()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Packs.AddNew();

			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");
				var asycudaBillTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = asycudaBillTabPage;
				var billsGrid = asycudaBillTabPage.FindSingle<ZGrid>("BillsGrid");

				AssertEquals("ABL_BillIssueDate IsVisible", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BillIssueDate).IsVisible);

				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var asycudaPackTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = asycudaPackTabPage;
				var asycudaPackUserControl = asycudaPackTabPage.FindSingleOrDefault<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

				AssertEquals("ContactPK IsVisible", true, packsGrid.GetColumnStyle(AsycudaPack.Schema.ContactPK).IsVisible);
				AssertEquals("IsHazardous IsVisible", true, packsGrid.GetColumnStyle(AsycudaPack.Schema.IsHazardous).IsVisible);
			}
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

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[] { "ABL_BillIssueDate" }, columnInfos.Select(s => s.ColumnName));
		}

		protected override void AssertGetPacksGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			var expectedColumns = new[] { AsycudaPack.Schema.ContactPK, AsycudaPack.Schema.IsHazardous };
			AssertContainsExactElementsInExactOrder(expectedColumns, columnInfos.Select(s => s.ColumnName));
		}
	}
}
