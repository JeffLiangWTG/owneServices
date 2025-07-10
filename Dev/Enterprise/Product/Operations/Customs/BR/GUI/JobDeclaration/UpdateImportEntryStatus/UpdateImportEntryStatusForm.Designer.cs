namespace Enterprise.Customs.BR.GUI
{
	partial class UpdateImportEntryStatusForm
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
			this.InstructionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EntryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EventDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RiskChannelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryStatusDropEdit.SuspendLayout();
			this.EventDateEdit.SuspendLayout();
			this.RiskChannelDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 162, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 24, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.UpdateImportEntryStatusObject);
			//
			// InstructionLabel
			//
			this.InstructionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InstructionLabel.AutoSize = true;
			this.InstructionLabel.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("0D23F671-455F-4401-B495-B9A877E4F93F", "", "Enter the Entry Status, Risk Channel or Release Date and then click Update");
			this.InstructionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.InstructionLabel.IsFontBold = true;
			this.InstructionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 15, true);
			this.InstructionLabel.Name = "InstructionLabel";
			this.InstructionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 13, true);
			this.InstructionLabel.TabIndex = 5;
			this.InstructionLabel.UseMnemonic = false;
			//
			// CloseButton
			//
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("35027526-507f-4405-a58b-daa3c9d78759", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 129, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			//
			// UpdateButton
			//
			this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("c914902c-fd8e-41c1-864d-b4f9adda9277", "Update");
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 129, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpdateButton.TabIndex = 3;
			this.UpdateButton.ToolTipCaption = null;
			this.UpdateButton.UseVisualStyleBackColor = true;
			this.UpdateButton.Click += new System.EventHandler(this.OnOkButton_Click);
			//
			// EntryStatusDropEdit
			//
			this.EntryStatusDropEdit.AllowDrop = true;
			this.EntryStatusDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EntryStatusDropEdit, "EntryStatus");
			this.EntryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 40, true);
			this.EntryStatusDropEdit.Name = "EntryStatusDropEdit";
			this.EntryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 23, true);
			this.EntryStatusDropEdit.TabIndex = 0;
			//
			// EventDateEdit
			//
			this.EventDateEdit.AllowDrop = true;
			this.EventDateEdit.AutoCompleteMonthThreshold = 1;
			this.EventDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EventDateEdit, "EventDate");
			this.EventDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EventDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 66, true);
			this.EventDateEdit.Name = "EventDateEdit";
			this.EventDateEdit.TabIndex = 1;
			//
			// RiskChannelDropEdit
			//
			this.RiskChannelDropEdit.AllowDrop = true;
			this.RiskChannelDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RiskChannelDropEdit, "RiskChannel");
			this.RiskChannelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 92, true);
			this.RiskChannelDropEdit.Name = "RiskChannelDropEdit";
			this.RiskChannelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 23, true);
			this.RiskChannelDropEdit.TabIndex = 2;
			//
			// UpdateImportEntryStatusForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("D407F54D-D2D7-4C6C-A814-3047DB9C8A44", "Update Entry Status");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 186, true);
			this.Controls.Add(this.RiskChannelDropEdit);
			this.Controls.Add(this.EventDateEdit);
			this.Controls.Add(this.EntryStatusDropEdit);
			this.Controls.Add(this.UpdateButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.InstructionLabel);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.UpdateImportEntryStatusObject);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 225, true);
			this.Name = "UpdateImportEntryStatusForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InstructionLabel, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.EntryStatusDropEdit, 0);
			this.Controls.SetChildIndex(this.EventDateEdit, 0);
			this.Controls.SetChildIndex(this.RiskChannelDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryStatusDropEdit.ResumeLayout(true);
			this.EntryStatusDropEdit.PerformLayout();
			this.EventDateEdit.ResumeLayout(true);
			this.EventDateEdit.PerformLayout();
			this.RiskChannelDropEdit.ResumeLayout(true);
			this.RiskChannelDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZLabel InstructionLabel;
		internal ZArchitecture.GUI.ZButton UpdateButton;
		internal ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.GUI.ZDropEdit EntryStatusDropEdit;
		internal ZArchitecture.GUI.ZDateEdit EventDateEdit;
		internal ZArchitecture.GUI.ZDropEdit RiskChannelDropEdit;
	}
}
