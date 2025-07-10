using System;
using System.Globalization;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	static class JobNetworkStrategyProvider
	{
		internal static InitialisationStrategyBase GetInitialisationStrategy(BMNetworkViewModel viewModel)
		{
			return viewModel.DiagramShape.IsDefaultDiagram
				? new DefaultDiagramInitialisationStrategy(viewModel)
				: new JobNetworkInitialisationStrategy(viewModel);
		}

		internal static JobNetworkUpdateEntitiesStrategy GetUpdateEntitiesStrategy(JobNetwork jobNetwork)
		{
			return jobNetwork.DiagramShape.IsScaled ? new ScaledDiagramUpdateEntitiesStrategy(jobNetwork)
				: jobNetwork.DiagramShape.IsDefaultDiagram ? new DefaultDiagramUpdateEntitiesStrategy(jobNetwork)
				: new JobNetworkUpdateEntitiesStrategy(jobNetwork);
		}

		internal static JobNetworkSupportedActionsStrategy GetSupportedActionsStrategy(JobNetwork jobNetwork)
		{
			return jobNetwork.DiagramShape.IsDefaultDiagram ? new DefaultDiagramSupportedActionsStrategy()
				: new JobNetworkSupportedActionsStrategy();
		}

		internal static IEntityPositionStrategy GetPositionStrategy(BMNetworkViewModel viewModel)
		{
			if (viewModel.DiagramShape.IsDeleted)
			{
				return new JobNetworkEntityPositionStrategy();
			}
			else
			{
				return viewModel.DiagramShape.IsScaled ? new ScaledDiagramEntityPositionStrategy()
					: viewModel.DiagramShape.IsDefaultDiagram ? new DefaultDiagramEntityPositionStrategy()
					: new JobNetworkEntityPositionStrategy();
			}
		}

		internal static DiagramLinkingStrategy GetLinkingStrategy(BMNCNShape shape)
		{
			switch (shape.BNS_ShapeType)
			{
				case ShapeTypeList.Codes.Diagram:
				case ShapeTypeList.Codes.Shape:
					return new DiagramLinkingStrategy();

				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to find a strategy for managing links for shape type: {0}", shape.BNS_ShapeType));
			}
		}

		internal static EntityCountChangedStrategy GetEntityCountChangedStrategy(JobNetwork network)
		{
			return (!network.DiagramShape.IsDeleted && network.DiagramShape.IsDefaultDiagram) ? new DefaultDiagramEntityCountChangedStrategy()
				: new EntityCountChangedStrategy();
		}

		internal static JobNetworkSaveStrategy GetSaveStrategy(JobNetwork network)
		{
			return network.DiagramEntity.IsScaled ? new ScaledSaveStrategy(network)
				: new JobNetworkSaveStrategy(network);
		}
	}
}
