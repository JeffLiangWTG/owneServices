namespace Enterprise.Registry.GUI
{
	partial class AutoRatingRequiredFieldsControl : RegistryBusinessObjectTemplateZUserControl
	{
		private Enterprise.ZArchitecture.GUI.ZCheckBox ServiceLevelCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox FrequencyCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CommodityCodeCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox TransitTimeCheckBox;

		void InitializeComponent()
		{
			this.ServiceLevelCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FrequencyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CommodityCodeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransitTimeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.AutoRatingRequiredFields);
			// 
			// ServiceLevelCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ServiceLevelCheckBox, "RequireServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AutoRatingRequiredFields)(null)).RequireServiceLevel)));
			this.ServiceLevelCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ServiceLevelCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutoRatingRequiredFieldsControl|4fd456e6-2be4-45af-b1cb-910c018c9746", "Service Level");
			this.ServiceLevelCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ServiceLevelCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServiceLevelCheckBox.Name = "ServiceLevelCheckBox";
			this.ServiceLevelCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 24, true);
			this.ServiceLevelCheckBox.TabIndex = 0;
			// 
			// FrequencyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FrequencyCheckBox, "RequireFrequency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AutoRatingRequiredFields)(null)).RequireFrequency)));
			this.FrequencyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FrequencyCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutoRatingRequiredFieldsControl|2ed11004-b4bc-4cd8-b7a0-0bc393a24058", "Frequency and Freq. Unit");
			this.FrequencyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FrequencyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.FrequencyCheckBox.Name = "FrequencyCheckBox";
			this.FrequencyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 24, true);
			this.FrequencyCheckBox.TabIndex = 2;
			// 
			// CommodityCodeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.CommodityCodeCheckBox, "RequireCommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AutoRatingRequiredFields)(null)).RequireCommodityCode)));
			this.CommodityCodeCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CommodityCodeCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutoRatingRequiredFieldsControl|6cad6226-244a-469e-a1b2-acf46212523e", "Commodity Code");
			this.CommodityCodeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CommodityCodeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.CommodityCodeCheckBox.Name = "CommodityCodeCheckBox";
			this.CommodityCodeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 24, true);
			this.CommodityCodeCheckBox.TabIndex = 1;
			// 
			// TransitTimeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TransitTimeCheckBox, "RequireTransitTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AutoRatingRequiredFields)(null)).RequireTransitTime)));
			this.TransitTimeCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.TransitTimeCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutoRatingRequiredFieldsControl|9760c575-7664-4d15-95c9-626820c4b107", "Transit Time");
			this.TransitTimeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TransitTimeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 96, true);
			this.TransitTimeCheckBox.Name = "TransitTimeCheckBox";
			this.TransitTimeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 24, true);
			this.TransitTimeCheckBox.TabIndex = 3;
			// 
			// AutoRatingRequiredFieldsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ServiceLevelCheckBox);
			this.Controls.Add(this.FrequencyCheckBox);
			this.Controls.Add(this.CommodityCodeCheckBox);
			this.Controls.Add(this.TransitTimeCheckBox);
			this.Name = "AutoRatingRequiredFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
