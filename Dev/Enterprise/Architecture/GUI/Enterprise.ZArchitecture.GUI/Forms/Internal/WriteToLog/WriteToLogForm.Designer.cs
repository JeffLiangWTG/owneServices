using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class WriteToLogForm
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
			this.WriteToLogButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 85, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger);
			// 
			// WriteToLogButton
			// 
			this.WriteToLogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.WriteToLogButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.WriteToLogButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("WriteToLogForm|46ac2f5e-905a-46cc-b4d5-397532f83738", "Log && Close");
			this.WriteToLogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 56, true);
			this.WriteToLogButton.Name = "WriteToLogButton";
			this.WriteToLogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.WriteToLogButton.TabIndex = 2;
			this.WriteToLogButton.UseVisualStyleBackColor = true;
			this.WriteToLogButton.Click += new System.EventHandler(this.WriteToLogButton_Click);
			// 
			// ReferenceTextBox
			// 
			this.ReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "Reference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger)(null)).Reference)));
			this.ReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("WriteToLogForm|89ad33e8-30c5-4689-ba23-a31918fa02db", "Reference");
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			this.ReferenceTextBox.Multiline = true;
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 42, true);
			this.ReferenceTextBox.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("WriteToLogForm|d2154688-cfec-4f45-bc1b-d5d2cce44038", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 56, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// WriteToLogForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 109, true);
			this.Controls.Add(this.ReferenceTextBox);
			this.Controls.Add(this.WriteToLogButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 149, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 143, true);
			this.Name = "WriteToLogForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.WriteToLogButton, 0);
			this.Controls.SetChildIndex(this.ReferenceTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
#if DEBUG
		internal
#endif
		ZButton WriteToLogButton;
		private ZTextBox ReferenceTextBox;
#if DEBUG
		internal
#endif
		ZButton CloseButton;
	}
}
