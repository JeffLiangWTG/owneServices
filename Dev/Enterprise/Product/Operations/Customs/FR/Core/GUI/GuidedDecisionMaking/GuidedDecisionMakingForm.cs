using System;
using Enterprise.Customs.FR.Business.GDM;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.GDM
{
	public partial class GuidedDecisionMakingForm : EU.GUI.GuidedDecisionMakingForm
	{
		public GuidedDecisionMakingForm(GuidedDecisionMakingBasic guidedDecisionMakingBasic, IGuidedDecisionMakingTarget guidedDecisionMakingTarget) : base(guidedDecisionMakingBasic, guidedDecisionMakingTarget)
		{
		}

		protected override Type ExpectedDataSourceType => typeof(GuidedDecisionMakingBasic);

		protected override IPanelLayoutProvider gDMBasicLayout => new GDMBasicLayout();

		protected override EU.GUI.SummaryControl GetSummaryControl() => new SummaryControl();
	}
}
