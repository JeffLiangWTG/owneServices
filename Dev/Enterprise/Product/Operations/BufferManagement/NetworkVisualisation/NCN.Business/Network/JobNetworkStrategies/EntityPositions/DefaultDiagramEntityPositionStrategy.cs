using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class DefaultDiagramEntityPositionStrategy : EntityPositionStrategy
	{
		public override IEnumerable<INetworkEntity> SetPositionsForNewEntities(IEnumerable<INetworkEntity> newEntities, IEnumerable<INetworkEntity> oldEntities, Location origin)
		{
			var entities = newEntities.Cast<ShapeNetworkEntity>();
			var list = new DisposableList(entities.Select(e => (BusinessObject)e.Shape).Select(b => b.SuspendSettingHasChanges()));

			try
			{
				return base.SetPositionsForNewEntities(newEntities, oldEntities, origin);
			}
			finally
			{
				list.Dispose();
			}
		}

		public override double DefaultHeight
		{
			get { return 150; }
		}
	}
}
