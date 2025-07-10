using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(PortOfLoadingUserControl))]
	sealed class PortOfLoadingUserControlTest : TestCaseWithFactory
	{
		public void TestResourceStyringBindingMember()
		{
			using (var control = new PortOfLoadingUserControl())
			{
				AssertEquals(nameof(AsycudaManifestHeader.AMA_RL_NKPortOfLoading), control.ResourceStringBindingMember);
			}
		}

		public void TestVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			using (var form = new ZForm(header))
			{
				using (var control = new PortOfLoadingUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var portOfLoadingTextBox = control.FindSingle<ZTextBox>("PortOfLoadingTextBox");
					AssertEquals(true, portOfLoadingTextBox.Visible);

					header.AMA_TransportMode = "SEA";
					AssertEquals(false, portOfLoadingTextBox.Visible);
				}
			}
		}
	}
}
