using System;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(DocumentSigningJobFilterBusinessObject))]
	class DocumentSigningJobFilterBusinessObjectTest : PrintJobFilterBusinessObjectTest
	{
		public void TestShouldShowOwn()
		{
			var initialUserContext = Env.CurrentUserContext;

			try
			{
				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_Code = "A.G";
				staff1.GS_LoginName = "Anton";

				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				staff2.GS_Code = "JSF";
				staff2.GS_LoginName = "Jason";

				Factory.Save();

				StmPrintJob job1, job2, job3, job4;

				using (Env.Instance.SetTemporaryUserContext(staff1.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					job1 = GetNewJob();
					job2 = GetNewJob();
					Factory.Save();
				}

				using (Env.Instance.SetTemporaryUserContext(staff2.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					job3 = GetNewJob();
					job4 = GetNewJob();
					Factory.Save();
				}

				using (Env.Instance.SetTemporaryUserContext(staff1.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					var filter = GetNewFilterStripBusinessObject();

					var collection = new StmPrintJobCollection(Factory);
					collection.Load(filter.Filter);

					AssertContainsExactElementsInAnyOrder(new[] { job1, job2 }, collection);
				}

				using (Env.Instance.SetTemporaryUserContext(staff2.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					var filter = GetNewFilterStripBusinessObject();

					var collection = new StmPrintJobCollection(Factory);
					collection.Load(filter.Filter);

					AssertContainsExactElementsInAnyOrder(new[] { job3, job4 }, collection);
				}
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		public void TestShouldShowAllForController()
		{
			var initialUserContext = Env.CurrentUserContext;

			try
			{
				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_Code = "A.G";
				staff1.GS_LoginName = "Anton";

				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				staff2.GS_Code = "JSF";
				staff2.GS_LoginName = "Jason";

				var staff3 = Factory.NewWithValidTestData<GlbStaff>();
				staff3.GS_Code = "~CO";
				staff3.GS_LoginName = "controller";
				staff3.GS_IsController = true;

				Factory.Save();

				StmPrintJob job1, job2, job3, job4;

				using (Env.Instance.SetTemporaryUserContext(staff1.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					job1 = GetNewJob();
					job2 = GetNewJob();
					Factory.Save();
				}

				using (Env.Instance.SetTemporaryUserContext(staff2.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					job3 = GetNewJob();
					job4 = GetNewJob();
					Factory.Save();
				}

				using (Env.Instance.SetTemporaryUserContext(staff3.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					var filter = GetNewFilterStripBusinessObject();

					var collection = new StmPrintJobCollection(Factory);
					collection.Load(filter.Filter);

					AssertContainsExactElementsInAnyOrder(new[] { job1, job2, job3, job4 }, collection);
				}
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		public void TestShouldShowSigningJobsOnly()
		{
			var job1 = GetNewJob();
			var job2 = GetNewJob();
			GetNewJob(DocumentsSignBy.NON);
			GetNewJob(DocumentsSignBy.PFX);

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			var collection = new StmPrintJobCollection(Factory);
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { job1, job2 }, collection);
		}

		#region Overrides
		StmPrintJob GetNewJob(string signBy)
		{
			var job = GetNewJob();
			job.SP_SignBy = signBy;

			return job;
		}

		protected override StmPrintJob GetNewJob()
		{
			var job = Factory.NewWithValidTestData<StmPrintJob>();
			job.SP_SignBy = DocumentsSignBy.DOS;
			job.SP_GS_NKJobSubmittedBy = Env.CurrentUser.Initials;
			return job;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DocumentSigningJobFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		#endregion
	}
}
