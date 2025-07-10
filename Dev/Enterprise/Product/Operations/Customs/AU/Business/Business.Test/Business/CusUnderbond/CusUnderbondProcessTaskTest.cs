using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondProcessTask))]
	class CusUnderbondProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			CusUnderbond cusUnderbond = Factory.New<CusUnderbond>();
			CusUnderbondProcessTask task = (CusUnderbondProcessTask)((IWorkflowProvider)cusUnderbond).WorkflowItems.AddNew();
			AssertEquals(cusUnderbond, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CusUnderbond cusUnderbond = Factory.New<CusUnderbond>();
			return ((IWorkflowProvider)cusUnderbond).WorkflowItems.AddNew();
		}
	}
}
