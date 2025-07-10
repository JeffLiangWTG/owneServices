namespace Enterprise.Customs.GB.Business.Declaration
{
	public class LineMerger : EU.Business.Declaration.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			JobDeclaration declaration = (JobDeclaration)Declaration;
			var result = declaration.CreateEntryCreationStrategy();
			return new Customs.Business.EntryCreationStrategy[] { result };
		}
	}
}
