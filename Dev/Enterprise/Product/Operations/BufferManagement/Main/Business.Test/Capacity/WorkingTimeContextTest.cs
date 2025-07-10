using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkingTimeContextTest : BMSTestCaseWithFactory
	{
		#region Create

		public void TestCreate()
		{
			object context = null;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var system = CreateSystem();
			var buffer = CreateBuffer(system);
			AssertNotNull(buffer);
			AssertNoExceptionThrown(delegate
			{ context = WorkingTimeContext.Create(buffer); });
			AssertNotNull(context);
			AssertNoExceptionThrown(delegate
			{ context = WorkingTimeContext.Create(buffer, resource); });
			AssertNotNull(context);

			var board = system.Boards.AddNew();
			var section = CreateBoardSection(buffer, board);
			AssertNotNull(section);
			AssertNoExceptionThrown(delegate
			{ context = WorkingTimeContext.Create(section); });
			AssertNotNull(context);
			AssertNoExceptionThrown(delegate
			{ context = WorkingTimeContext.Create(section, resource); });
			AssertNotNull(context);

			section.MS_MB_Board = ZGuid.Empty;
			AssertNull(section.Board);
			AssertNoExceptionThrown(delegate
			{ context = WorkingTimeContext.Create(section); });
			AssertNotNull(context);
			AssertNoExceptionThrown(delegate
			{ context = WorkingTimeContext.Create(section, resource); });
			AssertNotNull(context);

			section.MS_FC_Component = ZGuid.Empty;
			AssertNull(section.Component);
			AssertNoExceptionThrown(delegate
			{ context = WorkingTimeContext.Create(section, factory: Factory); });
			AssertNotNull(context);
			AssertNoExceptionThrown(delegate
			{ context = WorkingTimeContext.Create(section, resource); });
			AssertNotNull(context);

			BMBoardSection nullSection = null;
			AssertExceptionThrown<NullReferenceException>(delegate
			{ context = WorkingTimeContext.Create(nullSection, factory: Factory); });
			AssertExceptionThrown<NullReferenceException>(delegate
			{ context = WorkingTimeContext.Create(nullSection, resource); });

			AssertEquals(2, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear(); // all the null branches and departments will have created error reports (tested separately) but not thrown exceptions
		}

		public void TestCreateWillNotAccessGlbDepartmentAndGlbBranch()
		{
			WorkingTimeContext test1 = null;
			WorkingTimeContext test2 = null;
			AssertNotNull(GlbBranch.CurrentBranch);
			AssertNotNull(GlbDepartment.CurrentDepartment);

			Factory.ThreadSentry.RelinquishThreadOwnership();

			var thread = new System.Threading.Thread(() =>
			{
				using (CargoWise.Data.Db.DisposableActionForDbConnection())
				{
					Factory.ThreadSentry.TakeThreadOwnership();

					test1 = WorkingTimeContext.Create(new CargoWise.EntityFramework.BusinessObjectFactory());
					test2 = WorkingTimeContext.Create(new CargoWise.EntityFramework.BusinessObjectFactory());
				}
			});

			thread.Start();
			thread.Join();

			AssertNotEquals("New factory should have been created for each WorkingTimeContext", test1.Branch.Factory, test2.Branch.Factory);
			AssertNotEquals("New factory should have been created for each WorkingTimeContext", test1.Department.Factory, test2.Department.Factory);
		}

		public void TestCreateWithBranchDepartmentProvider_WhenNullBranchAndDepartmentProvided_ShouldUseCurrentBranchAndDepartmentAndReportError()
		{
			var provider = new DummyBranchDepartmentProvider(null, null);
			AssertNull(BMSTestHelper.GetBranch(provider, Factory));
			AssertNull(BMSTestHelper.GetDepartment(provider, Factory));
			var context = WorkingTimeContext.Create(provider, factory: Factory);

			AssertSamePK("The current branch should have been used as a fallback. SAD!", GlbBranch.CurrentBranch, context.Branch);
			AssertSamePK("The current department should have been used as a fallback. SAD!", GlbDepartment.CurrentDepartment, context.Department);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("WorkingTimeContext was initialised with a provider that provided a null branch and a null department. Provider type: [DummyBranchDepartmentProvider].", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCreateWithBranchDepartmentProvider_WhenNullDepartmentProvided_ShouldUseCurrentDepartmentAndReportError()
		{
			var branch = Factory.New<GlbBranch>();
			var provider = new DummyBranchDepartmentProvider(branch, null);
			var context = WorkingTimeContext.Create(provider, factory: Factory);

			AssertSamePK(branch, context.Branch);
			AssertSamePK(GlbDepartment.CurrentDepartment, context.Department);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("WorkingTimeContext was initialised with a provider that provided a null department. Provider type: [DummyBranchDepartmentProvider].", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCreateWithBranchDepartmentProvider_WhenNullBranchProvided_ShouldUseCurrentBranchAndReportError()
		{
			var department = Factory.New<GlbDepartment>();
			var provider = new DummyBranchDepartmentProvider(null, department);
			var context = WorkingTimeContext.Create(provider, factory: Factory);

			AssertSamePK(GlbBranch.CurrentBranch, context.Branch);
			AssertSamePK(department, context.Department);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("WorkingTimeContext was initialised with a provider that provided a null branch. Provider type: [DummyBranchDepartmentProvider].", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCreateWithBranchDepartmentProvider_WhenBranchAndDepartmentProvided_ShouldUseProvidedBranchAndDepartment_AndNotReportError()
		{
			var branch = Factory.New<GlbBranch>();
			var department = Factory.New<GlbDepartment>();
			var provider = new DummyBranchDepartmentProvider(branch, department);
			var context = WorkingTimeContext.Create(provider, factory: Factory);

			AssertSamePK(branch, context.Branch);
			AssertSamePK(department, context.Department);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestCreateWithNullBranchDepartmentStaffAndFactory_ShouldReportErrorButNotNullReferenceException()
		{
			var provider = new DummyBranchDepartmentProvider(null, null, Factory);

			AssertNoExceptionThrown("Initialising with completely null details should not throw an NRE.", () => WorkingTimeContext.Create(provider, null, null));

			AssertEquals("WorkingTimeContext was initialised with a provider that provided a null branch and a null department. Provider type: [DummyBranchDepartmentProvider].", ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestCreateWithNullBranchDepartmentStaffAndFactory_AndProviderHasNoFactory_ShouldThrowException()
		{
			var provider = new DummyBranchDepartmentProvider(null, null, null);

			var ex = AssertExceptionThrown<InvalidOperationException>("Initialising with completely null details and a non-factory provider should throw an IOE.", () => WorkingTimeContext.Create(provider, null, null));
			AssertEquals("The exception should be descriptive so the problem is easy to deal with, rather than the mysterious NRE we were getting before.", "No factory provided to initialise the WorkingTimeContext with. We need a resource, provider, or explicit factory supplied.", ex.Message);
		}

		#endregion
	}
}
