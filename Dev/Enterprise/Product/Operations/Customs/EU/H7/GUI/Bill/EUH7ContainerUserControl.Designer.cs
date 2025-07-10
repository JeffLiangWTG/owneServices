using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7ContainerUserControl
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
		void InitializeComponent()
		{
			this.ContainerNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerMode = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainerMode.SuspendLayout();
			this.ContainerNumber.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AsycudaBill);
			// 
			// ContainerNumber
			// 
			this.BindingSource.SetBindingMember(this.ContainerNumber, "ContainerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AsycudaBill)(null)).ContainerNumber)));
			this.ContainerNumber.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ContainerNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerNumber.Name = "ContainerNumber";
			this.ContainerNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 15, true);
			this.ContainerNumber.CaptionResourceString = Res.GetData("a9e28c8b-947d-4091-8289-3591e40747be", "Container");
			this.ContainerNumber.TabIndex = 0;
			// 
			// ContainerMode
			// 
			this.BindingSource.SetBindingMember(this.ContainerMode, "ABL_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AsycudaBill)(null)).ABL_ContainerMode)));
			this.ContainerMode.Dock = System.Windows.Forms.DockStyle.Right;
			this.ContainerMode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 0, true);
			this.ContainerMode.Name = "ContainerMode";
			this.ContainerMode.ShowDescriptionBox = false;
			this.ContainerMode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.ContainerMode.TabIndex = 1;
			// 
			// EUH7ContainerUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainerNumber);
			this.Controls.Add(this.ContainerMode);
			this.Name = "EUH7ContainerUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainerMode.ResumeLayout(true);
			this.ContainerMode.PerformLayout();
			this.ContainerNumber.ResumeLayout(true);
			this.ContainerNumber.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.ZTextBox ContainerNumber;
		Enterprise.ZArchitecture.GUI.ZDropEdit ContainerMode;

		#endregion
	}
}
