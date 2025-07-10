namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSInvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection
	{
		public EMCSInvoiceHeaderActiveCollection(EMCSJobDeclaration declaration)
			: base(declaration)
		{
		}

		public EMCSInvoiceHeaderActiveCollection(EMCSJobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
			: base(groupInvoice, isDirectRelationship)
		{
		}

		public new EMCSJobComInvoiceHeader AddNew()
		{
			return (EMCSJobComInvoiceHeader)base.AddNew();
		}

		public new EMCSJobComInvoiceHeader this[int index]
		{
			get { return (EMCSJobComInvoiceHeader)(base[index]); }
		}
	}
}
