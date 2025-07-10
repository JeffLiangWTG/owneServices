namespace Enterprise.Customs.IT.GUI
{
	partial class GoodsLocationFUserControl
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
            this.LocationQualifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.GoodsLocationAddressZDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.LocationQualifierDropEdit.SuspendLayout();
            this.GoodsLocationAddressZDocAddressControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
            // 
            // LocationQualifierDropEdit
            // 
            this.LocationQualifierDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LocationQualifierDropEdit, "JE_LocationQualifier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_LocationQualifier)));
            this.LocationQualifierDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("14D8651B-43AE-435C-ACFC-BAD790F9571B", "[30] Goods Location");
            this.LocationQualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.LocationQualifierDropEdit.Name = "LocationQualifierDropEdit";
            this.LocationQualifierDropEdit.PreBoundMaxLength = 2;
            this.LocationQualifierDropEdit.ShowDescriptionBox = false;
            this.LocationQualifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
            this.LocationQualifierDropEdit.TabIndex = 0;
            // 
            // GoodsLocationAddressZDocAddressControl
            // 
            this.GoodsLocationAddressZDocAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GoodsLocationAddressZDocAddressControl, "GoodsLocationAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).GoodsLocationAddress)));
            this.GoodsLocationAddressZDocAddressControl.BindToOrganisations = "Lookups+Organisations";
            this.GoodsLocationAddressZDocAddressControl.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("81CD2D47-EC79-4D48-A3A2-6431D4303192", "Organization Address");
            this.GoodsLocationAddressZDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
            this.GoodsLocationAddressZDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 0, true);
            this.GoodsLocationAddressZDocAddressControl.Name = "GoodsLocationAddressZDocAddressControl";
            this.GoodsLocationAddressZDocAddressControl.ReadOnly = false;
            this.GoodsLocationAddressZDocAddressControl.SingleLineNoGroupBoxPanelWidth = 280;
            this.GoodsLocationAddressZDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.GoodsLocationAddressZDocAddressControl.TabIndex = 1;
            this.GoodsLocationAddressZDocAddressControl.ValidationJustForced = false;
            // 
            // GoodsLocationFUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.LocationQualifierDropEdit);
            this.Controls.Add(this.GoodsLocationAddressZDocAddressControl);
            this.Name = "GoodsLocationFUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 23, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.LocationQualifierDropEdit.ResumeLayout(true);
            this.LocationQualifierDropEdit.PerformLayout();
            this.GoodsLocationAddressZDocAddressControl.ResumeLayout(true);
            this.GoodsLocationAddressZDocAddressControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit LocationQualifierDropEdit;
		internal MasterFiles.GUI.ZDocAddressControl GoodsLocationAddressZDocAddressControl;
	}
}
