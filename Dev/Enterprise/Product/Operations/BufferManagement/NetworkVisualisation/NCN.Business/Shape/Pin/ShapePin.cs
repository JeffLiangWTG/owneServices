using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapePin : IShapePin
	{
		public ShapeNetworkEntity Ancestor { get; set; }
		public double XOffset { get; set; }
		public double YOffset { get; set; }

		INetworkEntity INetworkPin.Owner
		{
			get { return Ancestor; }
		}
	}
}
