using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WorkflowManagerTaskTypesControl))]
	sealed class WorkflowManagerTaskTypesControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		public void TestParentGridIsAlwaysReadOnly()
		{
			using (ZForm form = new ZForm())
			{
				using (WorkflowManagerTaskTypesControl control = new WorkflowManagerTaskTypesControl())
				{
					form.Controls.Add(control);

					control.ReadOnly = true;
					AssertEquals("ParentGrid.ReadOnly", true, control.ParentGridInternal.ReadOnly);

					control.ReadOnly = false;
					AssertEquals("ParentGrid.ReadOnly", true, control.ParentGridInternal.ReadOnly);
				}
			}
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new WorkflowManagerTaskTypesControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((WorkflowManagerTaskTypesControl)control).ChildGridInternal.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes categorisedTaskTypes = collection.AddNew();
			WorkflowTaskType taskType = categorisedTaskTypes.TaskTypes.AddNew();
			return collection;
		}
	}
}
