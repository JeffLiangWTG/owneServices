using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(ManualScanning))]
	sealed class ManualScanningTest : ZFormBasherTest
	{
		public void TestEnterKeyboardEventSentFromScanningEquipmentDoesNotCloseTheForm()
		{
			var cusMAWB = Factory.NewWithValidTestData<CusMAWB>();
			var scanManager = new AirScanForOutturnManager(new ScanCusMAWB(cusMAWB));
			using (var scanForm = new ManualScanning(scanManager))
			{
				scanForm.Show();
				KeySender.PostKeyDown(scanForm, Keys.Enter);
				Application.DoEvents();
				Assert(!scanForm.IsDisposed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var cusMAWB = Factory.NewWithValidTestData<CusMAWB>();
			using (var form = new ZForm())
			{
				var scanManager = new AirScanForOutturnManager(new ScanCusMAWB(cusMAWB));
				return new ManualScanning(scanManager);
			}
		}
	}
}
