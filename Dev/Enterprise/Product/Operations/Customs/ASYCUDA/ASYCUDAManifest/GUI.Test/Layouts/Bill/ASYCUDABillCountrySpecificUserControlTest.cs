using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDAManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDAManifest.GUI.Testing
{
	sealed class ASYCUDABillCountrySpecificUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			using (var form = new ZForm(bill))
			using (var control = new ASYCUDABillCountrySpecificUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var separator = control.FindSingle<SeparatorUserControl>("BDEGMSeparatorUserControl");
				var sADOfficeCodeDropEdit = control.FindSingle<ZDropEdit>("SADOfficeCodeDropEdit");
				var sADRegistrationSerialTextBox = control.FindSingle<ZTextBox>("SADRegistrationSerialTextBox");
				var sADRegistrationNumberTextBox = control.FindSingle<ZTextBox>("SADRegistrationNumberTextBox");
				var sADRegistrationDateEdit = control.FindSingle<ZDateEdit>("SADRegistrationDateEdit");

				CombineAssertions(() =>
				{
					AssertEquals("BDEGMSeparatorUserControl", "Export General Manifest only", separator.CaptionResourceString.Caption);
					AssertEquals("SADOfficeCodeDropEdit", nameof(AsycudaBill.SADOfficeCode), sADOfficeCodeDropEdit.BindTo);
					AssertEquals("SADRegistrationSerialTextBox", nameof(AsycudaBill.SADRegistrationSerial), sADRegistrationSerialTextBox.BindTo);
					AssertEquals("SADRegistrationNumberTextBox", nameof(AsycudaBill.SADRegistrationNumber), sADRegistrationNumberTextBox.BindTo);
					AssertEquals("SADRegistrationDateEdit", nameof(AsycudaBill.SADRegistrationDate), sADRegistrationDateEdit.BindTo);
				});
			}
		}
	}
}
