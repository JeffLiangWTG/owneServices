using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.GDM;

public class GuidedDecisionMakingMultiInvoiceLinesTarget : EU.Business.GuidedDecisionMakingMultiInvoiceLinesTarget, IGuidedDecisionMakingTarget
{
	public GuidedDecisionMakingMultiInvoiceLinesTarget(List<EU.Business.Declaration.JobComInvoiceLine> invoiceLines) : base(invoiceLines)
	{
		Argument.NotNull(invoiceLines, nameof(invoiceLines));
		this.invoiceLines = invoiceLines;
	}

	readonly List<EU.Business.Declaration.JobComInvoiceLine> invoiceLines;

	public override ZString CountryOfDestination
	{
		set
		{
			var invoiceLine = invoiceLines.ElementAtOrDefault(0) as JobComInvoiceLine;
			if (!(invoiceLine.Declaration?.Validation.ValidationDecider?.IsRuleC0002Active ?? false)
				|| (invoiceLine.Declaration?.JE_GoodsDestination.IsEmpty ?? true))
			{
				base.CountryOfDestination = value;
			}
		}
	}

	public ZString RegionOrTerritoryOfDestination
	{
		set
		{
			var invoiceLine = invoiceLines.ElementAtOrDefault(0) as JobComInvoiceLine;
			var parentDeclaration = invoiceLine?.Declaration;
			if (parentDeclaration != null)
			{
				parentDeclaration.JE_RegionOrTerritoryOfDestination = value;
			}
		}
	}
}
