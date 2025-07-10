namespace Enterprise.StlAnalysis.Load
{
	partial class STLAnalysisForm
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
		private void InitializeComponent()
		{
			this.tbcMainTabControl = new System.Windows.Forms.TabControl();
			this.tbpEtlTabPage = new System.Windows.Forms.TabPage();
			this.uscLoadUserControl = new Enterprise.StlAnalysis.Load.EtlUserControl();
			this.tbpEditFeaturesTabPage = new System.Windows.Forms.TabPage();
			this.uscEditFeatureControl = new Enterprise.StlAnalysis.Load.EditFeatureControl();
			this.tbpLoadFromCsvTabPage = new System.Windows.Forms.TabPage();
			this.uscLoadFromCsvControl = new Enterprise.StlAnalysis.Load.LoadFromCsvControl();
			this.tbpAddHocConsultationTabPage = new System.Windows.Forms.TabPage();
			this.uscAdHocConsultationControl = new Enterprise.StlAnalysis.Load.AdHocConsultationControl();
			this.tbcMainTabControl.SuspendLayout();
			this.tbpEtlTabPage.SuspendLayout();
			this.tbpEditFeaturesTabPage.SuspendLayout();
			this.tbpLoadFromCsvTabPage.SuspendLayout();
			this.tbpAddHocConsultationTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// tbcMainTabControl
			// 
			this.tbcMainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbcMainTabControl.Controls.Add(this.tbpEtlTabPage);
			this.tbcMainTabControl.Controls.Add(this.tbpEditFeaturesTabPage);
			this.tbcMainTabControl.Controls.Add(this.tbpLoadFromCsvTabPage);
			this.tbcMainTabControl.Controls.Add(this.tbpAddHocConsultationTabPage);
			this.tbcMainTabControl.Location = new System.Drawing.Point(12, 12);
			this.tbcMainTabControl.Name = "tbcMainTabControl";
			this.tbcMainTabControl.SelectedIndex = 0;
			this.tbcMainTabControl.Size = new System.Drawing.Size(960, 717);
			this.tbcMainTabControl.TabIndex = 5;
			// 
			// tbpEtlTabPage
			// 
			this.tbpEtlTabPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.tbpEtlTabPage.Controls.Add(this.uscLoadUserControl);
			this.tbpEtlTabPage.Location = new System.Drawing.Point(4, 22);
			this.tbpEtlTabPage.Name = "tbpEtlTabPage";
			this.tbpEtlTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.tbpEtlTabPage.Size = new System.Drawing.Size(952, 691);
			this.tbpEtlTabPage.TabIndex = 2;
			this.tbpEtlTabPage.Text = "Load Transaction DW";
			// 
			// uscLoadUserControl
			// 
			this.uscLoadUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.uscLoadUserControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.uscLoadUserControl.Location = new System.Drawing.Point(6, 6);
			this.uscLoadUserControl.Name = "uscLoadUserControl";
			this.uscLoadUserControl.Size = new System.Drawing.Size(940, 679);
			this.uscLoadUserControl.TabIndex = 0;
			// 
			// tbpEditFeaturesTabPage
			// 
			this.tbpEditFeaturesTabPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.tbpEditFeaturesTabPage.Controls.Add(this.uscEditFeatureControl);
			this.tbpEditFeaturesTabPage.Location = new System.Drawing.Point(4, 22);
			this.tbpEditFeaturesTabPage.Name = "tbpEditFeaturesTabPage";
			this.tbpEditFeaturesTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.tbpEditFeaturesTabPage.Size = new System.Drawing.Size(952, 691);
			this.tbpEditFeaturesTabPage.TabIndex = 1;
			this.tbpEditFeaturesTabPage.Text = "Edit Features";
			// 
			// uscEditFeatureControl
			// 
			this.uscEditFeatureControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.uscEditFeatureControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.uscEditFeatureControl.Location = new System.Drawing.Point(6, 6);
			this.uscEditFeatureControl.Name = "uscEditFeatureControl";
			this.uscEditFeatureControl.Size = new System.Drawing.Size(940, 699);
			this.uscEditFeatureControl.TabIndex = 0;
			// 
			// tbpLoadFromCsvTabPage
			// 
			this.tbpLoadFromCsvTabPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.tbpLoadFromCsvTabPage.Controls.Add(this.uscLoadFromCsvControl);
			this.tbpLoadFromCsvTabPage.Location = new System.Drawing.Point(4, 22);
			this.tbpLoadFromCsvTabPage.Name = "tbpLoadFromCsvTabPage";
			this.tbpLoadFromCsvTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.tbpLoadFromCsvTabPage.Size = new System.Drawing.Size(952, 691);
			this.tbpLoadFromCsvTabPage.TabIndex = 3;
			this.tbpLoadFromCsvTabPage.Text = "Load From CSV";
			// 
			// uscLoadFromCsvControl
			// 
			this.uscLoadFromCsvControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.uscLoadFromCsvControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.uscLoadFromCsvControl.Location = new System.Drawing.Point(6, 6);
			this.uscLoadFromCsvControl.Name = "uscLoadFromCsvControl";
			this.uscLoadFromCsvControl.Size = new System.Drawing.Size(940, 699);
			this.uscLoadFromCsvControl.TabIndex = 0;
			// 
			// tbpAddHocConsultationTabPage
			// 
			this.tbpAddHocConsultationTabPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.tbpAddHocConsultationTabPage.Controls.Add(this.uscAdHocConsultationControl);
			this.tbpAddHocConsultationTabPage.Location = new System.Drawing.Point(4, 22);
			this.tbpAddHocConsultationTabPage.Name = "tbpAddHocConsultationTabPage";
			this.tbpAddHocConsultationTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.tbpAddHocConsultationTabPage.Size = new System.Drawing.Size(952, 691);
			this.tbpAddHocConsultationTabPage.TabIndex = 0;
			this.tbpAddHocConsultationTabPage.Text = "Add Hoc Consultation";
			// 
			// uscAdHocConsultationControl
			// 
			this.uscAdHocConsultationControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.uscAdHocConsultationControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.uscAdHocConsultationControl.Location = new System.Drawing.Point(6, 6);
			this.uscAdHocConsultationControl.Name = "uscAdHocConsultationControl";
			this.uscAdHocConsultationControl.Size = new System.Drawing.Size(940, 699);
			this.uscAdHocConsultationControl.TabIndex = 0;
			// 
			// STLAnalysisForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.ClientSize = new System.Drawing.Size(984, 741);
			this.Controls.Add(this.tbcMainTabControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(900, 700);
			this.Name = "STLAnalysisForm";
			this.Text = "Seat + Transaction Billing Analysis";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.STLAnalysisForm_FormClosing);
			this.Load += new System.EventHandler(this.STLAnalysisForm_Load);
			this.tbcMainTabControl.ResumeLayout(false);
			this.tbpEtlTabPage.ResumeLayout(false);
			this.tbpEditFeaturesTabPage.ResumeLayout(false);
			this.tbpLoadFromCsvTabPage.ResumeLayout(false);
			this.tbpAddHocConsultationTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tbcMainTabControl;
		private System.Windows.Forms.TabPage tbpAddHocConsultationTabPage;
		private System.Windows.Forms.TabPage tbpEditFeaturesTabPage;
		private System.Windows.Forms.TabPage tbpEtlTabPage;
		private EditFeatureControl uscEditFeatureControl;
		private EtlUserControl uscLoadUserControl;
		private AdHocConsultationControl uscAdHocConsultationControl;
		private System.Windows.Forms.TabPage tbpLoadFromCsvTabPage;
		private LoadFromCsvControl uscLoadFromCsvControl;

	}
}

