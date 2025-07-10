
namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class NctsGoodsItemRemarksUserControl
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
		private void InitializeComponent()
		{
			this.RemarksGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RemarksGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// RemarksGroupBox
			// 
			this.RemarksGroupBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("70354a5f-b1ab-4b99-a4b8-8c462ca701cc", "[44.15] Remarks");
			this.RemarksGroupBox.Controls.Add(this.RemarksTextBox);
			this.RemarksGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RemarksGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RemarksGroupBox.Name = "RemarksGroupBox";
			this.RemarksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 328, true);
			this.RemarksGroupBox.TabIndex = 0;
			this.RemarksGroupBox.TabStop = false;
			// 
			// RemarksTextBox
			// 
			this.RemarksTextBox.AcceptsReturn = true;
			this.RemarksTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RemarksTextBox, "Remarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).Remarks)));
			this.RemarksTextBox.CaptionResourceString = null;
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 16, true);
			this.RemarksTextBox.Multiline = true;
			this.RemarksTextBox.Name = "RemarksTextBox";
			this.RemarksTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 309, true);
			this.RemarksTextBox.TabIndex = 0;
			// 
			// NctsGoodsItemRemarksUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RemarksGroupBox);
			this.Name = "NctsGoodsItemRemarksUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 328, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RemarksGroupBox.ResumeLayout(false);
			this.RemarksGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox RemarksGroupBox;
		private ZArchitecture.ZTextBox RemarksTextBox;
	}
}
