using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.GUI
{
	public partial class SupplementaryCode1AndGDMUserControl
	{
		private void InitializeComponent()
		{
			this.GDMLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.SuppCode1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuppCode1DropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine);
			// 
			// GDMLink
			// 
			this.GDMLink.AutoSize = true;
			this.GDMLink.IsFontBold = false;
			this.GDMLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 3, true);
			this.GDMLink.Name = "GDMLink";
			this.GDMLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.GDMLink.TabIndex = 1;
			this.GDMLink.Text = Enterprise.Customs.EU.GUI.Res.GetString("396d2736-fcbd-4d32-bcf4-54d0f630daf2", "GDM");
			this.GDMLink.Click += new System.EventHandler(this.GDMLink_Clicked);
			// 
			// SuppCode1DropEdit
			// 
			this.SuppCode1DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SuppCode1DropEdit, "JI_SupplementaryCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_SupplementaryCode1)));
			this.SuppCode1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SuppCode1DropEdit.Name = "SuppCode1DropEdit";
			this.SuppCode1DropEdit.PreBoundMaxLength = 3;
			this.SuppCode1DropEdit.ShouldResizeByMaxLength = false;
			this.SuppCode1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.SuppCode1DropEdit.TabIndex = 2;
			// 
			// SupplementaryCode1AndGDMUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SuppCode1DropEdit);
			this.Controls.Add(this.GDMLink);
			this.Name = "SupplementaryCode1AndGDMUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SuppCode1DropEdit.ResumeLayout(true);
			this.SuppCode1DropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		Enterprise.ZArchitecture.GUI.ZLinkLabel GDMLink;
		private ZArchitecture.GUI.ZDropEdit SuppCode1DropEdit;
	}
}
