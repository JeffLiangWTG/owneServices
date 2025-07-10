using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Registry.GUI
{
	partial class RegistryApplyForm
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

		[SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification="The designer generated code voliates this rule")]
		new void InitializeComponent()
		{
			this.btnSelectDifferent = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnApplyOverrides = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// btnSelectDifferent
			// 
			this.btnSelectDifferent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSelectDifferent.AutoSize = true;
			this.btnSelectDifferent.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("1aa42165-a665-489d-a2d2-f063f1eff2e0", "Select a Different Base Level");
			this.btnSelectDifferent.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(999, 580, true);
			this.btnSelectDifferent.Name = "btnSelectDifferent";
			this.btnSelectDifferent.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 23, true);
			this.btnSelectDifferent.TabIndex = 5;
			this.btnSelectDifferent.UseVisualStyleBackColor = true;
			this.btnSelectDifferent.Click += new System.EventHandler(this.btnSelectDifferent_Click);
			// 
			// btnApplyOverrides
			// 
			this.btnApplyOverrides.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnApplyOverrides.AutoSize = true;
			this.btnApplyOverrides.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2e774a67-d829-4fff-8ed8-4b16567ed7d9", "Apply Override Values to Base");
			this.btnApplyOverrides.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(832, 580, true);
			this.btnApplyOverrides.Name = "btnApplyOverrides";
			this.btnApplyOverrides.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 23, true);
			this.btnApplyOverrides.TabIndex = 4;
			this.btnApplyOverrides.UseVisualStyleBackColor = true;
			this.btnApplyOverrides.Click += new System.EventHandler(this.btnApplyOverrides_Click);
			// 
			// RegistryApplyForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b3e1bac5-8c93-4fd4-b0f1-c77d045196db", "Apply Overrides Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1246, 633, true);
			this.Controls.Add(this.btnApplyOverrides);
			this.Controls.Add(this.btnSelectDifferent);
			this.Name = "RegistryApplyForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.btnSelectDifferent, 0);
			this.Controls.SetChildIndex(this.btnApplyOverrides, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZButton btnSelectDifferent;
		internal ZArchitecture.GUI.ZButton btnApplyOverrides;
	}
}
