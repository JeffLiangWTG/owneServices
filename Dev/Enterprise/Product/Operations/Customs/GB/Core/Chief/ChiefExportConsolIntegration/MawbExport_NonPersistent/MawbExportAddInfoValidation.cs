//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMawbExportAddInfoValidation
//
//    This class should be used for overriding validation in AutoMawbExportAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class MawbExportAddInfoValidation : AutoMawbExportAddInfoValidation
	{
		public MawbExportAddInfoValidation(AutoMawbExportAddInfo parent) : base(parent)
		{
		}

		protected override void CheckME_Profile()
		{
			const string Phase2ProfileBasedOn_GlbExternalPassword_CDSGatewayErrorMessage = "CDS Export Inventory Linking is in Phase Two, meaning all messages must go via a CSP. Select another profile such that a CSP is used to deliver all messages to CDS.";
			const string Phase1ProfileBasedOn_CDSGatewayInfoMessage = "CDS Export Inventory Dual Running is in Phase One. Do not send inventory-linking messages to CDS.  Instead select a CHIEF profile/badge.";
			const string Phase2ProfileBasedOn_CHIEFGatewayWarningMessage = "CDS Export Inventory Dual Running is in Phase Two. It is encouraged to use CDS and not CHIEF. Select a CDS profile/badge";

			base.CheckME_Profile();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ME_ProfileInfo);

			var badges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(System.Guid.Empty, MasterFiles.Business.GlbBranch.CurrentBranch.PK.ToGuid(), System.Guid.Empty);
			var profileHasRegRecordWithCDSGateway = (from BadgeCodeSetting badge in badges where badge.BadgeCode == Parent.ME_Profile && badge.CSPCode == GatewayList.Codes.CDS select badge).Any();
			if (GBExtensions.IsPhase1Active && profileHasRegRecordWithCDSGateway)
			{
				Parent.ME_ProfileInfo.AddMessageError(Phase1ProfileBasedOn_CDSGatewayInfoMessage);
			}
			else if (GBExtensions.IsPhase2Active)
			{
				var gbGlbExternalPasswordCollection = new GlbExternalPasswordCollection_GB(GlbCompany.CurrentCompany);
				gbGlbExternalPasswordCollection.Load();
				var profileIsBasedOnGlbExternalPassword = gbGlbExternalPasswordCollection.OfType<GlbExternalPassword_GB>()
					.Select(x => System.FormattableString.Invariant($"{x.EORI}.{x.Badge}")).Any(y => y == Parent.ME_Profile);
				if (profileHasRegRecordWithCDSGateway || profileIsBasedOnGlbExternalPassword)
				{
					Parent.ME_ProfileInfo.AddMessageError(Phase2ProfileBasedOn_GlbExternalPassword_CDSGatewayErrorMessage);
				}

				var profileHasRegRecordWithCHIEFApplicationCode = (from BadgeCodeSetting badge in badges where badge.BadgeCode == Parent.ME_Profile && badge.ApplicationCodeIsChief select badge).Any();
				if (profileHasRegRecordWithCHIEFApplicationCode)
				{
					Parent.ME_ProfileInfo.AddWarning(Phase2ProfileBasedOn_CHIEFGatewayWarningMessage);
				}
			}
		}
	}
}
