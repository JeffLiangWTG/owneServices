using Enterprise.Customs.AE.Manifest.Business;

namespace Enterprise.Customs.AE.Manifest.GUI
{
	partial class ManifestLayoutsUserControl
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
			this.CarrierMPCITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShippingAgentMPCITextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AsycudaManifestHeader);
			// 
			// CarrierMPCITextBox
			// 
			this.BindingSource.SetBindingMember(this.CarrierMPCITextBox, "CarrierMPCI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Manifest.Business.AsycudaManifestHeader)(null)).CarrierMPCI)));
			this.CarrierMPCITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 15, true);
			this.CarrierMPCITextBox.Name = "CarrierMPCITextBox";
			this.CarrierMPCITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.CarrierMPCITextBox.TabIndex = 0;
			// 
			// ShippingAgentMPCITextBox
			// 
			this.BindingSource.SetBindingMember(this.ShippingAgentMPCITextBox, "ShippingAgentMPCI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Manifest.Business.AsycudaManifestHeader)(null)).ShippingAgentMPCI)));
			this.ShippingAgentMPCITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 53, true);
			this.ShippingAgentMPCITextBox.Name = "ShippingAgentMPCITextBox";
			this.ShippingAgentMPCITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.ShippingAgentMPCITextBox.TabIndex = 1;
			// 
			// ManifestLayoutsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShippingAgentMPCITextBox);
			this.Controls.Add(this.CarrierMPCITextBox);
			this.Name = "ManifestLayoutsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox CarrierMPCITextBox;
		internal ZArchitecture.ZTextBox ShippingAgentMPCITextBox;
	}
}
