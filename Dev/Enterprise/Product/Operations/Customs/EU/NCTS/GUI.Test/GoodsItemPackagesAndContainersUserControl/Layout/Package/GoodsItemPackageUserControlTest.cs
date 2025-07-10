using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class GoodsItemPackageUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsPackage), control.BindingSource.DataSourceType);
		}

		public void TestPackageTypeDropEdit()
		{
			var packageTypeDropEdit = control.PackageTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", packageTypeDropEdit);
				AssertEquals("BindTo", nameof(NctsPackage.B5_UnitType), packageTypeDropEdit.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, packageTypeDropEdit.CharacterCasing);
			});
		}

		public void TestNumberOfPackagesCalcEdit()
		{
			var numberOfPackagesCalcEdit = control.NumberOfPackagesCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", numberOfPackagesCalcEdit);
				AssertEquals("BindTo", nameof(NctsPackage.B5_UnitCount), numberOfPackagesCalcEdit.BindTo);
			});
		}

		public void TestMarksAndNumbersTextBox()
		{
			var marksAndNumbersTextBox = control.MarksAndNumbersTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", marksAndNumbersTextBox);
				AssertEquals("BindTo", nameof(NctsPackage.B5_MarksAndNumbers), marksAndNumbersTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, marksAndNumbersTextBox.CharacterCasing);
			});
		}

		public void TestPackageIDTextBox()
		{
			var packageIDTextBox = control.PackageIDTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", packageIDTextBox);
				AssertEquals("BindTo", nameof(NctsPackage.B5_PackageID), packageIDTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, packageIDTextBox.CharacterCasing);
			});
		}

		public void TestBrandTextBox()
		{
			var brandTextBox = control.BrandTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", brandTextBox);
				AssertEquals("BindTo", nameof(NctsPackage.B5_Brand), brandTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, brandTextBox.CharacterCasing);
			});
		}

		public void TestModelTextBox()
		{
			var modelTextBox = control.ModelTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", modelTextBox);
				AssertEquals("BindTo", nameof(NctsPackage.B5_Model), modelTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, modelTextBox.CharacterCasing);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new GoodsItemPackageUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		GoodsItemPackageUserControl control;
	}
}
