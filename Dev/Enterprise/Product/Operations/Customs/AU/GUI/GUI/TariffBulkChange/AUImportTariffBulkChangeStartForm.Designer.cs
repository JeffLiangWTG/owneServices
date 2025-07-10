using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.GUI
{
	partial class AUImportTariffBulkChangeStartForm
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
		protected override void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AUImportTariffBulkChangeStartForm));
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffUpdateUserFileZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffItemChangeGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.TariffFinalUpdateZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.AutomaticConvertZCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InfoLabel = new CargoWise.Windows.UI.KLabel();
			this.TCOChangeDetails = new CargoWise.Windows.UI.KGroupBox();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.TCOUpdateUserFileZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffItemChangeGroupBox.SuspendLayout();
			this.TCOChangeDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 482, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(277);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.MinWidth = 0;
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.IsCaptionOverridden = true;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(924, 453, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.Text = "&Close";
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// TariffUpdateUserFileZButton
			// 
			this.TariffUpdateUserFileZButton.IsCaptionOverridden = true;
			this.TariffUpdateUserFileZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 119, true);
			this.TariffUpdateUserFileZButton.Name = "TariffUpdateUserFileZButton";
			this.TariffUpdateUserFileZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TariffUpdateUserFileZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TariffUpdateUserFileZButton.TabIndex = 3;
			this.TariffUpdateUserFileZButton.Text = "Perform Tariff Bulk Change Using Your/Other Party Concordance File";
			this.TariffUpdateUserFileZButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.TariffUpdateUserFileZButton.ToolTipCaption = null;
			this.TariffUpdateUserFileZButton.UseVisualStyleBackColor = true;
			this.TariffUpdateUserFileZButton.Click += new System.EventHandler(this.TariffBulkChangeFile);
			// 
			// TariffItemChangeGroupBox
			// 
			this.TariffItemChangeGroupBox.Controls.Add(this.TariffFinalUpdateZButton);
			this.TariffItemChangeGroupBox.Controls.Add(this.label2);
			this.TariffItemChangeGroupBox.Controls.Add(this.AutomaticConvertZCheckBox);
			this.TariffItemChangeGroupBox.Controls.Add(this.InfoLabel);
			this.TariffItemChangeGroupBox.Controls.Add(this.TariffUpdateUserFileZButton);
			this.TariffItemChangeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 12, true);
			this.TariffItemChangeGroupBox.Name = "TariffItemChangeGroupBox";
			this.TariffItemChangeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 307, true);
			this.TariffItemChangeGroupBox.TabIndex = 41;
			this.TariffItemChangeGroupBox.TabStop = false;
			this.TariffItemChangeGroupBox.Text = "Tariff Change Details";
			// 
			// TariffFinalUpdateZButton
			// 
			this.TariffFinalUpdateZButton.IsCaptionOverridden = true;
			this.TariffFinalUpdateZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 119, true);
			this.TariffFinalUpdateZButton.Name = "TariffFinalUpdateZButton";
			this.TariffFinalUpdateZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TariffFinalUpdateZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TariffFinalUpdateZButton.TabIndex = 44;
			this.TariffFinalUpdateZButton.Text = "Apply All Pending Changes to Data Base";
			this.TariffFinalUpdateZButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.TariffFinalUpdateZButton.ToolTipCaption = null;
			this.TariffFinalUpdateZButton.UseVisualStyleBackColor = true;
			this.TariffFinalUpdateZButton.Click += new System.EventHandler(this.ApplyTariffChanges);
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 51, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 66, true);
			this.label2.TabIndex = 43;
			this.label2.Text = resources.GetString("label2.Text");
			// 
			// AutomaticConvertZCheckBox
			// 
			this.AutomaticConvertZCheckBox.AutoSize = true;
			this.AutomaticConvertZCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutomaticConvertZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 20, true);
			this.AutomaticConvertZCheckBox.Name = "AutomaticConvertZCheckBox";
			this.AutomaticConvertZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 16, true);
			this.AutomaticConvertZCheckBox.TabIndex = 1;
			this.AutomaticConvertZCheckBox.Text = "Automatically Convert One Tariff to One Other Tariff";
			this.AutomaticConvertZCheckBox.UseVisualStyleBackColor = true;
			// 
			// InfoLabel
			// 
			this.InfoLabel.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 173, true);
			this.InfoLabel.Name = "InfoLabel";
			this.InfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 121, true);
			this.InfoLabel.TabIndex = 41;
			this.InfoLabel.Tag = "";
			this.InfoLabel.Text = resources.GetString("InfoLabel.Text");
			// 
			// TCOChangeDetails
			// 
			this.TCOChangeDetails.Controls.Add(this.label1);
			this.TCOChangeDetails.Controls.Add(this.TCOUpdateUserFileZButton);
			this.TCOChangeDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 325, true);
			this.TCOChangeDetails.Name = "TCOChangeDetails";
			this.TCOChangeDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 122, true);
			this.TCOChangeDetails.TabIndex = 42;
			this.TCOChangeDetails.TabStop = false;
			this.TCOChangeDetails.Text = "TCO Change Details";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 74, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 36, true);
			this.label1.TabIndex = 42;
			this.label1.Text = "The TCO Bulk Change should only be run after the Tariff Bulk changes are complete" +
    "d and ALL OLD Tariff Numbers are converted to NEW Tariff numbers. ";
			// 
			// TCOUpdateUserFileZButton
			// 
			this.TCOUpdateUserFileZButton.IsCaptionOverridden = true;
			this.TCOUpdateUserFileZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 21, true);
			this.TCOUpdateUserFileZButton.Name = "TCOUpdateUserFileZButton";
			this.TCOUpdateUserFileZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TCOUpdateUserFileZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TCOUpdateUserFileZButton.TabIndex = 5;
			this.TCOUpdateUserFileZButton.Text = "Perform TCO Bulk Change Using Your/Other Party Concordance File";
			this.TCOUpdateUserFileZButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.TCOUpdateUserFileZButton.ToolTipCaption = null;
			this.TCOUpdateUserFileZButton.UseVisualStyleBackColor = true;
			this.TCOUpdateUserFileZButton.Click += new System.EventHandler(this.TCOBulkChangeFile);
			// 
			// AUImportTariffBulkChangeStartForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 506, true);
			this.Controls.Add(this.TCOChangeDetails);
			this.Controls.Add(this.TariffItemChangeGroupBox);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange";
			this.Name = "AUImportTariffBulkChangeStartForm";
			this.Text = "Tariff Bulk Change";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.TariffItemChangeGroupBox, 0);
			this.Controls.SetChildIndex(this.TCOChangeDetails, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffItemChangeGroupBox.ResumeLayout(false);
			this.TariffItemChangeGroupBox.PerformLayout();
			this.TCOChangeDetails.ResumeLayout(false);
			this.TCOChangeDetails.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton TariffUpdateUserFileZButton;
		private CargoWise.Windows.UI.KGroupBox TariffItemChangeGroupBox;
		private CargoWise.Windows.UI.KLabel InfoLabel;
		private CargoWise.Windows.UI.KGroupBox TCOChangeDetails;
		private CargoWise.Windows.UI.KLabel label1;
		internal Enterprise.ZArchitecture.GUI.ZButton TCOUpdateUserFileZButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AutomaticConvertZCheckBox;
		private CargoWise.Windows.UI.KLabel label2;
		private Enterprise.ZArchitecture.GUI.ZButton TariffFinalUpdateZButton;
	}
}

