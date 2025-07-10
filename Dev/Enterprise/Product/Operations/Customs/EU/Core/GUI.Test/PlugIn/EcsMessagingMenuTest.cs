using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	public class EcsMessagingMenuTest : TestCaseWithFactory
	{
		public void TestMenuText()
		{
			AssertEquals("Exit Control", new EcsMessagingMenu().Text);
		}

		public void TestSetupMessageMenuProvider()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			var messageMenuProvider = new EcsMessageMenuProviderForTesting(exitHeader);

			var ecsMessageMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Latvia, new TestObjectHandle(messageMenuProvider) }
			};

			using (ObjectFactory.Substitute("EcsMessageMenuProviders", ecsMessageMenuProviders))
			{
				var menu = new EcsMessagingMenu();
				AssertNull("ExitHeader", menu.ExitHeader);
				AssertEquals("MenuItems.Count", 0, menu.MenuItems.Count);

				menu.ExitHeader = exitHeader;
				AssertEquals("ExitHeader", exitHeader, menu.ExitHeader);
				AssertEquals("MenuItems.Count", 3, menu.MenuItems.Count);
				AssertEquals("MenuItems[0]", "Arrive at exit location", menu.MenuItems[0].Text);
				AssertEquals("MenuItems[1]", "Depart from exit location", menu.MenuItems[1].Text);
				AssertEquals("MenuItems[2]", "Capture MRNs", menu.MenuItems[2].Text);
			}
		}
	}

	class EcsMessageMenuProviderForTesting : EcsMessageMenuProvider
	{
		public EcsMessageMenuProviderForTesting(CusExitControlHeader exitHeader) : base(exitHeader)
		{
		}

		public override IEnumerable<ZMenuItem> CreateMenuItems()
		{
			yield return new ZMenuItem("Arrive at exit location");
			yield return new ZMenuItem("Depart from exit location");
			yield return new ZMenuItem("Capture MRNs");
		}
	}
}
