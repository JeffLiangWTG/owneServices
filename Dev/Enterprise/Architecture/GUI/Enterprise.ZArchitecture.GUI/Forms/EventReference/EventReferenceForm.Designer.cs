using System;
using System.Security.AccessControl;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	partial class EventReferenceForm
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ParametersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FreeTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConfimButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DescriptionTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ParametersGrid)).BeginInit();
			this.DescriptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 297, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.EventReference);
			// 
			// ParametersGrid
			// 
			this.ParametersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ParametersGrid, "ParameterCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.EventReference)(null)).ParameterCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.Parameter)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.EventReference)(null)).ParameterCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.Parameter)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.EventReference)(null)).ParameterCollection)).SyncRoot)).ParamValue)));
			this.ParametersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("e4d4dd0d-6e29-45b5-b1f4-f2179897f081", "Code");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ae79474d-4372-4e84-9f47-0c61d1407552", "Value");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "ParamValue";
			this.ParametersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ParametersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParametersGrid.CopySelectedRowsAllowed = true;
			this.ParametersGrid.GridId = "bf20b48a-18e5-4388-8908-7de30ddd855c";
			this.ParametersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParametersGrid.LayoutKey = "zGrid1";
			this.ParametersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 166, true);
			this.ParametersGrid.Name = "ParametersGrid";
			this.ParametersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 96, true);
			this.ParametersGrid.TabIndex = 4;
			// 
			// FreeTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.FreeTextTextBox, "FreeText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.EventReference)(null)).FreeText)));
			this.FreeTextTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("365EB8EC-0F33-444D-8FDB-E9D433EEAEBF", "Free Text");
			this.FreeTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 142, true);
			this.FreeTextTextBox.Name = "FreeTextTextBox";
			this.FreeTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.FreeTextTextBox.TabIndex = 3;
			// 
			// ConfimButton
			// 
			this.ConfimButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ConfimButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("31892d66-e1dc-405f-9968-4aaff1d840ae", "OK");
			this.ConfimButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 268, true);
			this.ConfimButton.Name = "ConfimButton";
			this.ConfimButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ConfimButton.TabIndex = 5;
			this.ConfimButton.UseVisualStyleBackColor = true;
			this.ConfimButton.Click += new System.EventHandler(this.ConfimButton_Click);
			// 
			// DescriptionGroupBox
			// 
			this.DescriptionGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("c7f6c197-fcd4-4845-80d8-7ce4debac9c5", "Description");
			this.DescriptionGroupBox.Controls.Add(this.DescriptionTextLabel);
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 125, true);
			this.DescriptionGroupBox.TabIndex = 1;
			this.DescriptionGroupBox.TabStop = false;
			// 
			// DescriptionTextLabel
			// 
			this.DescriptionTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.DescriptionTextLabel.Name = "DescriptionTextLabel";
			this.DescriptionTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 106, true);
			this.DescriptionTextLabel.TabIndex = 2;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("7e99bf64-9ca2-4438-a4e4-b3933986045e", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 268, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 6;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// EventReferenceForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("582fc049-aac9-4a6b-a084-67ec36c78d72", "Event Reference");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 321, true);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.DescriptionGroupBox);
			this.Controls.Add(this.ConfimButton);
			this.Controls.Add(this.FreeTextTextBox);
			this.Controls.Add(this.ParametersGrid);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.EventReference);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 300, true);
			this.Name = "EventReferenceForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ParametersGrid, 0);
			this.Controls.SetChildIndex(this.FreeTextTextBox, 0);
			this.Controls.SetChildIndex(this.ConfimButton, 0);
			this.Controls.SetChildIndex(this.DescriptionGroupBox, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ParametersGrid)).EndInit();
			this.DescriptionGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGrid ParametersGrid;
		private ZTextBox FreeTextTextBox;
		private ZButton ConfimButton;
		private ZGroupBox DescriptionGroupBox;
		private ZLabel DescriptionTextLabel;
		new ZButton CancelButton;
	}
}
