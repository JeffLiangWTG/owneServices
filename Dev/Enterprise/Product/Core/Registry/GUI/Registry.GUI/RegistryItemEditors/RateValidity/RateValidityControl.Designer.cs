using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RateValidityControl : ZUserControl
	{
		Enterprise.ZArchitecture.GUI.ZCheckBox ApplyCheckBox;
		Enterprise.ZArchitecture.ZCalcEdit RateValidityCalcEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox BlankCheckbox;

		void InitializeComponent()
		{
			this.ApplyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RateValidityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BlankCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ApplyCheckBox
			// 
			this.ApplyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ApplyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.ApplyCheckBox.Name = "ApplyCheckBox";
			this.ApplyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 24, true);
			this.ApplyCheckBox.TabIndex = 1;
			this.ApplyCheckBox.CaptionResourceString = Res.GetData("5a305da5-614d-4c75-8b88-9bb83a7ec5e2", "Apply until end of the month");
			this.ApplyCheckBox.CheckedChanged += new System.EventHandler(this.ApplyCheckBox_CheckedChanged);
			// 
			// RateValidityCalcEdit
			// 
			this.RateValidityCalcEdit.Decimals = 0;
			this.RateValidityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.RateValidityCalcEdit.Name = "RateValidityCalcEdit";
			this.RateValidityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.RateValidityCalcEdit.TabIndex = 3;
			this.RateValidityCalcEdit.Text = "0";
			this.RateValidityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BlankCheckbox
			// 
			this.BlankCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BlankCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.BlankCheckbox.Name = "BlankCheckbox";
			this.BlankCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 24, true);
			this.BlankCheckbox.TabIndex = 4;
			this.BlankCheckbox.CaptionResourceString = Res.GetData("e165dde2-f76c-469d-9486-3ee343d50a27", "Leave Blank");
			this.BlankCheckbox.CheckedChanged += new System.EventHandler(this.BlankCheckbox_CheckedChanged);
			// 
			// RateValidityControl
			// 
			this.Controls.Add(this.BlankCheckbox);
			this.Controls.Add(this.RateValidityCalcEdit);
			this.Controls.Add(this.ApplyCheckBox);
			this.Name = "RateValidityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 199, true);
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
