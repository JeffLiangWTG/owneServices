using Enterprise.ZArchitecture.Business;

namespace Enterprise.TrustedMessaging.Business
{
	public class SystemUserAccountCollectionTerm : BaseTermsAgreement
	{
		public override string Type => "UAC";
		public override bool IsCurrentUserAllowedToAcknowledgeAgreement => true;
		public override string ErrorMessageForAcknowledgementNotAllowed => null;
		protected override bool IsLocalDisplayConditionSatisfied() => true;
	}
}
