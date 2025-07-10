namespace Enterprise.Customs.IL.Business
{
	public partial class CusContainer : Customs.Business.BaseCusContainer
	{
		public new CusContainer Clone() => (CusContainer)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusContainerLookups Lookups => (CusContainerLookups)base.Lookups;

		public new CusContainerValidation Validation => (CusContainerValidation)base.Validation;

		protected override Customs.Business.CusContainerLookups GetNewLookups() => new CusContainerLookups(this);

		protected override Customs.Business.CusContainerValidation GetNewValidation() => new CusContainerValidation(this);

		protected override System.Type GetJobDeclarationType() => typeof(JobDeclaration);
	}
}
