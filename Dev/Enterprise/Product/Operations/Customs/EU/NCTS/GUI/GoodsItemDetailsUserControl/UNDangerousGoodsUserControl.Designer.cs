namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class UNDangerousGoodsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.UNDangerousGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UNDangerousGoodsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// UNDangerousGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.UNDangerousGoodsTextBox, "UNDGsAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).UNDGsAsString)));
			this.UNDangerousGoodsTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UNDangerousGoodsTextBox, false);
			this.UNDangerousGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UNDangerousGoodsTextBox.Name = "UNDangerousGoodsTextBox";
			this.UNDangerousGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 15, true);
			this.UNDangerousGoodsTextBox.TabIndex = 0;
			// 
			// UNDangerousGoodsButton
			// 
			this.UNDangerousGoodsButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.UNDangerousGoodsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 0, true);
			this.UNDangerousGoodsButton.Name = "UNDangerousGoodsButton";
			this.UNDangerousGoodsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 22, true);
			this.UNDangerousGoodsButton.TabIndex = 1;
			this.UNDangerousGoodsButton.ToolTipCaption = null;
			// 
			// UNDangerousGoodsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UNDangerousGoodsTextBox);
			this.Controls.Add(this.UNDangerousGoodsButton);
			this.Name = "UNDangerousGoodsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox UNDangerousGoodsTextBox;
		internal ZArchitecture.GUI.ZButton UNDangerousGoodsButton;
	}
}
