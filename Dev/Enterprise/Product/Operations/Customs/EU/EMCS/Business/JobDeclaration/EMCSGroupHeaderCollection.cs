using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class EMCSGroupHeaderCollection : GroupHeaderCollection
	{
		public EMCSGroupHeaderCollection(EMCSJobDeclaration declaration)
			: base(declaration)
		{
		}

		public EMCSGroupHeaderCollection(EMCSJobComInvoiceHeader invoice)
			: base(invoice)
		{
		}

		public new EMCSJobComInvoiceGroupHeader this[int index]
		{
			get { return (EMCSJobComInvoiceGroupHeader)(Elements[index]); }
		}

		public new EMCSJobComInvoiceGroupHeader AddNew()
		{
			return (EMCSJobComInvoiceGroupHeader)base.AddNew();
		}
	}
}
