namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class Phase5ArrivalSummaryDeclarationUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SummaryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreviousSummaryDeclarationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.G4PreviousDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.G4PreviousDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SummaryTypeDropEdit.SuspendLayout();
			this.G4PreviousDocumentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.G4PreviousDocumentGrid)).BeginInit();
			this.G4PreviousDocumentGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// SummaryTypeDropEdit
			// 
			this.SummaryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SummaryTypeDropEdit, "ESNctsHeader.CEN_SummaryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ESNctsHeader.CEN_SummaryType)));
			this.SummaryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 13, true);
			this.SummaryTypeDropEdit.Name = "SummaryTypeDropEdit";
			this.SummaryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.SummaryTypeDropEdit.TabIndex = 0;
			// 
			// PreviousSummaryDeclarationTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousSummaryDeclarationTextBox, "ESNctsHeader.CEN_PreviousSummaryDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ESNctsHeader.CEN_PreviousSummaryDeclaration)));
			this.PreviousSummaryDeclarationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 52, true);
			this.PreviousSummaryDeclarationTextBox.Name = "PreviousSummaryDeclarationTextBox";
			this.PreviousSummaryDeclarationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.PreviousSummaryDeclarationTextBox.TabIndex = 5;
			// 
			// G4PreviousDocumentGroupBox
			// 
			this.G4PreviousDocumentGroupBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("4689FE6B-6B62-498A-B693-A1BD9407A875", "G4");
			this.G4PreviousDocumentGroupBox.Controls.Add(this.G4PreviousDocumentGrid);
			this.G4PreviousDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 101, true);
			this.G4PreviousDocumentGroupBox.Name = "G4PreviousDocumentGroupBox";
			this.G4PreviousDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 92, true);
			this.G4PreviousDocumentGroupBox.TabIndex = 1;
			this.G4PreviousDocumentGroupBox.TabStop = false;
			// 
			// G4PreviousDocumentGrid
			// 
			this.G4PreviousDocumentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.G4PreviousDocumentGrid, "ArrivalMovementHeader.G4PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.G4PreviousDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.G4PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.G4PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.G4PreviousDocumentGrid.CaptionVisible = false;
			this.G4PreviousDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("15ea8390-ef1b-4a29-a1a5-87009067a0da", "MRN");
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.G4PreviousDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.G4PreviousDocumentGrid.GridId = "17EE9E24-30C4-49E3-B749-A46C5353AE22";
			this.G4PreviousDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.G4PreviousDocumentGrid.LayoutKey = "G4PreviousDocumentGrid";
			this.G4PreviousDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.G4PreviousDocumentGrid.Name = "G4PreviousDocumentGrid";
			this.G4PreviousDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 67, true);
			this.G4PreviousDocumentGrid.TabIndex = 1;
			// 
			// Phase5ArrivalSummaryDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SummaryTypeDropEdit);
			this.Controls.Add(this.PreviousSummaryDeclarationTextBox);
			this.Controls.Add(this.G4PreviousDocumentGroupBox);
			this.Name = "Phase5ArrivalSummaryDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 457, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SummaryTypeDropEdit.ResumeLayout(true);
			this.SummaryTypeDropEdit.PerformLayout();
			this.G4PreviousDocumentGroupBox.ResumeLayout(false);
			this.G4PreviousDocumentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.G4PreviousDocumentGrid)).EndInit();
			this.G4PreviousDocumentGrid.ResumeLayout(false);
			this.G4PreviousDocumentGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit SummaryTypeDropEdit;
		internal ZArchitecture.ZTextBox PreviousSummaryDeclarationTextBox;
		internal ZArchitecture.GUI.ZGroupBox G4PreviousDocumentGroupBox;
		internal ZArchitecture.ZGrid G4PreviousDocumentGrid;
	}
}
