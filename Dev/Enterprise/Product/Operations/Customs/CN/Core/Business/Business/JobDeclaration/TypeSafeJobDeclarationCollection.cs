namespace Enterprise.Customs.CN.Business
{
	partial class JobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
	{
		public new JobDeclaration this[int index] => (JobDeclaration)Elements[index];

		public new JobDeclaration AddNew() => (JobDeclaration)base.AddNew();
	}
}
