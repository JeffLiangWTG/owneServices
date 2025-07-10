namespace Enterprise.Accounting.Module
{
	public partial class AccGLHeaderRangeFilterControl
	{
		Enterprise.ZArchitecture.GUI.ZCodeFindBox FromCode;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox ToCode;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FromCode = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ToCode = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Module.AccGLHeaderRangeFilter);
			//
			// FromCode
			//
			this.BindingSource.SetBindingMember(this.FromCode, "Property1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Module.AccGLHeaderRangeFilter)(null)).Property1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Module.AccGLHeaderRangeFilter)(null)).List1)));
			this.FromCode.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AccGLHeaderRangeFilterControl|8db2a13a-bef1-4c9d-bc09-9296542392d6", "From");
			this.FromCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 1, true);
			this.FromCode.Name = "FromCode";
			this.FromCode.ShowDescriptionBox = false;
			this.FromCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.FromCode.TabIndex = 0;
			//
			// ToCode
			//
			this.BindingSource.SetBindingMember(this.ToCode, "Property2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Module.AccGLHeaderRangeFilter)(null)).Property2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Module.AccGLHeaderRangeFilter)(null)).List2)));
			this.ToCode.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AccGLHeaderRangeFilterControl|52195764-b1ed-4881-ac43-6b242db01274", "To");
			this.ToCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 1, true);
			this.ToCode.Name = "ToCode";
			this.ToCode.ShowDescriptionBox = false;
			this.ToCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ToCode.TabIndex = 1;
			//
			// AccGLHeaderRangeFilterControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FromCode);
			this.Controls.Add(this.ToCode);
			this.Name = "AccGLHeaderRangeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
