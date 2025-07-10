using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MostInterestingLegProvider : Customs.Business.MostInterestingLegProvider
	{
		public MostInterestingLegProvider(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
		}

		protected override IMovementLeg GetOutboundLegCore(IEnumerable<IMovementLeg> transports)
		{
			if (declaration.IsImport)
			{
				var orderedLegs = transports.ToArray();
				MovementLegComparer.SortMovementLegsByPorts(orderedLegs);

				if (orderedLegs.Any())
				{
					return orderedLegs.First();
				}
			}
			return base.GetInboundLegCore(transports);
		}
	}
}
