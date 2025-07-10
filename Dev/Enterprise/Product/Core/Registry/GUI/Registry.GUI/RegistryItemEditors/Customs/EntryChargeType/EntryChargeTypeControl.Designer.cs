using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI.Customs
{
	partial class EntryChargeTypeControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.EntryChargeTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryChargeTypesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Customs.EntryChargeTypeSetting);
			// 
			// EntryChargeTypesGrid
			// 
			this.EntryChargeTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryChargeTypesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Customs.EntryChargeTypeSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Customs.EntryChargeTypeSetting)(null)).ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Customs.EntryChargeTypeSetting)(null)).ChargeType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Customs.EntryChargeTypeSetting)(null)).ChargeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.Customs.EntryChargeTypeSetting)(null)).AC_ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Customs.EntryChargeTypeSetting)(null)).ChargeCode_List)));
			this.EntryChargeTypesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "ChargeType_List";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EntryChargeTypeControl|498eee29-6b42-4d57-89b7-f3c5023d8a3e", "Charge Type");
			zDropEditColumnStyleInfo1.ColumnName = "ChargeType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EntryChargeTypeControl|ab403110-1930-4d08-9e70-4115feb88a54", "Charge Type Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.BindToList = "ChargeCode_List";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EntryChargeTypeControl|0870869a-d0ce-4588-96f6-2df2089a1fc1", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AC_ChargeCode";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCodeForRegistry;
			this.EntryChargeTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryChargeTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryChargeTypesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.EntryChargeTypesGrid.GridId = "81d418b9-a109-4b10-b7a9-067c82fb835e";
			this.EntryChargeTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryChargeTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryChargeTypesGrid.LayoutKey = "EntryChargeTypesGrid";
			this.EntryChargeTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryChargeTypesGrid.Name = "EntryChargeTypesGrid";
			this.EntryChargeTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.EntryChargeTypesGrid.TabIndex = 0;
			// 
			// EntryChargeTypeControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryChargeTypesGrid);
			this.Name = "EntryChargeTypeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryChargeTypesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected internal Enterprise.ZArchitecture.ZGrid EntryChargeTypesGrid;

	}
}
