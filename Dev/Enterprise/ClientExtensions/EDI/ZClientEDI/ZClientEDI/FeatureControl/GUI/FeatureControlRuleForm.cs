using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	public partial class FeatureControlRuleForm : ZTemplateForm, ICanAttachWithoutSecurity
	{
		public FeatureControlRuleForm(FeatureControlRule featureControlRule) : base(featureControlRule)
		{
			featureControlRule.BeforeParametersOverwrite += BeforeParametersOverwrite;
			featureControlRule.FCR_RuleTypeInfo.ValueChanged += (_, __) => OnRuleTypeChanged();
			OnRuleTypeChanged();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (BusinessEntity is FeatureControlRule rule)
			{
				rule.FCR_IsActiveInfo.ValueChanged += OnRuleActiveValueChanged;
				rule.IsGlobalRuleInfo.ValueChanged += OnRuleIsGlobalValueChanged;

				if (!EDISecurityCheckpoints.FeatureAdmin.IsAllowed)
				{
					GlobalCheckBox.ReadOnly = true;
					FeatureSetCheckBox.ReadOnly = true;
					FeatureSetDropEdit.ReadOnly = true;
				}
			}
		}

		protected override bool AllowNew => false;
		protected override bool AllowActionDataMenuItem => false;
		protected override bool ShowAuditTab => true;
		protected override bool ShowNotesTab => false;
		protected override bool SupportsEDocs => false;

		void PopupButton_Click(object sender, EventArgs e)
		{
			ParameterTextBox.Focus();
			var form = new ZTextBoxPopupForm(ParameterTextBox);
			if (ParameterTextBox.ReadOnly)
			{
				form.SetReadOnlyIncludingChildren();
			}
			ZFormModaliser.Show(form, FindForm());
		}

		void BeforeParametersOverwrite(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel =
				Globals.Message.Show(Res.GetString("86867a30-fced-4273-a895-4887b78b0c84", "This will clear the current saved parameters and replace them with the global parameters. Continue?"), Res.GetString("3ad94c47-1be1-46b6-88f8-b5fb44034b14", "Confirmation"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) != DialogResult.Yes;
		}

		void OnRuleTypeChanged()
		{
			if (BusinessEntity is FeatureControlRule rule && !rule.ReadOnly)
			{
				DatabaseModuleButtonGrid.SetButtonsReadOnly(rule.IsGlobalRule || rule.IsFeatureSetRule);
				DatabaseModuleButtonGrid.Refresh();
				FeatureSetDropEdit.Visible = rule.IsFeatureSetRule;
			}
		}

		void OnRuleActiveValueChanged(object sender, EventArgs e)
		{
			if (BusinessEntity is FeatureControlRule rule && rule.IsGlobalRule && rule.FCR_IsActive
				&& !EDISecurityCheckpoints.FeatureAdmin.IsAllowed)
			{
				EDISecurityCheckpoints.FeatureAdmin.ShowError();
				rule.FCR_IsActiveInfo.ValueChanged -= OnRuleActiveValueChanged;
				rule.FCR_IsActive = false;
				rule.FCR_IsActiveInfo.ValueChanged += OnRuleActiveValueChanged;
			}
		}

		void OnRuleIsGlobalValueChanged(object sender, EventArgs e)
		{
			if (BusinessEntity is FeatureControlRule rule && rule.IsGlobalRule && rule.FCR_IsActive
				&& !EDISecurityCheckpoints.FeatureAdmin.IsAllowed)
			{
				EDISecurityCheckpoints.FeatureAdmin.ShowError();
				rule.IsGlobalRuleInfo.ValueChanged -= OnRuleIsGlobalValueChanged;
				rule.IsGlobalRule = false;
				rule.IsGlobalRuleInfo.ValueChanged += OnRuleIsGlobalValueChanged;
			}
		}
	}
}
