using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUInvoicesGeneratorFromXSD : InvoicesGeneratorFromXSD
	{
		public AUInvoicesGeneratorFromXSD(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override InvoiceValueObjectDataAdapter GetInvoiceAdapter()
		{
			return new AUInvoiceValueObjectDataAdapter(declaration);
		}
	}
}
