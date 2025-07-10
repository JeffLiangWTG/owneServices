using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	partial class TempStorageRegisterPremisesUserControlTest : TestCaseWithFactory
	{
		public void TestPremisesCodeTextBox() => CombineAssertions(() =>
		{
			var codeTextBox = control.PremisesCodeTextBox;
			AssertType<ZTextBox>("Type", codeTextBox);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegPremises.SRP_Code), codeTextBox.GetBindingMember());
		});

		public void TestPremisesDescriptionTextBox() => CombineAssertions(() =>
		{
			var descriptionTextBox = control.PremisesDescriptionTextBox;
			AssertType<ZTextBox>("Type", descriptionTextBox);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegPremises.SRP_Description), descriptionTextBox.GetBindingMember());
		});

		public void TestLocationDropEdit()
		{
			var locationDropEdit = control.LocationDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", locationDropEdit);
				AssertEquals("GetBindingMember", nameof(CusTempStorageRegPremises.SRP_CustomsLocation), locationDropEdit.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TempStorageRegisterPremisesUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TempStorageRegisterPremisesUserControl control;
	}
}
