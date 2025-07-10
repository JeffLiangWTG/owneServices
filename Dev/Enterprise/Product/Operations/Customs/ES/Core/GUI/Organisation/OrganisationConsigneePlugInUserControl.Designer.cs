using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	partial class OrganisationConsigneePlugInUserControl : ZUserControl
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
			this.ESPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CheckBoxVATDeferment = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CheckBoxRetailer = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MethodOfPaymentCanDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ESPanel.SuspendLayout();
			this.MethodOfPaymentDropEdit.SuspendLayout();
			this.MethodOfPaymentCanDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.ESOrgImpAddInfo);
			// 
			// ESPanel
			// 
			this.ESPanel.Controls.Add(this.CheckBoxVATDeferment);
			this.ESPanel.Controls.Add(this.CheckBoxRetailer);
			this.ESPanel.Controls.Add(this.MethodOfPaymentDropEdit);
			this.ESPanel.Controls.Add(this.MethodOfPaymentCanDropEdit);
			this.ESPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ESPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ESPanel.Name = "ESPanel";
			this.ESPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ESPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 163, true);
			this.ESPanel.TabIndex = 2;
			this.ESPanel.Text = "ES";
			// 
			// CheckBoxVATDeferment
			// 
			this.BindingSource.SetBindingMember(this.CheckBoxVATDeferment, "ZO_VATDeferment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.ESOrgImpAddInfo)(null)).ZO_VATDeferment)));
			this.CheckBoxVATDeferment.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("7AA4CC9C-F4DC-4C3E-B8F3-D08989123687", "VAT Deferment");
			this.CheckBoxVATDeferment.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CheckBoxVATDeferment.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 15, true);
			this.CheckBoxVATDeferment.Name = "CheckBoxVATDeferment";
			this.CheckBoxVATDeferment.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 18, true);
			this.CheckBoxVATDeferment.TabIndex = 3;
			this.CheckBoxVATDeferment.UseVisualStyleBackColor = true;
			// 
			// CheckBoxRetailer
			// 
			this.BindingSource.SetBindingMember(this.CheckBoxRetailer, "ZO_Retailer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.ESOrgImpAddInfo)(null)).ZO_Retailer)));
			this.CheckBoxRetailer.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("D89BFFDC-F384-4775-86F9-557D5466C236", "Retailer");
			this.CheckBoxRetailer.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CheckBoxRetailer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 40, true);
			this.CheckBoxRetailer.Name = "CheckBoxRetailer";
			this.CheckBoxRetailer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 18, true);
			this.CheckBoxRetailer.TabIndex = 4;
			this.CheckBoxRetailer.UseVisualStyleBackColor = true;
			// 
			// MethodOfPaymentDropEdit
			// 
			this.MethodOfPaymentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MethodOfPaymentDropEdit, "ZO_MethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.ESOrgImpAddInfo)(null)).ZO_MethodOfPayment)));
			this.MethodOfPaymentDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("D73FA3DE-AA9E-4BA4-9CDF-F052042C7E2D", "Method Of Payment");
			this.MethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 65, true);
			this.MethodOfPaymentDropEdit.Name = "MethodOfPaymentDropEdit";
			this.MethodOfPaymentDropEdit.ShouldResizeByMaxLength = true;
			this.MethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.MethodOfPaymentDropEdit.TabIndex = 5;
			// 
			// MethodOfPaymentCanDropEdit
			// 
			this.MethodOfPaymentCanDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MethodOfPaymentCanDropEdit, "ZO_MethodOfPaymentCan");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.ESOrgImpAddInfo)(null)).ZO_MethodOfPaymentCan)));
			this.MethodOfPaymentCanDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("5F180327-1D1A-40A4-8063-C01EC742FA9F", "Method Of Payment Can");
			this.MethodOfPaymentCanDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 90, true);
			this.MethodOfPaymentCanDropEdit.Name = "MethodOfPaymentCanDropEdit";
			this.MethodOfPaymentCanDropEdit.ShouldResizeByMaxLength = true;
			this.MethodOfPaymentCanDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.MethodOfPaymentCanDropEdit.TabIndex = 5;
			// 
			// OrganisationConsigneePlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ESPanel);
			this.Name = "OrganisationConsigneePlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 163, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ESPanel.ResumeLayout(false);
			this.ESPanel.PerformLayout();
			this.MethodOfPaymentDropEdit.ResumeLayout(true);
			this.MethodOfPaymentDropEdit.PerformLayout();
			this.MethodOfPaymentCanDropEdit.ResumeLayout(true);
			this.MethodOfPaymentCanDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZPanel ESPanel;
		private ZCheckBox CheckBoxVATDeferment;
		private ZCheckBox CheckBoxRetailer;
		protected ZDropEdit MethodOfPaymentDropEdit;
		protected ZDropEdit MethodOfPaymentCanDropEdit;
	}
}
