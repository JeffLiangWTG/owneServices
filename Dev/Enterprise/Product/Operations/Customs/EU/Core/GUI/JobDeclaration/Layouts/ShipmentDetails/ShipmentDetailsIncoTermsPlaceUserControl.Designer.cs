namespace Enterprise.Customs.EU.GUI
{
	partial class ShipmentDetailsIncoTermsPlaceUserControl
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
			this.ShipmentIncoTermPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgreedPlaceCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AgreedPlaceCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// ShipmentIncoTermPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentIncoTermPlaceTextBox, "JE_ShipmentIncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_ShipmentIncoTermPlace)));
			this.ShipmentIncoTermPlaceTextBox.CaptionResourceString = null;
			this.ShipmentIncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentIncoTermPlaceTextBox.Name = "ShipmentIncoTermPlaceTextBox";
			this.ShipmentIncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 17, true);
			this.ShipmentIncoTermPlaceTextBox.TabIndex = 18;
			// 
			// AgreedPlaceCodeDropEdit
			// 
			this.AgreedPlaceCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgreedPlaceCodeDropEdit, "ZG_AgreedPlaceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_AgreedPlaceCode)));
			this.AgreedPlaceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 0, true);
			this.AgreedPlaceCodeDropEdit.Name = "AgreedPlaceCodeDropEdit";
			this.AgreedPlaceCodeDropEdit.PreBoundMaxLength = 1;
			this.AgreedPlaceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 17, true);
			this.AgreedPlaceCodeDropEdit.TabIndex = 19;
			// 
			// ShipmentDetailsIncoTermsPlaceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShipmentIncoTermPlaceTextBox);
			this.Controls.Add(this.AgreedPlaceCodeDropEdit);
			this.Name = "ShipmentDetailsIncoTermsPlaceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AgreedPlaceCodeDropEdit.ResumeLayout(true);
			this.AgreedPlaceCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox ShipmentIncoTermPlaceTextBox;
		internal ZArchitecture.GUI.ZDropEdit AgreedPlaceCodeDropEdit;
	}
}
