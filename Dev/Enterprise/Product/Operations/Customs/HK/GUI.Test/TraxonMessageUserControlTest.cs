using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.HK.GUI.Testing
{
	class TraxonMessageUserControlTest : TestCaseWithFactory
	{
		public void TestUpdateMessageStatusBoundTextBox4Color_Acknowledged()
		{
			using (var control = new TraxonMessageUserControl())
			{
				control.SetDataBinding(traxonConsolStatus, ZString.Empty);
				var statusTextBox = control.FindSingle<ZTextBox>("Traxon_MessageStatusBoundTextBox");
				statusTextBox.Text = "Acknowledged in a test to determine colour";
				CombineAssertions(() =>
				{
					AssertEquals("Background", Color.ForestGreen, statusTextBox.BackColor);
					AssertEquals("Foreground", Color.White, statusTextBox.ForeColor);
				});
			}
		}

		public void TestUpdateMessageStatusBoundTextBox4Color_NoMessagesSent()
		{
			using (var control = new TraxonMessageUserControl())
			{
				control.SetDataBinding(traxonConsolStatus, ZString.Empty);
				var statusTextBox = control.FindSingle<ZTextBox>("Traxon_MessageStatusBoundTextBox");
				statusTextBox.Text = TraxonConsolStatus.NoMessagesSent;

				CombineAssertions(() =>
				{
					AssertEquals("Background", SystemColors.Control, statusTextBox.BackColor);
					AssertEquals("Foreground", SystemColors.WindowText, statusTextBox.ForeColor);
				});
			}
		}

		public void TestUpdateMessageStatusBoundTextBox4Color_AwaitingResponse()
		{
			using (var control = new TraxonMessageUserControl())
			{
				control.SetDataBinding(traxonConsolStatus, ZString.Empty);
				var statusTextBox = control.FindSingle<ZTextBox>("Traxon_MessageStatusBoundTextBox");
				statusTextBox.Text = TraxonConsolStatus.AwaitingResponse;

				CombineAssertions(() =>
				{
					AssertEquals("Background", Color.LightCoral, statusTextBox.BackColor);
					AssertEquals("Foreground", Color.White, statusTextBox.ForeColor);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var consol = Factory.New<ForwardingConsol>();
			traxonConsolStatus = new TraxonConsolStatus(consol);
		}
		TraxonConsolStatus traxonConsolStatus;
	}
}
