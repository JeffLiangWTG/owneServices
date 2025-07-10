namespace Enterprise.Accounting.GUI.ARAP.Invoicing.TaxFramework
{
	partial class WithholdingJournalParentPivotForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.JournalsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JournalsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsPanel.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.JournalsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JournalsGrid)).BeginInit();
			this.JournalsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 272, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 25, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplayCollection);
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.postingButtonsUserControl);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 34, true);
			this.ButtonsPanel.TabIndex = 1;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 9, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// JournalsGroupBox
			// 
			this.JournalsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6eaa2a90-04cb-40a2-8fb4-d20bbcc21c87", "Withholding Journals");
			this.JournalsGroupBox.Controls.Add(this.JournalsGrid);
			this.JournalsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JournalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JournalsGroupBox.Name = "JournalsGroupBox";
			this.JournalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 238, true);
			this.JournalsGroupBox.TabIndex = 0;
			this.JournalsGroupBox.TabStop = false;
			// 
			// JournalsGrid
			// 
			this.JournalsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JournalsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).DebitCreditSign)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).GLAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).Branch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplay)(null)).Department)));
			this.JournalsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("06d0bd03-6124-4208-94b1-f2faa8076ef4", "Transaction Category");
			zTextBoxColumnStyleInfo1.ColumnName = "TransactionCategory";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("abd0f1e2-e55f-4db4-b85f-f1d21246d996", "Account");
			zTextBoxColumnStyleInfo2.ColumnName = "Organisation";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1ed70022-a619-43e8-98c5-f4c880163f77", "Post Date");
			zDateEditColumnStyleInfo1.ColumnName = "PostDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("59088e53-2d68-451f-a3b2-f9ea5ae3224d", "Invoice Date");
			zDateEditColumnStyleInfo2.ColumnName = "InvoiceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cbc73025-d01d-4fb5-871b-b878a2ba2e2e", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(213);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7340093a-f507-447b-9228-3a6b63f91bbf", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("58215306-0a81-4e48-a9ab-358a07b31c9c", "Exchange Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "ExchangeRate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("65bd3262-d519-4a07-9ac6-cb74b1ca327d", "Local Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8ee98c6b-6132-4520-b61f-823cc3157caf", "Currency");
			zTextBoxColumnStyleInfo4.ColumnName = "Currency";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b18e12a8-7625-43e1-b0e8-d0d4f2eb5495", "DR CR Sign", "Debit Credit Sign", "");
			zTextBoxColumnStyleInfo5.ColumnName = "DebitCreditSign";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("42491650-4699-4a7a-8c06-27d524899e5c", "GL Account");
			zTextBoxColumnStyleInfo6.ColumnName = "GLAccount";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5fcfd9cd-3660-4be2-bd1d-202b04cded44", "Branch");
			zTextBoxColumnStyleInfo7.ColumnName = "Branch";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1cffc50e-59ed-45cc-978b-999506e51fdd", "Department");
			zTextBoxColumnStyleInfo8.ColumnName = "Department";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JournalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.JournalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.JournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.JournalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JournalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.JournalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.JournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.JournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.JournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.JournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.JournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.JournalsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JournalsGrid.GridId = "4797cd7e-bf18-47af-8034-c62d33fd78a9";
			this.JournalsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JournalsGrid.LayoutKey = "JournalsGrid";
			this.JournalsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.JournalsGrid.Name = "JournalsGrid";
			this.JournalsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 222, true);
			this.JournalsGrid.TabIndex = 0;
			// 
			// WithholdingJournalParentPivotForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b98c6ade-5b46-497d-8aa5-6aa5329aa00b", "Realize Withholding Journals");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 297, true);
			this.Controls.Add(this.JournalsGroupBox);
			this.Controls.Add(this.ButtonsPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.WithholdingJournalForDisplayCollection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 213, true);
			this.Name = "WithholdingJournalParentPivotForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.JournalsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.JournalsGroupBox.ResumeLayout(false);
			this.JournalsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JournalsGrid)).EndInit();
			this.JournalsGrid.ResumeLayout(false);
			this.JournalsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ButtonsPanel;
		private ZArchitecture.GUI.ZGroupBox JournalsGroupBox;
		private ZArchitecture.ZGrid JournalsGrid;
		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
	}
}