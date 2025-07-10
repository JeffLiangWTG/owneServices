using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class OutboundCommunicationPartyConfigUserControl : ZUserControl
	{
		public enum AccessRequirement
		{
			SupportsOutbound,
			SupportsOutboundOAuth,
			SupportsOutboundBasicAuth,
			SupportsOutboundNoAuth
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
		public OutboundCommunicationPartyConfigUserControl()
		{
			InitializeComponent();
			BindingSource.SetBindingMember(outboundOAuthUserControl1, ".");
			BindingSource.SetBindingMember(outboundBasicAuthenticationUserControl1, ".");
			BindingSource.SetBindingMember(outboundNoAuthenticationUserControl1, ".");
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
			{
				outboundDisableWarning.Visible = !((DataSource as EDICommunicationParty)?.OutboundConfig?.IsActive ?? false);
				outboundActive.Enabled = (DataSource as EDICommunicationParty)?.ECP_IsActive ?? false;
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
				else if (Config.Auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.NoAuthentication)
				{
					ShowNoAuthControl();
				}
			}
		}

		protected void OutboundActive_OnCheckedChanged(object sender, EventArgs e)
		{
			var enableClient = (bool?)((EDICommunicationParty)DataSource)?.ECP_IsActive;
			var isOutboundActive = (AccessTypes == null || AccessTypes[AccessRequirement.SupportsOutbound]) && outboundActive.Checked;
			outboundDisableWarning.Visible = !isOutboundActive;
			SetControlEnabled(enableClient == true && isOutboundActive);
		}

		public void SetControlEnabled(bool isParentActive, bool applyOnActiveCheckbox = true)
		{
			if (applyOnActiveCheckbox)
			{
				outboundActive.Enabled = isParentActive;
			}

			SetControlEnabled(isParentActive);
		}

		void SetControlEnabled(bool isParentActive)
		{
			var isEnabled = isParentActive && outboundActive.Checked;
			Controls.Cast<Control>().Where(ctl => ctl.Parent == this && ctl != outboundDisableWarning && ctl != outboundActive)
				.ToList()
				.ForEach(ctl => ctl.Enabled = isEnabled);
		}

		void ShowOAuthControl()
		{
			outboundOAuthUserControl1.Show();
			outboundBasicAuthenticationUserControl1.Hide();
			outboundNoAuthenticationUserControl1.Hide();
		}
		void ShowBasicAuthControl()
		{
			outboundOAuthUserControl1.Hide();
			outboundBasicAuthenticationUserControl1.Show();
			outboundNoAuthenticationUserControl1.Hide();
		}

		void ShowNoAuthControl()
		{
			outboundBasicAuthenticationUserControl1.Hide();
			outboundOAuthUserControl1.Hide();
			outboundNoAuthenticationUserControl1.Show();
		}

		public EDICommunicationPartyConfig Config => ((EDICommunicationParty)DataSource)?.OutboundConfig;
	}
}
