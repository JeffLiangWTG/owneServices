using System;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class OrganisationWithMainAddressFact : IOrganisationWithMainAddressFact
	{
		public OrganisationWithMainAddressFact(OrgHeader org, IAddressFact addressFact)
		{
			Argument.NotNull(org, nameof(org));
			Argument.NotNull(addressFact, nameof(addressFact));

			PK = org.PK.ToGuid();
			Code = org.OH_Code.ToString();
			MainAddress = new FactJoin<IAddressFact>(addressFact);
			IsProxyOrgOfCurrentCompany = org.IsProxyOrg(GlbCompany.CurrentCompany);
			IsProxyOrgOfAnyCompany = IsProxyOrgOfCurrentCompany || org.IsProxyOrgOfAnyCompany();
		}

		public Guid PK { get; }

		public string Code { get; }

		public FactJoin<IAddressFact> MainAddress { get; }

		public bool IsProxyOrgOfAnyCompany { get; }

		public bool IsProxyOrgOfCurrentCompany { get; }
	}
}
