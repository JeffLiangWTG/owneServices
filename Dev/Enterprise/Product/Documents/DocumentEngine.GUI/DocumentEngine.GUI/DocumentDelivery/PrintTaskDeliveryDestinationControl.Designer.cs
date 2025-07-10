namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	partial class PrintTaskDeliveryDestinationControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ShowOnlyPrintersUserCanPrintToCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PrinterSelectionGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.NumCopiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.printerHelpControl1 = new Enterprise.DocumentEngine.GUI.DocumentDelivery.PrinterHelpControl();
			this.PrintAsDraftCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.PrintTaskSettings);
			// 
			// ShowOnlyPrintersUserCanPrintToCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ShowOnlyPrintersUserCanPrintToCheckBox, "PrinterDelivery+ShowOnlyPrintersUserCanPrintTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).PrinterDelivery.ShowOnlyPrintersUserCanPrintTo)));
			this.ShowOnlyPrintersUserCanPrintToCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryDestinationControl|fbd9a550-93c5-4f37-966d-e9bc123ae544", "Only show printers I can access");
			this.ShowOnlyPrintersUserCanPrintToCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowOnlyPrintersUserCanPrintToCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 7, true);
			this.ShowOnlyPrintersUserCanPrintToCheckBox.Name = "ShowOnlyPrintersUserCanPrintToCheckBox";
			this.ShowOnlyPrintersUserCanPrintToCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 18, true);
			this.ShowOnlyPrintersUserCanPrintToCheckBox.TabIndex = 22;
			// 
			// PrinterSelectionGuidDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PrinterSelectionGuidDropEdit, "PrinterDelivery+PrintQueuePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).PrinterDelivery.PrintQueuePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).PrinterDelivery.PrinterNames)));
			this.PrinterSelectionGuidDropEdit.BindToList = "PrinterDelivery+PrinterNames";
			this.PrinterSelectionGuidDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PrinterSelectionGuidDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryDestinationControl|2e88ad52-e9cf-40b9-9586-790ea7791cb5", "Printer");
			this.PrinterSelectionGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 5, true);
			this.PrinterSelectionGuidDropEdit.Name = "PrinterSelectionGuidDropEdit";
			this.PrinterSelectionGuidDropEdit.PreBoundMaxLength = 38;
			this.PrinterSelectionGuidDropEdit.ShowDescriptionBox = false;
			this.PrinterSelectionGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.PrinterSelectionGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.PrinterSelectionGuidDropEdit.TabIndex = 19;
			// 
			// NumCopiesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumCopiesCalcEdit, "PrinterDelivery+NumberOfCopies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).PrinterDelivery.NumberOfCopies)));
			this.NumCopiesCalcEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryDestinationControl|64e539fb-cb4d-4536-b4c7-dbb0c13fb43f", "Copies");
			this.NumCopiesCalcEdit.Decimals = 0;
			this.NumCopiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 5, true);
			this.NumCopiesCalcEdit.Name = "NumCopiesCalcEdit";
			this.NumCopiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.NumCopiesCalcEdit.TabIndex = 20;
			this.NumCopiesCalcEdit.Text = "0";
			this.NumCopiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// printerHelpControl1
			// 
			this.printerHelpControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 29, true);
			this.printerHelpControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 23, true);
			this.printerHelpControl1.Name = "printerHelpControl1";
			this.printerHelpControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 23, true);
			this.printerHelpControl1.TabIndex = 17;
			// 
			// PrintAsDraftCheckbox
			// 
			this.BindingSource.SetBindingMember(this.PrintAsDraftCheckbox, "IsDraft");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).IsDraft)));
			this.PrintAsDraftCheckbox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryDestinationControl|10fc9b1b-2000-488c-bca4-318db3182b93", "Draft");
			this.PrintAsDraftCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintAsDraftCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 7, true);
			this.PrintAsDraftCheckbox.Name = "PrintAsDraftCheckbox";
			this.PrintAsDraftCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 18, true);
			this.PrintAsDraftCheckbox.TabIndex = 21;
			// 
			// PrintTaskDeliveryDestinationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PrintAsDraftCheckbox);
			this.Controls.Add(this.ShowOnlyPrintersUserCanPrintToCheckBox);
			this.Controls.Add(this.PrinterSelectionGuidDropEdit);
			this.Controls.Add(this.NumCopiesCalcEdit);
			this.Controls.Add(this.printerHelpControl1);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 55, true);
			this.Name = "PrintTaskDeliveryDestinationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 55, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox ShowOnlyPrintersUserCanPrintToCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGuidDropEdit PrinterSelectionGuidDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit NumCopiesCalcEdit;
		private PrinterHelpControl printerHelpControl1;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PrintAsDraftCheckbox;

	}
}
