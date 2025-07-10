using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class EnquiryForm : CalloutForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			this.zGroupBox5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).BeginInit();
			this.zGroupBox6.SuspendLayout();
			this.FinanceTabPage.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// zGroupBox5
			// 
			this.zGroupBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 142, true);
			// 
			// LineChargesGrid
			// 
			this.LineChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 119, true);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).JR_DescInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).JR_Desc)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).Amount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).AmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).Discount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).DiscountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).GSTAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).GSTAmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).NettAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).NettAmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).NonTaxableAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).NonTaxableAmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).TaxableAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).TaxableAmountInfo)));
			// 
			// zGroupBox6
			// 
			this.zGroupBox6.Visible = false;
			// 
			// zLabel41
			// 
			this.zLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 16, true);
			// 
			// zLabel42
			// 
			this.zLabel42.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 61, true);
			// 
			// BisiUploadDateDateEdit
			// 
			this.BisiUploadDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 34, true);
			// 
			// BisiDownloadDateEdit
			// 
			this.BisiDownloadDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 78, true);
			// 
			// zGroupBox7
			// 
			this.zGroupBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 120, true);
			// 
			// FinanceTabPage
			// 
			this.FinanceTabPage.Text = "Enquiry";
			// 
			// IsExcludedFromBISIWarningBoundCheckBox
			// 
			this.IsExcludedFromBISIWarningBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 103, true);
			// 
			// BISIDownloadNotRequiredOrForcedCaptionTextBox
			// 
			this.BISIDownloadNotRequiredOrForcedCaptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 79, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 623, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 615, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(992);
			// 
			// EnquiryForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1130, 720, true);
			this.Name = "EnquiryForm";
			this.zGroupBox5.ResumeLayout(false);
			this.zGroupBox5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).EndInit();
			this.zGroupBox6.ResumeLayout(false);
			this.FinanceTabPage.ResumeLayout(false);
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
