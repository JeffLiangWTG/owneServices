using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	sealed class PrintJobFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			PrintJobFilterBusinessObject filterBizObj = new PrintJobFilterBusinessObject();

			using (ZForm form = new ZForm())
			using (PrintJobFilterControl filterControl = new PrintJobFilterControl(new StmPrintJobCollection(Factory), filterBizObj))
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}
	}
}
