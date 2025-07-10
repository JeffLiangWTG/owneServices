using System.Windows.Forms;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLBudgetModule))]
	class GLBudgetModuleTest : ZModuleBasherTest
	{
		public override Form GetFormToBash()
		{
			return new GLBudgetForm(Factory.New<GLBudget>());
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GLBudget;
		}
	}
}
