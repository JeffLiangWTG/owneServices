using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	public class JobNetworkActionBaseTest : NetworkTestCase
	{
		#region Default Values

		public void TestShouldUseDefaultValues_WhenNotOverridden()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = new DynamicActionsWithDefaultValuesOnly(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsApplicable());

			AssertEquals("Default name", action.GetName());
			AssertEquals("Default description", action.GetDescription());
			AssertEquals(true, action.IsActivated());
			AssertContainsExactElementsInAnyOrder(new INetworkAction[] { null }, action.GetChildActions());
		}

		public void TestShouldUseDefaultValues_WhenNotApplicable()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var isApplicable = NetworkActionAccessibility.Denied_ForTesting;
			var action = new DynamicActionToTestDefaultValues(networkViewModel, applicabilityGetter: () => isApplicable);
			NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(networkViewModel, action);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", "Not allowed.", action.IsApplicable());

			AssertEquals("Default name", action.GetName());
			AssertEquals("Default description", action.GetDescription());
			AssertEquals(true, action.IsActivated());
			AssertContainsExactElementsInAnyOrder(new INetworkAction[] { null }, action.GetChildActions());

			isApplicable = NetworkActionAccessibility.Allowed;
			networkViewModel.Network.Refresh(RefreshType.Affinities);
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsApplicable());

			AssertEquals("Dynamic name", action.GetName());
			AssertEquals("Dynamic description", action.GetDescription());
			AssertEquals(false, action.IsActivated());
			AssertContainsExactElementsInAnyOrder(new INetworkAction[] { null, null }, action.GetChildActions());
		}

		#endregion

		#region Saving before Execution

		public void TestShouldSaveBeforeExecution_IfSavingIsRequired_WhenDiagramIsSavedAndHasChanges()
		{
			var mocks = new MockRepository(MockBehavior.Default);

			var diagram = CreateDiagram(Factory);
			var controller = CreateMockableControllerWithMockableInteractionImplementor(mocks);

			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("The form will attempt to save before performing this operation.", "Confirmation Required"))
				.Returns(true);

			controller
				.Setup(m => m.TriggerSaveAction())
				.Returns(CargoWise.EntityFramework.ContinueWithSave.Yes);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			CreateShape(diagram); // let's make some changes

			var action = new ActionThatRequiresSavingBeforeExecution(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();
		}

		public void TestShouldNotSaveBeforeExecution_IfSavingIsRequired_WhenDiagramIsSavedAndHasNoChanges()
		{
			var mocks = new MockRepository(MockBehavior.Default);

			var diagram = CreateDiagram(Factory);
			var controller = CreateMockableControllerWithMockableInteractionImplementor(mocks);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			var action = new ActionThatRequiresSavingBeforeExecution(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();
			controller.Verify(m => m.UserInteractionImplementor.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			controller.Verify(m => m.TriggerSaveAction(), Times.Never());
		}

		public void TestShouldSaveBeforeExecution_IfSavingIsRequired_WhenDiagramIsNotSavedAndHasChanges()
		{
			var mocks = new MockRepository(MockBehavior.Default);

			var diagram = CreateDiagram(Factory);
			var controller = CreateMockableControllerWithMockableInteractionImplementor(mocks);

			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed(
					"The form will attempt to save before performing this operation.", "Confirmation Required"))
				.Returns(true);

			controller
				.Setup(m => m.TriggerSaveAction())
				.Returns(CargoWise.EntityFramework.ContinueWithSave.Yes);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			CreateShape(diagram); // let's make some changes

			var action = new ActionThatRequiresSavingBeforeExecution(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();
		}

		public void TestShouldSaveBeforeExecution_IfSavingIsRequired_WhenDiagramIsNotSavedAndHasNoChanges()
		{
			var mocks = new MockRepository(MockBehavior.Default);

			var diagram = CreateDiagram(Factory);
			var controller = CreateMockableControllerWithMockableInteractionImplementor(mocks);

			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("The form will attempt to save before performing this operation.", "Confirmation Required"))
				.Returns(true);

			controller
				.Setup(c => c.TriggerSaveAction())
				.Returns(CargoWise.EntityFramework.ContinueWithSave.Yes);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = new ActionThatRequiresSavingBeforeExecution(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();
		}

		public void TestShouldNotSaveBeforeExecution_IfSavingIsNotRequired()
		{
			var mocks = new MockRepository(MockBehavior.Default);

			var diagram = CreateDiagram(Factory);
			var controller = CreateMockableControllerWithMockableInteractionImplementor(mocks);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			CreateShape(diagram); // let's make some changes

			var action = new ActionThatDoesNotRequireSavingBeforeExecution(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();

			controller.Verify(m => m.UserInteractionImplementor.HasUserConfirmed(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ConfirmationNotification[]>()), Times.Never());
			controller.Verify(m => m.TriggerSaveAction(), Times.Never());
		}

		#endregion

		#region Validation

		#region Validation before Execution - CheckCanStartExecution Stage

		public void TestShouldValidateBeforeExecution_IfValidationIsRequired()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape = CreateShape(diagram);

			using (Factory.TemporarilyDisableValidation())
			{
				shape.BNS_Name = string.Empty;
			}
			Assert("Precondition: should have no validation errors as validation is not run yet", !network.DiagramShape.HasErrors);

			var action = new ActionThatRequiresValidationBeforeExecution(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("There are validation errors. Please fix these first.", action.CheckCanStartExecutionForNetwork());
			Assert("Should run validation and find errors", network.DiagramShape.HasErrors);
		}

		public void TestShouldNotValidateBeforeExecution_IfValidationIsNotRequired()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape = CreateShape(diagram);

			using (Factory.TemporarilyDisableValidation())
			{
				shape.BNS_Name = string.Empty;
			}
			Assert("Precondition: should have no validation errors as validation is not run yet", !network.DiagramShape.HasErrors);

			var action = new ActionThatDoesNotRequireValidationBeforeExecution(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());
			Assert("Should still have no validation errors as validation was skipped", !network.DiagramShape.HasErrors);

			Assert("Just to confirm that validation rules were violated - validation should not pass", !network.ValidateAndCheckThereAreNoErrors());
			Assert("Just to confirm that validation rules were violated - should find errors", network.DiagramShape.HasErrors);
		}

		public void TestShouldNotPromptTheUser_BeforeValidation()
		{
			var diagram = CreateDiagram(Factory);

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);
			var network = networkViewModel.GetJobNetwork();

			var shape = CreateShape(diagram);

			using (Factory.TemporarilyDisableValidation())
			{
				shape.BNS_Name = string.Empty;
			}
			Assert("Precondition: should have no validation errors as validation is not run yet", !network.DiagramShape.HasErrors);

			var action = new ActionThatRequiresValidationBeforeExecution(networkViewModel);

			action.CheckCanStartExecutionForNetwork();
			Assert("Should run validation and find errors", network.DiagramShape.HasErrors);

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ConfirmationNotification[]>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		#endregion

		#region Overall Execution in the Context of Validation

		public void TestShouldExecuteWithoutNotificationsToTheUser_IfValidationIsNotRequired()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);
			var network = networkViewModel.GetJobNetwork();

			using (Factory.TemporarilyDisableValidation())
			{
				shape.BNS_Name = string.Empty;
			}
			Assert("Precondition: should have no validation errors on shapes as validation is not run yet", !network.DiagramShape.HasErrors);
			Assert("Precondition: should have no validation errors on entities as validation is not run yet", !network.DiagramEntity.HasErrors);

			AssertEquals("Precondition: this is how we can determine if the action was executed or not", "Bisque", shape.BackColor);

			var action = new ActionThatDoesNotRequireValidationBeforeExecution(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(shape);
			Assert("Should still have no validation errors on shapes as validation was skipped", !network.DiagramShape.HasErrors);
			Assert("Should still have no validation errors on entities as validation was skipped", !network.DiagramEntity.HasErrors);

			AssertEquals("Action should execute", "Black", shape.BackColor);

			Assert("Just to confirm that validation rules were violated - validation should not pass", !network.ValidateAndCheckThereAreNoErrors());
			Assert("Just to confirm that validation rules were violated - should find errors", network.DiagramShape.HasErrors);

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>()
				, It.IsAny<ConfirmationNotification[]>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		public void TestShouldExecuteWithoutNotificationsToTheUser_IfValidationIsRequired_AndThereAreNeitherValidationErrorsNorWarnings()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram, "Shapie");

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);
			var network = networkViewModel.GetJobNetwork();

			Assert("Precondition: should have no validation notifications on shapes", !network.DiagramShape.HasNotifications);
			Assert("Precondition: should have no validation notifications on entities", !network.DiagramEntity.HasNotifications);

			AssertEquals("Precondition: this is how we can determine if the action was executed or not", "Bisque", shape.BackColor);

			var action = new ActionThatRequiresValidationBeforeExecution(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(shape);
			AssertEquals("Action should execute", "Black", shape.BackColor);

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ConfirmationNotification[]>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		public void TestShouldExecuteWithoutNotificationsToTheUser_IfValidationIsRequired_AndThereAreValidationWarningsOnly()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram, "Shapie");

			var mocks = new MockRepository(MockBehavior.Default);
			var interactionImplementor = mocks.Create<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);
			var validator = mocks.Create<IJobNetworkValidator>();

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller, validator: validator.Object);
			var network = networkViewModel.GetJobNetwork();

			var entity = shape.AsEntity(network);

			validator.Setup(v => v.Validate(It.IsAny<IJobNetwork>())).Callback(new Action<IJobNetwork>((n) =>
			{
				AssertEquals(network, n);
				shape.AddRowWarning("This is a way to emulate adding a warning on a shape during validation");
				entity.AddRowWarning("This is a way to emulate adding a warning on an shape entity during validation");
			}));

			Assert("Precondition: should have no validation notifications on shapes", !network.DiagramShape.HasNotifications);
			Assert("Precondition: should have no validation notifications on entities", !network.DiagramEntity.HasNotifications);

			AssertEquals("Precondition: this is how we can determine if the action was executed or not", "Bisque", shape.BackColor);

			var action = new ActionThatRequiresValidationBeforeExecution(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(shape);
			Assert("Should find validation notifications on shapes before execution", network.DiagramShape.HasNotifications);
			Assert("Should find validation warnings on shapes before execution", network.DiagramShape.HasWarnings);
			Assert("Should find no validation errors on shapes before execution", !network.DiagramShape.HasErrors);

			Assert("Should find validation notifications on entities before execution", network.DiagramEntity.HasNotifications);
			Assert("Should find validation warnings on entities before execution", network.DiagramEntity.HasWarnings);
			Assert("Should find no validation errors on entities before execution", !network.DiagramEntity.HasErrors);

			AssertEquals("Action should execute regardless warnings", "Black", shape.BackColor);

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>()
				, It.IsAny<ConfirmationNotification[]>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		public void TestShouldNotExecuteAndNotifyToTheUser_IfValidationIsRequired_AndThereAreValidationErrorsOnShape()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram, "Shapie");

			var mocks = new MockRepository(MockBehavior.Default);
			var interactionImplementor = mocks.Create<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);
			var validator = mocks.Create<IJobNetworkValidator>();

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller, validator: validator.Object);
			var network = networkViewModel.GetJobNetwork();

			interactionImplementor.Setup(m => m.ShowError(@"This action cannot be executed for the given shape(s) due to the following reasons:
New Diagram: There are validation errors. Please fix these first.", "Action cannot be executed"));

			validator.Setup(v => v.Validate(It.IsAny<IJobNetwork>())).Callback(new Action<IJobNetwork>((n) =>
			{
				AssertEquals(network, n);
				shape.AddRowError("This is a way to emulate adding an error on a shape during validation");
			}));

			Assert("Precondition: should have no validation notifications on shapes", !network.DiagramShape.HasNotifications);
			Assert("Precondition: should have no validation notifications on entities", !network.DiagramEntity.HasNotifications);

			AssertEquals("Precondition: this is how we can determine if the action was executed or not", "Bisque", shape.BackColor);

			var action = new ActionThatRequiresValidationBeforeExecution(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(shape);
			Assert("Should find validation errors on shapes before execution", network.DiagramShape.HasErrors);
			Assert("Should not find validation errors on entities before execution", !network.DiagramEntity.HasErrors);

			AssertEquals("Action should not execute", "Bisque", shape.BackColor);

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>()
				, It.IsAny<ConfirmationNotification[]>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		public void TestShouldNotExecuteAndNotifyToTheUser_IfValidationIsRequired_AndThereAreValidationErrorsOnEntity()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram, "Shapie");

			var mocks = new MockRepository(MockBehavior.Default);
			var interactionImplementor = mocks.Create<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);
			var validator = mocks.Create<IJobNetworkValidator>();

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller, validator: validator.Object);
			var network = networkViewModel.GetJobNetwork();

			var entity = shape.AsEntity(network);

			interactionImplementor.Setup(i => i.ShowError(@"This action cannot be executed for the given shape(s) due to the following reasons:
New Diagram: There are validation errors. Please fix these first.", "Action cannot be executed"));

			validator.Setup(m => m.Validate(It.IsAny<IJobNetwork>())).Callback(new Action<IJobNetwork>((n) =>
			{
				AssertEquals(network, n);
				entity.AddRowError("This is a way to emulate adding an error on an entity during validation");
			}));

			Assert("Precondition: should have no validation notifications on shapes", !network.DiagramShape.HasNotifications);
			Assert("Precondition: should have no validation notifications on entities", !network.DiagramEntity.HasNotifications);

			AssertEquals("Precondition: this is how we can determine if the action was executed or not", "Bisque", shape.BackColor);

			var action = new ActionThatRequiresValidationBeforeExecution(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(shape);
			Assert("Should find no validation errors on shapes before execution", !network.DiagramShape.HasErrors);
			Assert("Should find validation errors on entities before execution", network.DiagramEntity.HasErrors);

			AssertEquals("Action should not execute", "Bisque", shape.BackColor);

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>()
				, It.IsAny<ConfirmationNotification[]>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		public void TestShouldNotExecuteAndNotifyToTheUser_IfValidationIsRequired_AndThereWereValidationErrorsAlready()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(Factory, CreateRefresher(), interactionImplementor.Object);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);
			var network = networkViewModel.GetJobNetwork();

			interactionImplementor.Setup(m => m.ShowError(@"This action cannot be executed for the given shape(s) due to the following reasons:
New Diagram: There are validation errors. Please fix these first.", "Action cannot be executed"));

			shape.BNS_Name = string.Empty;
			Assert("Precondition: should have validation errors", network.DiagramShape.HasErrors);

			var action = new ActionThatRequiresValidationBeforeExecution(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(shape);
			AssertEquals("Action should not execute", "Bisque", shape.BackColor);

			AssertNull("There should be no other notifications except those that go through the user interaction implementor", UnitTestUserNotification.Instance.LastMessage.Text);
			interactionImplementor.Verify(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>()
				, It.IsAny<ConfirmationNotification[]>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>()), Times.Never());
		}

		#endregion

		#endregion

		#region Test Classes

		class DynamicActionsWithDefaultValuesOnly : JobNetworkActionBase
		{
			public DynamicActionsWithDefaultValuesOnly(INetworkViewModel networkViewModel)
				: base(networkViewModel)
			{
			}

			protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("CFE6CC2E-02A4-4690-A98B-7971AAA87D2C", "Default name");

			protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("79EA51A8-7188-4BB7-84D3-51382874B044", "Default description");

			protected override bool GetDefaultIsActivatedCore() => true;

			protected override IEnumerable<INetworkAction> GetDefaultChildActionsCore() => new INetworkAction[] { null };

			protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
			{
				throw new NotImplementedException();
			}
		}

		class DynamicActionToTestDefaultValues : DynamicActionsWithDefaultValuesOnly
		{
			public DynamicActionToTestDefaultValues(INetworkViewModel networkViewModel, Func<INetworkActionAccessibility> applicabilityGetter)
				: base(networkViewModel)
			{
				this.applicabilityGetter = applicabilityGetter;
			}

			readonly Func<INetworkActionAccessibility> applicabilityGetter;

			protected override ResourceString GetNameCore(BMNCNShape shape)
			{
				if (applicabilityGetter().IsAllowed)
				{
					return ResString.GetMultilingualString("E8434E22-D6ED-492B-816E-B08CC6D42B3F", "Dynamic name");
				}
				throw new InvalidOperationException();
			}

			protected override ResourceString GetDescriptionCore(BMNCNShape shape)
			{
				if (applicabilityGetter().IsAllowed)
				{
					return ResString.GetMultilingualString("A91663DA-9C95-4A26-8136-4739E2BA2C8E", "Dynamic description");
				}
				throw new InvalidOperationException();
			}

			protected override bool IsActivatedCore(BMNCNShape shape)
			{
				if (applicabilityGetter().IsAllowed)
				{
					return false;
				}
				throw new InvalidOperationException();
			}

			protected override IEnumerable<INetworkAction> GetChildActionsCore(BMNCNShape shape)
			{
				if (applicabilityGetter().IsAllowed)
				{
					return new INetworkAction[] { null, null };
				}
				throw new InvalidOperationException();
			}

			protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) => applicabilityGetter();

			protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
			{
				throw new NotImplementedException();
			}
		}

		class SimpleTestAction : JobNetworkActionBase
		{
			public SimpleTestAction(INetworkViewModel networkViewModel)
				: base(networkViewModel)
			{
			}

			protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
			{
				entity.BackColor = Color.Black;
				return null;
			}

			protected override ResourceString GetDefaultNameCore() => null;

			protected override ResourceString GetDefaultDescriptionCore() => null;

			protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape) => NetworkActionAccessibility.Allowed;
		}

		class ActionThatRequiresValidationBeforeExecution : SimpleTestAction
		{
			public ActionThatRequiresValidationBeforeExecution(INetworkViewModel networkViewModel)
				: base(networkViewModel)
			{
			}

			protected override bool RequiresValidateBeforeExecute => true;
		}

		class ActionThatDoesNotRequireValidationBeforeExecution : SimpleTestAction
		{
			public ActionThatDoesNotRequireValidationBeforeExecution(INetworkViewModel networkViewModel)
				: base(networkViewModel)
			{
			}

			protected override bool RequiresValidateBeforeExecute => false;
		}

		class ActionThatRequiresSavingBeforeExecution : SimpleTestAction
		{
			public ActionThatRequiresSavingBeforeExecution(INetworkViewModel networkViewModel)
				: base(networkViewModel)
			{
			}

			protected override bool RequiresSaveBeforeExecute => true;
		}

		class ActionThatDoesNotRequireSavingBeforeExecution : SimpleTestAction
		{
			public ActionThatDoesNotRequireSavingBeforeExecution(INetworkViewModel networkViewModel)
				: base(networkViewModel)
			{
			}

			protected override bool RequiresSaveBeforeExecute => false;
		}

		#endregion
	}
}
