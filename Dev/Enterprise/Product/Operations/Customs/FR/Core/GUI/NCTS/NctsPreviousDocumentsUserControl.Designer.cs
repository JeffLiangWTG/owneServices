namespace Enterprise.Customs.FR.GUI.NCTS
{
	partial class NctsPreviousDocumentsUserControl
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
			this.PrevDocsdescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrevDocsReferenceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsdescriptionTextBox.SuspendLayout();
			this.PrevDocsReferenceCodeFindBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc);
			//PrevDocsGroupBox
			this.PrevDocsGroupBox.Controls.Remove(PrevDocsClassDropEdit);
			this.PrevDocsGroupBox.Controls.Add(this.PrevDocsReferenceCodeFindBox);
			this.PrevDocsGroupBox.Controls.Add(PrevDocsdescriptionTextBox);
			// 
			// PreviousDocumentsGrid
			// 
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, "PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc)(null)).PreviousDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.NCTS.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.NCTS.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.NCTS.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.NCTS.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc)(null)))).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_LineNo)));
			this.PreviousDocumentsGrid.TabIndex = 1;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("058FCAEC-4E5E-4180-98EA-EAEB32FE4BBC", "Line No.");
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			//
			// PrevDocsClassDropEdit
			// 
			this.PrevDocsdescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PrevDocsdescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PrevDocsdescriptionTextBox, "PreviousDocuments.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.NCTS.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Description)));
			this.PrevDocsdescriptionTextBox.CaptionResourceString = null;
			this.PrevDocsdescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 38, true);
			this.PrevDocsdescriptionTextBox.Name = "PrevDocsdescriptionTextBox";
			this.PrevDocsdescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 20, true);
			this.PrevDocsdescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PrevDocsdescriptionTextBox.TabIndex = 5;
			// 
			// PrevDocsTypeDropEdit
			// 
			this.PrevDocsTypeDropEdit.TextChanged += PrevDocsTypeDropEdit_TextChanged;
			// 
			// PrevDocsReferenceCodeFindBox
			// 
			this.PrevDocsReferenceCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PrevDocsReferenceCodeFindBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PrevDocsReferenceCodeFindBox, "PreviousDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.NCTS.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.PrevDocsReferenceCodeFindBox.CaptionResourceString = null;
			this.PrevDocsReferenceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 62, true);
			this.PrevDocsReferenceCodeFindBox.Name = "PrevDocsReferenceCodeFindBox";
			this.PrevDocsReferenceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 20, true);
			this.PrevDocsReferenceCodeFindBox.TabIndex = 6;

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.PrevDocsGroupBox.ResumeLayout(false);
			this.PrevDocsGroupBox.PerformLayout();
			this.PrevDocsdescriptionTextBox.ResumeLayout(true);
			this.PrevDocsdescriptionTextBox.PerformLayout();
			this.PrevDocsReferenceCodeFindBox.ResumeLayout(true);
			this.PrevDocsReferenceCodeFindBox.PerformLayout();
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		protected Enterprise.ZArchitecture.ZTextBox PrevDocsdescriptionTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox PrevDocsReferenceCodeFindBox;
		#endregion
	}
}
