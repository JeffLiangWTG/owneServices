namespace Enterprise.Customs.KR.GUI
{
	partial class HeaderDetailsUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.HeaderDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicHeaderDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.StatementHeaderBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.splitter = new System.Windows.Forms.Splitter();
			this.FeesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EntryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntriesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AmountDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AmountDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderDetailsGroupBox.SuspendLayout();
			this.StatementHeaderBottomPanel.SuspendLayout();
			this.FeesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesGrid)).BeginInit();
			this.FeesGrid.SuspendLayout();
			this.EntryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).BeginInit();
			this.EntriesGrid.SuspendLayout();
			this.AmountDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusStatementHeader);
			// 
			// HeaderDetailsGroupBox
			// 
			this.HeaderDetailsGroupBox.Controls.Add(this.DynamicHeaderDetailsPanel);
			this.HeaderDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HeaderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderDetailsGroupBox.Name = "HeaderDetailsGroupBox";
			this.HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 217, true);
			this.HeaderDetailsGroupBox.TabIndex = 0;
			this.HeaderDetailsGroupBox.TabStop = false;
			// 
			// DynamicHeaderDetailsPanel
			// 
			this.DynamicHeaderDetailsPanel.AllowDrop = true;
			this.DynamicHeaderDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicHeaderDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicHeaderDetailsPanel.Name = "DynamicHeaderDetailsPanel";
			this.DynamicHeaderDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 198, true);
			this.DynamicHeaderDetailsPanel.TabIndex = 6;
			// 
			// StatementHeaderBottomPanel
			// 
			this.StatementHeaderBottomPanel.AutoSize = true;
			this.StatementHeaderBottomPanel.Controls.Add(this.AmountDetailsGroupBox);
			this.StatementHeaderBottomPanel.Controls.Add(this.splitter);
			this.StatementHeaderBottomPanel.Controls.Add(this.FeesGroupBox);
			this.StatementHeaderBottomPanel.Controls.Add(this.EntryGroupBox);
			this.StatementHeaderBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatementHeaderBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			this.StatementHeaderBottomPanel.Name = "StatementHeaderBottomPanel";
			this.StatementHeaderBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 409, true);
			this.StatementHeaderBottomPanel.TabIndex = 7;
			// 
			// splitter
			// 
			this.splitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter.Location = new System.Drawing.Point(271, 0);
			this.splitter.Name = "splitter";
			this.splitter.Size = new System.Drawing.Size(4, 409);
			this.splitter.TabIndex = 11;
			this.splitter.TabStop = false;
			// 
			// FeesGroupBox
			// 
			this.FeesGroupBox.Controls.Add(this.FeesGrid);
			this.FeesGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.FeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 0, true);
			this.FeesGroupBox.Name = "FeesGroupBox";
			this.FeesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 409, true);
			this.FeesGroupBox.TabIndex = 10;
			this.FeesGroupBox.TabStop = false;
			// 
			// FeesGrid
			// 
			this.FeesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FeesGrid, "StatementLines.Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)).SyncRoot)).B4_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)).SyncRoot)).ChargeTypeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)).SyncRoot)).B4_ChargeAmount)));
			this.FeesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "B4_ChargeType";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeTypeName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "B4_ChargeAmount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.FeesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FeesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FeesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeesGrid.GridId = "7EEF50AF-246E-4310-A1E7-48DA54420AAD";
			this.FeesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeesGrid.LayoutKey = "FeesGrid";
			this.FeesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FeesGrid.Name = "FeesGrid";
			this.FeesGrid.ReadOnly = true;
			this.FeesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 390, true);
			this.FeesGrid.TabIndex = 11;
			// 
			// EntryGroupBox
			// 
			this.EntryGroupBox.Controls.Add(this.EntriesGrid);
			this.EntryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryGroupBox.Name = "EntryGroupBox";
			this.EntryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 409, true);
			this.EntryGroupBox.TabIndex = 8;
			this.EntryGroupBox.TabStop = false;
			// 
			// EntriesGrid
			// 
			this.EntriesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntriesGrid, "StatementLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_SequenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).FormattedNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).FormattedLinePaymentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).BaseAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_CustomsFeesTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.KR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_EntryDate)));
			this.EntriesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "B3_SequenceNumber";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.ColumnName = "FormattedNumber";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(113);
			zTextBoxColumnStyleInfo4.ColumnName = "B3_EntryType";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "FormattedLinePaymentNumber";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "BaseAmount";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "B3_CustomsFeesTotal";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "B3_EntryDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.EntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.EntriesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntriesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntriesGrid.GridId = "3393D7C3-DF1A-44C1-A33A-8D8B31EC7E2F";
			this.EntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntriesGrid.LayoutKey = "EntriesGrid";
			this.EntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntriesGrid.Name = "EntriesGrid";
			this.EntriesGrid.ReadOnly = true;
			this.EntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 390, true);
			this.EntriesGrid.TabIndex = 9;
			// 
			// AmountDetailsGroupBox
			// 
			this.AmountDetailsGroupBox.Controls.Add(this.AmountDetailsPanel);
			this.AmountDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AmountDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AmountDetailsGroupBox.Name = "AmountDetailsGroupBox";
			this.AmountDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 409, true);
			this.AmountDetailsGroupBox.TabIndex = 12;
			this.AmountDetailsGroupBox.TabStop = false;
			// 
			// AmountDetailsPanel
			// 
			this.AmountDetailsPanel.AllowDrop = true;
			this.AmountDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AmountDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AmountDetailsPanel.Name = "AmountDetailsPanel";
			this.AmountDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 390, true);
			this.AmountDetailsPanel.TabIndex = 6;
			// 
			// HeaderDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StatementHeaderBottomPanel);
			this.Controls.Add(this.HeaderDetailsGroupBox);
			this.Name = "HeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 626, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderDetailsGroupBox.ResumeLayout(false);
			this.HeaderDetailsGroupBox.PerformLayout();
			this.StatementHeaderBottomPanel.ResumeLayout(false);
			this.StatementHeaderBottomPanel.PerformLayout();
			this.FeesGroupBox.ResumeLayout(false);
			this.FeesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesGrid)).EndInit();
			this.FeesGrid.ResumeLayout(false);
			this.FeesGrid.PerformLayout();
			this.EntryGroupBox.ResumeLayout(false);
			this.EntryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).EndInit();
			this.EntriesGrid.ResumeLayout(false);
			this.EntriesGrid.PerformLayout();
			this.AmountDetailsGroupBox.ResumeLayout(false);
			this.AmountDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal Enterprise.ZArchitecture.GUI.ZGroupBox HeaderDetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZPanel StatementHeaderBottomPanel;
		internal Enterprise.ZArchitecture.ZGrid FeesGrid;
		internal Enterprise.ZArchitecture.ZGrid EntriesGrid;
		internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicHeaderDetailsPanel;
		private ZArchitecture.GUI.ZGroupBox EntryGroupBox;
		private ZArchitecture.GUI.ZGroupBox FeesGroupBox;
		private System.Windows.Forms.Splitter splitter;
		internal ZArchitecture.GUI.ZGroupBox AmountDetailsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel AmountDetailsPanel;
	}
}
