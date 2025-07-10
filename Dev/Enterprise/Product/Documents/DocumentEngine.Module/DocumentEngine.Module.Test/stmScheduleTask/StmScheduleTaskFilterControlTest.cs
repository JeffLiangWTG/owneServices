using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	sealed class StmScheduleTaskFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			StmScheduleTaskFilterBusinessObject filterBizObj = new StmScheduleTaskFilterBusinessObject();

			using (ZForm form = new ZForm())
			using (StmScheduleTaskFilterControl filterControl = new StmScheduleTaskFilterControl(new StmScheduleTaskCollection(Factory, DummyBizoSchema.Constants.Prefix), filterBizObj))
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}
	}
}
