using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class SetStatusActionExecutionStrategy : MultipleSelectedEntitiesExecutionStrategy
	{
		protected override IEnumerable<INetworkEntity> GetEntitiesToExecuteCore(IDynamicNetworkAction action)
		{
			return SortEntitiesFromChildrenToParents(base.GetEntitiesToExecuteCore(action));
		}

		IEnumerable<INetworkEntity> SortEntitiesFromChildrenToParents(IEnumerable<INetworkEntity> entities)
		{
			return entities.OrderByDescending(e => GetEntityGeneration(e, e)).ToArray();
		}

		int GetEntityGeneration(IProposedNetworkEntity entity, IProposedNetworkEntity startingEntity) =>
			entity.Parent == startingEntity
			? -1 // potential case of a circular hierarchy (in this case generation is not defined)
			: entity.Parent == null
				? 0
				: GetEntityGeneration(entity.Parent, startingEntity) + 1;
	}
}
