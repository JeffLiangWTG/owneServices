using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class VoyageManifestImportMenuTest : TestCase
	{
		public void TestText()
		{
			AssertEquals("Menu text", "Import Data", Menu.Text);
		}

		public void TestMenuItems()
		{
			AssertEquals("We have one item", 1, Menu.MenuItems.Count);
			AssertEquals("name matches", "Import ShipNET", Menu.MenuItems[0].Text);
		}

		public void TestOnClickShipNetEvent()
		{
			TestHelperImportMenu.Initialise();
			using (ZForm form = new ZForm())
			using (TestHelperImportMenu helper = TestHelperImportMenu.New())
			{
				form.Menu = new MainMenu();
				form.Menu.MenuItems.Add(helper);
				form.Show();
				AssertEquals("precondition", false, helper.OnClickShipNetEventHit);
				helper.MenuItems[0].PerformClick();
				AssertEquals("OnTsManifestClick has been hit", true, helper.OnClickShipNetEventHit);
			}
		}

		public void TestConstructorDelegate()
		{
			using (MenuItem menu = VoyageManifestImportMenu.New())
			{
				AssertEquals("by default returns base class", typeof(VoyageManifestImportMenu), menu.GetType());
			}

			TestHelperImportMenu.Initialise();
			using (MenuItem menu = VoyageManifestImportMenu.New())
			{
				AssertEquals("subclass returned via delegate", typeof(TestHelperImportMenu), menu.GetType());
			}
		}

		VoyageManifestImportMenu menu;
		VoyageManifestImportMenu Menu => menu ?? (menu = VoyageManifestImportMenu.New());

		protected override void TearDown()
		{
			if (menu != null)
			{
				menu.Dispose();
			}
			base.TearDown();
		}

		sealed class TestHelperImportMenu : VoyageManifestImportMenu
		{
			public new static TestHelperImportMenu New() => new TestHelperImportMenu();

			public static void Initialise()
			{
				constructor = new ConstructorDelegate(New);
			}

			public bool OnClickShipNetEventHit;

			protected override void OnClickShipNetEvent(object sender, EventArgs args)
			{
				OnClickShipNetEventHit = true;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					constructor = null;
				}
				base.Dispose(disposing);
			}
		}
	}
}
