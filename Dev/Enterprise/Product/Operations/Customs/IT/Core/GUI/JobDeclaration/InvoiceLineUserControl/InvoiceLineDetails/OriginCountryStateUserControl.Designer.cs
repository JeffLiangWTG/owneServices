namespace Enterprise.Customs.IT.GUI
{
	partial class OriginCountryStateUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OriginStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OriginStateDropEdit.SuspendLayout();
			this.GoodsOriginDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine);
			// 
			// OriginStateDropEdit
			// 
			this.OriginStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginStateDropEdit, "JI_StateOrRegionOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(null)).JI_StateOrRegionOfOrigin)));
			this.OriginStateDropEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OriginStateDropEdit, false);
			this.OriginStateDropEdit.Dock = System.Windows.Forms.DockStyle.Right;
			this.OriginStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(213, 0, true);
			this.OriginStateDropEdit.Name = "OriginStateDropEdit";
			this.OriginStateDropEdit.PreBoundMaxLength = 4;
			this.OriginStateDropEdit.ShowDescriptionBox = false;
			this.OriginStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 21, true);
			this.OriginStateDropEdit.TabIndex = 26;
			// 
			// GoodsOriginDropEdit
			// 
			this.GoodsOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginDropEdit, "JI_CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(null)).JI_CountryOfOrigin)));
			this.GoodsOriginDropEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GoodsOriginDropEdit, false);
			this.GoodsOriginDropEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsOriginDropEdit.Name = "GoodsOriginDropEdit";
			this.GoodsOriginDropEdit.PreBoundMaxLength = 3;
			this.GoodsOriginDropEdit.ShouldResizeByMaxLength = false;
			this.GoodsOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 21, true);
			this.GoodsOriginDropEdit.TabIndex = 25;
			// 
			// OriginCountryStateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OriginStateDropEdit);
			this.Controls.Add(this.GoodsOriginDropEdit);
			this.Name = "OriginCountryStateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OriginStateDropEdit.ResumeLayout(true);
			this.OriginStateDropEdit.PerformLayout();
			this.GoodsOriginDropEdit.ResumeLayout(true);
			this.GoodsOriginDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZDropEdit GoodsOriginDropEdit;
		internal ZArchitecture.GUI.ZDropEdit OriginStateDropEdit;
	}
}
