namespace Enterprise.Security.ActiveDirectory.GUI.Registry
{
	partial class ADRegistryControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.UpdateNoteLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.UpdateNoteHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SyncDirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SyncModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IntegrationEnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EntitySyncDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SingleSignOnGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsSingleSignOnCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SingleSignOnHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SyncDirectionGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionGroupBox.SuspendLayout();
			this.SyncDirectionDropEdit.SuspendLayout();
			this.SyncModeDropEdit.SuspendLayout();
			this.EntitySyncDropEdit.SuspendLayout();
			this.SingleSignOnGroupBox.SuspendLayout();
			this.SyncDirectionGroupDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Security.ActiveDirectory.ADConfig);
			// 
			// UpdateNoteLinkLabel
			// 
			this.UpdateNoteLinkLabel.AutoSize = true;
			this.UpdateNoteLinkLabel.CaptionResourceString = Enterprise.Security.ActiveDirectory.GUI.Res.GetData("a80abe98-2b23-4fbd-9714-827fb6b47c3b", "For more information, see this update note:");
			this.UpdateNoteLinkLabel.IsFontBold = false;
			this.UpdateNoteLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 31, true);
			this.UpdateNoteLinkLabel.Name = "UpdateNoteLinkLabel";
			this.UpdateNoteLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 13, true);
			this.UpdateNoteLinkLabel.TabIndex = 1;
			this.UpdateNoteLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.UpdateNoteLinkLabel_LinkClicked);
			// 
			// UpdateNoteHintLabel
			// 
			this.UpdateNoteHintLabel.AutoSize = true;
			this.UpdateNoteHintLabel.CaptionResourceString = Enterprise.Security.ActiveDirectory.GUI.Res.GetData("341696ae-853f-40a7-9137-1f5d2a2d093a", "Please read this update note before using this feature:");
			this.UpdateNoteHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UpdateNoteHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
			this.UpdateNoteHintLabel.Name = "UpdateNoteHintLabel";
			this.UpdateNoteHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 13, true);
			this.UpdateNoteHintLabel.TabIndex = 11;
			// 
			// OptionGroupBox
			// 
			this.OptionGroupBox.CaptionResourceString = Enterprise.Security.ActiveDirectory.GUI.Res.GetData("97c26840-a9de-4a15-9b53-fd9024a670f8", "Enable Integration");
			this.OptionGroupBox.Controls.Add(this.SyncDirectionGroupDropEdit);
			this.OptionGroupBox.Controls.Add(this.SyncDirectionDropEdit);
			this.OptionGroupBox.Controls.Add(this.SyncModeDropEdit);
			this.OptionGroupBox.Controls.Add(this.IntegrationEnabledCheckBox);
			this.OptionGroupBox.Controls.Add(this.EntitySyncDropEdit);
			this.OptionGroupBox.Controls.Add(this.UpdateNoteLinkLabel);
			this.OptionGroupBox.Controls.Add(this.UpdateNoteHintLabel);
			this.OptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionGroupBox.Name = "OptionGroupBox";
			this.OptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 181, true);
			this.OptionGroupBox.TabIndex = 0;
			this.OptionGroupBox.TabStop = false;
			// 
			// SyncDirectionDropEdit
			// 
			this.SyncDirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SyncDirectionDropEdit, "SyncDirectionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Security.ActiveDirectory.ADConfig)(null)).SyncDirectionCode)));
			this.SyncDirectionDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SyncDirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 130, true);
			this.SyncDirectionDropEdit.Name = "SyncDirectionDropEdit";
			this.SyncDirectionDropEdit.PreBoundMaxLength = 3;
			this.SyncDirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 26, true);
			this.SyncDirectionDropEdit.TabIndex = 5;
			// 
			// SyncModeDropEdit
			// 
			this.SyncModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SyncModeDropEdit, "SyncModeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Security.ActiveDirectory.ADConfig)(null)).SyncModeCode)));
			this.SyncModeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SyncModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 104, true);
			this.SyncModeDropEdit.Name = "SyncModeDropEdit";
			this.SyncModeDropEdit.PreBoundMaxLength = 3;
			this.SyncModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 26, true);
			this.SyncModeDropEdit.TabIndex = 4;
			// 
			// IntegrationEnabledCheckBox
			// 
			this.IntegrationEnabledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntegrationEnabledCheckBox, "IsADIntegrationEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADConfig)(null)).IsADIntegrationEnabled)));
			this.IntegrationEnabledCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IntegrationEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 59, true);
			this.IntegrationEnabledCheckBox.Name = "IntegrationEnabledCheckBox";
			this.IntegrationEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.IntegrationEnabledCheckBox.TabIndex = 2;
			this.IntegrationEnabledCheckBox.UseVisualStyleBackColor = true;
			// 
			// EntitySyncDropEdit
			// 
			this.EntitySyncDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntitySyncDropEdit, "EntitiesToSyncCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Security.ActiveDirectory.ADConfig)(null)).EntitiesToSyncCode)));
			this.EntitySyncDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EntitySyncDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 78, true);
			this.EntitySyncDropEdit.Name = "EntitySyncDropEdit";
			this.EntitySyncDropEdit.PreBoundMaxLength = 3;
			this.EntitySyncDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 26, true);
			this.EntitySyncDropEdit.TabIndex = 3;
			// 
			// SingleSignOnGroupBox
			// 
			this.SingleSignOnGroupBox.CaptionResourceString = Enterprise.Security.ActiveDirectory.GUI.Res.GetData("b59dd8a1-a7df-4ca4-b873-b3834069c05a", "Single Sign On");
			this.SingleSignOnGroupBox.Controls.Add(this.IsSingleSignOnCheckBox);
			this.SingleSignOnGroupBox.Controls.Add(this.SingleSignOnHintLabel);
			this.SingleSignOnGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 181, true);
			this.SingleSignOnGroupBox.Name = "SingleSignOnGroupBox";
			this.SingleSignOnGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 91, true);
			this.SingleSignOnGroupBox.TabIndex = 1;
			this.SingleSignOnGroupBox.TabStop = false;
			// 
			// IsSingleSignOnCheckBox
			// 
			this.IsSingleSignOnCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSingleSignOnCheckBox, "IsSingleSignOn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADConfig)(null)).IsSingleSignOn)));
			this.IsSingleSignOnCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IsSingleSignOnCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 53, true);
			this.IsSingleSignOnCheckBox.Name = "IsSingleSignOnCheckBox";
			this.IsSingleSignOnCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.IsSingleSignOnCheckBox.TabIndex = 6;
			this.IsSingleSignOnCheckBox.UseVisualStyleBackColor = true;
			// 
			// SingleSignOnHintLabel
			// 
			this.SingleSignOnHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SingleSignOnHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.SingleSignOnHintLabel.Name = "SingleSignOnHintLabel";
			this.SingleSignOnHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 32, true);
			this.SingleSignOnHintLabel.TabIndex = 12;
			this.SingleSignOnHintLabel.UseMnemonic = false;
			// 
			// SyncDirectionGroupDropEdit
			// 
			this.SyncDirectionGroupDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SyncDirectionGroupDropEdit, "SyncDirectionGroupCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Security.ActiveDirectory.ADConfig)(null)).SyncDirectionGroupCode)));
			this.SyncDirectionGroupDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SyncDirectionGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 156, true);
			this.SyncDirectionGroupDropEdit.Name = "SyncDirectionGroupDropEdit";
			this.SyncDirectionGroupDropEdit.PreBoundMaxLength = 3;
			this.SyncDirectionGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 26, true);
			this.SyncDirectionGroupDropEdit.TabIndex = 13;
			// 
			// ADRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SingleSignOnGroupBox);
			this.Controls.Add(this.OptionGroupBox);
			this.Name = "ADRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 315, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionGroupBox.ResumeLayout(false);
			this.OptionGroupBox.PerformLayout();
			this.SyncDirectionDropEdit.ResumeLayout(true);
			this.SyncDirectionDropEdit.PerformLayout();
			this.SyncModeDropEdit.ResumeLayout(true);
			this.SyncModeDropEdit.PerformLayout();
			this.EntitySyncDropEdit.ResumeLayout(true);
			this.EntitySyncDropEdit.PerformLayout();
			this.SingleSignOnGroupBox.ResumeLayout(false);
			this.SingleSignOnGroupBox.PerformLayout();
			this.SyncDirectionGroupDropEdit.ResumeLayout(true);
			this.SyncDirectionGroupDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZLabel UpdateNoteHintLabel;
		ZArchitecture.GUI.ZLinkLabel UpdateNoteLinkLabel;
		ZArchitecture.GUI.ZGroupBox OptionGroupBox;
		ZArchitecture.GUI.ZDropEdit EntitySyncDropEdit;
		ZArchitecture.GUI.ZGroupBox SingleSignOnGroupBox;
		ZArchitecture.ZLabel SingleSignOnHintLabel;
		ZArchitecture.GUI.ZCheckBox IntegrationEnabledCheckBox;
		ZArchitecture.GUI.ZDropEdit SyncModeDropEdit;
		ZArchitecture.GUI.ZDropEdit SyncDirectionDropEdit;
		ZArchitecture.GUI.ZCheckBox IsSingleSignOnCheckBox;
		private ZArchitecture.GUI.ZDropEdit SyncDirectionGroupDropEdit;
	}
}
