namespace Enterprise.Integration.Billing
{
	public interface IStlScript : IStlItem
	{
		string ScriptText { get; }
		int TimeoutSecs { get; }
		string Company { get; }
		string Branch { get; }
		string TransactionDateUtc { get; }
		string Reference1 { get; }
		string Reference2 { get; }
		string Reference3 { get; }
		string Reference4 { get; }
		string GuidReference { get; }
		string AdditionalRefs { get; }
		string User { get; }
		string ActiveOn { get; }
		string Preparation { get; }
		string From { get; }
		string Where { get; }
		string BillableCount { get; }
		bool WithRecompile { get; }
		string Name { get; }
		string MinCW1Version { get; }
		string MaxCW1Version { get; }
		int AdditionalRefsEncoding { get; }
	}
}
