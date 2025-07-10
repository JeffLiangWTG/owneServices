using System;
using System.Windows.Forms;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAReleaseNotificationsModule))]
	public class CAReleaseNotificationsModuleTest : ZModuleBasherTest
	{
		[ExpectNoExceptions]
		public void TestActionMenu()
		{
			CACustomsDataRegistry.Instance.RNSActive.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (var module = new CAReleaseNotificationsModule())
			{
				var menus = module.NewActionMenuItemsInternal.FindByText("Send Release Status Query").MenuItems;
				foreach (MenuItem menuItem in menus)
				{
					menuItem.PerformClick();
				}
				menus = module.NewActionMenuItemsInternal.FindByText("Arrival Certification Message").MenuItems;
				foreach (MenuItem menuItem in menus)
				{
					menuItem.PerformClick();
				}
			}

			CACustomsDataRegistry.Instance.RNSActive.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			using (var module = new CAReleaseNotificationsModule())
			{
				AssertNull(module.NewActionMenuItemsInternal.FindByText("Send Release Status Query"));
				AssertNull(module.NewActionMenuItemsInternal.FindByText("Arrival Certification Message"));
			}
		}

		public void TestProperties()
		{
			using (var module = new CAReleaseNotificationsModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.CAReleaseNotifications, module.SecurityCheckpoint);
				AssertEquals("EmbeddedControl", typeof(CAReleaseNotificationsFilterControl), module.EmbeddedControl.GetType());
				AssertEquals("FilterBusinessObject", typeof(CAReleaseNotificationsFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CAReleaseNotifications;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
