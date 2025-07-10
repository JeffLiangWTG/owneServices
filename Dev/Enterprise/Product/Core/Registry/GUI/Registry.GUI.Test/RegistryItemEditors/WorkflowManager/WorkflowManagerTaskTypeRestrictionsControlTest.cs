using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WorkflowManagerTaskTypeRestrictionsControl))]
	sealed class WorkflowManagerTaskTypeRestrictionsControlTest : RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl()
		{
			return new WorkflowManagerTaskTypeRestrictionsControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((WorkflowManagerTaskTypeRestrictionsControl)control).ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new TaskTypeRestrictionsCollection();
			var restrictions = collection.AddNew();
			var taskType = restrictions.TaskTypesCollection.AddNew();

			return collection;
		}
	}
}
