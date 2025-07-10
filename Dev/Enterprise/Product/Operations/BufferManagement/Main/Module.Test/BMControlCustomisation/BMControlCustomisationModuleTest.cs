using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMControlCustomisationModule))]
	class BMControlCustomisationModuleTest : ZModuleBasherTest
	{
		public void TestCloneSystemDefaults()
		{
			using (var module = new BMControlCustomisationModule())
			{
				AssertNotNull(module.EmbeddedControl); // to build the context menu
				AssertEquals(3, module.ActionsMenuItem.MenuItems.Count);

				var cloneMenuItem = module.ActionsMenuItem.MenuItems[2];
				AssertEquals("Clone System Default", cloneMenuItem.Text);

				var controlTypes = new CustomisedControlTypeList();
				AssertEquals(controlTypes.Count, cloneMenuItem.MenuItems.Count);

				for (var i = 0; i < controlTypes.Count; i++)
				{
					AssertEquals(controlTypes[i].Description, cloneMenuItem.MenuItems[i].Text);
					cloneMenuItem.MenuItems[i].PerformClick();

					using (var form = Application.OpenForms.OfType<BMControlCustomisationForm>().SingleOrDefault())
					{
						AssertNotNull(form);
						AssertEquals(ODisplayMode.New, form.DisplayMode);
						AssertEquals(false, form.BusinessEntity.IsInDatabase);
					}
				}
			}
		}

		public void TestChangeControlType_ShouldValidateIncorrectPropertySource()
		{
			using (var module = new BMControlCustomisationModule())
			{
				AssertNotNull(module.EmbeddedControl); // to build the context menu
				AssertEquals(3, module.ActionsMenuItem.MenuItems.Count);

				var cloneMenuItem = module.ActionsMenuItem.MenuItems[2];
				AssertEquals("Clone System Default", cloneMenuItem.Text);

				cloneMenuItem.MenuItems.Cast<ZMenuItem>().Single(m => m.Text == CustomisedControlTypeList.Descriptions.TaskCard).PerformClick();

				using (var form = Application.OpenForms.OfType<BMControlCustomisationForm>().SingleOrDefault())
				{
					AssertNotNull(form);

					var customisation = form.BusinessEntity;
					var line = customisation.CustomisationLines.Cast<BMControlCustomisationLine>().First(l => l.PropertySource == PropertySourceList.Codes.ProcessTask);

					AssertNoErrors(line);
					AssertEquals(false, form.BusinessEntity.IsInDatabase);

					customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;
					form.FireSaveButton();

					AssertHasError(line.PropertySourceInfo, "Workflow cards may not display properties from tasks.");
					AssertEquals(false, form.BusinessEntity.IsInDatabase);

					customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
					form.FireSaveButton();

					AssertNoErrors(line);
					AssertEquals(true, form.BusinessEntity.IsInDatabase);
				}
			}
		}

		public void TestCloneSystemDefaultsWithoutPermission()
		{
			using (var module = new BMControlCustomisationModule())
			{
				AssertNotNull(module.EmbeddedControl); // to build the context menu
				AssertEquals(3, module.ActionsMenuItem.MenuItems.Count);

				var cloneMenuItem = module.ActionsMenuItem.MenuItems[2];
				AssertEquals("Clone System Default", cloneMenuItem.Text);

				var controlTypes = new CustomisedControlTypeList();
				AssertEquals(controlTypes.Count, cloneMenuItem.MenuItems.Count);

				Env.Security.BMControlCustomisationNew.IsAllowed = false;
				for (var i = 0; i < controlTypes.Count; i++)
				{
					AssertEquals(controlTypes[i].Description, cloneMenuItem.MenuItems[i].Text);
					cloneMenuItem.MenuItems[i].PerformClick();

					AssertMultilineASCIIEquals("Customized Visual Layouts Security Checkpoint", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Buffer Management -> Customized Visual Layouts -> New
", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BMControlCustomisation;
		}
	}
}
