using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class ComplianceWorkflowInitiationForm : ZChildForm
	{
		public ComplianceWorkflowInitiationForm(IComplianceItemRiskStatusProvider complianceRiskStatusProvider, ComplianceRiskStatus complianceRiskStatus) : base()
		{
			InitializeComponent();

			ComplianceRiskStatusProvider = complianceRiskStatusProvider;
			ComplianceRiskStatus = complianceRiskStatus;
			DialogResult = DialogResult.Cancel;

			ComplianceWorkflowRiskHeader.SetRiskInitiationInfo((ComplianceRiskTypes)(-1), ComplianceRiskStatus);
			ComplianceWorkflowRiskParties.SetRiskInitiationInfo(ComplianceRiskTypes.Parties, ComplianceRiskStatus);
			ComplianceWorkflowRiskLocations.SetRiskInitiationInfo(ComplianceRiskTypes.Locations, ComplianceRiskStatus);
			ComplianceWorkflowRiskCommodities.SetRiskInitiationInfo(ComplianceRiskTypes.Commodities, ComplianceRiskStatus);
			ComplianceWorkflowRiskAssessment.SetRiskInitiationInfo(ComplianceRiskTypes.Assessment, ComplianceRiskStatus);

			ComplianceWorkflowRiskAssessment.AllowOverlap(ComplianceWorkflowRiskLocations);
			ComplianceWorkflowRiskAssessment.AllowOverlap(ViewComplianceRiskLabel);

			UpdateSecurityUserControlsIfNeeded();
		}

		public override string FormVerb => string.Empty;

		IComplianceItemRiskStatusProvider ComplianceRiskStatusProvider { get; }
		ComplianceRiskStatus ComplianceRiskStatus { get; }

		void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.Cancel	&& allowAndDeclineAssessmentNotGranted)
			{
				ComplianceRiskStatus.AssessmentDecisionRequiredWorkflow();
			}
		}

		async void YesButton_Click(object sender, EventArgs e)
		{
			if (ComplianceRiskSecurityRights.IsAllowedAllowComplianceAssessmentWithShowError((IBusiness)ComplianceRiskStatusProvider, showErrorWhenNotAllowed: true))
			{
				DialogResult = DialogResult.Yes;
				Hide();
				var successInitialize = ComplianceRiskStatus.InitializeComplianceAssessmentWithErrorHandling();
				Close();

				if (ComplianceRiskStatus.PlugInParent?.CommodityRiskStatusChecker != null && successInitialize)
				{
					await ComplianceRiskStatus.PlugInParent.CommodityRiskStatusChecker.CheckAllCommoditiesRiskStatus(true);
				}
			}
		}

		void NoButton_Click(object sender, EventArgs e)
		{
			if (ComplianceRiskSecurityRights.AllowOrDeclineCommodityRiskAssessmentGranted((IBusiness)ComplianceRiskStatusProvider))
			{
				DialogResult = DialogResult.No;
				ComplianceRiskStatus.DeclineComplianceAssessmentWithErrorHandling();
				Close();
			}
		}

		void ViewComplianceRiskLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			DialogResult = DialogResult.Abort;
			Close();
		}

		void OKSecurityButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void UpdateSecurityUserControlsIfNeeded()
		{
			allowAndDeclineAssessmentNotGranted = ComplianceRiskSecurityRights.AllowAndDeclineCommodityRiskAssessmentNotGranted((IBusiness)ComplianceRiskStatusProvider, out var allowAssessment, out var declineAssessment);
			if (allowAndDeclineAssessmentNotGranted)
			{
				PromptLabel.Text = Res.GetString("3CCCA124-C47E-46DA-9FB2-789A9143666F", @"The commodities on this job may need to be checked for compliance risk before proceeding.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}
{1}", allowAssessment.DisplayTextPathToSecurityRight, declineAssessment.DisplayTextPathToSecurityRight);

				YesButton.Visible = false;
				NoButton.Visible = false;
				OKSecurityButton.Visible = true;
			}
		}

		bool allowAndDeclineAssessmentNotGranted;
	}
}
