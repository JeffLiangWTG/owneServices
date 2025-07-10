using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class InboundCommunicationPartyConfigUserControl : ZUserControl
	{
		public enum AccessRequirement
		{
			SupportsInbound,
			RequiresBranch,
			RequiresDepartment,
			SupportsInboundOAuth,
			SupportsInboundBasicAuth,
		}
		Dictionary<AccessRequirement, bool> accessTypes;
		public Dictionary<AccessRequirement, bool> AccessTypes
		{
			get
			{
				return accessTypes;
			}
			set
			{
				accessTypes = value;
			}
		}
		public InboundCommunicationPartyConfigUserControl()
		{
			InitializeComponent();
			BindingSource.SetBindingMember(inboundOAuthUserControl1, ".");
			BindingSource.SetBindingMember(inboundBasicAuthenticationUserControl1, ".");
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
			{
				inboundDisableWarning.Visible = !((DataSource as EDICommunicationParty)?.InboundConfig?.IsActive ?? false);
				inboundActive.Enabled = (DataSource as EDICommunicationParty)?.ECP_IsActive ?? false;
				ShowAuthControl();
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is EDICommunicationParty party)
			{
				Config.Auth.ECA_AuthorizationModeInfo.ValueChanged += OnAuthorizationModeChanged;
			}
		}

		protected void OnAuthorizationModeChanged(object sender, EventArgs e)
		{
			ShowAuthControl();
		}

		void ShowAuthControl()
		{
			if (Config != null)
			{
				if (Config.Auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.OAuthAuthentication)
				{
					ShowOAuthControl();
				}
				else if (Config.Auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.BasicAuthentication)
				{
					ShowBasicAuthControl();
				}
			}
		}

		protected void InboundActive_OnCheckedChanged(object sender, EventArgs e)
		{
			var enableClient = (bool?)((EDICommunicationParty)DataSource)?.ECP_IsActive;
			var isInboundActive = (AccessTypes == null || AccessTypes[AccessRequirement.SupportsInbound]) && inboundActive.Checked;
			inboundDisableWarning.Visible = !isInboundActive;
			SetControlEnabled(enableClient == true && isInboundActive);
		}

		public void SetControlEnabled(bool isParentActive, bool applyOnActiveCheckbox = true)
		{
			if (applyOnActiveCheckbox)
			{
				inboundActive.Enabled = isParentActive;
			}

			SetControlEnabled(isParentActive);
		}

		void SetControlEnabled(bool isParentActive)
		{
			var isEnabled = isParentActive && inboundActive.Checked;
			Controls.Cast<Control>().Where(ctl => ctl.Parent == this && ctl != inboundDisableWarning && ctl != inboundActive)
				.ToList()
				.ForEach(ctl => ctl.Enabled = isEnabled);
			if (isParentActive && AccessTypes != null)
			{
				ShowBranchControl(AccessTypes[AccessRequirement.RequiresBranch]);
				ShowDepartmentControl(AccessTypes[AccessRequirement.RequiresDepartment]);
			}
		}

		void ShowOAuthControl()
		{
			inboundOAuthUserControl1.Show();
			inboundBasicAuthenticationUserControl1.Hide();
		}
		void ShowBasicAuthControl()
		{
			inboundOAuthUserControl1.Hide();
			inboundBasicAuthenticationUserControl1.Show();
		}

		void ShowBranchControl(bool enabled)
		{
			branchGuidFindBox.Enabled = enabled;
		}

		void ShowDepartmentControl(bool enabled)
		{
			departmentGuidFindBox.Enabled = enabled;
		}

		public EDICommunicationPartyConfig Config => ((EDICommunicationParty)DataSource)?.InboundConfig;
	}
}
