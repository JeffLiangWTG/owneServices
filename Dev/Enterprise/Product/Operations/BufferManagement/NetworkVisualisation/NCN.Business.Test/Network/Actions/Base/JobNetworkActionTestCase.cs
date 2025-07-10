using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestsSubclassesOf(typeof(JobNetworkActionBase))]
	abstract class JobNetworkActionTestCase<T> : NetworkTestCase
		where T : JobNetworkActionBase
	{
		public void TestDeletingShapeShouldNotCauseActionToGenerateExceptions()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			diagram.SwitchToScaled();

			var shape = CreateShape(diagram, "Shape");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var entity = shape.AsEntity(network);

			var action = GetAction(networkViewModel);

			networkViewModel.SelectEntities(new INetworkEntity[] { entity });

			var node = networkViewModel.GetNodeForEntity(entity);
			AssertNoExceptionThrown(() => networkViewModel.RemoveFromDiagram(node));
		}

		public void TestDeletingMultipleShapesShouldNotCauseActionToGenerateExceptions()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			diagram.SwitchToScaled();

			var shape1 = CreateShape(diagram, "Shape1");
			var shape2 = CreateShape(diagram, "Shape2");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var entity1 = shape1.AsEntity(network);
			var entity2 = shape2.AsEntity(network);

			var action = GetAction(networkViewModel);

			networkViewModel.SelectEntities(new INetworkEntity[] { entity1, entity2 });

			AssertNoExceptionThrown(() => new RemoveFromDiagramAction(networkViewModel).Execute());
		}

		public void TestExecute()
		{
			TestExecuteCore();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestIsApplicable()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			TestIsApplicableCore();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestIsEnabled()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			TestIsEnabledCore();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGetName()
		{
			TestGetNameCore();
		}

		public void TestGetDescription()
		{
			TestGetDescriptionCore();
		}

		public void TestGetIcon()
		{
			TestGetIconCore();
		}

		public virtual void TestCanPerformOnApprovedDiagram()
		{
			var diagram = Factory.NewWithValidTestData<BMNCNShape>();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ScrollPosition = ScrollPositionList.Codes.Default;
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			new ApproveDiagramAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals("Precondition", true, diagram.IsApproved);

			Factory.Save();

			var action = GetAction(networkViewModel);
			AssertApprovedShapeErrorMessage(action, network, diagram, "This action cannot be performed on an approved diagram.");
		}

		public virtual void TestCanPerformOnApprovedShape()
		{
			var diagram = Factory.NewWithValidTestData<BMNCNShape>();
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			new ApproveDiagramAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals("Precondition", true, diagram.IsApproved);

			Factory.Save();

			var action = GetAction(networkViewModel);
			AssertApprovedShapeErrorMessage(action, network, childShape, "This action cannot be performed on an approved shape.");
		}

		static void AssertApprovedShapeErrorMessage(JobNetworkActionBase action, IJobNetwork network, BMNCNShape shape, string expectedError)
		{
			if (action.IsEnabledAfterActivatingEntity_ForTest(shape).IsAllowed)
			{
				try
				{
					action.ExecuteAfterActivatingEntity_ForTest(shape);
				}
				catch
				{
				}

				if (action.CanPerformOnApprovedShape_ExposedForTesting)
				{
					AssertNotContains(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertContains(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			else
			{
				Assert("Action is disabled therefore PerformPreExecutionChecks is not called, so testing if it can perform on an approved something or not does not make sense.", true);
			}
		}

		public virtual void TestChildActions()
		{
			var diagram = Factory.NewWithValidTestData<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childActionsProperty = GetAction(networkViewModel).GetType().GetMethod("GetChildActionsCore", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			if (childActionsProperty != null)
			{
				TestChildActionsCore();
			}
			else
			{
				Assert("Action does not override GetChildActionsCore - nothing to test", true);
			}
		}

		protected virtual void TestChildActionsCore()
		{
			Fail("Must override TestChildActionsCore to test implementation of GetChildActionsCore");
		}

		protected abstract void TestExecuteCore();

		protected abstract void TestIsApplicableCore();

		protected abstract void TestIsEnabledCore();

		protected abstract void TestGetNameCore();

		protected abstract void TestGetDescriptionCore();

		protected T GetAction(INetworkViewModel networkViewModel)
		{
			var action = GetActionCore(networkViewModel);
			NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(networkViewModel, action);

			return action;
		}

		protected abstract T GetActionCore(INetworkViewModel networkViewModel);

		protected T GetAction() => GetAction(CreateNetworkViewModel(CreateDiagram(Factory)));

		protected abstract void TestGetIconCore();

		#region Execution Strategy

		public void TestExecutionStrategy()
		{
			var action = GetAction();
			AssertType(GetExpectedExecutionStrategyType(), action.ExecutionStrategy);
		}

		protected virtual Type GetExpectedExecutionStrategyType() => typeof(SingleEntityExecutionStrategy);

		#endregion

		#region Assertion Methods

		#region Accessibility Testing

		protected void AssertCannotExecuteInWorkflowRelationshipDesigner()
		{
			var setup = new ActionTestingSetup(Factory, GetAction);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("This action is not accessible in the Workflow Relationship Designer.", setup.ActionForDefaultDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.DefaultDiagram));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("This action is not accessible in the Workflow Relationship Designer.", setup.ActionForDefaultDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.DefaultWorkflow));
		}

		protected void AssertIsApplicableToAllDiagramsAndShapesOnly(string expectedErrorMessage = null)
		{
			var setup = new ActionTestingSetup(Factory, GetAction);

			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.NormalDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.NormalShape));

			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForDefaultDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.DefaultDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForDefaultDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.DefaultWorkflow));

			AssertNotApplicableToBuffers(expectedErrorMessage);
			AssertNotApplicableToAnnotations(expectedErrorMessage);
		}

		protected void AssertNotApplicableToBuffers(string expectedErrorMessage = null)
		{
			var setup = new ActionTestingSetup(Factory, GetAction);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(expectedErrorMessage ?? DefaultErrorMessageForBuffersAndAnnotations, setup.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.NormalBuffer));
		}

		protected void AssertNotApplicableToAnnotations(string expectedErrorMessage = null)
		{
			var setup = new ActionTestingSetup(Factory, GetAction);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(expectedErrorMessage ?? DefaultErrorMessageForBuffersAndAnnotations, setup.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.NormalAnnotation));
		}

		protected void AssertIsEnabledForAllDiagramsAndShapes()
		{
			var setup = new ActionTestingSetup(Factory, GetAction);

			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(setup.NormalDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(setup.NormalShape));

			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForDefaultDiagram.IsEnabledAfterActivatingEntity_ForTest(setup.DefaultDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForDefaultDiagram.IsEnabledAfterActivatingEntity_ForTest(setup.DefaultWorkflow));
		}

		protected void AssertIsApplicableToNonDefaultDiagramsAndShapesOnly(string expectedErrorMessage = null)
		{
			var setup = new ActionTestingSetup(Factory, GetAction);
			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.NormalDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.NormalShape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(expectedErrorMessage ?? DefaultErrorMessageForBuffersAndAnnotations, setup.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.NormalBuffer));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(expectedErrorMessage ?? DefaultErrorMessageForBuffersAndAnnotations, setup.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(setup.NormalAnnotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected void AssertIsEnabledForNonDefaultDiagramsAndShapes()
		{
			var setup = new ActionTestingSetup(Factory, GetAction);
			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(setup.NormalDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(setup.NormalShape));
		}

		protected void AssertIsEnabledForShapesOnly(string expectedErrorMessage = null)
		{
			var setup = new ActionTestingSetup(Factory, GetAction);
			NetworkActionAccessibilityTest.AssertAllowed(setup.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(setup.NormalShape));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(expectedErrorMessage ?? DefaultErrorMessageForBuffersAndAnnotations, setup.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(setup.NormalDiagram));
		}

		const string DefaultErrorMessageForBuffersAndAnnotations = "The shape should not be a buffer or annotation.";

		#endregion

		#region Main Properties Testing

		protected void AssertStaticName(string expectedName)
		{
			AssertStaticProperty(expectedName, (action, shape) => action.GetNameAfterActivatingEntity_ForTest(shape));
		}

		protected void AssertStaticDescription(string expectedDescription)
		{
			AssertStaticProperty(expectedDescription, (action, shape) => action.GetDescriptionAfterActivatingEntity_ForTest(shape));
		}

		void AssertStaticProperty<TValue>(TValue expectedValue, Func<T, BMNCNShape, TValue> valueGetter)
		{
			var setup = new ActionTestingSetup(Factory, GetAction);

			AssertStaticPropertyIfApplicableToShape(expectedValue, setup.ActionForNormalDiagram, setup.NormalDiagram, valueGetter);
			AssertStaticPropertyIfApplicableToShape(expectedValue, setup.ActionForNormalDiagram, setup.NormalShape, valueGetter);
			AssertStaticPropertyIfApplicableToShape(expectedValue, setup.ActionForNormalDiagram, setup.NormalAnnotation, valueGetter);
			AssertStaticPropertyIfApplicableToShape(expectedValue, setup.ActionForNormalDiagram, setup.NormalBuffer, valueGetter);

			AssertStaticPropertyIfApplicableToShape(expectedValue, setup.ActionForDefaultDiagram, setup.DefaultDiagram, valueGetter);
			AssertStaticPropertyIfApplicableToShape(expectedValue, setup.ActionForDefaultDiagram, setup.DefaultWorkflow, valueGetter);
		}

		void AssertStaticPropertyIfApplicableToShape<TValue>(TValue expectedValue, T action, BMNCNShape shape, Func<T, BMNCNShape, TValue> valueGetter)
		{
			if (action.IsApplicableToEntity(shape).IsAllowed)
			{
				AssertEquals(expectedValue, valueGetter(action, shape));
			}
			else
			{
				Assert("We shouldn't test properties of non-applicable network actions", true);
			}
		}

		protected void AssertActionHasCorrectIcon(string iconName)
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);

			AssertEquals(iconName, GetAction(networkViewModel).GetIconName());
		}

		protected class ActionTestingSetup
		{
			public ActionTestingSetup(BusinessObjectFactory factory, Func<NetworkViewModel, T> actionGetter)
			{
				NormalDiagram = CreateDiagram(factory, name: "NormalDiagram");

				NormalShape = CreateShape(NormalDiagram, name: "NormalShape");

				NormalAnnotation = CreateShape(NormalDiagram, shapeType: ShapeTypeList.Codes.Annotation, name: "NormalAnnotation");
				NormalBuffer = CreateShape(NormalDiagram, shapeType: ShapeTypeList.Codes.Buffer, name: "NormalBuffer");

				NetworkViewModelForNormalDiagram = CreateNetworkViewModel(NormalDiagram);
				NetworkForNormalDiagram = NetworkViewModelForNormalDiagram.GetJobNetwork();
				ActionForNormalDiagram = actionGetter(NetworkViewModelForNormalDiagram);

				var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory);
				var workflow = jobHeader.ProcessHeaders[0];

				DefaultDiagram = jobHeader.GetDefaultDiagram();
				DefaultDiagram.Name = "DefaultDiagram";

				DefaultWorkflow = workflow.GetDefaultShape(DefaultDiagram);

				NetworkViewModelForDefaultDiagram = CreateNetworkViewModel(DefaultDiagram);
				NetworkForDefaultDiagram = NetworkViewModelForDefaultDiagram.GetJobNetwork();
				ActionForDefaultDiagram = actionGetter(NetworkViewModelForDefaultDiagram);

				NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(NetworkViewModelForNormalDiagram, ActionForNormalDiagram);
				NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(NetworkViewModelForDefaultDiagram, ActionForDefaultDiagram);
			}

			public NetworkViewModel NetworkViewModelForNormalDiagram;
			public IJobNetwork NetworkForNormalDiagram;
			public T ActionForNormalDiagram;
			public BMNCNShape NormalDiagram;
			public BMNCNShape NormalShape;
			public BMNCNShape NormalAnnotation;
			public BMNCNShape NormalBuffer;

			public NetworkViewModel NetworkViewModelForDefaultDiagram;
			public IJobNetwork NetworkForDefaultDiagram;
			public T ActionForDefaultDiagram;
			public BMNCNShapeDefaultDiagram DefaultDiagram;
			public BMNCNShape DefaultWorkflow;
		}

		#endregion

		#region PreExecution Checks Testing

		protected void AssertNoNeedToTestItAsItIsContainerForOtherNetworkActions()
		{
			Assert("Should not execute as it's just a container for other network actions so there is no need to test it", true);
		}

		public void AssertDoesNotExecuteAndNotifyToTheUser_IfThereAreValidationErrors()
		{
			var diagram = CreateDiagram(Factory);

			var mocks = new MockRepository(MockBehavior.Default);
			var interactionImplementor = mocks.Create<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);
			var validator = mocks.Create<IJobNetworkValidator>();

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller, validator: validator.Object);
			var network = networkViewModel.GetJobNetwork();

			var shape = CreateShape(diagram);

			interactionImplementor.Setup(m => m.ShowError(@"This action cannot be executed for the given shape(s) due to the following reasons:
New Diagram: There are validation errors. Please fix these first.", "Action cannot be executed"));

			validator.Setup(m => m.Validate(It.IsAny<IJobNetwork>())).Callback(new Action<IJobNetwork>((n) =>
			 {
				 AssertEquals(network, n);
				 shape.AddRowError("This is a way to emulate adding an error during validation");
			 }));

			Assert("Precondition: should have no validation notifications on diagram shape", !network.DiagramShape.HasNotifications);
			Assert("Precondition: should have no validation notifications on diagram entity", !network.DiagramEntity.HasNotifications);

			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("There are validation errors. Please fix these first.", action.CheckCanStartExecution());
			action.Execute();
			Assert("Should find validation errors on diagram entity before execution", network.DiagramShape.HasErrors);

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>()
				, It.IsAny<ConfirmationNotification[]>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		protected void AssertRequiresSavingBeforeExecution()
		{
			var diagram = CreateDiagram(Factory);

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);

			Factory.Save();
			Assert("Precondition", !diagram.HasChanges);

			interactionImplementor
				.Setup(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ConfirmationNotification[]>()))
				.Returns(new Func<string, string, ConfirmationNotification[], bool>(
					(message, caption, notifications) =>
					{
						AssertContains("The form will attempt to save before performing this operation.", message);
						AssertEquals("Confirmation Required", caption);
						return true;
					}));

			var shape = CreateShape(diagram);
			Assert("Precondition", diagram.HasChanges);
			Assert("Precondition", !shape.IsInDatabase);

			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());

			Assert("The diagram should be saved", !diagram.HasChanges);

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		protected void AssertDoesNotRequireSavingBeforeExecution()
		{
			var diagram = CreateDiagram(Factory);

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);

			Factory.Save();
			Assert("Precondition", !diagram.HasChanges);

			interactionImplementor
				.Setup(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ConfirmationNotification[]>()))
				.Returns(new Func<string, string, ConfirmationNotification[], bool>((message, caption, notifications) =>
				{
					AssertNotContains("The form will attempt to save before performing this operation.", message);
					return true;
				}));

			var shape = CreateShape(diagram);
			Assert("Precondition", diagram.HasChanges);
			Assert("Precondition", !shape.IsInDatabase);

			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());

			Assert("The diagram should still not be saved", diagram.HasChanges);
			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		protected void AssertRequiresSpecificConfirmationBeforeExecution(string confirmationMessage)
		{
			var diagram = CreateDiagram(Factory);

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);

			interactionImplementor
				.Setup(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ConfirmationNotification[]>()))
				.Returns(new Func<string, string, ConfirmationNotification[], bool>((message, caption, notifications) =>
				{
					AssertContains(confirmationMessage, message);
					AssertEquals("Confirmation Required", caption);
					return true;
				}));

			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		#endregion

		#region Execution Testing

		protected void AssertSpawningActionDoesNotSaveSpawnedDiagram()
		{
			var diagram = CreateDiagram(Factory);

			var mocks = new MockRepository(MockBehavior.Default);
			var controller = CreateMockableControllerWithMockableInteractionImplementor(mocks);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);

			controller
				.Setup(m => m.ViewDiagram(It.IsAny<INetworkEntity>()))
				.Callback(new Action<INetworkEntity>((entity) =>
				{
					var shape = entity as BMNCNShape;
					AssertNotNull(shape);

					Assert("Should be a diagram", shape is BMNCNRootDiagramShape);

					Assert("Spawned diagram should not be saved", !shape.IsInDatabase);
					Assert("Spawned diagram should not be saved", shape.HasChanges);
				}));

			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed(It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<ConfirmationNotification[]>()))
				.Returns(true);

			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionAfterActivatingEntity_ForTest(diagram));
			action.Execute();
		}

		protected void AssertExecute(Action<Mock<IJobNetwork>, BMNCNShape> networkExpectationProvider, bool executeOnDiagram = false)
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var network = mocks.Create<IJobNetwork>();
			var controller = mocks.Create<IBMNetworkEntityController>();
			var interactionImplementor = mocks.Create<IBMNetworkUserInteractionImplementor>();
			var networkViewModel = mocks.Create<INetworkViewModel>();

			var diagram = CreateDiagram(Factory);
			diagram.BNS_JobType = "ORG";

			var childShape = CreateShape(diagram);
			childShape.Name = "Link to me too";

			networkViewModel.Setup(m => m.Network).Returns(network.Object);
			network.Setup(m => m.Controller).Returns(controller.Object);
			controller.Setup(m => m.UserInteractionImplementor).Returns(interactionImplementor.Object);
			networkExpectationProvider.Invoke(network, executeOnDiagram ? diagram : childShape);

			var action = GetAction(networkViewModel.Object);
			action.ExecuteForEntityWithoutAccessCheck(executeOnDiagram ? diagram : childShape);
		}

		#endregion

		#endregion
	}
}
