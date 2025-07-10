using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using AuthorizationCodes = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusAuthorizationUsagePhase5Validation : EU.Business.CusAuthorizationUsageValidation
	{
		public CusAuthorizationUsagePhase5Validation(CusAuthorizationUsage parent) : base(parent)
		{
		}

		public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

		public ICusAuthorizationUsagePhase5ValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<ICusAuthorizationUsagePhase5ValidationDecider> validationDeciderCached;

		ICusAuthorizationUsagePhase5ValidationDecider GetValidationDecider() => Parent.Header?.Configuration.CusAuthorizationUsageConfiguration.GetValidationDecider(Parent.Header);

		protected override void CheckAGC_Code()
		{
			base.CheckAGC_Code();
			if (Parent is CusAuthorizationUsage authUsage && authUsage.Header is NctsHeader header
				&& header.MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				CheckAGC_Code_G0114Rule(authUsage, header, movementHeader);
				CheckAGC_Code_NR0063Rule(authUsage, header, movementHeader);
			}
		}

		protected override void CheckAGC_NumberMandatory(ZPropertyInfo agcNumberInfo)
		{
			var prefix = string.Empty;
			if (Parent.Header.IsDepartureMovement && ValidationDecider is { IsRuleTR0005Active: true })
			{
				prefix = NctsConstants.ValidationRuleMessagePrefixes.TR0005;
			}
			MandatoryValidation.CheckEntered(agcNumberInfo, errorNotificationPrefix: prefix);
		}

		void CheckAGC_Code_G0114Rule(CusAuthorizationUsage authUsage, NctsHeader header, NctsDepartureMovementHeader movementHeader)
		{
			if (ValidationDecider is { IsRuleG0114Active: true }
				&& authUsage.AGC_Code == AuthorizationCodes.AuthorizedConsignorTransit
				&& !movementHeader.IsSimplifiedNctsProcedure)
			{
				var error = Res.GetString("80FF3E08-E7C3-4090-9284-4CD80CC9DD6D",
					"[G0114] When Authorization ACR is provided, Simplified Procedure must be selected.");
				authUsage.AGC_CodeInfo.AddMessageError(error);
			}
		}

		void CheckAGC_Code_NR0063Rule(CusAuthorizationUsage authUsage, NctsHeader header, NctsDepartureMovementHeader movementHeader)
		{
			var goodsLocationValidationDecider = (authUsage.Parent as ICusGoodsLocationProviderWithValidationDecider)?.GoodsLocationValidationDecider;

			if (goodsLocationValidationDecider is IDeparturePhase5CusGoodsLocationValidationDecider { IsRuleNR0063Active: true }
				&& authUsage.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit
				&& movementHeader.GoodsLocation.CGL_Type == CusGoodsLocationTypeList.Codes.DesignatedLocation)
			{
				var configuration = header.Configuration.ValidationRuleConfiguration;
				authUsage.AGC_CodeInfo.AddMessageError(configuration?.Messages.NR0063Message);
			}
		}
	}
}
