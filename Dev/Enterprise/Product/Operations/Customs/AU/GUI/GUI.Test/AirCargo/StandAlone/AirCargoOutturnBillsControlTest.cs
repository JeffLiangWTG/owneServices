using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoOutturnBillsControlTest : TestCaseWithFactory
	{
		public void TestMessageTabRemoved()
		{
			using (var form = new ZForm(underbond))
			using (var control = new AirCargoOutturnBillsControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(underbond, "");
				form.Show();
				foreach (ZTabPage tab in control.FindSingle<ZTemplateTabControl>("MainTabControl").TabPages)
				{
					AssertNotEquals("MessagesTabPage", tab.Name);
				}
			}
		}

		public void TestOutturnMessagesGridBinding()
		{
			using (var form = new ZForm(underbond))
			using (var control = new AirCargoOutturnBillsControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(underbond, "");
				form.Show();
				AssertEquals("OutturnMessages", control.outturnMessagesUserControl.MessagesGrid.BindTo);
			}
		}

		public void TestCargoMessagesGridBinding()
		{
			using (var form = new ZForm(underbond))
			using (var control = new AirCargoOutturnBillsControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(underbond, "");
				form.Show();
				AssertEquals("CargoMessages", control.cargoMessagesControl.MessagesGrid.BindTo);
			}
		}

		CusUnderbond underbond;
		protected override void SetUp()
		{
			underbond = Factory.New<CusUnderbond>();
			base.SetUp();
		}
	}
}
