
namespace Enterprise.Customs.AU.GUI
{
	partial class AUExportTariffBulkChangeStartForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AUExportTariffBulkChangeStartForm));
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffUpdateUserFileZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffItemChangeGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.TariffFinalUpdateZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InfoLabel = new CargoWise.Windows.UI.KLabel();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.AutomaticConvertZCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffItemChangeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 482, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUExportTariffBulkChange);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.IsCaptionOverridden = true;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(919, 440, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.Text = "&Close";
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// TariffUpdateUserFileZButton
			// 
			this.TariffUpdateUserFileZButton.IsCaptionOverridden = true;
			this.TariffUpdateUserFileZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 129, true);
			this.TariffUpdateUserFileZButton.Name = "TariffUpdateUserFileZButton";
			this.TariffUpdateUserFileZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TariffUpdateUserFileZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TariffUpdateUserFileZButton.TabIndex = 3;
			this.TariffUpdateUserFileZButton.Text = "Perform AHECC Bulk Change Using Your/Other Party Concordance File";
			this.TariffUpdateUserFileZButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.TariffUpdateUserFileZButton.ToolTipCaption = null;
			this.TariffUpdateUserFileZButton.UseVisualStyleBackColor = true;
			this.TariffUpdateUserFileZButton.Click += new System.EventHandler(this.TariffBulkChangeFile);
			// 
			// TariffItemChangeGroupBox
			// 
			this.TariffItemChangeGroupBox.Controls.Add(this.TariffFinalUpdateZButton);
			this.TariffItemChangeGroupBox.Controls.Add(this.InfoLabel);
			this.TariffItemChangeGroupBox.Controls.Add(this.label2);
			this.TariffItemChangeGroupBox.Controls.Add(this.AutomaticConvertZCheckBox);
			this.TariffItemChangeGroupBox.Controls.Add(this.TariffUpdateUserFileZButton);
			this.TariffItemChangeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 12, true);
			this.TariffItemChangeGroupBox.Name = "TariffItemChangeGroupBox";
			this.TariffItemChangeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 307, true);
			this.TariffItemChangeGroupBox.TabIndex = 41;
			this.TariffItemChangeGroupBox.TabStop = false;
			this.TariffItemChangeGroupBox.Text = "AHECC Change Details";
			// 
			// TariffFinalUpdateZButton
			// 
			this.TariffFinalUpdateZButton.IsCaptionOverridden = true;
			this.TariffFinalUpdateZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 129, true);
			this.TariffFinalUpdateZButton.Name = "TariffFinalUpdateZButton";
			this.TariffFinalUpdateZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TariffFinalUpdateZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TariffFinalUpdateZButton.TabIndex = 48;
			this.TariffFinalUpdateZButton.Text = "Apply All Pending Changes to Data Base";
			this.TariffFinalUpdateZButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.TariffFinalUpdateZButton.ToolTipCaption = null;
			this.TariffFinalUpdateZButton.UseVisualStyleBackColor = true;
			this.TariffFinalUpdateZButton.Click += new System.EventHandler(this.ApplyTariffChanges);
			// 
			// InfoLabel
			// 
			this.InfoLabel.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 187, true);
			this.InfoLabel.Name = "InfoLabel";
			this.InfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 108, true);
			this.InfoLabel.TabIndex = 47;
			this.InfoLabel.Text = resources.GetString("InfoLabel.Text");
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 55, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 65, true);
			this.label2.TabIndex = 46;
			this.label2.Text = resources.GetString("label2.Text");
			// 
			// AutomaticConvertZCheckBox
			// 
			this.AutomaticConvertZCheckBox.AutoSize = true;
			this.AutomaticConvertZCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutomaticConvertZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 20, true);
			this.AutomaticConvertZCheckBox.Name = "AutomaticConvertZCheckBox";
			this.AutomaticConvertZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 16, true);
			this.AutomaticConvertZCheckBox.TabIndex = 1;
			this.AutomaticConvertZCheckBox.Text = "Automatically Convert One AHECC to One Other AHECC";
			this.AutomaticConvertZCheckBox.UseVisualStyleBackColor = true;
			// 
			// AUExportTariffBulkChangeStartForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 506, true);
			this.Controls.Add(this.TariffItemChangeGroupBox);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUExportTariffBulkChange);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.AUExportTariffBulkChange";
			this.Name = "AUExportTariffBulkChangeStartForm";
			this.Text = "AHECC Bulk Change";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.TariffItemChangeGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffItemChangeGroupBox.ResumeLayout(false);
			this.TariffItemChangeGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton TariffUpdateUserFileZButton;
		private CargoWise.Windows.UI.KGroupBox TariffItemChangeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AutomaticConvertZCheckBox;
		private CargoWise.Windows.UI.KLabel label2;
		private CargoWise.Windows.UI.KLabel InfoLabel;
		private Enterprise.ZArchitecture.GUI.ZButton TariffFinalUpdateZButton;
	}
}

