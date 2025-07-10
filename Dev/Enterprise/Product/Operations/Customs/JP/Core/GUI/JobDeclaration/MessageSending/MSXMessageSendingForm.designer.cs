using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class MSXMessageSendingForm
	{
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.DocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExportPathButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExportPathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).BeginInit();
			this.DocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 547, true);
			this.SendButton.TabIndex = 10;
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 547, true);
			this.CancelButton2.TabIndex = 11;
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("4C268110-5684-498F-BC81-73EDE50737FC", "Messages");
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 256, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 237, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 580, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.MSXMessageSendingObjectParent);
			// 
			// DocumentsGroupBox
			// 
			this.DocumentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DocumentsGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("9EDD4D09-89BD-4144-A4D1-16E6DDE5B14A", "Documents");
			this.DocumentsGroupBox.Controls.Add(this.DocumentsGrid);
			this.DocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 272, true);
			this.DocumentsGroupBox.Name = "DocumentsGroupBox";
			this.DocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 242, true);
			this.DocumentsGroupBox.TabIndex = 4;
			this.DocumentsGroupBox.TabStop = false;
			// 
			// DocumentsGrid
			// 
			this.DocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentsGrid, "SendingObjectsCollection.Attachments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Attachments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectAttachment)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Attachments)).SyncRoot)).File)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectAttachment)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Attachments)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectAttachment)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Attachments)).SyncRoot)).TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectAttachment)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Attachments)).SyncRoot)).FileSize)));
			this.DocumentsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "File";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "Type";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("0E611B47-5F87-40F8-8A6D-E988CA395AAE", "Type");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("F08C241D-3C2F-4FBE-973A-F1708CBAEFF3", "Type Description");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "FileSize";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.DocumentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGrid.GridId = "13A8D182-32AB-499C-BA0A-950391237355";
			this.DocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentsGrid.LayoutKey = "DocumentsGrid";
			this.DocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DocumentsGrid.Name = "DocumentsGrid";
			this.DocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 223, true);
			this.DocumentsGrid.TabIndex = 0;
			// 
			// ExportPathButton
			//
			this.ExportPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportPathButton.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("1e27a9ba-4a4d-4205-8733-e7b98e17487a", "Choose");
			this.ExportPathButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 521, true);
			this.ExportPathButton.Name = "ExportPathButton";
			this.ExportPathButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.ExportPathButton.TabIndex = 8;
			this.ExportPathButton.ToolTipCaption = null;
			// 
			// ExportPathTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportPathTextBox, "ExportPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.MSXMessageSendingObjectParent)(null)).ExportPath)));
			this.ExportPathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 521, true);
			this.ExportPathTextBox.Name = "ExportPathTextBox";
			this.ExportPathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 20, true);
			this.ExportPathTextBox.TabIndex = 7;
			this.ExportPathTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportButton.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("6ecbd91c-b42d-409d-8371-7630cf1e08e3", "Export");
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 547, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.ExportButton.TabIndex = 9;
			this.ExportButton.ToolTipCaption = null;
			// 
			// MSXMessageSendingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 603, true);
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.ExportPathButton);
			this.Controls.Add(this.ExportPathTextBox);
			this.Controls.Add(this.DocumentsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.JP.Business.MSXMessageSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 640, true);
			this.Name = "MSXMessageSendingForm";
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.DocumentsGroupBox, 0);
			this.Controls.SetChildIndex(this.ExportPathTextBox, 0);
			this.Controls.SetChildIndex(this.ExportPathButton, 0);
			this.Controls.SetChildIndex(this.ExportButton, 0);
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocumentsGroupBox.ResumeLayout(false);
			this.DocumentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).EndInit();
			this.DocumentsGrid.ResumeLayout(false);
			this.DocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZGroupBox DocumentsGroupBox;
		ZGrid DocumentsGrid;
		ZButton ExportPathButton;
		ZTextBox ExportPathTextBox;
		ZButton ExportButton;
	}
}
