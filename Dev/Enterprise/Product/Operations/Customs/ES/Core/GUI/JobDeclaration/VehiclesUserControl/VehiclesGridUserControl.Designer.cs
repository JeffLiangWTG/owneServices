using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI
{
	partial class VehiclesGridUserControl
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
			this.VehiclesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VehiclesGrid)).BeginInit();
			this.VehiclesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ICusVehicleCollection<CusVehicle, JobComInvoiceLine>);

			this.VehiclesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.VehiclesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusVehicle)(null)))));
			this.VehiclesGrid.CaptionVisible = false;
			this.VehiclesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VehiclesGrid.GridId = "7922BB5C-AF2F-4337-AD4D-AE4507F2B368";
			this.VehiclesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VehiclesGrid.LayoutKey = "VehiclesGrid";
			this.VehiclesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VehiclesGrid.Name = "VehiclesGrid";
			this.VehiclesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 30, true);
			this.VehiclesGrid.TabIndex = 1;
			// 
			// VehiclesGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VehiclesGrid);
			this.Name = "VehiclesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VehiclesGrid)).EndInit();
			this.VehiclesGrid.ResumeLayout(false);
			this.VehiclesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal Enterprise.ZArchitecture.ZGrid VehiclesGrid;
	}
}
