using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentTriageProcessTask))]
	public class IncidentTriageProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			var incidentTriage = Factory.New<IncidentTriage>();
			var task = (IncidentTriageProcessTask)((IWorkflowProvider)incidentTriage).WorkflowItems.AddNew();
			AssertEquals(incidentTriage, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var incident = Factory.New<IncidentTriage>();
			return ((IWorkflowProvider)incident).WorkflowItems.AddNew();
		}
	}
}
