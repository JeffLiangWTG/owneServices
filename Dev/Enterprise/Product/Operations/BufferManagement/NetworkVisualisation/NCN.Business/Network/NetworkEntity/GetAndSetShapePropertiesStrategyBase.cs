namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public abstract class GetAndSetShapePropertiesStrategyBase
	{
		protected GetAndSetShapePropertiesStrategyBase(BMNCNShape shape)
		{
			Shape = shape;
		}

		protected BMNCNShape Shape;

		internal abstract string GetShapeName();
		internal abstract void SetShapeName(string newName);
	}
}
