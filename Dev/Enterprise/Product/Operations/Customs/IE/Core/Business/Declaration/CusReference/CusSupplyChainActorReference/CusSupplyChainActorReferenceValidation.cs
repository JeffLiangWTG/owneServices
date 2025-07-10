using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusSupplyChainActorReferenceValidation : EU.Business.Declaration.CusSupplyChainActorReferenceValidation
	{
		public CusSupplyChainActorReferenceValidation(CusSupplyChainActorReference parent) : base(parent)
		{
		}

		protected override void CheckCFR_Reference()
		{
			base.CheckCFR_Reference();
			var parent = Parent;
			if (!parent.CFR_Reference.IsEmpty)
			{
				if (!EuEoriProviderAndValidator.ValidEORIorTCUIFormat(parent.CFR_Reference, parent.Factory))
				{
					parent.CFR_ReferenceInfo.AddMessageError(Res.GetString("6F57FF2D-21D2-4144-B20A-E2BB752FB9FF", "Additional Supply Chain Actor's Identification should have a valid EORI or TCUI format: A member state or third country code [a2] plus a unique identifier [an..15]"));
				}
			}
		}
	}
}
