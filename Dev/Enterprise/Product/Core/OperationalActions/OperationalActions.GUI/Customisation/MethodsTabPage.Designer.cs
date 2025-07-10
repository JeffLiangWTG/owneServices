namespace Enterprise.Services.OperationalActions.GUI
{
	partial class MethodsTabPage
	{
		private void InitializeComponent()
		{
			CargoWise.Windows.UI.KSplitContainer methodSettingsSplitter;
			Enterprise.ZArchitecture.ZGrid actionMethodGrid;
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGroupBox settingsGroupBox;
			Enterprise.Services.OperationalActions.GUI.SettingsHostControl settingsHost;
			methodSettingsSplitter = new CargoWise.Windows.UI.KSplitContainer();
			actionMethodGrid = new Enterprise.ZArchitecture.ZGrid();
			settingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			settingsHost = new Enterprise.Services.OperationalActions.GUI.SettingsHostControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			methodSettingsSplitter.Panel1.SuspendLayout();
			methodSettingsSplitter.Panel2.SuspendLayout();
			methodSettingsSplitter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(actionMethodGrid)).BeginInit();
			settingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalAction);
			// 
			// methodSettingsSplitter
			// 
			methodSettingsSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
			methodSettingsSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			methodSettingsSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			methodSettingsSplitter.Name = "methodSettingsSplitter";
			// 
			// methodSettingsSplitter.Panel1
			// 
			methodSettingsSplitter.Panel1.Controls.Add(actionMethodGrid);
			// 
			// methodSettingsSplitter.Panel2
			// 
			methodSettingsSplitter.Panel2.Controls.Add(settingsGroupBox);
			methodSettingsSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 170, true);
			methodSettingsSplitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(507);
			methodSettingsSplitter.TabIndex = 4;
			// 
			// actionMethodGrid
			// 
			actionMethodGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(actionMethodGrid, "MethodDescriptors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).MethodDescriptors)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Services.OperationalActions.Business.OperationalActionMethodDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).MethodDescriptors)).SyncRoot)).MethodGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Services.OperationalActions.Business.OperationalActionMethodDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).MethodDescriptors)).SyncRoot)).MethodID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Services.OperationalActions.Business.OperationalActionMethodDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).MethodDescriptors)).SyncRoot)).Order)));
			actionMethodGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "MethodGroup";
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo2.ColumnName = "MethodID";
			zGuidDropEditColumnStyleInfo2.IsMandatory = true;
			zGuidDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Order";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			actionMethodGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			actionMethodGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			actionMethodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			actionMethodGrid.GridId = "48e2be74-edbf-4801-8969-fda7b1b931ff";
			actionMethodGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			actionMethodGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			actionMethodGrid.LayoutKey = "documentsGrid";
			actionMethodGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			actionMethodGrid.Name = "actionMethodGrid";
			actionMethodGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 170, true);
			actionMethodGrid.TabIndex = 3;
			// 
			// settingsGroupBox
			// 
			settingsGroupBox.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("MethodsTabPage|a63be53a-f9d3-4055-bf97-d0cc337f59d2", "Process Settings");
			settingsGroupBox.Controls.Add(settingsHost);
			settingsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			settingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			settingsGroupBox.Name = "settingsGroupBox";
			settingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 170, true);
			settingsGroupBox.TabIndex = 5;
			settingsGroupBox.TabStop = false;
			// 
			// settingsHost
			// 
			this.BindingSource.SetBindingMember(settingsHost, "MethodDescriptors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Services.OperationalActions.Business.OperationalActionMethodDescriptor)(((Enterprise.Services.OperationalActions.Business.OperationalActionMethodDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).MethodDescriptors)).SyncRoot)))));
			settingsHost.Dock = System.Windows.Forms.DockStyle.Fill;
			settingsHost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			settingsHost.Name = "settingsHost";
			settingsHost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 151, true);
			settingsHost.TabIndex = 4;
			// 
			// MethodsTabPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(methodSettingsSplitter);
			this.Name = "MethodsTabPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			methodSettingsSplitter.Panel1.ResumeLayout(false);
			methodSettingsSplitter.Panel2.ResumeLayout(false);
			methodSettingsSplitter.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(actionMethodGrid)).EndInit();
			settingsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}
}
