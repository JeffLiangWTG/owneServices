namespace Enterprise.Customs.CA.GUI
{
	partial class LVSLineAllDetailsUserControl
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
			this.DetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LVSLineDetailsUserControl = new Enterprise.Customs.CA.GUI.LVSLineDetailsUserControl();
			this.LVSDutiesAndTaxesUserControl = new Enterprise.Customs.CA.GUI.LVSDutiesAndTaxesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsSplitContainer.Panel1.SuspendLayout();
			this.DetailsSplitContainer.Panel2.SuspendLayout();
			this.DetailsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobComInvoiceLine);
			// 
			// DetailsSplitContainer
			// 
			this.DetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsSplitContainer.Name = "DetailsSplitContainer";
			this.DetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 194, true);
			// 
			// DetailsSplitContainer.Panel1
			// 
			this.DetailsSplitContainer.Panel1.Controls.Add(this.LVSLineDetailsUserControl);
			this.DetailsSplitContainer.Panel1MinSize = 455;
			// 
			// DetailsSplitContainer.Panel2
			// 
			this.DetailsSplitContainer.Panel2.Controls.Add(this.LVSDutiesAndTaxesUserControl);
			this.DetailsSplitContainer.Panel2MinSize = 500;
			this.DetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(477);
			this.DetailsSplitContainer.SplitterWidth = 2;
			this.DetailsSplitContainer.TabIndex = 8;
			// 
			// LVSLineDetailsUserControl
			// 
			this.LVSLineDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LVSLineDetailsUserControl, ".");
			this.LVSLineDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSLineDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LVSLineDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 192, true);
			this.LVSLineDetailsUserControl.Name = "LVSLineDetailsUserControl";
			this.LVSLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 194, true);
			this.LVSLineDetailsUserControl.TabIndex = 0;
			// 
			// LVSDutiesAndTaxesUserControl
			// 
			this.LVSDutiesAndTaxesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LVSDutiesAndTaxesUserControl, ".");
			this.LVSDutiesAndTaxesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSDutiesAndTaxesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LVSDutiesAndTaxesUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 192, true);
			this.LVSDutiesAndTaxesUserControl.Name = "LVSDutiesAndTaxesUserControl";
			this.LVSDutiesAndTaxesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 194, true);
			this.LVSDutiesAndTaxesUserControl.TabIndex = 0;
			// 
			// LVSLineAllDetailsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsSplitContainer);
			this.Name = "LVSLineAllDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 194, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsSplitContainer.Panel1.ResumeLayout(false);
			this.DetailsSplitContainer.Panel2.ResumeLayout(false);
			this.DetailsSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer DetailsSplitContainer;
		internal LVSLineDetailsUserControl LVSLineDetailsUserControl;
		private LVSDutiesAndTaxesUserControl LVSDutiesAndTaxesUserControl;
	}
}
