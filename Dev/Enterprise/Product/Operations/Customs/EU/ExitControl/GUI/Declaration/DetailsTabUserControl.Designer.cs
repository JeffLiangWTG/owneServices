using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class DetailsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HeaderOrganisationDetailsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).BeginInit();
			this.DetailsSplitContainer.Panel1.SuspendLayout();
			this.DetailsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitHeader);
			// 
			// DetailsSplitContainer
			// 
			this.DetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsSplitContainer.Name = "DetailsSplitContainer";
			// 
			// DetailsSplitContainer.Panel1
			// 
			this.DetailsSplitContainer.Panel1.Controls.Add(this.HeaderOrganisationDetailsDynamicLayoutPanel);
			this.DetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1450, 748, true);
			this.DetailsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.DetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(480);
			this.DetailsSplitContainer.TabIndex = 0;
			// 
			// HeaderOrganisationDetailsDynamicLayoutPanel
			// 
			this.HeaderOrganisationDetailsDynamicLayoutPanel.AllowDrop = true;
			this.HeaderOrganisationDetailsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderOrganisationDetailsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderOrganisationDetailsDynamicLayoutPanel.Name = "HeaderOrganisationDetailsDynamicLayoutPanel";
			this.HeaderOrganisationDetailsDynamicLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.HeaderOrganisationDetailsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 748, true);
			this.HeaderOrganisationDetailsDynamicLayoutPanel.TabIndex = 0;
			// 
			// DetailsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsSplitContainer);
			this.Name = "DetailsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1450, 748, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).EndInit();
			this.DetailsSplitContainer.ResumeLayout(false);
			this.DetailsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer DetailsSplitContainer;
		internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel HeaderOrganisationDetailsDynamicLayoutPanel;
	}
}
