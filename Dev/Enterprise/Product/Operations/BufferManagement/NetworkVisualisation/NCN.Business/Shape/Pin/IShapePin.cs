using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public interface IShapePin : INetworkPin
	{
		ShapeNetworkEntity Ancestor { get; }
		double XOffset { get; }
		double YOffset { get; }
	}
}
