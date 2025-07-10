using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CheckoutForm : ZChildForm
	{
		private Enterprise.ZArchitecture.ZLabel TrackingNumberLabel;
		internal Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZGuidDropEdit LabelPrinterGuidDropEdit;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.GUI.ZGuidDropEdit InvoicePrinterGuidDropEdit;
		internal EnterDoesntTabZTextBox TrackingNumberTextBox;

		protected override void InitializeComponent()
		{
			this.TrackingNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TrackingNumberTextBox = new Enterprise.Client.UPE.GUI.EnterDoesntTabZTextBox();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LabelPrinterGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.InvoicePrinterGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 8;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(253);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(253);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.Checkout);
			// 
			// TrackingNumberLabel
			// 
			this.TrackingNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 9, true);
			this.TrackingNumberLabel.Name = "TrackingNumberLabel";
			this.TrackingNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
			this.TrackingNumberLabel.TabIndex = 0;
			this.TrackingNumberLabel.Text = "Tracking Number:";
			// 
			// TrackingNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TrackingNumberTextBox, "TrackingNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.UPE.Business.Checkout)(null)).TrackingNumber)));
			this.TrackingNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 7, true);
			this.TrackingNumberTextBox.Name = "TrackingNumberTextBox";
			this.TrackingNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.TrackingNumberTextBox.TabIndex = 1;
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 101, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 6;
			this.PrintButton.Text = "&Print";
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 101, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Text = "&Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// LabelPrinterGuidDropEdit
			// 
			this.BindingSource.SetBindingMember(this.LabelPrinterGuidDropEdit, "LabelPrinter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.Checkout)(null)).LabelPrinter)));
			this.LabelPrinterGuidDropEdit.BindToList = "PrinterNames";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Checkout)(null)).PrinterNames)));
			this.LabelPrinterGuidDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("CheckoutForm|5248c835-663d-4edc-ad51-e636fe16069e", "Label Printer");
			this.LabelPrinterGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 33, true);
			this.LabelPrinterGuidDropEdit.Name = "LabelPrinterGuidDropEdit";
			this.LabelPrinterGuidDropEdit.PreBoundMaxLength = 52;
			this.LabelPrinterGuidDropEdit.ShowDescriptionBox = false;
			this.LabelPrinterGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.LabelPrinterGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.LabelPrinterGuidDropEdit.TabIndex = 3;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 35, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "Label Printer:";
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 61, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
			this.zLabel2.TabIndex = 4;
			this.zLabel2.Text = "Invoice Printer:";
			// 
			// InvoicePrinterGuidDropEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoicePrinterGuidDropEdit, "InvoicePrinter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.Checkout)(null)).InvoicePrinter)));
			this.InvoicePrinterGuidDropEdit.BindToList = "PrinterNames";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Checkout)(null)).PrinterNames)));
			this.InvoicePrinterGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 59, true);
			this.InvoicePrinterGuidDropEdit.Name = "InvoicePrinterGuidDropEdit";
			this.InvoicePrinterGuidDropEdit.PreBoundMaxLength = 52;
			this.InvoicePrinterGuidDropEdit.ShowDescriptionBox = false;
			this.InvoicePrinterGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.InvoicePrinterGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.InvoicePrinterGuidDropEdit.TabIndex = 5;
			// 
			// CheckoutForm
			// 
			this.AcceptButton = this.PrintButton;

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 152, true);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.InvoicePrinterGuidDropEdit);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.LabelPrinterGuidDropEdit);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.TrackingNumberLabel);
			this.Controls.Add(this.TrackingNumberTextBox);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceType = typeof(Enterprise.Client.UPE.Business.Checkout);
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.Checkout";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CheckoutForm";
			this.Text = "CheckoutForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TrackingNumberTextBox, 0);
			this.Controls.SetChildIndex(this.TrackingNumberLabel, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.LabelPrinterGuidDropEdit, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.InvoicePrinterGuidDropEdit, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
