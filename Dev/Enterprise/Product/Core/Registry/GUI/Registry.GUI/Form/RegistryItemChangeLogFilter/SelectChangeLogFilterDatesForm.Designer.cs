using System.ComponentModel;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class SelectChangeLogFilterDatesForm : ZChildForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components;

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
            this.fromDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.toDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.fromDate.SuspendLayout();
            this.toDate.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 125, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 24, true);
            this.MainStatusBar.SizingGrip = false;
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(FilterRegistryChangeLogsByDate);
            // 
            // MessageStatusBarPanel
            // 
            this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(217);
            // 
            // ErrorStatusBarPanel
            // 
            this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(217);
            // 
            // fromDate
            // 
            this.fromDate.AllowDrop = true;
            this.fromDate.AutoCompleteMonthThreshold = 1;
            this.fromDate.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8e9451a6-cec2-4f05-9d2c-b73aa92ee1a4", "From Date");
            this.fromDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 12, true);
            this.fromDate.Name = "fromDate";
            this.fromDate.TabIndex = 1;
			this.BindingSource.SetBindingMember(this.fromDate, "FromDate");
            // 
            // toDate
            // 
            this.toDate.AllowDrop = true;
            this.toDate.AutoCompleteMonthThreshold = 1;
            this.toDate.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("97512bd6-2236-4203-aacf-fa5ecce21456", "To Date");
            this.toDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 38, true);
            this.toDate.Name = "toDate";
            this.toDate.TabIndex = 2;
			this.BindingSource.SetBindingMember(this.toDate, "ToDate");
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("07332b7a-58d5-46b0-bbeb-5058e43562ea", "OK");
            this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 67, true);
            this.okButton.Name = "okButton";
            this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 23, true);
            this.okButton.TabIndex = 3;
            this.okButton.ToolTipCaption = null;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("157853a5-90f5-46fc-a88d-bec2da4c599b", "Cancel");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 96, true);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 23, true);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.ToolTipCaption = null;
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // SelectChangeLogFilterDatesForm
            // 
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 149, true);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.toDate);
            this.Controls.Add(this.fromDate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SelectChangeLogFilterDatesForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.fromDate, 0);
            this.Controls.SetChildIndex(this.toDate, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.fromDate.ResumeLayout(true);
            this.fromDate.PerformLayout();
            this.toDate.ResumeLayout(true);
            this.toDate.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZDateEdit fromDate;
		internal ZDateEdit toDate;
		internal ZButton okButton;
		internal ZButton cancelButton;
	}
}
