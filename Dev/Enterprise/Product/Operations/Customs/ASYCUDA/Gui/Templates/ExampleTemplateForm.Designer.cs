namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class ExampleTemplateForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.radioButton1 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.radioButton2 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.radioButton3 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.radioButton4 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.templateControl = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 736, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.GUI.ExampleTemplateFormViewModel);
			// 
			// radioButton1
			// 
			this.radioButton1.AutoCheck = false;
			this.radioButton1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.radioButton1, "Layout1Selected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ASYCUDA.GUI.ExampleTemplateFormViewModel)(null)).Layout1Selected)));
			this.radioButton1.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("bec7b2b8-800e-4cdd-ac1d-3c6d90ce642a", "All Controls");
			this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.radioButton1.TabIndex = 2;
			this.radioButton1.TabStop = true;
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			this.radioButton2.AutoCheck = false;
			this.radioButton2.AutoSize = true;
			this.BindingSource.SetBindingMember(this.radioButton2, "Layout2Selected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ASYCUDA.GUI.ExampleTemplateFormViewModel)(null)).Layout2Selected)));
			this.radioButton2.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("75e687d0-863f-4618-bb68-78067eabf685", "IAM");
			this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 12, true);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 17, true);
			this.radioButton2.TabIndex = 3;
			this.radioButton2.TabStop = true;
			this.radioButton2.UseVisualStyleBackColor = true;
			// 
			// radioButton3
			// 
			this.radioButton3.AutoCheck = false;
			this.radioButton3.AutoSize = true;
			this.BindingSource.SetBindingMember(this.radioButton3, "Layout3Selected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ASYCUDA.GUI.ExampleTemplateFormViewModel)(null)).Layout3Selected)));
			this.radioButton3.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("43b9bcbb-70f1-4424-b49c-3cc1d475cfe6", "Additional Controls");
			this.radioButton3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButton3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 12, true);
			this.radioButton3.Name = "radioButton3";
			this.radioButton3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
			this.radioButton3.TabIndex = 4;
			this.radioButton3.TabStop = true;
			this.radioButton3.UseVisualStyleBackColor = true;
			// 
			// radioButton4
			// 
			this.radioButton4.AutoCheck = false;
			this.radioButton4.AutoSize = true;
			this.BindingSource.SetBindingMember(this.radioButton4, "Layout4Selected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ASYCUDA.GUI.ExampleTemplateFormViewModel)(null)).Layout4Selected)));
			this.radioButton4.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("af0d43ee-84a7-4f0e-95b8-ab53f2883a24", "Column Layout");
			this.radioButton4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButton4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 12, true);
			this.radioButton4.Name = "radioButton4";
			this.radioButton4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.radioButton4.TabIndex = 5;
			this.radioButton4.TabStop = true;
			this.radioButton4.UseVisualStyleBackColor = true;
			// 
			// templateControl
			// 
			this.templateControl.AllowDrop = true;
			this.templateControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.templateControl.AutoScroll = true;
			this.templateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.templateControl.Name = "templateControl";
			this.templateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 700, true);
			this.templateControl.TabIndex = 6;
			// 
			// ExampleTemplateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("82082b38-514c-42a9-8fa5-46a9b42d910c", "Example Template Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 760, true);
			this.Controls.Add(this.templateControl);
			this.Controls.Add(this.radioButton4);
			this.Controls.Add(this.radioButton3);
			this.Controls.Add(this.radioButton2);
			this.Controls.Add(this.radioButton1);
			this.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.GUI.ExampleTemplateFormViewModel);
			this.Name = "ExampleTemplateForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.radioButton1, 0);
			this.Controls.SetChildIndex(this.radioButton2, 0);
			this.Controls.SetChildIndex(this.radioButton3, 0);
			this.Controls.SetChildIndex(this.radioButton4, 0);
			this.Controls.SetChildIndex(this.templateControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZRadioButton radioButton1;
		private Enterprise.ZArchitecture.GUI.ZRadioButton radioButton2;
		private Enterprise.ZArchitecture.GUI.ZRadioButton radioButton3;
		private Enterprise.ZArchitecture.GUI.ZRadioButton radioButton4;
		private Enterprise.ZArchitecture.GUI.DynamicLayoutPanel templateControl;
	}
}
