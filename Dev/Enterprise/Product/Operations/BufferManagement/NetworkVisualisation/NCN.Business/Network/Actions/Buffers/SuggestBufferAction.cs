using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class SuggestBufferAction : JobNetworkAction
	{
		public SuggestBufferAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Overrides

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckDiagramIsScaled(Network.DiagramEntity, shape)
					.Union(BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(entity)));
		}

		protected override bool CanPerformOnApprovedShape => false;

		protected override bool RequiresSaveBeforeExecute => true;

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			if (HasBuffers(Network))
			{
				foreach (var buffer in Network.Shapes.Where(s => s.IsBufferShape).ToArray())
				{
					buffer.Delete();
				}
			}
			else if (!Network.GetCriticalChain().Any())
			{
				var message = Res.GetString("245a6f85-1e90-4e46-80cc-3717cd9b9fe6", "There is no critical chain in this diagram, so buffers cannot be added.");
				var caption = Res.GetString("7a2fea1b-389f-4207-822f-616c0282aa6c", "Cannot suggest buffers");

				UserInteractionImplementor.ShowMessage(message, caption);
			}
			else
			{
				AddSuggestedBuffers(Network);
			}

			Network.Refresh(RefreshType.RedrawDiagram);
		}

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("E1597DAD-59D7-420B-AC88-C391AB845749", "Suggest/Remove Buffers");

		protected override ResourceString GetNameCore(BMNCNShape shape)
		{
			return HasBuffers(Network)
				? ResString.GetMultilingualString("09c0563e-971f-4691-aa28-61b0132ccb45", "Remove All Buffers")
				: ResString.GetMultilingualString("6601dfc3-e865-47b7-b2d0-49a5200157c2", "Suggest Buffers");
		}

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("4044B1FF-8C16-4D1A-A98E-7C9C0620DF56", "Suggests relevant buffers or removes all buffer shapes from this diagram");

		protected override ResourceString GetDescriptionCore(BMNCNShape shape)
		{
			return HasBuffers(Network)
				? ResString.GetMultilingualString("85e908f0-b1da-4613-bf56-ee36143c0618", "Removes all buffer shapes from this diagram")
				: ResString.GetMultilingualString("06c6d1bd-e68d-4983-a7e6-6c83ea964f64", "Suggests relevant buffers to be inserted into the diagram");
		}

		protected override string IconName => (NoResString)"Buffer"; // resource name

		#endregion

		#region Implementation

		static bool HasBuffers(IJobNetwork network)
		{
			return network.Shapes.Any(s => s.IsBufferShape);
		}

		static void AddSuggestedBuffers(IJobNetwork network)
		{
			BufferCreator.AddProjectBuffer(network);
			BufferCreator.AddFeedingBuffers(network);
		}

		#endregion
	}
}
