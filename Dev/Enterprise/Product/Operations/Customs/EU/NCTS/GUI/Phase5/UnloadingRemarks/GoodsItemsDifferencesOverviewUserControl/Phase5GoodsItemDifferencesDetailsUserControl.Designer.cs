namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemDifferencesDetailsUserControl
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
            this.SequenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ItemNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc);
            // 
            // SequenceNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.SequenceNumberTextBox, "BY_LineNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_LineNo)));
            this.SequenceNumberTextBox.CaptionResourceString = null;
            this.SequenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 24, true);
            this.SequenceNumberTextBox.Name = "SequenceNumberTextBox";
            this.SequenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
            this.SequenceNumberTextBox.TabIndex = 0;
            // 
            // ItemNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.ItemNumberTextBox, "BY_DeclarationGoodsItemNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_DeclarationGoodsItemNumber)));
            this.ItemNumberTextBox.CaptionResourceString = null;
            this.ItemNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 50, true);
            this.ItemNumberTextBox.Name = "ItemNumberTextBox";
            this.ItemNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
            this.ItemNumberTextBox.TabIndex = 0;
			// 
			// Phase5GoodsItemDifferencesDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SequenceNumberTextBox);
            this.Controls.Add(this.ItemNumberTextBox);
            this.Name = "Phase5GoodsItemDifferencesDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 407, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox SequenceNumberTextBox;
		internal ZArchitecture.ZTextBox ItemNumberTextBox;

	}
}
