using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class IdentificationOfGoodsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TestIdentificationOfGoodsGroupBox()
		{
			var groupBox = control.IdentificationofGoodsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), groupBox.Location);
				AssertEquals("Caption", "Identification Of Goods", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestIdentificationOfGoodsSubGroupBox()
		{
			var groupBox = control.IdentificationofGoodsSubGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 146, true), groupBox.Location);
				AssertEquals("Caption", "Identification Of Goods", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestProcessedProductsGroupBox()
		{
			var groupBox = control.ProcessedProductsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 51, true), groupBox.Location);
				AssertEquals("Caption", "Processed Products", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestIdentificationOfGoodsCodeTextBox()
		{
			var box = control.IdentificationOfGoodsCodeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", box);
				AssertEquals("BindTo", "CustomsEntryInstructions.ZG_IdOfGoodCode", box.BindTo);
				AssertCollectionContains("Within ActivitiesAndProceduresGroupBox", box, control.IdentificationofGoodsSubGroupBox.Controls);
			});
		}

		public void TestIdentificationOfGoodsDetailsTextBox()
		{
			var box = control.IdentificationOfGoodsDetailsTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", box);
				AssertEquals("BindTo", "CustomsEntryInstructions.IdentificationofGoodsDetails", box.BindTo);
				AssertCollectionContains("Within IdentificationofGoodsSubGroupBox", box, control.IdentificationofGoodsSubGroupBox.Controls);
			});
		}

		public void TestGoodsDescriptionTextBox()
		{
			var box = control.GoodsDescriptionTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", box);
				AssertEquals("BindTo", "CustomsEntryInstructions.ProcessedProductDescription", box.BindTo);
				AssertCollectionContains("Within ProcessedProductsGroupBox", box, control.ProcessedProductsGroupBox.Controls);
			});
		}

		public void TestCommodityCodeTextBox()
		{
			var box = control.CommodityCodeTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", box);
				AssertEquals("BindTo", "CustomsEntryInstructions.ZG_ProcessedProductsCommodityCode", box.BindTo);
				AssertCollectionContains("Within ProcessedProductsGroupBox", box, control.ProcessedProductsGroupBox.Controls);
			});
		}

		public void TestRateOfYieldTextBox()
		{
			var box = control.RateOfYieldTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", box);
				AssertEquals("BindTo", "CustomsEntryInstructions.ZG_RateOfYield", box.BindTo);
			});
		}

		IdentificationOfGoodsUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new IdentificationOfGoodsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
