namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class ExampleAdditionalControlTemplate
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
			this.ManifestTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MasterBOLTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManifestTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader);
			// 
			// ManifestTypeDropEdit
			// 
			this.ManifestTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManifestTypeDropEdit, "AMA_ManifestType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_ManifestType)));
			this.ManifestTypeDropEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("71ee64dd-7a49-4317-8c78-5ca8c3e2bdaa", "Manifest Type (2)");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ManifestTypeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.ManifestTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 3, true);
			this.ManifestTypeDropEdit.Name = "ManifestTypeDropEdit";
			this.ManifestTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ManifestTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 17, true);
			this.ManifestTypeDropEdit.TabIndex = 0;
			// 
			// MasterBOLTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterBOLTextBox, "MasterBOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).MasterBOL)));
			this.MasterBOLTextBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("36ef1bc9-f07b-445c-9b08-d153d662a0af", "Master (2)");
			this.MasterBOLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 24, true);
			this.MasterBOLTextBox.Name = "MasterBOLTextBox";
			this.MasterBOLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 17, true);
			this.MasterBOLTextBox.TabIndex = 5;
			// 
			// ExampleAdditionalControlTemplate
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManifestTypeDropEdit);
			this.Controls.Add(this.MasterBOLTextBox);
			this.Name = "ExampleAdditionalControlTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 410, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManifestTypeDropEdit.ResumeLayout(true);
			this.ManifestTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox MasterBOLTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ManifestTypeDropEdit;
	}
}
