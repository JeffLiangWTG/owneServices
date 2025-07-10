using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.ES.GUI
{
	partial class TransportDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MasterBillAndIATAUserControl = new MasterBillAndIATAUserControl();
			this.TransportInlandRailUserControl = new TransportInlandRailUserControl();
			//add rail
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MasterBillAndIATAUserControl.SuspendLayout();
			this.TransportInlandRailUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// MasterBillAndIATAUserControl
			// 
			this.MasterBillAndIATAUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MasterBillAndIATAUserControl, ".");
			this.MasterBillAndIATAUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 76, true);
			this.MasterBillAndIATAUserControl.Name = "MasterBillAndIATAUserControl";
			this.MasterBillAndIATAUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.MasterBillAndIATAUserControl.TabIndex = 0;
			// 
			// TransportInlandRailUserControl
			// 
			this.TransportInlandRailUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandRailUserControl, ".");
			this.TransportInlandRailUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 76, true);
			this.TransportInlandRailUserControl.Name = "TransportInlandRailUserControl";
			this.TransportInlandRailUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.TransportInlandRailUserControl.TabIndex = 1;
			// 
			// TransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MasterBillAndIATAUserControl);
			this.Controls.Add(this.TransportInlandRailUserControl);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 329, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MasterBillAndIATAUserControl.ResumeLayout(true);
			this.MasterBillAndIATAUserControl.PerformLayout();
			this.TransportInlandRailUserControl.ResumeLayout(true);
			this.TransportInlandRailUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal MasterBillAndIATAUserControl MasterBillAndIATAUserControl;
		internal TransportInlandRailUserControl TransportInlandRailUserControl;
	}
}
