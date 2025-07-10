using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class AlternativeEvidenceUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AlternativeEvidenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AlternativeEvidencesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AlternativeEvidenceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidencesGrid)).BeginInit();
			this.AlternativeEvidencesGrid.SuspendLayout();
			this.AdditionalDocumentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsGrid)).BeginInit();
			this.AdditionalDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.IAlternativeEvidenceCollection<Enterprise.Customs.EU.ExitControl.Business.AlternativeEvidence>);
			// 
			// AlternativeEvidenceGroupBox
			// 
			this.AlternativeEvidenceGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("F55AE871-5A6C-4664-B8AF-A1880414C9AA", "Alternative Evidence");
			this.AlternativeEvidenceGroupBox.Controls.Add(this.AlternativeEvidencesGrid);
			this.AlternativeEvidenceGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.AlternativeEvidenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AlternativeEvidenceGroupBox.Name = "AlternativeEvidenceGroupBox";
			this.AlternativeEvidenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 341, true);
			this.AlternativeEvidenceGroupBox.TabIndex = 0;
			this.AlternativeEvidenceGroupBox.TabStop = false;
			// 
			// AlternativeEvidencesGrid
			// 
			this.AlternativeEvidencesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AlternativeEvidencesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.AlternativeEvidence)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.AlternativeEvidence)(null)).CY_Code)));
			this.AlternativeEvidencesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AlternativeEvidencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AlternativeEvidencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlternativeEvidencesGrid.GridId = "b4f66057-3df3-4d00-b724-2151e2a4f795";
			this.AlternativeEvidencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AlternativeEvidencesGrid.LayoutKey = "AlternativeEvidencesGrid";
			this.AlternativeEvidencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AlternativeEvidencesGrid.Name = "AlternativeEvidencesGrid";
			this.AlternativeEvidencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 322, true);
			this.AlternativeEvidencesGrid.TabIndex = 0;
			// 
			// AdditionalDocumentGroupBox
			// 
			this.AdditionalDocumentGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("9DD7EAFC-825E-4E54-96F1-6E55B8A3E063", "Additional Document");
			this.AdditionalDocumentGroupBox.Controls.Add(this.AdditionalDocumentsGrid);
			this.AdditionalDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 0, true);
			this.AdditionalDocumentGroupBox.Name = "AdditionalDocumentGroupBox";
			this.AdditionalDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 341, true);
			this.AdditionalDocumentGroupBox.TabIndex = 1;
			this.AdditionalDocumentGroupBox.TabStop = false;
			// 
			// AdditionalDocumentsGrid
			// 
			this.AdditionalDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalDocumentsGrid, "AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.AlternativeEvidence)(null)).AdditionalInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.AlternativeEvidence)(null)).AdditionalInfos)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.AlternativeEvidence)(null)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.AlternativeEvidence)(null)).AdditionalInfos)).SyncRoot)).CSI_ReferenceNumber)));
			this.AdditionalDocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AdditionalDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AdditionalDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDocumentsGrid.GridId = "b7d3493e-b0ed-45ac-9391-5a2e4010b159";
			this.AdditionalDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalDocumentsGrid.LayoutKey = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AdditionalDocumentsGrid.Name = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 322, true);
			this.AdditionalDocumentsGrid.TabIndex = 0;
			// 
			// AlternativeEvidenceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalDocumentGroupBox);
			this.Controls.Add(this.AlternativeEvidenceGroupBox);
			this.Name = "AlternativeEvidenceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 341, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AlternativeEvidenceGroupBox.ResumeLayout(false);
			this.AlternativeEvidenceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidencesGrid)).EndInit();
			this.AlternativeEvidencesGrid.ResumeLayout(false);
			this.AlternativeEvidencesGrid.PerformLayout();
			this.AdditionalDocumentGroupBox.ResumeLayout(false);
			this.AdditionalDocumentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsGrid)).EndInit();
			this.AdditionalDocumentsGrid.ResumeLayout(false);
			this.AdditionalDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox AlternativeEvidenceGroupBox;
		internal ZArchitecture.ZGrid AlternativeEvidencesGrid;
		internal ZArchitecture.GUI.ZGroupBox AdditionalDocumentGroupBox;
		internal ZArchitecture.ZGrid AdditionalDocumentsGrid;
	}
}
