using Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic;

namespace Enterprise.Accounting.GUI.JobManagement
{
	partial class AutoJobClosureDiagnosisForm
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		private new void InitializeComponent()
		{
			this.TextBoxStackTrace = new Enterprise.ZArchitecture.ZTextBox();
			this.diagnoseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.numberOfJobsSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.clipboardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 563, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TraceMonitor);
			// 
			// TextBoxStackTrace
			//
			this.BindingSource.SetBindingMember(this.TextBoxStackTrace, "Log");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JCSDiagnosisMonitor)(null)).Log)));
			this.TextBoxStackTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxStackTrace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TextBoxStackTrace.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("672de253-de6f-476e-a5ae-db8ebbd9ff78", "Diagnostic Log");
			this.TextBoxStackTrace.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBoxStackTrace.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextBoxStackTrace.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 47, true);
			this.TextBoxStackTrace.Multiline = true;
			this.TextBoxStackTrace.Name = "TextBoxStackTrace";
			this.TextBoxStackTrace.ReadOnly = true;
			this.TextBoxStackTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.TextBoxStackTrace.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 475, true);
			this.TextBoxStackTrace.TabIndex = 2;
			// 
			// diagnoseButton
			// 
			this.diagnoseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.diagnoseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5cd00dd5-7c3c-49a1-a9db-e9d817150181", "Diagnose");
			this.diagnoseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(614, 17, true);
			this.diagnoseButton.Name = "diagnoseButton";
			this.diagnoseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
			this.diagnoseButton.TabIndex = 1;
			this.diagnoseButton.ToolTipCaption = null;
			this.diagnoseButton.UseVisualStyleBackColor = true;
			this.diagnoseButton.Click += new System.EventHandler(this.BtnCollect_Click);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("38f88e27-918e-47e3-9b10-b21cb3a4473f", "Close");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 531, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.closeButton.TabIndex = 4;
			this.closeButton.ToolTipCaption = null;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.BtnClose_Click);
			// 
			// numberOfJobsSelectedLabel
			// 
			this.numberOfJobsSelectedLabel.AutoSize = true;
			this.numberOfJobsSelectedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.numberOfJobsSelectedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 17, true);
			this.numberOfJobsSelectedLabel.Name = "numberOfJobsSelectedLabel";
			this.numberOfJobsSelectedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 12, true);
			this.numberOfJobsSelectedLabel.TabIndex = 0;
			this.numberOfJobsSelectedLabel.Text = "Number of Jobs selected";
			this.numberOfJobsSelectedLabel.UseMnemonic = false;
			// 
			// clipboardButton
			// 
			this.clipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.clipboardButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fc8d7171-7b33-485e-b92d-68c4cda565c9", "Copy to Clipboard");
			this.clipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 531, true);
			this.clipboardButton.Name = "clipboardButton";
			this.clipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.clipboardButton.TabIndex = 3;
			this.clipboardButton.ToolTipCaption = null;
			this.clipboardButton.UseVisualStyleBackColor = true;
			this.clipboardButton.Click += new System.EventHandler(this.clipboardButton_Click);
			// 
			// AutoJobClosureDiagnosisForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("342eebf7-3ee4-43c5-97f6-ecbd1dc29f4d", "Diagnose Automatic Job Closure");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 587, true);
			this.Controls.Add(this.clipboardButton);
			this.Controls.Add(this.numberOfJobsSelectedLabel);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.diagnoseButton);
			this.Controls.Add(this.TextBoxStackTrace);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.TraceMonitor);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 622, true);
			this.Name = "AutoJobClosureDiagnosisForm";
			this.TopMost = true;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TraceForm_FormClosing);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TextBoxStackTrace, 0);
			this.Controls.SetChildIndex(this.diagnoseButton, 0);
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.numberOfJobsSelectedLabel, 0);
			this.Controls.SetChildIndex(this.clipboardButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox TextBoxStackTrace;
		private ZArchitecture.GUI.ZButton diagnoseButton;
		private ZArchitecture.GUI.ZButton closeButton;
		private ZArchitecture.ZLabel numberOfJobsSelectedLabel;
		private ZArchitecture.GUI.ZButton clipboardButton;
	}
}
