namespace Enterprise.DocumentEngineIntegration.RollUpSort
{
	public interface IDocLine : ISortableDocLine
	{
		bool PreventGrouping { get; }
	}
}
