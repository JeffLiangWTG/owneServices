using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusBRForeignOperatorProcessTask))]
	sealed class CusBRForeignOperatorProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			var task = (CusBRForeignOperatorProcessTask)((IWorkflowProvider)foreignOperator).WorkflowItems.AddNew();
			AssertEquals(foreignOperator, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			return ((IWorkflowProvider)foreignOperator).WorkflowItems.AddNew();
		}
	}
}
