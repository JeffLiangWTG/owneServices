namespace Enterprise.Customs.BR.GUI
{
	partial class DispatchInstructionUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReferenceNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReferenceNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumbersGrid)).BeginInit();
			this.NumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// ReferenceNumbersGroupBox
			// 
			this.ReferenceNumbersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ReferenceNumbersGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("2BE5877C-7518-4B96-89FC-AF332FA99997", "Dispatch Instruction Documents");
			this.ReferenceNumbersGroupBox.Controls.Add(this.NumbersGrid);
			this.ReferenceNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferenceNumbersGroupBox.Name = "ReferenceNumbersGroupBox";
			this.ReferenceNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 284, true);
			this.ReferenceNumbersGroupBox.TabIndex = 1;
			this.ReferenceNumbersGroupBox.TabStop = false;
			// 
			// NumbersGrid
			// 
			this.NumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NumbersGrid, "DispatchInstructionNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).DispatchInstructionNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.DispatchInstructionNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).DispatchInstructionNumbers)).SyncRoot)).CE_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.DispatchInstructionNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).DispatchInstructionNumbers)).SyncRoot)).CE_EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.DispatchInstructionNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).DispatchInstructionNumbers)).SyncRoot)).AdditionalReferenceNumberTypeDescription)));
			this.NumbersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("7B0921EB-170A-4151-87ED-E40124B72186", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "CE_EntryType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("8A1F317F-59ED-4B3D-BA70-6B1D31B59B71", "Number");
			zTextBoxColumnStyleInfo1.ColumnName = "CE_EntryNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("58FBB526-373A-4C5E-9A88-A4706F895C73", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "AdditionalReferenceNumberTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo2.IsVisible = false;
			this.NumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumbersGrid.GridId = "6AC45B71-80AB-4664-83C5-BCA755B87E81";
			this.NumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NumbersGrid.LayoutKey = "DispatchInstructionNumbersGrid";
			this.NumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.NumbersGrid.Name = "NumbersGrid";
			this.NumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 265, true);
			this.NumbersGrid.TabIndex = 0;
			// 
			// DispatchInstructionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumbersGroupBox);
			this.Name = "DispatchInstructionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 284, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReferenceNumbersGroupBox.ResumeLayout(false);
			this.ReferenceNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumbersGrid)).EndInit();
			this.NumbersGrid.ResumeLayout(false);
			this.NumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ReferenceNumbersGroupBox;
		private ZArchitecture.ZGrid NumbersGrid;
	}
}
