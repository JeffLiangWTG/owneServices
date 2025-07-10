using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Registry.GUI
{
	internal partial class AccountingTransactionsNumberSequenceCustomisationControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.descTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.elementsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.elementsGrid)).BeginInit();
			this.elementsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisationCollection);
			// 
			// descTextBox
			// 
			this.BindingSource.SetBindingMember(this.descTextBox, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation)(null)).Description)));
			this.descTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccountingTransactionsNumberSequenceCustomisationControl|7853BBAA-D62D-4d39-A75E-BFFD2224F585", "Element Description");
			this.descTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.descTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 278, true);
			this.descTextBox.Multiline = true;
			this.descTextBox.Name = "descTextBox";
			this.descTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 58, true);
			this.descTextBox.TabIndex = 2;
			// 
			// elementsGrid
			// 
			this.elementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.elementsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation)(null)).ElementName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation)(null)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation)(null)).Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation)(null)).Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation)(null)).Fountain)));
			this.elementsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccountingTransactionsNumberSequenceCustomisationControl|1cf94170-40ba-4b24-ad1e-173dba3a1a84", "Element Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ElementName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccountingTransactionsNumberSequenceCustomisationControl|60fcca6c-8b31-45cf-b5ca-1e3817ad710d", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "Include";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccountingTransactionsNumberSequenceCustomisationControl|31a3fe19-c51b-4fdd-be2f-b0ef5b4baecc", "Order");
			zCalcEditColumnStyleInfo1.ColumnName = "Order";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(37);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccountingTransactionsNumberSequenceCustomisationControl|6c884e5f-76c8-428f-a9a5-6867cc4d836b", "Digit/Code");
			zTextBoxColumnStyleInfo2.ColumnName = "Code";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccountingTransactionsNumberSequenceCustomisationControl|82a45ffb-aade-4fa4-aa28-a5a6995a7755", "Length");
			zCalcEditColumnStyleInfo2.ColumnName = "Length";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccountingTransactionsNumberSequenceCustomisationControl|914582dc-92c1-43d9-a8c5-b0dcc4c343ef", "Fountain");
			zCheckBoxColumnStyleInfo2.ColumnName = "Fountain";
			this.elementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.elementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.elementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.elementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.elementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.elementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.elementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.elementsGrid.LayoutKey = "panel1";
			this.elementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.elementsGrid.Name = "elementsGrid";
			this.elementsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.elementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 278, true);
			this.elementsGrid.TabIndex = 1;
			// 
			// AccountingTransactionsNumberSequenceCustomisationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.elementsGrid);
			this.Controls.Add(this.descTextBox);
			this.Name = "AccountingTransactionsNumberSequenceCustomisationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 336, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.elementsGrid)).EndInit();
			this.elementsGrid.ResumeLayout(false);
			this.elementsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZTextBox descTextBox;
		internal ZArchitecture.ZGrid elementsGrid;
	}
}
