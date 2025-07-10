using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.GDM
{
	public class GuidedDecisionMakingSingleInvoiceLineTarget : EU.Business.GuidedDecisionMakingSingleInvoiceLineTarget, IGuidedDecisionMakingTarget
	{
		public GuidedDecisionMakingSingleInvoiceLineTarget(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
			Argument.NotNull(invoiceLine, nameof(invoiceLine));
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		public override ZString CountryOfDestination
		{
			set
			{
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
				var parentDeclaration = invoiceLine.Declaration;
				if (parentDeclaration != null)
				{
					parentDeclaration.JE_RegionOrTerritoryOfDestination = value;
				}
			}
		}
	}
}
