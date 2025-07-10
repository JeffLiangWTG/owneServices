namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class REXAcknowledgementUserControl
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
			this.REXAcknowledgementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NoticeIDGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.REXAcknowledgementGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NoticeIDGrid)).BeginInit();
			this.NoticeIDGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// REXAcknowledgementGroupBox
			// 
			this.REXAcknowledgementGroupBox.Controls.Add(this.NoticeIDGrid);
			this.REXAcknowledgementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.REXAcknowledgementGroupBox.Name = "REXAcknowledgementGroupBox";
			this.REXAcknowledgementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 291, true);
			this.REXAcknowledgementGroupBox.TabIndex = 1;
			this.REXAcknowledgementGroupBox.TabStop = false;
			// 
			// NoticeIDGrid
			// 
			this.NoticeIDGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NoticeIDGrid, "QuarantineExDocHeader+Acknowledgements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Acknowledgements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocRexAcknowledgement)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Acknowledgements)).SyncRoot)).CY_Data)));
			this.NoticeIDGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnComparer = null;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.NoticeIDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NoticeIDGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NoticeIDGrid.GridId = "bd42140b-78a8-4091-9e0e-5a92d883a83a";
			this.NoticeIDGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NoticeIDGrid.LayoutKey = "ShipsCompartmentsInspectionsGrid";
			this.NoticeIDGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.NoticeIDGrid.Name = "NoticeIDGrid";
			this.NoticeIDGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 274, true);
			this.NoticeIDGrid.TabIndex = 0;
			// 
			// REXAcknowledgementUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.REXAcknowledgementGroupBox);
			this.Name = "REXAcknowledgementUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 303, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.REXAcknowledgementGroupBox.ResumeLayout(false);
			this.REXAcknowledgementGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NoticeIDGrid)).EndInit();
			this.NoticeIDGrid.ResumeLayout(false);
			this.NoticeIDGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox REXAcknowledgementGroupBox;
		private ZArchitecture.ZGrid NoticeIDGrid;
	}
}
