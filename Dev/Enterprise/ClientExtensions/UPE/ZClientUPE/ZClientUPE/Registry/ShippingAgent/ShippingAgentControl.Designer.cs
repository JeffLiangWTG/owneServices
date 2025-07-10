namespace Enterprise.Client.UPE.Registry.GUI
{
	partial class ShippingAgentControl
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
			this.ShippingAgentZAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShippingAgentZAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Registry.Business.ShippingAgentObject);
			// 
			// ShippingAgentZAddressControl
			// 
			this.ShippingAgentZAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingAgentZAddressControl, "Wrapper.ShippingAgentAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.Registry.Business.ShippingAgentObject)(null)).Wrapper.ShippingAgentAddress)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShippingAgentZAddressControl, false);
			this.ShippingAgentZAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ShippingAgentZAddressControl.Name = "ShippingAgentZAddressControl";
			this.ShippingAgentZAddressControl.PopupCaption = "";
			this.ShippingAgentZAddressControl.ReadOnly = false;
			this.ShippingAgentZAddressControl.ShowAddress = false;
			this.ShippingAgentZAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ShippingAgentZAddressControl.TabIndex = 0;
			// 
			// ShippingAgentControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ShippingAgentZAddressControl);
			this.Name = "ShippingAgentControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 27, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShippingAgentZAddressControl.ResumeLayout(true);
			this.ShippingAgentZAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZAddressControl ShippingAgentZAddressControl;

		#endregion
	}
}
