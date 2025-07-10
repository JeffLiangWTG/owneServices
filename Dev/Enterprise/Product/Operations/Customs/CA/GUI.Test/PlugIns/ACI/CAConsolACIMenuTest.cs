using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CAConsolACIMenuTest : TestCaseWithFactory
	{
		public void TestVisibility_ForOceanBill()
		{
			using (var menu = new CAConsolACIMenuForTest(MasterBill, new ConsolACIMessageManager(() => MasterBill)))
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("SendMessages.Visible", true, menu.SendMessages.Visible);
				AssertEquals("WithdrawMessages.Visible", true, menu.WithdrawMessages.Visible);
				AssertEquals("ResetToOriginal.Visible", true, menu.ResetToOriginal.Visible);
				AssertEquals("'Refresh All' menu item should be null, because exists only for Consol", null, menu.refreshAll);
			}
		}

		public void TestVisibliity_ForConsol()
		{
			using (var menu = new CAConsolACIMenuForTest(consol, new ConsolACIMessageManager(null)))
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("SendMessages.Visible", true, menu.SendMessages.Visible);
				AssertEquals("WithdrawMessages.Visible", true, menu.WithdrawMessages.Visible);
				AssertEquals("ResetToOriginal.Visible", true, menu.ResetToOriginal.Visible);
				AssertEquals("Refresh All should be visible", true, menu.refreshAll.Visible);
			}
		}

		CusSCAOceanBill masterBill;
		CusSCAOceanBill MasterBill => masterBill ?? (masterBill = Factory.New<CusSCAOceanBill>());

		ForwardingConsol consol;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
		}

		sealed class CAConsolACIMenuForTest : CAConsolACIMenu
		{
			internal CAConsolACIMenuForTest(CusSCAOceanBill oceanBill, ConsolACIMessageManager manager) : base(oceanBill, manager)
			{
			}

			internal CAConsolACIMenuForTest(ForwardingConsol consol, ConsolACIMessageManager manager) : base(consol, manager)
			{
			}

			internal MenuItem SendMessages => sendMessages;

			internal MenuItem WithdrawMessages => withdrawMessages;

			internal MenuItem ResetToOriginal => resetToOriginal;
		}
	}
}
