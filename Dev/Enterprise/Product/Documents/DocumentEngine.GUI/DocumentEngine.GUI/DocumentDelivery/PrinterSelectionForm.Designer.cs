using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class PrinterSelectionForm : ZChildForm
	{
		ZLabel SelectPrinterLabel;
		protected ZButton OKButton;
		protected Enterprise.ZArchitecture.ZGrid PrintersGrid;
		Enterprise.ZArchitecture.ZCalcEdit NumCopiesCalcEdit;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SelectPrinterLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NumCopiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 264, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(241);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(241);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.DeliveryInstructions);
			// 
			// SelectPrinterLabel
			// 
			this.SelectPrinterLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrinterSelectionForm|9570a828-3722-4003-90c2-3cc7466eb5fb", "Please select a printer to send this document to");
			this.SelectPrinterLabel.IsFontBold = true;
			this.SelectPrinterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 7, true);
			this.SelectPrinterLabel.Name = "SelectPrinterLabel";
			this.SelectPrinterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 22, true);
			this.SelectPrinterLabel.TabIndex = 0;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrinterSelectionForm|6afad309-7240-45ba-9179-32d7ade49642", "&OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 234, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// PrintersGrid
			// 
			this.PrintersGrid.AllowNavigation = false;
			this.PrintersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PrintersGrid, "PrinterDelivery.Printers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.Printers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.Printers)).SyncRoot)).SQ_DisplayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.Printers)).SyncRoot)).SQ_QueueName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.Printers)).SyncRoot)).SQ_ServerName)));
			this.PrintersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "SQ_DisplayName";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(380);
			zTextBoxColumnStyleInfo2.ColumnName = "SQ_QueueName";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "SQ_ServerName";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			this.PrintersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PrintersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PrintersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PrintersGrid.GridId = "85d3e60d-b445-4725-8b6d-d07d6777f75a";
			this.PrintersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PrintersGrid.IsWholeRowSelectedOnClick = true;
			this.PrintersGrid.LayoutKey = "PrintersGrid";
			this.PrintersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 37, true);
			this.PrintersGrid.Name = "PrintersGrid";
			this.PrintersGrid.ParentRowsVisible = false;
			this.PrintersGrid.ReadOnly = true;
			this.PrintersGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PrintersGrid.RowHeadersVisible = false;
			this.PrintersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 190, true);
			this.PrintersGrid.TabIndex = 1;
			this.PrintersGrid.DoubleClick += new System.EventHandler(this.PrintersGrid_DoubleClick);
			// 
			// NumCopiesCalcEdit
			// 
			this.NumCopiesCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.NumCopiesCalcEdit, "PrinterDelivery+NumberOfCopies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.NumberOfCopies)));
			this.NumCopiesCalcEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrinterSelectionForm|4804bf5a-d40f-4391-9c1b-266cf0bc2d89", "Number of Copies");
			this.NumCopiesCalcEdit.Decimals = 0;
			this.NumCopiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 234, true);
			this.NumCopiesCalcEdit.Name = "NumCopiesCalcEdit";
			this.NumCopiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.NumCopiesCalcEdit.TabIndex = 3;
			this.NumCopiesCalcEdit.Text = "0";
			this.NumCopiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PrinterSelectionForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 286, true);
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrinterSelectionForm|03ccd2a0-5a0a-4b06-a2ac-299403fb8fd9", "Select Printer");
			this.Controls.Add(this.NumCopiesCalcEdit);
			this.Controls.Add(this.PrintersGrid);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.SelectPrinterLabel);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.DeliveryInstructions);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.DeliveryInstructions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "PrinterSelectionForm";
			this.Controls.SetChildIndex(this.SelectPrinterLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.PrintersGrid, 0);
			this.Controls.SetChildIndex(this.NumCopiesCalcEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintersGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#region Dispose

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
