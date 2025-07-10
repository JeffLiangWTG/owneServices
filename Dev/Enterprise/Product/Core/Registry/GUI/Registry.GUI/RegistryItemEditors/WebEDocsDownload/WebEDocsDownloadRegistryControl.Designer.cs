namespace Enterprise.Registry.GUI
{
	partial class WebEDocsDownloadRegistryControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ModulesComboBox = new CargoWise.Windows.UI.KComboBox();
			this.AllowAllDocTypesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.RefDocTypeEntryCollection);
			// 
			// ModulesComboBox
			// 
			this.ModulesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ModulesComboBox.FormattingEnabled = true;
			this.ModulesComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 10, true);
			this.ModulesComboBox.Name = "ModulesComboBox";
			this.ModulesComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 19, true);
			this.ModulesComboBox.TabIndex = 0;
			this.ModulesComboBox.SelectedIndexChanged += ModulesComboBox_SelectedIndexChanged;
			this.ModulesComboBox.DisplayMember = "Description";
			// 
			// AllowAllDocTypesCheckBox
			// 
			this.AllowAllDocTypesCheckBox.AutoSize = true;
			this.AllowAllDocTypesCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|FA16978C-03C8-4508-ABBC-C3913F537AC9", "Allow All Doc Types");
			this.AllowAllDocTypesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllowAllDocTypesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 10, true);
			this.AllowAllDocTypesCheckBox.Name = "AllowAllDocTypesCheckBox";
			this.AllowAllDocTypesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 17, true);
			this.AllowAllDocTypesCheckBox.TabIndex = 1;
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, "CurrentDocTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RefDocTypeEntry)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.RefDocTypeEntry)(null)).RefDocTypePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RefDocTypeEntry)(null)).RefDocTypeCollection)));
			this.Grid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "RefDocTypeCollection";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WebEDocsDownloadRegistryControl|b1f3efe9-c80b-42e4-9776-25bb43d9d898", "Doc Type");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "RefDocTypePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefDocType;
			//
			//zTextBoxColumnStyleInfo1
			//
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WebEDocsDownloadRegistryControl|165563d8-00f3-4181-b173-b50748ae82e6", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "RefDocTypeName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.GridId = "93776585-cf40-482b-8f1a-2b31377d8b7b";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "zGrid1";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 35, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 300, true);
			this.Grid.TabIndex = 2;
			// 
			// WebEDocsDownloadRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ModulesComboBox);
			this.Controls.Add(this.AllowAllDocTypesCheckBox);
			this.Controls.Add(this.Grid);
			this.Name = "WebEDocsDownloadRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 353, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.Grid.ResumeLayout(false);
			this.Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZGrid Grid;
		internal CargoWise.Windows.UI.KComboBox ModulesComboBox;
		internal ZArchitecture.GUI.ZCheckBox AllowAllDocTypesCheckBox;
	}
}
