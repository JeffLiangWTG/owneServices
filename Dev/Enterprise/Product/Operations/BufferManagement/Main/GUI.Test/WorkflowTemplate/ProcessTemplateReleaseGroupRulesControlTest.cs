
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ProcessTemplateReleaseGroupRulesControlTest : BMSGUITestCase
	{
		public void TestSetApplicableWorkflowCategoriesGridReadOnly()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var rule1 = template.ReleaseGroupRules.AddNew();
			var rule2 = template.ReleaseGroupRules.AddNew();

			using (var control = new ProcessTemplateReleaseGroupRulesControl())
			using (var form = new ZForm(template))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(2, control.RulesGrid.List.Count);
				AssertEquals(true, control.ApplicableWorkflowCategoriesGrid.ReadOnly);

				control.RulesGrid.ListManager.Position = 1;
				AssertEquals(true, control.ApplicableWorkflowCategoriesGrid.ReadOnly);

				control.RulesGrid.ListManager.Position = 2;
				AssertEquals(true, control.ApplicableWorkflowCategoriesGrid.ReadOnly);

				control.RulesGrid.ListManager.Position = 0;
				AssertEquals(2, control.RulesGrid.List.Count);
				AssertEquals(true, control.ApplicableWorkflowCategoriesGrid.ReadOnly);

				var categoriesApplicableIndex = control.RulesGrid.Columns.IndexOf(x => x.ColumnName == ProcessTemplateReleaseGroupRuleSchema.PTR_AreAllWorkflowCategoriesApplicable.Name);
				control.RulesGrid[0, categoriesApplicableIndex] = new ZBool(false);

				AssertEquals(false, control.ApplicableWorkflowCategoriesGrid.ReadOnly);

				control.RulesGrid.ListManager.Position = 1;
				AssertEquals(true, control.ApplicableWorkflowCategoriesGrid.ReadOnly);

				control.RulesGrid[1, categoriesApplicableIndex] = new ZBool(false);
				AssertEquals(false, control.ApplicableWorkflowCategoriesGrid.ReadOnly);

				control.RulesGrid.ListManager.Position = 0;
				AssertEquals(false, control.ApplicableWorkflowCategoriesGrid.ReadOnly);

				control.RulesGrid[0, categoriesApplicableIndex] = new ZBool(true);
				AssertEquals(true, control.ApplicableWorkflowCategoriesGrid.ReadOnly);
			}
		}
	}
}
