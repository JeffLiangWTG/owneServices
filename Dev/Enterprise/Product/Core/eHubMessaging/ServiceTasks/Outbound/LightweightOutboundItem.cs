using System;
using CargoWise.EntityFramework;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	abstract class LightweightOutboundItem<TFullItem>
		where TFullItem : BusinessObject
	{
		protected LightweightOutboundItem()
		{
		}

		public Guid PK => GetPK();
		public TFullItem FullItem => GetFullItem();

		protected abstract TFullItem GetFullItem();
		protected abstract Guid GetPK();
	}
}
