using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ExpressionTemplateForm
	{
		ZGrid PlaceholdersGrid;
		ZLabel PlaceholdersLabel;
		ZButton PlaceholderButton;

		new void InitializeComponent()
		{
			var zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			this.PlaceholdersLabel = new ZLabel();
			this.PlaceholdersGrid = new ZGrid();
			this.PlaceholderButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PlaceholdersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// templateTextBox
			// 
			this.templateTextBox.TabIndex = 6;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 364, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ExpressionNoteTemplate);
			// 
			// PlaceholdersLabel
			// 
			this.PlaceholdersLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ExpressionTemplateForm|c0c704c5-7678-4e9c-a4a0-33c7c3d2d029", "Placeholders");
			this.PlaceholdersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 92, true);
			this.PlaceholdersLabel.Name = "PlaceholdersLabel";
			this.PlaceholdersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 23, true);
			this.PlaceholdersLabel.TabIndex = 7;
			// 
			// PlaceholdersGrid
			// 
			this.PlaceholdersGrid.AllowNavigation = false;
			this.PlaceholdersGrid.AllowSorting = false;
			this.PlaceholdersGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.PlaceholdersGrid, "Placeholders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ExpressionNoteTemplate)(null)).Placeholders);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ExpressionPlaceholder)(((System.Collections.IList)(((ExpressionNoteTemplate)(null)).Placeholders)).SyncRoot)).Sequence);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ExpressionPlaceholder)(((System.Collections.IList)(((ExpressionNoteTemplate)(null)).Placeholders)).SyncRoot)).Description);
			this.PlaceholdersGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ExpressionTemplateForm|ac1ef6f8-6c56-4e14-ae92-c4d94ee970ae", "Sequence");
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ExpressionTemplateForm|96aa6212-aa7b-4da1-834d-41ef58fe4d53", "Placeholder Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.PlaceholdersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PlaceholdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PlaceholdersGrid.CopySelectedRowsAllowed = true;
			this.PlaceholdersGrid.GridId = "593aab8a-5df3-4ec4-932a-bd47531802b0";
			this.PlaceholdersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PlaceholdersGrid.LayoutKey = "zGrid1";
			this.PlaceholdersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 118, true);
			this.PlaceholdersGrid.Name = "PlaceholdersGrid";
			this.PlaceholdersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 165, true);
			this.PlaceholdersGrid.TabIndex = 8;
			// 
			// PlaceholderButton
			// 
			this.PlaceholderButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.PlaceholderButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ExpressionTemplateForm|1f1cf5d4-464f-4a52-80b2-59ff09de9294", "Placeholder");
			this.PlaceholderButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 37, true);
			this.PlaceholderButton.Name = "PlaceholderButton";
			this.PlaceholderButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PlaceholderButton.TabIndex = 5;
			this.PlaceholderButton.UseVisualStyleBackColor = false;
			this.PlaceholderButton.Click += new EventHandler(this.PlaceholderButton_Click);
			// 
			// ExpressionTemplateForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 388, true);
			this.Controls.Add(this.PlaceholderButton);
			this.Controls.Add(this.PlaceholdersGrid);
			this.Controls.Add(this.PlaceholdersLabel);
			this.DataSourceType = typeof(ExpressionNoteTemplate);
			this.Name = "ExpressionTemplateForm";
			this.Controls.SetChildIndex(this.descriptionTextBox, 0);
			this.Controls.SetChildIndex(this.templateTextBox, 0);
			this.Controls.SetChildIndex(this.TemplateLabel, 0);
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PlaceholdersLabel, 0);
			this.Controls.SetChildIndex(this.PlaceholdersGrid, 0);
			this.Controls.SetChildIndex(this.PlaceholderButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PlaceholdersGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
