namespace Enterprise.Customs.FR.Business.Declaration
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

		public new JobComInvoiceHeader AddNew()
		{
			return (JobComInvoiceHeader)base.AddNew();
		}

		public new JobComInvoiceHeader this[int index] => (JobComInvoiceHeader)(base[index]);

		protected override Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader newElement, Customs.Business.BaseJobDeclaration declaration)
		{
			return new DefaultSetterForInvoiceHeader((JobComInvoiceHeader)newElement, (JobDeclaration)declaration);
		}

		protected override void OnAdded(Customs.Business.BaseJobComInvoiceHeader businessObject)
		{
			base.OnAdded(businessObject);

			if (businessObject is JobComInvoiceHeader jobComInvoice && !IsNonCommittedElement(jobComInvoice))
			{
				jobComInvoice.GroupHeader?.UpdateCharges();
			}
		}
	}
}
