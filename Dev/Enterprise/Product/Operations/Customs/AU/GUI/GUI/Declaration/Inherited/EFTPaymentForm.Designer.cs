using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using System.Windows.Forms;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class EFTPaymentForm
	{
		protected override void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.oKBoundButton = new ZButton();
			this.cancelBoundButton = new ZButton();
			this.eFTPaymentInfosGroupBox = new ZGroupBox();
			this.gridPanel = new ZPanel();
			this.eFTPaymentInformationGrid = new ZArchitecture.ZGrid();
			this.scheduledPaymentDate = new ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.eFTPaymentInfosGroupBox.SuspendLayout();
			this.gridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.eFTPaymentInformationGrid)).BeginInit();
			this.eFTPaymentInformationGrid.SuspendLayout();
			this.scheduledPaymentDate.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(241);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EFTPaymentInformationCollection);
			// 
			// oKBoundButton
			// 
			this.oKBoundButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oKBoundButton.IsCaptionOverridden = true;
			this.oKBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 200, true);
			this.oKBoundButton.Name = "oKBoundButton";
			this.oKBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 23, true);
			this.oKBoundButton.TabIndex = 53;
			this.oKBoundButton.Text = "Send";
			this.oKBoundButton.ToolTipCaption = null;
			this.oKBoundButton.Click += new System.EventHandler(this.OKBoundButton_Click);
			// 
			// cancelBoundButton
			// 
			this.cancelBoundButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBoundButton.IsCaptionOverridden = true;
			this.cancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 200, true);
			this.cancelBoundButton.Name = "cancelBoundButton";
			this.cancelBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			this.cancelBoundButton.TabIndex = 54;
			this.cancelBoundButton.Text = "Cancel";
			this.cancelBoundButton.ToolTipCaption = null;
			this.cancelBoundButton.Click += new System.EventHandler(this.CancelBoundButton_Click);
			// 
			// eFTPaymentInfosGroupBox
			// 
			this.eFTPaymentInfosGroupBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.eFTPaymentInfosGroupBox.Controls.Add(this.gridPanel);
			this.eFTPaymentInfosGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.eFTPaymentInfosGroupBox.Name = "eFTPaymentInfosGroupBox";
			this.eFTPaymentInfosGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 192, true);
			this.eFTPaymentInfosGroupBox.TabIndex = 56;
			this.eFTPaymentInfosGroupBox.TabStop = false;
			this.eFTPaymentInfosGroupBox.Text = "EFT Payment Information";
			// 
			// gridPanel
			// 
			this.gridPanel.Controls.Add(this.eFTPaymentInformationGrid);
			this.gridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.gridPanel.Name = "gridPanel";
			this.gridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 177, true);
			this.gridPanel.TabIndex = 56;
			// 
			// eFTPaymentInformationGrid
			// 
			this.eFTPaymentInformationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.eFTPaymentInformationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((EFTPaymentInformation)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EFTPaymentInformation)(null)).BGMReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EFTPaymentInformation)(null)).EntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EFTPaymentInformation)(null)).CustomsChargeAmountPayableNow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EFTPaymentInformation)(null)).AQISServicePaymentAmountPayableNow)));
			this.eFTPaymentInformationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Entry Reference No";
			zTextBoxColumnStyleInfo1.ColumnName = "BGMReferenceNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Entry No";
			zTextBoxColumnStyleInfo2.ColumnName = "EntryNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Customs Charge to Pay Now";
			zCalcEditColumnStyleInfo1.ColumnName = "CustomsChargeAmountPayableNow";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Quarantine Service to Pay Now";
			zCalcEditColumnStyleInfo2.ColumnName = "AQISServicePaymentAmountPayableNow";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.eFTPaymentInformationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.eFTPaymentInformationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.eFTPaymentInformationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.eFTPaymentInformationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.eFTPaymentInformationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eFTPaymentInformationGrid.GridId = "0fbd56aa-9afd-44a1-b8a4-246559eeaf7c";
			this.eFTPaymentInformationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.eFTPaymentInformationGrid.LayoutKey = "zGrid1";
			this.eFTPaymentInformationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eFTPaymentInformationGrid.Name = "eFTPaymentInformationGrid";
			this.eFTPaymentInformationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 177, true);
			this.eFTPaymentInformationGrid.TabIndex = 2;
			// 
			// scheduledPaymentDate
			// 
			this.scheduledPaymentDate.AllowDrop = true;
			this.scheduledPaymentDate.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.scheduledPaymentDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.scheduledPaymentDate, "ScheduledPaymentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((EFTPaymentInformation)(null)).ScheduledPaymentDate)));
			this.scheduledPaymentDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.scheduledPaymentDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 204, true);
			this.scheduledPaymentDate.Name = "scheduledPaymentDate";
			this.scheduledPaymentDate.TabIndex = 52;
			// 
			// EFTPaymentForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 254, true);
			this.Controls.Add(this.eFTPaymentInfosGroupBox);
			this.Controls.Add(this.oKBoundButton);
			this.Controls.Add(this.cancelBoundButton);
			this.Controls.Add(this.scheduledPaymentDate);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(EFTPaymentInformationCollection);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.EFTPaymentInformationCollection";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 288, true);
			this.Name = "EFTPaymentForm";
			this.Controls.SetChildIndex(this.scheduledPaymentDate, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.cancelBoundButton, 0);
			this.Controls.SetChildIndex(this.oKBoundButton, 0);
			this.Controls.SetChildIndex(this.eFTPaymentInfosGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.eFTPaymentInfosGroupBox.ResumeLayout(false);
			this.eFTPaymentInfosGroupBox.PerformLayout();
			this.gridPanel.ResumeLayout(false);
			this.gridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.eFTPaymentInformationGrid)).EndInit();
			this.eFTPaymentInformationGrid.ResumeLayout(false);
			this.eFTPaymentInformationGrid.PerformLayout();
			this.scheduledPaymentDate.ResumeLayout(true);
			this.scheduledPaymentDate.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZButton oKBoundButton;
		ZButton cancelBoundButton;
		ZGroupBox eFTPaymentInfosGroupBox;
		ZPanel gridPanel;
		ZArchitecture.ZGrid eFTPaymentInformationGrid;
		internal ZDateEdit scheduledPaymentDate;
	}
}
