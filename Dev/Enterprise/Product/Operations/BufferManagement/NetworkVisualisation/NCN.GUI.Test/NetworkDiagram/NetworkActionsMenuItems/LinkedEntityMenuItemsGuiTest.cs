using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Test;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class LinkedEntityMenuItemsGuiTest : NetworkGUITestCase
	{
		#region Linking Menu Items

		public void TestLinkWorkflowMenuItem_ShouldLinkShapeToSelectedWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "The phase seems well adjusted");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Try adjusting the phase");

			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertNull(diagram.ProcessHeader);
			AssertNull(shape.ProcessHeader);

			ClickLinkToWorkflowAction(diagram, networkViewModel, jobHeader);
			ClickLinkToWorkflowAction(shape, networkViewModel, workflow);

			AssertEquals(jobHeader, diagram.ProcessHeader);
			AssertEquals(workflow, shape.ProcessHeader);
		}

		public void TestLinkDiagramMenuItem_ShouldLinkShapeToSelectedDiagram()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			var otherDiagram1 = CreateDiagram(Factory, name: "Try the amplitude");
			var otherDiagram2 = CreateDiagram(Factory, name: "That's the leftmost dial");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertNull(diagram.RelatedShape);
			AssertNull(shape.RelatedShape);

			ClickLinkToDiagramAction(diagram, networkViewModel, otherDiagram1);
			ClickLinkToDiagramAction(shape, networkViewModel, otherDiagram2);

			AssertEquals(null, diagram.RelatedShape);
			AssertEquals(otherDiagram2, shape.RelatedShape);
		}

		public void TestLinkDiagramMenuItem_WhenShapesSelected_ShouldLinkShapeToSelectedShape()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			var otherDiagram1 = CreateDiagram(Factory);
			var otherDiagram2 = CreateDiagram(Factory);
			var otherShape1 = CreateShape(otherDiagram1, name: "Reposition the phase");
			var otherShape2 = CreateShape(otherDiagram2, name: "Try the dial on the right");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertNull(diagram.RelatedShape);
			AssertNull(shape.RelatedShape);

			ClickLinkToDiagramAction(diagram, networkViewModel, otherShape1);
			ClickLinkToDiagramAction(shape, networkViewModel, otherShape2);

			AssertEquals(null, diagram.RelatedShape);
			AssertEquals(null, shape.RelatedShape);

			ClickLinkToDiagramAction(shape, networkViewModel, otherDiagram1);
			AssertEquals(otherDiagram1, shape.RelatedShape);
		}

		static void ClickLinkToWorkflowAction(BMNCNShape shape, NetworkViewModel networkViewModel, ProcessHeader workflowToSelectInPopup)
		{
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflowToSelectInPopup))
			{
				LinkedEntityMenuItemsTest.ClickLinkToWorkflowAction(shape, networkViewModel);
			}
		}

		static void ClickLinkToDiagramAction(BMNCNShape shape, NetworkViewModel networkViewModel, BMNCNShape shapeToSelectInPopup)
		{
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(shapeToSelectInPopup))
			{
				LinkedEntityMenuItemsTest.ClickLinkToDiagramAction(shape, networkViewModel);
			}
		}

		#endregion

		#region Open Linked Entity Menu Item

		public void TestOpenLinkedEntity_LinkedToWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Atrus pls");
			var diagram = CreateDiagram(jobHeader);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			LinkedEntityMenuItemsTest.ClickOpenLinkedWorkflowAction(diagram, networkViewModel);
			Application.DoEvents();

			using (var form = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				AssertSamePK((BusinessObject)jobHeader.Parent, form.BusinessEntity);
			}
		}

		public void TestOpenLinkedEntity_LinkedToDiagram()
		{
			var diagram = CreateDiagram(Factory);
			var otherDiagram = CreateDiagram(Factory, name: "You adjust the damn phase");

			NetworkTestCase.LinkToRelatedDiagram(diagram, otherDiagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			LinkedEntityMenuItemsTest.ClickOpenLinkedDiagramAction(diagram, networkViewModel);
			Application.DoEvents();

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				AssertSamePK(otherDiagram, form.BusinessEntity);

				ApplicationHelper.DoEvents();
				Application.DoEvents();
			}
		}

		#endregion

		#region Double-clicking Shape

#if !WINZOR
		// This test is specifically for WPF, we have a similar test for Winzor as E2E tests
		public void TestDoubleClickShape_LinkedToWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "N-no that's too far");
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(jobHeader, diagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				DoubleClickNode(shape, form);
				Application.DoEvents();

				using (var orgForm = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
				{
					AssertNotNull(orgForm);
					AssertSamePK((BusinessObject)jobHeader.Parent, orgForm.BusinessEntity);
				}

				ApplicationHelper.DoEvents();
				Application.DoEvents();
			}
		}

		public void TestDoubleClickShape_LinkedToDiagram()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			var otherDiagram = CreateDiagram(Factory, name: "A little to the left");

			NetworkTestCase.LinkToRelatedDiagram(shape, otherDiagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				DoubleClickNode(shape, form);
				Application.DoEvents();

				using (var otherDiagramForm = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault(f => f.BusinessEntity.Identifier != diagram.PK))
				{
					AssertNotNull(otherDiagramForm);
					AssertSamePK(otherDiagram, otherDiagramForm.BusinessEntity);
				}

				ApplicationHelper.DoEvents();
				Application.DoEvents();
			}
		}

		public void TestDoubleClickShape_LinkedToNestedShape()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			var otherDiagram = CreateDiagram(Factory, name: "A little to the left");
			var otherShape = CreateShape(otherDiagram);

			NetworkTestCase.LinkToRelatedDiagram(shape, otherDiagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				DoubleClickNode(shape, form);
				Application.DoEvents();

				using (var otherDiagramForm = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault(f => f.BusinessEntity.Identifier != diagram.PK))
				{
					AssertNotNull(otherDiagramForm);
					AssertSamePK(otherDiagram, otherDiagramForm.BusinessEntity);
				}

				ApplicationHelper.DoEvents();
				Application.DoEvents();
			}
		}
#endif

		#endregion
	}
}
