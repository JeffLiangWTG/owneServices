using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.SWT.GUI
{
	partial class SWTPrinterSelectionForm : ZChildForm
	{
		Enterprise.ZArchitecture.ZLabel zPrinterLabel;
		ZGuidDropEdit PrinterSelectionGuidDropEdit;
		ZButton PrintButton;
		ZButton zButton2;

		new void InitializeComponent()
		{
			this.zPrinterLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PrinterSelectionGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 76, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.DeliveryInstructions);
			// 
			// zPrinterLabel
			// 
			this.zPrinterLabel.AutoSize = true;
			this.zPrinterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.zPrinterLabel.Name = "zPrinterLabel";
			this.zPrinterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 13, true);
			this.zPrinterLabel.TabIndex = 1;
			this.zPrinterLabel.Text = "Printer:";
			// 
			// PrinterSelectionGuidDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PrinterSelectionGuidDropEdit, "PrinterDelivery+PrintQueuePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.PrintQueuePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.PrinterNames)));
			this.PrinterSelectionGuidDropEdit.BindToList = "PrinterDelivery+PrinterNames";
			this.PrinterSelectionGuidDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PrinterSelectionGuidDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("SWTPrinterSelectionForm|67a4128a-bf07-453a-96b2-1af635b0bdf0", "Select a printer to use");
			this.PrinterSelectionGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 12, true);
			this.PrinterSelectionGuidDropEdit.Name = "PrinterSelectionGuidDropEdit";
			this.PrinterSelectionGuidDropEdit.PreBoundMaxLength = 38;
			this.PrinterSelectionGuidDropEdit.ShowDescriptionBox = false;
			this.PrinterSelectionGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.PrinterSelectionGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.PrinterSelectionGuidDropEdit.TabIndex = 2;
			// 
			// PrintButton
			// 
			this.PrintButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 47, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 23, true);
			this.PrintButton.TabIndex = 5;
			this.PrintButton.Text = "Print Reports";
			this.PrintButton.UseVisualStyleBackColor = true;
			// 
			// zButton2
			// 
			this.zButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.zButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 47, true);
			this.zButton2.Name = "zButton2";
			this.zButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButton2.TabIndex = 10;
			this.zButton2.Text = "Cancel";
			this.zButton2.UseVisualStyleBackColor = true;
			// 
			// SWTPrinterSelectionForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 100, true);
			this.Controls.Add(this.zPrinterLabel);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.PrinterSelectionGuidDropEdit);
			this.Controls.Add(this.zButton2);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.DeliveryInstructions);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.DeliveryInstructions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "SWTPrinterSelectionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.zButton2, 0);
			this.Controls.SetChildIndex(this.PrinterSelectionGuidDropEdit, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.zPrinterLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
