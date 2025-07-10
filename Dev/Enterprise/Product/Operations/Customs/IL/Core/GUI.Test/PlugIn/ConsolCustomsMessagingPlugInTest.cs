using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class ConsolCustomsMessagingPlugInTest : CustomsMessagingPlugInTestBase
	{
		public void TestCustomsMessagingMenuItems_AllEnabled()
		{
			using (ILCustomsDataRegistry.Instance.EnableILGatePassMovements.SetTemporaryValue(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var plugin = GetPlugInToTest())
			{
				var customsMessagingPlugInMenu = plugin.TopLevelMenu;
				AssertNotNull("TopLevelMenu exist", customsMessagingPlugInMenu);
				AssertType<ZMenuItem>(customsMessagingPlugInMenu);
				AssertEquals("the menu text", MainMenu, customsMessagingPlugInMenu.Text);
				AssertEquals("Menu items count", 1, customsMessagingPlugInMenu.MenuItems.Count);

				var gatepassMovementMenu = customsMessagingPlugInMenu.MenuItems[0];
				AssertNotNull("gatepassMovementMenu exist", gatepassMovementMenu);
				AssertType<ZMenuItem>(gatepassMovementMenu);
				AssertEquals("the GatepassMovementMenu text", GatepassMovementMenu, gatepassMovementMenu.Text);
				AssertContainsExactElementsInAnyOrder("There should be expected menu items (Reload DocumentsComplete.xml from file into DB)", new[] { NoMessagesMenu, GatepassMovementSendMenu }, gatepassMovementMenu.MenuItems.ToList<ZMenuItem>().Select(s => s.Caption.GetUnresolvedString()));
			}
		}

		public void TestCustomsMessagingMenuItems_WhenILGatePassMovementsIsDisable()
		{
			using (ILCustomsDataRegistry.Instance.EnableILGatePassMovements.SetTemporaryValue(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var plugin = GetPlugInToTest())
			{
				var customsMessagingPlugInMenu = plugin.TopLevelMenu;
				AssertNull("TopLevelMenu doesn't exist", customsMessagingPlugInMenu);
			}
		}

		public void TestMenuItemVisibility()
		{
			using (ILCustomsDataRegistry.Instance.EnableILGatePassMovements.SetTemporaryValue(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				const string GatepassMovementPath = "Customs Messaging|Gatepass Movement";
				const string SendGatepassMovementPath = GatepassMovementPath + "|Send Gatepass Movement";
				var factory = Factory;
				var consol = factory.NewWithValidTestData<ForwardingConsol>();

				var italianConsignee = factory.NewWithValidTestData<OrgHeader>();
				italianConsignee.OH_RL_NKClosestPort = "ITMIL";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					consol.JK_RL_NKDischargePort = "ITMIL";
					CombineAssertions("When DischartPort CountryCode not Israel", () =>
					{
						AssertMenuItemVisible(form, GatepassMovementPath, false);
						AssertMenuItemVisible(form, SendGatepassMovementPath, false);
					});

					consol.JK_RL_NKDischargePort = "ILASH";
					CombineAssertions("When DischartPort CountryCode is Israel", () =>
					{
						AssertMenuItemVisible(form, GatepassMovementPath, true);
						AssertMenuItemVisible(form, SendGatepassMovementPath, true);
					});
				}
			}
		}

		protected override ICustomsMessagingPlugInBaseForTest GetPlugInForTest() => new ConsolCustomsMessagingPlugInBaseForTest(Factory.NewWithValidTestData<ForwardingConsol>());

		protected override ZPlugIn GetPlugInToTest() => new ConsolCustomsMessagingPlugIn(Factory.NewWithValidTestData<ForwardingConsol>());

		const string GatepassMovementMenu = "Gatepass Movement";

		const string GatepassMovementSendMenu = "Send Gatepass Movement";

		const string NoMessagesMenu = "No Messages Available";
	}

	public sealed class ConsolCustomsMessagingPlugInBaseForTest : ConsolCustomsMessagingPlugIn, ICustomsMessagingPlugInBaseForTest
	{
		public ConsolCustomsMessagingPlugInBaseForTest(ForwardingConsol consol) : base(consol)
		{
		}

		string ICustomsMessagingPlugInBaseForTest.Name => base.Name;

		bool ICustomsMessagingPlugInBaseForTest.CanDelete => base.CanDelete;

		bool ICustomsMessagingPlugInBaseForTest.HasUserControl => base.HasUserControl;

		LicenceCheckpoint ICustomsMessagingPlugInBaseForTest.LicenceCheckPoint => base.LicenceCheckPoint;

		IBusiness ICustomsMessagingPlugInBaseForTest.BusinessEntity => base.BusinessEntity;
	}
}
