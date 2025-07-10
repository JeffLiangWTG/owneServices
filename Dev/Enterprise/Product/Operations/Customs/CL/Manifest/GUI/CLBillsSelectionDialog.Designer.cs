namespace Enterprise.Customs.CL.Manifest.GUI
{
	partial class CLBillsSelectionDialog
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
		new void InitializeComponent()
		{
			this.ReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AmendReasonComboBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AmendTypeComboBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AmendReasonComboBox.SuspendLayout();
			this.AmendTypeComboBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SelectedItemCountLabel
			// 
			this.SelectedItemCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 31, true);
			// 
			// DescPanel
			// 
			this.DescPanel.Controls.Add(this.AmendTypeComboBox);
			this.DescPanel.Controls.Add(this.AmendReasonComboBox);
			this.DescPanel.Controls.Add(this.ReasonTextBox);
			this.DescPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			this.DescPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 53, true);
			this.DescPanel.Controls.SetChildIndex(this.ReasonTextBox, 0);
			this.DescPanel.Controls.SetChildIndex(this.SelectedItemCountLabel, 0);
			this.DescPanel.Controls.SetChildIndex(this.AmendReasonComboBox, 0);
			this.DescPanel.Controls.SetChildIndex(this.AmendTypeComboBox, 0);
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 201, true);
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 217, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CL.Manifest.Business.CLMessageChooser);
			// 
			// AmendTypeComboBox
			// 
			this.AmendTypeComboBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmendTypeComboBox, "AmendType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CL.Manifest.Business.CLMessageChooser)(null)).AmendType)));
			this.AmendTypeComboBox.CaptionResourceString = Enterprise.Customs.CL.Manifest.GUI.Res.GetData("0DEE298D-C158-459D-97B6-5CE738274F24", "Amend Type");
			this.AmendTypeComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 4, true);
			this.AmendTypeComboBox.Name = "AmendTypeComboBox";
			this.AmendTypeComboBox.ShouldResizeByMaxLength = true;
			this.AmendTypeComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.AmendTypeComboBox.TabIndex = 2;
			// 
			// AmendReasonComboBox
			// 
			this.AmendReasonComboBox.AllowDrop = true;
			this.AmendReasonComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AmendReasonComboBox, "AmendReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CL.Manifest.Business.CLMessageChooser)(null)).AmendReason)));
			this.AmendReasonComboBox.CaptionResourceString = Enterprise.Customs.CL.Manifest.GUI.Res.GetData("0550A0A2-3A14-4D9B-9EDB-F800BE1B5260", "Amend Reason");
			this.AmendReasonComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 4, true);
			this.AmendReasonComboBox.Name = "AmendReasonComboBox";
			this.AmendReasonComboBox.ShouldResizeByMaxLength = true;
			this.AmendReasonComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 17, true);
			this.AmendReasonComboBox.TabIndex = 3;
			// 
			// ReasonTextBox
			// 
			this.ReasonTextBox.AllowDrop = true;
			this.ReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReasonTextBox, "Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLMessageChooser)(null)).Reason)));
			this.ReasonTextBox.CaptionResourceString = null;
			this.ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 29, true);
			this.ReasonTextBox.Name = "ReasonTextBox";
			this.ReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 17, true);
			this.ReasonTextBox.TabIndex = 4;
			// 
			// CLBillsSelectionDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 300, true);
			this.DataSourceType = typeof(Enterprise.Customs.CL.Manifest.Business.CLMessageChooser);
			this.Name = "CLBillsSelectionDialog";
			this.DescPanel.ResumeLayout(false);
			this.DescPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AmendReasonComboBox.ResumeLayout(true);
			this.AmendReasonComboBox.PerformLayout();
			this.AmendTypeComboBox.ResumeLayout(true);
			this.AmendTypeComboBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		ZArchitecture.ZTextBox ReasonTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit AmendReasonComboBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit AmendTypeComboBox;

		#endregion
	}
}
