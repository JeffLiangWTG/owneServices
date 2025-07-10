using System;
using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.Client.EDI.MarketingManager.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MarketingManager.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MarketingManager.Module
{
	public class WiseServicePartnerSurveyPluginController : SurveyCampaignPlugInController
	{
		protected override VoteExamSurveyPlugIn GetVoteExamSurveyPlugIn(GlbCompanyCampaign campaign)
		{
			return new WiseServicePartnerSurveyPlugin((EDIGlbCompanyCampaign)campaign);
		}

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.WiseServicePartnerSurveyPlugIn; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIGlbCompanyCampaign); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}
	}
}
