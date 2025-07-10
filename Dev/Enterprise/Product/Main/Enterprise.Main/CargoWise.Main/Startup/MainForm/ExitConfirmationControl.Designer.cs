
namespace Enterprise.Startup
{
	partial class ExitConfirmationControl
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

		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.Name = "ExitConfirmationControl";
			this.HomeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TopLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			// 
			// HomeButton
			// 
			this.HomeButton.CaptionResourceString = CargoWise.Main.Res.GetData("f359f124-09bd-4500-af37-19e869705092", "Go &Home");
			this.HomeButton.DialogResult = System.Windows.Forms.DialogResult.No;
			this.HomeButton.FlatAppearance.BorderSize = 0;
			this.HomeButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HomeButton.ForeColor = System.Drawing.Color.White;
			this.HomeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 43, true);
			this.HomeButton.Name = "HomeButton";
			this.HomeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 123, true);
			this.HomeButton.TabIndex = 0;
			this.HomeButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.HomeButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
			this.HomeButton.UseVisualStyleBackColor = true;
			// 
			// ExitButton
			// 
			this.ExitButton.CaptionResourceString = CargoWise.Main.Res.GetData("3cff68ec-2179-4fea-be54-1788b0cedc3e", "E&xit the application");
			this.ExitButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.ExitButton.FlatAppearance.BorderSize = 0;
			this.ExitButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ExitButton.ForeColor = System.Drawing.Color.White;
			this.ExitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 43, true);
			this.ExitButton.Name = "ExitButton";
			this.ExitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 123, true);
			this.ExitButton.TabIndex = 1;
			this.ExitButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.ExitButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
			this.ExitButton.UseVisualStyleBackColor = true;
			// 
			// TopLabel
			// 
			this.TopLabel.CaptionResourceString = CargoWise.Main.Res.GetData("cf5f894b-e628-4d14-8822-bb23c0ac0929", "What do you want to do?");
			this.TopLabel.IsFontBold = true;
			this.TopLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.TopLabel.Name = "TopLabel";
			this.TopLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 31, true);
			this.TopLabel.TabIndex = 2;
			// 
			// ExitConfirmationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("e29bb0e3-603f-43e4-bd0e-20b2bc00b1e0", "Confirm Action");
			this.Controls.Add(this.TopLabel);
			this.Controls.Add(this.ExitButton);
			this.Controls.Add(this.HomeButton);
			this.Name = "ExitConfirmationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 172, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		}

		#endregion

		ZArchitecture.GUI.ZButton HomeButton;
		ZArchitecture.GUI.ZButton ExitButton;
		ZArchitecture.ZLabel TopLabel;
	}
}
