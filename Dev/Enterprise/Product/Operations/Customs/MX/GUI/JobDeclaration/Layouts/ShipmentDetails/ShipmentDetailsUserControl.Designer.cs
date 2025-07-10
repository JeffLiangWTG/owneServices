namespace Enterprise.Customs.MX.GUI
{
	partial class ShipmentDetailsUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShipmentDetailsUserControl));
			this.GoodsOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsOriginDropEdit.SuspendLayout();
			this.GoodsDestinationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Business.JobDeclaration);
			// 
			// GoodsOriginDropEdit
			// 
			this.GoodsOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginDropEdit, "JE_GoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.MX.Business.JobDeclaration)(null)).JE_GoodsOrigin)));
			this.GoodsOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.GoodsOriginDropEdit.Name = "GoodsOriginDropEdit";
			this.GoodsOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.GoodsOriginDropEdit.TabIndex = 20;
			// 
			// GoodsDestinationDropEdit
			// 
			this.GoodsDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsDestinationDropEdit, "JE_GoodsDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.MX.Business.JobDeclaration)(null)).JE_GoodsDestination)));
			this.GoodsDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 43, true);
			this.GoodsDestinationDropEdit.Name = "GoodsDestinationDropEdit";
			this.GoodsDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.GoodsDestinationDropEdit.TabIndex = 21;
			// 
			// ShipmentDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GoodsOriginDropEdit);
			this.Controls.Add(this.GoodsDestinationDropEdit);
			this.Name = "ShipmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 68, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsDestinationDropEdit.ResumeLayout(true);
			this.GoodsDestinationDropEdit.PerformLayout();
			this.GoodsOriginDropEdit.ResumeLayout(true);
			this.GoodsOriginDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDropEdit GoodsOriginDropEdit;
		public ZArchitecture.GUI.ZDropEdit GoodsDestinationDropEdit;
	}
}
