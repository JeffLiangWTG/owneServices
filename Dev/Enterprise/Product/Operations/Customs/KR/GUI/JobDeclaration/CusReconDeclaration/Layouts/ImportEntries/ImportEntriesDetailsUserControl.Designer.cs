namespace Enterprise.Customs.KR.GUI
{
	partial class ImportEntriesDetailsUserControl
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
            this.SequenceNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ImportEntryNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.ImportEntryLineNumCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CustomsDisbursementBillCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AmendSequenceNumber5WNDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ImportEntryNumberCodeFindBox.SuspendLayout();
            this.ImportEntryLineNumCodeFindBox.SuspendLayout();
            this.CustomsDisbursementBillCodeFindBox.SuspendLayout();
			this.AmendSequenceNumber5WNDropEdit.SuspendLayout();
			this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusReconDeclaration);
            // 
            // SequenceNumberCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.SequenceNumberCalcEdit, "CusReconEntryLines.Header+CRE_SequenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).Header.CRE_SequenceNumber)));
            this.SequenceNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
            this.SequenceNumberCalcEdit.Name = "SequenceNumberCalcEdit";
			this.SequenceNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
            this.SequenceNumberCalcEdit.TabIndex = 0;
            this.SequenceNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SequenceNumberCalcEdit.TrackDisposedAccess = true;
            // 
            // ImportEntryNumberCodeFindBox
            // 
            this.ImportEntryNumberCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ImportEntryNumberCodeFindBox, "CusReconEntryLines.Header+CRE_OriginalEntryNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).Header.CRE_OriginalEntryNumber)));
            this.ImportEntryNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 31, true);
            this.ImportEntryNumberCodeFindBox.Name = "ImportEntryNumberCodeFindBox";
			this.ImportEntryNumberCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.KR.ImportEntryDetails;
            this.ImportEntryNumberCodeFindBox.ParentType = null;
            this.ImportEntryNumberCodeFindBox.ShowDescriptionBox = false;
			this.ImportEntryNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.ImportEntryNumberCodeFindBox.TabIndex = 1;

            // 
            // ImportEntryLineNumCodeFindBox
            // 
            this.ImportEntryLineNumCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ImportEntryLineNumCodeFindBox, "CusReconEntryLines.FormattedOriginalEntryLineNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).FormattedOriginalEntryLineNumber)));
            this.ImportEntryLineNumCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 60, true);
            this.ImportEntryLineNumCodeFindBox.Name = "ImportEntryLineNumCodeFindBox";
            this.ImportEntryLineNumCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.KR.EntryLineDetailsFor5UL;
            this.ImportEntryLineNumCodeFindBox.ParentType = null;
            this.ImportEntryLineNumCodeFindBox.ShowDescriptionBox = false;
            this.ImportEntryLineNumCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 19, true);
            this.ImportEntryLineNumCodeFindBox.TabIndex = 6;
            // 
            // CustomsDisbursementBillCodeFindBox
            // 
            this.CustomsDisbursementBillCodeFindBox.AllowDrop = true;
            this.CustomsDisbursementBillCodeFindBox.AllowModuleMultiSelect = true;
            this.BindingSource.SetBindingMember(this.CustomsDisbursementBillCodeFindBox, "CusReconEntryLines.Header+CRE_CustomsBillNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).Header.CRE_CustomsBillNumber)));
            this.CustomsDisbursementBillCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 90, true);
            this.CustomsDisbursementBillCodeFindBox.Name = "CustomsDisbursementBillCodeFindBox";
			this.CustomsDisbursementBillCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.KR.EntryCustomsBillsFor5UL;
            this.CustomsDisbursementBillCodeFindBox.ParentType = null;
            this.CustomsDisbursementBillCodeFindBox.ShowDescriptionBox = false;
            this.CustomsDisbursementBillCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
            this.CustomsDisbursementBillCodeFindBox.TabIndex = 4;
			// 
			// AmendSequenceNumber5WNDropEdit
			// 
			this.AmendSequenceNumber5WNDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmendSequenceNumber5WNDropEdit, "CusReconEntryLines.Header+Amendment5WNVersionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).Header.Amendment5WNVersionNumber)));
			this.AmendSequenceNumber5WNDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 119, true);
			this.AmendSequenceNumber5WNDropEdit.Name = "AmendSequenceNumber5WNDropEdit";
			this.AmendSequenceNumber5WNDropEdit.PreBoundMaxLength = 2;
			this.AmendSequenceNumber5WNDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
			this.AmendSequenceNumber5WNDropEdit.TabIndex = 5;
			// 
			// ImportEntriesDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AmendSequenceNumber5WNDropEdit);
			this.Controls.Add(this.CustomsDisbursementBillCodeFindBox);
            this.Controls.Add(this.ImportEntryLineNumCodeFindBox);
            this.Controls.Add(this.ImportEntryNumberCodeFindBox);
            this.Controls.Add(this.SequenceNumberCalcEdit);
            this.Name = "ImportEntriesDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 152, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ImportEntryNumberCodeFindBox.ResumeLayout(true);
            this.ImportEntryNumberCodeFindBox.PerformLayout();
            this.ImportEntryLineNumCodeFindBox.ResumeLayout(true);
            this.ImportEntryLineNumCodeFindBox.PerformLayout();
            this.CustomsDisbursementBillCodeFindBox.ResumeLayout(true);
            this.CustomsDisbursementBillCodeFindBox.PerformLayout();
			this.AmendSequenceNumber5WNDropEdit.ResumeLayout(true);
			this.AmendSequenceNumber5WNDropEdit.PerformLayout();
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit SequenceNumberCalcEdit;
		internal ZArchitecture.GUI.ZCodeFindBox ImportEntryNumberCodeFindBox;
        internal ZArchitecture.GUI.ZCodeFindBox ImportEntryLineNumCodeFindBox;
        internal ZArchitecture.GUI.ZCodeFindBox CustomsDisbursementBillCodeFindBox;
		public ZArchitecture.GUI.ZDropEdit AmendSequenceNumber5WNDropEdit;
	}
}
