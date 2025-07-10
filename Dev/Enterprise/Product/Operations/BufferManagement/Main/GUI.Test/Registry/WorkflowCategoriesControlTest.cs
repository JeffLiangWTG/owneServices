using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(WorkflowCategoriesControl))]
	public class WorkflowCategoriesControlTest : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		public void TestParentGridIsAlwaysReadOnly()
		{
			using (var form = new ZForm())
			{
				using (var control = new WorkflowCategoriesControl())
				{
					form.Controls.Add(control);

					control.ReadOnly = true;
					AssertEquals("ParentGrid.ReadOnly", true, control.ParentGrid.ReadOnly);

					control.ReadOnly = false;
					AssertEquals("ParentGrid.ReadOnly", true, control.ParentGrid.ReadOnly);
				}
			}
		}

		public void TestGridsToggleReadOnly()
		{
			using (var form = new ZForm())
			{
				using (var control = new WorkflowCategoriesControl())
				{
					form.Controls.Add(control);

					control.ReadOnly = true;
					AssertEquals(true, control.ChildGrid.ReadOnly);

					control.ReadOnly = false;
					AssertEquals(false, control.ChildGrid.ReadOnly);
				}
			}
		}

		protected override RegistryZUserControl GetNewControl() => new WorkflowCategoriesControl();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
			=> ((WorkflowCategoriesControl)control).ChildGrid.ReadOnly;

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new CategorisedWorkflowCategoriesCollection();
			var categorisedCategories = collection.AddNew();
			var category = categorisedCategories.Categories.AddNew();
			return collection;
		}
	}
}
