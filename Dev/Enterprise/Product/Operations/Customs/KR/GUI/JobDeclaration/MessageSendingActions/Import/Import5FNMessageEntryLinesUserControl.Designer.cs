namespace Enterprise.Customs.KR.GUI
{
	partial class Import5FNMessageEntryLinesUserControl
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
      Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
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
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).ShouldSend)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).EntryLineNo)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).DutyReduction)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).HSCode)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).ModelName)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).SerialNumber)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).DutyReductionCode)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).InstalmentCode)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).SpecificUse)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject)(null)).MessageSendingEntryLines)).SyncRoot)).PostClearanceYN)));
      this.EntryLinesGrid.CaptionVisible = false;
      zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSend";
      zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
      zCheckBoxColumnStyleInfo1.IsMandatory = true;
      zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
      zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo1.ColumnName = "EntryLineNo";
      zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
      zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
      zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("e5202178-c56a-461e-8ab5-a3defb2a4d3e", "Duty Reduction Type");
      zTextBoxColumnStyleInfo1.ColumnName = "DutyReduction";
      zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
      zTextBoxColumnStyleInfo2.ColumnName = "HSCode";
      zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("e9e74aec-00ae-4fd6-a111-11bc2e0a7828", "Model Name");
      zTextBoxColumnStyleInfo3.ColumnName = "ModelName";
      zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
      zTextBoxColumnStyleInfo4.ColumnName = "SerialNumber";
      zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
      zTextBoxColumnStyleInfo5.ColumnName = "DutyReductionCode";
      zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
      zTextBoxColumnStyleInfo6.ColumnName = "InstalmentCode";
      zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
      zCheckBoxColumnStyleInfo2.ColumnName = "SpecificUse";
      zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
      zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73);
      zTextBoxColumnStyleInfo7.ColumnName = "PostClearanceYN";
      zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
      this.EntryLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
      this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
      this.EntryLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
      this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
      this.EntryLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.EntryLinesGrid.GridId = "A312077F-E169-4532-92F8-F7951FE69998";
      this.EntryLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.EntryLinesGrid.LayoutKey = "EntryLinesGrid";
      this.EntryLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.EntryLinesGrid.Name = "EntryLinesGrid";
      this.EntryLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 120, true);
      this.EntryLinesGrid.TabIndex = 0;
      // 
      // Import5FNMessageEntryLinesUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.EntryLinesGrid);
      this.Name = "Import5FNMessageEntryLinesUserControl";
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
	}
}
