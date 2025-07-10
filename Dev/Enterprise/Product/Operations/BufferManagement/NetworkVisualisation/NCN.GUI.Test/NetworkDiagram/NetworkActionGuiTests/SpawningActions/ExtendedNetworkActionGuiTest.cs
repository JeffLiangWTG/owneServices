using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class ExtendedNetworkActionGuiTest : SpawningIndependentNetworkActionGuiTestCase<ExtendedNetworkAction>
	{
		#region Circular Dependencies

		public void TestExtendedNetworkShouldShowAllCircularAttachmentsUsingConnectors()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");

			var link1to2 = workflow1.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow2);
			var link2to1 = workflow2.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow1); //Circular dependency

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);
			var workflowShape2 = CreateShape(workflow2, diagram);

			Factory.Save();

			CombineAssertions("Preconditions on process headers", () =>
			{
				AssertEquals("jobHeader children", 2, jobHeader.ChildHeaders.Count());
				AssertEquals("workflow1", 1, workflow1.PostrequisiteLinks.Count(l => l.HeaderTo == workflow2));
				AssertEquals("workflow2", 1, workflow2.PostrequisiteLinks.Count(l => l.HeaderTo == workflow1));
				AssertEquals("The graph should have the cycle", false, jobHeader.DependencyGraph.IsDirectedAcyclicGraph());
			});

			CombineAssertions("Preconditions on diagram", () =>
			{
				AssertEquals("Attachments", 0, diagram.AllAttachments.Count);
			});

			BusinessObjectFactory factoryForExtendedNetwork;

			var controller = CreateController(Factory, saveAction: () => ContinueWithSave.Yes);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);
			var network = networkViewModel.GetJobNetwork();
			var action = new ExtendedNetworkAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
			}

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);
			var extendedNetworkViewModel = CreateNetworkViewModel(extendedDiagram);
			var extendedNetwork = extendedNetworkViewModel.GetJobNetwork();

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertEquals("Extended network should have 2 attachments", 2, extendedDiagram.AllAttachments.Count);
				AssertEquals("Attachments should be rendered using 2 connections", 2, extendedNetworkViewModel.Connections.Count());
			});
		}

		public void TestExecute_ForDiagramWithSimplestCircularDependency_ShouldDisplayValidationErrors()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");

			var link1to2 = workflow1.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow2);
			var link2to1 = workflow2.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow1); //Circular dependency

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram, "Test Workflow 1");
			var workflowShape2 = CreateShape(workflow2, diagram, "Test Workflow 2");

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var action = new ExtendedNetworkAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				Application.DoEvents();
				var shownNetwork = form.NetworkDiagramControl.Network;

				AssertErrorDialogShown();
				AssertEquals("Diagram should have validation errors", true, shownNetwork.DiagramEntity.HasErrors);

				CombineAssertions(() =>
				{
					AssertErrorDialogShown();
					AssertEquals("Diagram should have validation errors", true, shownNetwork.DiagramEntity.HasErrors);
					Assert("Should have circular dependency errors", shownNetwork.DiagramEntity.GetErrors().Any(e => e.Message.Contains(@"A looped dependency presents on the diagram. The following workflows are involved in the loop:
Organization (XVBQP68SIYXQ) - Test Workflow 1
Organization (XVBQP68SIYXQ) - Test Workflow 2")));
				});
			}
		}

		public void TestExecute_ForDiagramWithCircularDependencies_ShouldDisplayValidationErrors()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 3");

			var link1to2 = workflow1.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow2);
			var link2to3 = workflow2.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow3);
			var link3to1 = workflow3.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow1); //Circular dependency

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram, "Test Workflow 1");
			var workflowShape2 = CreateShape(workflow2, diagram, "Test Workflow 2");

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var action = new ExtendedNetworkAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				Application.DoEvents();
				var shownNetwork = form.NetworkDiagramControl.Network;

				AssertErrorDialogShown();
				AssertEquals("Diagram should have validation errors", true, shownNetwork.DiagramEntity.HasErrors);

				CombineAssertions(() =>
				{
					AssertErrorDialogShown();
					AssertEquals("Diagram should have validation errors", true, shownNetwork.DiagramEntity.HasErrors);
					Assert("Should have circular dependency errors", shownNetwork.DiagramEntity.GetErrors().Any(e => e.Message.Contains(@"A looped dependency presents on the diagram. The following workflows are involved in the loop:
Organization (XVBQP68SIYXQ) - Test Workflow 1
Organization (XVBQP68SIYXQ) - Test Workflow 2
Organization (XVBQP68SIYXQ) - Test Workflow 3")));
				});
			}
		}

		#endregion

		#region Circular References Made by Both Depedencies and Hierarchies

		public void TestExecute_ForDiagramWithCircularReferencesMadeByChainOfBothDepedenciesAndHierarchies_ShouldDisplayValidationErrors()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Parent Workflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Child Workflow");
			var postreqWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Postreq Workflow");

			var linkParentToChild = BMSTestHelper.MakeChildOfAndGetLink(childWorkflow, parentWorkflow);
			var linkChildToPostreq = childWorkflow.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(postreqWorkflow);
			var linkPostreqToParent = postreqWorkflow.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(parentWorkflow); //Circular dependency

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var parentWorkflowShape = CreateShape(parentWorkflow, diagram, "Parent Workflow");
			var childWorkflowShape = CreateShape(childWorkflow, parentWorkflowShape, "Child Workflow");

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var action = new ExtendedNetworkAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				Application.DoEvents();
				var shownNetwork = form.NetworkDiagramControl.Network;

				CombineAssertions(() =>
				{
					AssertErrorDialogShown();
					AssertEquals("Diagram should have validation errors", true, shownNetwork.DiagramEntity.HasErrors);
					Assert("Should have circular dependency errors", shownNetwork.DiagramEntity.GetErrors().Any(e => e.Message.Contains(@"A looped dependency presents on the diagram. The following workflows are involved in the loop:
Organization (XVBQP68SIYXQ) - Child Workflow
Organization (XVBQP68SIYXQ) - Parent Workflow
Organization (XVBQP68SIYXQ) - Postreq Workflow")));
				});
			}
		}

		public void TestExecute_ForExtendedDiagramWithCircularDependencies_ShouldNotAllowSavingUntilAllCircularDependenciesResolved()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1_2");
			var workflow1_3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1_3");

			var link1_1to2 = workflow1_1.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow1_2);
			var link1_2to3 = workflow1_2.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow1_3);
			var link1_3to1 = workflow1_3.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow1_1); //Circular dependency

			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2_1");
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2_2");
			var workflow2_3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2_3");

			var link2_1to2 = workflow2_1.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow2_2);
			var link2_2to3 = workflow2_2.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow2_3);
			var link2_3to1 = workflow2_3.GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(workflow2_1); //Another circular dependency

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1_1 = CreateShape(workflow1_1, diagram);
			var workflowShape1_2 = CreateShape(workflow1_2, diagram);

			var workflowAttachment1_1 = CreateDependencyAttachment(diagram, link1_1to2, workflowShape1_1, workflowShape1_2);

			var workflowShape2_1 = CreateShape(workflow2_1, diagram);
			var workflowShape2_2 = CreateShape(workflow2_2, diagram);

			var workflowAttachment2_1 = CreateDependencyAttachment(diagram, link2_1to2, workflowShape2_1, workflowShape2_2);

			Factory.Save();

			CombineAssertions("Preconditions on process headers", () =>
			{
				AssertEquals("jobHeader children", 6, jobHeader.ChildHeaders.Count());
				AssertEquals("workflow1_1", 1, workflow1_1.PostrequisiteLinks.Count(l => l.HeaderTo == workflow1_2));
				AssertEquals("workflow1_2", 1, workflow1_2.PostrequisiteLinks.Count(l => l.HeaderTo == workflow1_3));
				AssertEquals("workflow1_3", 1, workflow1_3.PostrequisiteLinks.Count(l => l.HeaderTo == workflow1_1));
				AssertEquals("workflow2_1", 1, workflow2_1.PostrequisiteLinks.Count(l => l.HeaderTo == workflow2_2));
				AssertEquals("workflow2_2", 1, workflow2_2.PostrequisiteLinks.Count(l => l.HeaderTo == workflow2_3));
				AssertEquals("workflow2_3", 1, workflow2_3.PostrequisiteLinks.Count(l => l.HeaderTo == workflow2_1));
				AssertEquals("The graph should have the cycles", false, jobHeader.DependencyGraph.IsDirectedAcyclicGraph());
			});

			BusinessObjectFactory extendedNetworkFactory;

			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = new ExtendedNetworkAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().Last())
			{
				AssertNotNull(form);
				Application.DoEvents();

				var shownDiagramEntity = form.NetworkDiagramControl.Network.DiagramEntity;
				extendedNetworkFactory = shownDiagramEntity.Factory;
				var extendedDiagram = LoadShapeFromFactoryByName(extendedNetworkFactory, nameForExtendedNetwork);
				var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);
				var extendedNetworkViewModel = CreateNetworkViewModel(extendedDiagram);
				var extendedNetwork = extendedNetworkViewModel.GetJobNetwork();

				CombineAssertions("Preconditions", () =>
				{
					AssertNotNull("Extended network should have been created successfully", extendedDiagram);
					AssertNull("Extended network should not be saved in the Database", databaseDiagram);

					AssertEquals(ExtendedNetworkAction.GetNameForExtendedNetwork(diagram), shownDiagramEntity.Name);
					AssertEquals("Shown diagram entity should have validation errors", true, shownDiagramEntity.HasErrors);
					Assert("Shown diagram entity should have circular dependency errors", shownDiagramEntity.GetErrors().Any(e => e.Message.Contains("A looped dependency presents on the diagram. The following workflows are involved in the loop")));
				});

				CombineAssertions("Trying to save the diagram with two unresolved chains", () =>
				{
					form.FireSaveButton();

					databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);
					AssertNull("Extended network should not be saved in the Database as circular dependencies exist", databaseDiagram);
				});

				CombineAssertions("Trying to save the diagram with one unresolved chain", () =>
				{
					link1_3to1.Delete();
					Factory.Save();

					form.FireSaveButton();

					databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);
					AssertNull("Extended network should not be saved in the Database as circular dependencies still exist", databaseDiagram);
				});

				CombineAssertions("Trying to save the diagram with no unresolved chains", () =>
				{
					link2_3to1.Delete();
					Factory.Save();

					form.FireSaveButton();

					databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);
					AssertNotNull("Extended network should now be saved in the Database", databaseDiagram);
				});
			}
		}

		#endregion

		BMNCNShape LoadShapeFromFactoryByName(BusinessObjectFactory factory, string name) => factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, name));

		void AssertErrorDialogShown()
		{
			AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("Should not show message boxes", ZFormModaliser.LastFormShownDialogForTest);
		}

		#region Test Overrides

		protected override INetworkViewModel GetNetworkViewModel(MockRepository mocks, BusinessObjectFactory factory)
		{
			var diagram = CreateDiagram(factory);
			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(factory, CreateRefresher(), interactionImplementor.Object);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);

			interactionImplementor.Setup(m =>
					m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ConfirmationNotification[]>()))
				.Returns(true);

			return networkViewModel;
		}

		protected override ExtendedNetworkAction GetAction(INetworkViewModel networkViewModel) => new ExtendedNetworkAction(networkViewModel);

		#endregion
	}
}
