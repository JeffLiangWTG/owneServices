namespace Enterprise.Customs.IE.GUI
{
	public partial class ImportInvoiceLineDetailsUserControl
	{
		void InitializeComponent()
		{
			this.CountryOfSupplyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryOfSupplyCodeFindBox.SuspendLayout();
			this.CountryOfOriginCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine);
			// 
			// CountryOfSupplyCodeFindBox
			// 
			this.CountryOfSupplyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfSupplyCodeFindBox, "ZG_CountryOfSupply");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).ZG_CountryOfSupply)));
			this.CountryOfSupplyCodeFindBox.BindToList = null;
			this.CountryOfSupplyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 175, true);
			this.CountryOfSupplyCodeFindBox.Name = "CountryOfSupplyCodeFindBox";
			this.CountryOfSupplyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryOfSupplyCodeFindBox.ParentType = null;
			this.CountryOfSupplyCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfSupplyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
			this.CountryOfSupplyCodeFindBox.TabIndex = 0;
			// 
			// CountryOfOriginCodeFindBox
			// 
			this.CountryOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfOriginCodeFindBox, "JI_CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).JI_CountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).Lookups.CountryList)));
			this.CountryOfOriginCodeFindBox.BindToList = "Lookups+CountryList";
			this.CountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 108, true);
			this.CountryOfOriginCodeFindBox.Name = "CountryOfOriginCodeFindBox";
			this.CountryOfOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryOfOriginCodeFindBox.ParentType = null;
			this.CountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
			this.CountryOfOriginCodeFindBox.TabIndex = 4;
			// 
			// ImportInvoiceLineDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CountryOfSupplyCodeFindBox);
			this.Controls.Add(this.CountryOfOriginCodeFindBox);
			this.Name = "ImportInvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 525, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryOfSupplyCodeFindBox.ResumeLayout(true);
			this.CountryOfSupplyCodeFindBox.PerformLayout();
			this.CountryOfOriginCodeFindBox.ResumeLayout(true);
			this.CountryOfOriginCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryOfSupplyCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryOfOriginCodeFindBox;
	}
}
