namespace Enterprise.Customs.DK.Business.Declaration
{
	public class InvoiceHeaderActiveCollection : EU.Business.Declaration.InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration) : base(declaration)
		{
		}

		public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship) : base(groupInvoice, isDirectRelationship)
		{
		}

		public new JobComInvoiceHeader AddNew() => (JobComInvoiceHeader)base.AddNew();

		public new JobComInvoiceHeader this[int index] => (JobComInvoiceHeader)base[index];
	}
}
