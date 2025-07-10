using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class NettingPeriodFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var periods = new NettingSystemPeriodCollection(Factory);
			var filterBO = new NettingPeriodFilterBusinessObject();

			using (ZForm form = new ZForm())
			{
				GLBudgetFilterControl filterControl = new GLBudgetFilterControl(periods, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}
	}
}
