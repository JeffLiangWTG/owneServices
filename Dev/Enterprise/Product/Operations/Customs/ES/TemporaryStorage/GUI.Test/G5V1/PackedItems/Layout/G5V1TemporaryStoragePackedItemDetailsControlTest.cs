using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	public class G5V1TemporaryStoragePackedItemDetailsControlTest : TestCaseWithFactory
	{
		public void TestUCRTextBox()
		{
			var ucrTextBox = control.UCRTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", ucrTextBox);
				AssertEquals("GetBindingMember", "Bills.PackedItems.UCR", ucrTextBox.GetBindingMember());
			});
		}

		public void TestPresentationDateEdit()
		{
			var presentationDateEdit = control.PresentationDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", presentationDateEdit);
				AssertEquals("GetBindingMember", "Bills.PackedItems.PresentationDate", presentationDateEdit.GetBindingMember());
			});
		}

		public void TestMissingCheckBox()
		{
			var missingCheckBox = control.MissingCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", missingCheckBox);
				AssertEquals("GetBindingMember", "Bills.PackedItems.IsMissing", missingCheckBox.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new G5V1TemporaryStoragePackedItemDetailsControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		G5V1TemporaryStoragePackedItemDetailsControl control;
	}
}
