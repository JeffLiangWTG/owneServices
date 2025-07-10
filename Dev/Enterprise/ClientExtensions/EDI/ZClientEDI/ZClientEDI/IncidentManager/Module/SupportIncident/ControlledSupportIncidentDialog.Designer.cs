using CargoWise.Common;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public partial class ControlledSupportIncidentDialog : ZChildForm
	{
		protected new void InitializeComponent()
		{
			this.contentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.openAsViewModeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.openAsEditModeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.openLinkedGroupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 129, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 5, true);
			this.MainStatusBar.Visible = false;
			//
			// contentsLabel
			//
			this.contentsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.contentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.contentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 4, true);
			this.contentsLabel.Name = "contentsLabel";
			this.contentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 88, true);
			this.contentsLabel.TabIndex = 0;
			//
			// openAsEditModeButton
			//
			this.openAsEditModeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.openAsEditModeButton.CaptionResourceString = ZClientEDI.Res.GetData("e666ad05-09a2-4a19-9640-a05440fb1c16", "Override and Edit");
			this.openAsEditModeButton.IsCaptionOverridden = true;
			this.openAsEditModeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 103, true);
			this.openAsEditModeButton.Name = "openAsEditModeButton";
			this.openAsEditModeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 23, true);
			this.openAsEditModeButton.TabIndex = 1;
			this.openAsEditModeButton.Text = this.openAsEditModeButton.CaptionResourceString.Caption;
			this.openAsEditModeButton.ToolTipCaption = null;
			this.openAsEditModeButton.Click += new System.EventHandler(this.openAsEditModeButton_Click);
			//
			// openLinkedGroupButton
			//
			this.openLinkedGroupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.openLinkedGroupButton.CaptionResourceString = ZClientEDI.Res.GetData("8b55f4b7-6233-4ee9-9730-b4063b51199f", "Open Incident Group");
			this.openLinkedGroupButton.IsCaptionOverridden = true;
			this.openLinkedGroupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 103, true);
			this.openLinkedGroupButton.Name = "openLinkedGroupButton";
			this.openLinkedGroupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23, true);
			this.openLinkedGroupButton.TabIndex = 2;
			this.openLinkedGroupButton.Text = this.openLinkedGroupButton.CaptionResourceString.Caption;
			this.openLinkedGroupButton.ToolTipCaption = null;
			this.openLinkedGroupButton.Click += new System.EventHandler(this.openLinkedGroupButton_Click);
			//
			// openAsViewModeButton
			//
			this.openAsViewModeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.openAsViewModeButton.CaptionResourceString = ZClientEDI.Res.GetData("28564a45-c4c3-4c91-bdd1-a5d7e7b4ee86", "View Incident");
			this.openAsViewModeButton.IsCaptionOverridden = true;
			this.openAsViewModeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 103, true);
			this.openAsViewModeButton.Name = "openAsViewModeButton";
			this.openAsViewModeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.openAsViewModeButton.TabIndex = 3;
			this.openAsViewModeButton.Text = this.openAsViewModeButton.CaptionResourceString.Caption;
			this.openAsViewModeButton.ToolTipCaption = null;
			this.openAsViewModeButton.Click += new System.EventHandler(this.openAsViewModeButton_Click);
			//
			// ControlledSupportIncidentDialog
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 134, true);
			this.Controls.Add(this.openAsViewModeButton);
			this.Controls.Add(this.openAsEditModeButton);
			this.Controls.Add(this.openLinkedGroupButton);
			this.Controls.Add(this.contentsLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ControlledSupportIncidentDialog";
			this.Controls.SetChildIndex(this.contentsLabel, 0);
			this.Controls.SetChildIndex(this.openLinkedGroupButton, 0);
			this.Controls.SetChildIndex(this.openAsEditModeButton, 0);
			this.Controls.SetChildIndex(this.openAsViewModeButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
