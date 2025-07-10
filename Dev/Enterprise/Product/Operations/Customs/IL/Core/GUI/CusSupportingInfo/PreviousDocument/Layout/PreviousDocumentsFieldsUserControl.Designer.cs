namespace Enterprise.Customs.IL.GUI
{
	partial class PreviousDocumentsFieldsUserControl
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
			this.CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CodeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.PreviousDocument);
			// 
			// CodeCodeFindBox
			// 
			this.CodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CodeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.PreviousDocument)(null)).CSI_Code)));
			this.CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 19, true);
			this.CodeCodeFindBox.Name = "CodeCodeFindBox";
			this.CodeCodeFindBox.ParentType = null;
			this.CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.CodeCodeFindBox.TabIndex = 0;
			// 
			// ReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.PreviousDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 45, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.ReferenceTextBox.TabIndex = 0;
			// 
			// PreviousDocumentsFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeCodeFindBox);
			this.Controls.Add(this.ReferenceTextBox);
			this.Name = "PreviousDocumentsFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 368, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CodeCodeFindBox.ResumeLayout(true);
			this.CodeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CodeCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox ReferenceTextBox;
	}
}
