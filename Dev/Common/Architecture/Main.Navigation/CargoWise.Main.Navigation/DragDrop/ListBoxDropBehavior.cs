using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CargoWise.Common;
using Microsoft.Xaml.Behaviors;

namespace CargoWise.Main.Navigation.DragDrop;
public class ListBoxDropBehavior : Behavior<ItemsControl>
{
	public SolidColorBrush AdornerBrush { get; set; }

	protected override void OnAttached()
	{
		base.OnAttached();

		AssociatedObject.AllowDrop = true;
		AssociatedObject.DragEnter += new DragEventHandler(AssociatedObject_DragEnter);
		AssociatedObject.DragOver += new DragEventHandler(AssociatedObject_DragOver);
		AssociatedObject.DragLeave += new DragEventHandler(AssociatedObject_DragLeave);
		AssociatedObject.Drop += new DragEventHandler(AssociatedObject_Drop);
	}

	void AssociatedObject_Drop(object sender, DragEventArgs e)
	{
		var eventData = e.Data;
		if (TryGetData(eventData, out var source) && source != null)
		{
			var dropContainer = sender as ItemsControl;
			var droppedOverItem = UIHelper.GetUIElement(dropContainer, e.GetPosition(dropContainer));
			if (droppedOverItem != null)
			{
				var dropIndex = -1;
				dropIndex = dropContainer.ItemContainerGenerator.IndexFromContainer(droppedOverItem);

				(source as IDragable)?.Remove(source);
				(AssociatedObject.DataContext as IDropable)?.Drop(source, dropIndex, UIHelper.IsPositionAboveElement(droppedOverItem, e.GetPosition(droppedOverItem)));
			}
		}

		if (insertAdornerManager != null)
		{
			insertAdornerManager.Clear();
		}

		e.Handled = true;
	}

	internal bool TryGetData(IDataObject eventData, out object data)
	{
		var dataType = (AssociatedObject.DataContext as IDropable)?.DataType;
		if (dataType != null)
		{
			try
			{
				if (eventData.GetDataPresent(dataType))
				{
					data = eventData.GetData(dataType);
					return true;
				}
			}
			catch (COMException ex) 
			{
				if (ex.ErrorCode != unchecked((int)0x80040069))
				{
					// Do not need to handle invalid transfer mediums (HRESULT: 0x80040069 (DV_E_TYMED)), just skipping will not affect normal logic
					var menuSection = AssociatedObject.DataContext as MenuSection;
					ErrorReporter.ReportOnce("ListBoxDropBehavior_TryGetData_COMException",
	@$"DragDropException entries.
dataType of AssociatedObject is :  {dataType}
AssociatedObject:  {AssociatedObject}
DataContext: {menuSection?.Name}
DataContext is Recent Or Favorite: {menuSection?.IsRecentOrFavorite}
Count of DataContext: {menuSection?.Items?.Count}
eventData is null:{eventData}", ex);
				}
			}
		}

		data = null;
		return false;
	}

	void AssociatedObject_DragLeave(object sender, DragEventArgs e)
	{
		SetDragDropEffects(e);

		if (insertAdornerManager != null)
		{
			insertAdornerManager.Clear();
		}

		e.Handled = true;
	}

	void AssociatedObject_DragOver(object sender, DragEventArgs e)
	{
		SetDragDropEffects(e);

		if (insertAdornerManager != null && e.Effects != DragDropEffects.None)
		{
			var dropContainer = sender as ItemsControl;
			if (dropContainer != null)
			{
				var droppedOverItem = UIHelper.GetUIElement(dropContainer, e.GetPosition(dropContainer));
				if (droppedOverItem != null)
				{
					var isAboveElement = UIHelper.IsPositionAboveElement(droppedOverItem, e.GetPosition(droppedOverItem));
					insertAdornerManager.Update(droppedOverItem, isAboveElement);
				}
			}
		}

		e.Handled = true;
	}

	void AssociatedObject_DragEnter(object sender, DragEventArgs e)
	{
		SetDragDropEffects(e);

		if (insertAdornerManager == null && e.Effects != DragDropEffects.None)
		{
			var adLayer = AdornerLayer.GetAdornerLayer((ItemsControl)sender);
			var adBrush = AdornerBrush;
			insertAdornerManager = new ListBoxAdornerManager(adLayer, adBrush);
		}

		e.Handled = true;
	}

	internal void SetDragDropEffects(DragEventArgs e)
	{
		Argument.NotNull(e, nameof(e)); // Suggested By ReviewBot 
		Argument.NotNull(e.Data, nameof(e.Data)); // Suggested By ReviewBot 

		e.Effects = TryGetData(e.Data, out _) ? DragDropEffects.Move : DragDropEffects.None;
	}

	ListBoxAdornerManager insertAdornerManager;
}

