using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5ArrivalContainersAndSealsUserControl
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
			this.ContainersEquipmentAndSealsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainersEquipmentAndSealsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainersEquipmentAndSealsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersEquipmentAndSealsSplitContainer)).BeginInit();
			this.ContainersEquipmentAndSealsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// ContainersEquipmentAndSealsGroupBox
			// 
			this.ContainersEquipmentAndSealsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("5dcfaa07-3c80-4a4c-b64a-87012c11df5c", "Containers/Equipment and Seals");
			this.ContainersEquipmentAndSealsGroupBox.Controls.Add(this.ContainersEquipmentAndSealsSplitContainer);
			this.ContainersEquipmentAndSealsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersEquipmentAndSealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersEquipmentAndSealsGroupBox.Name = "ContainersEquipmentAndSealsGroupBox";
			this.ContainersEquipmentAndSealsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 5, 0, 0, true);
			this.ContainersEquipmentAndSealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 809, true);
			this.ContainersEquipmentAndSealsGroupBox.TabIndex = 0;
			this.ContainersEquipmentAndSealsGroupBox.TabStop = false;
			// 
			// ContainersEquipmentAndSealsSplitContainer
			// 
			this.ContainersEquipmentAndSealsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersEquipmentAndSealsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 29, true);
			this.ContainersEquipmentAndSealsSplitContainer.Name = "ContainersEquipmentAndSealsSplitContainer";
			this.ContainersEquipmentAndSealsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.ContainersEquipmentAndSealsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 780, true);
			this.ContainersEquipmentAndSealsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(389);
			this.ContainersEquipmentAndSealsSplitContainer.TabIndex = 0;
			// 
			// Phase5ArrivalContainersAndSealsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("4c6687a0-ebc5-422c-8aae-119b8606f1b2", "Container/Equipment and Seals");
			this.Controls.Add(this.ContainersEquipmentAndSealsGroupBox);
			this.Name = "Phase5ArrivalContainersAndSealsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 809, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainersEquipmentAndSealsGroupBox.ResumeLayout(false);
			this.ContainersEquipmentAndSealsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersEquipmentAndSealsSplitContainer)).EndInit();
			this.ContainersEquipmentAndSealsSplitContainer.ResumeLayout(false);
			this.ContainersEquipmentAndSealsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZGroupBox ContainersEquipmentAndSealsGroupBox;
		internal CargoWise.Windows.UI.KSplitContainer ContainersEquipmentAndSealsSplitContainer;
	}
}
