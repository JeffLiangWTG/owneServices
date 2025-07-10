using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MarketingManager.Business
{
	#region TypeDecider

	public class EDIGlbCompanyCampaignTypeDecider : GlbCompanyCampaignTypeDecider
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
			return (baseType == typeof(GlbCompanyCampaign)) ? typeof(EDIGlbCompanyCampaign) : baseType;
		}
	}

	#endregion

	public class EDIGlbCompanyCampaign : GlbCompanyCampaign
	{
		public EDIGlbCompanyCampaign(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ModuleIdentifier DripMarketingFilterRuleModule
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRuleEDI;
			}
		}

		protected override List<string> GetEmailCampaignsList()
		{
			var ediEmailCampaigns = new List<string> { EDICampaignTypeList.Codes.ProductConsultantSurvey, EDICampaignTypeList.Codes.WiseServicePartnerSurvey };
			ediEmailCampaigns.AddRange(base.GetEmailCampaignsList());

			return ediEmailCampaigns;
		}

		#region Flags

		public bool IsProductConsultantSurvey
		{
			get { return G0_BroadcastVoteSurveyExam == EDICampaignTypeList.Codes.ProductConsultantSurvey; }
		}

		protected override bool IsSurveyCampaignType(string campaignType)
		{
			return base.IsSurveyCampaignType(campaignType) || campaignType == EDICampaignTypeList.Codes.ProductConsultantSurvey || campaignType == EDICampaignTypeList.Codes.WiseServicePartnerSurvey;
		}

		public bool IsWiseServicePartnerSurvey
		{
			get { return G0_BroadcastVoteSurveyExam == EDICampaignTypeList.Codes.WiseServicePartnerSurvey; }
		}

		#endregion

		#region Lookups

		protected override GlbCompanyCampaignLookups GetNewLookups()
		{
			return new EDIGlbCompanyCampaignLookups(this);
		}

		#endregion

		#region CampaignsItemsSent

		public new EDIGlbCompanyCampaignItemCollection CampaignsItemsSent
		{
			get { return (EDIGlbCompanyCampaignItemCollection)base.CampaignsItemsSent; }
		}

		protected override GlbCompanyCampaignItemCampaignDependentCollection GetNewCampaignItemCollection()
		{
			return new EDIGlbCompanyCampaignItemCollection(this);
		}

		#endregion
	}
}

