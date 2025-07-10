using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using CargoWise.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.DragDrop.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class ListBoxDropBehaviorTest : TestCase
{
	[RequiresSTA]
	public void TestDragAndDrop()
	{
		using (var dragDropTestHelper = new DragDropTestHelper())
		{
			dragDropTestHelper.ShowWindow();
			DragDropTestHelper.DoEvents();
			dragDropTestHelper.AttachDragBehavior();
			Assert("Drop has not occurred", !dragDropTestHelper.DataContext.DropHappened);
			var firstElement = dragDropTestHelper.GetItemsControlItem(0);
			var thirdElement = dragDropTestHelper.GetItemsControlItem(2);
			var point = new Point(1, 1);
			Assert("Drag has not occurred", !((DragableForTest)thirdElement.DataContext).DragHappened);
			var dragEnterArgs = dragDropTestHelper.GetMockDropEventArgs(thirdElement.DataContext, firstElement, point, System.Windows.DragDrop.DragEnterEvent);
			dragDropTestHelper.ItemsControl.RaiseEvent(dragEnterArgs);
			DragDropTestHelper.DoEvents();
			var dropEventsArgs = dragDropTestHelper.GetMockDropEventArgs(thirdElement.DataContext, firstElement, point, System.Windows.DragDrop.DropEvent);
			dragDropTestHelper.ItemsControl.RaiseEvent(dropEventsArgs);
			DragDropTestHelper.DoEvents();
			Assert("Drop has occurred", dragDropTestHelper.DataContext.DropHappened);
			Assert("Drag has occurred", ((DragableForTest)thirdElement.DataContext).DragHappened);
		}
	}

	[RequiresSTA]
	public void TestTryGetData()
	{
		using (var dragDropTestHelper = new DragDropTestHelper())
		{
			var dataObject = new Mock<IDataObject>();
			dataObject.Setup(x => x.GetDataPresent(It.IsAny<Type>())).Returns(false);
			var result = dragDropTestHelper.DropBehavior.TryGetData(dataObject.Object, out var source);
			Assert("should return false if cannot get data present", !result);
			AssertNull(source);

			dataObject.Setup(x => x.GetDataPresent(It.IsAny<Type>())).Returns(true);
			var tymedException = new COMException("Mocked DV_E_TYMED exception", unchecked((int)0x80040069));
			dataObject.Setup(x => x.GetData(It.IsAny<Type>())).Throws(tymedException);
			result = dragDropTestHelper.DropBehavior.TryGetData(dataObject.Object, out source);
			Assert("should return false if encounter DV_E_TYMED COMException while getting data", !result);
			AssertNull(source);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			dataObject.Setup(x => x.GetData(It.IsAny<Type>())).Throws(new COMException());
			result = dragDropTestHelper.DropBehavior.TryGetData(dataObject.Object, out source);
			Assert("should return false if encounter other COMException while getting data", !result);
			AssertNull(source);
			AssertEquals("ListBoxDropBehavior_TryGetData_COMException", ErrorReporter.LastKeyReported);

			dataObject.Setup(x => x.GetData(It.IsAny<Type>())).Returns(dataObject.Object);
			result = dragDropTestHelper.DropBehavior.TryGetData(dataObject.Object, out source);
			Assert("should get data successfully", result);
			AssertEquals(dataObject.Object, source);

			ErrorReporter.Clear();
		}
	}

	[RequiresSTA]
	public void TestSetDragDropEffects()
	{
		using (var dragDropTestHelper = new DragDropTestHelper())
		{
			var dataObject = new Mock<IDataObject>();
			dataObject.Setup(x => x.GetDataPresent(It.IsAny<Type>())).Returns(false);
			var point = new Point(1, 1);
			var firstElement = dragDropTestHelper.GetItemsControlItem(0);
			var dragEnterArgs = dragDropTestHelper.GetMockDropEventArgs(dragDropTestHelper.DataContext, firstElement, point, System.Windows.DragDrop.DropEvent);
			dragDropTestHelper.DropBehavior.SetDragDropEffects(dragEnterArgs);
			AssertEquals(DragDropEffects.Move, dragEnterArgs.Effects);
			dragEnterArgs = dragDropTestHelper.GetMockDropEventArgs(dragDropTestHelper.DataContext, firstElement, point, System.Windows.DragDrop.DropEvent, dataObject.Object);
			dragDropTestHelper.DropBehavior.SetDragDropEffects(dragEnterArgs);
			AssertEquals(DragDropEffects.None, dragEnterArgs.Effects);
		}
	}
}

