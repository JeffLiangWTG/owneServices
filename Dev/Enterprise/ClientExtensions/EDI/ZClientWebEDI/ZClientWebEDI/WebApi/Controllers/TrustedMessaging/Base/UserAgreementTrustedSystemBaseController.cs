using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class UserAgreementTrustedSystemBaseController : UserAgreementBaseController<UserAgreementInfo, GetAcceptancesInfo, TrustedInfo>
	{
		public UserAgreementTrustedSystemBaseController() : base()
		{
		}

		public UserAgreementTrustedSystemBaseController(NLogWrapper logger) : base(logger)
		{
		}

		protected override UserAgreementRequestDataValidation GetUserAgreementRequestDataValidation(UserAgreementInfo requestInfo)
		{
			return new UserAgreementRequestDataTrustedSystemValidation(requestInfo);
		}

		protected override UserAgreementSubmissionRequestDataValidation GetUserAgreementSubmissionRequestDataValidation(UserAgreementInfo requestInfo)
		{
			return new UserAgreementSubmissionRequestDataTrustedSystemValidation(requestInfo);
		}

		protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, TrustedInfo info)
		{
			return TrustedSystemHelper.GetLicenceDatabase((ITrustedSystemContext)context, info);
		}

		protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, IEnterpriseAgreementInfo info)
		{
			return TrustedSystemHelper.GetLicenceDatabase((ITrustedSystemContext)context, (TrustedInfo)info);
		}
	}
}
