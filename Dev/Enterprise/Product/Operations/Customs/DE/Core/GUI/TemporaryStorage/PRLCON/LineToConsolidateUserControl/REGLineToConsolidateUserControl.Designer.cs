namespace Enterprise.Customs.DE.GUI
{
	partial class REGLineToConsolidateUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxWithSelectedEventColumnStyleInfo zCodeFindBoxWithSelectedEventColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxWithSelectedEventColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LineDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AtbNumberZCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxWithSelectedEvent();
			this.PackageQtyZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LineNoZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			this.LineDetailsGroupBox.SuspendLayout();
			this.AtbNumberZCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec);
			// 
			// LinesGrid
			// 
			this.LinesGrid.AllowNavigation = false;
			this.LinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LinesGrid, "CusTempStorageLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_ReferenceNumberLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).FormattedReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).Lookups.CusTempStorageRegLineCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_PackageQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_CustomsStatus)));
			this.LinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TSL_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TSL_ReferenceNumberLine";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zCodeFindBoxWithSelectedEventColumnStyleInfo1.BindToList = "Lookups.CusTempStorageRegLineCollection";
			zCodeFindBoxWithSelectedEventColumnStyleInfo1.ColumnName = "FormattedReferenceNumber";
			zCodeFindBoxWithSelectedEventColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EU.DE.ImportFromSumARegister;
			zCodeFindBoxWithSelectedEventColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(187);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TSL_PackageQty";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zTextBoxColumnStyleInfo1.ColumnName = "TSL_CustomsStatus";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zCodeFindBoxWithSelectedEventColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.GridId = "65f1cbfd-aaf0-4c42-9799-3a85507997fd";
			this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinesGrid.LayoutKey = "LinesGrid";
			this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesGrid.Name = "LinesGrid";
			this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 116, true);
			this.LinesGrid.TabIndex = 1;
			// 
			// LineDetailsGroupBox
			// 
			this.LineDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("53709158-d263-4d23-805d-5719b1ee366a", "Line Details");
			this.LineDetailsGroupBox.Controls.Add(this.AtbNumberZCodeFindBox);
			this.LineDetailsGroupBox.Controls.Add(this.PackageQtyZCalcEdit);
			this.LineDetailsGroupBox.Controls.Add(this.LineNoZTextBox);
			this.LineDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LineDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 121, true);
			this.LineDetailsGroupBox.Name = "LineDetailsGroupBox";
			this.LineDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 47, true);
			this.LineDetailsGroupBox.TabIndex = 101;
			this.LineDetailsGroupBox.TabStop = false;
			// 
			// AtbNumberZCodeFindBox
			// 
			this.AtbNumberZCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AtbNumberZCodeFindBox, "CusTempStorageLines.FormattedReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).FormattedReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).Lookups.CusTempStorageRegLineCollection)));
			this.AtbNumberZCodeFindBox.BindToList = "Lookups.CusTempStorageRegLineCollection";
			this.AtbNumberZCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 17, true);
			this.AtbNumberZCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EU.DE.ImportFromSumARegister;
			this.AtbNumberZCodeFindBox.Name = "AtbNumberZCodeFindBox";
			this.AtbNumberZCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AtbNumberZCodeFindBox.ParentType = null;
			this.AtbNumberZCodeFindBox.PreBoundMaxLength = 18;
			this.AtbNumberZCodeFindBox.ShouldResize = false;
			this.AtbNumberZCodeFindBox.ShowDescriptionBox = false;
			this.AtbNumberZCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.AtbNumberZCodeFindBox.TabIndex = 2;
			this.AtbNumberZCodeFindBox.Selected += new Enterprise.ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventHandler(this.ZCodeFindBoxWithSelectedEvent_Selected);
			// 
			// PackageQtyZCalcEdit
			// 
			this.PackageQtyZCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageQtyZCalcEdit, "CusTempStorageLines.TSL_PackageQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_PackageQty)));
			this.PackageQtyZCalcEdit.CaptionResourceString = null;
			this.PackageQtyZCalcEdit.DecimalPlaces = 2;
			this.PackageQtyZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 17, true);
			this.PackageQtyZCalcEdit.Name = "PackageQtyZCalcEdit";
			this.PackageQtyZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.PackageQtyZCalcEdit.TabIndex = 3;
			this.PackageQtyZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LineNoZTextBox
			// 
			this.BindingSource.SetBindingMember(this.LineNoZTextBox, "CusTempStorageLines.TSL_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_LineNo)));
			this.LineNoZTextBox.CaptionResourceString = null;
			this.LineNoZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 17, true);
			this.LineNoZTextBox.Name = "LineNoZTextBox";
			this.LineNoZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.LineNoZTextBox.TabIndex = 1;
			// 
			// REGLineToConsolidateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LineDetailsGroupBox);
			this.Controls.Add(this.LinesGrid);
			this.Name = "REGLineToConsolidateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 168, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			this.LineDetailsGroupBox.ResumeLayout(false);
			this.LineDetailsGroupBox.PerformLayout();
			this.AtbNumberZCodeFindBox.ResumeLayout(true);
			this.AtbNumberZCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid LinesGrid;
		ZArchitecture.GUI.ZGroupBox LineDetailsGroupBox;
		ZArchitecture.ZTextBox LineNoZTextBox;
		ZArchitecture.ZCalcEdit PackageQtyZCalcEdit;
		internal ZArchitecture.GUI.ZCodeFindBoxWithSelectedEvent AtbNumberZCodeFindBox;
	}
}
