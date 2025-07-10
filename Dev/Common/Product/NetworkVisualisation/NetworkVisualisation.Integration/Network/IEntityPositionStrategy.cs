using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface IEntityPositionStrategy
	{
		double ConvertXToPixels(double x);
		double ConvertYToPixels(double y);

		double ConvertPixelsToX(double value);
		double ConvertPixelsToY(double value);

		double DefaultWidth { get; }
		double DefaultHeight { get; }

		Location DefaultPosition(INetworkEntity entity);
		IEnumerable<INetworkEntity> SetPositionsForNewEntities(IEnumerable<INetworkEntity> entities, IEnumerable<INetworkEntity> allEntities, Location origin);
		Location GetPositionForNewEntities(Location lastCreatedNodeLocationRelativeToViewport, ViewportRect viewport, INetworkViewModel networkViewModel);

		double LeftMargin { get; }
		double TopMargin { get; }
		double RightMargin { get; }
		double BottomMargin { get; }
	}
}
