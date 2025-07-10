namespace CargoWise.NetworkVisualisation.Business
{
	abstract class CoordinateStrategyProvider
	{
		protected internal CoordinatePropertyStrategyBase X { get; protected set; }
		protected internal CoordinatePropertyStrategyBase Y { get; protected set; }
		protected internal CoordinatePropertyStrategyBase Width { get; protected set; }
		protected internal CoordinatePropertyStrategyBase Height { get; protected set; }
	}
}
