namespace Enterprise.Registry.GUI
{
	public partial class LocationsChargesControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZLabel LocationsLabel;
		internal Enterprise.ZArchitecture.ZLabel ChargeCodesLabel;
		internal Enterprise.ZArchitecture.ZGrid LocationsGrid;
		internal Enterprise.ZArchitecture.ZGrid ChargesGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ChargeCodesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LocationsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LocationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LocationsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.LocationsChargesGroup);
			// 
			// ChargeCodesLabel
			// 
			this.ChargeCodesLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LocationsChargesControl|9793d5be-2283-4518-8df9-41bde216de3d", "Charge Codes");
			this.ChargeCodesLabel.IsFontBold = true;
			this.ChargeCodesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.ChargeCodesLabel.Name = "ChargeCodesLabel";
			this.ChargeCodesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.ChargeCodesLabel.TabIndex = 9;
			// 
			// LocationsLabel
			// 
			this.LocationsLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LocationsChargesControl|a5a4bf84-767b-4b6c-b132-6f4887dffecb", "UNLOCO");
			this.LocationsLabel.IsFontBold = true;
			this.LocationsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LocationsLabel.Name = "LocationsLabel";
			this.LocationsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 23, true);
			this.LocationsLabel.TabIndex = 8;
			// 
			// ChargesGrid
			// 
			this.ChargesGrid.AllowNavigation = false;
			this.ChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChargesGrid, "Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LocationsChargesGroup)(null)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.ChargeCodeGroup)(((System.Collections.IList)(((Enterprise.Registry.Business.LocationsChargesGroup)(null)).Charges)).SyncRoot)).ChargeCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeCodeGroup)(((System.Collections.IList)(((Enterprise.Registry.Business.LocationsChargesGroup)(null)).Charges)).SyncRoot)).ChargeCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeCodeGroup)(((System.Collections.IList)(((Enterprise.Registry.Business.LocationsChargesGroup)(null)).Charges)).SyncRoot)).ChargeCodeDescription)));
			this.ChargesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "ChargeCodeList";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LocationsChargesControl|272bafdd-1e83-4812-98b2-fddcd0aff01c", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCodePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCodeForRegistry;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LocationsChargesControl|e66e5f91-3fc9-4423-891a-525b9ef0644f", "Charge Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeDescription";
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargesGrid.GridId = "070ad431-21c7-4e5f-8b7b-4e7ee383ca3f";
			this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargesGrid.LayoutKey = "ChargeCodesGrid";
			this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.ChargesGrid.Name = "ChargesGrid";
			this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 168, true);
			this.ChargesGrid.TabIndex = 6;
			// 
			// LocationsGrid
			// 
			this.LocationsGrid.AllowNavigation = false;
			this.LocationsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LocationsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LocationsChargesGroup)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LocationsChargesGroup)(null)).Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LocationsChargesGroup)(null)).LocationList)));
			this.LocationsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "LocationList";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LocationsChargesControl|d5118355-253c-4447-903c-16520be7a246", "Location");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Location";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.LocationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.LocationsGrid.GridId = "5b21910e-cfd4-492d-81f8-e3cc336eb3a3";
			this.LocationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LocationsGrid.LayoutKey = "LocationsGrid";
			this.LocationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.LocationsGrid.Name = "LocationsGrid";
			this.LocationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 128, true);
			this.LocationsGrid.TabIndex = 5;
			// 
			// LocationsChargesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChargesGrid);
			this.Controls.Add(this.LocationsLabel);
			this.Controls.Add(this.ChargeCodesLabel);
			this.Controls.Add(this.LocationsGrid);
			this.Name = "LocationsChargesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 344, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LocationsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
