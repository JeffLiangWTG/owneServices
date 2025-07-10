using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class NctsOrganisationPluginUserControl
	{
		ZGroupBox guaranteesGroup;
		ZGrid grid;

		void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo officeCodeColumn = new ZCodeFindBoxColumnStyleInfo();
			this.guaranteesGroup = new ZGroupBox();
			this.grid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.guaranteesGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.OrgHeaderWrapper);
			// 
			// guaranteesGroup
			// 
			this.guaranteesGroup.Controls.Add(this.grid);
			this.guaranteesGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.guaranteesGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.guaranteesGroup.Name = "guaranteesGroup";
			this.guaranteesGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 174, true);
			this.guaranteesGroup.TabIndex = 0;
			this.guaranteesGroup.TabStop = false;
			this.guaranteesGroup.Text = "Guarantees";
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid, "BondDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.OrgHeaderWrapper)(null)).BondDetails);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.NctsGuarantee)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.NctsGuarantee)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.NctsGuarantee)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondAmount);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.NctsGuarantee)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_Password);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.NctsGuarantee)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondFiledPort);
			this.grid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("e088dbc0-254e-4c4d-80ec-20b4e33e4302", "Guarantee Type");
			zDropEditColumnStyleInfo1.ColumnName = "PW_BondType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("39bcc8b9-5c5d-494d-9b58-d08143d8c91b", "Guarantee Reference");
			zTextBoxColumnStyleInfo1.ColumnName = "PW_BondNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("6432ab3b-e6c0-437d-8f47-0f4dbad7ef0e", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "PW_BondAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("4e55d88f-aa52-4356-b02c-3c99301f85d4", "Access Code", "PIN, Password or Access Code");
			zTextBoxColumnStyleInfo2.ColumnName = "PW_Password";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			officeCodeColumn.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("aa9466d2-30b1-42de-ac17-0bca81488e0c", "Office Code");
			officeCodeColumn.ColumnName = "PW_BondFiledPort";
			officeCodeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(officeCodeColumn);
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.GridId = "cf7ba71c-24a4-4bcd-ad5f-ca03875a346c";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "grid";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.grid.Name = "grid";
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 157, true);
			this.grid.TabIndex = 0;
			// 
			// NctsOrganisationPluginUserControl
			// 
			this.Controls.Add(this.guaranteesGroup);
			this.Name = "NctsOrganisationPluginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 174, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.guaranteesGroup.ResumeLayout(false);
			this.guaranteesGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
