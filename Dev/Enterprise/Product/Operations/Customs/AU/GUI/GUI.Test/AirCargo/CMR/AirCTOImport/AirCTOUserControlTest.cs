using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCTOUserControlTest : TestCaseWithFactory
	{
		public void TestUnderbondPlugin()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			using (ZForm form = new ZForm(mAWB))
			{
				using (AirCTOUserControl control = new AirCTOUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals("Should be only one plugin", 1, control.airCTOMainTabControl.PlugIns.Instances.Length);
					AssertEquals("Plugin Type", typeof(AU.GUI.CMRCusUnderbondPlugin), control.airCTOMainTabControl.PlugIns.Instances[0].GetType());
				}
			}
		}

		public void TestMessagesUserControlBindTo()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			using (ZForm form = new ZForm(mAWB))
			{
				using (AirCTOUserControl control = new AirCTOUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals("MessagesGrid.BindTo", "FakeFlightOuturnUnderbond+Messages", control.messagesUserControl.MessagesGrid.BindTo);
					AssertEquals("MessageTextTextBox.BindTo", "FakeFlightOuturnUnderbond+Messages.EM_FormattedMessageText", control.messagesUserControl.MessageTextTextBox.BindTo);
				}
			}
		}
	}
}
