using System.Drawing;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(FakeForm))]
	sealed class ZManifestMessageHistoryUserControlTest : ZFormBasherTest
	{
		public void TestStatusColorOnIdle()
		{
			var consol = Factory.New<ForwardingConsol>();
			var idlMessage = Factory.New<CMRIDLMessage>();
			idlMessage.EM_ReceiveTransmit = Messaging.Business.EDIInterchange.Direction.Receive;
			consol.Messages.Add(idlMessage);
			var status = new ESMManifestStatus(consol);
			AssertEquals("precondition", "Idle", status.E2_MessageStatus);
			using (var form = new ZForm(status))
			using (var control = new ZManifestMessageHistoryUserControl())
			{
				control.SetDataBinding(status, "");
				form.Controls.Add(control);
				form.Show();
				var messageStatusBoundTextBox = control.FindSingle<ZTextBox>("E2_MessageStatusBoundTextBox");
				AssertEquals(Color.LightYellow, messageStatusBoundTextBox.BackColor);
				AssertEquals(Color.LightCoral, messageStatusBoundTextBox.ForeColor);
			}
		}

		protected override Form GetFormToBashCore() => new FakeForm(Factory.New<ForwardingConsol>());

		sealed class FakeForm : ZForm
		{
			public FakeForm(ForwardingConsol consol) : base(new Exit2ManifestStatus(consol))
			{
			}

			ZManifestMessageHistoryUserControl manifestMessageHistoryUserControl;
			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				manifestMessageHistoryUserControl = new ZManifestMessageHistoryUserControl();
				Size = manifestMessageHistoryUserControl.ClientSize;
				manifestMessageHistoryUserControl.Dock = DockStyle.Fill;
				Controls.Add(manifestMessageHistoryUserControl);
				DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
				DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.Exit2ManifestStatus";
				Name = "ZManifestMessageHistoryUserControl";
			}
		}
	}
}
