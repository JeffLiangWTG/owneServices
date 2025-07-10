using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	sealed class G5V1TemporaryStoragePreviousDocumentsDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestReferenceNumber2TextBox()
		{
			using (var control = new G5V1TemporaryStoragePreviousDocumentsDetailsUserControl())
			{
				var referenceNumberTextBox = control.ReferenceNumber2TextBox;
				AssertNotNull("ReferenceNumber2TextBox", referenceNumberTextBox);
				referenceNumberTextBox.AssertThisControl(x => x.WithBindTo("CSI_ReferenceNumber2")
															   .WithCaption("Flight Number"));
			}
		}
	}
}
