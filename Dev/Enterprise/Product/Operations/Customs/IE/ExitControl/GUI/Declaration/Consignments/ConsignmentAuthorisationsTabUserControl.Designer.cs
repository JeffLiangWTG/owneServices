namespace Enterprise.Customs.IE.ExitControl.GUI
{
	partial class ConsignmentAuthorisationsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.AuthorisationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).BeginInit();
			this.AuthorisationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.ICusAuthorizationUsageCollection<Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage, Enterprise.Customs.IE.ExitControl.Business.CusExitConsignment>);
			// 
			// AuthorisationsGrid
			// 
			this.AuthorisationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AuthorisationsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage)(null)).AGC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage)(null)).AGC_Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage)(null)).AGC_OH_Owner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage)(null)).Lookups.Owners)));
			this.AuthorisationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "AGC_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AGC_Number";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.CusAuthorisations;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+Owners";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AGC_OH_Owner";
			zOrganisationFindBoxColumnStyleInfo1.IsMandatory = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AuthorisationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.AuthorisationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorisationsGrid.GridId = "97A99D3F-CF8B-4D3D-942F-C259AAC0A706";
			this.AuthorisationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AuthorisationsGrid.LayoutKey = "AuthorisationsGrid";
			this.AuthorisationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AuthorisationsGrid.Name = "AuthorisationsGrid";
			this.AuthorisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
			this.AuthorisationsGrid.TabIndex = 0;
			// 
			// ConsignmentAuthorisationsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AuthorisationsGrid);
			this.Name = "ConsignmentAuthorisationsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).EndInit();
			this.AuthorisationsGrid.ResumeLayout(false);
			this.AuthorisationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZGrid AuthorisationsGrid;
	}
}
