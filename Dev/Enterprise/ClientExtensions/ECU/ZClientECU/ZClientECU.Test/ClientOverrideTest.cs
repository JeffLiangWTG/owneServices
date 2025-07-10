using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Client.ECU.ConsolExport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.ZClientECU.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestOverrides()
		{
			ClientOverride @override = ClientOverride.Instance;
			AssertEquals("Client", Clients.ECU, @override.Client);
			AssertEquals("Client Display Name should be 'ECU'", "ECU", @override.ClientDisplayName);
			AssertEquals("Help Web Page should be empty", "", @override.HelpWebPage);
		}

		[GuiTest]
		public void TestInitialiseAndUninitialise()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			using (ZForm form = new ZForm(consol))
			{
				using (ActionDataMenuItem menu = ActionDataMenuItem.New(form))
				{
					AssertEquals("Menu should be initialised by default in this test; return client's action data menu item", typeof(ECUActionMenu), menu.GetType());
				}

				ClientOverride.Instance.Uninitialise();
				using (ActionDataMenuItem menu = ActionDataMenuItem.New(form))
				{
					AssertEquals("Menu should be unintialised; return normal action data menu item", typeof(ActionDataMenuItem), menu.GetType());
				}

				ClientOverride.Instance.Initialise();
				using (ActionDataMenuItem menu = ActionDataMenuItem.New(form))
				{
					AssertEquals("Menu should be initialised again on; return client's action data menu item", typeof(ECUActionMenu), menu.GetType());
				}
			}
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
