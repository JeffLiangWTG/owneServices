namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	static class GetAndSetShapePropertiesStrategyProvider
	{
		static public GetAndSetShapePropertiesStrategyBase GetShapePropertiesStrategy(BMNCNShape shape)
		{
			if (shape.IsDefaultDiagram || shape.IsDefaultDiagramChild)
			{
				return new DefaultShapePropertiesStrategy(shape);
			}
			else
			{
				return new ShapePropertiesStrategy(shape);
			}
		}
	}
}
