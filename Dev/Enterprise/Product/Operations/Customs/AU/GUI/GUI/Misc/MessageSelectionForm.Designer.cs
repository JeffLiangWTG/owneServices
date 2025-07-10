using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class MessageSelectionForm
	{
		protected override void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.messageAttacheeCollectionGrid = new ZArchitecture.ZGrid();
			this.oKBoundButton = new ZButton();
			this.cancelBoundButton = new ZButton();
			this.label1 = new ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.messageAttacheeCollectionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(248);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(248);
			// 
			// MessageAttacheeCollectionGrid
			// 
			this.messageAttacheeCollectionGrid.AllowNavigation = false;
			this.messageAttacheeCollectionGrid.BindTo = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((MessageAttacheeSelectionCollection)(null)))));
			this.messageAttacheeCollectionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Reference Number";
			zTextBoxColumnStyleInfo1.ColumnName = "UserFriendlyCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCheckBoxColumnStyleInfo1.Caption = "Should Send Now";
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSendNow";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.messageAttacheeCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.messageAttacheeCollectionGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.messageAttacheeCollectionGrid.EnableToolTips = false;
			this.messageAttacheeCollectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.messageAttacheeCollectionGrid.LayoutKey = "MessageAttacheeCollectionGrid";
			this.messageAttacheeCollectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 48, true);
			this.messageAttacheeCollectionGrid.Name = "MessageAttacheeCollectionGrid";
			this.messageAttacheeCollectionGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.messageAttacheeCollectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 112, true);
			this.messageAttacheeCollectionGrid.TabIndex = 1;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((MessageAttacheeSelection)(((object)(((MessageAttacheeSelectionCollection)(null)))))).UserFriendlyCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((MessageAttacheeSelection)(((object)(((MessageAttacheeSelectionCollection)(null)))))).UserFriendlyCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((MessageAttacheeSelection)(((object)(((MessageAttacheeSelectionCollection)(null)))))).ShouldSendNow)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((MessageAttacheeSelection)(((object)(((MessageAttacheeSelectionCollection)(null)))))).ShouldSendNowInfo)));
			// 
			// OKBoundButton
			// 
			this.oKBoundButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oKBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 168, true);
			this.oKBoundButton.Name = "OKBoundButton";
			this.oKBoundButton.TabIndex = 6;
			this.oKBoundButton.Text = "&Send";
			this.oKBoundButton.Click += new System.EventHandler(this.OKBoundButton_Click);
			// 
			// CancelBoundButton
			// 
			this.cancelBoundButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 168, true);
			this.cancelBoundButton.Name = "CancelBoundButton";
			this.cancelBoundButton.TabIndex = 7;
			this.cancelBoundButton.Text = "&Cancel";
			this.cancelBoundButton.Click += new System.EventHandler(this.CancelBoundButton_Click);
			// 
			// Label1
			// 
			this.label1.IsFontBold = true;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.label1.Name = "Label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 32, true);
			this.label1.TabIndex = 8;
			this.label1.Text = "Select the Entry(s) you wish to send amendment / withdrawal message(s) on and sel" +
				"ect SEND. If you want to CANCEL the messaging process, select CANCEL now.";
			// 
			// MessageSelectionForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 216, true);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cancelBoundButton);
			this.Controls.Add(this.oKBoundButton);
			this.Controls.Add(this.messageAttacheeCollectionGrid);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.MessageAttacheeSelectionCollection";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "MessageSelectionForm";
			this.Controls.SetChildIndex(this.messageAttacheeCollectionGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.oKBoundButton, 0);
			this.Controls.SetChildIndex(this.cancelBoundButton, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.messageAttacheeCollectionGrid)).EndInit();
			this.ResumeLayout(false);
		}

		internal ZButton oKBoundButton;
		internal ZButton cancelBoundButton;
		ZArchitecture.ZLabel label1;
		ZArchitecture.ZGrid messageAttacheeCollectionGrid;
	}
}
