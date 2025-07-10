namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapePropertiesStrategy : GetAndSetShapePropertiesStrategyBase
	{
		public ShapePropertiesStrategy(BMNCNShape shape)
			: base(shape)
		{
		}

		internal override string GetShapeName()
		{
			return Shape.Name;
		}

		internal override void SetShapeName(string newName)
		{
			Shape.Name = newName;
		}
	}
}
