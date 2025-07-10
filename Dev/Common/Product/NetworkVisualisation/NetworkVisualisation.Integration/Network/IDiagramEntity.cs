using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Integration
{
	public enum ScrollPositionStates
	{
		None,
		FirstOpenShape,
		Start,
		Current
	}

	public interface IDiagramEntity : INetworkEntityWithChildren
	{
		bool IsDiagramScaled { get; }
		int Scale { get; }
		int ResolutionIncrement { get; }
		int ScaleUnitPixelSize { get; }
		bool IsDiagramSurfaceFixed { get; }
		bool ShouldShowNonScheduledSection { get; set; }
		double NonScheduledSectionWidth { get; set; }
		bool ShapeInspectorVisible { get; set; }

		ScrollPositionStates ScrollPositions { get; }

		IEnumerable<IDiagramChannel> DiagramChannels { get; }

		void CreateAffinityLink(INetworkEntity networkEntity, IAffinity affinity);
		void RemoveAffinityLink(INetworkEntity networkEntity, IAffinity affinity);
	}
}
