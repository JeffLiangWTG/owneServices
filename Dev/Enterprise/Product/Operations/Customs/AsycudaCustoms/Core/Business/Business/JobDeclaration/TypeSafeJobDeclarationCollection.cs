namespace Enterprise.Customs.AsycudaCustoms.Business
{
	partial class JobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
	{
		public new JobDeclaration this[int index]
		{
			get { return (JobDeclaration)Elements[index]; }
		}

		public new JobDeclaration AddNew()
		{
			return (JobDeclaration)base.AddNew();
		}
	}
}
