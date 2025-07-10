namespace Enterprise.ZArchitecture.GUI
{
	partial class ZColorSchemeRuleForm
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
		protected new void InitializeComponent()
		{
			this.RuleNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OKBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 81, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.GridColourStripBusinessObject);
			// 
			// RuleNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.RuleNameTextBox, "RuleName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.ZArchitecture.Business.GridColourStripBusinessObject)(null)).RuleName)));
			this.RuleNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RuleNameTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZColorSchemeRuleForm|fc1cf2ec-d0f8-48c8-b2be-87838247cb93", "Rule Name", "Rule Name", "Name of the rule.");
			this.RuleNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 13, true);
			this.RuleNameTextBox.Name = "RuleNameTextBox";
			this.RuleNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.RuleNameTextBox.TabIndex = 0;
			// 
			// OKBtn
			// 
			this.OKBtn.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZColorSchemeRuleForm|9e0b20b8-2597-4ad9-8224-e439999b1de0", "OK");
			this.OKBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 52, true);
			this.OKBtn.Name = "OKBtn";
			this.OKBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKBtn.TabIndex = 1;
			this.OKBtn.UseVisualStyleBackColor = true;
			this.OKBtn.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelBtn
			// 
			this.CancelBtn.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZColorSchemeRuleForm|31f708b9-6910-487e-abaf-91cc577a433e", "Cancel");
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 51, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelBtn.TabIndex = 2;
			this.CancelBtn.UseVisualStyleBackColor = true;
			this.CancelBtn.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ZColorSchemeRuleForm
			// 
			this.AcceptButton = this.OKBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelBtn;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 105, true);
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZColorSchemeRuleForm|a92a80f2-6f42-479b-8748-2e037dd7d3e8", "Color Scheme Rule");
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.RuleNameTextBox);
			this.Controls.Add(this.OKBtn);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.GridColourStripBusinessObject);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MinimizeBox = false;
			this.Name = "ZColorSchemeRuleForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.OKBtn, 0);
			this.Controls.SetChildIndex(this.RuleNameTextBox, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal ZTextBox RuleNameTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton OKBtn;
		Enterprise.ZArchitecture.GUI.ZButton CancelBtn;

	}
}
