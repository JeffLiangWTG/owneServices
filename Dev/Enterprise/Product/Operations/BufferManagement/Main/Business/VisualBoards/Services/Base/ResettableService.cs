using System;
using CargoWise.Common;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public abstract class ResettableService<T> : IBoardFactoryService
	{
		protected ResettableService(Func<T> serviceDataCreator)
		{
			lazyServiceData = ResettableLazy.Create(serviceDataCreator, isThreadSafe: true);
		}

		readonly ResettableLazy<T> lazyServiceData;

		protected abstract BoardServiceStalenessPolicy StalenessPolicy { get; }

		protected T ServiceData => lazyServiceData.Value;

		#region IBoardFactoryService Members

		BoardServiceStalenessPolicy IBoardFactoryService.StalenessPolicy => StalenessPolicy;

		void IBoardFactoryService.ClearCache()
		{
			lazyServiceData.Reset();
		}

		#endregion
	}
}
