using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ZEditAssayCodeForm
	{
		protected override void InitializeComponent()
		{
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.zS_AUBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zS_NIBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zS_SNBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zS_PTBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zS_PBBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zS_CUBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zS_AGBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zS_WOBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zS_ZNBoundCalcEditt = new Enterprise.ZArchitecture.ZCalcEdit();
			this.oKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 346, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.MinWidth = 0;
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine);
			// 
			// zLabel1
			// 
			this.zLabel1.BindTo = null;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 30, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "Gold (GPT) :";
			// 
			// zLabel2
			// 
			this.zLabel2.BindTo = null;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 62, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel2.TabIndex = 2;
			this.zLabel2.Text = "Silver (GPT) :";
			// 
			// zLabel3
			// 
			this.zLabel3.BindTo = null;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 94, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel3.TabIndex = 3;
			this.zLabel3.Text = "Copper (PER) :";
			// 
			// zLabel4
			// 
			this.zLabel4.BindTo = null;
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 126, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel4.TabIndex = 4;
			this.zLabel4.Text = "Lead (PER) :";
			// 
			// zLabel5
			// 
			this.zLabel5.BindTo = null;
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 158, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel5.TabIndex = 5;
			this.zLabel5.Text = "Platinum (PER) :";
			// 
			// zLabel6
			// 
			this.zLabel6.BindTo = null;
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 190, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel6.TabIndex = 6;
			this.zLabel6.Text = "Nickel (PER) :";
			// 
			// zLabel7
			// 
			this.zLabel7.BindTo = null;
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 222, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel7.TabIndex = 7;
			this.zLabel7.Text = "Tin (PER):";
			// 
			// zLabel8
			// 
			this.zLabel8.BindTo = null;
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 254, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel8.TabIndex = 8;
			this.zLabel8.Text = "Tungsten (PER):";
			// 
			// zLabel9
			// 
			this.zLabel9.BindTo = null;
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 286, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel9.TabIndex = 9;
			this.zLabel9.Text = "Zinc (PER):";
			// 
			// ZS_AUBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zS_AUBoundCalcEdit, "AddInfo+ZA_AssayAU_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AddInfo.ZA_AssayAU_Hidden)));
			this.zS_AUBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 31, true);
			this.zS_AUBoundCalcEdit.Name = "ZS_AUBoundCalcEdit";
			this.zS_AUBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zS_AUBoundCalcEdit.TabIndex = 0;
			this.zS_AUBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZS_NIBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zS_NIBoundCalcEdit, "AddInfo+ZA_AssayNI_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AddInfo.ZA_AssayNI_Hidden)));
			this.zS_NIBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 191, true);
			this.zS_NIBoundCalcEdit.Name = "ZS_NIBoundCalcEdit";
			this.zS_NIBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zS_NIBoundCalcEdit.TabIndex = 5;
			this.zS_NIBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZS_SNBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zS_SNBoundCalcEdit, "AddInfo+ZA_AssaySN_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AddInfo.ZA_AssaySN_Hidden)));
			this.zS_SNBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 223, true);
			this.zS_SNBoundCalcEdit.Name = "ZS_SNBoundCalcEdit";
			this.zS_SNBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zS_SNBoundCalcEdit.TabIndex = 6;
			this.zS_SNBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZS_PTBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zS_PTBoundCalcEdit, "AddInfo+ZA_AssayPT_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AddInfo.ZA_AssayPT_Hidden)));
			this.zS_PTBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 159, true);
			this.zS_PTBoundCalcEdit.Name = "ZS_PTBoundCalcEdit";
			this.zS_PTBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zS_PTBoundCalcEdit.TabIndex = 4;
			this.zS_PTBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZS_PBBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zS_PBBoundCalcEdit, "AddInfo+ZA_AssayPB_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AddInfo.ZA_AssayPB_Hidden)));
			this.zS_PBBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 127, true);
			this.zS_PBBoundCalcEdit.Name = "ZS_PBBoundCalcEdit";
			this.zS_PBBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zS_PBBoundCalcEdit.TabIndex = 3;
			this.zS_PBBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZS_CUBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zS_CUBoundCalcEdit, "AddInfo+ZA_AssayCU_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AddInfo.ZA_AssayCU_Hidden)));
			this.zS_CUBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 95, true);
			this.zS_CUBoundCalcEdit.Name = "ZS_CUBoundCalcEdit";
			this.zS_CUBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zS_CUBoundCalcEdit.TabIndex = 2;
			this.zS_CUBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZS_AGBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zS_AGBoundCalcEdit, "AddInfo+ZA_AssayAG_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AddInfo.ZA_AssayAG_Hidden)));
			this.zS_AGBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 63, true);
			this.zS_AGBoundCalcEdit.Name = "ZS_AGBoundCalcEdit";
			this.zS_AGBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zS_AGBoundCalcEdit.TabIndex = 1;
			this.zS_AGBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZS_WOBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zS_WOBoundCalcEdit, "AddInfo+ZA_AssayWO_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AddInfo.ZA_AssayWO_Hidden)));
			this.zS_WOBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 255, true);
			this.zS_WOBoundCalcEdit.Name = "ZS_WOBoundCalcEdit";
			this.zS_WOBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zS_WOBoundCalcEdit.TabIndex = 7;
			this.zS_WOBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZS_ZNBoundCalcEditt
			// 
			this.BindingSource.SetBindingMember(this.zS_ZNBoundCalcEditt, "AddInfo+ZA_AssayZN_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AddInfo.ZA_AssayZN_Hidden)));
			this.zS_ZNBoundCalcEditt.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 288, true);
			this.zS_ZNBoundCalcEditt.Name = "ZS_ZNBoundCalcEditt";
			this.zS_ZNBoundCalcEditt.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zS_ZNBoundCalcEditt.TabIndex = 8;
			this.zS_ZNBoundCalcEditt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OKButton
			// 
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 320, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.oKButton.TabIndex = 9;
			this.oKButton.Text = "&OK";
			this.oKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// zLabel10
			// 
			this.zLabel10.BindTo = null;
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 8, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 16, true);
			this.zLabel10.TabIndex = 21;
			this.zLabel10.Text = "Concentration";
			this.zLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ZEditAssayCodeForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 368, true);
			this.Controls.Add(this.zLabel10);
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.zS_ZNBoundCalcEditt);
			this.Controls.Add(this.zS_WOBoundCalcEdit);
			this.Controls.Add(this.zS_AGBoundCalcEdit);
			this.Controls.Add(this.zS_CUBoundCalcEdit);
			this.Controls.Add(this.zS_PBBoundCalcEdit);
			this.Controls.Add(this.zS_PTBoundCalcEdit);
			this.Controls.Add(this.zS_SNBoundCalcEdit);
			this.Controls.Add(this.zS_NIBoundCalcEdit);
			this.Controls.Add(this.zS_AUBoundCalcEdit);
			this.Controls.Add(this.zLabel9);
			this.Controls.Add(this.zLabel8);
			this.Controls.Add(this.zLabel7);
			this.Controls.Add(this.zLabel6);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.JobDeclaration";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ZEditAssayCodeForm";
			this.Text = "Assay Element Codes";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.zLabel5, 0);
			this.Controls.SetChildIndex(this.zLabel6, 0);
			this.Controls.SetChildIndex(this.zLabel7, 0);
			this.Controls.SetChildIndex(this.zLabel8, 0);
			this.Controls.SetChildIndex(this.zLabel9, 0);
			this.Controls.SetChildIndex(this.zS_AUBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.zS_NIBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.zS_SNBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.zS_PTBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.zS_PBBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.zS_CUBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.zS_AGBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.zS_WOBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.zS_ZNBoundCalcEditt, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.zLabel10, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.ZLabel zLabel4;
		private ZArchitecture.ZLabel zLabel5;
		private ZArchitecture.ZLabel zLabel6;
		private ZArchitecture.ZLabel zLabel7;
		private ZArchitecture.ZLabel zLabel8;
		private ZArchitecture.ZLabel zLabel9;
		private ZButton oKButton;
		private ZArchitecture.ZLabel zLabel10;
		private ZArchitecture.ZCalcEdit zS_AUBoundCalcEdit;
		private ZArchitecture.ZCalcEdit zS_NIBoundCalcEdit;
		private ZArchitecture.ZCalcEdit zS_PTBoundCalcEdit;
		private ZArchitecture.ZCalcEdit zS_PBBoundCalcEdit;
		private ZArchitecture.ZCalcEdit zS_CUBoundCalcEdit;
		private ZArchitecture.ZCalcEdit zS_AGBoundCalcEdit;
		private ZArchitecture.ZCalcEdit zS_ZNBoundCalcEditt;
		private ZArchitecture.ZCalcEdit zS_SNBoundCalcEdit;
		private ZArchitecture.ZCalcEdit zS_WOBoundCalcEdit;
	}
}
