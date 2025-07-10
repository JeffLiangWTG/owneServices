namespace Enterprise.Customs.MY.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject, Integration.Customs.MY.IJobDeclarationFilterBusinessObject
	{
		public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);
	}
}
