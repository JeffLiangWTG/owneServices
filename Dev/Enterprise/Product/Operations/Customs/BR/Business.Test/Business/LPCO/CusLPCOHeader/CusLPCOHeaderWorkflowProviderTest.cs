using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusLPCOHeader))]
	public class CusLPCOHeaderWorkflowProviderTest : WorkflowProviderTest<CusLPCOHeader, ProcessTaskCollection<CusLPCOHeaderProcessTask, CusLPCOHeader>>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CusBRLPCOHeaderWorkflowDescriptorCode;

		public void TestWorkflows()
		{
			var cusLPCOHeader = GetNewBusinessObject(Factory);
			AssertEquals("Enterprise.BufferManagement.Business.ProcessHeaderCollection", cusLPCOHeader.Workflows.GetType().FullName);
		}

		public void TestWorkflowItems()
		{
			var cusLPCOHeader = GetNewBusinessObject(Factory);
			AssertType<ProcessTaskCollection<CusLPCOHeaderProcessTask, CusLPCOHeader>>(cusLPCOHeader.WorkflowItems);
			var workflowItems = cusLPCOHeader.WorkflowItems.AddNew();
			cusLPCOHeader.Delete();
			Assert("WorkflowItems should be deleted", workflowItems.IsDeleted);
		}

		public void TestGetWorkflowInformationProvider()
		{
			var cusLPCOHeader = GetNewBusinessObject(Factory);
			AssertNull(cusLPCOHeader.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var cusLPCOHeader = GetNewBusinessObject(Factory);
			cusLPCOHeader.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;

			var ranker = (ColumnValueRanker)((IWorkflowProvider)cusLPCOHeader).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { cusLPCOHeader.CPH_OH_PermitHolder, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}
	}
}
