using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business;

internal class GuidedDecisionMakingMultiInvoiceLinesTarget : EU.Business.GuidedDecisionMakingMultiInvoiceLinesTarget
{
	public GuidedDecisionMakingMultiInvoiceLinesTarget(List<EU.Business.Declaration.JobComInvoiceLine> invoiceLines) : base(invoiceLines)
	{
		Argument.NotNull(invoiceLines, nameof(invoiceLines));
		this.invoiceLines = invoiceLines;
	}

	readonly List<EU.Business.Declaration.JobComInvoiceLine> invoiceLines;

	protected override void SetVATCodeCore(ZString vatCode, ZString vatAdditionalCode)
	{
		var exemptionCode = "EX";
		invoiceLines.ForEach(invoiceLine =>
		{
			if (vatCode == exemptionCode)
			{
				invoiceLine.JI_ZZF_NKTaxType = exemptionCode;
			}
			else
			{
				base.SetVATCodeCore(vatCode, vatAdditionalCode);
			}
		});
	}
}
