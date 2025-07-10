using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class VisualBoardTestConfig : SchematicTestConfig
	{
		protected VisualBoardTestConfig(BusinessObjectFactory factory, string[] workflowTypes, string systemName, bool shouldUseExistingSystem)
			: base(factory, workflowTypes, systemName, shouldUseExistingSystem)
		{
		}

		internal new static VisualBoardTestConfig Create(BusinessObjectFactory factory, string[] workflowTypes, string systemName, bool shouldUseExistingSystem)
		{
			return new VisualBoardTestConfig(factory, workflowTypes, systemName, shouldUseExistingSystem);
		}

		void InitBufferBoard()
		{
			bufferBoard = BMSTestHelper.CreateBoard(System, "Buffer Board");
			bufferSection = BMSTestHelper.CreateBoardSection(Buffer, bufferBoard);

			bufferSection.SectionConfiguration.CellsPerSubsection = 13;
			bufferSection.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
		}

		void InitBucketBoard()
		{
			bucketBoard = BMSTestHelper.CreateBoard(System, "Bucket Board");
			bucketSection = BMSTestHelper.CreateBoardSection(Bucket, bucketBoard);
		}

		public BMBoard BufferBoard
		{
			get
			{
				if (bufferBoard == null)
				{
					InitBufferBoard();
				}
				return bufferBoard;
			}
		}

		public BMBoardSection BufferSection
		{
			get
			{
				if (bufferBoard == null)
				{
					InitBufferBoard();
				}
				return bufferSection;
			}
		}

		public BMBoard BucketBoard
		{
			get
			{
				if (bucketBoard == null)
				{
					InitBucketBoard();
				}
				return bucketBoard;
			}
		}

		public BMBoardSection BucketSection
		{
			get
			{
				if (bucketBoard == null)
				{
					InitBucketBoard();
				}
				return bucketSection;
			}
		}

		BMBoardSection bufferSection;
		BMBoard bufferBoard;
		BMBoardSection bucketSection;
		BMBoard bucketBoard;
	}
}
