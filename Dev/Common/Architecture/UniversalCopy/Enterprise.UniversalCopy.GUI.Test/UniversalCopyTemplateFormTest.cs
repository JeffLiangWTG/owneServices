using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using UCManager = Enterprise.UniversalCopy.GUI.Testing.UniversalCopyManagerTest.UniversalCopyManagerForTest;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	[TestedType(typeof(UniversalCopyTemplateForm))]
	public class UniversalCopyTemplateFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			UniversalCopyTemplate newTemplate = Factory.CreateNewFactory().New<UniversalCopyTemplate>();
			newTemplate.S9_ModuleID = "DUM_UC";
			newTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			newTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true)), newTemplate);
			newTemplate.ClearHasChanges();

			return new UniversalCopyTemplateForm(newTemplate, null);
		}

		#endregion

		public void TestApplyButtonIsDisabledWhenSaved()
		{
			var template = GetNewCopyTemplate();
			using (var manager = new UCManager(typeof(DummyBusinessObject), DummyModuleIDs.Dummy))
			{
				using (var form = new UniversalCopyTemplateForm(template, manager))
				{
					form.Show();
					form.Template.CopyTemplateTree.ConfigurationName = "name";
					form.Template.CopyTemplateTree.DefaultToCopy(1);
					form.CommandButtonApply.PerformClick();

					Assert(!form.CommandButtonApply.Enabled);
					AssertEquals("&Save", form.CommandButtonApply.Text);
					AssertNoExceptionThrown(() => form.CommandButtonApply.PerformClick());
				}
			}
		}

		public void TestCreateEditDeletePrivatePublishTemplate()
		{
			var template = GetNewCopyTemplate();
			template.S9_IsPublished = true;
			using (var manager = new UCManager(typeof(DummyBusinessObject), DummyModuleIDs.Dummy))
			{
				manager.Security.UniversalCopyCEDPrivateCheckpoint.IsAllowed = true;
				manager.Security.UniversalCopyEditPublishCheckpoint.IsAllowed = false;
				manager.Security.UniversalCopyDeletePublishCheckpoint.IsAllowed = false;
				using (var form = new UniversalCopyTemplateForm(template, manager))
				{
					form.Show();
					Assert(form.DisplayMode == ODisplayMode.ReadOnly);
				}

				manager.Security.UniversalCopyEditPublishCheckpoint.IsAllowed = true;
				using (var form = new UniversalCopyTemplateForm(template, manager))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					form.Show();
					Assert(form.DisplayMode != ODisplayMode.ReadOnly);

					MethodInfo dynMethod = form.GetType().GetMethod("SwitchToDeleteMode", BindingFlags.NonPublic | BindingFlags.Instance);
					dynMethod.Invoke(form, new object[] { null, null });

					AssertEquals("You don't have permission to delete published Universal Copy templates.", UnitTestUserNotification.Instance.LastMessage.Text);

					manager.Security.UniversalCopyEditPublishCheckpoint.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessages();
					form.Template.CopyTemplateTree.DefaultToCopy();
					form.Template.CopyTemplateTree.ConfigurationName = "TestBla";
					form.FireSaveButton();

					AssertEquals("You don't have permission to publish Universal Copy templates.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDefaultToCopy()
		{
			var template = GetNewCopyTemplate();
			template.S9_IsPublished = true;
			using (var manager = new UCManager(typeof(DummyBusinessObject), DummyModuleIDs.Dummy))
			{
				using (var form = new UniversalCopyTemplateForm(template, manager))
				{
					form.Show();

					foreach (PropertyCopyTemplateBizo node in form.Template.CopyTemplateTree.PropertyNodes)
					{
						AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.DoNotCopy, node.CopyMethod);
					}

					form.Template.CopyTemplateTree.DefaultToCopy(1);
					foreach (PropertyCopyTemplateBizo node in form.Template.CopyTemplateTree.PropertyNodes)
					{
						AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.Copy, node.CopyMethod);
					}
					foreach (EntityCopyTemplateBizo node in form.Template.CopyTemplateTree.ChildNodes)
					{
						foreach (PropertyCopyTemplateBizo node2 in node.PropertyNodes)
						{
							AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.DoNotCopy, node2.CopyMethod);
						}
					}

					form.Template.CopyTemplateTree.DefaultToCopy(2);
					foreach (PropertyCopyTemplateBizo node in form.Template.CopyTemplateTree.PropertyNodes)
					{
						AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.Copy, node.CopyMethod);
					}
					foreach (EntityCopyTemplateBizo node in form.Template.CopyTemplateTree.ChildNodes)
					{
						foreach (PropertyCopyTemplateBizo node2 in node.PropertyNodes)
						{
							AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.Copy, node2.CopyMethod);
						}
						foreach (EntityCopyTemplateBizo node2 in node.ChildNodes)
						{
							foreach (PropertyCopyTemplateBizo node3 in node2.PropertyNodes)
							{
								AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.DoNotCopy, node3.CopyMethod);
							}
						}
					}

					form.Template.CopyTemplateTree.DefaultToCopy(3);
					foreach (PropertyCopyTemplateBizo node in form.Template.CopyTemplateTree.PropertyNodes)
					{
						AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.Copy, node.CopyMethod);
					}
					foreach (EntityCopyTemplateBizo node in form.Template.CopyTemplateTree.ChildNodes)
					{
						foreach (PropertyCopyTemplateBizo node2 in node.PropertyNodes)
						{
							AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.Copy, node2.CopyMethod);
						}
						foreach (EntityCopyTemplateBizo node2 in node.ChildNodes)
						{
							foreach (PropertyCopyTemplateBizo node3 in node2.PropertyNodes)
							{
								AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.Copy, node3.CopyMethod);
							}
						}
					}
				}
			}
		}

		public void TestDefaultToCopy_JobShipment()
		{
			var template = GetNewCopyTemplate_JobShipment();
			template.S9_IsPublished = true;
			using (var manager = new UCManager(ObjectFactory.GetType<Integration.Forwarding.IForwardingShipment>(), ModuleIDs.JobShipment))
			{
				using (var form = new UniversalCopyTemplateForm(template, manager))
				{
					form.Show();
					form.Template.CopyTemplateTree.DefaultToCopy(2);
					//I think this should fail, and I should edit it until it passes
					foreach (PropertyCopyTemplateBizo node in form.Template.CopyTemplateTree.PropertyNodes)
					{
						AssertEquals(PropertyCopyTemplateBizo.CopyMethodCodes.Copy, node.CopyMethod);
					}
					foreach (EntityCopyTemplateBizo node in form.Template.CopyTemplateTree.ChildNodes)
					{
						if (node is CollectionCopyTemplateBizo cnode)
						{
							//collections with no properties can't be copied (like Conversations), so don't default them as such
							//if in the future Conversations *has* properties under it, feel free to delete the test, idk how else to trigger it
							if (node.Description == "Conversations")
							{
								AssertEquals(CollectionCopyTemplateBizo.CopyMethodCodes.DoNotCopy, cnode.CopyMethod);
							}
							else
							{
								AssertNotEquals(CollectionCopyTemplateBizo.CopyMethodCodes.DoNotCopy, cnode.CopyMethod);
							}
						}
					}
				}
			}
		}

		UniversalCopyTemplate GetNewCopyTemplate()
		{
			UniversalCopyTemplate newTemplate = Factory.New<UniversalCopyTemplate>();
			newTemplate.S9_ModuleID = DummyModuleIDs.Dummy.Name + "_UC";
			newTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			newTemplate.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			newTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true)), newTemplate);
			newTemplate.S9_FilterName = "Test";
			return newTemplate;
		}

		UniversalCopyTemplate GetNewCopyTemplate_JobShipment()
		{
			UniversalCopyTemplate newTemplate = Factory.New<UniversalCopyTemplate>();
			newTemplate.S9_ModuleID = ModuleIDs.JobShipment.Name + "_UC";
			newTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			newTemplate.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			newTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(ObjectFactory.GetType<Integration.Forwarding.IForwardingShipment>(), true)), newTemplate);
			newTemplate.S9_FilterName = "Test";
			return newTemplate;
		}
	}
}
