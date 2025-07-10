using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.DragDrop.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class FrameworkElementDragBehaviorTest : TestCase
{
	FrameworkElementDragBehavior frameworkElementDragBehavior;

	protected override void SetUp()
	{
		frameworkElementDragBehavior = new FrameworkElementDragBehavior();
	}

	[RequiresSTA]
	public void TestMinimumDistance()
	{
		using (var dragDropTestHelper = new DragDropTestHelper())
		{
			dragDropTestHelper.ShowWindow();
			DragDropTestHelper.DoEvents();
			dragDropTestHelper.AttachDragBehavior();
			var firstElement = dragDropTestHelper.GetItemsControlItem(0);
			var firstBehavior = dragDropTestHelper.DragBehaviors[0];
			var mouseEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
			{ RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent, Source = firstElement };
			firstElement.RaiseEvent(mouseEventArgs);
			DragDropTestHelper.DoEvents();
			var movedFrom = GetMovedFromForTest();
			Assert("Not moved far enough", !MouseMovedFarEnoughForTest(new Point(movedFrom.X + (SystemParameters.MinimumHorizontalDragDistance / 2), movedFrom.Y + (SystemParameters.MinimumVerticalDragDistance / 2))));
			Assert("Moved far enough", MouseMovedFarEnoughForTest(new Point(movedFrom.X + SystemParameters.MinimumHorizontalDragDistance + 1, movedFrom.Y + SystemParameters.MinimumVerticalDragDistance + 1)));
		}
	}

	[RequiresSTA]
	public void TestAssociatedObject_PreviewMouseMoveIgnoresCOMException()
	{
		using (var dragDropTestHelper = new DragDropTestHelper())
		{
			dragDropTestHelper.ShowWindow();
			DragDropTestHelper.DoEvents();
			dragDropTestHelper.AttachDragBehavior();

			var firstElement = dragDropTestHelper.GetItemsControlItem(0);

			dragDropTestHelper.DragBehaviors[0].isTestCOMException = true;

			var mouseMoveEventArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0)
			{
				RoutedEvent = UIElement.PreviewMouseMoveEvent,
				Source = firstElement
			};

			AssertNoExceptionThrown(() => firstElement.RaiseEvent(mouseMoveEventArgs));
		}
	}

	public bool MouseMovedFarEnoughForTest(Point movedTo)
	{
		return frameworkElementDragBehavior.MouseMovedFarEnough(movedTo);
	}

	public Point GetMovedFromForTest()
	{
		return frameworkElementDragBehavior.movedFrom;
	}

	[RequiresSTA]
	public void TestAllowDragBehavioursOnMouseMoveEvents()
	{
		using (var dragDropTestHelper = new DragDropTestHelper())
		{
			dragDropTestHelper.ShowWindow();
			DragDropTestHelper.DoEvents();
			dragDropTestHelper.AttachDragBehavior();
			var draggableElement = dragDropTestHelper.GetItemsControlItem(0);

			var mockFrameworkElementDragBehavior = new Mock<FrameworkElementDragBehavior>();
			mockFrameworkElementDragBehavior.Setup(mockFrameworkElementDragBehavior => mockFrameworkElementDragBehavior.ShouldAllowMouseMove(It.IsAny<MouseEventArgs>())).Returns(true);

			Interaction.GetBehaviors(draggableElement).Add(mockFrameworkElementDragBehavior.Object);

			mockFrameworkElementDragBehavior.Object.AssociatedObject_PreviewMouseMove(draggableElement, new MouseEventArgs(Mouse.PrimaryDevice, 0)
			{
				RoutedEvent = UIElement.PreviewMouseMoveEvent,
				Source = draggableElement
			});

			AssertNoExceptionThrown(() => mockFrameworkElementDragBehavior.Verify(frameworkElementDragBehavior => frameworkElementDragBehavior.DoDragDrop(It.IsAny<DataObject>()), Times.Once, "Drag should have occured"));
		}
	}

	[RequiresSTA]
	public void TestDenyDragBehavioursOnMouseMoveEvents()
	{
		using (var dragDropTestHelper = new DragDropTestHelper())
		{
			dragDropTestHelper.ShowWindow();
			DragDropTestHelper.DoEvents();
			dragDropTestHelper.AttachDragBehavior();
			var draggableElement = dragDropTestHelper.GetItemsControlItem(0);
			((DragableForTest)draggableElement.DataContext).DragHappened = false;

			var mockFrameworkElementDragBehavior = new Mock<FrameworkElementDragBehavior>();

			Interaction.GetBehaviors(draggableElement).Add(mockFrameworkElementDragBehavior.Object);

			mockFrameworkElementDragBehavior.Setup(mockFrameworkElementDragBehavior => mockFrameworkElementDragBehavior.ShouldAllowMouseMove(It.IsAny<MouseEventArgs>())).Returns(false);

			mockFrameworkElementDragBehavior.Object.AssociatedObject_PreviewMouseMove(draggableElement, new MouseEventArgs(Mouse.PrimaryDevice, 0)
			{
				RoutedEvent = UIElement.PreviewMouseMoveEvent,
				Source = draggableElement
			});

			AssertNoExceptionThrown(() => mockFrameworkElementDragBehavior.Verify(frameworkElementDragBehavior => frameworkElementDragBehavior.DoDragDrop(It.IsAny<DataObject>()), Times.Never, "Drag should not have occured"));
		}
	}

	[RequiresSTA]
	public void TestPreviewMouseButtonDownEvents()
	{
		using (var dragDropTestHelper = new DragDropTestHelper())
		{
			dragDropTestHelper.ShowWindow();
			DragDropTestHelper.DoEvents();
			dragDropTestHelper.AttachDragBehavior();
			var draggableElement = dragDropTestHelper.GetItemsControlItem(0);

			var previewMouseLeftButtonDownEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
			{
				RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
				Source = draggableElement
			};

			Interaction.GetBehaviors(draggableElement).Add(frameworkElementDragBehavior);

			draggableElement.RaiseEvent(previewMouseLeftButtonDownEventArgs);

			Assert("Left button press inside Associated Object should set IsMousePressed as True", frameworkElementDragBehavior.IsMousePressed);

			var previewMouseLeftButtonUpEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
			{
				RoutedEvent = UIElement.PreviewMouseLeftButtonUpEvent,
				Source = draggableElement
			};

			draggableElement.RaiseEvent(previewMouseLeftButtonUpEventArgs);

			Assert("Left button up inside Associated Object should set IsMousePressed as False", !frameworkElementDragBehavior.IsMousePressed);
		}
	}
}
