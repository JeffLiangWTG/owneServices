using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class GLBudgetFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			GLBudgetCollection budgets = new GLBudgetCollection(Factory);
			GLBudgetFilterBusinessObject filterBO = new GLBudgetFilterBusinessObject();

			using (ZForm form = new ZForm())
			{
				GLBudgetFilterControl filterControl = new GLBudgetFilterControl(budgets, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}
	}
}
