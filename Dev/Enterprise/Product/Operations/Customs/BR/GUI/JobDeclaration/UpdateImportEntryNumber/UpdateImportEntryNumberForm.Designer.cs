using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BR.GUI
{
	partial class UpdateImportEntryNumberForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateImportEntryNumberForm));
			this.InstructionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RegistrationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RegistrationDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 138, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 24, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.UpdateImportEntryNumberObject);
			//
			// InstructionLabel
			//
			this.InstructionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InstructionLabel.AutoSize = true;
			this.InstructionLabel.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("a6ea9a56-9a82-49e5-aa00-2c955e5b6ab2", "", "Enter the corresponding Entry Number and Registration Date and then click Update");
			this.InstructionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.InstructionLabel.IsFontBold = true;
			this.InstructionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 15, true);
			this.InstructionLabel.Name = "InstructionLabel";
			this.InstructionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 13, true);
			this.InstructionLabel.TabIndex = 4;
			//
			// CloseButton
			//
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("35027526-507f-4405-a58b-daa3c9d78759", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 105, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			//
			// UpdateButton
			//
			this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("c914902c-fd8e-41c1-864d-b4f9adda9277", "Update");
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 105, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpdateButton.TabIndex = 2;
			this.UpdateButton.ToolTipCaption = null;
			this.UpdateButton.UseVisualStyleBackColor = true;
			this.UpdateButton.Click += new System.EventHandler(this.OnOkButton_Click);
			//
			// RegistrationDateDateEdit
			//
			this.RegistrationDateDateEdit.AllowDrop = true;
			this.RegistrationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RegistrationDateDateEdit, "RegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.UpdateImportEntryNumberObject)(null)).RegistrationDate)));
			this.RegistrationDateDateEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RegistrationDateDateEdit, false);
			this.RegistrationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 70, true);
			this.RegistrationDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RegistrationDateDateEdit.Name = "RegistrationDateDateEdit";
			this.RegistrationDateDateEdit.TabIndex = 1;
			//
			// EntryNumberTextBox
			//
			this.EntryNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "EntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.UpdateImportEntryNumberObject)(null)).EntryNumber)));
			this.EntryNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EntryNumberTextBox, false);
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 44, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.EntryNumberTextBox.TabIndex = 0;
			//
			// UpdateImportEntryNumberForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("53e0dc0d-81d7-49d0-b797-93f369c7ee1b", "Update Entry Number");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 162, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 162, true);
			this.Controls.Add(this.EntryNumberTextBox);
			this.Controls.Add(this.RegistrationDateDateEdit);
			this.Controls.Add(this.UpdateButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.InstructionLabel);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.UpdateImportEntryNumberObject);
			this.Name = "UpdateImportEntryNumberForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InstructionLabel, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.RegistrationDateDateEdit, 0);
			this.Controls.SetChildIndex(this.EntryNumberTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RegistrationDateDateEdit.ResumeLayout(true);
			this.RegistrationDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZLabel InstructionLabel;
		internal ZArchitecture.GUI.ZButton UpdateButton;
		internal ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.GUI.ZDateEdit RegistrationDateDateEdit;
		internal ZArchitecture.ZTextBox EntryNumberTextBox;
	}
}
