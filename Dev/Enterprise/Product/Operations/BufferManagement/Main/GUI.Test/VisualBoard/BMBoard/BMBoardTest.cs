using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMBoard))]
	public class BMBoardTest : EnterpriseBusinessObjectTestCase
	{
		#region Clone

		public void TestCloneBMBoard()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DQU", "Douglas Quaid");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = staff.GS_Code;
			var buffer1 = BMSTestHelper.CreateBuffer(system);
			var buffer2 = BMSTestHelper.CreateBuffer(system);
			var bucket1 = BMSTestHelper.CreateBucket(system);
			var bucket2 = BMSTestHelper.CreateBucket(system);
			var section1 = BMSTestHelper.CreateBoardSection(buffer1, board);
			section1.Row = 2;
			var section2 = BMSTestHelper.CreateBoardSection(buffer2, board);
			var section3 = BMSTestHelper.CreateBoardSection(bucket1, board);
			var section4 = BMSTestHelper.CreateBoardSection(bucket2, board);

			FilterStripsTestHelper.AddStartsWithFilter(section1.WorkflowFilter, "Completion Statement", "I hate every ape I see...");
			FilterStripsTestHelper.AddStartsWithFilter(section1.TaskFilter, "Description", "...from chimpan-A to chimpanzee.");

			var clone = (BMBoard)board.Clone();

			AssertEquals(board.Sections.Count, clone.Sections.Count);
			AssertNotEquals(board.PK, clone.PK);
			AssertEquals(staff.GS_Code, clone.MB_GS_NKStaffCode);

			var clonedSection1 = clone.Sections.Single(x => x.Row == 2);
			BMSTestHelper.AssertModuleFilterDeepClone(section1.WorkflowFilter, clonedSection1.WorkflowFilter, clonedSection1);
			BMSTestHelper.AssertModuleFilterDeepClone(section1.TaskFilter, clonedSection1.TaskFilter, clonedSection1);

			var clone2 = (BMBoard)board.TemplateCopy();

			AssertEquals("BMBoardSupports Template Copy and it calls clone", board.Sections.Count, clone2.Sections.Count);
			AssertNotEquals(board.PK, clone2.PK);
			AssertEquals(staff.GS_Code, clone2.MB_GS_NKStaffCode);
		}

		#endregion
	}
}
