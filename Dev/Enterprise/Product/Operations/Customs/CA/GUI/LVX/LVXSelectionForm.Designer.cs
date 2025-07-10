using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class LVXSelectionForm
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
		private new void InitializeComponent()
		{
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectionCriteriaGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.lvxSelectionCriteriaUserControl1 = new Enterprise.Customs.CA.GUI.LVXSelectionCriteriaUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SelectionCriteriaGroupBox.SuspendLayout();
			this.lvxSelectionCriteriaUserControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 233, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.LVXSelectionCriteriaBO);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 209, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1f5a5d9e-0423-4409-8900-494844396f95", "Cancel");
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2ea39d75-0ada-4f2b-85e5-406043fb2549", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 209, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// SelectionCriteriaGroupBox
			// 
			this.SelectionCriteriaGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectionCriteriaGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ca725a61-040e-43e1-94ca-edcffa5c2e66", "Selection Criteria");
			this.SelectionCriteriaGroupBox.Controls.Add(this.lvxSelectionCriteriaUserControl1);
			this.SelectionCriteriaGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.SelectionCriteriaGroupBox.Name = "SelectionCriteriaGroupBox";
			this.SelectionCriteriaGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 202, true);
			this.SelectionCriteriaGroupBox.TabIndex = 4;
			this.SelectionCriteriaGroupBox.TabStop = false;
			// 
			// lvxSelectionCriteriaUserControl1
			// 
			this.lvxSelectionCriteriaUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.lvxSelectionCriteriaUserControl1, ".");
			this.lvxSelectionCriteriaUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvxSelectionCriteriaUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.lvxSelectionCriteriaUserControl1.Name = "lvxSelectionCriteriaUserControl1";
			this.lvxSelectionCriteriaUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 183, true);
			this.lvxSelectionCriteriaUserControl1.TabIndex = 0;
			// 
			// LVXSelectionForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 257, true);
			this.Controls.Add(this.SelectionCriteriaGroupBox);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.LVXSelectionCriteriaBO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 296, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 296, true);
			this.Name = "LVXSelectionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.SelectionCriteriaGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SelectionCriteriaGroupBox.ResumeLayout(false);
			this.SelectionCriteriaGroupBox.PerformLayout();
			this.lvxSelectionCriteriaUserControl1.ResumeLayout(true);
			this.lvxSelectionCriteriaUserControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private new ZButton CancelButton;
		private ZButton OKButton;
		private ZGroupBox SelectionCriteriaGroupBox;
		private LVXSelectionCriteriaUserControl lvxSelectionCriteriaUserControl1;
	}
}
