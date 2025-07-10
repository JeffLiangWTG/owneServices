namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class DefaultShapePropertiesStrategy : GetAndSetShapePropertiesStrategyBase
	{
		internal DefaultShapePropertiesStrategy(BMNCNShape shape)
			: base(shape)
		{
		}

		internal override string GetShapeName()
		{
			return Shape.ProcessHeader?.FH_CompletionStatement.SubstringSafe(0, Shape.BNS_NameInfo.MaxLength);
		}

		internal override void SetShapeName(string newName)
		{
			Shape.ProcessHeader.FH_CompletionStatement = newName;
		}
	}
}
