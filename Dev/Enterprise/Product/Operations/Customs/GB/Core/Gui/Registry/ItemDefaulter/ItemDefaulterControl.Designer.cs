
namespace Enterprise.Customs.GB.GUI.Registry
{
	partial class ItemDefaulterControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.TaxCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxCodesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Registry.ItemDefaulterSetting);
			// 
			// TaxCodesGrid
			// 
			this.TaxCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxCodesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Registry.ItemDefaulterSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.ItemDefaulterSetting)(null)).SourceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.ItemDefaulterSetting)(null)).SourceValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.ItemDefaulterSetting)(null)).TargetType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.ItemDefaulterSetting)(null)).TargetCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Registry.ItemDefaulterSetting)(null)).TargetOrgAddress)));
			this.TaxCodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("cb3ce065-6890-457c-94a0-4380d3decf4a", "Source Type", "When adding a...", "");
			zDropEditColumnStyleInfo1.ColumnName = "SourceType";
			zDropEditColumnStyleInfo1.GroupName = null;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8275ccf5-482d-4d2c-8690-796e749bf755", "Source Value", "... with value matching....", "");
			zTextBoxColumnStyleInfo1.ColumnName = "SourceValue";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("2dc01a52-7b5b-4625-9a05-a24e3b6e00c8", "Target Type", "Then set/add a ....", "");
			zDropEditColumnStyleInfo2.ColumnName = "TargetType";
			zDropEditColumnStyleInfo2.GroupName = null;
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("fd445bf4-1a56-4c29-88f5-225f4c6efcde", "Target Code", "... with this value/type...", "");
			zTextBoxColumnStyleInfo2.ColumnName = "TargetCode";
			zTextBoxColumnStyleInfo2.GroupName = null;
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("11ef2b5e-d27a-4d15-b8ec-7819ac893c9c", "Target Address", "... or this SPOFF", "");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TargetOrgAddress";
			zGuidFindBoxColumnStyleInfo1.GroupName = null;
			this.TaxCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TaxCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TaxCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TaxCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TaxCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TaxCodesGrid.CopySelectedRowsAllowed = true;
			this.TaxCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxCodesGrid.GridId = "c3e6dec3-9fd8-41e6-8def-c25aab6efd60";
			this.TaxCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxCodesGrid.LayoutKey = "TaxCodesGrid";
			this.TaxCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxCodesGrid.Name = "TaxCodesGrid";
			this.TaxCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.TaxCodesGrid.TabIndex = 0;
			// 
			// ItemDefaulterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TaxCodesGrid);
			this.Name = "ItemDefaulterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxCodesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public Enterprise.ZArchitecture.ZGrid TaxCodesGrid;

	}
}
