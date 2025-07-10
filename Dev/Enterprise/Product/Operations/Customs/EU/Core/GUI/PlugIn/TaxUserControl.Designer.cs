using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class TaxUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();

			zCalcEditColumnStyleInfoForCalcPercentage = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();

			zTextBoxColumnStyleInfoForMethodOfCalculation = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.TaxSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TaxGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TaxBaseQuantityUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaxMethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaxRateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TaxRateDutyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaxAmountCalcEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.TaxBaseAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxBaseQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxSplitContainer)).BeginInit();
			this.TaxSplitContainer.Panel1.SuspendLayout();
			this.TaxSplitContainer.Panel2.SuspendLayout();
			this.TaxSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxGrid)).BeginInit();
			this.TaxGroupBox.SuspendLayout();
			this.TaxRateGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DeclarationType;
			// 
			// TaxSplitContainer
			// 
			this.TaxSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxSplitContainer.Name = "TaxSplitContainer";
			this.TaxSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// TaxSplitContainer.Panel1
			// 
			this.TaxSplitContainer.Panel1.Controls.Add(this.TaxGrid);
			// 
			// TaxSplitContainer.Panel2
			// 
			this.TaxSplitContainer.Panel2.Controls.Add(this.TaxGroupBox);
			this.TaxSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 245, true);
			this.TaxSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.TaxSplitContainer.TabIndex = 0;
			// 
			// TaxGrid
			// 
			this.TaxGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxGrid, "FilteredInvoiceLines.Taxes");
			this.TaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Data+G4_Type";

			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "Data+G4_BaseAmount";
			
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "Data+G4_BaseQuantity";
			
			zTextBoxColumnStyleInfo1.ColumnName = "Data+G4_Amount";
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

			zTextBoxColumnStyleInfoForMethodOfCalculation.ColumnName = JobComInvoiceLineTax.Schema.JLT_MethodOfCalculation;

			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "Data+G4_MethodOfPayment";

			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "Data+JLT_BaseQuantityUQ";

			zCalcEditColumnStyleInfoForCalcPercentage.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfoForCalcPercentage.Caption = "Calc. %";
			zCalcEditColumnStyleInfoForCalcPercentage.ColumnName = "Data+G4_CalculatedPercentage";
			zCalcEditColumnStyleInfoForCalcPercentage.IsReadOnly = true;
			zCalcEditColumnStyleInfoForCalcPercentage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right; 
			this.TaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.TaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			TaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoForMethodOfCalculation);
			this.TaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.TaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfoForCalcPercentage);
			this.TaxGrid.CopySelectedRowsAllowed = true;
			this.TaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxGrid.GridId = "A81B1415-03CF-44F1-B8E6-8265C232786A";
			this.TaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxGrid.LayoutKey = "TaxGrid";
			this.TaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxGrid.Name = "TaxGrid";
			this.TaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 65, true);
			this.TaxGrid.TabIndex = 1;
			// 
			// TaxGroupBox
			// 
			this.TaxGroupBox.Controls.Add(this.TaxMethodOfPaymentDropEdit);
			this.TaxGroupBox.Controls.Add(this.TaxBaseQuantityUQDropEdit);
			this.TaxGroupBox.Controls.Add(this.TaxRateGroupBox);
			this.TaxGroupBox.Controls.Add(this.TaxAmountCalcEdit);
			this.TaxGroupBox.Controls.Add(this.TaxBaseAmountCalcEdit);
			this.TaxGroupBox.Controls.Add(this.TaxBaseQtyCalcEdit);
			this.TaxGroupBox.Controls.Add(this.TaxTypeDropEdit);
			this.TaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.TaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxGroupBox.Name = "TaxGroupBox";
			this.TaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 176, true);
			this.TaxGroupBox.TabIndex = 1;
			this.TaxGroupBox.TabStop = false;
			this.TaxGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("96F8BF9A-BCB6-40F2-BF0D-AE0A35FF6453", "[47] Taxes");
			// 
			// TaxMethodOfPaymentDropEdit
			// 
			this.TaxMethodOfPaymentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxMethodOfPaymentDropEdit, "FilteredInvoiceLines.Taxes.Data.G4_MethodOfPayment");
			this.TaxMethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 45, true);
			this.TaxMethodOfPaymentDropEdit.Name = "TaxMethodOfPaymentDropEdit";
			this.TaxMethodOfPaymentDropEdit.PreBoundMaxLength = 1;
			this.TaxMethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.TaxMethodOfPaymentDropEdit.TabIndex = 3;
			// 
			// TaxBaseQuantityUQDropEdit
			// 
			this.TaxBaseQuantityUQDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxBaseQuantityUQDropEdit, "FilteredInvoiceLines.Taxes.JLT_BaseQuantityUQ");
			this.TaxBaseQuantityUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 97, true);
			this.TaxBaseQuantityUQDropEdit.Name = "TaxBaseQuantityUQDropEdit";
			this.TaxBaseQuantityUQDropEdit.PreBoundMaxLength = 3;
			this.TaxBaseQuantityUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.TaxBaseQuantityUQDropEdit.TabIndex = 6;
			// 
			// TaxRateGroupBox
			// 
			this.TaxRateGroupBox.Controls.Add(this.TaxRateDutyDropEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TaxRateGroupBox, false);
			this.TaxRateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 19, true);
			this.TaxRateGroupBox.Name = "TaxRateGroupBox";
			this.TaxRateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 110, true);
			this.TaxRateGroupBox.TabIndex = 8;
			this.TaxRateGroupBox.TabStop = false;
			this.TaxRateGroupBox.Text = "Box 47c";
			// 
			// TaxRateDutyDropEdit
			// 
			this.TaxRateDutyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxRateDutyDropEdit, "FilteredInvoiceLines.Taxes.Data.G4_RateDuty");
			this.TaxRateDutyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 13, true);
			this.TaxRateDutyDropEdit.Name = "TaxRateDutyDropEdit";
			this.TaxRateDutyDropEdit.PreBoundMaxLength = 3;
			this.TaxRateDutyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TaxRateDutyDropEdit.TabIndex = 9;
			// 
			// TaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TaxAmountCalcEdit, "FilteredInvoiceLines.Taxes.Data.G4_Amount");
			this.TaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 123, true);
			this.TaxAmountCalcEdit.Name = "TaxAmountCalcEdit";
			this.TaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TaxAmountCalcEdit.TabIndex = 7;
			this.TaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxBaseAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TaxBaseAmountCalcEdit, "FilteredInvoiceLines.Taxes.Data.G4_BaseAmount");
			this.TaxBaseAmountCalcEdit.DecimalPlaces = 2;
			this.TaxBaseAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 71, true);
			this.TaxBaseAmountCalcEdit.Name = "TaxBaseAmountCalcEdit";
			this.TaxBaseAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TaxBaseAmountCalcEdit.TabIndex = 4;
			this.TaxBaseAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxBaseQtyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TaxBaseQtyCalcEdit, "FilteredInvoiceLines.Taxes.Data.G4_BaseQuantity");
			this.TaxBaseQtyCalcEdit.DecimalPlaces = 2;
			this.TaxBaseQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 97, true);
			this.TaxBaseQtyCalcEdit.Name = "TaxBaseQtyCalcEdit";
			this.TaxBaseQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TaxBaseQtyCalcEdit.TabIndex = 5;
			this.TaxBaseQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxTypeDropEdit
			// 
			this.TaxTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxTypeDropEdit, "FilteredInvoiceLines.Taxes.Data.G4_Type");
			this.TaxTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 19, true);
			this.TaxTypeDropEdit.Name = "TaxTypeDropEdit";
			this.TaxTypeDropEdit.PreBoundMaxLength = 3;
			this.TaxTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.TaxTypeDropEdit.TabIndex = 2;
			// 
			// TaxUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TaxSplitContainer);
			this.Name = "TaxUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 245, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TaxSplitContainer.Panel1.ResumeLayout(false);
			this.TaxSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TaxSplitContainer)).EndInit();
			this.TaxSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TaxGrid)).EndInit();
			this.TaxGroupBox.ResumeLayout(false);
			this.TaxGroupBox.PerformLayout();
			this.TaxRateGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitContainer TaxSplitContainer;
		protected ZArchitecture.ZGrid TaxGrid;
		protected ZArchitecture.GUI.ZGroupBox TaxGroupBox;
		protected ZArchitecture.GUI.ZDropEdit TaxMethodOfPaymentDropEdit;
		protected ZArchitecture.GUI.ZDropEdit TaxBaseQuantityUQDropEdit;
		protected ZArchitecture.GUI.ZGroupBox TaxRateGroupBox;
		protected ZArchitecture.GUI.ZDropEdit TaxRateDutyDropEdit;
		protected ZArchitecture.ZTextBox TaxAmountCalcEdit;
		protected ZArchitecture.ZCalcEdit TaxBaseAmountCalcEdit;
		protected ZArchitecture.ZCalcEdit TaxBaseQtyCalcEdit;
		protected ZArchitecture.GUI.ZDropEdit TaxTypeDropEdit;

		protected Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoForMethodOfCalculation;
		protected Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfoForCalcPercentage;
	}
}
