using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrdersModuleOverride))]
	public class WoolworthsOrdersModuleOverrideTest : ZModuleBasherTest
	{
		public void TestImportContainerCustomsDeclarationsMenuItem()
		{
			using (TestWoolworthsOrdersModuleOverride module = new TestWoolworthsOrdersModuleOverride())
			{
				MenuAssertion.AssertHasMenu("Menu item should exist", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import Order Customs Declarations");
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Orders;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
		}

		class TestWoolworthsOrdersModuleOverride : WoolworthsOrdersModuleOverride
		{
			public new FilterModuleMenuItemDescriptorCollection ImportMenuItems
			{
				get
				{
					return base.ImportMenuItems;
				}
			}

			public new FilterModuleMenuItemDescriptorCollection ExportMenuItems
			{
				get
				{
					return base.ExportMenuItems;
				}
			}
		}
	}
}
