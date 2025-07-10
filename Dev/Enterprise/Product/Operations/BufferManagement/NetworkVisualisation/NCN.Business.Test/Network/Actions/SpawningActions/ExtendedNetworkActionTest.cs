using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	//see also ExtendedNetworkActionGuiTest
	[TestedType(typeof(ExtendedNetworkAction))]
	class ExtendedNetworkActionTest : JobNetworkActionTestCase<ExtendedNetworkAction>
	{
		#region PreExecution Checks

		public void TestShouldNotExecuteAndNotifyToTheUser_IfThereAreValidationErrors()
		{
			AssertDoesNotExecuteAndNotifyToTheUser_IfThereAreValidationErrors();
		}

		public void TestShouldNotRequireSavingBeforeExecution()
		{
			AssertDoesNotRequireSavingBeforeExecution();
		}

		public void TestShouldRequireUserConfirmationBeforeExecution()
		{
			AssertRequiresSpecificConfirmationBeforeExecution("Are you sure you want to create an extended diagram for the given shape?");
		}

		#endregion

		#region Execute

		protected override void TestExecuteCore()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");

			workflow1.MakePrerequisiteOf(workflow2);

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network shoud have 4 shapes", 4, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network shoud have 1 attachment", 1, extendedDiagram.AllAttachments.Count);
			});

			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_OnShape_ShouldOpenNewDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");

			workflow1.MakePrerequisiteOf(workflow2);

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape = CreateShape(workflow1, diagram);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(workflowShape);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 4 shapes", 4, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network should have 1 attachment", 1, extendedDiagram.AllAttachments.Count);
			});
		}

		public void TestExecute_OnShape_ShouldOpenNewDiagramAndCorrectlyResizeMainShape()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 3");

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var someShape = CreateShape(workflow1, diagram);

			someShape.Width = 100;
			someShape.Left = 200;
			someShape.Height = 100;
			someShape.Top = 200;

			var furtherRightShape = CreateShape(workflow3, diagram);

			furtherRightShape.Width = 105;
			furtherRightShape.Left = 200;
			furtherRightShape.Height = 50;
			furtherRightShape.Top = 100;

			var furtherDownShape = CreateShape(workflow2, diagram);

			furtherDownShape.Width = 100;
			furtherDownShape.Left = 1;
			furtherDownShape.Height = 200;
			furtherDownShape.Top = 300;

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 4, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram.RootShape);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var farRightEntity = furtherDownShape.AsEntity(network);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 4 shapes", 5, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);

				var topLevelShape = extendedDiagram.ChildShapes.Single();
				var shape = extendedDiagram.AsDiagramEntity() as ShapeNetworkEntity;
				var entity = topLevelShape.AsEntity(shape);

				AssertEquals("Top Level Shape width should be 305: furtherToTheRightShape Left + Width", furtherRightShape.Left + furtherRightShape.Width, topLevelShape.Width);
				AssertEquals("Top Level Shape height should be 310: furtherDownShape Top + Height", furtherDownShape.Top + furtherDownShape.Height, topLevelShape.Height);
			});
		}

		public void TestExecute_OnScaledDiagramShape_ShouldOpenNewDiagramCorrectlyResizeAndPositionShapes()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 3");

			var diagram = CreateDiagram(jobHeader);
			diagram.SwitchToScaled();
			diagram.BNS_Name = "Test Diagram";

			var someShape = CreateShape(workflow1, diagram);

			someShape.Name = nameof(someShape);
			someShape.Width = 150;
			someShape.Left = 200;
			someShape.Height = 100;
			someShape.Top = 200;

			var furtherToTheRightShape = CreateShape(workflow3, diagram);

			furtherToTheRightShape.Name = nameof(furtherToTheRightShape);
			furtherToTheRightShape.Width = 200;
			furtherToTheRightShape.Left = 300;
			furtherToTheRightShape.Height = 100;
			furtherToTheRightShape.Top = 100;

			var furtherDownShape = CreateShape(workflow2, diagram);

			furtherDownShape.Name = nameof(furtherDownShape);
			furtherDownShape.Width = 100;
			furtherDownShape.Left = 1;
			furtherDownShape.Height = 400;
			furtherDownShape.Top = 500;

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 4, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			NodeViewModel someEntityOrigin;
			NodeViewModel furtherRightEntityOrigin;
			NodeViewModel furtherDownEntityOrigin;

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram.RootShape);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			someEntityOrigin = networkViewModel.Nodes.Single(e => e.Entity.EntityPK == someShape.PK);
			furtherRightEntityOrigin = networkViewModel.Nodes.Single(e => e.Entity.EntityPK == furtherToTheRightShape.PK);
			furtherDownEntityOrigin = networkViewModel.Nodes.Single(e => e.Entity.EntityPK == furtherDownShape.PK);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 4 shapes", 5, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);

				var newDiagram = extendedDiagram.AsDiagramEntity() as ShapeNetworkEntity;
				var newTopLevelShape = extendedDiagram.ChildShapes.Single();
				var newTopLevelEntity = newTopLevelShape.AsEntity(newDiagram);
				var newSomeEntity = newTopLevelShape.ChildShapes.Single(s => s.Name == someShape.Name).AsEntity(newDiagram);
				var newFurtherRightShapeEnitity = newTopLevelShape.ChildShapes.Single(s => s.Name == furtherToTheRightShape.Name).AsEntity(newDiagram);
				var newFurtherDownShape = newTopLevelShape.ChildShapes.Single(s => s.Name == furtherDownShape.Name).AsEntity(newDiagram);

				AssertEquals("New SomeShape should have same position X that origin entity", Math.Round(someEntityOrigin.X), Math.Round(newSomeEntity.X));
				AssertEquals("New SomeShape should have the same width of origin entity", Math.Round(someEntityOrigin.Width), Math.Round(newSomeEntity.Width));
				AssertEquals("New SomeShape should have same position Y that origin entity", Math.Round(someEntityOrigin.Y), Math.Round(newSomeEntity.Y));
				AssertEquals("New SomeShape should have the same height of origin entity", Math.Round(someEntityOrigin.Height), Math.Round(newSomeEntity.Height));

				AssertEquals("New furtherRightEntity should have same position X that origin entity", Math.Round(furtherRightEntityOrigin.X), Math.Round(newFurtherRightShapeEnitity.X));
				AssertEquals("New furtherRightEntity should have the same width of origin entity", Math.Round(furtherRightEntityOrigin.Width), Math.Round(newFurtherRightShapeEnitity.Width));
				AssertEquals("New furtherRightEntity should have same position Y that origin entity", Math.Round(furtherRightEntityOrigin.Y), Math.Round(newFurtherRightShapeEnitity.Y));
				AssertEquals("New furtherRightEntity should have the same height of origin", Math.Round(furtherRightEntityOrigin.Height), Math.Round(newFurtherRightShapeEnitity.Height));

				AssertEquals("New furtherDownEntity should have same position X that origin entity", Math.Round(furtherDownEntityOrigin.X), Math.Round(newFurtherDownShape.X));
				AssertEquals("New furtherDownEntity should have the same width of origin entity", Math.Round(furtherDownEntityOrigin.Width), Math.Round(newFurtherDownShape.Width));
				AssertEquals("New furtherDownEntity should have same position Y that origin entity", Math.Round(furtherDownEntityOrigin.Y), Math.Round(newFurtherDownShape.Y));
				AssertEquals("New furtherDownEntity should have the same height of origin", Math.Round(furtherDownEntityOrigin.Height), Math.Round(newFurtherDownShape.Height));

				AssertEquals("Top Level Entity width should be furtherRightEntityOrigin X + Width", Math.Round(furtherRightEntityOrigin.X + furtherRightEntityOrigin.Width), Math.Round(newTopLevelEntity.Width));
				AssertEquals("Top Level Entity height should be furtherDownEntityOrigin Y + Height", Math.Round(furtherDownEntityOrigin.Y + furtherDownEntityOrigin.Height), Math.Round(newTopLevelEntity.Height));
			});
		}

		public void TestExecute_ForDefaultDiagram_ShouldOpenNewDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "This is just a test workflow");

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			defaultDiagram.BNS_Name = "Test Diagram";

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.DefaultDiagram, defaultDiagram.BNS_ShapeType);
			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(defaultDiagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(defaultDiagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(defaultDiagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(defaultDiagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 3 shapes", 3, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
			});
		}

		public void TestExecute_OnDefaultWorkflow_InDefaultDiagram_ShouldOpenNewDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			defaultDiagram.BNS_Name = "Test Diagram";

			var defaultWorkflowShape = defaultDiagram.ChildShapes[0];

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.DefaultWorkflow, defaultWorkflowShape.BNS_ShapeType);
			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(defaultDiagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(defaultDiagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(defaultDiagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(defaultWorkflowShape);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 3 shapes", 3, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
			});
		}

		public void TestExecute_OnBuffer_ShouldNotBePossible_AsActionIsDisabled()
		{
			var diagram = CreateDiagram(Factory);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);

			Factory.Save();

			var controller = CreateMockableController(Mocks);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var action = GetAction(networkViewModel);
			AssertEquals("Extended network button should be disabled", false, action.IsEnabledAfterActivatingEntity_ForTest(buffer).IsAllowed);
			AssertExceptionThrown(typeof(InvalidOperationException), () => action.ExecuteForEntityWithoutAccessCheck(buffer));
		}

		public void TestExecute_OnAnnotation_ShouldNotBePossible_AsActionIsDisabled()
		{
			var diagram = CreateDiagram(Factory);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			Factory.Save();

			var controller = CreateMockableController(Mocks);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var action = GetAction(networkViewModel);
			AssertEquals("Extended network button should be disabled", false, action.IsEnabledAfterActivatingEntity_ForTest(annotation).IsAllowed);
			AssertExceptionThrown(typeof(InvalidOperationException), () => action.ExecuteForEntityWithoutAccessCheck(annotation));
		}

		public void TestExecute_ForDiagramWithSomeNonWorkflowShapes_ShouldOpenNewDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 4");

			workflow1.MakePrerequisiteOf(workflow3);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow2, workflow4);

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);
			var workflowShape2 = CreateShape(workflow2, diagram);
			var nonWorkflowShape1 = CreateShape(diagram);
			var nonWorkflowShape2 = CreateShape(diagram);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 5, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network shoud have 8 shapes", 8, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network shoud have 1 attachment", 1, extendedDiagram.AllAttachments.Count);
			});
		}

		public void TestExecute_ForDiagramWithMultipleShapesAndAnnotation_ShouldOpenNewDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";
			var workflowShape1 = CreateShape(workflow1, diagram);
			var workflowShape2 = CreateShape(workflow2, diagram);
			var annotation1 = CreateShape(diagram, "Test Annotation", shapeType: ShapeTypeList.Codes.Annotation);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 4, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network shoud have 5 shapes", 5, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
			});
		}

		public void TestExecute_ForDiagramWithNestedShapes_ShouldOpensNewDiagramAndCorrectlyShowNesting()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 4");

			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow2, workflow1);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow4, workflow3);

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);
			var workflowShape2 = CreateShape(workflow4, diagram);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 3, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 6 shapes", 6, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
			});
		}

		public void TestExecute_ForDiagramWithDependencies_ShouldOpensNewDiagramAndCorrectlyShowDependencies()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader2, "Test Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 4");

			var link1 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3 = workflow3.GetOrCreateDependencyLink(workflow4);

			var diagram = CreateDiagram(jobHeader2);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape2 = CreateShape(workflow2, diagram);
			var workflowShape3 = CreateShape(workflow3, diagram);
			var workflowAttachment = CreateDependencyAttachment(diagram, link2, workflowShape2, workflowShape3);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 3, CountChildShapesDownHierarchy(diagram));
			AssertEquals("Precondition", 1, diagram.AllAttachments.Count);

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForJobHeader2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader2);
			var shapeForWorkflow1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow1);
			var shapeForWorkflow2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow2);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 7 shapes", 7, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 2 top level shapes", 2, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network shoud have 3 attachments", 3, extendedDiagram.AllAttachments.Count);

				AssertEquals("Job Header1 should be a top level shape", extendedDiagram, shapeForJobHeader1.ParentShape);
				AssertEquals("Job Header2 should be a top level shape", extendedDiagram, shapeForJobHeader2.ParentShape);
				AssertEquals("Workflow 1 should be nested inside job header 1", shapeForJobHeader1, shapeForWorkflow1.ParentShape);
				AssertEquals("Workflow 2 should be nested inside job header 2", shapeForJobHeader2, shapeForWorkflow2.ParentShape);
			});
		}

		public void TestExecute_ForDiagramWithExistingLinkedShapes_ButNoAttachment_ShouldOpensNewDiagramAndCorrectlyShowLink()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 4");

			var link1 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2 = workflow1.GetOrCreateDependencyLink(workflow3);
			var link3 = workflow1.GetOrCreateDependencyLink(workflow4);

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);
			workflowShape1.BNS_Name = "WF1";
			var workflowShape2 = CreateShape(workflow2, diagram);
			workflowShape2.BNS_Name = "WF2";
			var workflowShape3 = CreateShape(workflow3, diagram);
			workflowShape3.BNS_Name = "WF3";

			var workflowAttachment = CreateDependencyAttachment(diagram, link1, workflowShape1, workflowShape2);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 4, CountChildShapesDownHierarchy(diagram));
			AssertEquals("Precondition", 1, diagram.AllAttachments.Count);

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 6 shapes", 6, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network shoud have 3 attachments", 3, extendedDiagram.AllAttachments.Count);
			});
		}

		public void TestExecute_ForDiagramLinkedToComplicatedNetworkWithLotsOfDependenciesAndHierarchies_ShouldOpenNewDiagramAndCorrectlyShowAllRelations()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 4");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 5");
			var workflow6 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 6");
			var workflow7 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 7");
			var workflow8 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 8");
			var workflow9 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 9");
			var workflow10 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 10");
			var workflow11 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 11");
			var workflow12 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 12");

			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow2, workflow1);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow3, workflow1);

			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow9, workflow8);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow11, workflow8);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow10, workflow9);

			var link1 = workflow1.GetOrCreateDependencyLink(workflow5);
			var link2 = workflow3.GetOrCreateDependencyLink(workflow6);
			var link3 = workflow4.GetOrCreateDependencyLink(workflow5);
			var link4 = workflow5.GetOrCreateDependencyLink(workflow7);
			var link5 = workflow5.GetOrCreateDependencyLink(workflow9);
			var link6 = workflow6.GetOrCreateDependencyLink(workflow11);
			var link7 = workflow6.GetOrCreateDependencyLink(workflow12);
			var link8 = workflow7.GetOrCreateDependencyLink(workflow12);

			var diagram = CreateDiagram(jobHeader1);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));
			AssertEquals("Precondition", 0, diagram.AllAttachments.Count);

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForWorkflow1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow1);
			var shapeForWorkflow3 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow3);
			var shapeForWorkflow8 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow8);
			var shapeForWorkflow9 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow9);
			var shapeForWorkflow10 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow10);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 14 shapes", 14, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network shoud have 8 attachments", 8, extendedDiagram.AllAttachments.Count);

				AssertEquals("The Job Header should be a top level shape on the diagram", extendedDiagram, shapeForJobHeader1.ParentShape);
				AssertEquals("Workflow1 should be nested inside the job header", shapeForJobHeader1, shapeForWorkflow1.ParentShape);
				AssertEquals("Workflow3 should be nested inside workflow1", shapeForWorkflow1, shapeForWorkflow3.ParentShape);
				AssertEquals("Workflow8 should be nested inside the job header", shapeForJobHeader1, shapeForWorkflow8.ParentShape);
				AssertEquals("Workflow9 should be nested inside workflow8", shapeForWorkflow8, shapeForWorkflow9.ParentShape);
				AssertEquals("Workflow10 should be nested inside workflow9", shapeForWorkflow9, shapeForWorkflow10.ParentShape);
			});
		}

		public void TestExecute_ForDiagramLinkedToNetworkWithDependenciesWithinAndAcrossHierarchies_ShouldOpenNewDiagramAndCorrectlyShowAllRelations()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var childHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 4");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 5");
			var workflow51 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 5.1");
			var workflow52 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 5.2");
			var workflowA = BMSTestHelper.CreateWorkflow(childHeader, "Test Workflow A");
			var workflowB = BMSTestHelper.CreateWorkflow(childHeader, "Test Workflow B");

			var link1 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3 = workflow3.GetOrCreateDependencyLink(workflow4);
			var link4 = workflow4.GetOrCreateDependencyLink(workflow5);
			var link5 = workflow51.GetOrCreateDependencyLink(workflow52);
			var link6 = childHeader.GetOrCreateDependencyLink(workflow3);

			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow51, workflow5);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow52, workflow5);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(childHeader, jobHeader1);

			var diagram = CreateDiagram(jobHeader1);
			diagram.BNS_Name = "Test Diagram";

			var childShape = CreateShape(childHeader, diagram);
			var workflowShape3 = CreateShape(workflow3, diagram);
			var workflowShape5 = CreateShape(workflow5, diagram);

			var attachment1 = CreateDependencyAttachment(diagram, link6, childShape, workflowShape3);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 4, CountChildShapesDownHierarchy(diagram));
			AssertEquals("Precondition", 1, diagram.AllAttachments.Count);

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForChildHeader = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, childHeader);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 10 shapes", 10, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shapes", 1, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network shoud have 6 attachments", 6, extendedDiagram.AllAttachments.Count);

				AssertEquals("Child Header should still be nested inside its parent job header", shapeForJobHeader1, shapeForChildHeader.ParentShape);
				AssertNotEquals("Child Header should not be a top level shape on the diagram", extendedDiagram, shapeForChildHeader.ParentShape);
			});
		}

		public void TestExecute_ForDiagramNotLinkedToAJobHeader_ShouldOpenNewDiagramAndCorrectlyShowAllRelations()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader2, "Test Workflow 4");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader2, "Test Workflow 5");
			var workflow6 = BMSTestHelper.CreateWorkflow(jobHeader3, "Test Workflow 6");
			var workflow7 = BMSTestHelper.CreateWorkflow(jobHeader3, "Test Workflow 7");
			var workflow8 = BMSTestHelper.CreateWorkflow(jobHeader3, "Test Workflow 8");

			var link1 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3 = workflow3.GetOrCreateDependencyLink(workflow4);
			var link4 = workflow4.GetOrCreateDependencyLink(workflow5);
			var link5 = workflow5.GetOrCreateDependencyLink(workflow6);
			var link6 = workflow6.GetOrCreateDependencyLink(workflow7);

			BMSTestHelper.MakeChildOf(workflow7, workflow8);

			var diagram = CreateDiagram(Factory);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);
			var workflowShape4 = CreateShape(workflow4, diagram);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 3, CountChildShapesDownHierarchy(diagram));
			AssertEquals("Precondition", 0, diagram.AllAttachments.Count);

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);
			var shapeForOldDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, "Test Diagram");

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForJobHeader2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader2);
			var shapeForJobHeader3 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader3);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 13 shapes", 13, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 2 top level shapes", 2, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network shoud have 6 attachments", 6, extendedDiagram.AllAttachments.Count);

				AssertEquals("First Job Header should be nested inside the unlinked old diagram shape", shapeForOldDiagram, shapeForJobHeader1.ParentShape);
				AssertEquals("Second Job Header should be nested inside the unlinked old diagram shape", shapeForOldDiagram, shapeForJobHeader2.ParentShape);
				AssertNotEquals("Third Job Header should not be nested inside the unlinked old diagram shape", shapeForOldDiagram, shapeForJobHeader3.ParentShape);
				AssertEquals("Third Job Header should be a top level shape on the diagram", extendedDiagram, shapeForJobHeader3.ParentShape);
			});
		}

		public void TestExecute_ForLinkedDiagramContainingShapesLinkedToOtherJobs_ShouldOpenNewDiagramAndCorrectlyShowAllRelations()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader2, "Test Workflow 3");

			var link1 = workflow1.GetOrCreateDependencyLink(workflow3);

			var diagram = CreateDiagram(jobHeader1);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);
			var workflowShape2 = CreateShape(workflow2, diagram);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 3, CountChildShapesDownHierarchy(diagram));
			AssertEquals("Precondition", 0, diagram.AllAttachments.Count);

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForJobHeader2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader2);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 6 shapes", 6, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 2 top level shapes", 2, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network shoud have 1 attachment", 1, extendedDiagram.AllAttachments.Count);

				AssertEquals("First Job Header should be a top level shape", extendedDiagram, shapeForJobHeader1.ParentShape);
				AssertNotEquals("Second Job Header should not have been nested inside first", shapeForJobHeader1, shapeForJobHeader2.ParentShape);
				AssertEquals("Second Job Header should be a top level shape", extendedDiagram, shapeForJobHeader2.ParentShape);
			});
		}

		#endregion

		#region Extended Diagrams For Scaled Diagrams

		public void TestShouldNotPromptTheUserAboutBuffers_AndShouldCreateExtendedDiagram_ForScaledDiagramsWithNoBuffers()
		{
			var diagram = CreateDiagram(Factory, "Diagram 1");
			var shape1 = CreateShape(diagram, "Shape 1");
			var shape2 = CreateShape(diagram, "Shape 2");
			var annotation1 = CreateShape(diagram, "Annotation 1", shapeType: ShapeTypeList.Codes.Annotation);

			Factory.Save();

			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create an extended diagram for the given shape?", "Confirmation Required"))
				.Returns(true);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);
			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);

			var extendedNetworkViewModel = CreateNetworkViewModel(extendedDiagram);
			var extendednetwork = extendedNetworkViewModel.GetJobNetwork();

			AssertContainsExactElementsInAnyOrder("Should copy shapes and annotations",
				new[] { "Diagram 1", "Shape 1", "Shape 2", "Annotation 1" }, extendednetwork.Shapes.Select(s => s.BNS_Name));
			controller.Verify(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create an extended diagram for the given shape?", "Confirmation Required"), Times.Once);
			controller.Verify(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the extended diagram. Do you want to proceed?", "Buffers cannot be recreated on extended diagrams"), Times.Never);
			controller.Verify(m => m.UserInteractionImplementor.HasUserAnsweredYes(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestShouldPromptTheUserAboutBuffers_AndShouldCreateExtendedDiagramWithNoBuffers_ForScaledDiagramsWithBuffers_WhenUserAnsweredYes()
		{
			var diagram = CreateDiagram(Factory, "Diagram 1");
			var shape1 = CreateShape(diagram, "Shape 1");
			var shape2 = CreateShape(diagram, "Shape 2");
			var annotation1 = CreateShape(diagram, "Annotation 1", shapeType: ShapeTypeList.Codes.Annotation);
			var buffer1 = CreateShape(diagram, "Buffer 1", shapeType: ShapeTypeList.Codes.Buffer);

			var attachment = Factory.New<BMNCNAttachment>();
			attachment.BNA_BNS_Owner = diagram.PK;
			attachment.BNA_BNS_FromShape = shape2.PK;
			attachment.BNA_BNS_ToShape = buffer1.PK;
			attachment.BNA_Type = AttachmentTypeList.Codes.Dependency;

			Factory.Save();

			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create an extended diagram for the given shape?", "Confirmation Required"))
				.Returns(true);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the extended diagram. Do you want to proceed?", "Buffers cannot be recreated on extended diagrams"))
				.Returns(true);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals("Precondition: should contain one attachement within the diagram", 1, diagram.AllAttachments.Count);
			AssertEquals("Precondition: should contain one attachement for the buffer", 1, buffer1.AsEntity(network).Attachments.Count);

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);
			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);

			var extendedNetworkViewModel = CreateNetworkViewModel(extendedDiagram);
			var extendednetwork = extendedNetworkViewModel.GetJobNetwork();

			AssertContainsExactElementsInAnyOrder("Should copy shapes and annotations only with no buffers",
				new[] { "Diagram 1", "Shape 1", "Shape 2", "Annotation 1" }, extendednetwork.Shapes.Select(s => s.BNS_Name));
			AssertEquals("Should contain no attachements for the buffer", 0, extendedDiagram.AllAttachments.Count);
			controller.Verify(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create an extended diagram for the given shape?", "Confirmation Required"), Times.Once);
			controller.Verify(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the extended diagram. Do you want to proceed?", "Buffers cannot be recreated on extended diagrams"), Times.Once);
		}

		public void TestShouldPromptTheUserAboutBuffers_AndShouldCancel_ForScaledDiagramWithBuffers_WhenUserAnsweredNo()
		{
			var diagram = CreateDiagram(Factory, "Diagram 1");
			var shape1 = CreateShape(diagram, "Shape 1");
			var shape2 = CreateShape(diagram, "Shape 2");
			var annotation1 = CreateShape(diagram, "Annotation 1", shapeType: ShapeTypeList.Codes.Annotation);
			var buffer1 = CreateShape(diagram, "Buffer 1", shapeType: ShapeTypeList.Codes.Buffer);

			var attachment = Factory.New<BMNCNAttachment>();
			attachment.BNA_BNS_Owner = diagram.PK;
			attachment.BNA_BNS_FromShape = shape2.PK;
			attachment.BNA_BNS_ToShape = buffer1.PK;
			attachment.BNA_Type = AttachmentTypeList.Codes.Dependency;

			Factory.Save();

			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create an extended diagram for the given shape?", "Confirmation Required"))
				.Returns(true);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the extended diagram. Do you want to proceed?", "Buffers cannot be recreated on extended diagrams"))
				.Returns(false);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;

			AssertNull("Should cancel diagram creation", factoryForExtendedNetwork);
			controller.Verify(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create an extended diagram for the given shape?", "Confirmation Required"), Times.Once);
			controller.Verify(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the extended diagram. Do you want to proceed?", "Buffers cannot be recreated on extended diagrams"), Times.Once);
		}

		#endregion

		#region Saving/Updating

		public void TestExecute_ForDiagram_ThenChangeInitialDiagram_ShouldNotAffectNewDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2");

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";
			var workflowShape1 = CreateShape(workflow1, diagram);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions("Preconditions", () =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network shoud have 3 shapes", 3, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
			});

			var workflowShape2 = CreateShape(workflow2, diagram);

			Factory.Save();

			extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertEquals("Initial Diagram should now have 3 shapes", 3, CountChildShapesDownHierarchy(diagram));
				AssertNotNull("Extended network should still exist", extendedDiagram);
				AssertEquals("Extended network shoud still have 3 shapes", 3, CountChildShapesDownHierarchy(extendedDiagram));
				AssertNull("Extended network should still not be saved in the Database", databaseDiagram);
			});
		}

		public void TestExecute_ForDiagram_ThenChangeNewDiagram_ShouldNotAffectInitialDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2");

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";
			var workflowShape1 = CreateShape(workflow1, diagram);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions("Preconditions", () =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network shoud have 3 shapes", 3, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
			});

			var workflowShape2 = factoryForExtendedNetwork.New<BMNCNShape>();
			workflowShape2.BNS_ShapeType = ShapeTypeList.Codes.Shape;
			workflowShape2.MakeChildOf(extendedDiagram);

			Factory.Save();

			extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should still exist", extendedDiagram);
				AssertEquals("Extended network should now have 4 shapes", 4, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Initial diagram should still have 2 shapes", 2, CountChildShapesDownHierarchy(diagram));
				AssertNull("Extended network should still not be saved in the Database", databaseDiagram);
			});
		}
		#endregion

		#region Nesting Conditions

		public void TestExecute_ForNewShapeWithJobHeaderPresent_ShouldBePlacedWithinExistingJobHeader()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 2");

			var link1 = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);

			Factory.Save();

			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForWorkflow2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow2);

			CombineAssertions(() =>
			{
				AssertEquals("Extended network should have 4 shapes", 4, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);

				AssertEquals("First Job Header should be a top level shape", extendedDiagram, shapeForJobHeader1.ParentShape);
				AssertEquals("Second workflow should be nested in existing job header", shapeForJobHeader1, shapeForWorkflow2.ParentShape);
			});
		}

		public void TestExecute_ForNewShapeWithJobHeaderPresent_AndExistingParentShape_ShouldBePlacedWithinExistingParentShape()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 2");

			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow2, workflow1);

			var diagram = CreateDiagram(jobHeader1);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);

			Factory.Save();

			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForWorkflow1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow1);
			var shapeForWorkflow2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow2);

			CombineAssertions(() =>
			{
				AssertEquals("Extended network should have 4 shapes", 4, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);

				AssertEquals("First Job Header should be a top level shape", extendedDiagram, shapeForJobHeader1.ParentShape);
				AssertEquals("First workflow should be nested in existing job header", shapeForJobHeader1, shapeForWorkflow1.ParentShape);
				AssertEquals("Second workflow should be nested in workflow 1", shapeForWorkflow1, shapeForWorkflow2.ParentShape);
			});
		}

		public void TestExecute_ForNewShapeWithJobHeaderMissing_AndParentShapeIsLinkedEntity_ShouldCreateJobHeaderAsTopLevelShape_AndNestNewShapeWithinNewJobHeader()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Test Workflow 2");

			workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);

			Factory.Save();

			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);

			var shapeForJobHeader2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader2);
			var shapeForWorkflow2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow2);

			CombineAssertions(() =>
			{
				AssertEquals("Extended network should have 5 shapes", 5, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 2 top level shapes", 2, extendedDiagram.ChildShapes.Count);

				AssertEquals("Second Job Header should be a top level shape", extendedDiagram, shapeForJobHeader2.ParentShape);
				AssertEquals("Second workflow should be nested in second job header", shapeForJobHeader2, shapeForWorkflow2.ParentShape);
			});
		}

		public void TestExecute_ForNewShapeWithJobHeaderMissing_AndParentShapeIsNotLinkedEntity_ShouldCreateJobHeaderWithinParentShape_AndNestNewShapeWithinNewJobHeader()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Test Workflow 2");

			jobHeader1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(Factory);
			diagram.BNS_Name = "Unlinked Diagram";

			var jobHeaderShape1 = CreateShape(jobHeader1, diagram);

			Factory.Save();

			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var shapeForPreviousUnlinkedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, "Unlinked Diagram");

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForJobHeader2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader2);
			var shapeForWorkflow2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow2);

			CombineAssertions(() =>
			{
				AssertEquals("Extended network should have 5 shapes", 5, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);

				AssertEquals("Previous unlinked diagram should be a top level shape", extendedDiagram, shapeForPreviousUnlinkedDiagram.ParentShape);
				AssertEquals("First Job Header should be nested in unlinked shape", shapeForPreviousUnlinkedDiagram, shapeForJobHeader1.ParentShape);
				AssertEquals("Second Job Header should be nested in unlinked shape", shapeForPreviousUnlinkedDiagram, shapeForJobHeader2.ParentShape);
				AssertEquals("Second workflow should be nested in second job header", shapeForJobHeader2, shapeForWorkflow2.ParentShape);
			});
		}

		public void TestExecute_ForNewJobHeaderShape_AndParentShapeIsLinkedEntity_ShouldCreateJobHeaderOnlyOnce_AndPlaceAsTopLevelShape()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");

			workflow1.GetOrCreateDependencyLink(jobHeader2);

			var diagram = CreateDiagram(jobHeader1);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);

			Factory.Save();

			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForJobHeader2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader2);
			var shapeForWorkflow1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, workflow1);

			CombineAssertions(() =>
			{
				AssertEquals("Extended network should have 4 shapes", 4, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 2 top level shapes", 2, extendedDiagram.ChildShapes.Count);

				AssertEquals("First Job Header should be a top level shape", extendedDiagram, shapeForJobHeader1.ParentShape);
				AssertEquals("Second Job Header should be a top level shape", extendedDiagram, shapeForJobHeader2.ParentShape);

				AssertEquals("Second Job header should only be created once", 1, factoryForExtendedNetwork.Load<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, jobHeader2.PK)).Length);
			});
		}

		public void TestExecute_ForNewJobHeaderShape_AndParentShapeIsNotLinkedEntity_ShouldCreateJobHeaderOnlyOnce_AndPlaceWithinParentShape()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			var diagram = CreateDiagram(Factory);
			diagram.BNS_Name = "Unlinked Diagram";

			var jobHeaderShape1 = CreateShape(jobHeader1, diagram);

			Factory.Save();

			AssertEquals("Precondition", 2, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var shapeForPreviousUnlinkedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, "Unlinked Diagram");

			var shapeForJobHeader1 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader1);
			var shapeForJobHeader2 = LoadShapeFromFactoryByLinkedEntity(factoryForExtendedNetwork, jobHeader2);

			CombineAssertions(() =>
			{
				AssertEquals("Extended network should have 4 shapes", 4, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);

				AssertEquals("Previous unlinked diagram should be a top level shape", extendedDiagram, shapeForPreviousUnlinkedDiagram.ParentShape);
				AssertEquals("First Job Header should be nested in unlinked shape", shapeForPreviousUnlinkedDiagram, shapeForJobHeader1.ParentShape);
				AssertEquals("Second Job Header should be nested in unlinked shape", shapeForPreviousUnlinkedDiagram, shapeForJobHeader2.ParentShape);

				AssertEquals("Second Job header should only be created once", 1, factoryForExtendedNetwork.Load<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, jobHeader2.PK)).Length);
			});
		}

		#endregion

		#region Circular Dependencies and Hierarchies

		#region Circular Dependencies

		public void TestExecute_ForDiagramWithSimplestCircularDependency_ShouldOpenDiagram()
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
				AssertEquals("Diagram type", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
				AssertEquals("Shapes", 3, CountChildShapesDownHierarchy(diagram));
				AssertEquals("Attachments", 0, diagram.AllAttachments.Count);
			});

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);
			var extendedNetworkViewModel = CreateNetworkViewModel(extendedDiagram);
			var extendedNetwork = extendedNetworkViewModel.GetJobNetwork();

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 4 shapes", 4, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
				AssertEquals("Extended network should have 2 attachments", 2, extendedDiagram.AllAttachments.Count);
			});
		}

		public void TestExecute_ForDiagramWithCircularDependencies_ShouldOpenDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 3");

			var link1to2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2to3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3to1 = workflow3.GetOrCreateDependencyLink(workflow1); //Circular dependency

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);
			var workflowShape2 = CreateShape(workflow2, diagram);

			var workflowAttachment1 = CreateDependencyAttachment(diagram, link1to2, workflowShape1, workflowShape2);

			Factory.Save();

			CombineAssertions("Preconditions on process headers", () =>
			{
				AssertEquals("jobHeader children", 3, jobHeader.ChildHeaders.Count());
				AssertEquals("workflow1", 1, workflow1.PostrequisiteLinks.Count(l => l.HeaderTo == workflow2));
				AssertEquals("workflow2", 1, workflow2.PostrequisiteLinks.Count(l => l.HeaderTo == workflow3));
				AssertEquals("workflow3", 1, workflow3.PostrequisiteLinks.Count(l => l.HeaderTo == workflow1));
				AssertEquals("The graph should have the cycle", false, jobHeader.DependencyGraph.IsDirectedAcyclicGraph());
			});

			CombineAssertions("Preconditions on diagram", () =>
			{
				AssertEquals("Diagram type", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
				AssertEquals("Shapes", 3, CountChildShapesDownHierarchy(diagram));
				AssertEquals("Attachments", 1, diagram.AllAttachments.Count);
			});

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);
			var extendedNetworkViewModel = CreateNetworkViewModel(extendedDiagram);
			var extendedNetwork = extendedNetworkViewModel.GetJobNetwork();

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 5 shapes", 5, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
			});
		}

		#endregion

		#region Circular Hierarchies

		public void TestExecute_ForDiagramWithCircularHierarchy_ShouldDisplayErrorAndNotOpenDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 3");

			var link1 = BufferManagement.Business.Test.BMSTestHelper.MakeChildOfAndGetLink(workflow2, workflow1);
			var link2 = BufferManagement.Business.Test.BMSTestHelper.MakeChildOfAndGetLink(workflow3, workflow2);
			var link3 = BufferManagement.Business.Test.BMSTestHelper.MakeChildOfAndGetLink(workflow1, workflow3); //Circular hierarchy

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);
			var workflowShape2 = CreateShape(workflow2, workflowShape1);

			Factory.Save();

			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("Precondition", 3, CountChildShapesDownHierarchy(diagram));

			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.UserInteractionImplementor.ShowError("Cannot view Extended Network as cyclic hierarchy exists", "Error Creating Extended Network"));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			AssertNull("Extended network should not be created", extendedDiagram);
			AssertNull("Extended network should not be saved in the Database", databaseDiagram);
		}

		#endregion

		#region Circular References Made by Both Depedencies and Hierarchies

		public void TestExecute_ForDiagramWithCircularHierarchyAndDependency_ShouldDisplayErrorAndNotOpenDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Test Workflow 3");

			var link1 = BufferManagement.Business.Test.BMSTestHelper.MakeChildOfAndGetLink(workflow2, workflow1);
			var link2 = BufferManagement.Business.Test.BMSTestHelper.MakeChildOfAndGetLink(workflow3, workflow2);
			var link3 = BufferManagement.Business.Test.BMSTestHelper.MakeChildOfAndGetLink(workflow1, workflow3); //Circular hierarchy

			var link4 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link5 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link6 = workflow3.GetOrCreateDependencyLink(workflow1); //Circular dependency

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);

			Factory.Save();

			CombineAssertions("Preconditions on process headers", () =>
			{
				AssertEquals("jobHeader children", 0, jobHeader.ChildHeaders.Count());
				AssertEquals("workflow1", 1, workflow1.PostrequisiteLinks.Count(l => l.HeaderTo == workflow2));
				AssertEquals("workflow2", 1, workflow2.PostrequisiteLinks.Count(l => l.HeaderTo == workflow3));
				AssertEquals("workflow3", 1, workflow3.PostrequisiteLinks.Count(l => l.HeaderTo == workflow1));
				AssertEquals("We are not able to determine if we have a cyclic graph here as jobHeader no longer has children", true, jobHeader.DependencyGraph.IsDirectedAcyclicGraph());
			});

			CombineAssertions("Preconditions on diagram", () =>
			{
				AssertEquals("Diagram type", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
				AssertEquals("Shapes", 2, CountChildShapesDownHierarchy(diagram));
				AssertEquals("Attachments", 0, diagram.AllAttachments.Count);
			});

			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m =>
				m.UserInteractionImplementor.ShowError("Cannot view Extended Network as cyclic hierarchy exists",
					"Error Creating Extended Network"));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);

			AssertNull("Extended network should not be created", extendedDiagram);
			AssertNull("Extended network should not be saved in the Database", databaseDiagram);
		}

		public void TestExecute_ForDiagramWithCircularReferencesMadeByChainOfBothDepedenciesAndHierarchies_ShouldOpenDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Parent Workflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Child Workflow");
			var postreqWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Postreq Workflow");

			var linkParentToChild = BufferManagement.Business.Test.BMSTestHelper.MakeChildOfAndGetLink(childWorkflow, parentWorkflow);
			var linkChildToPostreq = childWorkflow.GetOrCreateDependencyLink(postreqWorkflow);
			var linkPostreqToParent = postreqWorkflow.GetOrCreateDependencyLink(parentWorkflow); //Circular dependency

			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_Name = "Test Diagram";

			var parentWorkflowShape = CreateShape(parentWorkflow, diagram);
			var childWorkflowShape = CreateShape(childWorkflow, parentWorkflowShape);

			Factory.Save();

			CombineAssertions("Preconditions on process headers", () =>
			{
				AssertEquals("jobHeader children", 2, jobHeader.ChildHeaders.Count());
				AssertEquals("childWorkflow", 1, childWorkflow.PostrequisiteLinks.Count(l => l.HeaderTo == postreqWorkflow));
				AssertEquals("postreqWorkflow", 1, postreqWorkflow.PostrequisiteLinks.Count(l => l.HeaderTo == parentWorkflow));
				AssertEquals("The graph should have no cycle (the cycle is indirect and not obvious)", true, jobHeader.DependencyGraph.IsDirectedAcyclicGraph());
			});

			CombineAssertions("Preconditions on diagram", () =>
			{
				AssertEquals("Diagram type", ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
				AssertEquals("Shapes", 3, CountChildShapesDownHierarchy(diagram));
				AssertEquals("Attachments", 0, diagram.AllAttachments.Count);
			});

			var controller = CreateMockableController(Mocks);
			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(diagram);

			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(l => l.Name == nameForExtendedNetwork)));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);
			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var extendedDiagram = LoadShapeFromFactoryByName(factoryForExtendedNetwork, nameForExtendedNetwork);
			var databaseDiagram = LoadShapeFromFactoryByName(new BusinessObjectFactory(), nameForExtendedNetwork);
			var extendedNetworkViewModel = CreateNetworkViewModel(extendedDiagram);
			var extendedNetwork = extendedNetworkViewModel.GetJobNetwork();

			CombineAssertions(() =>
			{
				AssertNotNull("Extended network should have been created successfully", extendedDiagram);
				AssertNull("Extended network should not be saved in the Database", databaseDiagram);
				AssertEquals("Extended network should be diagram type", ShapeTypeList.Codes.Diagram, extendedDiagram.BNS_ShapeType);
				AssertEquals("Extended network should have 5 shapes", 5, CountChildShapesDownHierarchy(extendedDiagram));
				AssertEquals("Extended network should have 1 top level shape", 1, extendedDiagram.ChildShapes.Count);
			});
		}

		#endregion

		#endregion

		#region Overrides

		protected override void TestGetNameCore()
		{
			AssertStaticName("Show Extended Network");
		}

		protected override void TestGetDescriptionCore()
		{
			AssertStaticDescription("Open a new Network Diagram automatically built with all workflows connected to this diagram or any of its nested shapes via pre-requisite relationships or parent/child relationships.");
		}

		protected override void TestIsApplicableCore()
		{
			AssertIsApplicableToAllDiagramsAndShapesOnly("Only supported for shapes and diagrams.");
		}

		protected override void TestIsEnabledCore()
		{
			AssertIsEnabledForAllDiagramsAndShapes();
		}

		public override void TestCanPerformOnApprovedDiagram()
		{
			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ViewDiagram(It.IsAny<INetworkEntity>()));
			using (ObjectFactory.Substitute(controller.Object))
			{
				base.TestCanPerformOnApprovedDiagram();
			}
		}

		public override void TestCanPerformOnApprovedShape()
		{
			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ViewDiagram(It.IsAny<INetworkEntity>()));

			using (ObjectFactory.Substitute(controller.Object))
			{
				base.TestCanPerformOnApprovedShape();
			}
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("ExtendedDiagram");
		}

		protected override ExtendedNetworkAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new ExtendedNetworkAction(networkViewModel);
		}

		#endregion

		#region Implementation

		int CountChildShapesDownHierarchy(BMNCNShape shape)
		{
			int count = 1;

			foreach (var child in shape.ChildShapes)
			{
				count += CountChildShapesDownHierarchy(child);
			}

			return count;
		}

		BMNCNShape LoadShapeFromFactoryByName(BusinessObjectFactory factory, string name) => factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, name));

		BMNCNShape LoadShapeFromFactoryByLinkedEntity(BusinessObjectFactory factory, ProcessHeader header) => factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, header.PK));

		#endregion
	}
}
