using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ChequeTransactionModule))]
	public class ChequeTransactionModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ChequeTransaction;
		}

		public void TestAllowedAction()
		{
			using (var module = (ChequeTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals(false, module.AllowView);
				AssertEquals(false, module.AllowEdit);
				AssertEquals(false, module.AllowNew);
				AssertEquals(false, module.AllowDelete);
			}
		}

		public void TestMenuItems()
		{
			using (var module = (ChequeTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var viewMenuItem = module.FormActionMenu.FindByText("View") as ZMenuItem;

				AssertNull(viewMenuItem);
			}
		}
	}
}
