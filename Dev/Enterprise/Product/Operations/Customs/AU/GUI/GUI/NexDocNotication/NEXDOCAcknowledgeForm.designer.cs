namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class NEXDOCAcknowledgeForm
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
		protected override void InitializeComponent()
		{
			this.AcceptButtonNew = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RejectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButtonNew = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HeaderLable = new Enterprise.ZArchitecture.ZLabel();
			this.ExporterReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RexNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 156, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification);
			// 
			// AcceptButtonNew
			// 
			this.AcceptButtonNew.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("239742e5-17e2-488d-ba68-656511f496de", "Accept");
			this.AcceptButtonNew.IsCaptionOverridden = false;
			this.AcceptButtonNew.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 123, true);
			this.AcceptButtonNew.Name = "AcceptButtonNew";
			this.AcceptButtonNew.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AcceptButtonNew.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AcceptButtonNew.TabIndex = 6;
			this.AcceptButtonNew.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AcceptButtonNew.ToolTipCaption = null;
			this.AcceptButtonNew.Click += new System.EventHandler(this.AcceptButtonNew_Click);
			// 
			// RejectButton
			// 
			this.RejectButton.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("906b812b-b771-4895-bce3-dd0431fc59cd", "Reject");
			this.RejectButton.IsCaptionOverridden = false;
			this.RejectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 123, true);
			this.RejectButton.Name = "RejectButton";
			this.RejectButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RejectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RejectButton.TabIndex = 7;
			this.RejectButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RejectButton.ToolTipCaption = null;
			this.RejectButton.Click += new System.EventHandler(this.RejectButton_Click);
			// 
			// CancelButtonNew
			// 
			this.CancelButtonNew.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("0d2569df-cc92-4b5d-970d-9442ce8d5041", "Cancel");
			this.CancelButtonNew.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonNew.IsCaptionOverridden = false;
			this.CancelButtonNew.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 123, true);
			this.CancelButtonNew.Name = "CancelButtonNew";
			this.CancelButtonNew.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButtonNew.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButtonNew.TabIndex = 8;
			this.CancelButtonNew.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButtonNew.ToolTipCaption = null;
			// 
			// HeaderLable
			// 
			this.HeaderLable.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HeaderLable.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 9, true);
			this.HeaderLable.Name = "HeaderLable";
			this.HeaderLable.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 23, true);
			this.HeaderLable.TabIndex = 1;
			// 
			// ExporterReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExporterReferenceTextBox, "QN_ExporterReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).QN_ExporterReference)));
			this.ExporterReferenceTextBox.CaptionResourceString = null;
			this.ExporterReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 72, true);
			this.ExporterReferenceTextBox.Name = "ExporterReferenceTextBox";
			this.ExporterReferenceTextBox.ReadOnly = true;
			this.ExporterReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 20, true);
			this.ExporterReferenceTextBox.TabIndex = 5;
			// 
			// RexNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RexNumberTextBox, "QN_RexNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).QN_RexNumber)));
			this.RexNumberTextBox.CaptionResourceString = null;
			this.RexNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 49, true);
			this.RexNumberTextBox.Name = "RexNumberTextBox";
			this.RexNumberTextBox.ReadOnly = true;
			this.RexNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 20, true);
			this.RexNumberTextBox.TabIndex = 3;
			// 
			// NEXDOCAcknowledgeForm
			// 
			this.AcceptButton = this.AcceptButtonNew;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelButtonNew;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 180, true);
			this.Controls.Add(this.AcceptButtonNew);
			this.Controls.Add(this.RejectButton);
			this.Controls.Add(this.CancelButtonNew);
			this.Controls.Add(this.ExporterReferenceTextBox);
			this.Controls.Add(this.RexNumberTextBox);
			this.Controls.Add(this.HeaderLable);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 219, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 219, true);
			this.Name = "NEXDOCAcknowledgeForm";
			this.Text = "NEXDOCAcknowledgeForm";
			this.Controls.SetChildIndex(this.HeaderLable, 0);
			this.Controls.SetChildIndex(this.RexNumberTextBox, 0);
			this.Controls.SetChildIndex(this.ExporterReferenceTextBox, 0);
			this.Controls.SetChildIndex(this.CancelButtonNew, 0);
			this.Controls.SetChildIndex(this.RejectButton, 0);
			this.Controls.SetChildIndex(this.AcceptButtonNew, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZButton AcceptButtonNew;
		private Enterprise.ZArchitecture.GUI.ZButton RejectButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelButtonNew;
		private Enterprise.ZArchitecture.ZLabel HeaderLable;

		#endregion
		private ZArchitecture.ZTextBox RexNumberTextBox;
		private ZArchitecture.ZTextBox ExporterReferenceTextBox;
	}
}
