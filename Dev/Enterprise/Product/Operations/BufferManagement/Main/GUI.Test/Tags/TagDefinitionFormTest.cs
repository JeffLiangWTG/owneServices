using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(TagDefinitionForm))]
	class TagDefinitionFormTest : ZFormBasherTest
	{
		public void TestNonSystemDefinitionsAreEditable()
		{
			using (var form = CreateTestTagDefinitionForm(false))
			{
				form.Show();
				AssertEquals(true, form.FindAll<ZCheckBox>().Single(x => x.Name == "IsSystemCheckBox").ReadOnly);

				var tagMagnitudeGrid = form.FindAll<ZGrid>().Single(c => c.Name == "TagMagnitudeGrid");
				AssertEquals(false, tagMagnitudeGrid.ReadOnly);

				var isActiveFound = false;
				foreach (ZGridColumnInfo colStyle in tagMagnitudeGrid.ColumnStyles)
				{
					if (colStyle.ColumnName == "TGM_IsActive")
					{
						isActiveFound = true;
						AssertEquals(true, colStyle.IsMandatory);
						break;
					}
				}
				AssertEquals(true, isActiveFound);
			}
		}

		public void TestTagDelete()
		{
			using (var form = CreateTestTagDefinitionForm(false))
			{
				form.Show();
				form.DisplayMode = ODisplayMode.Edit;

				var control = form.FindAll<ZGrid>().Single(c => c.Name == "TagMagnitudeGrid");
				var magnitudes = ((TagDefinition)form.DataSource).Magnitudes;
				var magnitude = magnitudes.FirstOrDefault(x => x.TGM_Code == "ALS");
				Assert("Precondition: magnitude 'ALS' is not deleted yet", !magnitude.IsDeleted);

				control.Select(0);
				control.DeleteMenuItem.PerformClick();
				AssertEquals("Tag(s) have been applied to some items. Please remove the links to those items or deactivate the tag(s) instead.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(4, magnitudes.Count);
				Assert("Magnitude 'ALS' is not deleted because it has links", !magnitude.IsDeleted);

				control.UnSelect(0);
				magnitude = magnitudes.FirstOrDefault(x => x.TGM_Code == "ORT");
				Assert("Precondition: magnitude 'ORT' is not deleted yet", !magnitude.IsDeleted);

				control.Select(3);
				control.DeleteMenuItem.PerformClick();
				AssertEquals(3, magnitudes.Count);
				Assert("Magnitude 'ORT' is deleted now because it doesn't have any links", magnitude.IsDeleted);
			}
		}

		public void TestMultipleTagsDelete()
		{
			using (var form = CreateTestTagDefinitionForm(true))
			{
				var originalForm = form;
				form.Show();
				form.DisplayMode = ODisplayMode.Edit;

				var control = form.FindAll<ZGrid>().Single(c => c.Name == "TagMagnitudeGrid");
				var magnitudes = ((TagDefinition)form.DataSource).Magnitudes;

				control.Select(0);
				control.Select(1);
				control.Select(2);
				control.DeleteMenuItem.PerformClick();
				AssertEquals("Tag(s) have been applied to some items. Please remove the links to those items or deactivate the tag(s) instead.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(4, magnitudes.Count);
				Assert(magnitudes.All(m => !m.IsDeleted));

				control.UnSelect(1);
				control.DeleteMenuItem.PerformClick();
				AssertEquals("Tag(s) have been applied to some items. Please remove the links to those items or deactivate the tag(s) instead.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(magnitudes.All(m => !m.IsDeleted));

				control.UnSelect(2);
				control.Select(3);
				control.DeleteMenuItem.PerformClick();
				AssertEquals("Tag(s) have been applied to some items. Please remove the links to those items or deactivate the tag(s) instead.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(4, magnitudes.Count);
				Assert(magnitudes.All(m => !m.IsDeleted));
			}
		}

		public void TestShowTagFormAndAssertExpectedBizosHeldInMemory()
		{
			using (var form = CreateTestTagDefinitionForm(false))
			{
				_ = ((BusinessObject)form.BusinessEntity).GetNotes().HasNotes;
				var databaseLoadCountBeforeDelete = Factory.DatabaseLoadCount;

				form.Show();

				var control = form.FindAll<ZGrid>().Single(c => c.Name == "TagMagnitudeGrid");
				var tagDefinition = (TagDefinition)form.DataSource;

				control.Select(0);
				control.Select(1);
				control.Select(2);
				control.DeleteMenuItem.PerformClick();

				AssertEquals("Should not be any additional database loads and DatabaseLoadCount should remain the same", databaseLoadCountBeforeDelete, Factory.DatabaseLoadCount);
			}
		}

		#region Implementation

		TagDefinitionForm CreateTestTagDefinitionForm(bool createTagRule)
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "AMA", "Ask me anything");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "ALS", "Secretly, don't ask me that.");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "AND", "Or that.");
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "BUT", "And that.");
			var magnitude4 = BMSTestHelper.CreateTagMagnitude(definition, "ORT", "Also this.");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Jimminy Jillikers");
			workflow.AddTag(magnitude1);

			if (createTagRule)
			{
				var tagRule1 = BMSTestHelper.CreateTagRule(magnitude2, "Rule1", TagRuleActionTypeList.Codes.AddTag);
				tagRule1.TagTemplate.TGL_TGM_Magnitude = magnitude2.PK;
				var tagRule2 = BMSTestHelper.CreateTagRule(magnitude3, "Rule2", TagRuleActionTypeList.Codes.AddTag);
				tagRule2.TagTemplate.TGL_TGM_Magnitude = magnitude3.PK;
			}

			Factory.Save();
			var form = new TagDefinitionForm(definition);
			return form;
		}

		protected override Form GetFormToBashCore()
		{
			return new TagDefinitionForm(Factory.New<TagDefinition>());
		}

		#endregion
	}
}
