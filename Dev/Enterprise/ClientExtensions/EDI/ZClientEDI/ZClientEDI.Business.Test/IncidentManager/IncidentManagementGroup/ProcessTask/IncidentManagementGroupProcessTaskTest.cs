using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementGroupProcessTask))]
	public class IncidentManagementGroupProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentType()
		{
			CachedTask.P9_ParentID = Factory.New<IncidentManagementGroup>().PK;
			AssertEquals(typeof(IncidentManagementGroup), CachedTask.Parent.GetType());
		}

		public void TestParentControllerID()
		{
			AssertEquals(ClientControllerRegistration.IncidentManagementGroup, CachedTask.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var incident = Factory.New<IncidentManagementGroup>();
			return incident.WorkflowItems.AddNew();
		}

		ProcessTask CachedTask
		{
			get { return (ProcessTask)CachedBusinessObject; }
		}
	}
}
