using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSAddInfoJobComInvoiceLineValueSetStrategy : AddInfoJobComInvoiceLineValueSetStrategy
	{
		public CDSAddInfoJobComInvoiceLineValueSetStrategy(EU.Business.Declaration.AddInfoJobComInvoiceLine addInfoJobComInvoiceLine) : base(addInfoJobComInvoiceLine)
		{
			this.addInfoJobComInvoiceLine = addInfoJobComInvoiceLine;
		}

		readonly EU.Business.Declaration.AddInfoJobComInvoiceLine addInfoJobComInvoiceLine;

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);

			switch (valueThatHasChanged.Name)
			{
				case EU.Business.AutoEUAddInfo.Schema.ZG_MethodOfPayment:
					CreateSupportingDocsIfRequired();
					break;
			}
		}

		void CreateSupportingDocsIfRequired()
		{
			var invoiceLine = (JobComInvoiceLine)addInfoJobComInvoiceLine.Parent;
			var declaration = invoiceLine.Declaration;

			declaration.UCCHelper?.DoIfPaymentMethodAndDefermentAccountNumberBothAreSet(declaration, invoiceLine);
		}
	}
}
