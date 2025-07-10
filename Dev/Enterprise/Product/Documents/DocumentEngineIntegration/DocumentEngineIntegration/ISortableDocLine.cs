namespace Enterprise.DocumentEngineIntegration.RollUpSort
{
	public interface ISortableDocLine
	{
		int OrgLevelSortOrder { get; }
		int ChargePrintSeqSortOrder { get; }
		int UserEnteredSortOrder { get; }
		string AlphabeticalSortOrder { get; }
	}
}
