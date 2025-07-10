using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSInvoiceLineViewCollection : Customs.Business.InvoiceLineViewCollection<EMCSJobComInvoiceLine>
	{
		public EMCSInvoiceLineViewCollection(EMCSJobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}

		readonly EMCSJobDeclaration declaration;

		protected override BusinessObject AddNewCore()
		{
			return AddNew(typeof(EMCSJobComInvoiceLine));
		}

		protected override bool AllowNewCore => base.AllowNewCore && !declaration.IsMessageStatusSentOrAcknowledged;
	}
}