#region Helpers

[System.Diagnostics.Contracts.ContractVerification(false)]
internal class DragDropTestHelper : IDisposable
{
	public DropableForTest DataContext { get; internal set; }
	public ItemsControl ItemsControl { get; internal set; }
	public Window Window { get; internal set; }
	public ListBoxDropBehavior DropBehavior { get; }
	public FrameworkElementDragBehavior[] DragBehaviors { get; internal set; }

	public DragDropTestHelper()
	{
		DataContext = new DropableForTest();

		ItemsControl = new ItemsControl
		{
			DataContext = DataContext,
			ItemsSource = DataContext.Items,
			ItemsPanel = GetItemsPanel(),
			ItemTemplate = GetItemTemplate()
		};

		DropBehavior = new ListBoxDropBehavior();
		DropBehavior.Attach(ItemsControl);

		Window = new Window { Content = new Grid { Children = { ItemsControl } }, Left = 0, Top = 0, WindowStartupLocation = WindowStartupLocation.Manual, WindowState = WindowState.Maximized };
	}

	public void ShowWindow()
	{
		Window.Show();
	}

	public void FocusWindow()
	{
		Window.Activate();
	}

	public FrameworkElement GetItemsControlItem(int index)
	{
		return ItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
	}

	public void AttachDragBehavior()
	{
		DragBehaviors = new FrameworkElementDragBehavior[ItemsControl.Items.Count];

		for (var i = 0; i < ItemsControl.Items.Count; i++)
		{
			var element = ItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
			if (element != null)
			{
				DragBehaviors[i] = new FrameworkElementDragBehavior();
				DragBehaviors[i].Attach(element);
			}
		}
	}

	public static void DoEvents()
	{
		var frame = new DispatcherFrame();
		Action doExit = () => frame.Continue = false;
		Dispatcher.CurrentDispatcher.BeginInvoke(doExit, DispatcherPriority.ApplicationIdle);
		Dispatcher.PushFrame(frame);
	}

	ItemsPanelTemplate GetItemsPanel()
	{
		var factoryPanel = new FrameworkElementFactory(typeof(StackPanel));
		factoryPanel.SetValue(StackPanel.IsItemsHostProperty, true);

		return new ItemsPanelTemplate { VisualTree = factoryPanel };
	}

	DataTemplate GetItemTemplate()
	{
		var template = new DataTemplate(typeof(DragableForTest));

		var factoryPanel = new FrameworkElementFactory(typeof(StackPanel));

		var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
		textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
		textBlockFactory.SetValue(TextBlock.HeightProperty, 50.0);
		factoryPanel.AppendChild(textBlockFactory);

		template.VisualTree = factoryPanel;

		return template;
	}

	public void Dispose()
	{
		if (Window != null)
		{
			Window.Close();
		}
	}

	public DragEventArgs GetMockDropEventArgs(object dataContext, DependencyObject target, Point dropPoint, RoutedEvent routedEvent, IDataObject dataObject = null)
	{
#if NET
		var mockEventArgs = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(DragEventArgs)) as DragEventArgs;
#else
#pragma warning disable RS0030 // Type or member is obsolete - fixed as per above for NET core
		var mockEventArgs = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(DragEventArgs)) as DragEventArgs;
#pragma warning restore RS0030 // Type or member is obsolete
#endif
		var fields = typeof(DragEventArgs).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		foreach (var field in fields)
		{
			if (field != null)
			{
				switch (field.Name)
				{
					case "_data":
						if (dataObject != null)
						{
							field.SetValue(mockEventArgs, dataObject);
						}
						else
						{
							var data = new DataObject();
							data.SetData(typeof(DragableForTest), dataContext);
							field.SetValue(mockEventArgs, data);
						}
						break;

					case "_target":
						field.SetValue(mockEventArgs, target);
						break;

					case "_dropPoint":
						field.SetValue(mockEventArgs, dropPoint);
						break;

					case "_allowedEffects":
					case "_effects":
						field.SetValue(mockEventArgs, DragDropEffects.Move);
						break;

					case "_dragDropKeyStates":
						field.SetValue(mockEventArgs, DragDropKeyStates.None);
						break;
				}
			}
		}

		mockEventArgs.RoutedEvent = routedEvent;
		mockEventArgs.Source = ItemsControl;

		return mockEventArgs;
	}
}

#endregion



