namespace Enterprise.Customs.IL.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);
	}
}
