using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	sealed class EUH7PackUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestSetVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			using (var form = new ZForm(manifest))
			using (var control = new EUH7PackUserControl())
			{
				control.SetDataBinding(bill, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("PacksGrid");

				CombineAssertions(() =>
				{
					EUH7GUITestHelper.AssertGridLayout(grid,
						[
							AsycudaPack.Schema.APA_PackQty,
							AsycudaPack.Schema.APA_PackUQ,
							AsycudaPack.Schema.APA_CommodityCode,
							AsycudaPack.Schema.APA_GoodsDescription,
							AsycudaPack.Schema.APA_MarksAndNumbers,
							AsycudaPack.Schema.APA_Weight,
							AsycudaPack.Schema.APA_WeightUQ,
							AsycudaPack.Schema.APA_Volume,
							AsycudaPack.Schema.APA_VolumeUQ,
							AsycudaPack.Schema.APA_VINNumber
						]);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_PackQty);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_PackUQ);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_CommodityCode);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_GoodsDescription);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_MarksAndNumbers);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_Weight);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_WeightUQ);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_Volume);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_VolumeUQ);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_VINNumber);
				});
			}

			using (var form = new ZForm(bill))
			using (var control = new EUH7PackUserControl())
			{
				control.SetDataBinding(bill, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("PacksGrid");

				CombineAssertions(() =>
				{
					EUH7GUITestHelper.AssertGridLayout(grid,
						[
							AsycudaPack.Schema.APA_PackQty,
							AsycudaPack.Schema.APA_PackUQ,
							AsycudaPack.Schema.APA_CommodityCode,
							AsycudaPack.Schema.APA_GoodsDescription,
							AsycudaPack.Schema.APA_MarksAndNumbers,
							AsycudaPack.Schema.APA_Weight,
							AsycudaPack.Schema.APA_WeightUQ,
							AsycudaPack.Schema.APA_Volume,
							AsycudaPack.Schema.APA_VolumeUQ,
							AsycudaPack.Schema.APA_VINNumber
						]);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_PackQty);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_PackUQ);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_CommodityCode);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_GoodsDescription);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_MarksAndNumbers);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_Weight);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_WeightUQ);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_Volume);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_VolumeUQ);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPack.Schema.APA_VINNumber);
				});
			}
		}

		public void TestSetPacksGridColumnAvaialability()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			using (var form = new ZForm(manifest))
			using (var control = new EUH7PackUserControlForColumnAvailabilityTest())
			{
				control.SetDataBinding(bill, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("PacksGrid");

				CombineAssertions(() =>
				{
					Assert("APA_CommodityCode", grid.GetColumnStyle("APA_CommodityCode").IsUnavailable);
					Assert("APA_Volume", grid.GetColumnStyle("APA_Volume").IsUnavailable);

					foreach (var name in new[] { "APA_PackQty", "APA_PackUQ",
												"APA_GoodsDescription", "APA_MarksAndNumbers",
												"APA_Weight", "APA_WeightUQ", "APA_VolumeUQ", "APA_VINNumber" })
					{
						Assert(name, !grid.GetColumnStyle(name).IsUnavailable);
					}
				});
			}
		}

		class EUH7PackUserControlForColumnAvailabilityTest : EUH7PackUserControl
		{
			protected override Dictionary<bool, string[]> PacksGridColumnAvailability()
			{
				return new Dictionary<bool, string[]>
				{
					{
						false,
						new[] { "APA_CommodityCode", "APA_Volume" }
					}
				};
			}
		}
	}
}
