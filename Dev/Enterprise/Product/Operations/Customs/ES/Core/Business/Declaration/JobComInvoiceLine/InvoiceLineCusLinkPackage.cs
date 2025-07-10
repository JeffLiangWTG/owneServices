using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceLineCusLinkPackage : BaseCusLinkPackage
	{
		public InvoiceLineCusLinkPackage(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZInt PackQty
		{
			get => base.PackQty;
			set
			{
				base.PackQty = value;
				Validation.ValidateAll();
			}
		}
	}
}
