using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WorkflowManagerIterationReasonsControl))]
	sealed class WorkflowManagerIterationReasonsControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		public void TestParentGridIsAlwaysReadOnly()
		{
			using (var form = new ZForm())
			{
				using (var control = new WorkflowManagerIterationReasonsControl())
				{
					form.Controls.Add(control);

					control.ReadOnly = true;
					AssertEquals("ParentGrid.ReadOnly", true, control.ParentGridInternal.ReadOnly);

					control.ReadOnly = false;
					AssertEquals("ParentGrid.ReadOnly", true, control.ParentGridInternal.ReadOnly);
				}
			}
		}

		public void TestGridsToggleReadOnly()
		{
			using (var form = new ZForm())
			{
				using (var control = new WorkflowManagerIterationReasonsControl())
				{
					form.Controls.Add(control);

					control.ReadOnly = true;
					AssertEquals(true, control.ChildGridInternal.ReadOnly);
					AssertEquals(true, control.ValidationDropEditInternal.ReadOnly);

					control.ReadOnly = false;
					AssertEquals(false, control.ChildGridInternal.ReadOnly);
					AssertEquals(false, control.ValidationDropEditInternal.ReadOnly);
				}
			}
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new WorkflowManagerIterationReasonsControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((WorkflowManagerIterationReasonsControl)control).ChildGridInternal.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new CategorisedWorkflowIterationReasonsCollection();
			var categorisedIterationReasons = collection.AddNew();
			var iterationReason = categorisedIterationReasons.IterationReasons.AddNew();
			return collection;
		}
	}
}
