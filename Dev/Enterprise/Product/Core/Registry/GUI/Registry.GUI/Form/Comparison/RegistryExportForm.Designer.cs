using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Registry.GUI
{
	partial class RegistryExportForm
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
		[SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "The designer generated code voliates this rule")]
		new void InitializeComponent()
		{
			this.btnApplyToAnotherLevel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnExport = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// btnApplyToAnotherLevel
			// 
			this.btnApplyToAnotherLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnApplyToAnotherLevel.AutoSize = true;
			this.btnApplyToAnotherLevel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("91b84dc7-558c-4970-91e3-f696dbd382d8", "Apply to Another Level");
			this.btnApplyToAnotherLevel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1026, 580, true);
			this.btnApplyToAnotherLevel.Name = "btnApplyToAnotherLevel";
			this.btnApplyToAnotherLevel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 23, true);
			this.btnApplyToAnotherLevel.TabIndex = 5;
			this.btnApplyToAnotherLevel.UseVisualStyleBackColor = true;
			this.btnApplyToAnotherLevel.Click += new System.EventHandler(this.btnApplyToAnotherLevel_Click);
			// 
			// btnExport
			// 
			this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExport.AutoSize = true;
			this.btnExport.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2f9792d2-d342-4601-aea0-9df3ccde8d63", "Export");
			this.btnExport.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(939, 580, true);
			this.btnExport.Name = "btnExport";
			this.btnExport.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.btnExport.TabIndex = 4;
			this.btnExport.UseVisualStyleBackColor = true;
			this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
			// 
			// RegistryExportForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ff9ed7ad-e44a-4676-9c0b-33ff9a882ea6", "Registry Export Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1246, 633, true);
			this.Controls.Add(this.btnExport);
			this.Controls.Add(this.btnApplyToAnotherLevel);
			this.Name = "RegistryExportForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.btnApplyToAnotherLevel, 0);
			this.Controls.SetChildIndex(this.btnExport, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZButton btnApplyToAnotherLevel;
		internal ZArchitecture.GUI.ZButton btnExport;
	}
}
