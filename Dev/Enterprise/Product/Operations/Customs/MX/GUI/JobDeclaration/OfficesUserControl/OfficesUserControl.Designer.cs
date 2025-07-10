namespace Enterprise.Customs.MX.GUI
{
	partial class CustomsAreaUserControl
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
			this.EntryOrExitAreaFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ClearanceAreaFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryOrExitAreaFindBox.SuspendLayout();
			this.ClearanceAreaFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Business.JobDeclaration);
			// 
			// EntryOrExitAreaFindBox
			// 
			this.EntryOrExitAreaFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryOrExitAreaFindBox, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.MX.Business.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.EntryOrExitAreaFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 12, true);
			this.EntryOrExitAreaFindBox.Name = "EntryOrExitAreaFindBox";
			this.EntryOrExitAreaFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 20, true);
			this.EntryOrExitAreaFindBox.TabIndex = 1;
			// 
			// ClearanceAreaFindBox
			// 
			this.ClearanceAreaFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClearanceAreaFindBox, "JE_SubLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.MX.Business.JobDeclaration)(null)).JE_SubLocationOfGoods)));
			this.ClearanceAreaFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 36, true);
			this.ClearanceAreaFindBox.Name = "ClearanceAreaFindBox";
			this.ClearanceAreaFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 20, true);
			this.ClearanceAreaFindBox.TabIndex = 2;
			// 
			// CustomsAreaUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryOrExitAreaFindBox);
			this.Controls.Add(this.ClearanceAreaFindBox);
			this.Name = "CustomsAreaUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 73, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryOrExitAreaFindBox.ResumeLayout(true);
			this.EntryOrExitAreaFindBox.PerformLayout();
			this.ClearanceAreaFindBox.ResumeLayout(true);
			this.ClearanceAreaFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZCodeFindBox EntryOrExitAreaFindBox;
		public ZArchitecture.GUI.ZCodeFindBox ClearanceAreaFindBox;
	}
}
