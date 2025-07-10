namespace Enterprise.Customs.DE.GUI
{
	partial class ShipmentTypeUserControl
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
			this.BorderTransportMeansDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BorderTransportMeansDropEdit.SuspendLayout();
			this.MethodOfPaymentDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// BorderTransportMeansDropEdit
			// 
			this.BorderTransportMeansDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderTransportMeansDropEdit, "ZG_BorderTransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).ZG_BorderTransportMeans)));
			this.BorderTransportMeansDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 43, true);
			this.BorderTransportMeansDropEdit.Name = "BorderTransportMeansDropEdit";
			this.BorderTransportMeansDropEdit.PreBoundMaxLength = 2;
			this.BorderTransportMeansDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.BorderTransportMeansDropEdit.TabIndex = 22;
			// 
			// MethodOfPaymentDropEdit
			// 
			this.MethodOfPaymentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MethodOfPaymentDropEdit, "ZG_MethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).ZG_MethodOfPayment)));
			this.MethodOfPaymentDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("081bfbd6-19d4-4795-8c31-34adb2207d7e", "Payment Method");
			this.MethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 16, true);
			this.MethodOfPaymentDropEdit.Name = "MethodOfPaymentDropEdit";
			this.MethodOfPaymentDropEdit.PreBoundMaxLength = 2;
			this.MethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.MethodOfPaymentDropEdit.TabIndex = 4;
			// 
			// ShipmentTypeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BorderTransportMeansDropEdit);
			this.Controls.Add(this.MethodOfPaymentDropEdit);
			this.Name = "ShipmentTypeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 84, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BorderTransportMeansDropEdit.ResumeLayout(true);
			this.BorderTransportMeansDropEdit.PerformLayout();
			this.MethodOfPaymentDropEdit.ResumeLayout(true);
			this.MethodOfPaymentDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZDropEdit BorderTransportMeansDropEdit;
		internal ZArchitecture.GUI.ZDropEdit MethodOfPaymentDropEdit;
	}
}
