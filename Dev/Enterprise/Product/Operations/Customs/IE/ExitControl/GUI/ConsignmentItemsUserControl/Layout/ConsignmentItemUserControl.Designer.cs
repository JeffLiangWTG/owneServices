namespace Enterprise.Customs.IE.ExitControl.GUI
{
	partial class ConsignmentItemUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.MainKSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PackingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackingDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AuthorisationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AuthorisationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainKSplitContainer)).BeginInit();
			this.MainKSplitContainer.Panel1.SuspendLayout();
			this.MainKSplitContainer.Panel2.SuspendLayout();
			this.MainKSplitContainer.SuspendLayout();
			this.PackingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).BeginInit();
			this.PackingDetailsGrid.SuspendLayout();
			this.AuthorisationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).BeginInit();
			this.AuthorisationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentItemCollection<Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem>);
			// 
			// MainKSplitContainer
			// 
			this.MainKSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainKSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainKSplitContainer.Name = "MainKSplitContainer";
			this.MainKSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainKSplitContainer.Panel1
			// 
			this.MainKSplitContainer.Panel1.Controls.Add(this.PackingDetailsGroupBox);
			// 
			// MainKSplitContainer.Panel2
			// 
			this.MainKSplitContainer.Panel2.Controls.Add(this.AuthorisationsGroupBox);
			this.MainKSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 340, true);
			this.MainKSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(144);
			this.MainKSplitContainer.SplitterWidth = 6;
			this.MainKSplitContainer.TabIndex = 0;
			// 
			// PackingDetailsGroupBox
			// 
			this.PackingDetailsGroupBox.CaptionResourceString = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("62724DFA-1A33-41DB-8F04-6CFAFE92C4B6", "Packing Details");
			this.PackingDetailsGroupBox.Controls.Add(this.PackingDetailsGrid);
			this.PackingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackingDetailsGroupBox.Name = "PackingDetailsGroupBox";
			this.PackingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 144, true);
			this.PackingDetailsGroupBox.TabIndex = 0;
			this.PackingDetailsGroupBox.TabStop = false;
			// 
			// PackingDetailsGrid
			// 
			this.PackingDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackingDetailsGrid, "CusExitConsignmentPackagePivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusExitConsignmentPackagePivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusExitConsignmentPackagePivots)).SyncRoot)).Package.CXP_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusExitConsignmentPackagePivots)).SyncRoot)).Package.CXP_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusExitConsignmentPackagePivots)).SyncRoot)).Package.CXP_PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusExitConsignmentPackagePivots)).SyncRoot)).Package.CXP_MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusExitConsignmentPackagePivots)).SyncRoot)).CNP_CXN_Container)));
			this.PackingDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Package+CXP_Sequence";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Package+CXP_Quantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "Package+CXP_PackageType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "Package+CXP_MarksAndNumbers";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo1.ColumnName = "CNP_CXN_Container";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			this.PackingDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackingDetailsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.PackingDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGrid.GridId = "B28630A4-E6C7-4123-A66E-5A0302298184";
			this.PackingDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingDetailsGrid.LayoutKey = "PackingDetailsGrid";
			this.PackingDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackingDetailsGrid.Name = "PackingDetailsGrid";
			this.PackingDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 125, true);
			this.PackingDetailsGrid.TabIndex = 0;
			// 
			// AuthorisationsGroupBox
			// 
			this.AuthorisationsGroupBox.CaptionResourceString = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("46013E88-E6BB-417C-8688-EEB8E821676F", "Authorizations");
			this.AuthorisationsGroupBox.Controls.Add(this.AuthorisationsGrid);
			this.AuthorisationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorisationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AuthorisationsGroupBox.Name = "AuthorisationsGroupBox";
			this.AuthorisationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 190, true);
			this.AuthorisationsGroupBox.TabIndex = 0;
			this.AuthorisationsGroupBox.TabStop = false;
			// 
			// AuthorisationsGrid
			// 
			this.AuthorisationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AuthorisationsGrid, "CusAuthorizationUsages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusAuthorizationUsages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusAuthorizationUsages)).SyncRoot)).AGC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusAuthorizationUsages)).SyncRoot)).AGC_Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusAuthorizationUsages)).SyncRoot)).AGC_OH_Owner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem)(null)).CusAuthorizationUsages)).SyncRoot)).Lookups.Owners)));
			this.AuthorisationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "AGC_Code";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AGC_Number";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.CusAuthorisations;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+Owners";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AGC_OH_Owner";
			zOrganisationFindBoxColumnStyleInfo1.IsMandatory = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AuthorisationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AuthorisationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.AuthorisationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorisationsGrid.GridId = "97A99D3F-CF8B-4D3D-942F-C259AAC0A706";
			this.AuthorisationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AuthorisationsGrid.LayoutKey = "AuthorisationsGrid";
			this.AuthorisationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AuthorisationsGrid.Name = "AuthorisationsGrid";
			this.AuthorisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 171, true);
			this.AuthorisationsGrid.TabIndex = 0;
			// 
			// ConsignmentItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainKSplitContainer);
			this.Name = "ConsignmentItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 340, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainKSplitContainer.Panel1.ResumeLayout(false);
			this.MainKSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainKSplitContainer)).EndInit();
			this.MainKSplitContainer.ResumeLayout(false);
			this.MainKSplitContainer.PerformLayout();
			this.PackingDetailsGroupBox.ResumeLayout(false);
			this.PackingDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).EndInit();
			this.PackingDetailsGrid.ResumeLayout(false);
			this.PackingDetailsGrid.PerformLayout();
			this.AuthorisationsGroupBox.ResumeLayout(false);
			this.AuthorisationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).EndInit();
			this.AuthorisationsGrid.ResumeLayout(false);
			this.AuthorisationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZGrid PackingDetailsGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox PackingDetailsGroupBox;
		CargoWise.Windows.UI.KSplitContainer MainKSplitContainer;
		Enterprise.ZArchitecture.ZGrid AuthorisationsGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox AuthorisationsGroupBox;
	}
}
