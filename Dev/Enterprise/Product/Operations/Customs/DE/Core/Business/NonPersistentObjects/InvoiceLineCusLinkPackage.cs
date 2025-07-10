using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class InvoiceLineCusLinkPackage : BaseCusLinkPackage
	{
		public InvoiceLineCusLinkPackage(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		public override ZBool IsLinked
		{
			get => base.IsLinked;
			set
			{
				base.IsLinked = value;
				invoiceLine.AddInfoValidation.ValidateZG_IsMainPack();
			}
		}

		protected override bool GetIsPackQty_ReadOnlyCore()
		{
			var result = base.GetIsPackQty_ReadOnlyCore();
			if (invoiceLine.IsExport)
			{
				result = invoiceLine.JI_IsMainPack || result;
			}
			return result;
		}
	}
}
