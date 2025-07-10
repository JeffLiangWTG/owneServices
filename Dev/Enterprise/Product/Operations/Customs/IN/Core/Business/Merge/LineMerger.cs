namespace Enterprise.Customs.IN.Business;

public class LineMerger : Customs.Business.LineMerger
{
	public LineMerger(JobDeclaration declaration)
		: base(declaration)
	{
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
	{
		return new EntryCreationStrategy[] { new EntryCreationStrategy(Declaration) };
	}
}
