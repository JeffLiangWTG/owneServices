using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Workflow.ProcessHeader;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test.Workflow.ProcessHeader
{
	public class ProcessHeaderUniversalCopyCollectionSorterTest : TestCaseWithFactory
	{
		public void TestGetSortedCollection_SortsProcessHeadersByParentListingJobHeaderFirst()
		{
			// Arrange
			VisualBoards.Business.Test.VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSRegistry.Instance.WorkflowManagementMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);

			var job1PK = Guid.Parse("00000000-0000-0000-0000-000000000001");
			var job1Header = Factory.NewWithValidTestData<ProcessJobHeader>();
			job1Header.FH_ParentId = job1PK;
			job1Header.FH_Category = "JOB";
			var job1Workflow1 = CreateProcessHeader(job1Header);
			var job1Workflow2 = CreateProcessHeader(job1Header);

			var job2PK = Guid.Parse("00000000-0000-0000-0000-000000000002");
			var job2Header = Factory.NewWithValidTestData<ProcessJobHeader>();
			job2Header.FH_ParentId = job2PK;
			job2Header.FH_Category = "JOB";
			var job2Workflow1 = CreateProcessHeader(job2Header);
			var job2Workflow2 = CreateProcessHeader(job2Header);

			var processHeaders = new[]
			{
				job1Workflow1,
				job2Workflow1,
				job2Workflow2,
				job1Workflow2,
				job1Header,
				job2Header
			};

			var sorter = new ProcessHeaderUniversalCopyCollectionSorter();

			// Act
			var resultHeaders = sorter.GetSortedCollection(processHeaders).Cast<Business.ProcessHeader>().ToList();

			// Assert
			AssertEquals(6, resultHeaders.Count);

			AssertEquals(job1Header, resultHeaders[0]);
			Assert("Assert second is either job1Workflow1 or job1Workflow2", new[] { job1Workflow1, job1Workflow2 }.Contains(resultHeaders[1]));
			Assert("Assert third is either job1Workflow1 or job1Workflow2", new[] { job1Workflow1, job1Workflow2 }.Contains(resultHeaders[2]));
			AssertEquals(job2Header, resultHeaders[3]);
			Assert("Assert fifth is either job2Workflow1 or job2Workflow2", new[] { job2Workflow1, job2Workflow2 }.Contains(resultHeaders[4]));
			Assert("Assert sixth is either job2Workflow1 or job2Workflow2", new[] { job2Workflow1, job2Workflow2 }.Contains(resultHeaders[5]));
		}

		Business.ProcessHeader CreateProcessHeader(ProcessJobHeader jobHeader)
		{
			var processHeader = Factory.NewWithValidTestData<Business.ProcessHeader>();
			processHeader.FH_FH_ParentHeader = jobHeader.PK;
			processHeader.FH_ParentId = jobHeader.FH_ParentId;
			processHeader.FH_ParentTableCode = jobHeader.FH_ParentTableCode;
			processHeader.FH_Category = "DEV";
			return processHeader;
		}
	}
}
