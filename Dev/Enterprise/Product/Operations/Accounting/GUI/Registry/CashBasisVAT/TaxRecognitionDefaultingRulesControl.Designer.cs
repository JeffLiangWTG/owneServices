using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class TaxRecognitionDefaultingRulesControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			this.InputTaxRecognitionRulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.APOrganizationOverrideDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.APInputServicesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.APInputGoodsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OutputTaxRecognitionRulesGropuBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AROrganizationOverrideDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AROutputServicesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AROutputGoodsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InputTaxRecognitionRulesGroupBox.SuspendLayout();
			this.OutputTaxRecognitionRulesGropuBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.TaxRecognitionDefaultingRules);
			// 
			// InputTaxRecognitionRulesGroupBox
			// 
			this.InputTaxRecognitionRulesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a5d91008-4438-4fb5-ab60-76caeae37d17", "Input Tax Recognition Rules");
			this.InputTaxRecognitionRulesGroupBox.Controls.Add(this.APOrganizationOverrideDropEdit);
			this.InputTaxRecognitionRulesGroupBox.Controls.Add(this.APInputServicesDropEdit);
			this.InputTaxRecognitionRulesGroupBox.Controls.Add(this.APInputGoodsDropEdit);
			this.InputTaxRecognitionRulesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.InputTaxRecognitionRulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InputTaxRecognitionRulesGroupBox.Name = "InputTaxRecognitionRulesGroupBox";
			this.InputTaxRecognitionRulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 105, true);
			this.InputTaxRecognitionRulesGroupBox.TabIndex = 0;
			this.InputTaxRecognitionRulesGroupBox.TabStop = false;
			// 
			// APOrganizationOverrideDropEdit
			// 
			this.APOrganizationOverrideDropEdit.AllowDrop = true;
			this.APOrganizationOverrideDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.APOrganizationOverrideDropEdit, "APOrganizationOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.TaxRecognitionDefaultingRules)(null)).APOrganizationOverride)));
			this.APOrganizationOverrideDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("844c331c-e76e-45c7-83a0-d2c4dc0f6560", "AP Organization Override");
			this.APOrganizationOverrideDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 71, true);
			this.APOrganizationOverrideDropEdit.Name = "APOrganizationOverrideDropEdit";
			this.APOrganizationOverrideDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.APOrganizationOverrideDropEdit.TabIndex = 2;
			// 
			// APInputServicesDropEdit
			// 
			this.APInputServicesDropEdit.AllowDrop = true;
			this.APInputServicesDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.APInputServicesDropEdit, "APInputServices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.TaxRecognitionDefaultingRules)(null)).APInputServices)));
			this.APInputServicesDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("12d58224-3371-4d14-b696-b179dfb620bb", "AP Input Services");
			this.APInputServicesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 45, true);
			this.APInputServicesDropEdit.Name = "APInputServicesDropEdit";
			this.APInputServicesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.APInputServicesDropEdit.TabIndex = 1;
			// 
			// APInputGoodsDropEdit
			// 
			this.APInputGoodsDropEdit.AllowDrop = true;
			this.APInputGoodsDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.APInputGoodsDropEdit, "APInputGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.TaxRecognitionDefaultingRules)(null)).APInputGoods)));
			this.APInputGoodsDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ef089c9f-073c-4783-ab99-45e73a621878", "AP Input Goods");
			this.APInputGoodsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 19, true);
			this.APInputGoodsDropEdit.Name = "APInputGoodsDropEdit";
			this.APInputGoodsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.APInputGoodsDropEdit.TabIndex = 0;
			// 
			// OutputTaxRecognitionRulesGropuBox
			// 
			this.OutputTaxRecognitionRulesGropuBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("db125bf3-ad17-4f53-8f16-87d0d7ee630c", "Output Tax Recognition Rules");
			this.OutputTaxRecognitionRulesGropuBox.Controls.Add(this.AROrganizationOverrideDropEdit);
			this.OutputTaxRecognitionRulesGropuBox.Controls.Add(this.AROutputServicesDropEdit);
			this.OutputTaxRecognitionRulesGropuBox.Controls.Add(this.AROutputGoodsDropEdit);
			this.OutputTaxRecognitionRulesGropuBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutputTaxRecognitionRulesGropuBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 105, true);
			this.OutputTaxRecognitionRulesGropuBox.Name = "OutputTaxRecognitionRulesGropuBox";
			this.OutputTaxRecognitionRulesGropuBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 105, true);
			this.OutputTaxRecognitionRulesGropuBox.TabIndex = 1;
			this.OutputTaxRecognitionRulesGropuBox.TabStop = false;
			// 
			// AROrganizationOverrideDropEdit
			// 
			this.AROrganizationOverrideDropEdit.AllowDrop = true;
			this.AROrganizationOverrideDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AROrganizationOverrideDropEdit, "AROrganizationOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.TaxRecognitionDefaultingRules)(null)).AROrganizationOverride)));
			this.AROrganizationOverrideDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9e8fbe02-52e3-4761-8dfa-1f12f45ed2af", "AR Organization Override");
			this.AROrganizationOverrideDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 71, true);
			this.AROrganizationOverrideDropEdit.Name = "AROrganizationOverrideDropEdit";
			this.AROrganizationOverrideDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.AROrganizationOverrideDropEdit.TabIndex = 2;
			// 
			// AROutputServicesDropEdit
			// 
			this.AROutputServicesDropEdit.AllowDrop = true;
			this.AROutputServicesDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AROutputServicesDropEdit, "AROutputServices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.TaxRecognitionDefaultingRules)(null)).AROutputServices)));
			this.AROutputServicesDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("32334d11-d0ce-40e8-ac09-efc4d4449282", "AR Output Services");
			this.AROutputServicesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 45, true);
			this.AROutputServicesDropEdit.Name = "AROutputServicesDropEdit";
			this.AROutputServicesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.AROutputServicesDropEdit.TabIndex = 1;
			// 
			// AROutputGoodsDropEdit
			// 
			this.AROutputGoodsDropEdit.AllowDrop = true;
			this.AROutputGoodsDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AROutputGoodsDropEdit, "AROutputGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.TaxRecognitionDefaultingRules)(null)).AROutputGoods)));
			this.AROutputGoodsDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2f4a54d1-b200-43de-9365-f61f86d0a813", "AR Output Goods");
			this.AROutputGoodsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 19, true);
			this.AROutputGoodsDropEdit.Name = "AROutputGoodsDropEdit";
			this.AROutputGoodsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.AROutputGoodsDropEdit.TabIndex = 0;
			// 
			// TaxRecognitionDefaultingRulesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OutputTaxRecognitionRulesGropuBox);
			this.Controls.Add(this.InputTaxRecognitionRulesGroupBox);
			this.Name = "TaxRecognitionDefaultingRulesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 210, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InputTaxRecognitionRulesGroupBox.ResumeLayout(false);
			this.OutputTaxRecognitionRulesGropuBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		ZGroupBox InputTaxRecognitionRulesGroupBox;
		ZGroupBox OutputTaxRecognitionRulesGropuBox;
		ZDropEdit APOrganizationOverrideDropEdit;
		ZDropEdit APInputServicesDropEdit;
		ZDropEdit APInputGoodsDropEdit;
		ZDropEdit AROrganizationOverrideDropEdit;
		ZDropEdit AROutputServicesDropEdit;
		ZDropEdit AROutputGoodsDropEdit;

	}
}
