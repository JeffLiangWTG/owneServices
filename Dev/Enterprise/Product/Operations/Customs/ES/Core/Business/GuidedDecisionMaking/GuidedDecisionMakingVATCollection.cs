using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.Business
{
	public class GuidedDecisionMakingVATCollection : EU.Business.GuidedDecisionMakingVATCollection
	{
		public GuidedDecisionMakingVATCollection(GuidedDecisionMakingBasic gDMBasic) : base(gDMBasic)
		{
		}

		protected override IEnumerable<VATApplicabilityView> GetVATApplicabilitiesCore()
		{
			var vatCodePrefix = gDMBasic.DestinationStateIsCanaryIsland ? UniversalReferenceConstants.RefCusTaxOrFee.CanaryIslandVATPrefix : UniversalReferenceConstants.RefCusTaxOrFee.VatPrefix;
			return base.GetVATApplicabilitiesCore().Where(x => x.ZX5_ZZF_NKTaxOrFeeCode.StartsWith(vatCodePrefix));
		}

		protected override void AddExtraVATs(List<GuidedDecisionMakingVAT> gdmVATs)
		{
			var gdmVAT = new GuidedDecisionMakingVAT(gDMBasic);
			gdmVAT.VATCode = "EX";
			gdmVAT.Description = Res.GetString("FDAC39E3-6DD2-41B6-871A-980B48FC594A", "Exemption");
			gdmVAT.IsTicked = gdmVAT.VATCode == gDMBasic.CapturedVATCode;
			gdmVATs.Add(gdmVAT);
		}

		protected new GuidedDecisionMakingBasic gDMBasic => base.gDMBasic as GuidedDecisionMakingBasic;
	}
}
