
namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	partial class FeatureControlExportPopupForm
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
		protected override void InitializeComponent()
		{
			this.ButtonExport = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LicenceDatabaseFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ExportedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AllowAllFeatureStagesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportRawTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LicenceDatabaseFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 568, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 0, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.FeatureControl.Business.FeatureControlExportBizObj);
			// 
			// ButtonExport
			// 
			this.ButtonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonExport.IsCaptionOverridden = true;
			this.ButtonExport.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 15, true);
			this.ButtonExport.Name = "ButtonExport";
			this.ButtonExport.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 23, true);
			this.ButtonExport.TabIndex = 3;
			this.ButtonExport.Text = "Export";
			this.ButtonExport.ToolTipCaption = null;
			this.ButtonExport.UseVisualStyleBackColor = true;
			this.ButtonExport.Click += new System.EventHandler(this.ButtonOk_Click);
			// 
			// LicenceDatabaseFindBox
			// 
			this.LicenceDatabaseFindBox.AllowDrop = true;
			this.LicenceDatabaseFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LicenceDatabaseFindBox, "DatabasePk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlExportBizObj)(null)).DatabasePk)));
			this.LicenceDatabaseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 17, true);
			this.LicenceDatabaseFindBox.Name = "LicenceDatabaseFindBox";
			this.LicenceDatabaseFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LicenceDatabaseFindBox.ParentType = null;
			this.LicenceDatabaseFindBox.ShowDescriptionBox = false;
			this.LicenceDatabaseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.LicenceDatabaseFindBox.TabIndex = 1;
			// 
			// ExportedTextBox
			// 
			this.ExportedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExportedTextBox, "RuleContent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlExportBizObj)(null)).RuleContent)));
			this.ExportedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExportedTextBox, false);
			this.ExportedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 301, true);
			this.ExportedTextBox.Multiline = true;
			this.ExportedTextBox.Name = "ExportedTextBox";
			this.ExportedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ExportedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 255, true);
			this.ExportedTextBox.TabIndex = 5;
			// 
			// AllowAllFeatureStagesCheckBox
			// 
			this.AllowAllFeatureStagesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AllowAllFeatureStagesCheckBox, "AllowAllFeatureStages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlExportBizObj)(null)).AllowAllFeatureStages)));
			this.AllowAllFeatureStagesCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AllowAllFeatureStagesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 19, true);
			this.AllowAllFeatureStagesCheckBox.Name = "AllowAllFeatureStagesCheckBox";
			this.AllowAllFeatureStagesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 16, true);
			this.AllowAllFeatureStagesCheckBox.TabIndex = 2;
			this.AllowAllFeatureStagesCheckBox.Text = "Enable All Feature Codes on Client System";
			this.AllowAllFeatureStagesCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExportRawTextBox
			// 
			this.ExportRawTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExportRawTextBox, "RuleContentRaw");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlExportBizObj)(null)).RuleContentRaw)));
			this.ExportRawTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExportRawTextBox, false);
			this.ExportRawTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 42, true);
			this.ExportRawTextBox.Multiline = true;
			this.ExportRawTextBox.Name = "ExportRawTextBox";
			this.ExportRawTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ExportRawTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 245, true);
			this.ExportRawTextBox.TabIndex = 4;
			// 
			// FeatureControlExportPopupForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 568, true);
			this.Controls.Add(this.ExportRawTextBox);
			this.Controls.Add(this.AllowAllFeatureStagesCheckBox);
			this.Controls.Add(this.ExportedTextBox);
			this.Controls.Add(this.LicenceDatabaseFindBox);
			this.Controls.Add(this.ButtonExport);
			this.DataSourceType = typeof(Enterprise.Client.EDI.FeatureControl.Business.FeatureControlExportBizObj);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FeatureControlExportPopupForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.Text = "Export";
			this.Controls.SetChildIndex(this.ButtonExport, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LicenceDatabaseFindBox, 0);
			this.Controls.SetChildIndex(this.ExportedTextBox, 0);
			this.Controls.SetChildIndex(this.AllowAllFeatureStagesCheckBox, 0);
			this.Controls.SetChildIndex(this.ExportRawTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LicenceDatabaseFindBox.ResumeLayout(true);
			this.LicenceDatabaseFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton ButtonExport;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox LicenceDatabaseFindBox;
		private ZArchitecture.ZTextBox ExportedTextBox;
		private ZArchitecture.GUI.ZCheckBox AllowAllFeatureStagesCheckBox;
		private ZArchitecture.ZTextBox ExportRawTextBox;
	}
}
