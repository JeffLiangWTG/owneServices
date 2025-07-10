namespace Enterprise.ZArchitecture.GUI
{
	partial class PlaceholderReplacementForm
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.InstructionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DoneButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.MacroTextbox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 277, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.ExpressionNoteTemplate);
			// 
			// InstructionLabel
			// 
			this.InstructionLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("3418737a-ea3e-441d-9111-100499cb9606", "Please enter a value for each placeholder in the expression");
			this.InstructionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.InstructionLabel.Name = "InstructionLabel";
			this.InstructionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 23, true);
			this.InstructionLabel.TabIndex = 1;
			// 
			// DoneButton
			// 
			this.DoneButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("d9a94094-a7a0-426b-b73b-0195d5382379", "Done");
			this.DoneButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 243, true);
			this.DoneButton.Name = "DoneButton";
			this.DoneButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DoneButton.TabIndex = 4;
			this.DoneButton.UseVisualStyleBackColor = true;
			this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "Placeholders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.ExpressionNoteTemplate)(null)).Placeholders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ZArchitecture.Business.ExpressionPlaceholder)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.ExpressionNoteTemplate)(null)).Placeholders)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.ExpressionPlaceholder)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.ExpressionNoteTemplate)(null)).Placeholders)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.ExpressionPlaceholder)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.ExpressionNoteTemplate)(null)).Placeholders)).SyncRoot)).Replacement)));
			this.zGrid1.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("4a5cc375-ee46-4196-85a3-272846befe7d", "Sequence");
			zCalcEditColumnStyleInfo2.ColumnName = "Sequence";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("aed83ea6-f399-4471-94fc-5312f728cb5d", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("606f2585-1eff-4db2-ad9b-ac7f11706470", "Replacement");
			zTextBoxColumnStyleInfo4.ColumnName = "Replacement";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid1.CopySelectedRowsAllowed = true;
			this.zGrid1.GridId = "96f21ca5-c1c5-4c05-b552-5f8d9f55e6c6";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 62, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 175, true);
			this.zGrid1.TabIndex = 3;
			// 
			// MacroTextbox
			// 
			this.BindingSource.SetBindingMember(this.MacroTextbox, "TemplateText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.ExpressionNoteTemplate)(null)).TemplateText)));
			this.MacroTextbox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("deb24133-8659-4710-9789-ec39968b9b8a", "Macro to be inserted");
			this.MacroTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 35, true);
			this.MacroTextbox.Name = "MacroTextbox";
			this.MacroTextbox.ReadOnly = true;
			this.MacroTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 20, true);
			this.MacroTextbox.TabIndex = 2;
			// 
			// PlaceholderReplacementForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("b3b02262-1bf7-4844-8d5a-368ce4afc2f2", "Placeholder Replacement");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 301, true);
			this.Controls.Add(this.MacroTextbox);
			this.Controls.Add(this.DoneButton);
			this.Controls.Add(this.zGrid1);
			this.Controls.Add(this.InstructionLabel);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.ExpressionNoteTemplate);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "PlaceholderReplacementForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InstructionLabel, 0);
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(this.DoneButton, 0);
			this.Controls.SetChildIndex(this.MacroTextbox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZLabel InstructionLabel;
		private ZGrid zGrid1;
		private ZButton DoneButton;
		private ZTextBox MacroTextbox;
	}
}