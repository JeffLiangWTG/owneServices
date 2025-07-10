using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(JobAutoPopulationForm))]
	public class JobAutoPopulationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			Job job = Factory.NewJobForTesting<Job>();
			return new JobAutoPopulationForm(job);
		}

		public void TestMenuItemMakeInactiveIsInvisible()
		{
			var job = Factory.NewJobForTesting<Job>();
			AssertEquals("Job is active", true, job.JH_IsActive);

			using (var form = new JobAutoPopulationForm(job))
			{
				var makeInactiveMenuItem = form.Menu.MenuItems.FindByName(ZFormMenuStrategy.MakeInactiveName, true);

				AssertEquals("MenuItem 'Make Inactive' should be invisible", false, makeInactiveMenuItem.Visible);
			}
		}

		public void TestMenuItemMakeActiveIsInvisible()
		{
			var job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "001";
			Factory.Save();

			job.MarkAsInactive();
			AssertEquals("Job is inactive", false, job.JH_IsActive);

			Factory.Save();

			using (var form = new JobAutoPopulationForm(job))
			{
				var makeActiveMenuItem = form.Menu.MenuItems.FindByName(ZFormMenuStrategy.MakeActiveName, true);

				AssertEquals("MenuItem 'Make Active' should be invisible", false, makeActiveMenuItem.Visible);
			}
		}
	}
}
