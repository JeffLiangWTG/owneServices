using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class ManualCancelForm
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
		protected new void InitializeComponent()
		{
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RNSManualReleaseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReleaseDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReleaseReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RNSManualReleaseGroupBox.SuspendLayout();
			this.ReleaseDateZDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 120, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.ManualReleaseCancelBO);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 95, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ManualCancelForm|F7EE41B9-EAFD-4FC5-919C-446CC64A687A", "Save");
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// RNSManualReleaseGroupBox
			// 
			this.RNSManualReleaseGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RNSManualReleaseGroupBox.Controls.Add(this.ReleaseDateZDateEdit);
			this.RNSManualReleaseGroupBox.Controls.Add(this.ReleaseReasonTextBox);
			this.RNSManualReleaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.RNSManualReleaseGroupBox.Name = "RNSManualReleaseGroupBox";
			this.RNSManualReleaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 89, true);
			this.RNSManualReleaseGroupBox.TabIndex = 1;
			this.RNSManualReleaseGroupBox.TabStop = false;
			this.RNSManualReleaseGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ManualCancelForm|11DC023C-01C1-4D62-A037-E3CFC6FB5719", "Manual Cancel");
			// 
			// ReleaseDateZDateEdit
			// 
			this.ReleaseDateZDateEdit.AllowDrop = true;
			this.ReleaseDateZDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReleaseDateZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReleaseDateZDateEdit, "ManualReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ManualReleaseCancelBO)(null)).ManualReleaseDate)));
			this.ReleaseDateZDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ManualCancelForm|6BEDF4D5-AAC3-47DF-8CA3-6227AA181925", "Manual Cancel Date");
			this.ReleaseDateZDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReleaseDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 58, true);
			this.ReleaseDateZDateEdit.Name = "ReleaseDateZDateEdit";
			this.ReleaseDateZDateEdit.TabIndex = 1;
			// 
			// ReleaseReasonTextBox
			// 
			this.ReleaseReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseReasonTextBox, "ManualReleaseReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.CA.Business.ManualReleaseCancelBO)(null)).ManualReleaseReason)));
			this.ReleaseReasonTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ManualCancelForm|B1C47915-9771-4B8E-AEFF-6CC850AA149F", "Manual Cancel Reason");
			this.ReleaseReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 20, true);
			this.ReleaseReasonTextBox.Name = "ReleaseReasonTextBox";
			this.ReleaseReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 20, true);
			this.ReleaseReasonTextBox.TabIndex = 0;
			// 
			// ManualCancelForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 144, true);
			this.Controls.Add(this.RNSManualReleaseGroupBox);
			this.Controls.Add(this.SaveButton);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.ManualReleaseCancelBO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 183, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 183, true);
			this.Name = "ManualCancelForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.RNSManualReleaseGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RNSManualReleaseGroupBox.ResumeLayout(false);
			this.RNSManualReleaseGroupBox.PerformLayout();
			this.ReleaseDateZDateEdit.ResumeLayout(true);
			this.ReleaseDateZDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZButton SaveButton;
		private ZGroupBox RNSManualReleaseGroupBox;
		public ZDateEdit ReleaseDateZDateEdit;
		public ZTextBox ReleaseReasonTextBox;
	}
}