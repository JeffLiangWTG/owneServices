namespace Enterprise.Customs.ASYCUDA.Module
{
	partial class CountryRelatedFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Property2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Property2FindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Property2DropEdit.SuspendLayout();
			this.Property2FindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Module.CountryRelatedFilter);
			// 
			// Property2DropEdit
			// 
			this.Property2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Property2DropEdit, "Property2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Module.CountryRelatedFilter)(null)).Property2)));
			this.Property2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 1, true);
			this.Property2DropEdit.Name = "Property2DropEdit";
			this.Property2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.Property2DropEdit.TabIndex = 2;
			this.Property2DropEdit.Visible = false;
			// 
			// Property2FindBox
			// 
			this.Property2FindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Property2FindBox, "Property2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Module.CountryRelatedFilter)(null)).Property2)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.Property2FindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.Property2FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 1, true);
			this.Property2FindBox.Name = "Property2FindBox";
			this.Property2FindBox.ShouldResize = true;
			this.Property2FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.Property2FindBox.TabIndex = 3;
			this.Property2FindBox.Visible = false;
			// 
			// CountryRelatedFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Property2FindBox);
			this.Controls.Add(this.Property2DropEdit);
			this.Name = "CountryRelatedFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Property2DropEdit.ResumeLayout(true);
			this.Property2DropEdit.PerformLayout();
			this.Property2FindBox.ResumeLayout(true);
			this.Property2FindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit Property2DropEdit;
		private ZArchitecture.GUI.ZCodeFindBox Property2FindBox;
	}
}
