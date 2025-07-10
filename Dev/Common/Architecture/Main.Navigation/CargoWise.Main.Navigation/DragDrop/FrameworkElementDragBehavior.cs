using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace CargoWise.Main.Navigation.DragDrop;

public class FrameworkElementDragBehavior : Behavior<FrameworkElement>
{
	protected override void OnAttached()
	{
		base.OnAttached();

		AssociatedObject.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(AssociatedObject_PreviewMouseLeftButtonDown);
		AssociatedObject.PreviewMouseMove += new MouseEventHandler(AssociatedObject_PreviewMouseMove);
		AssociatedObject.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(AssociatedObject_PreviewMouseLeftButtonUp);
	}

	void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		movedFrom = e.GetPosition(AssociatedObject);
		isMousePressed = true;
	}

	void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		isMousePressed = false;
	}

	internal void AssociatedObject_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		if(ShouldAllowMouseMove(e))
		{
			DragInProcessOrAssociatedObjectIsNotDragable = true;
			if (AssociatedObject.DataContext is IDragable dragObject)
			{
				var data = new DataObject(dragObject);

				try
				{
#if DEBUG
					ThrowExceptionIfTestCOMException();
#endif
					DoDragDrop(data);
				}
				catch (COMException)
				{
					// There is no need to handle COMException here, as it indicates that the Drag & Drop action is invalid and has no effect. Therefore, we can simply skip it.
				}
				finally
				{
					DragInProcessOrAssociatedObjectIsNotDragable = false;
					isMousePressed = false;
				}
			}
		}
	}

	public virtual void DoDragDrop(DataObject data)
	{
		System.Windows.DragDrop.DoDragDrop(AssociatedObject, data, DragDropEffects.Move);
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		if(AssociatedObject != null)
		{
			AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
			AssociatedObject.PreviewMouseMove -= AssociatedObject_PreviewMouseMove;
			AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
		}
	}

	bool isMousePressed;
	internal bool IsMousePressed => isMousePressed;

	public bool MouseMovedFarEnough(Point movedTo)
	{
		return Math.Abs(movedTo.X - movedFrom.X) > SystemParameters.MinimumHorizontalDragDistance ||
			Math.Abs(movedTo.Y - movedFrom.Y) > SystemParameters.MinimumVerticalDragDistance;
	}

	public virtual bool ShouldAllowMouseMove(MouseEventArgs e)
	{
		return (e.LeftButton == MouseButtonState.Pressed && MouseMovedFarEnough(e.GetPosition(AssociatedObject)) && !DragInProcessOrAssociatedObjectIsNotDragable && IsMousePressed
#if DEBUG
|| isTestCOMException
#endif
			);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
	static bool DragInProcessOrAssociatedObjectIsNotDragable;
	internal Point movedFrom;

#if DEBUG
	internal bool isTestCOMException;
	void ThrowExceptionIfTestCOMException()
	{
		if (isTestCOMException)
		{
			throw new COMException("Just test COMException");
		}
	}
#endif

}
