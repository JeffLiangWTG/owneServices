using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.GDM
{
	public class GuidedDecisionMakingVATCollection : EU.Business.GuidedDecisionMakingVATCollection
	{
		public GuidedDecisionMakingVATCollection(GuidedDecisionMakingBasic gDMBasic) : base(gDMBasic)
		{
		}

		protected override IEnumerable<VATApplicabilityView> GetVATApplicabilitiesCore()
		{
			return UniversalReferenceDataHelper.GetVATApplicabilitiesWithSecondaryTradeGroup(Factory, gDMBasic.Tariff, gDMBasic.DataGrouping, gDMBasic.GetSecondaryTradeGroup().ToArray(), gDMBasic.EffectiveDate);
		}

		protected new GuidedDecisionMakingBasic gDMBasic => base.gDMBasic as GuidedDecisionMakingBasic;
	}
}
