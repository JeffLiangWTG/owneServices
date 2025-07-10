namespace Enterprise.Customs.MX.Manifest.GUI
{
	partial class BillsSelectionDialog
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
            this.ReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DescPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
            this.ItemsGrid.SuspendLayout();
            this.ItemsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ReasonDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // SelectedItemCountLabel
            // 
            this.SelectedItemCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 10, true);
            // 
            // DescPanel
            // 
            this.DescPanel.Controls.Add(this.ReasonDropEdit);
            this.DescPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
            this.DescPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 32, true);
            this.DescPanel.Controls.SetChildIndex(this.ReasonDropEdit, 0);
            this.DescPanel.Controls.SetChildIndex(this.SelectedItemCountLabel, 0);
            // 
            // ItemsGrid
            // 
            this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 219, true);
            // 
            // ItemsGroupBox
            // 
            this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 238, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Manifest.Business.MXMessageChooser);
            // 
            // ReasonDropEdit
            // 
            this.ReasonDropEdit.AllowDrop = true;
            this.ReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.ReasonDropEdit, "Reason");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.MX.Manifest.Business.MXMessageChooser)(null)).Reason)));
            this.ReasonDropEdit.CaptionResourceString = Enterprise.Customs.MX.Manifest.GUI.Res.GetData("221DCBF2-1FB4-4A52-AF38-EDAA483DC310", "Reason");
            this.ReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 4, true);
            this.ReasonDropEdit.Name = "ReasonDropEdit";
            this.ReasonDropEdit.ShouldResizeByMaxLength = true;
            this.ReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
            this.ReasonDropEdit.TabIndex = 2;
            // 
            // BillsSelectionDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 300, true);
            this.DataSourceType = typeof(Enterprise.Customs.MX.Manifest.Business.MXMessageChooser);
            this.Name = "BillsSelectionDialog";
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
            this.ReasonDropEdit.ResumeLayout(true);
            this.ReasonDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZDropEdit ReasonDropEdit;

		#endregion
	}
}
