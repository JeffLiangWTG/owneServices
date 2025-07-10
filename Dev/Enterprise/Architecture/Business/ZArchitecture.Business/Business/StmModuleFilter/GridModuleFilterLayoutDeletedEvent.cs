using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Internal
{
	/// <summary>
	/// GridModuleFilterLayoutDeletedEvent.LayoutDeleted is invoked when a StmModuleFilter is deleted.
	/// </summary>
	public class GridModuleFilterLayoutDeletedEvent : IService
	{
		public static void AddLayoutDeletedEventHandler(BusinessObjectFactory factory, GridModuleFilterLayoutDeletedEventHandler eventHandler)
		{
			Get(factory).LayoutDeleted += eventHandler;
		}

		public static void RemoveLayoutDeletedEventHandler(BusinessObjectFactory factory, GridModuleFilterLayoutDeletedEventHandler eventHandler)
		{
			GridModuleFilterLayoutDeletedEvent instance = factory.ServiceContainer.GetService<GridModuleFilterLayoutDeletedEvent>();

			if (instance != null)
			{
				instance.LayoutDeleted -= eventHandler;
			}
		}

		static GridModuleFilterLayoutDeletedEvent Get(BusinessObjectFactory factory)
		{
			GridModuleFilterLayoutDeletedEvent result = factory.ServiceContainer.GetService<GridModuleFilterLayoutDeletedEvent>();

			if (result == null)
			{
				result = new GridModuleFilterLayoutDeletedEvent();
				factory.ServiceContainer.AddService(result);
			}

			return result;
		}

		public static void OnLayoutDeleted(BusinessObjectFactory factory, ZGuid layoutPk)
		{
			GridModuleFilterLayoutDeletedEvent instance = factory.ServiceContainer.GetService<GridModuleFilterLayoutDeletedEvent>();

			if (instance != null && instance.LayoutDeleted != null)
			{
				instance.LayoutDeleted(new GridModuleFilterLayoutDeletedEventArgs(layoutPk));
			}
		}

		event GridModuleFilterLayoutDeletedEventHandler LayoutDeleted;

		public delegate void GridModuleFilterLayoutDeletedEventHandler(GridModuleFilterLayoutDeletedEventArgs args);
	}

	public class GridModuleFilterLayoutDeletedEventArgs : EventArgs
	{
		public GridModuleFilterLayoutDeletedEventArgs(ZGuid layoutPk)
		{
			this.layoutPk = layoutPk;
		}

		public readonly ZGuid layoutPk;
	}
}
