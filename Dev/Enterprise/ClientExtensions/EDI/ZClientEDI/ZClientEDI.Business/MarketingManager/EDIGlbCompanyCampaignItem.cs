using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Client.EDI.MarketingManager.Business
{
	#region Type Decider

	public class EDIGlbCompanyCampaignItemTypeDecider : GlbCompanyCampaignItemTypeDecider
	{
		public override Type GetTypeForNew()
		{
			return GetType(base.GetTypeForNew());
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetType(base.GetTypeForLoad(row, factory));
		}

		Type GetType(Type baseType)
		{
			return (baseType == typeof(GlbCompanyCampaignItem)) ? typeof(EDIGlbCompanyCampaignItem) : baseType;
		}
	}

	#endregion

	public class EDIGlbCompanyCampaignItem : GlbCompanyCampaignItem
	{
		public EDIGlbCompanyCampaignItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new EDIGlbCompanyCampaign CompanyCampaign
		{
			get
			{
				if ((RecipientType == CampaignContactTypeCodeList.Descriptions.GlbStaff.ToString())
				|| RecipientType == CampaignContactTypeCodeList.Descriptions.HRJobApplicant.ToString())
				{
					return null;
				}
				return (EDIGlbCompanyCampaign)base.CompanyCampaign;
			}
		}

		public override bool CanAutoStartVoteExamSurvey
		{
			get
			{
				return base.CanAutoStartVoteExamSurvey || (CompanyCampaign != null && CompanyCampaign.IsProductConsultantSurvey);
			}
		}

		public override void ResetTrackingInfo(bool isScheduled)
		{
			base.ResetTrackingInfo(isScheduled);

			if (CompanyCampaign != null && CompanyCampaign.IsWiseServicePartnerSurvey)
			{
				G8_ClosedDateUtc = ZDateTime.Empty;
				PersistedAnswers.RemoveAndDeleteAll();
			}
		}

		public override string GetMyAccountUserAgreementUrl(string agreementType, bool sendAgreementCopy) => UserAgreementUrlProvider.GetMyAccountUserAgreementUrlFromContact(this, RecipientAsOrgContact, "Campaign", agreementType, sendAgreementCopy);

		public override ZString ResendConfirmationMessage
		{
			get
			{
				if (CompanyCampaign != null && CompanyCampaign.IsWiseServicePartnerSurvey)
				{
					return "About to resend campaign to this contact and submitted survey result will be cleared. Would you like to continue?";
				}
				else
				{
					return base.ResendConfirmationMessage;
				}
			}
		}
	}
}

