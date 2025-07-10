using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class CapacityContextTest : BMSTestCaseWithFactory
	{
		public void TestCreate()
		{
			var branch_board = Factory.NewWithValidTestData<GlbBranch>();
			var branch_component = Factory.NewWithValidTestData<GlbBranch>();
			var branch_global = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			var department_board = Factory.NewWithValidTestData<GlbDepartment>();
			var department_component = Factory.NewWithValidTestData<GlbDepartment>();
			var department_global = Factory.Load<GlbDepartment>(Env.CurrentDepartment.PK);

			var system = CreateSystem();
			var buffer = CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(buffer, board);

			board.MB_GB_AgingBranch = branch_board.PK;
			board.MB_GE_AgingDepartment = department_board.PK;

			buffer.FC_GB_AgingBranch = branch_component.PK;
			buffer.FC_GE_AgingDepartment = department_component.PK;

			var boardContext = WorkingTimeContext.Create(section);
			AssertEquals(branch_board, boardContext.Branch);
			AssertEquals(department_board, boardContext.Department);

			var componentContext = WorkingTimeContext.Create(buffer);
			AssertEquals(branch_component, componentContext.Branch);
			AssertEquals(department_component, componentContext.Department);

			var globalContext = WorkingTimeContext.Create(Factory);
			AssertEquals(branch_global.PK, globalContext.Branch.PK);
			AssertEquals(department_global.PK, globalContext.Department.PK);
		}

		public void TestCreate_FallbackToComponent()
		{
			var branch_component = Factory.NewWithValidTestData<GlbBranch>();
			var branch_global = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			var department_component = Factory.NewWithValidTestData<GlbDepartment>();
			var department_global = Factory.Load<GlbDepartment>(Env.CurrentDepartment.PK);

			var system = CreateSystem();
			var buffer = CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(buffer, board);

			buffer.FC_GB_AgingBranch = branch_component.PK;
			buffer.FC_GE_AgingDepartment = department_component.PK;

			var boardContext = WorkingTimeContext.Create(section);
			AssertEquals(branch_component, boardContext.Branch);
			AssertEquals(department_component, boardContext.Department);

			var componentContext = WorkingTimeContext.Create(buffer);
			AssertEquals(branch_component, componentContext.Branch);
			AssertEquals(department_component, componentContext.Department);

			var globalContext = WorkingTimeContext.Create(Factory);
			AssertEquals(branch_global.PK, globalContext.Branch.PK);
			AssertEquals(department_global.PK, globalContext.Department.PK);
		}

		public void TestCreate_FallbackToGlobal()
		{
			var branch_global = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			var department_global = Factory.Load<GlbDepartment>(Env.CurrentDepartment.PK);

			var system = CreateSystem();
			var buffer = CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(buffer, board);

			var boardContext = WorkingTimeContext.Create(section);
			AssertEquals(branch_global.PK, boardContext.Branch.PK);
			AssertEquals(department_global.PK, boardContext.Department.PK);

			var componentContext = WorkingTimeContext.Create(buffer);
			AssertEquals(branch_global.PK, componentContext.Branch.PK);
			AssertEquals(department_global.PK, componentContext.Department.PK);

			var globalContext = WorkingTimeContext.Create(Factory);
			AssertEquals(branch_global.PK, globalContext.Branch.PK);
			AssertEquals(department_global.PK, globalContext.Department.PK);
		}
	}
}
