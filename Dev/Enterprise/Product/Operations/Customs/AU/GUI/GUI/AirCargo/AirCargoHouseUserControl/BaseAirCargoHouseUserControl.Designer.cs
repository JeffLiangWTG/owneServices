namespace Enterprise.Customs.AU.AirCargo.GUI
{
	partial class BaseAirCargoHouseUserControl
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
			this.components = new System.ComponentModel.Container();
			this.masterDetailsUserControl = new Enterprise.Customs.AU.AirCargo.GUI.MasterDetailsUserControl();
			this.CommercialStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommercialStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MasterTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.HouseDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.airCargoHouseBillPartiesUserControl = new Enterprise.Customs.AU.GUI.AirCagoHouseBillPartiesUserControl();
			this.HouseCustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AirCargoHouseCustomFieldsControl = new Enterprise.Customs.GUI.CustomFieldsWrapperControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.masterDetailsUserControl.SuspendLayout();
			this.CommercialStatusDropEdit.SuspendLayout();
			this.MasterTabControl.SuspendLayout();
			this.HouseDetailsTabPage.SuspendLayout();
			this.airCargoHouseBillPartiesUserControl.SuspendLayout();
			this.HouseCustomFieldsTabPage.SuspendLayout();
			this.AirCargoHouseCustomFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusHAWB);
			// 
			// masterDetailsUserControl
			// 
			this.masterDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.masterDetailsUserControl, ".");
			this.masterDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.masterDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.masterDetailsUserControl.Name = "masterDetailsUserControl";
			this.masterDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1033, 109, true);
			this.masterDetailsUserControl.TabIndex = 0;
			// 
			// CommercialStatusDropEdit
			// 
			this.CommercialStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommercialStatusDropEdit, "CS_CommercialStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_CommercialStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).Lookups.CommercialStatusList)));
			this.CommercialStatusDropEdit.BindToList = "Lookups+CommercialStatusList";
			this.CommercialStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 281, true);
			this.CommercialStatusDropEdit.Name = "CommercialStatusDropEdit";
			this.CommercialStatusDropEdit.PreBoundMaxLength = 12;
			this.CommercialStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CommercialStatusDropEdit.TabIndex = 77;
			// 
			// CommercialStatusLabel
			// 
			this.CommercialStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CommercialStatusLabel.IsFontBold = true;
			this.CommercialStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 281, true);
			this.CommercialStatusLabel.Name = "CommercialStatusLabel";
			this.CommercialStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 18, true);
			this.CommercialStatusLabel.TabIndex = 76;
			this.CommercialStatusLabel.Text = "Commercial Status:";
			this.CommercialStatusLabel.UseMnemonic = false;
			// 
			// MasterTabControl
			// 
			this.MasterTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MasterTabControl.Controls.Add(this.HouseDetailsTabPage);
			this.MasterTabControl.Controls.Add(this.HouseCustomFieldsTabPage);
			this.MasterTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MasterTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.MasterTabControl.Name = "MasterTabControl";
			this.MasterTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1033, 339, true);
			this.MasterTabControl.TabIndex = 78;
			// 
			// HouseDetailsTabPage
			// 
			this.HouseDetailsTabPage.Controls.Add(this.CommercialStatusDropEdit);
			this.HouseDetailsTabPage.Controls.Add(this.CommercialStatusLabel);
			this.HouseDetailsTabPage.Controls.Add(this.airCargoHouseBillPartiesUserControl);
			this.HouseDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseDetailsTabPage.Name = "HouseDetailsTabPage";
			this.HouseDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1025, 312, true);
			this.HouseDetailsTabPage.TabIndex = 0;
			this.HouseDetailsTabPage.Text = "House Details";
			// 
			// airCargoHouseBillPartiesUserControl
			// 
			this.airCargoHouseBillPartiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.airCargoHouseBillPartiesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)))));
			this.airCargoHouseBillPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 3, true);
			this.airCargoHouseBillPartiesUserControl.Name = "airCargoHouseBillPartiesUserControl";
			this.airCargoHouseBillPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 265, true);
			this.airCargoHouseBillPartiesUserControl.TabIndex = 0;
			// 
			// HouseCustomFieldsTabPage
			// 
			this.HouseCustomFieldsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.HouseCustomFieldsTabPage.Controls.Add(this.AirCargoHouseCustomFieldsControl);
			this.HouseCustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseCustomFieldsTabPage.Name = "HouseCustomFieldsTabPage";
			this.HouseCustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HouseCustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1025, 312, true);
			this.HouseCustomFieldsTabPage.TabIndex = 2;
			this.HouseCustomFieldsTabPage.Text = "Custom Fields";
			// 
			// AirCargoHouseCustomFieldsControl
			// 
			this.AirCargoHouseCustomFieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirCargoHouseCustomFieldsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.EntityFramework.BusinessObject)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)))));
			this.AirCargoHouseCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AirCargoHouseCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AirCargoHouseCustomFieldsControl.Name = "AirCargoHouseCustomFieldsControl";
			this.AirCargoHouseCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 306, true);
			this.AirCargoHouseCustomFieldsControl.TabIndex = 0;
			// 
			// BaseAirCargoHouseUserControl
			// 
			this.Controls.Add(this.MasterTabControl);
			this.Controls.Add(this.masterDetailsUserControl);
			this.Name = "BaseAirCargoHouseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1033, 448, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.masterDetailsUserControl.ResumeLayout(true);
			this.masterDetailsUserControl.PerformLayout();
			this.CommercialStatusDropEdit.ResumeLayout(true);
			this.CommercialStatusDropEdit.PerformLayout();
			this.MasterTabControl.ResumeLayout(false);
			this.MasterTabControl.PerformLayout();
			this.HouseDetailsTabPage.ResumeLayout(false);
			this.HouseDetailsTabPage.PerformLayout();
			this.airCargoHouseBillPartiesUserControl.ResumeLayout(true);
			this.airCargoHouseBillPartiesUserControl.PerformLayout();
			this.HouseCustomFieldsTabPage.ResumeLayout(false);
			this.HouseCustomFieldsTabPage.PerformLayout();
			this.AirCargoHouseCustomFieldsControl.ResumeLayout(true);
			this.AirCargoHouseCustomFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl GoodsValueCurrencyControl;
		protected Enterprise.Customs.AU.AirCargo.GUI.MasterDetailsUserControl masterDetailsUserControl;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit CommercialStatusDropEdit;
		protected Enterprise.ZArchitecture.ZLabel CommercialStatusLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl MasterTabControl;
		private ZArchitecture.GUI.ZTabPage HouseCustomFieldsTabPage;
		private Customs.GUI.CustomFieldsWrapperControl AirCargoHouseCustomFieldsControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage HouseDetailsTabPage;
		internal protected Enterprise.Customs.AU.GUI.AirCagoHouseBillPartiesUserControl airCargoHouseBillPartiesUserControl;
	}
}
