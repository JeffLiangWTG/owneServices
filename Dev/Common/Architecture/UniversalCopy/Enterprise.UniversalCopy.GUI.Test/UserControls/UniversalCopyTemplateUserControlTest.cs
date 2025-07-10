using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using UCManager = Enterprise.UniversalCopy.GUI.Testing.UniversalCopyManagerTest.UniversalCopyManagerForTest;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	public class UniversalCopyTemplateUserControlTest : TestCaseWithFactory
	{
		public void TestSelectNode_NoException()
		{
			var elementType = ObjectFactory.GetType<Integration.Forwarding.IForwardingShipment>();
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(elementType, true);
			var moduleId = ModuleIDs.JobShipment;
			var factory = new UniversalCopyFactory(interfaceType, moduleId);
			using (var grid = new DummyZFilterGridForUniversalCopyTest(moduleId, elementType))
			using (var manager = new GridUniversalCopyManager(grid))
			using (var form = new UniversalCopyTemplateForm(factory.GetNewCopyTemplate(interfaceType), manager))
			{
				form.Show();
				Application.DoEvents();
				var control = form.Controls.Find("ucTemplateUserControl", true)[0] as UniversalCopyTemplateUserControl;
				var treeView = control.templateTreeView;
				foreach (UniversalCopyTreeNode node in treeView.Nodes)
				{
					AssertNoException(treeView, node);

					var orderNode = node.Nodes.Cast<UniversalCopyTreeNode>().FirstOrDefault(x => x.Text == "Display Order");
					orderNode.Expand();
					AssertNoException(treeView, orderNode);

					var prePlaningNode = orderNode.Nodes.Cast<UniversalCopyTreeNode>().FirstOrDefault(x => x.Text == "Shipment Pre Planning");
					prePlaningNode.Expand();
					AssertNoException(treeView, prePlaningNode);

					var shipmentNode = prePlaningNode.Nodes.Cast<UniversalCopyTreeNode>().FirstOrDefault(x => x.Text == "Shipment");
					shipmentNode.Expand();
					AssertNoException(treeView, shipmentNode);

					orderNode = shipmentNode.Nodes.Cast<UniversalCopyTreeNode>().FirstOrDefault(x => x.Text == "Display Order");
					orderNode.Expand();
					AssertNoException(treeView, orderNode);
				}
			}
		}

		public void TestCopyManagerProperty()
		{
			using (var control = new UniversalCopyTemplateUserControl())
			{
				AssertNull(control.CopyManager);
				AssertNull(control.templateTreeView.CopyManager);

				using (var manager = new UCManager(typeof(DummyBusinessObject), DummyModuleIDs.Dummy))
				{
					control.CopyManager = manager;
					AssertSame("Should propagate copy manager to treeview control", manager, control.templateTreeView.CopyManager);

					control.CopyManager = null;
					AssertNull("Should also reset copy manager on treeview control", control.templateTreeView.CopyManager);

					control.templateTreeView.CopyManager = manager;
					AssertNull("Should not back-propagate copy manager to parent control", control.CopyManager);

					control.CopyManager = null;
					AssertNull(control.templateTreeView.CopyManager);
				}
			}
		}

		public void TestGetPropertyListModuleId()
		{
			var copyTemplateTree = new CopyTemplateTree(
				GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(OrgHeader), true),
				typeof(OrgHeader),
				BusinessObjectCopyManager.CopyTreeConfiguration);

			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.Organisation.Name + "_UC";
			template.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			template.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			template.CopyTemplateTree = new CopyTemplateTreeBizo(copyTemplateTree, template);
			template.S9_FilterName = "Test";
			template.S9_IsPublished = true;

			using (var manager = new UCManager(typeof(OrgHeader), ModuleIDs.Organisation))
			using (var form = new UniversalCopyTemplateForm(template, manager))
			{
				form.Show();

				var bizo = form.Template.CopyTemplateTree.PropertyNodes
							.Cast<PropertyCopyTemplateBizo>().FirstOrDefault(q => q.Name == "OH_RSL_ShippingLine");

				var control = form.Controls.Find("ucTemplateUserControl", true)[0] as UniversalCopyTemplateUserControl;
				var moduleId = control.GetPropertyListModuleId(bizo);
				AssertEquals(ModuleIDs.RefShippingLine, moduleId);
				AssertEquals("Guid", bizo.ValueType);
				Assert(bizo.CopyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Value));
			}
		}

		[ExpectNoExceptions]
		public void TestGetPropertyListModuleIdWithActiveProcessTaskTemplateCollection()
		{
			var copyTemplateTree = new CopyTemplateTree(
				GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(ProcessTaskNotification), true),
				typeof(ProcessTaskNotification),
				BusinessObjectCopyManager.CopyTreeConfiguration);

			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.ProcessTemplates.Name + "_UC";
			template.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			template.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			template.CopyTemplateTree = new CopyTemplateTreeBizo(copyTemplateTree, template);
			template.S9_FilterName = "Test";
			template.S9_IsPublished = true;

			using (var manager = new UCManager(typeof(ProcessTaskNotification), ModuleIDs.ProcessTemplates))
			using (var form = new UniversalCopyTemplateForm(template, manager))
			{
				form.Show();

				var bizo = form.Template.CopyTemplateTree.PropertyNodes
					.Cast<PropertyCopyTemplateBizo>().FirstOrDefault(q => q.Name == "PQ_P0_WorkflowTemplate");

				var control = form.Controls.Find("ucTemplateUserControl", true)[0] as UniversalCopyTemplateUserControl;
				var moduleId = control.GetPropertyListModuleId(bizo);
			}
		}

		void AssertNoException(UniversalCopyTemplateTreeView treeView, UniversalCopyTreeNode node)
		{
			foreach (UniversalCopyTreeNode child in node.Nodes)
			{
				AssertNoExceptionThrown(child.Text, () => treeView.PerformSelect(child));
			}
		}

		class DummyZFilterGridForUniversalCopyTest : ZFilterGrid
		{
			public DummyZFilterGridForUniversalCopyTest(ModuleIdentifier moduleID, Type elementType)
			{
				SetModuleId(moduleID);
				SetElementTypeForTest(elementType);
			}
		}
	}
}
