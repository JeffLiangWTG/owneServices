using System.Windows.Forms;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(GLBudgetForm))]
	public class GLBudgetFormTest : ZFormBasherTest
	{
		public void TestAuditPluginIsAdded()
		{
			using (var form = (GLBudgetForm)GetFormToBashCore())
			{
				AssertNotNull("GLBudget form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		protected override Form GetFormToBashCore()
		{
			GLBudget budget = Factory.New<GLBudget>();
			return new GLBudgetForm(budget);
		}
	}
}
