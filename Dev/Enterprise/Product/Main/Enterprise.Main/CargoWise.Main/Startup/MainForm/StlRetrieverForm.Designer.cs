using Enterprise.ZArchitecture;
using System;
using System.Collections.Generic;

using Enterprise.ZArchitecture.Core;
using CargoWise.Types;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace Enterprise.Billing.StlCollector.Retriever
{
	partial class StlRetrieverForm
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
            this.collectButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
            this.progressBar = new CargoWise.Windows.UI.KProgressBar();
            this.outputTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.script = new CargoWise.Windows.UI.KComboBox();
            this.FromDate = new CargoWise.Windows.UI.KDateTimePicker();
            this.ToDate = new CargoWise.Windows.UI.KDateTimePicker();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 376, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 24, true);
            // 
            // collectButton
            // 
            this.collectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.collectButton.CaptionResourceString = CargoWise.Main.Res.GetData("B851EDAC-1B51-4A31-AB54-284444719A43", "Collect");
            this.collectButton.IsCaptionOverridden = false;
            this.collectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 23, true);
            this.collectButton.Name = "collectButton";
            this.collectButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            this.collectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 30, true);
            this.collectButton.TabIndex = 7;
            this.collectButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.collectButton.ToolTipCaption = null;
            this.collectButton.UseVisualStyleBackColor = true;
            this.collectButton.Click += new System.EventHandler(this.collectButton_Click);
            // 
            // zLabel1
            // 
            this.zLabel1.CaptionResourceString = CargoWise.Main.Res.GetData("81F53AF9-5403-491D-93C8-A35CC8552D82", "To:");
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 27, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 22, true);
            this.zLabel1.TabIndex = 2;		
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // zLabel2
            // 
            this.zLabel2.CaptionResourceString = CargoWise.Main.Res.GetData("F02A0144-F24F-4CD8-8410-CBF3E5AEC359", "From:");
            this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 5, true);
            this.zLabel2.Name = "zLabel2";
            this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.zLabel2.TabIndex = 3;
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // zLabel3
            // 
            this.zLabel3.CaptionResourceString = CargoWise.Main.Res.GetData("84AB7AFA-A36E-4E73-A432-D3B400CAF59A", "Price Item:");
            this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 52, true);
            this.zLabel3.Name = "zLabel3";
            this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
            this.zLabel3.TabIndex = 4;
            this.zLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			//this.progressBar.Location = new System.Drawing.Point(12, 75);
			this.progressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 75, true);
			this.progressBar.Maximum = 3;
            this.progressBar.Name = "progressBar";
			//this.progressBar.Size = new System.Drawing.Size(576, 20);
			this.progressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
			this.progressBar.TabIndex = 7;
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.CaptionResourceString = CargoWise.Main.Res.GetData("e7da3191-65af-4504-a858-a015f226f812", "Output");
            this.outputTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.outputTextBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.outputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 105, true);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.outputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 255, true);
            this.outputTextBox.TabIndex = 10;
            // 
            // script
            // 
            this.script.AllowDrop = true;
            this.script.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.script.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.script.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 53, true);
            this.script.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 0, true);
            this.script.Name = "script";
            this.script.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 21, true);
            this.script.TabIndex = 8;
            this.script.SelectedIndexChanged += new System.EventHandler(this.script_SelectedIndexChanged);
            // 
            // FromDate
            // 
            this.FromDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 3, true);
            this.FromDate.Name = "FromDate";
            this.FromDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
            this.FromDate.TabIndex = 11;
            this.FromDate.Value = new System.DateTime(2020, 7, 27, 0, 0, 0, 0);
            this.FromDate.ValueChanged += new System.EventHandler(this.FromDate_ValueChanged);
			this.FromDate.Format = DateTimePickerFormat.Custom;
			this.FromDate.CustomFormat = "dd/MM/yyyy";
			// 
			// ToDate
			// 
			this.ToDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 29, true);
            this.ToDate.Name = "ToDate";
            this.ToDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
            this.ToDate.TabIndex = 12;
            this.ToDate.Value = new System.DateTime(2020, 7, 28, 0, 0, 0, 0);
            this.ToDate.ValueChanged += new System.EventHandler(this.ToDate_ValueChanged);
			this.ToDate.Format = DateTimePickerFormat.Custom;
			this.ToDate.CustomFormat = "dd/MM/yyyy";
			// 
			// StlRetrieverForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = CargoWise.Main.Res.GetData("3E8AE464-BAAD-4DC7-B8D1-26AD369868C5", "STL Data Collection");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
            this.Controls.Add(this.ToDate);
            this.Controls.Add(this.FromDate);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.zLabel2);
            this.Controls.Add(this.zLabel1);
            this.Controls.Add(this.collectButton);
            this.Controls.Add(this.zLabel3);
            this.Controls.Add(this.script);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
            this.Name = "StlRetrieverForm";
            this.Controls.SetChildIndex(this.script, 0);
            this.Controls.SetChildIndex(this.zLabel3, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.collectButton, 0);
            this.Controls.SetChildIndex(this.zLabel1, 0);
            this.Controls.SetChildIndex(this.zLabel2, 0);
            this.Controls.SetChildIndex(this.progressBar, 0);
            this.Controls.SetChildIndex(this.outputTextBox, 0);
            this.Controls.SetChildIndex(this.FromDate, 0);
            this.Controls.SetChildIndex(this.ToDate, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton collectButton;
		private ZLabel zLabel1;
		private ZLabel zLabel2;
		private ZLabel zLabel3;
		private ZTextBox outputTextBox;
		private CargoWise.Windows.UI.KProgressBar progressBar;
		protected CargoWise.Windows.UI.KComboBox script;
		private CargoWise.Windows.UI.KDateTimePicker FromDate;
		private CargoWise.Windows.UI.KDateTimePicker ToDate;
	}
}
