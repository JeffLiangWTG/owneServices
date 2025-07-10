using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI
{
	partial class VehiclesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
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

		private void InitializeComponent()
		{
			this.VehiclesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.VehiclesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicVehicleDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VehiclesSplitContainer.Panel1.SuspendLayout();
			this.VehiclesSplitContainer.Panel2.SuspendLayout();
			this.VehiclesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.VehiclesSplitContainer)).BeginInit();
			this.VehiclesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ICusVehicleCollection<CusVehicle, JobComInvoiceLine>);
			// 
			// VehiclesSplitContainer
			// 
			this.VehiclesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VehiclesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VehiclesSplitContainer.Name = "VehiclesSplitContainer";
			this.VehiclesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.VehiclesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 389, true);
			// 
			// VehiclesSplitContainer.Panel1
			// 
			this.VehiclesSplitContainer.Panel1.Name = "VehiclesGridPanel";
			// 
			// VehiclesSplitContainer.Panel2
			// 
			this.VehiclesSplitContainer.Panel2.Controls.Add(this.VehiclesGroupBox);
			this.VehiclesSplitContainer.Panel2.Name = "VehicleDetailsPanel";
			this.VehiclesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(172);
			this.VehiclesSplitContainer.TabIndex = 0;

			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusVehicle)(null)))));
			// 
			// VehiclesGroupBox
			// 
			this.VehiclesGroupBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("FAF1B0E7-9950-4622-BD83-BF8FD19429AB", "Vehicle");
			this.VehiclesGroupBox.Controls.Add(this.DynamicVehicleDetailsPanel);
			this.VehiclesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VehiclesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.VehiclesGroupBox.Name = "VehiclesGroupBox";
			this.VehiclesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 140, true);
			this.VehiclesGroupBox.TabIndex = 0;
			this.VehiclesGroupBox.TabStop = false;
			// 
			// DynamicVehicleDetailsPanel
			// 
			this.DynamicVehicleDetailsPanel.AllowDrop = true;
			this.DynamicVehicleDetailsPanel.AutoScroll = true;
			this.DynamicVehicleDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicVehicleDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.DynamicVehicleDetailsPanel.Name = "DynamicVehicleDetailsPanel";
			this.DynamicVehicleDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 125, true);
			this.DynamicVehicleDetailsPanel.TabIndex = 0;
			// 
			// VehiclesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VehiclesSplitContainer);
			this.Name = "VehiclesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VehiclesSplitContainer.Panel1.ResumeLayout(false);
			this.VehiclesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.VehiclesSplitContainer)).EndInit();
			this.VehiclesSplitContainer.ResumeLayout(false);
			this.VehiclesSplitContainer.PerformLayout();
			this.VehiclesGroupBox.ResumeLayout(false);
			this.VehiclesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal CargoWise.Windows.UI.KSplitContainer VehiclesSplitContainer;
		private ZArchitecture.GUI.ZGroupBox VehiclesGroupBox;
		internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicVehicleDetailsPanel;
	}
}
