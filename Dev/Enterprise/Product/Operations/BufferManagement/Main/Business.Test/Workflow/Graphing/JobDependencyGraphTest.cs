using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class JobDependencyGraphTest : BMSTestCaseWithFactory
	{
		public void TestSequence_OfTemplateJobHeader()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			var jobHeader = template.GetJobHeader();
			var workflow1 = BMSTestHelper.CreateWorkflow(template);
			var workflow2 = BMSTestHelper.CreateWorkflow(template);

			var link = BMSTestHelper.CreateDependencyLink(template, workflow1, workflow2);

			Factory.Save();

			AssertEquals(string.Empty, jobHeader.Sequence);
			AssertEquals("1", workflow1.Sequence);
			AssertEquals("2", workflow2.Sequence);
		}

		public void TestProcessHeaderOrder_NoDependencies()
		{
			var jobHeader = CreateProcessJobHeader(4);
			AssertEquals("1, 2, 3, 4", GetOrder(jobHeader));
		}

		public void TestProcessHeaderOrder_Sequence()
		{
			var jobHeader = CreateProcessJobHeader(4);
			AddLinks(jobHeader, Tuple.Create(3, 2), Tuple.Create(2, 1), Tuple.Create(1, 0));
			AssertEquals("4, 3, 2, 1", GetOrder(jobHeader));
		}

		public void TestProcessHeaderOrder_MultiRoot()
		{
			var jobHeader = CreateProcessJobHeader(7);
			AddLinks(jobHeader, Tuple.Create(0, 2), Tuple.Create(1, 2), Tuple.Create(1, 3), Tuple.Create(2, 4), Tuple.Create(2, 5), Tuple.Create(3, 6));
			AssertEquals("1, 2, 3, 4, 5, 6, 7", GetOrder(jobHeader));
		}

		public void TestProcessHeaderOrder_MultiRoot_2()
		{
			var jobHeader = CreateProcessJobHeader(5);
			AddLinks(jobHeader, Tuple.Create(0, 1), Tuple.Create(2, 0), Tuple.Create(4, 0), Tuple.Create(4, 3));
			AssertEquals("3, 5, 1, 4, 2", GetOrder(jobHeader));
		}

		public void TestProcessHeaderOrder_MultiRoot_3()
		{
			var jobHeader = CreateProcessJobHeader(5);
			AddLinks(jobHeader, Tuple.Create(1, 0), Tuple.Create(0, 2), Tuple.Create(0, 4), Tuple.Create(3, 4));
			AssertEquals("2, 4, 1, 3, 5", GetOrder(jobHeader));
		}

		public void TestProcessHeaderOrder_Cyclic()
		{
			var jobHeader = CreateProcessJobHeader(3);
			AddLinks(jobHeader, Tuple.Create(0, 1), Tuple.Create(1, 2), Tuple.Create(2, 0));
			AssertEquals(string.Empty, GetOrder(jobHeader));
		}

		public void TestProcessHeaderOrder_IgnoreNonDependecies()
		{
			var jobHeader = CreateProcessJobHeader(3);
			AddLinks(jobHeader, Tuple.Create(2, 1), Tuple.Create(1, 0));
			CreateLink(jobHeader.ProcessHeaders[0].PK, jobHeader.ProcessHeaders[2].PK, "###");
			AssertEquals("3, 2, 1", GetOrder(jobHeader));
		}

		public void TestProcessHeaderOrder_IgnoreExternalDependecies()
		{
			var jobHeader1 = CreateProcessJobHeader(2);
			var jobHeader2 = CreateProcessJobHeader(2);
			AddLinks(jobHeader1, Tuple.Create(1, 0));
			AddLinks(jobHeader2, Tuple.Create(1, 0));
			CreateLink(jobHeader1.ProcessHeaders[0].PK, jobHeader2.ProcessHeaders[1].PK);
			CreateLink(jobHeader2.ProcessHeaders[0].PK, jobHeader1.ProcessHeaders[1].PK);
			AssertEquals("2, 1", GetOrder(jobHeader1));
			AssertEquals("2, 1", GetOrder(jobHeader2));
		}

		public void TestProcessHeaderOrder_IgnoreDuplicates()
		{
			var jobHeader = CreateProcessJobHeader(3);
			AddLinks(jobHeader, Tuple.Create(2, 1), Tuple.Create(1, 0), Tuple.Create(2, 1));
			AssertEquals("3, 2, 1", GetOrder(jobHeader));
		}

		public void TestProcessHeaderOrder_Order()
		{
			var jobHeader = CreateProcessJobHeader(6);
			for (int i = 0; i < jobHeader.ProcessHeaders.Count; i++)
			{
				jobHeader.ProcessHeaders[i].FH_CompletionStatement = ((char)('A' + jobHeader.ProcessHeaders.Count - i - 1)).ToString();
			}
			AddLinks(jobHeader, Tuple.Create(0, 1), Tuple.Create(0, 2), Tuple.Create(3, 4), Tuple.Create(3, 5));
			AssertEquals("C, F, A, B, D, E", GetOrder(jobHeader));
		}

		public void TestRefresh()
		{
			var jobHeader = CreateProcessJobHeader(2);
			var graph = JobDependencyGraph.Create(jobHeader);
			AssertEquals("1, 2", GetOrder(jobHeader, graph));
			AddLinks(jobHeader, Tuple.Create(1, 0));
			AssertEquals("1, 2", GetOrder(jobHeader, graph));
			graph.Refresh();
			AssertEquals("2, 1", GetOrder(jobHeader, graph));
		}

		string GetOrder(ProcessJobHeader jobHeader, JobDependencyGraph graph = null)
		{
			if (graph == null)
			{
				graph = JobDependencyGraph.Create(jobHeader);
			}

			var result =
				from processHeader in jobHeader.ProcessHeaders.Cast<ProcessHeader>()
				let sequence = graph.GetSequence(processHeader)
				where !string.IsNullOrEmpty(sequence)
				orderby sequence
				select processHeader.FH_CompletionStatement;
			return string.Join(", ", result);
		}

		ProcessJobHeader CreateProcessJobHeader(int numOfProcessHeaders)
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			jobHeader.ProcessHeaders[0].FH_CompletionStatement = "1";
			for (int i = 1; i < numOfProcessHeaders; i++)
			{
				jobHeader.ProcessHeaders.AddNew().FH_CompletionStatement = (i + 1).ToString();
			}

			return jobHeader;
		}

		void AddLinks(ProcessJobHeader jobHeader, params Tuple<int, int>[] dependencies)
		{
			foreach (var dependency in dependencies)
			{
				CreateLink(jobHeader.ProcessHeaders[dependency.Item1].PK, jobHeader.ProcessHeaders[dependency.Item2].PK);
			}
		}

		ProcessHeaderLink CreateLink(ZGuid headerFrom, ZGuid headerTo, string linkType = "DEP")
		{
			var link = Factory.New<ProcessHeaderLink>();
			link.FP_FH_HeaderFrom = headerFrom;
			link.FP_FH_HeaderTo = headerTo;
			link.FP_LinkType = linkType;

			return link;
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
