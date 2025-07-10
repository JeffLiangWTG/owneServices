using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class OrganisationPlugInUserControl
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
			this.typeOfBusinessTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.itemOfBusinessTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.bankAccNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.bankCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.vATDefermentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bankCodeDropEdit.SuspendLayout();
			this.vATDefermentDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.OrgHeaderWrapper);
			// 
			// typeOfBusinessTextBox
			// 
			this.BindingSource.SetBindingMember(this.typeOfBusinessTextBox, "ZO_TypeOfBusiness");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.OrgHeaderWrapper)(null)).ZO_TypeOfBusiness)));
			this.typeOfBusinessTextBox.CaptionResourceString = null;
			this.typeOfBusinessTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 11, true);
			this.typeOfBusinessTextBox.Name = "typeOfBusinessTextBox";
			this.typeOfBusinessTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 19, true);
			this.typeOfBusinessTextBox.TabIndex = 0;
			// 
			// itemOfBusinessTextBox
			// 
			this.BindingSource.SetBindingMember(this.itemOfBusinessTextBox, "ZO_ItemOfBusiness");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.OrgHeaderWrapper)(null)).ZO_ItemOfBusiness)));
			this.itemOfBusinessTextBox.CaptionResourceString = null;
			this.itemOfBusinessTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 34, true);
			this.itemOfBusinessTextBox.Name = "itemOfBusinessTextBox";
			this.itemOfBusinessTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 19, true);
			this.itemOfBusinessTextBox.TabIndex = 1;
			// 
			// bankAccNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.bankAccNoTextBox, "ZO_BankAccNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.OrgHeaderWrapper)(null)).ZO_BankAccNo)));
			this.bankAccNoTextBox.CaptionResourceString = null;
			this.bankAccNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 79, true);
			this.bankAccNoTextBox.Name = "bankAccNoTextBox";
			this.bankAccNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 19, true);
			this.bankAccNoTextBox.TabIndex = 4;
			// 
			// bankCodeDropEdit
			// 
			this.bankCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bankCodeDropEdit, "ZO_BankCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.OrgHeaderWrapper)(null)).ZO_BankCode)));
			this.bankCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 57, true);
			this.bankCodeDropEdit.Name = "bankCodeDropEdit";
			this.bankCodeDropEdit.PreBoundMaxLength = 3;
			this.bankCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 19, true);
			this.bankCodeDropEdit.TabIndex = 2;
			// 
			// vATDefermentDropEdit
			// 
			this.vATDefermentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.vATDefermentDropEdit, "ZO_VATDeferment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.OrgHeaderWrapper)(null)).ZO_VATDeferment)));
			this.vATDefermentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 102, true);
			this.vATDefermentDropEdit.Name = "vATDefermentDropEdit";
			this.vATDefermentDropEdit.PreBoundMaxLength = 2;
			this.vATDefermentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 19, true);
			this.vATDefermentDropEdit.TabIndex = 5;
			// 
			// OrganisationPlugInUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.vATDefermentDropEdit);
			this.Controls.Add(this.bankCodeDropEdit);
			this.Controls.Add(this.bankAccNoTextBox);
			this.Controls.Add(this.itemOfBusinessTextBox);
			this.Controls.Add(this.typeOfBusinessTextBox);
			this.Name = "OrganisationPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bankCodeDropEdit.ResumeLayout(true);
			this.bankCodeDropEdit.PerformLayout();
			this.vATDefermentDropEdit.ResumeLayout(true);
			this.vATDefermentDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.ZTextBox itemOfBusinessTextBox;
		internal ZArchitecture.ZTextBox bankAccNoTextBox;
		internal ZDropEdit bankCodeDropEdit;
		internal ZDropEdit vATDefermentDropEdit;
		internal ZArchitecture.ZTextBox typeOfBusinessTextBox;
		#endregion
	}
}
