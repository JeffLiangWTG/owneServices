using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public abstract class JobNetworkAction : JobNetworkActionBase
	{
		protected JobNetworkAction(INetworkViewModel networkViewModel, bool shouldUpdateOnNetworkEvents = true)
			: this(networkViewModel, executionStrategy: null, group: 0, groupIndex: 0, shouldUpdateOnNetworkEvents)
		{
		}

		protected JobNetworkAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: this(networkViewModel, executionStrategy: null, group: group, groupIndex: groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected JobNetworkAction(INetworkViewModel networkViewModel, INetworkActionExecutionStrategy executionStrategy = null, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, executionStrategy, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected sealed override INetworkActionResult ExecuteForEntityCore(INetworkEntity entityToExecute)
		{
			ExecuteForShape(entityToExecute.AsShape());
			ExecuteForShapeNetworkEntity(entityToExecute as ShapeNetworkEntity);
			return null;
		}

		protected abstract void ExecuteForShape(BMNCNShape shape);

		protected virtual void ExecuteForShapeNetworkEntity(ShapeNetworkEntity shapeEntity) { }
	}
}
