using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateJobAction : CreateJobActionBase
	{
		public CreateJobAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true, bool linkOnly = false)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
			LinkOnly = linkOnly;
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return (LinkOnly || !shape.IsDiagram ? BMNetworkActionAccessibilityHelper.CheckShapeIsNotLinkedToRealEntity(shape) : BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(shape))
					.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckShapeHasJobTypes(shape));
		}

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entityToExecute)
		{
			var shape = entityToExecute.AsShape();

			if (LinkOnly)
			{
				SetDefaultsForNewShapeCore(Network, shape, null);
			}
			else
			{
				if (base.ExecuteForEntityCore(entityToExecute) is INetworkEntity entity)
				{
					var newShape = entity.AsShape();

					if (!newShape.IsLinkedToRealEntity)
					{
						Network.DeleteEntity(newShape);
					}
				}
			}

			return null;
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("e8c3466e-fc2f-4cf9-998d-9e4f0496f081", "Create Job");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("d71accfd-f30b-4902-98ee-32209fbda583", "Creates and opens a new job");
		}

		protected bool LinkOnly { get; }
	}
}
