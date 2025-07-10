using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Startup.Tools
{
	partial class HotKeyMonitorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			HotKeyMonitorProvider.GetHotKeyMonitor().ClearAll();
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.PNLButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TxtKeyPressSpans = new Enterprise.ZArchitecture.ZTextBox();
			this.BtnCopy = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BtnClear = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BtnStart = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TxtAdditionalMsg = new Enterprise.ZArchitecture.ZTextBox();
			this.SpanTimes = new Enterprise.ZArchitecture.GUI.ZListBox();
			this.OnlyShowProcessedKeysCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.PNLButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 629, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 21, true);
			// 
			// PNLButtons
			//
			this.PNLButtons.Controls.Add(this.TxtAdditionalMsg);
			this.PNLButtons.Controls.Add(this.BtnCopy);
			this.PNLButtons.Controls.Add(this.BtnClear);
			this.PNLButtons.Controls.Add(this.BtnStart);
			this.PNLButtons.Controls.Add(this.OnlyShowProcessedKeysCheckBox);
			this.PNLButtons.Dock = System.Windows.Forms.DockStyle.Top;
			this.PNLButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PNLButtons.Name = "PNLButtons";
			this.PNLButtons.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.PNLButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 92, true);
			this.PNLButtons.TabIndex = 0;
			// 
			// TxtKeyPressSpans
			//
			this.TxtKeyPressSpans.CaptionResourceString = CargoWise.Main.Res.GetData("b95d1827-847d-45ca-8086-47648045c19e", "Text Key Press Spans");
			this.TxtKeyPressSpans.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TxtKeyPressSpans.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TxtKeyPressSpans.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 95, true);
			this.TxtKeyPressSpans.Multiline = true;
			this.TxtKeyPressSpans.Name = "TxtKeyPressSpans";
			this.TxtKeyPressSpans.ReadOnly = true;
			this.TxtKeyPressSpans.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.TxtKeyPressSpans.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 488, true);
			this.TxtKeyPressSpans.TabIndex = 6;
			this.TxtKeyPressSpans.WordWrap = false;
			// 
			// BtnCopy
			// 
			this.BtnCopy.BackColor = System.Drawing.Color.SkyBlue;
			this.BtnCopy.IsCaptionOverridden = true;
			this.BtnCopy.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(850, 2, true);
			this.BtnCopy.Name = "BtnCopy";
			this.BtnCopy.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BtnCopy.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 29, true);
			this.BtnCopy.TabIndex = 1;
			this.BtnCopy.Text = Res.GetString("9c8f9fe0-3ee9-4295-86cb-4aaa4542c3d7", "Copy All");
			this.BtnCopy.ToolTipCaption = null;
			this.BtnCopy.UseVisualStyleBackColor = false;
			this.BtnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
			// 
			// BtnClear
			// 
			this.BtnClear.BackColor = System.Drawing.Color.LightPink;
			this.BtnClear.IsCaptionOverridden = true;
			this.BtnClear.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(928, 2, true);
			this.BtnClear.Name = "BtnClear";
			this.BtnClear.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BtnClear.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 29, true);
			this.BtnClear.TabIndex = 2;
			this.BtnClear.Text = Res.GetString("bda29513-c26d-4b0f-8423-76c875ba2338", "Clear");
			this.BtnClear.ToolTipCaption = null;
			this.BtnClear.UseVisualStyleBackColor = false;
			this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// BtnStart
			// 
			this.BtnStart.BackColor = System.Drawing.Color.DarkSeaGreen;
			this.BtnStart.IsCaptionOverridden = true;
			this.BtnStart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(850, 33, true);
			this.BtnStart.Name = "BtnStart";
			this.BtnStart.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BtnStart.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 29, true);
			this.BtnStart.TabIndex = 3;
			this.BtnStart.Text = Res.GetString("1585a857-c36c-413b-8d2f-ad65405229bd", "Start");
			this.BtnStart.ToolTipCaption = null;
			this.BtnStart.UseVisualStyleBackColor = false;
			this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
			// 
			// TxtAdditionalMsg
			//
			this.TxtAdditionalMsg.CaptionResourceString = CargoWise.Main.Res.GetData("942a5b6b-85cf-4b54-96fb-e460310c1c43", "Text Additional Message");
			this.TxtAdditionalMsg.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TxtAdditionalMsg.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.TxtAdditionalMsg.Multiline = true;
			this.TxtAdditionalMsg.Name = "TxtAdditionalMsg";
			this.TxtAdditionalMsg.Text = Res.GetString("35bcfa1f-c2ed-4785-b613-a8a7ef1abf8f", "You can add more details here...");
			this.TxtAdditionalMsg.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 90, true);
			this.TxtAdditionalMsg.TabIndex = 0;
			// 
			// SpanTimes
			//
			this.SpanTimes.Dock = DockStyle.Fill;
			this.SpanTimes.FormattingEnabled = true;
			this.SpanTimes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 95, true);
			this.SpanTimes.Name = "SpanTimes";
			this.SpanTimes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 520, true);
			this.SpanTimes.TabIndex = 5;
			this.SpanTimes.SelectedIndexChanged += new System.EventHandler(this.SpanTimes_SelectedIndexChanged);
			// 
			// OnlyShowProcessedKeysCheckBox
			// 
			this.OnlyShowProcessedKeysCheckBox.AutoSize = true;
			this.OnlyShowProcessedKeysCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(852, 68, true);
			this.OnlyShowProcessedKeysCheckBox.Name = "OnlyShowProcessedKeysCheckBox";
			this.OnlyShowProcessedKeysCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.OnlyShowProcessedKeysCheckBox.TabIndex = 4;
			this.OnlyShowProcessedKeysCheckBox.Text = Res.GetString("3081fd6d-e20b-4714-b7f7-a12f16b15d30", "Only Show Processed Keys");
			this.OnlyShowProcessedKeysCheckBox.UseVisualStyleBackColor = true;
			this.OnlyShowProcessedKeysCheckBox.CheckedChanged += new System.EventHandler(this.OnlyShowProcessedKeysCheckBox_CheckedChanged);
			// 
			// SplitContainer
			//
			this.SplitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 95, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 488, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(314);
			this.SplitContainer.TabIndex = 6;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.SpanTimes);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.TxtKeyPressSpans);
			// 
			// HotKeyMonitorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 605, true);
			this.Controls.Add(this.PNLButtons);
			this.Controls.Add(this.SplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 605, true);
			this.Name = "HotKeyMonitorForm";
			this.Text = Res.GetString("71bb1773-2dcf-431f-9e6e-a92cf779e53d", "Hot Key Monitor Form");
			this.TopMost = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PNLButtons, 0);
			this.Controls.Add(this.SplitContainer);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.PNLButtons.ResumeLayout(false);
			this.PNLButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZPanel PNLButtons;
		private Enterprise.ZArchitecture.ZTextBox TxtKeyPressSpans;
		private Enterprise.ZArchitecture.GUI.ZButton BtnCopy;
		private Enterprise.ZArchitecture.GUI.ZButton BtnClear;
		private Enterprise.ZArchitecture.GUI.ZButton BtnStart;
		private Enterprise.ZArchitecture.ZTextBox TxtAdditionalMsg;
		private Enterprise.ZArchitecture.GUI.ZListBox SpanTimes;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OnlyShowProcessedKeysCheckBox;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
	}
}
