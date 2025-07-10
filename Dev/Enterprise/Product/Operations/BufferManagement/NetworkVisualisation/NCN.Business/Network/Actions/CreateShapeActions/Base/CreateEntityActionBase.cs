using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public abstract class CreateEntityActionBase : JobNetworkActionBase
	{
		protected CreateEntityActionBase(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, executionStrategy: null, group: group, groupIndex: groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Implementation

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entityToExecute)
		{
			return CreateShape(entityToExecute.AsShape());
		}

		protected virtual INetworkActionResult CreateShape(BMNCNShape parentShape)
		{
			using (((IBusinessObjectCollection)parentShape.ChildShapes).SuspendListChanged())
			{
				var result = parentShape.ChildShapes.AddNew();
				SetDefaultsForNewShape(Network, result, parentShape);

				return Network.AddNewShape(result);
			}
		}

		protected override bool RequiresSaveBeforeExecute
		{
			get { return false; }
		}

		protected abstract void SetDefaultsForNewShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape);

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeCanHaveChildren(shape);
		}

		#endregion
	}
}
