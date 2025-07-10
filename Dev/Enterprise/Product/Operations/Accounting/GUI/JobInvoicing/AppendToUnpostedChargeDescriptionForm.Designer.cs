using System.ComponentModel;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AppendToUnpostedChargeDescriptionForm
	{


		#region Windows Form Designer generated code

		//private IContainer components;

		new void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new ZMultiLineTextBoxColumnInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new ZMultiLineTextBoxColumnInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.MainPanel = new ZPanel();
			this.ChargesGrid = new ZArchitecture.ZGrid();
			this.BottomPanel = new ZPanel();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			((ISupportInitialize)(this.ChargesGrid)).BeginInit();
			this.ChargesGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 193, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(410);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(410);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ChargeDescriptionOverrideAdaptor);
			// 
			// MainPanel
			// 
			this.MainPanel.AutoScroll = true;
			this.MainPanel.Controls.Add(this.ChargesGrid);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 149, true);
			this.MainPanel.TabIndex = 1;
			// 
			// ChargesGrid
			// 
			this.ChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargesGrid, "SelectedWrappedCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).OriginalDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).TextToAppend)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_RX_NKSellCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_OSSellAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_LocalSellAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_AT_SellGSTRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_OSSellGSTAmt_Calc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_InvoiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_RX_NKCostCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_OSCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_LocalCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_AT_CostGSTRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ChargeWithAppendedDescription)(((System.Collections.IList)(((ChargeDescriptionOverrideAdaptor)(null)).SelectedWrappedCharges)).SyncRoot)).JR_OSCostGSTAmt)));
			this.ChargesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JR_AC";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("07f91fca-0905-41e6-bf44-4e3b2a2c872f", "Original Description");
			zMultiLineTextBoxColumnInfo1.ColumnName = "OriginalDescription";
			zMultiLineTextBoxColumnInfo1.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("21668736-286f-4da0-95b2-c8e9d9fb2235", "Text to Append");
			zMultiLineTextBoxColumnInfo2.ColumnName = "TextToAppend";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo3.ColumnName = "JR_Desc";
			zMultiLineTextBoxColumnInfo3.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JR_JH";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "JR_GB";
			zGuidFindBoxColumnStyleInfo3.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "JR_GE";
			zGuidFindBoxColumnStyleInfo4.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "JR_RX_NKSellCurrency";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JR_OSSellAmt";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JR_LocalSellAmt";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "JR_AT_SellGSTRate";
			zGuidFindBoxColumnStyleInfo5.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JR_OSSellGSTAmt_Calc";
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d7e3d68f-f6fe-4715-9c9c-54aefc2231da", "Sell GST Amount");
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "JR_InvoiceType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6363ccbb-4dad-4709-a756-dc911bbacff9", "Charge Type");
			zTextBoxColumnStyleInfo3.ColumnName = "ChargeType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "JR_RX_NKCostCurrency";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JR_OSCostAmt";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JR_LocalCostAmt";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "JR_AT_CostGSTRate";
			zGuidFindBoxColumnStyleInfo6.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JR_OSCostGSTAmt";
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ChargesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.ChargesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ChargesGrid.CopySelectedRowsAllowed = true;
			this.ChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargesGrid.GridId = "1050E90A-9D37-467A-82E6-DEB6DE6C1495";
			this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargesGrid.LayoutKey = "ChargesGrid";
			this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargesGrid.Name = "ChargesGrid";
			this.ChargesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 149, true);
			this.ChargesGrid.TabIndex = 2;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 149, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 44, true);
			this.BottomPanel.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 3, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// AppendToUnpostedChargeDescriptionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("af025d6c-4b9a-4f5c-8b2d-120aa61c1800", "Append Charge Line Description ");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 217, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(ChargeDescriptionOverrideAdaptor);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 200, true);
			this.Name = "AppendToUnpostedChargeDescriptionForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((ISupportInitialize)(this.ChargesGrid)).EndInit();
			this.ChargesGrid.ResumeLayout(false);
			this.ChargesGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}