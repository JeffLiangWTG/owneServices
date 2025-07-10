using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(DocumentSigningJobController))]
	class DocumentSigningJobControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new TestDocumentSigningJobController();
			AssertEquals("CheckPointForView", Env.Security.DocumentSigningJobs, controller.CheckPointForView);
			AssertEquals("CheckPointForEdit", Env.Security.DocumentSigningJobs, controller.CheckPointForEdit);
			AssertEquals("CheckPointForNew", Env.Security.DocumentSigningJobs, controller.CheckPointForNew);
			AssertEquals("CheckPointForDelete", Env.Security.DocumentSigningJobs, controller.CheckPointForDelete);
		}

		public void TestCantFormViewDocumentSigningJobs()
		{
			var job = Factory.New<StmPrintJob>();
			var controller = new TestDocumentSigningJobController();

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

		class TestDocumentSigningJobController : DocumentSigningJobController
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
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_SignBy = DocumentsSignBy.DOS;
			Factory.Save();
			return printJob;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocumentSigningJob;
		}

		#endregion
	}
}
