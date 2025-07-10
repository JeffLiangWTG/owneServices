using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class InvoiceHeaderActiveCollection : EU.Business.Declaration.InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration) : base(declaration)
		{
		}

		public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
			: base(groupInvoice, isDirectRelationship)
		{
		}

		public new JobComInvoiceHeader AddNew() => (JobComInvoiceHeader)base.AddNew();

		public new JobComInvoiceHeader this[int index] => (JobComInvoiceHeader)base[index];

		protected override Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader newElement, BaseJobDeclaration declaration)
		{
			return new DefaultSetterForInvoiceHeader((JobComInvoiceHeader)newElement, (JobDeclaration)declaration);
		}
	}
}
