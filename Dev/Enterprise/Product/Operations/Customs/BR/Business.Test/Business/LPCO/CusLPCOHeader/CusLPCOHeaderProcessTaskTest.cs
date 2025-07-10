using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusLPCOHeaderProcessTask))]
	sealed class CusLPCOHeaderProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			var lpcoHeader = Factory.New<CusLPCOHeader>();
			var task = (CusLPCOHeaderProcessTask)((IWorkflowProvider)lpcoHeader).WorkflowItems.AddNew();
			AssertEquals(lpcoHeader, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var lpcoHeader = Factory.New<CusLPCOHeader>();
			return ((IWorkflowProvider)lpcoHeader).WorkflowItems.AddNew();
		}
	}
}
