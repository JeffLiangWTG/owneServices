using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Internal
{
	/// <summary>
	/// GridModuleFilterSavedLayoutUpdatedEvent.OnLayoutUpdated is invoked 
	/// when S9_FilterName and S9_SaveColumnLayout changes of a StmModuleFilter that was in db are saved successfully
	/// </summary>
	public class GridModuleFilterSavedLayoutUpdatedEvent : IService
	{
		public static void AddLayoutUpdatedEventHandler(BusinessObjectFactory factory, GridModuleFilterSavedLayoutUpdatedEventHandler eventHandler)
		{
			Get(factory).LayoutUpdated += eventHandler;
		}

		public static void RemoveLayoutUpdatedEventHandler(BusinessObjectFactory factory, GridModuleFilterSavedLayoutUpdatedEventHandler eventHandler)
		{
			GridModuleFilterSavedLayoutUpdatedEvent instance = factory.ServiceContainer.GetService<GridModuleFilterSavedLayoutUpdatedEvent>();

			if (instance != null)
			{
				instance.LayoutUpdated -= eventHandler;
			}
		}

		static GridModuleFilterSavedLayoutUpdatedEvent Get(BusinessObjectFactory factory)
		{
			GridModuleFilterSavedLayoutUpdatedEvent result = factory.ServiceContainer.GetService<GridModuleFilterSavedLayoutUpdatedEvent>();

			if (result == null)
			{
				result = new GridModuleFilterSavedLayoutUpdatedEvent();
				factory.ServiceContainer.AddService(result);
			}

			return result;
		}

		public static void OnLayoutUpdated(BusinessObjectFactory factory, StmModuleFilter filter, bool previousSaveColumnLayout)
		{
			OnLayoutUpdated(factory, new GridModuleFilterSavedLayoutUpdatedEventArgs(filter, previousSaveColumnLayout));
		}

		static void OnLayoutUpdated(BusinessObjectFactory factory, GridModuleFilterSavedLayoutUpdatedEventArgs eventArgs)
		{
			GridModuleFilterSavedLayoutUpdatedEvent instance = factory.ServiceContainer.GetService<GridModuleFilterSavedLayoutUpdatedEvent>();

			if (instance != null && instance.LayoutUpdated != null)
			{
				instance.LayoutUpdated(eventArgs);
			}
		}

		event GridModuleFilterSavedLayoutUpdatedEventHandler LayoutUpdated;

		public delegate void GridModuleFilterSavedLayoutUpdatedEventHandler(GridModuleFilterSavedLayoutUpdatedEventArgs args);
	}

	public class GridModuleFilterSavedLayoutUpdatedEventArgs : EventArgs
	{
		public GridModuleFilterSavedLayoutUpdatedEventArgs(StmModuleFilter filter, bool previousSaveColumnLayout)
		{
			this.filter = filter;
			this.previousSaveColumnLayout = previousSaveColumnLayout;

			if (previousSaveColumnLayout != filter.S9_SaveColumnLayout)
			{
				this.saveColumnLayoutOption = filter.S9_SaveColumnLayout ? SaveColumnLayout.Yes : SaveColumnLayout.No;
			}
			else
			{
				this.saveColumnLayoutOption = SaveColumnLayout.Ignore;
			}
		}

		public readonly StmModuleFilter filter;

		public readonly bool previousSaveColumnLayout;
		public readonly SaveColumnLayout saveColumnLayoutOption;
	}
}
