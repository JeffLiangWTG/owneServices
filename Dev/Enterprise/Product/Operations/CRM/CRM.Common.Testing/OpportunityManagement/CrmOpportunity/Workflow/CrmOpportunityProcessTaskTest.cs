using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(CrmOpportunityProcessTasks))]
	sealed class CrmOpportunityProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			var task = (CrmOpportunityProcessTasks)GetNewBusinessObject();
			AssertNull("Doesn't have a controller ID", task.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = Factory.NewWithValidTestData<CrmOpportunity>();
			opp.COP_OH_Organization = org.PK;
			return opp.WorkflowItems.AddNew();
		}
	}
}
