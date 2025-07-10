using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class EDICommunicationPartyUserControl : ZUserControl
	{
		public EDICommunicationPartyUserControl()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				SuspendLayout();

				// Inbound tab setup
				var inbound = new InboundCommunicationPartyConfigUserControl();
				inbound.Dock = DockStyle.Fill;
				var inboundPage = new ZTabPage
				{
					Text = Res.GetString("EDICommunicationPartyUserControl|InboundTab", "Inbound")
				};
				inboundPage.Controls.Add(inbound);
				tabControl.TabPages.Add(inboundPage);
				BindingSource.SetBindingMember(inbound, ".");
				inboundControl = inbound;
				// Outbound tab setup
				var outbound = new OutboundCommunicationPartyConfigUserControl();
				outbound.Dock = DockStyle.Fill;
				var outboundPage = new ZTabPage
				{
					Text = Res.GetString("EDICommunicationPartyUserControl|OutboundTab", "Outbound")
				};
				outboundPage.Controls.Add(outbound);
				tabControl.TabPages.Add(outboundPage);
				BindingSource.SetBindingMember(outbound, ".");
				outboundControl = outbound;

				SetDataBinding(CurrentDataItem, "");

				ResumeLayout();
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataSource.ECP_ApplicationCodeInfo.ValueChanged -= SetControlVisibility;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				DataSource.ECP_ApplicationCodeInfo.ValueChanged += SetControlVisibility;

				SetControlVisibility();
			}
		}

		void SetControlVisibility(object sender, EventArgs e)
		{
			SetControlVisibility();
		}

		void SetControlVisibility()
		{
			var template = DataSource;
			var isICodeDescriptionPairList = template.Lookups.ApplicationCodeList is ICodeDescriptionPairList;
			var isInboundActive = activeClient.Checked && template.SupportsEntity(AccessRequirement.SupportsInbound) && isICodeDescriptionPairList;
			inboundControl.AccessTypes = new Dictionary<InboundCommunicationPartyConfigUserControl.AccessRequirement, bool>()
			{
				{ InboundCommunicationPartyConfigUserControl.AccessRequirement.SupportsInbound, template.SupportsEntity(AccessRequirement.SupportsInbound) && isICodeDescriptionPairList },
				{ InboundCommunicationPartyConfigUserControl.AccessRequirement.RequiresBranch, template.SupportsEntity(AccessRequirement.RequiresBranch) && isICodeDescriptionPairList },
				{ InboundCommunicationPartyConfigUserControl.AccessRequirement.RequiresDepartment, template.SupportsEntity(AccessRequirement.RequiresDepartment) && isICodeDescriptionPairList },
				{ InboundCommunicationPartyConfigUserControl.AccessRequirement.SupportsInboundBasicAuth, template.SupportsEntity(AccessRequirement.SupportsInboundBasicAuth) && isICodeDescriptionPairList },
				{ InboundCommunicationPartyConfigUserControl.AccessRequirement.SupportsInboundOAuth, template.SupportsEntity(AccessRequirement.SupportsInboundOAuth) && isICodeDescriptionPairList }
			};
			inboundControl?.SetControlEnabled(isInboundActive);

			var isOutboundActive = activeClient.Checked && template.SupportsEntity(AccessRequirement.SupportsOutbound) && isICodeDescriptionPairList;
			outboundControl.AccessTypes = new Dictionary<OutboundCommunicationPartyConfigUserControl.AccessRequirement, bool>()
			{
				{ OutboundCommunicationPartyConfigUserControl.AccessRequirement.SupportsOutbound, template.SupportsEntity(AccessRequirement.SupportsOutbound) && isICodeDescriptionPairList },
				{ OutboundCommunicationPartyConfigUserControl.AccessRequirement.SupportsOutboundNoAuth, template.SupportsEntity(AccessRequirement.SupportsOutboundNoAuth) && isICodeDescriptionPairList },
				{ OutboundCommunicationPartyConfigUserControl.AccessRequirement.SupportsOutboundBasicAuth, template.SupportsEntity(AccessRequirement.SupportsOutboundBasicAuth) && isICodeDescriptionPairList },
				{ OutboundCommunicationPartyConfigUserControl.AccessRequirement.SupportsOutboundOAuth, template.SupportsEntity(AccessRequirement.SupportsOutboundOAuth) && isICodeDescriptionPairList }
			};
			outboundControl.SetControlEnabled(isOutboundActive);
		}

		readonly InboundCommunicationPartyConfigUserControl inboundControl;

		readonly OutboundCommunicationPartyConfigUserControl outboundControl;
		new EDICommunicationParty DataSource
		{
			get { return (EDICommunicationParty)base.DataSource; }
		}

		protected void ActiveCheckBox_OnCheckedChanged(object sender, EventArgs e)
		{
			SetControlVisibility();
		}
	}
}
