namespace Enterprise.Customs.CA.GUI
{
	partial class LVSLineCasualImportUserControl
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
		private void InitializeComponent()
		{
			this.CasualImportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsCasualImportCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CasualImportDestinationProvinceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CasualImportCommodityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsExemptCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CasualImportGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobComInvoiceLine);
			// 
			// CasualImportGroupBox
			// 
			this.CasualImportGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.CasualImportGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSLineCasualImportUserControl|09faa014-eaac-4878-b374-1d5e77810039", "Casual Import");
			this.CasualImportGroupBox.Controls.Add(this.IsCasualImportCheckBox);
			this.CasualImportGroupBox.Controls.Add(this.CasualImportDestinationProvinceDropEdit);
			this.CasualImportGroupBox.Controls.Add(this.CasualImportCommodityDropEdit);
			this.CasualImportGroupBox.Controls.Add(this.IsExemptCheckBox);
			this.CasualImportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 3, true);
			this.CasualImportGroupBox.Name = "CasualImportGroupBox";
			this.CasualImportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 100, true);
			this.CasualImportGroupBox.TabIndex = 8;
			this.CasualImportGroupBox.TabStop = false;
			// 
			// IsCasualImportCheckBox
			// 
			this.IsCasualImportCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsCasualImportCheckBox, "CA_IsCasualImport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_IsCasualImport)));
			this.IsCasualImportCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSLineCasualImportUserControl|1fa88081-29ae-4f4e-bec1-cf76da8a6ee2", "Casual Import", "Casual Import Ind", "Casual Import Indicator", "Indicator which identifies whether or not this is a casual import.");
			this.IsCasualImportCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsCasualImportCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 20, true);
			this.IsCasualImportCheckBox.Name = "IsCasualImportCheckBox";
			this.IsCasualImportCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsCasualImportCheckBox.TabIndex = 9;
			this.IsCasualImportCheckBox.UseVisualStyleBackColor = true;
			// 
			// CasualImportDestinationProvinceDropEdit
			// 
			this.CasualImportDestinationProvinceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CasualImportDestinationProvinceDropEdit, "CA_CasualImportDestinationProvince");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_CasualImportDestinationProvince)));
			this.CasualImportDestinationProvinceDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSLineCasualImportUserControl|2d7d7ec4-6b15-45ce-8a9c-5ae7cbe02e5f", "Dest. Province", "Casual Import Destination Province", "Province that represents the destination within Canada of the casual import.");
			this.CasualImportDestinationProvinceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 40, true);
			this.CasualImportDestinationProvinceDropEdit.Name = "CasualImportDestinationProvinceDropEdit";
			this.CasualImportDestinationProvinceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CasualImportDestinationProvinceDropEdit.TabIndex = 10;
			// 
			// CasualImportCommodityDropEdit
			// 
			this.CasualImportCommodityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CasualImportCommodityDropEdit, "CA_CasualImportCommodity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_CasualImportCommodity)));
			this.CasualImportCommodityDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSLineCasualImportUserControl|d59c87fb-ff32-4682-98bb-eb82c7e32606", "Commodity", "Casual Import Commodity", "The commodity of the casual import.");
			this.CasualImportCommodityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 66, true);
			this.CasualImportCommodityDropEdit.Name = "CasualImportCommodityDropEdit";
			this.CasualImportCommodityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CasualImportCommodityDropEdit.TabIndex = 10;
			// 
			// IsExemptCheckBox
			// 
			this.IsExemptCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsExemptCheckBox, "CA_IsExempt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_IsExempt)));
			this.IsExemptCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSLineCasualImportUserControl|77cdc8db-1bff-49f8-a126-9acde98d0c08", "Exempt", "Exempt Ind", "Exempt Indicator", "Indicator which identifies whether or not this is an exempt commodity.");
			this.IsExemptCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsExemptCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 92, true);
			this.IsExemptCheckBox.Name = "IsExemptCheckBox";
			this.IsExemptCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsExemptCheckBox.TabIndex = 11;
			this.IsExemptCheckBox.UseVisualStyleBackColor = true;
			// 
			// LVSLineCasualImportUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CasualImportGroupBox);
			this.Name = "LVSLineCasualImportUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 104, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CasualImportGroupBox.ResumeLayout(false);
			this.CasualImportGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox CasualImportGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsCasualImportCheckBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CasualImportDestinationProvinceDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CasualImportCommodityDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsExemptCheckBox;
	}
}
