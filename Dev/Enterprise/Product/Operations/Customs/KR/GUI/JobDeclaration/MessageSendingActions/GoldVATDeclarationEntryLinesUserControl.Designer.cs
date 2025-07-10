namespace Enterprise.Customs.KR.GUI
{
	partial class GoldVATDeclarationEntryLinesUserControl
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
      Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
      Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
      Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
      this.EntryLinesGrid = new Enterprise.ZArchitecture.ZGrid();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.EntryLinesGrid)).BeginInit();
      this.EntryLinesGrid.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject);
      // 
      // EntryLinesGrid
      // 
      this.EntryLinesGrid.AllowNavigation = false;
      this.BindingSource.SetBindingMember(this.EntryLinesGrid, "MessageSendingEntryLines");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).EntryLineNo)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).IsGoldOrItsProduct)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).HSCode)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).HSDescription)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).NetWeightInKG)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).NetWeightUnit)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).ValueForVAT)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).VAT)));
      this.EntryLinesGrid.CaptionVisible = false;
      zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo1.ColumnName = "EntryLineNo";
      zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
      zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zCheckBoxColumnStyleInfo1.ColumnName = "IsGoldOrItsProduct";
      zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
      zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zTextBoxColumnStyleInfo1.ColumnName = "HSCode";
      zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
      zTextBoxColumnStyleInfo2.ColumnName = "HSDescription";
      zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
      zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo2.ColumnName = "NetWeightInKG";
      zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
      zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zTextBoxColumnStyleInfo3.ColumnName = "NetWeightUnit";
      zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
      zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo3.ColumnName = "ValueForVAT";
      zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
      zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
      zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo4.ColumnName = "VAT";
      zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
      zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
      this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
      this.EntryLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
      this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
      this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
      this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
      this.EntryLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.EntryLinesGrid.GridId = "dbcf0ed3-952e-41f7-b09d-c20b6080ee5b";
      this.EntryLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.EntryLinesGrid.LayoutKey = "EntryLinesGrid";
      this.EntryLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.EntryLinesGrid.Name = "EntryLinesGrid";
      this.EntryLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 120, true);
      this.EntryLinesGrid.TabIndex = 0;
      // 
      // GoldVATDeclarationEntryLinesUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.EntryLinesGrid);
      this.Name = "GoldVATDeclarationEntryLinesUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 120, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.EntryLinesGrid)).EndInit();
      this.EntryLinesGrid.ResumeLayout(false);
      this.EntryLinesGrid.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		public ZArchitecture.ZGrid EntryLinesGrid;

		#endregion

		//public ZArchitecture.ZGrid EntryLinesGrid;
	}
}
