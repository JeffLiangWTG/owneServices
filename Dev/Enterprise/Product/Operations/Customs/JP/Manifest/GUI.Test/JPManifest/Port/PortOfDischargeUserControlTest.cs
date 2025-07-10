using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(PortOfDischargeUserControl))]
	sealed class PortOfDischargeUserControlTest : TestCaseWithFactory
	{
		public void TestResourceStyringBindingMember()
		{
			using (var control = new PortOfDischargeUserControl())
			{
				AssertEquals(nameof(AsycudaManifestHeader.AMA_RL_NKPortOfDischarge), control.ResourceStringBindingMember);
			}
		}

		public void TestVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			using (var form = new ZForm(header))
			{
				using (var control = new PortOfDischargeUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var portOfDischargeTextBox = control.FindSingle<ZTextBox>("PortOfDischargeTextBox");
					AssertEquals(true, portOfDischargeTextBox.Visible);

					header.AMA_TransportMode = "SEA";
					AssertEquals(false, portOfDischargeTextBox.Visible);
				}
			}
		}
	}
}
