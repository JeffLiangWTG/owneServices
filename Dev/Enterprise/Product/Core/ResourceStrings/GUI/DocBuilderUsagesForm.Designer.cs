namespace Enterprise.ResourceStrings.GUI
{
	partial class DocBuilderUsagesForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.keyTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.captionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.usagesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.usagesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 329, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.HelpDataString);
			// 
			// keyTextbox
			// 
			this.keyTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.keyTextbox, "HD_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_Code)));
			this.keyTextbox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("02fe0b8c-1fc3-4305-90b6-1f6bf6a16221", "Key");
			this.keyTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.keyTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 12, true);
			this.keyTextbox.Name = "keyTextbox";
			this.keyTextbox.ReadOnly = true;
			this.keyTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 20, true);
			this.keyTextbox.TabIndex = 1;
			// 
			// captionTextBox
			// 
			this.captionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.captionTextBox, "HD_Caption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_Caption)));
			this.captionTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("fffd6eb1-19fb-4f02-b5c1-533ee73bcf63", "Caption");
			this.captionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.captionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 39, true);
			this.captionTextBox.Name = "captionTextBox";
			this.captionTextBox.ReadOnly = true;
			this.captionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 20, true);
			this.captionTextBox.TabIndex = 2;
			// 
			// usagesGrid
			// 
			this.usagesGrid.AllowNavigation = false;
			this.usagesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.usagesGrid, "DocBuilderUsages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).DocBuilderUsages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.IDocBuilderUsage)(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).DocBuilderUsages)).SyncRoot)).Macro)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.IDocBuilderUsage)(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).DocBuilderUsages)).SyncRoot)).TemplateName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.IDocBuilderUsage)(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).DocBuilderUsages)).SyncRoot)).AllDocumentNames)));
			this.usagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("325753f0-f8c0-40e1-9e87-ece98057a644", "Content");
			zTextBoxColumnStyleInfo1.ColumnName = "Macro";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("5b8fd3ba-4fab-49f5-a09d-a8c7135e68c3", "Template");
			zTextBoxColumnStyleInfo2.ColumnName = "TemplateName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("1bf836f1-b141-4553-9788-1fdc04a75777", "Documents");
			zMultiLineTextBoxColumnInfo1.ColumnName = "AllDocumentNames";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.usagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.usagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.usagesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.usagesGrid.CopySelectedRowsAllowed = true;
			this.usagesGrid.GridId = "ed8575d3-e464-495e-b038-f0b03881c02a";
			this.usagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.usagesGrid.LayoutKey = "usagesGrid";
			this.usagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 65, true);
			this.usagesGrid.Name = "usagesGrid";
			this.usagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 258, true);
			this.usagesGrid.TabIndex = 3;
			// 
			// DocBuilderUsagesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 353, true);
			this.Controls.Add(this.captionTextBox);
			this.Controls.Add(this.usagesGrid);
			this.Controls.Add(this.keyTextbox);
			this.DataSourceType = typeof(Enterprise.ResourceStrings.Business.HelpDataString);
			this.Name = "DocBuilderUsagesForm";
			this.Controls.SetChildIndex(this.keyTextbox, 0);
			this.Controls.SetChildIndex(this.usagesGrid, 0);
			this.Controls.SetChildIndex(this.captionTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.usagesGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox keyTextbox;
		private ZArchitecture.ZTextBox captionTextBox;
		private ZArchitecture.ZGrid usagesGrid;
	}
}
