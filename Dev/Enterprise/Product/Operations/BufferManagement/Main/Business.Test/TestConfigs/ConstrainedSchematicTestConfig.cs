using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ConstrainedSchematicTestConfig : SchematicTestConfig
	{
		protected ConstrainedSchematicTestConfig(BusinessObjectFactory factory, string workflowType, bool makeResourcesPartOfReleaseGroup, bool shouldUseExistingSystem)
			: base(factory, new[] { workflowType }, "WTGDEV", shouldUseExistingSystem)
		{
			PreConstraintBuffer = BMSTestHelper.CreateSubBuffer(Buffer, "Pre-Constraint", timespanMinutes: 64 * 60, offsetMinutes: 0);
			Constraint = BMSTestHelper.CreateConstraint(Buffer, "Constraint", offsetMinutes: 64 * 60);
			PostConstraintBuffer = BMSTestHelper.CreateSubBuffer(Buffer, "Post-Constraint", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			ComponentReleaseGroupLink = ConstrainedModeHelper.SwitchToConstrainedMode(ReleaseGroup, Buffer.PK);

			CCR = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "CCR", "CCR");
			NonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "NC1", "Non-CCR1");
			NonCCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "NC2", "Non-CCR2");

			CCR.DesignateAsCCR(Buffer);

			if (makeResourcesPartOfReleaseGroup)
			{
				ReleaseGroup.Staff.AddRange(new[] { CCR, NonCCR1, NonCCR2 });
			}
		}

		internal static ConstrainedSchematicTestConfig Create(BusinessObjectFactory factory, string workflowType, bool makeResourcesPartOfReleaseGroup, bool shouldUseExistingSystem)
		{
			return new ConstrainedSchematicTestConfig(factory, workflowType, makeResourcesPartOfReleaseGroup, shouldUseExistingSystem);
		}

		public BMComponent PreConstraintBuffer { get; }
		public BMComponent Constraint { get; }
		public BMComponent PostConstraintBuffer { get; }

		public BMComponentReleaseGroupLink ComponentReleaseGroupLink { get; }

		public GlbStaff CCR { get; }
		public GlbStaff NonCCR1 { get; }
		public GlbStaff NonCCR2 { get; }

		public BMBoard BufferAndReleaseSchedulerBoard
		{
			get
			{
				if (bufferAndReleaseSchedulerBoard == null)
				{
					InitBoard();
				}

				return bufferAndReleaseSchedulerBoard;
			}
		}

		public BMBoardSection BufferSection
		{
			get
			{
				if (bufferAndReleaseSchedulerBoard == null)
				{
					InitBoard();
				}

				return bufferSection;
			}
		}

		public BMBoardSection ReleaseSchedulerSection
		{
			get
			{
				if (bufferAndReleaseSchedulerBoard == null)
				{
					InitBoard();
				}

				return releaseSchedulerSection;
			}
		}

		void InitBoard()
		{
			bufferAndReleaseSchedulerBoard = BMSTestHelper.CreateBoard(System);
			bufferSection = BMSTestHelper.CreateBoardSection(Buffer, bufferAndReleaseSchedulerBoard, col: 1);
			releaseSchedulerSection = BMSTestHelper.CreateReleaseSchedulerBoardSection(Buffer, ReleaseGroup, bufferAndReleaseSchedulerBoard);

			bufferSection.SectionConfiguration.CellsPerSubsection = 13;
			bufferSection.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
		}

		BMBoard bufferAndReleaseSchedulerBoard;
		BMBoardSection bufferSection;
		BMBoardSection releaseSchedulerSection;
	}
}
