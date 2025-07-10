
namespace Enterprise.Client.JAS.GUI
{
	partial class JASDataExporterUserControl
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
				if (this.ExportDirectoryBrowserDialog != null)
				{
					this.ExportDirectoryBrowserDialog.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DirectoryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExportDirectoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeliveryMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeliveryMethodLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TargetLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EmailPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zRadioButton1 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton2 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.EmailGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportDirectoryBrowserDialog = new Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DirectoryPanel.SuspendLayout();
			this.EmailPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.JAS.Business.JASDataExporterBizO);
			// 
			// DirectoryPanel
			// 
			this.DirectoryPanel.Controls.Add(this.ExportDirectoryTextBox);
			this.DirectoryPanel.Controls.Add(this.BrowseButton);
			this.DirectoryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 26, true);
			this.DirectoryPanel.Name = "DirectoryPanel";
			this.DirectoryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 34, true);
			this.DirectoryPanel.TabIndex = 8;
			// 
			// ExportDirectoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportDirectoryTextBox, "ExportDirectory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(null)).ExportDirectory)));
			this.ExportDirectoryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExportDirectoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.ExportDirectoryTextBox.Name = "ExportDirectoryTextBox";
			this.ExportDirectoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.ExportDirectoryTextBox.TabIndex = 0;
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 0, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 23, true);
			this.BrowseButton.TabIndex = 1;
			this.BrowseButton.Text = "...";
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// DeliveryMethodDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DeliveryMethodDropEdit, "DeliveryMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(null)).DeliveryMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(null)).DeliveryMethodList)));
			this.DeliveryMethodDropEdit.BindToList = "DeliveryMethodList";
			this.DeliveryMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 0, true);
			this.DeliveryMethodDropEdit.Name = "DeliveryMethodDropEdit";
			this.DeliveryMethodDropEdit.PreBoundMaxLength = 3;
			this.DeliveryMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.DeliveryMethodDropEdit.TabIndex = 4;
			// 
			// DeliveryMethodLabel
			// 
			this.DeliveryMethodLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeliveryMethodLabel.Name = "DeliveryMethodLabel";
			this.DeliveryMethodLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.DeliveryMethodLabel.TabIndex = 7;
			this.DeliveryMethodLabel.Text = "Delivery Method";
			// 
			// TargetLabel
			// 
			this.TargetLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.TargetLabel.Name = "TargetLabel";
			this.TargetLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.TargetLabel.TabIndex = 5;
			this.TargetLabel.Text = "Target";
			// 
			// EmailPanel
			// 
			this.EmailPanel.Controls.Add(this.zRadioButton1);
			this.EmailPanel.Controls.Add(this.zRadioButton2);
			this.EmailPanel.Controls.Add(this.EmailGroupGuidFindBox);
			this.EmailPanel.Controls.Add(this.EmailTextBox);
			this.EmailPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 26, true);
			this.EmailPanel.Name = "EmailPanel";
			this.EmailPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 47, true);
			this.EmailPanel.TabIndex = 6;
			// 
			// zRadioButton1
			// 
			this.zRadioButton1.AutoCheck = false;
			this.zRadioButton1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zRadioButton1, "IsIndividualEmailRecipient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(null)).IsIndividualEmailRecipient)));
			this.zRadioButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.zRadioButton1.Name = "zRadioButton1";
			this.zRadioButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 17, true);
			this.zRadioButton1.TabIndex = 0;
			this.zRadioButton1.TabStop = true;
			this.zRadioButton1.Text = "Enter Email Address";
			this.zRadioButton1.UseVisualStyleBackColor = true;
			// 
			// zRadioButton2
			// 
			this.zRadioButton2.AutoCheck = false;
			this.zRadioButton2.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zRadioButton2, "IsGroupEmailRecipient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(null)).IsGroupEmailRecipient)));
			this.zRadioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 3, true);
			this.zRadioButton2.Name = "zRadioButton2";
			this.zRadioButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 17, true);
			this.zRadioButton2.TabIndex = 1;
			this.zRadioButton2.TabStop = true;
			this.zRadioButton2.Text = "Select Group to Email";
			this.zRadioButton2.UseVisualStyleBackColor = true;
			// 
			// EmailGroupGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.EmailGroupGuidFindBox, "EmailGroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(null)).EmailGroupPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(null)).EmailGroups)));
			this.EmailGroupGuidFindBox.BindToList = "EmailGroups";
			this.EmailGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.EmailGroupGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			this.EmailGroupGuidFindBox.Name = "EmailGroupGuidFindBox";
			this.EmailGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.EmailGroupGuidFindBox.TabIndex = 0;
			// 
			// EmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.EmailTextBox, "EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(null)).EmailAddress)));
			this.EmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 26, true);
			this.EmailTextBox.Name = "EmailTextBox";
			this.EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.EmailTextBox.TabIndex = 2;
			// 
			// JASDataExporterUserControl
			// 
			this.Controls.Add(this.DirectoryPanel);
			this.Controls.Add(this.DeliveryMethodDropEdit);
			this.Controls.Add(this.DeliveryMethodLabel);
			this.Controls.Add(this.TargetLabel);
			this.Controls.Add(this.EmailPanel);
			this.Name = "JASDataExporterUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 82, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DirectoryPanel.ResumeLayout(false);
			this.DirectoryPanel.PerformLayout();
			this.EmailPanel.ResumeLayout(false);
			this.EmailPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZPanel DirectoryPanel;
		internal Enterprise.ZArchitecture.ZTextBox ExportDirectoryTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton BrowseButton;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit DeliveryMethodDropEdit;
		internal Enterprise.ZArchitecture.ZLabel DeliveryMethodLabel;
		internal Enterprise.ZArchitecture.ZLabel TargetLabel;
		internal Enterprise.ZArchitecture.GUI.ZPanel EmailPanel;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton1;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton2;
		internal Enterprise.ZArchitecture.ZTextBox EmailTextBox;
		internal Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog ExportDirectoryBrowserDialog;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox EmailGroupGuidFindBox;
	}
}
