namespace Enterprise.Customs.CA.Business.InterfaceImplementations.Testing
{
	sealed class JobDeclarationCustomsChargesHelper : JobDeclarationCustomsCharges
	{
		public JobDeclarationCustomsChargesHelper(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public Customs.Business.CusEntryHeader[] GetEntriesExposed
		{
			get { return GetEntries(); }
		}
	}
}
