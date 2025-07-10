using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business
{
	public class GuidedDecisionMakingSingleInvoiceLineTarget : EU.Business.GuidedDecisionMakingSingleInvoiceLineTarget
	{
		public GuidedDecisionMakingSingleInvoiceLineTarget(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
			Argument.NotNull(invoiceLine, nameof(invoiceLine));
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		protected override void SetVATCodeCore(ZString vatCode, ZString vatAdditionalCode)
		{
			var exemptionCode = "EX";
			if (vatCode == exemptionCode)
			{
				invoiceLine.JI_ZZF_NKTaxType = exemptionCode;
			}
			else
			{
				base.SetVATCodeCore(vatCode, vatAdditionalCode);
			}
		}
	}
}
