using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockProjectItemsEvents : MarshalByRefObject, EnvDTE.ProjectItemsEvents
	{
		public event EnvDTE._dispProjectItemsEvents_ItemAddedEventHandler ItemAdded;
		public void FireItemAdded(EnvDTE.ProjectItem projectItem)
		{
			ItemAdded?.Invoke(projectItem);
		}

		public event EnvDTE._dispProjectItemsEvents_ItemRemovedEventHandler ItemRemoved;
		public void FireItemRemoved(EnvDTE.ProjectItem projectItem)
		{
			ItemRemoved?.Invoke(projectItem);
		}

		public event EnvDTE._dispProjectItemsEvents_ItemRenamedEventHandler ItemRenamed;
		public void FireItemRenamed(EnvDTE.ProjectItem projectItem, string oldName)
		{
			ItemRenamed?.Invoke(projectItem, oldName);
		}
	}
}
