namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class RetrospectivelyBroadcastEConversationsForm
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MainLabelTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainGridTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainLabelBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainGridBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BottomLabel = new Enterprise.ZArchitecture.ZLabel();
			this.saveToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.MainLabelTopPanel.SuspendLayout();
			this.MainGridTopPanel.SuspendLayout();
			this.MainLabelBottomPanel.SuspendLayout();
			this.ButtonPanel.SuspendLayout();
			this.MainGridBottomPanel.SuspendLayout();
			this.saveToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 261, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(205);
			this.MainSplitContainer.TabIndex = 0;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.MainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.MainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.MainSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 450, true);
			// 
			// MainGridTopPanel
			//
			this.MainGridTopPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainGridTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGridTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MainGridTopPanel.Name = "MainGridTopPanel";
			this.MainGridTopPanel.TabIndex = 1;
			this.MainGridTopPanel.AutoSize = true;
			this.MainGridTopPanel.AutoScroll = false;
			this.MainGridTopPanel.TabIndex = 1;
			// 
			// TopLabel
			// 
			this.TopLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TopLabel.Name = "TopLabel";
			this.TopLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 46, true);
			this.TopLabel.TabIndex = 1;
			this.TopLabel.Text = "Broadcast messages exist for the related workitems. If you would like to retrospectively send these broadcasts to the newly attached incident subscribers, tick the eConversations and Incidents/Organisations, you would like to send the eConversations to.";
			MainLabelTopPanel.Controls.Add(TopLabel);
			// 
			// MainLabelTopPanel
			// 
			this.MainLabelTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainLabelTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MainLabelTopPanel.Name = "MainLabelTopPanel";
			this.MainLabelTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 50, true);
			this.MainLabelTopPanel.TabIndex = 1;

			this.MainSplitContainer.Panel1.Controls.Add(this.MainGridTopPanel);
			this.MainSplitContainer.Panel1.Controls.Add(this.MainLabelTopPanel);
			// 
			// MainGridBottomPanel
			//
			this.MainGridBottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainGridBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGridBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MainGridBottomPanel.Name = "MainGridBottomPanel";
			this.MainGridBottomPanel.TabIndex = 1;
			this.MainGridBottomPanel.AutoSize = true;
			this.MainGridBottomPanel.AutoScroll = false;
			this.MainGridBottomPanel.TabIndex = 1;
			// 
			// BottomLabel
			// 
			this.BottomLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BottomLabel.Name = "BottomLabel";
			this.BottomLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 23, true);
			this.BottomLabel.TabIndex = 4;
			this.BottomLabel.Text = "Broadcast the selected messages to.";
			this.MainLabelBottomPanel.Controls.Add(BottomLabel);
			// 
			// MainLabelBottomPanel
			// 
			this.MainLabelBottomPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainLabelBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainLabelBottomPanel.Name = "MainLabelBottomPanel";
			this.MainLabelBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 30, true);
			this.MainLabelBottomPanel.TabIndex = 1;
			// 
			// ButtonPanel
			//
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 28, true);
			this.ButtonPanel.TabIndex = 1;
			// 
			// saveToolStrip
			// 
			this.saveToolStrip.AutoSize = true;
			this.saveToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom)));
			this.saveToolStrip.Name = "saveToolStrip";
			this.saveToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.saveToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 25, true);
			this.saveToolStrip.TabIndex = 0;
			this.saveToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.saveToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.ButtonPanel.Controls.Add(saveToolStrip);

			this.MainSplitContainer.Panel2.Controls.Add(this.ButtonPanel);
			this.MainSplitContainer.Panel2.Controls.Add(this.MainGridBottomPanel);
			this.MainSplitContainer.Panel2.Controls.Add(this.MainLabelBottomPanel);
			// 
			// RetrospectivelyBroadcastEConversationsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = false;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 550, true);
			this.Controls.Add(this.MainSplitContainer);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "RetrospectivelyBroadcastEConversationsForm";
			this.Text = "Existing Broadcast eConversations";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.MainLabelTopPanel.ResumeLayout(false);
			this.MainLabelTopPanel.PerformLayout();
			this.MainGridTopPanel.ResumeLayout(false);
			this.MainGridTopPanel.PerformLayout();
			this.MainLabelBottomPanel.ResumeLayout(false);
			this.MainLabelBottomPanel.PerformLayout();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.MainGridBottomPanel.ResumeLayout(false);
			this.MainGridBottomPanel.PerformLayout();
			this.saveToolStrip.ResumeLayout(false);
			this.saveToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion


		CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		ZArchitecture.ZLabel TopLabel;
		Enterprise.ZArchitecture.GUI.ZPanel MainLabelTopPanel;
		Enterprise.ZArchitecture.GUI.ZPanel MainGridTopPanel;
		ZArchitecture.ZLabel BottomLabel;
		Enterprise.ZArchitecture.GUI.ZPanel MainLabelBottomPanel;
		Enterprise.ZArchitecture.GUI.ZPanel ButtonPanel;
		Enterprise.ZArchitecture.GUI.ZPanel MainGridBottomPanel;
		Enterprise.ZArchitecture.GUI.ZToolStrip saveToolStrip;
	}
}
