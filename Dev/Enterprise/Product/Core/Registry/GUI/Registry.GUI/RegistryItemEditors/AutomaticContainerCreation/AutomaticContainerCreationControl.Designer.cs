using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class AutomaticContainerCreationControl
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
		void InitializeComponent()
		{
            this.optionGroupBox = new CargoWise.Windows.UI.KGroupBox();
            this.createUpToATDOrShippingInstructionRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.neverCreateRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.alwaysCreateRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.optionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.AutomaticContainerCreation);
            // 
            // optionGroupBox
            // 
            this.optionGroupBox.Controls.Add(this.createUpToATDOrShippingInstructionRadioButton);
            this.optionGroupBox.Controls.Add(this.neverCreateRadioButton);
            this.optionGroupBox.Controls.Add(this.alwaysCreateRadioButton);
            this.optionGroupBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.optionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
            this.optionGroupBox.Name = "optionGroupBox";
            this.optionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 141, true);
            this.optionGroupBox.TabIndex = 0;
            this.optionGroupBox.TabStop = false;
            // 
            // createUpToATDOrShippingInstructionRadioButton
            // 
            this.createUpToATDOrShippingInstructionRadioButton.AutoCheck = false;
            this.createUpToATDOrShippingInstructionRadioButton.AutoSize = true;
            this.createUpToATDOrShippingInstructionRadioButton.BackColor = System.Drawing.Color.Transparent;
            this.BindingSource.SetBindingMember(this.createUpToATDOrShippingInstructionRadioButton, "IsCreateUpToATDOrShippingInstruction");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AutomaticContainerCreation)(null)).IsCreateUpToATDOrShippingInstruction)));
            this.createUpToATDOrShippingInstructionRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("59777743-1887-4756-99f1-e7cfef7fdd2d", "Create up to ATD or Shipping Instruction");
            this.createUpToATDOrShippingInstructionRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 80, true);
            this.createUpToATDOrShippingInstructionRadioButton.Name = "createUpToATDOrShippingInstructionRadioButton";
            this.createUpToATDOrShippingInstructionRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 15, true);
            this.createUpToATDOrShippingInstructionRadioButton.TabIndex = 2;
            this.createUpToATDOrShippingInstructionRadioButton.TabStop = true;
            this.createUpToATDOrShippingInstructionRadioButton.UseVisualStyleBackColor = false;
            // 
            // neverCreateRadioButton
            // 
            this.neverCreateRadioButton.AutoCheck = false;
            this.neverCreateRadioButton.AutoSize = true;
            this.neverCreateRadioButton.BackColor = System.Drawing.Color.Transparent;
            this.BindingSource.SetBindingMember(this.neverCreateRadioButton, "IsNeverCreate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AutomaticContainerCreation)(null)).IsNeverCreate)));
            this.neverCreateRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("cc57638e-9604-4e23-947c-d72b0023deff", "Never Create");
            this.neverCreateRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 51, true);
            this.neverCreateRadioButton.Name = "neverCreateRadioButton";
            this.neverCreateRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 15, true);
            this.neverCreateRadioButton.TabIndex = 1;
            this.neverCreateRadioButton.TabStop = true;
            this.neverCreateRadioButton.UseVisualStyleBackColor = false;
            // 
            // alwaysCreateRadioButton
            // 
            this.alwaysCreateRadioButton.AutoCheck = false;
            this.alwaysCreateRadioButton.AutoSize = true;
            this.alwaysCreateRadioButton.BackColor = System.Drawing.Color.Transparent;
            this.BindingSource.SetBindingMember(this.alwaysCreateRadioButton, "IsAlwaysCreate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AutomaticContainerCreation)(null)).IsAlwaysCreate)));
            this.alwaysCreateRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c8eaa8ca-095f-42e7-a9e1-7510206cd775", "Always Create");
            this.alwaysCreateRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 23, true);
            this.alwaysCreateRadioButton.Name = "alwaysCreateRadioButton";
            this.alwaysCreateRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 15, true);
            this.alwaysCreateRadioButton.TabIndex = 0;
            this.alwaysCreateRadioButton.TabStop = true;
            this.alwaysCreateRadioButton.UseVisualStyleBackColor = false;
            // 
            // AutomaticContainerCreationControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.optionGroupBox);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 57, true);
            this.Name = "AutomaticContainerCreationControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 141, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.optionGroupBox.ResumeLayout(false);
            this.optionGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZRadioButton neverCreateRadioButton;
		protected CargoWise.Windows.UI.KGroupBox optionGroupBox;
		protected ZRadioButton createUpToATDOrShippingInstructionRadioButton;
		protected ZArchitecture.GUI.ZRadioButton alwaysCreateRadioButton;
	}
}
