using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(PrintJobController))]
	sealed class ControllerTest : Enterprise.ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			TestPrintJobController controller = new TestPrintJobController();
			AssertEquals("CheckPointForView", Env.Security.PrintJobs, controller.CheckPointForView);
			AssertEquals("CheckPointForEdit", Env.Security.PrintJobs, controller.CheckPointForEdit);
			AssertEquals("CheckPointForNew", Env.Security.PrintJobs, controller.CheckPointForNew);
			AssertEquals("CheckPointForDelete", Env.Security.PrintJobs, controller.CheckPointForDelete);
		}

		public void TestCantFormViewPrintJobs()
		{
			var job = Factory.New<StmPrintJob>();
			var controller = new TestPrintJobController();

			controller.ShowViewForm(job);
			AssertNull("No form should be shown for view", controller.LastShownForm);
		}

		public override void TestViewForm()
		{
			AssertNull("ViewForm should be null", Controller.ShowViewForm(GetBusinessObjectThatIsInTheDatabase()));
		}

		public override void TestEditForm()
		{
			AssertNull("EditForm should be null", Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()));
		}

		#region Test Classes

		class TestPrintJobController : PrintJobController
		{
			public new SecurityCheckpoint CheckPointForView
			{
				get { return base.CheckPointForView; }
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get { return base.CheckPointForEdit; }
			}

			public new SecurityCheckpoint CheckPointForNew
			{
				get { return base.CheckPointForNew; }
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get { return base.CheckPointForDelete; }
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject printJob = Factory.New(typeof(StmPrintJob));
			Factory.Save();
			return printJob;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PrintJob;
		}

		#endregion
	}
}
