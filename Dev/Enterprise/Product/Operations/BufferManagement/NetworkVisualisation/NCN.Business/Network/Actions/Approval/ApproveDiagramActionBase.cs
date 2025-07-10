using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public abstract class ApproveDiagramActionBase : JobNetworkAction
	{
		protected ApproveDiagramActionBase(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		internal static IEnumerable<IApprovable> GetItemsRequiringApproval(ShapeNetworkEntity entity, bool expectedApprovalState = false)
		{
			return GetApprovalCandidates(entity).Where(a => a.IsApproved == expectedApprovalState);
		}

		static IEnumerable<IApprovable> GetApprovalCandidates(ShapeNetworkEntity entity)
		{
			var descendants = entity.Descendants().Cast<ShapeNetworkEntity>();

			foreach (var pivot in descendants.SelectMany(s => s.Attachments))
			{
				if (!pivot.IsDeleted)
				{
					var toShape = pivot.ToShape;

					if (toShape != null && toShape.CanApprove)
					{
						yield return pivot;
					}
				}
			}

			foreach (var node in descendants)
			{
				if (!node.IsDeleted && node.Shape.CanApprove)
				{
					yield return node;
				}
			}
		}
	}
}
