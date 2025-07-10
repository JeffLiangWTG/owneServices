namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	partial class ESH7AsycudaBillControlTemplate
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
			this.DocumentationRequiredTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.G3LocalReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.G3MovementReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.H7MovementReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill);
			// 
			// DocumentationRequiredTextBox
			// 
			this.BindingSource.SetBindingMember(this.DocumentationRequiredTextBox, "DocumentationRequiredDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill)(null)).DocumentationRequiredDescription)));
			this.DocumentationRequiredTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DocumentationRequiredTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 11, true);
			this.DocumentationRequiredTextBox.Name = "DocumentationRequiredTextBox";
			this.DocumentationRequiredTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 16, true);
			this.DocumentationRequiredTextBox.TabIndex = 0;
			// 
			// G3LocalReferenceNumberTextBox
			//
			this.G3LocalReferenceNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.G3LocalReferenceNumberTextBox, "G3LocalReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill)(null)).G3LocalReferenceNumber)));
			this.G3LocalReferenceNumberTextBox.Name = "G3LocalReferenceNumberTextBox";
			//
			// G3MovementReferenceNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.G3MovementReferenceNumberTextBox, "G3MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill)(null)).G3MovementReferenceNumber)));
			this.G3MovementReferenceNumberTextBox.Name = "G3MovementReferenceNumberTextBox";
			//
			// H7MovementReferenceNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.H7MovementReferenceNumberTextBox, "H7MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill)(null)).H7MovementReferenceNumber)));
			this.H7MovementReferenceNumberTextBox.Name = "H7MovementReferenceNumberTextBox";
			// 
			// ESH7AsycudaBillControlTemplate
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DocumentationRequiredTextBox);
			this.Controls.Add(this.G3LocalReferenceNumberTextBox);
			this.Controls.Add(this.G3MovementReferenceNumberTextBox);
			this.Controls.Add(this.H7MovementReferenceNumberTextBox);
			this.Name = "ESH7AsycudaBillControlTemplate";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox DocumentationRequiredTextBox;
		internal ZArchitecture.ZTextBox G3LocalReferenceNumberTextBox;
		internal ZArchitecture.ZTextBox G3MovementReferenceNumberTextBox;
		internal ZArchitecture.ZTextBox H7MovementReferenceNumberTextBox;
	}
}
